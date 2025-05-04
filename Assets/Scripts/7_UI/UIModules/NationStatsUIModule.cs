using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Entities;

namespace UI
{
    /// <summary>
    /// UI Module for displaying basic nation statistics
    /// </summary>
    public class NationStatsUIModule : UIModuleBase
    {
        [Header("Nation Reference")]
        [SerializeField] private NationEntity currentNation;
        
        [Header("UI Settings")]
        [SerializeField] private Color panelColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        [SerializeField] private Color headerColor = new Color(0.2f, 0.4f, 0.8f, 1f);
        [SerializeField] private Vector2 statsPanelSize = new Vector2(280f, 380f);
        [SerializeField] private float updateInterval = 0.5f;
        [SerializeField] private Vector2 panelPosition = new Vector2(-925f, -30f); // Left side offset
        
        // UI Elements
        private GameObject statsPanel;
        private TextMeshProUGUI titleText;
        private Image colorIndicator;
        private Dictionary<string, TextMeshProUGUI> statTexts = new Dictionary<string, TextMeshProUGUI>();
        
        // Data tracking
        private float updateTimer = 0f;
        
        public void SetNation(NationEntity nation)
        {
            Debug.Log($"SetNation called with nation: {(nation != null ? nation.Name : "null")}");
            
            currentNation = nation;
            
            // Make the panel visible when a nation is set
            if (statsPanel != null)
            {
                statsPanel.SetActive(true);
            }
            
            if (titleText != null && currentNation != null)
            {
                titleText.text = currentNation.Name.ToUpper();
                colorIndicator.color = currentNation.Color;
                Debug.Log($"Setting panel title to {currentNation.Name.ToUpper()} and color to {currentNation.Color}");
                UpdateStats();
            }
            else
            {
                Debug.LogWarning($"Could not update UI: titleText={titleText != null}, currentNation={currentNation != null}");
            }
        }
        
        public void Show()
        {
            if (statsPanel != null)
            {
                statsPanel.SetActive(true);
            }
        }
        
        public void Hide()
        {
            if (statsPanel != null)
            {
                statsPanel.SetActive(false);
            }
        }
        
        public override void Initialize()
        {
            base.Initialize();
            CreateUIElements();
        }
        
        private void Update()
        {
            // Update stats periodically
            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval && currentNation != null)
            {
                updateTimer = 0f;
                UpdateStats();
            }
        }
        
        private void CreateUIElements()
        {
            // Check if elements already exist
            if (statsPanel != null)
            {
                Debug.Log("UI elements already created, skipping creation.");
                return;
            }
            
            // Create main panel
            statsPanel = new GameObject("NationStatsUIPanel"); // Renamed to avoid conflicts
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
        
        private void UpdateStats()
        {
            if (currentNation == null)
                return;

            try
            {
                // Debug logging
                Debug.Log($"Updating stats for nation: {currentNation.Name} (ID: {currentNation.Id})");
                if (currentNation.Economy != null)
                {
                    Debug.Log($"Economy data: Treasury={currentNation.Economy.TreasuryBalance}, GDP={currentNation.Economy.GDP}, Growth={currentNation.Economy.GDPGrowthRate}");
                }
                else
                {
                    Debug.LogWarning($"Nation {currentNation.Name} has no Economy component!");
                }

                // Update basic nation stats
                UpdateStatValue("Regions", currentNation.GetRegionIds()?.Count.ToString() ?? "0");
                
                // Economy stats with null checks
                if (currentNation.Economy != null)
                {
                    UpdateStatValue("Treasury", FormatNumber(currentNation.Economy.TreasuryBalance));
                    UpdateStatValue("GDP", FormatNumber(currentNation.Economy.GDP));
                    UpdateStatValue("Growth", FormatPercent(currentNation.Economy.GDPGrowthRate));
                    UpdateStatValue("Total Wealth", FormatNumber(currentNation.Economy.TotalWealth));
                    UpdateStatValue("Production", FormatNumber(currentNation.Economy.TotalProduction));
                }
                else
                {
                    // Use test values if Economy component is missing
                    Debug.Log("Using test values for economy stats");
                    UpdateStatValue("Treasury", FormatNumber(1000));
                    UpdateStatValue("GDP", FormatNumber(2500));
                    UpdateStatValue("Growth", FormatPercent(0.05f));
                    UpdateStatValue("Total Wealth", FormatNumber(5000));
                    UpdateStatValue("Production", FormatNumber(750));
                }
                
                // Stability stats with null check
                if (currentNation.Stability != null)
                {
                    Debug.Log($"Stability data: Stability={currentNation.Stability.Stability}, Unrest={currentNation.Stability.UnrestLevel}");
                    UpdateStatValue("Stability", FormatPercent(currentNation.Stability.Stability));
                    UpdateStatValue("Unrest", FormatPercent(currentNation.Stability.UnrestLevel));
                }
                else
                {
                    Debug.LogWarning($"Nation {currentNation.Name} has no Stability component!");
                    UpdateStatValue("Stability", "0%");
                    UpdateStatValue("Unrest", "0%");
                }
                
                // Update policy settings with try-catch for each one
                try { UpdateStatValue("Economic", FormatPolicyValue(currentNation.GetPolicy(NationEntity.PolicyType.Economic))); } 
                catch (System.Exception e) { 
                    Debug.LogWarning($"Failed to get Economic policy: {e.Message}");
                    UpdateStatValue("Economic", "N/A"); 
                }
                
                try { UpdateStatValue("Diplomatic", FormatPolicyValue(currentNation.GetPolicy(NationEntity.PolicyType.Diplomatic))); } 
                catch (System.Exception e) { 
                    Debug.LogWarning($"Failed to get Diplomatic policy: {e.Message}");
                    UpdateStatValue("Diplomatic", "N/A"); 
                }
                
                try { UpdateStatValue("Military", FormatPolicyValue(currentNation.GetPolicy(NationEntity.PolicyType.Military))); } 
                catch (System.Exception e) { 
                    Debug.LogWarning($"Failed to get Military policy: {e.Message}");
                    UpdateStatValue("Military", "N/A"); 
                }
                
                try { UpdateStatValue("Social", FormatPolicyValue(currentNation.GetPolicy(NationEntity.PolicyType.Social))); } 
                catch (System.Exception e) { 
                    Debug.LogWarning($"Failed to get Social policy: {e.Message}");
                    UpdateStatValue("Social", "N/A"); 
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Error updating nation stats: {e.Message}");
            }
        }
        
        private void UpdateStatValue(string key, string value)
        {
            if (statTexts.ContainsKey(key))
            {
                statTexts[key].text = value;
            }
        }
        
        private string FormatNumber(float value)
        {
            if (value >= 1000000)
                return (value / 1000000f).ToString("F2") + "M";
            else if (value >= 1000)
                return (value / 1000f).ToString("F1") + "K";
            else
                return value.ToString("F1");
        }
        
        private string FormatPercent(float value)
        {
            return (value * 100).ToString("F1") + "%";
        }
        
        private string FormatPolicyValue(float value)
        {
            if (value < 0.25f)
                return "Very Low";
            else if (value < 0.4f)
                return "Low";
            else if (value <= 0.6f)
                return "Balanced";
            else if (value <= 0.75f)
                return "High";
            else
                return "Very High";
        }
    }
}