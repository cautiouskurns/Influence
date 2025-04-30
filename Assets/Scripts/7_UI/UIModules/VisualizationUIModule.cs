using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    /// <summary>
    /// UI Module for visualization controls (color mapping options)
    /// </summary>
    public class VisualizationUIModule : UIModuleBase
    {
        [Header("References")]
        [SerializeField] private MapManager mapManager;
        
        [Header("UI Controls")]
        [SerializeField] private Button defaultColorButton;
        [SerializeField] private Button positionColorButton;
        [SerializeField] private Button wealthColorButton; 
        [SerializeField] private Button productionColorButton;
        [SerializeField] private Button nationColorButton; // Added nation color button
        [SerializeField] private Button terrainColorButton; // New terrain color button
        
        [Header("Legend")]
        [SerializeField] private GameObject legendPanel;
        [SerializeField] private TextMeshProUGUI legendTitle;
        [SerializeField] private Image minColorImage;
        [SerializeField] private Image maxColorImage;
        [SerializeField] private TextMeshProUGUI minValueText;
        [SerializeField] private TextMeshProUGUI maxValueText;
        
        [Header("Visual Settings")]
        [SerializeField] private Color buttonColor = new Color(0.2f, 0.2f, 0.3f);
        [SerializeField] private Color buttonTextColor = Color.white;
        [SerializeField] private Vector2 buttonSize = new Vector2(100, 40);
        [SerializeField] private float buttonSpacing = 10f;
        
        // Reference to the MapColorController
        private MapColorController colorController;
        
        public override void Initialize()
        {
            base.Initialize();
            
            // Find MapManager if not set
            if (mapManager == null)
            {
                mapManager = FindFirstObjectByType<MapManager>();
                if (mapManager == null)
                {
                    Debug.LogError("VisualizationUIModule: No MapManager found in scene");
                    return;
                }
            }
            
            SetupUI();
            CreateOrUpdateColorController();
        }
        
        private void SetupUI()
        {
            // Add horizontal layout group if needed
            HorizontalLayoutGroup layout = GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
            {
                layout = gameObject.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = buttonSpacing;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
                layout.padding = new RectOffset(10, 10, 5, 5);
            }
            
            // Create control buttons if they don't exist
            if (defaultColorButton == null)
            {
                defaultColorButton = CreateButton("DefaultColorButton", "Default");
            }
            
            if (positionColorButton == null)
            {
                positionColorButton = CreateButton("PositionColorButton", "Position");
            }
            
            if (wealthColorButton == null)
            {
                wealthColorButton = CreateButton("WealthColorButton", "Wealth");
            }
            
            if (productionColorButton == null)
            {
                productionColorButton = CreateButton("ProductionColorButton", "Production");
            }
            
            if (nationColorButton == null)
            {
                nationColorButton = CreateButton("NationColorButton", "Nation");
            }
            
            if (terrainColorButton == null)
            {
                terrainColorButton = CreateButton("TerrainColorButton", "Terrain");
            }
            
            // Create legend panel if it doesn't exist
            if (legendPanel == null)
            {
                CreateLegendPanel();
            }
        }
        
        private Button CreateButton(string name, string text)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(transform, false);
            
            // Add image component for background
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = buttonColor;
            
            // Add button component
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            
            // Set button colors
            ColorBlock colors = button.colors;
            colors.normalColor = buttonColor;
            colors.highlightedColor = new Color(buttonColor.r + 0.1f, buttonColor.g + 0.1f, buttonColor.b + 0.1f, 1f);
            colors.pressedColor = new Color(buttonColor.r - 0.1f, buttonColor.g - 0.1f, buttonColor.b - 0.1f, 1f);
            button.colors = colors;
            
            // Add text child
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = 16;
            textComponent.color = buttonTextColor;
            textComponent.alignment = TextAlignmentOptions.Center;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            // Set button size
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.sizeDelta = buttonSize;
            
            // Add layout element
            LayoutElement buttonLayout = buttonObj.AddComponent<LayoutElement>();
            buttonLayout.preferredWidth = buttonSize.x;
            buttonLayout.preferredHeight = buttonSize.y;
            
            return button;
        }
        
        private void CreateLegendPanel()
        {
            // Create legend panel
            GameObject panel = new GameObject("LegendPanel");
            legendPanel = panel;
            
            // Position the panel directly in the main canvas to ensure it's in the top-right
            Canvas mainCanvas = FindFirstObjectByType<Canvas>();
            if (mainCanvas != null)
            {
                panel.transform.SetParent(mainCanvas.transform, false);
            }
            else
            {
                panel.transform.SetParent(transform.root, false);
            }
            
            // Configure the panel with correct anchoring for top-right corner
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 1);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.pivot = new Vector2(1, 1);
            panelRect.sizeDelta = new Vector2(180, 200);
            panelRect.anchoredPosition = new Vector2(-20, -20);
            
            // Add panel components
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            // Add vertical layout
            VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10;
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            
            // Add legend title
            GameObject titleObj = new GameObject("LegendTitle");
            titleObj.transform.SetParent(panel.transform, false);
            
            legendTitle = titleObj.AddComponent<TextMeshProUGUI>();
            legendTitle.text = "Legend";
            legendTitle.fontSize = 18;
            legendTitle.fontStyle = FontStyles.Bold;
            legendTitle.alignment = TextAlignmentOptions.Center;
            
            // Set title height
            LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = 30;
            
            // Add min color container
            GameObject minColorContainer = new GameObject("MinColorContainer");
            minColorContainer.transform.SetParent(panel.transform, false);
            
            // Add horizontal layout for min color
            HorizontalLayoutGroup minLayout = minColorContainer.AddComponent<HorizontalLayoutGroup>();
            minLayout.spacing = 5;
            minLayout.childAlignment = TextAnchor.MiddleLeft;
            minLayout.childForceExpandWidth = false;
            
            // Min color image
            GameObject minColorObj = new GameObject("MinColorImage");
            minColorObj.transform.SetParent(minColorContainer.transform, false);
            
            minColorImage = minColorObj.AddComponent<Image>();
            minColorImage.color = Color.red;
            
            LayoutElement minColorLayout = minColorObj.AddComponent<LayoutElement>();
            minColorLayout.preferredWidth = 20;
            minColorLayout.preferredHeight = 20;
            
            // Min color label
            GameObject minLabelObj = new GameObject("MinValueText");
            minLabelObj.transform.SetParent(minColorContainer.transform, false);
            
            minValueText = minLabelObj.AddComponent<TextMeshProUGUI>();
            minValueText.text = "Minimum";
            minValueText.fontSize = 14;
            minValueText.alignment = TextAlignmentOptions.Left;
            
            // Add max color container
            GameObject maxColorContainer = new GameObject("MaxColorContainer");
            maxColorContainer.transform.SetParent(panel.transform, false);
            
            // Add horizontal layout for max color
            HorizontalLayoutGroup maxLayout = maxColorContainer.AddComponent<HorizontalLayoutGroup>();
            maxLayout.spacing = 5;
            maxLayout.childAlignment = TextAnchor.MiddleLeft;
            maxLayout.childForceExpandWidth = false;
            
            // Max color image
            GameObject maxColorObj = new GameObject("MaxColorImage");
            maxColorObj.transform.SetParent(maxColorContainer.transform, false);
            
            maxColorImage = maxColorObj.AddComponent<Image>();
            maxColorImage.color = Color.green;
            
            LayoutElement maxColorLayout = maxColorObj.AddComponent<LayoutElement>();
            maxColorLayout.preferredWidth = 20;
            maxColorLayout.preferredHeight = 20;
            
            // Max color label
            GameObject maxLabelObj = new GameObject("MaxValueText");
            maxLabelObj.transform.SetParent(maxColorContainer.transform, false);
            
            maxValueText = maxLabelObj.AddComponent<TextMeshProUGUI>();
            maxValueText.text = "Maximum";
            maxValueText.fontSize = 14;
            maxValueText.alignment = TextAlignmentOptions.Left;
            
            // Initially hide the legend
            legendPanel.SetActive(false);
        }
        
        private void CreateOrUpdateColorController()
        {
            // Check if the controller already exists
            colorController = GetComponent<MapColorController>();
            if (colorController == null)
            {
                // Create a new controller
                colorController = gameObject.AddComponent<MapColorController>();
            }
            
            // Set controller references
            colorController.mapManager = mapManager;
            colorController.defaultColorButton = defaultColorButton;
            colorController.positionColorButton = positionColorButton;
            colorController.wealthColorButton = wealthColorButton; 
            colorController.productionColorButton = productionColorButton;
            colorController.nationColorButton = nationColorButton;
            colorController.terrainColorButton = terrainColorButton; // Add terrain button reference
            
            // Set legend references
            colorController.legendPanel = legendPanel;
            colorController.legendTitle = legendTitle;
            colorController.minColorImage = minColorImage;
            colorController.maxColorImage = maxColorImage;
            colorController.minValueText = minValueText;
            colorController.maxValueText = maxValueText;
        }
    }
}