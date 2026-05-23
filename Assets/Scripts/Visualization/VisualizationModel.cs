using System.Collections.Generic;
using Data;
using Services;
using UniRx;
using UnityEngine;

namespace Visualization
{
    public enum SearchState
    {
        Idle,
        Loading,
        Searching,
        Done,
        Error
    }
    
    public class VisualizationModel
    {
        public IReadOnlyReactiveProperty<SearchState> State => _state;
        public IReadOnlyReactiveProperty<float> Progress => _progress;
        public IReadOnlyReactiveProperty<int> FoundCount => _foundCount;
        public IReadOnlyReactiveProperty<string> StatusText => _statusText;
        public IReadOnlyReactiveProperty<string> ErrorText => _errorText;

        public IReadOnlyReactiveProperty<SearchProgress> CurrentStep => _currentStep;
        
        public IReadOnlyReactiveProperty<List<OffsetResult>> Results => _results;
        
        public IReadOnlyList<Matrix4x4> ModelMatrices { get; private set; }
        public IReadOnlyList<Matrix4x4> SpaceMatrices { get; private set; }

        private readonly ReactiveProperty<SearchState> _state       = new(SearchState.Idle);
        private readonly ReactiveProperty<float> _progress    = new(0f);
        private readonly ReactiveProperty<int> _foundCount  = new(0);
        private readonly ReactiveProperty<string> _statusText  = new("Ready");
        private readonly ReactiveProperty<string> _errorText   = new();
        private readonly ReactiveProperty<SearchProgress>   _currentStep = new();
        private readonly ReactiveProperty<List<OffsetResult>> _results   = new();
        

        public void SetLoading()
        {
            _state.Value = SearchState.Loading;
            _statusText.Value = "Loading...";
            _progress.Value = 0f;
        }

        public void SetDataLoaded(List<Matrix4x4> model, List<Matrix4x4> space)
        {
            ModelMatrices = model;
            SpaceMatrices = space;
            _statusText.Value = $"Loaded: model={model.Count}, space={space.Count}";
        }

        public void SetSearching()
        {
            _state.Value = SearchState.Searching;
            _statusText.Value = "Search offsets...";
        }

        public void UpdateProgress(SearchProgress step)
        {
            _progress.Value = step.Progress01;
            _foundCount.Value = step.foundSoFar;
            _currentStep.Value = step;
            _statusText.Value = $"Check {step.currentSpaceIndex}/{step.totalSpace} | Found: {step.foundSoFar}";
        }

        public void SetDone(List<OffsetResult> results)
        {
            _state.Value = SearchState.Done;
            _results.Value = results;
            _foundCount.Value = results.Count;
            _statusText.Value = $"Done! Found offsets: {results.Count}";
            _progress.Value   = 1f;
        }

        public void SetError(string message)
        {
            _state.Value = SearchState.Error;
            _errorText.Value = message;
            _statusText.Value = $"Error: {message}";
        }
    }
}