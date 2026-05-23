using System;
using Services;
using UniRx;
using UnityEngine;
using Visualization;
using Zenject;

namespace UI
{
    public class SearchUIPresenter : IInitializable, IDisposable
    {
        private readonly SearchUIView _view;
        private readonly VisualizationModel _model;
        private readonly VisualizationPresenter _vizPresenter;
        private readonly IExportService _exporter;
        private readonly CompositeDisposable _disposables = new();

        private const int MaxVisibleOffsets = 15;

        public SearchUIPresenter(
            SearchUIView view,
            VisualizationModel model,
            VisualizationPresenter vizPresenter,
            IExportService exporter)
        {
            _view = view;
            _model = model;
            _vizPresenter = vizPresenter;
            _exporter = exporter;
        }

        public void Initialize()
        {
            _view.OnStartClicked  += () => _vizPresenter.RunFullPipelineAsync().Forget();
            _view.OnCancelClicked += () => _vizPresenter.Cancel();
            _view.OnExportClicked += OnExportClicked;
            
            _model.Progress
                .Subscribe(v => _view.SetProgress(v))
                .AddTo(_disposables);

            _model.StatusText
                .Subscribe(t => _view.SetStatus(t))
                .AddTo(_disposables);

            _model.FoundCount
                .Subscribe(c => _view.SetFoundCount(c))
                .AddTo(_disposables);

            _model.State
                .Subscribe(s => _view.SetState(s))
                .AddTo(_disposables);

            _model.Results
                .Where(r => r != null)
                .Subscribe(results =>
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine($"Found {results.Count} offsets:");
                    for (int i = 0; i < Mathf.Min(results.Count, MaxVisibleOffsets); i++)
                    {
                        var m = results[i].Offset;
                        sb.AppendLine($"  [{i}] pos=({m.m03:F2},{m.m13:F2},{m.m23:F2})");
                    }
                    if (results.Count > MaxVisibleOffsets) sb.AppendLine("  ...");
                    _view.SetResults(sb.ToString());
                })
                .AddTo(_disposables);
        }

        private async void OnExportClicked()
        {
            if (_model.Results.Value == null) return;
            await _exporter.ExportOffsetsAsync(_model.Results.Value);
        }

        public void Dispose()
        {
            _view.Unsubscribe();
            _disposables?.Dispose();
        }
    }
}