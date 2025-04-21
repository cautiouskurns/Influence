using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core;
using System.Collections.Generic;
using Systems;
using Managers;

namespace UI
{
    /// <summary>
    /// UI Module for displaying simulation statistics and metrics
    /// </summary>
    public class StatsDisplayUIModule : UIModuleBase
    {
        [Header("Data References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private EconomicSystem economicSystem;
        
        [Header("UI Settings")]
        [SerializeField] private Color panelColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        [SerializeField] private Color headerColor = new Color(0.2f, 0.4f, 0.8f, 1f);
        [SerializeField] private Vector2 statsPanelSize = new Vector2(300f, 400f);
        [SerializeField] private float updateInterval = 0.5f;
        
        // UI Elements
        private GameObject statsPanel;
        private TextMeshProUGUI titleText;
        private Dictionary<string, TextMeshProUGUI> statTexts = new Dictionary<string, TextMeshProUGUI>();
        
        // Data tracking
        private float updateTimer = 0f;
        
        public override void Initialize()
        {
            base.Initialize();
            
            // Find references if not set
            if (gameManager == null)
                gameManager = FindFirstObjectByType<GameManager>();
                
            if (economicSystem == null)
                economicSystem = FindFirstObjectByType<EconomicSystem>();
            
            // Create UI elements
            CreateUIElements();
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
        
        private void CreateUIElements()
        {
            // Create main panel
            statsPanel = new GameObject("StatsPanel");
            statsPanel.transform.SetParent(transform, false);
            
            // Add panel image
            Image panelImage = statsPanel.AddComponent<Image>();
            panelImage.color = panelColor;
            
            // Configure panel size and position
            RectTransform panelRect = statsPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = statsPanelSize;
            
            // Add layout group for content
            VerticalLayoutGroup layout = statsPanel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 5;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            
            // Add title
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(statsPanel.transform, false);
            
            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "SIMULATION STATISTICS";
            titleText.fontSize = 20;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = headerColor;
            titleText.alignment = TextAlignmentOptions.Center;
            
            // Set title height
            LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = 30;
            titleLayout.minHeight = 30;
            
            // Add divider
            CreateDivider();
            
            // Add stat categories and fields
            CreateStatCategory("Economy", new string[] { 
                "Total Wealth", 
                "Average Wealth", 
                "Wealth Inequality",
                "Exchange Volume"
            });
            
            CreateStatCategory("Population", new string[] { 
                "Total Population", 
                "Growth Rate",
                "Migration Rate"
            });
            
            CreateStatCategory("Resources", new string[] { 
                "Production Rate", 
                "Consumption Rate",
                "Resource Balance" 
            });
            
            // Initialize stats with current values
            UpdateStats();
        }
        
        private void CreateDivider()
        {
            GameObject dividerObj = new GameObject("Divider");
            dividerObj.transform.SetParent(statsPanel.transform, false);
            
            Image dividerImage = dividerObj.AddComponent<Image>();
            dividerImage.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            
            RectTransform dividerRect = dividerObj.GetComponent<RectTransform>();
            
            LayoutElement dividerLayout = dividerObj.AddComponent<LayoutElement>();
            dividerLayout.preferredHeight = 2;
            dividerLayout.flexibleWidth = 1;
            dividerLayout.minHeight = 2;
        }
        
        private void CreateStatCategory(string categoryName, string[] statNames)
        {
            // Create category header
            GameObject categoryObj = new GameObject(categoryName + "Category");
            categoryObj.transform.SetParent(statsPanel.transform, false);
            
            TextMeshProUGUI categoryText = categoryObj.AddComponent<TextMeshProUGUI>();
            categoryText.text = categoryName.ToUpper();
            categoryText.fontSize = 16;
            categoryText.fontStyle = FontStyles.Bold;
            categoryText.color = headerColor;
            
            LayoutElement categoryLayout = categoryObj.AddComponent<LayoutElement>();
            categoryLayout.preferredHeight = 25;
            categoryLayout.minHeight = 25;
            
            // Create stats
            foreach (string statName in statNames)
            {
                GameObject statObj = new GameObject(statName.Replace(" ", "") + "Stat");
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
                labelText.text = statName + ":";
                labelText.fontSize = 20;
                labelText.color = Color.white;
                labelText.alignment = TextAlignmentOptions.Left;
                
                LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
                labelLayout.preferredWidth = 150;
                labelLayout.minWidth = 150;
                
                // Create value text
                GameObject valueObj = new GameObject("Value");
                valueObj.transform.SetParent(statObj.transform, false);
                
                TextMeshProUGUI valueText = valueObj.AddComponent<TextMeshProUGUI>();
                valueText.text = "0";
                valueText.fontSize = 20;
                valueText.color = Color.white;
                valueText.alignment = TextAlignmentOptions.Left;
                
                LayoutElement valueLayout = valueObj.AddComponent<LayoutElement>();
                valueLayout.preferredWidth = 100;
                valueLayout.minWidth = 100;
                
                // Store reference to value text for updates
                string key = categoryName + "." + statName;
                statTexts[key] = valueText;
            }
            
            // Add a small space after category
            CreateSpacer(10);
        }
        
        private void CreateSpacer(float height)
        {
            GameObject spacerObj = new GameObject("Spacer");
            spacerObj.transform.SetParent(statsPanel.transform, false);
            
            LayoutElement spacerLayout = spacerObj.AddComponent<LayoutElement>();
            spacerLayout.preferredHeight = height;
            spacerLayout.minHeight = height;
        }
        
        private void UpdateStats()
        {
            if (economicSystem == null || gameManager == null)
                return;
                
            // Update economic stats
            UpdateStatValue("Economy.Total Wealth", FormatNumber(economicSystem.GetTotalWealth()));
            // UpdateStatValue("Economy.Average Wealth", FormatNumber(economicSystem.GetAverageWealth()));
            // UpdateStatValue("Economy.Wealth Inequality", FormatPercent(economicSystem.GetWealthInequality()));
            // UpdateStatValue("Economy.Exchange Volume", FormatNumber(economicSystem.GetExchangeVolume()));
            
            // // Update population stats
            UpdateStatValue("Population.Total Population", FormatNumber(gameManager.GetTotalPopulation()));
            UpdateStatValue("Population.Growth Rate", FormatPercent(gameManager.GetPopulationGrowthRate()));
            UpdateStatValue("Population.Migration Rate", FormatPercent(gameManager.GetMigrationRate()));
            
            // // Update resource stats
            // UpdateStatValue("Resources.Production Rate", FormatNumber(economicSystem.GetTotalProduction()));
            // UpdateStatValue("Resources.Consumption Rate", FormatNumber(economicSystem.GetTotalConsumption()));
            // UpdateStatValue("Resources.Resource Balance", FormatNumber(economicSystem.GetResourceBalance()));
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
    }
}