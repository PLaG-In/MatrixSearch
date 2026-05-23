using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Services;
using UniRx;
using UnityEngine;
using Zenject;

namespace Visualization
{
    public class VisualizationPresenter : IInitializable, IDisposable
    {
        private readonly VisualizationModel  _model;
        private readonly VisualizationView   _view;
        private readonly IDataLoaderService  _loader;
        private readonly IMatrixSearchService _searcher;
        private readonly IExportService      _exporter;
        private readonly CompositeDisposable _disposables = new();

        private CancellationTokenSource _cts;
        
        private const int VisualUpdateEvery = 5;
        private int _visualCounter;

        public VisualizationPresenter(
            VisualizationModel model,
            VisualizationView view,
            IDataLoaderService loader,
            IMatrixSearchService searcher,
            IExportService exporter)
        {
            _model = model;
            _view = view;
            _loader = loader;
            _searcher = searcher;
            _exporter = exporter;
        }

        public void Initialize()
        {
            _model.Results
                .Where(r => r != null)
                .Subscribe(results =>
                {
                    foreach (var r in results)
                        _view.ShowMatch(r.Offset, _model.ModelMatrices);
                })
                .AddTo(_disposables);
            
            _model.CurrentStep
                .Where(_ => _model.State.Value == SearchState.Searching)
                .Subscribe(step =>
                {
                    _visualCounter++;
                    if (_visualCounter % VisualUpdateEvery == 0)
                        _view.ShowCandidate(step.candidateOffset, _model.ModelMatrices);
                })
                .AddTo(_disposables);
        }
        

        public async UniTaskVoid RunFullPipelineAsync()
        {
            _cts = new CancellationTokenSource();

            try
            {
                await LoadDataAsync(_cts.Token);
                await SearchAsync(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[Presenter] Search canceled");
                _model.SetError("Cancelled");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Presenter] Error: {e}");
                _model.SetError(e.Message);
            }
        }

        private async UniTask LoadDataAsync(CancellationToken ct)
        {
            _model.SetLoading();

            var (modelMatrices, spaceMatrices) = await UniTask.WhenAll(
                _loader.LoadMatricesAsync("model.json"),
                _loader.LoadMatricesAsync("space.json")
            );

            ct.ThrowIfCancellationRequested();

            _model.SetDataLoaded(modelMatrices, spaceMatrices);

            _view.SpawnSpaceCubes(spaceMatrices);
            _view.SpawnModelCubes(modelMatrices);

            Debug.Log($"[Presenter] Data loaded. model={modelMatrices.Count}, space={spaceMatrices.Count}");
        }

        private async UniTask SearchAsync(CancellationToken ct)
        {
            _model.SetSearching();
            _visualCounter = 0;

            var progress = new Progress<SearchProgress>(_model.UpdateProgress);

            var results = await _searcher.FindOffsetsAsync(
                _model.ModelMatrices,
                _model.SpaceMatrices,
                progress,
                ct);

            _view.ClearCandidates();
            _model.SetDone(results);
        }

        public void Cancel() => _cts?.Cancel();

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _disposables?.Dispose();
        }
    }
}