using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI;
using System.Collections.Generic;

namespace Systems.UI
{
    /// <summary>
    /// View component responsible for displaying basic region information.
    /// Shows region name, wealth, and additional economic metrics.
    /// </summary>
    public class RegionStatsUIView : UIModuleBase
    {
        [Header("UI Settings")]
        [SerializeField] private Color panelColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        [SerializeField] private Color headerColor = new Color(0.8f, 0.4f, 0.2f, 1f);
        [SerializeField] private Vector2 statsPanelSize = new Vector2(350f, 400f); // Adjusted size
        [SerializeField] private Vector2 panelPosition = new Vector2(-20f, -20f); // Left side position
        
        // UI Elements
        private GameObject statsPanel;
        private TextMeshProUGUI titleText;
        private Dictionary<string, TextMeshProUGUI> statTexts = new Dictionary<string, TextMeshProUGUI>();
        
        public override void Initialize()
        {
            Debug.Log("RegionStatsUIView Initialize called");
            base.Initialize();
            CreateUIElements();
        }
        
        public override void Show()
        {
            if (statsPanel != null)
            {
                statsPanel.SetActive(true);
                // Force to front
                statsPanel.transform.SetAsLastSibling();
                Debug.Log("RegionStatsUIView panel shown");
            }
            else
            {
                Debug.LogWarning("Cannot show RegionStatsUIView - panel is null");
                CreateUIElements();
                if (statsPanel != null)
                {
                    statsPanel.SetActive(true);
                    statsPanel.transform.SetAsLastSibling();
                    Debug.Log("RegionStatsUIView panel created and shown");
                }
            }
        }
        
        public override void Hide()
        {
            if (statsPanel != null)
            {
                statsPanel.SetActive(false);
                Debug.Log("RegionStatsUIView panel hidden");
            }
        }
        
        /// <summary>
        /// Display region data prepared by the ViewModel
        /// </summary>
        public void DisplayRegion(RegionStatsViewModel.RegionDisplayData displayData)
        {
            Debug.Log("DisplayRegion called with prepared display data");
            
            if (statsPanel == null)
            {
                Debug.LogError("DisplayRegion failed - statsPanel is null. Creating UI elements...");
                CreateUIElements();
            }
            
            if (statsPanel != null)
            {
                statsPanel.SetActive(true);
            }
            
            if (titleText != null)
            {
                titleText.text = displayData.regionName;
                
                // Update all the stat fields
                UpdateDisplay(displayData);
                
                Debug.Log($"Updated region display with name: {displayData.regionName} and wealth: {displayData.wealth}");
            }
            else
            {
                Debug.LogError($"DisplayRegion UI elements not ready: titleText={titleText != null}");
            }
        }
        
        /// <summary>
        /// Update the display with prepared data
        /// </summary>
        public void UpdateDisplay(RegionStatsViewModel.RegionDisplayData data)
        {
            Debug.Log("UpdateDisplay called with fresh display data");
            
            // Update all the stat values
            UpdateStatValue("Wealth", data.wealth);
            UpdateStatValue("Production", data.production);
            UpdateStatValue("Population", data.population);
            UpdateStatValue("Infrastructure", data.infrastructure);
            UpdateStatValue("Resources", data.resources);
            UpdateStatValue("Growth", data.growth);
            UpdateStatValue("Nation", data.nationName);
            
            // Force layout refresh
            if (statsPanel != null)
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(statsPanel.GetComponent<RectTransform>());
            }
        }
        
        private void UpdateStatValue(string key, string value)
        {
            if (statTexts.ContainsKey(key) && statTexts[key] != null)
            {
                statTexts[key].text = value;
            }
            else
            {
                Debug.LogWarning($"Tried to update non-existent stat field: {key}");
            }
        }
        
        private void CreateUIElements()
        {
            // Check if elements already exist
            if (statsPanel != null)
            {
                Debug.Log("RegionStatsUIView UI elements already created");
                return;
            }
            
            Debug.Log("Creating RegionStatsUIView UI elements");
            
            // Create main panel
            statsPanel = new GameObject("RegionStatsPanel");
            statsPanel.transform.SetParent(transform, false);
            
            // Add panel image
            Image panelImage = statsPanel.AddComponent<Image>();
            panelImage.color = panelColor;
            
            // Configure panel size and position - changed to right side
            RectTransform panelRect = statsPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 0.5f);
            panelRect.anchorMax = new Vector2(1, 0.5f);
            panelRect.pivot = new Vector2(1, 0.5f);
            panelRect.sizeDelta = statsPanelSize;
            panelRect.anchoredPosition = panelPosition;
            
            // Add layout group for content
            VerticalLayoutGroup layout = statsPanel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 8;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            
            // Create title
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(statsPanel.transform, false);
            
            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "REGION NAME";
            titleText.fontSize = 20;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = headerColor;
            titleText.alignment = TextAlignmentOptions.Center;
            
            LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = 30;
            titleLayout.minHeight = 25;
            
            // Create divider
            CreateDivider();
            
            // Create stat fields
            CreateStatField("Wealth", "0");
            CreateStatField("Production", "0");
            CreateStatField("Population", "0");
            CreateStatField("Infrastructure", "Level 0 (0%)");
            
            // Create divider
            CreateDivider();
            
            // Create additional stats
            CreateStatCategory("Resources & Growth");
            CreateStatField("Resources", "Food: 0, Materials: 0, Fuel: 0");
            CreateStatField("Growth", "0%");
            CreateStatField("Nation", "Independent");
            
            Debug.Log("UI elements created successfully");
            
            // Make sure the panel has proper canvas sorting order
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                Canvas panelCanvas = statsPanel.AddComponent<Canvas>();
                panelCanvas.overrideSorting = true;
                panelCanvas.sortingOrder = 10; // Ensure it's on top
                
                // Add a GraphicRaycaster for UI interaction
                statsPanel.AddComponent<GraphicRaycaster>();
            }
            
            // Hide panel initially until a region is set
            statsPanel.SetActive(false);
        }
        
        private void CreateDivider()
        {
            GameObject dividerObj = new GameObject("Divider");
            dividerObj.transform.SetParent(statsPanel.transform, false);
            
            Image dividerImage = dividerObj.AddComponent<Image>();
            dividerImage.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            
            LayoutElement dividerLayout = dividerObj.AddComponent<LayoutElement>();
            dividerLayout.preferredHeight = 2;
            dividerLayout.flexibleWidth = 1;
            dividerLayout.minHeight = 2;
        }
        
        private void CreateStatCategory(string categoryName)
        {
            // Create category header
            GameObject categoryObj = new GameObject(categoryName + "Category");
            categoryObj.transform.SetParent(statsPanel.transform, false);
            
            TextMeshProUGUI categoryText = categoryObj.AddComponent<TextMeshProUGUI>();
            categoryText.text = categoryName.ToUpper();
            categoryText.fontSize = 16;
            categoryText.fontStyle = FontStyles.Bold;
            categoryText.color = headerColor;
            categoryText.alignment = TextAlignmentOptions.Center;
            
            LayoutElement categoryLayout = categoryObj.AddComponent<LayoutElement>();
            categoryLayout.preferredHeight = 25;
            categoryLayout.minHeight = 25;
        }
        
        private void CreateStatField(string label, string defaultValue = "0")
        {
            GameObject statObj = new GameObject(label.Replace(" ", "") + "Stat");
            statObj.transform.SetParent(statsPanel.transform, false);
            
            // Create horizontal layout for this stat
            HorizontalLayoutGroup statLayout = statObj.AddComponent<HorizontalLayoutGroup>();
            statLayout.childAlignment = TextAnchor.MiddleLeft;
            statLayout.childControlWidth = false;
            statLayout.childForceExpandWidth = false;
            statLayout.spacing = 5;
            
            LayoutElement statObjLayout = statObj.AddComponent<LayoutElement>();
            statObjLayout.preferredHeight = 22;
            statObjLayout.minHeight = 22;
            
            // Create label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(statObj.transform, false);
            
            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = label + ":";
            labelText.fontSize = 14;
            labelText.color = Color.white;
            labelText.alignment = TextAlignmentOptions.Left;
            
            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 100;
            labelLayout.minWidth = 100;
            
            // Create value text
            GameObject valueObj = new GameObject("Value");
            valueObj.transform.SetParent(statObj.transform, false);
            
            TextMeshProUGUI valueText = valueObj.AddComponent<TextMeshProUGUI>();
            valueText.text = defaultValue;
            valueText.fontSize = 14;
            valueText.color = Color.white;
            valueText.alignment = TextAlignmentOptions.Left;
            
            LayoutElement valueLayout = valueObj.AddComponent<LayoutElement>();
            valueLayout.preferredWidth = 120;
            valueLayout.minWidth = 120;
            valueLayout.flexibleWidth = 1;
            
            // Store reference to value text for updates
            statTexts[label] = valueText;
        }
    }
}