using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Scenarios
{
    public class ScenarioUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject scenarioInfoPanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject defeatPanel;
        
        [Header("Info Panel")]
        [SerializeField] private TMP_Text scenarioNameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text turnCounterText;
        [SerializeField] private TMP_Text objectiveText;
        
        [Header("Result Panels")]
        [SerializeField] private Button continueButton;

        private ScenarioManager scenarioManager;
        
        private void Awake()
        {
            scenarioManager = FindFirstObjectByType<ScenarioManager>();
            
            // Initialize continue button
            if (continueButton != null)
                continueButton.onClick.AddListener(HideAllPanels);
            
            // Default state
            HideAllPanels();
            if (scenarioInfoPanel != null)
                scenarioInfoPanel.SetActive(true);
        }
        
        // Updates the scenario info display
        public void SetScenarioInfo(string name, string description, string objective)
        {
            if (scenarioNameText != null)
                scenarioNameText.text = name;
                
            if (descriptionText != null)
                descriptionText.text = description;
                
            if (objectiveText != null)
                objectiveText.text = objective;
        }
        
        // Updates just the turn counter
        public void UpdateTurnCounter(int currentTurn)
        {
            if (turnCounterText != null && scenarioManager != null)
                turnCounterText.text = $"Turn: {currentTurn} / {scenarioManager.TurnLimit}";
        }
        
        // Shows the victory panel
        public void ShowVictoryPanel()
        {
            HideAllPanels();
            if (victoryPanel != null)
                victoryPanel.SetActive(true);
        }
        
        // Shows the defeat panel
        public void ShowDefeatPanel()
        {
            HideAllPanels();
            if (defeatPanel != null)
                defeatPanel.SetActive(true);
        }
        
        // Hides all panels
        public void HideAllPanels()
        {
            if (scenarioInfoPanel != null)
                scenarioInfoPanel.SetActive(false);
                
            if (victoryPanel != null)
                victoryPanel.SetActive(false);
                
            if (defeatPanel != null)
                defeatPanel.SetActive(false);
        }
    }
}