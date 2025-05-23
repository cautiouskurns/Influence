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
        [SerializeField] private Vector2 statsPanelSize = new Vector2(500f, 450f); // Made taller to fit all content
        [SerializeField] private Vector2 panelPosition = new Vector2(-600f, -500f); // Bottom right corner
        
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
            
            // Force layout refresh with additional adjustments for text fitting
            if (statsPanel != null)
            {
                Canvas.ForceUpdateCanvases();
                
                // Check if any text fields might be overflowing and adjust their size if needed
                foreach (var stat in statTexts)
                {
                    CheckAndAdjustTextSize(stat.Value);
                }
                
                LayoutRebuilder.ForceRebuildLayoutImmediate(statsPanel.GetComponent<RectTransform>());
            }
        }
        
        /// <summary>
        /// Check if text might be overflowing and adjust its size if needed
        /// </summary>
        private void CheckAndAdjustTextSize(TextMeshProUGUI textField)
        {
            if (textField == null) return;
            
            // If text might be too long, enable auto-sizing with a minimum size
            if (textField.text.Length > 15)
            {
                textField.enableAutoSizing = true;
                textField.fontSizeMin = 10;
                textField.fontSizeMax = 14;
                textField.overflowMode = TextOverflowModes.Truncate;
            }
            else
            {
                textField.enableAutoSizing = false;
                textField.fontSize = 14;
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
            
            // Add panel image with improved appearance
            Image panelImage = statsPanel.AddComponent<Image>();
            panelImage.color = panelColor;
            panelImage.type = Image.Type.Sliced; // Use sliced image for better borders
            
            // Configure panel size and position - bottom right
            RectTransform panelRect = statsPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 0);
            panelRect.anchorMax = new Vector2(1, 0);
            panelRect.pivot = new Vector2(1, 0);
            panelRect.sizeDelta = statsPanelSize;
            panelRect.anchoredPosition = panelPosition;
            
            // Add content container with padding for better text fit
            GameObject contentContainer = new GameObject("ContentContainer");
            contentContainer.transform.SetParent(statsPanel.transform, false);
            
            RectTransform contentRect = contentContainer.GetComponent<RectTransform>();
            if (contentRect == null)
            {
                contentRect = contentContainer.AddComponent<RectTransform>();
            }
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.pivot = new Vector2(0.5f, 0.5f);
            contentRect.offsetMin = new Vector2(10, 10); // Left, bottom padding
            contentRect.offsetMax = new Vector2(-10, -10); // Right, top padding
            
            // Add layout group for content with improved spacing
            VerticalLayoutGroup layout = contentContainer.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 8;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            
            // Create header section
            CreateHeaderSection(contentContainer.transform);
            
            // Create divider
            CreateDivider(contentContainer.transform);
            
            // Create primary stats directly in the content container without scroll view
            CreateStatField("Regions", "0", contentContainer.transform);
            CreateStatField("Treasury", "0", contentContainer.transform);
            CreateStatField("GDP", "0", contentContainer.transform);
            CreateStatField("Growth", "0%", contentContainer.transform);
            CreateStatField("Total Wealth", "0", contentContainer.transform);
            CreateStatField("Production", "0", contentContainer.transform);
            
            // Create divider in content
            CreateDivider(contentContainer.transform);
            
            // Create stability stats
            CreateStatCategory("Stability", contentContainer.transform);
            CreateStatField("Stability", "0%", contentContainer.transform);
            CreateStatField("Unrest", "0%", contentContainer.transform);
            
            // Create divider in content
            CreateDivider(contentContainer.transform);
            
            // Create policy stats
            CreateStatCategory("Policy Settings", contentContainer.transform);
            CreateStatField("Economic", "Balanced", contentContainer.transform);
            CreateStatField("Diplomatic", "Balanced", contentContainer.transform);
            CreateStatField("Military", "Balanced", contentContainer.transform);
            CreateStatField("Social", "Balanced", contentContainer.transform);
            
            Debug.Log("UI elements created successfully");
            
            // Hide panel initially until a nation is set
            statsPanel.SetActive(false);
        }
        
        private void CreateHeaderSection(Transform parent)
        {
            // Add header container
            GameObject headerContainer = new GameObject("HeaderContainer");
            headerContainer.transform.SetParent(parent, false);
            
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
            
            // Add title with improved text settings
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(headerContainer.transform, false);
            
            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "NATION NAME";
            titleText.fontSize = 22;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = headerColor;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.overflowMode = TextOverflowModes.Ellipsis;
            titleText.textWrappingMode = TextWrappingModes.Normal;
            
            LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
            titleLayout.flexibleWidth = 1;
        }
        
        private void CreateDivider(Transform parent)
        {
            GameObject dividerObj = new GameObject("Divider");
            dividerObj.transform.SetParent(parent, false);
            
            Image dividerImage = dividerObj.AddComponent<Image>();
            dividerImage.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
            
            LayoutElement dividerLayout = dividerObj.AddComponent<LayoutElement>();
            dividerLayout.preferredHeight = 2;
            dividerLayout.flexibleWidth = 1;
            dividerLayout.minHeight = 2;
        }
        
        private void CreateStatCategory(string categoryName, Transform parent)
        {
            // Create category header
            GameObject categoryObj = new GameObject(categoryName + "Category");
            categoryObj.transform.SetParent(parent, false);
            
            TextMeshProUGUI categoryText = categoryObj.AddComponent<TextMeshProUGUI>();
            categoryText.text = categoryName.ToUpper();
            categoryText.fontSize = 16;
            categoryText.fontStyle = FontStyles.Bold;
            categoryText.color = headerColor;
            categoryText.alignment = TextAlignmentOptions.Center;
            categoryText.overflowMode = TextOverflowModes.Ellipsis;
            
            LayoutElement categoryLayout = categoryObj.AddComponent<LayoutElement>();
            categoryLayout.preferredHeight = 25;
            categoryLayout.minHeight = 25;
            categoryLayout.flexibleWidth = 1;
        }
        
        private void CreateStatField(string label, string defaultValue = "0", Transform parent = null)
        {
            // Use statsPanel as parent if none specified
            if (parent == null) parent = statsPanel.transform;
            
            GameObject statObj = new GameObject(label.Replace(" ", "") + "Stat");
            statObj.transform.SetParent(parent, false);
            
            // Create horizontal layout for this stat with better spacing
            HorizontalLayoutGroup statLayout = statObj.AddComponent<HorizontalLayoutGroup>();
            statLayout.childAlignment = TextAnchor.MiddleLeft;
            statLayout.childControlWidth = false;
            statLayout.childForceExpandWidth = false;
            statLayout.spacing = 10;
            statLayout.padding = new RectOffset(5, 5, 0, 0);
            
            LayoutElement statObjLayout = statObj.AddComponent<LayoutElement>();
            statObjLayout.preferredHeight = 25;
            statObjLayout.minHeight = 22;
            statObjLayout.flexibleWidth = 1;
            
            // Create label with improved text settings
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(statObj.transform, false);
            
            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = label + ":";
            labelText.fontSize = 14;
            labelText.color = Color.white;
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.overflowMode = TextOverflowModes.Truncate;
            
            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 120;
            labelLayout.minWidth = 100;
            
            // Create value text with improved overflow handling
            GameObject valueObj = new GameObject("Value");
            valueObj.transform.SetParent(statObj.transform, false);
            
            TextMeshProUGUI valueText = valueObj.AddComponent<TextMeshProUGUI>();
            valueText.text = defaultValue;
            valueText.fontSize = 14;
            valueText.color = Color.white;
            valueText.alignment = TextAlignmentOptions.Left;
            valueText.overflowMode = TextOverflowModes.Ellipsis;
            valueText.textWrappingMode = TextWrappingModes.Normal;
            
            LayoutElement valueLayout = valueObj.AddComponent<LayoutElement>();
            valueLayout.preferredWidth = 150;
            valueLayout.minWidth = 100;
            valueLayout.flexibleWidth = 1;
            
            // Store reference to value text for updates
            statTexts[label] = valueText;
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
    }
}