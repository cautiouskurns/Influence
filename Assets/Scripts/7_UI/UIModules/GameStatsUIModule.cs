using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Managers;

namespace UI
{
    /// <summary>
    /// UI Module for displaying game statistics like turn counter, date, etc.
    /// </summary>
    public class GameStatsUIModule : UIModuleBase
    {
        [Header("UI Settings")]
        [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.7f);
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private float updateInterval = 0.5f;
        
        // UI Elements
        private GameObject statsPanel;
        private TextMeshProUGUI turnCounterText;
        
        // Reference to GameManager
        private GameManager gameManager;
        
        // Update timer
        private float updateTimer = 0f;
        
        // Current turn for tracking changes
        private int lastTurn = 0;
        
        public override void Initialize()
        {
            base.Initialize();
            
            // Get reference to the GameManager
            gameManager = GameManager.Instance;
            
            // Create UI elements if they don't exist
            if (statsPanel == null)
            {
                CreateUIElements();
            }
            
            // Refresh the UI immediately
            UpdateStats();
            
            // Subscribe to turn change events
            EventBus.Subscribe("EconomicTick", OnEconomicTick);
        }
        
        private void OnDestroy()
        {
            // Unsubscribe when destroyed
            EventBus.Unsubscribe("EconomicTick", OnEconomicTick);
        }
        
        private void Update()
        {
            // Update stats periodically
            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                updateTimer = 0f;
                UpdateStats();
            }
        }
        
        private void OnEconomicTick(object data)
        {
            // Update UI immediately when a turn passes
            UpdateStats();
        }
        
        private void CreateUIElements()
        {
            // Create main panel
            statsPanel = new GameObject("GameStatsPanel");
            statsPanel.transform.SetParent(transform, false);
            
            // Add panel background
            Image panelImage = statsPanel.AddComponent<Image>();
            panelImage.color = backgroundColor;
            
            // Configure panel size and position - now anchored to bottom-left
            RectTransform panelRect = statsPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(0, 0);
            panelRect.pivot = new Vector2(0, 0);
            panelRect.sizeDelta = new Vector2(120, 40);
            panelRect.anchoredPosition = new Vector2(10, 10);
            
            // Create turn counter text
            GameObject turnTextObj = new GameObject("TurnCounterText");
            turnTextObj.transform.SetParent(statsPanel.transform, false);
            
            turnCounterText = turnTextObj.AddComponent<TextMeshProUGUI>();
            turnCounterText.text = "Turn: 1";
            turnCounterText.fontSize = 18;
            turnCounterText.fontStyle = FontStyles.Bold;
            turnCounterText.color = textColor;
            turnCounterText.alignment = TextAlignmentOptions.Center;
            
            RectTransform textRect = turnTextObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
        
        private void UpdateStats()
        {
            if (gameManager == null || turnCounterText == null)
                return;
            
            int currentTurn = gameManager.GetCurrentTurn();
            
            // Only update the text if the turn has changed
            if (currentTurn != lastTurn)
            {
                turnCounterText.text = $"Turn: {currentTurn}";
                lastTurn = currentTurn;
            }
        }
    }
}