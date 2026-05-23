using System;
using UnityEngine;
using UnityEngine.UI;
using Visualization;
using TMPro;

namespace UI
{
    public class SearchUIView : MonoBehaviour
    {
        [Header("Progress")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI statusLabel;

        [Header("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button exportButton;

        [Header("Results")]
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private TextMeshProUGUI resultsText;

        public event Action OnStartClicked;
        public event Action OnCancelClicked;
        public event Action OnExportClicked;

        private void Awake()
        {
            startButton.onClick.AddListener(() => OnStartClicked?.Invoke());
            cancelButton.onClick.AddListener(() => OnCancelClicked?.Invoke());
            exportButton.onClick.AddListener(() => OnExportClicked?.Invoke());
        }

        public void SetProgress(float value01)
        { 
            progressBar.value = value01;
        }

        public void SetStatus(string text)
        {
            statusLabel.text = text;
        }

        public void SetState(SearchState state)
        {
            bool searching = state is SearchState.Searching or SearchState.Loading;

            startButton.interactable  = state is SearchState.Idle or SearchState.Done or SearchState.Error;
            cancelButton.interactable = searching;
            exportButton.interactable = state == SearchState.Done;
            resultsPanel.SetActive(state is SearchState.Done or SearchState.Error);
        }

        public void SetResults(string text)
        {
            resultsText.text = text;
        }
        
        public void Unsubscribe()
        {
            OnStartClicked = null;
            OnCancelClicked = null;
            OnExportClicked = null;
        }
    }
}