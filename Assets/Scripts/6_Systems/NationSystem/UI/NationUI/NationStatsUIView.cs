using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Entities;
using UI;

namespace Systems.UI
{
    /// <summary>
    /// View component responsible for creating and updating UI elements for nation statistics display.
    /// This class handles the visual representation of nation data.
    /// </summary>
    public class NationStatsUIView : UIModuleBase
    {
        [Header("UI Settings")]
        [SerializeField] private Color panelColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        [SerializeField] private Color headerColor = new Color(0.2f, 0.4f, 0.8f, 1f);
        [SerializeField] private Vector2 statsPanelSize = new Vector2(280f, 380f);
        [SerializeField] private Vector2 panelPosition = new Vector2(-925f, -30f); // Left side offset
        
        // UI Elements
        private GameObject statsPanel;
        private TextMeshProUGUI titleText;
        private Image colorIndicator;
        private Dictionary<string, TextMeshProUGUI> statTexts = new Dictionary<string, TextMeshProUGUI>();
        
        public override void Initialize()
        {
            Debug.Log("NationStatsUIView Initialize called");
            base.Initialize();
            CreateUIElements();
        }
        
        public override void Show()
        {
            if (statsPanel != null)
            {
                statsPanel.SetActive(true);
                Debug.Log("NationStatsUIView panel shown");
            }
            else
            {
                Debug.LogWarning("Cannot show NationStatsUIView - panel is null");
            }
        }
        
        public override void Hide()
        {
            if (statsPanel != null)
            {
                statsPanel.SetActive(false);
                Debug.Log("NationStatsUIView panel hidden");
            }
        }
        
        /// <summary>
        /// Display nation data prepared by the ViewModel
        /// </summary>
        public void DisplayNation(NationStatsViewModel.NationDisplayData displayData)
        {
            Debug.Log("DisplayNation called with prepared display data");
            
            if (statsPanel == null)
            {
                Debug.LogError("DisplayNation failed - statsPanel is null. Creating UI elements...");
                CreateUIElements();
            }
            
            if (statsPanel != null)
            {
                statsPanel.SetActive(true);
            }
            
            if (titleText != null && colorIndicator != null)
            {
                titleText.text = displayData.nationName;
                colorIndicator.color = displayData.nationColor;
                
                // Update all stat values from the display data
                UpdateDisplay(displayData);
                Debug.Log($"Updated nation display with name: {displayData.nationName}");
            }
            else
            {
                Debug.LogError($"DisplayNation UI elements not ready: titleText={titleText != null}, colorIndicator={colorIndicator != null}");
            }
        }
        
        /// <summary>
        /// Update the display with prepared data
        /// </summary>
        public void UpdateDisplay(NationStatsViewModel.NationDisplayData data)
        {
            Debug.Log("UpdateDisplay called with fresh display data");
            
            // Update display with prepared data from ViewModel
            UpdateStatValue("Regions", data.regionsCount);
            
            // Economy stats
            UpdateStatValue("Treasury", data.treasury);
            UpdateStatValue("GDP", data.gdp);
            UpdateStatValue("Growth", data.growthRate);
            UpdateStatValue("Total Wealth", data.totalWealth);
            UpdateStatValue("Production", data.production);
            
            // Stability stats
            UpdateStatValue("Stability", data.stability);
            UpdateStatValue("Unrest", data.unrest);
            
            // Policy settings
            UpdateStatValue("Economic", data.economicPolicy);
            UpdateStatValue("Diplomatic", data.diplomaticPolicy);
            UpdateStatValue("Military", data.militaryPolicy);
            UpdateStatValue("Social", data.socialPolicy);
            
            // Force layout refresh
            if (statsPanel != null)
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(statsPanel.GetComponent<RectTransform>());
            }
        }
        
        private void CreateUIElements()
        {
            // Check if elements already exist
            if (statsPanel != null)
            {
                Debug.Log("NationStatsUIView UI elements already created");
                return;
            }
            
            Debug.Log("Creating NationStatsUIView UI elements");
            
            // Create main panel
            statsPanel = new GameObject("NationStatsPanel");
            statsPanel.transform.SetParent(transform, false);
            
            // Add panel image
            Image panelImage = statsPanel.AddComponent<Image>();
            panelImage.color = panelColor;
            
            // Configure panel size and position
            RectTransform panelRect = statsPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 1);
            panelRect.anchorMax = new Vector2(0, 1);
            panelRect.pivot = new Vector2(0, 1);
            panelRect.sizeDelta = statsPanelSize;
            panelRect.anchoredPosition = panelPosition;
            
            // Add layout group for content
            VerticalLayoutGroup layout = statsPanel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 5;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            
            // Create header section
            CreateHeaderSection();
            
            // Add divider
            CreateDivider();
            
            // Add basic stats fields
            CreateStatField("Regions", "0");
            CreateStatField("Treasury", "0");
            CreateStatField("GDP", "0");
            CreateStatField("Growth", "0%");
            CreateStatField("Total Wealth", "0");
            CreateStatField("Production", "0");
            CreateStatField("Stability", "0%");
            CreateStatField("Unrest", "0%");
            
            // Add divider
            CreateDivider();
            
            // Add policy stats
            CreateStatCategory("Policy Settings");
            CreateStatField("Economic", "Balanced");
            CreateStatField("Diplomatic", "Balanced");
            CreateStatField("Military", "Balanced");
            CreateStatField("Social", "Balanced");
            
            Debug.Log("UI elements created successfully");
            
            // Hide panel initially until a nation is set
            statsPanel.SetActive(false);
        }
        
        private void CreateHeaderSection()
        {
            // Add header container
            GameObject headerContainer = new GameObject("HeaderContainer");
            headerContainer.transform.SetParent(statsPanel.transform, false);
            
            // Add horizontal layout for header
            HorizontalLayoutGroup headerLayout = headerContainer.AddComponent<HorizontalLayoutGroup>();
            headerLayout.childAlignment = TextAnchor.MiddleCenter;
            headerLayout.spacing = 10;
            
            LayoutElement headerElement = headerContainer.AddComponent<LayoutElement>();
            headerElement.preferredHeight = 30;
            headerElement.minHeight = 30;
            
            // Add color indicator
            GameObject colorObj = new GameObject("ColorIndicator");
            colorObj.transform.SetParent(headerContainer.transform, false);
            
            colorIndicator = colorObj.AddComponent<Image>();
            colorIndicator.color = Color.white;
            
            LayoutElement colorLayout = colorObj.AddComponent<LayoutElement>();
            colorLayout.preferredWidth = 20;
            colorLayout.preferredHeight = 20;
            colorLayout.minWidth = 20;
            colorLayout.minHeight = 20;
            
            // Add title
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(headerContainer.transform, false);
            
            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "NATION NAME";
            titleText.fontSize = 20;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = headerColor;
            titleText.alignment = TextAlignmentOptions.Center;
            
            LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
            titleLayout.flexibleWidth = 1;
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
            statObjLayout.preferredHeight = 20;
            statObjLayout.minHeight = 20;
            
            // Create label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(statObj.transform, false);
            
            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = label + ":";
            labelText.fontSize = 14;
            labelText.color = Color.white;
            labelText.alignment = TextAlignmentOptions.Left;
            
            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 120;
            labelLayout.minWidth = 120;
            
            // Create value text
            GameObject valueObj = new GameObject("Value");
            valueObj.transform.SetParent(statObj.transform, false);
            
            TextMeshProUGUI valueText = valueObj.AddComponent<TextMeshProUGUI>();
            valueText.text = defaultValue;
            valueText.fontSize = 14;
            valueText.color = Color.white;
            valueText.alignment = TextAlignmentOptions.Left;
            
            LayoutElement valueLayout = valueObj.AddComponent<LayoutElement>();
            valueLayout.preferredWidth = 90;
            valueLayout.minWidth = 90;
            valueLayout.flexibleWidth = 1;
            
            // Store reference to value text for updates
            statTexts[label] = valueText;
        }
        
        private void UpdateStatValue(string key, string value)
        {
            if (statTexts.ContainsKey(key))
            {
                statTexts[key].text = value;
                // Debug.Log($"Updated stat {key} to {value}");
            }
            else
            {
                Debug.LogWarning($"Tried to update non-existent stat field: {key}");
            }
        }
    }
}