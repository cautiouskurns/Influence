using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Managers;
using Core;

namespace Scenarios
{
    public class ScenarioUI : MonoBehaviour
    {
        [Header("Screen Text")]
        public TextMeshProUGUI screenInfoText;
        
        [Header("Advance Turn Button")]
        public Button advanceTurnButton;
        
        private ScenarioManager scenarioManager;
        private TurnManager turnManager;
        private string scenarioName;
        private string description;
        private string objective;
        private int currentTurn;
        private bool victoryAchieved;
        private bool defeatAchieved;
        private bool wasPaused = false;
        
        private void Awake()
        {
            scenarioManager = FindFirstObjectByType<ScenarioManager>();
            turnManager = FindFirstObjectByType<TurnManager>();
            
            // Create screen info text if not assigned
            if (screenInfoText == null)
            {
                CreateScreenInfoText();
            }
            
            // Create advance turn button if not assigned
            // if (advanceTurnButton == null)
            // {
            //     CreateAdvanceTurnButton();
            // }
            
            // Initialize
            scenarioName = "No scenario loaded";
            description = "";
            objective = "";
            currentTurn = 0;
            victoryAchieved = false;
            defeatAchieved = false;
            
            UpdateScreenText();
        }
        
        private void CreateScreenInfoText()
        {
            // Find canvas or create one
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            // Create the text object
            GameObject textObj = new GameObject("ScreenInfoText");
            textObj.transform.SetParent(canvas.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 1);
            textRect.anchorMax = new Vector2(0, 1);
            textRect.pivot = new Vector2(0, 1);
            textRect.sizeDelta = new Vector2(500, 200);
            textRect.anchoredPosition = new Vector2(10, -10);
            
            // Add TextMeshPro component
            screenInfoText = textObj.AddComponent<TextMeshProUGUI>();
            screenInfoText.fontSize = 26;
            screenInfoText.color = Color.white;
            screenInfoText.alignment = TextAlignmentOptions.TopLeft;
        }
        
        private void CreateAdvanceTurnButton()
        {
            // Find canvas or create one
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            // Create button object
            GameObject buttonObj = new GameObject("AdvanceTurnButton");
            buttonObj.transform.SetParent(canvas.transform, false);
            
            // Position the button in the top-right corner
            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(1, 1);
            buttonRect.anchorMax = new Vector2(1, 1);
            buttonRect.pivot = new Vector2(1, 1);
            buttonRect.sizeDelta = new Vector2(150, 50);
            buttonRect.anchoredPosition = new Vector2(-20, -20);
            
            // Add button visuals and component
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.6f, 0.2f, 1f); // Green color
            
            Button button = buttonObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.3f, 0.7f, 0.3f, 1f);
            colors.pressedColor = new Color(0.1f, 0.5f, 0.1f, 1f);
            button.colors = colors;
            
            // Create text for the button
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Advance Turn";
            buttonText.fontSize = 18;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            
            // Set the button variable
            advanceTurnButton = button;
            
            // Add onClick event
            advanceTurnButton.onClick.AddListener(AdvanceTurn);
        }
        
        // Method to advance the turn
        public void AdvanceTurn()
        {
            if (turnManager != null)
            {
                // Your TurnManager doesn't have an EndTurn method, but uses a coroutine with Pause/Resume
                // We'll briefly resume the simulation to allow one tick to happen, then pause it again
                
                // Remember if it was paused originally
                wasPaused = turnManager.IsPaused;
                
                // Resume to allow a tick
                turnManager.Resume();
                
                // We'll pause it after a short delay if it was paused originally
                if (wasPaused)
                {
                    Invoke("PauseAfterTick", 0.1f);
                }
                
                Debug.Log("Advanced to next turn");
            }
            else
            {
                Debug.LogWarning("TurnManager not found");
            }
        }
        
        // Helper method to pause the turn manager after a tick
        private void PauseAfterTick()
        {
            if (turnManager != null && wasPaused)
            {
                turnManager.Pause();
            }
        }
        
        // Updates the scenario info display
        public void SetScenarioInfo(string name, string description, string objective)
        {
            this.scenarioName = name;
            this.description = description;
            this.objective = objective;
            
            UpdateScreenText();
        }
        
        // Updates just the turn counter
        public void UpdateTurnCounter(int currentTurn)
        {
            this.currentTurn = currentTurn;
            UpdateScreenText();
        }
        
        // Shows the victory status
        public void ShowVictoryPanel()
        {
            victoryAchieved = true;
            defeatAchieved = false;
            UpdateScreenText();
        }
        
        // Shows the defeat status
        public void ShowDefeatPanel()
        {
            defeatAchieved = true;
            victoryAchieved = false;
            UpdateScreenText();
        }
        
        // Updates the screen text with all current information
        private void UpdateScreenText()
        {
            if (screenInfoText == null)
                return;
                
            string turnInfo = scenarioManager != null ? $"Turn: {currentTurn} / {scenarioManager.TurnLimit}" : $"Turn: {currentTurn}";
            
            string statusInfo = "";
            if (victoryAchieved)
                statusInfo = "\n<color=green>VICTORY ACHIEVED!</color>";
            else if (defeatAchieved)
                statusInfo = "\n<color=red>DEFEAT!</color>";
                
            string text = $"<b>{scenarioName}</b>\n" +
                          $"{description}\n\n" +
                          $"<color=yellow>Objective:</color> {objective}\n" +
                          $"{turnInfo}" +
                          statusInfo;
                          
            screenInfoText.text = text;
        }
        
        // For compatibility with existing code
        public void HideAllPanels()
        {
            // Not used in this simplified version
        }
    }
}