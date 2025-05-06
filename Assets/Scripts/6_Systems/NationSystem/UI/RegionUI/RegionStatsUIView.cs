using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI;

namespace Systems.UI
{
    /// <summary>
    /// View component responsible for displaying basic region information.
    /// This is a simplified version that shows only region name and wealth.
    /// </summary>
    public class RegionStatsUIView : UIModuleBase
    {
        [Header("UI Settings")]
        [SerializeField] private Color panelColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        [SerializeField] private Color headerColor = new Color(0.8f, 0.4f, 0.2f, 1f);
        [SerializeField] private Vector2 statsPanelSize = new Vector2(200f, 100f);
        [SerializeField] private Vector2 panelPosition = new Vector2(0f, -100f); // Bottom center offset
        
        // UI Elements
        private GameObject statsPanel;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI wealthText;
        
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
                Debug.Log("RegionStatsUIView panel shown");
            }
            else
            {
                Debug.LogWarning("Cannot show RegionStatsUIView - panel is null");
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
            
            if (titleText != null && wealthText != null)
            {
                titleText.text = displayData.regionName;
                wealthText.text = "Wealth: " + displayData.wealth;
                
                Debug.Log($"Updated region display with name: {displayData.regionName} and wealth: {displayData.wealth}");
            }
            else
            {
                Debug.LogError($"DisplayRegion UI elements not ready: titleText={titleText != null}, wealthText={wealthText != null}");
            }
        }
        
        /// <summary>
        /// Update the display with prepared data
        /// </summary>
        public void UpdateDisplay(RegionStatsViewModel.RegionDisplayData data)
        {
            Debug.Log("UpdateDisplay called with fresh display data");
            
            if (titleText != null && wealthText != null)
            {
                titleText.text = data.regionName;
                wealthText.text = "Wealth: " + data.wealth;
                Debug.Log($"Updated region display with wealth: {data.wealth}");
            }
            else
            {
                Debug.LogError("Cannot update display - UI elements are null");
            }
            
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
            
            // Configure panel size and position
            RectTransform panelRect = statsPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
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
            
            // Create title
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(statsPanel.transform, false);
            
            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "REGION NAME";
            titleText.fontSize = 18;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = headerColor;
            titleText.alignment = TextAlignmentOptions.Center;
            
            LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = 30;
            titleLayout.minHeight = 25;
            
            // Create divider
            GameObject dividerObj = new GameObject("Divider");
            dividerObj.transform.SetParent(statsPanel.transform, false);
            
            Image dividerImage = dividerObj.AddComponent<Image>();
            dividerImage.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            
            LayoutElement dividerLayout = dividerObj.AddComponent<LayoutElement>();
            dividerLayout.preferredHeight = 2;
            dividerLayout.flexibleWidth = 1;
            dividerLayout.minHeight = 2;
            
            // Create wealth text
            GameObject wealthObj = new GameObject("WealthText");
            wealthObj.transform.SetParent(statsPanel.transform, false);
            
            wealthText = wealthObj.AddComponent<TextMeshProUGUI>();
            wealthText.text = "Wealth: 0";
            wealthText.fontSize = 16;
            wealthText.color = Color.white;
            wealthText.alignment = TextAlignmentOptions.Center;
            
            LayoutElement wealthLayout = wealthObj.AddComponent<LayoutElement>();
            wealthLayout.preferredHeight = 25;
            wealthLayout.minHeight = 20;
            
            Debug.Log("UI elements created successfully");
            
            // Hide panel initially until a region is set
            statsPanel.SetActive(false);
        }
    }
}