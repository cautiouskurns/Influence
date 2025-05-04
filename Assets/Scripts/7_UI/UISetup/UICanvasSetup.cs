using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    /// <summary>
    /// Helper class to set up a UI Canvas with simulation controls.
    /// This class can work standalone or integrate with the UIManager system.
    /// </summary>
    public class UICanvasSetup : MonoBehaviour
    {
        [Header("UI Integration")]
        [SerializeField] private bool useUIManagerSystem = true;
        [SerializeField] private UIManager uiManager;
        
        [Header("UI Prefabs")]
        public Button buttonPrefab;
        public Slider sliderPrefab;
        public TextMeshProUGUI textPrefab;

        [Header("UI References")]
        public Button playButton;
        public Button pauseButton;
        public Button stepButton;
        public Slider speedSlider;
        public TextMeshProUGUI statusText;

        [Header("References")]
        [SerializeField] private MapManager mapManager;

        [Header("Panel Settings")]
        [SerializeField] private float controlPanelHeight = 60f;
        [SerializeField] private float legendPanelWidth = 160f;
        [SerializeField] private float legendPanelHeight = 200f;
        [SerializeField] private Color panelBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        [Header("Button Settings")]
        [SerializeField] private float buttonSpacing = 10f;
        [SerializeField] private Vector2 buttonSize = new Vector2(100f, 40f);
        [SerializeField] private Color buttonColor = new Color(0.2f, 0.2f, 0.3f, 1f);
        [SerializeField] private Color buttonTextColor = Color.white;

        // UI Components
        private Canvas mainCanvas;
        private GameObject controlPanel;
        private GameObject legendPanel;
        private Button[] colorButtons = new Button[4];
        private MapColorController colorController;

        private SimulationController simulationController;
        private SimulationUIModule simulationUIModule;
        private VisualizationUIModule visualizationUIModule;

        private void Awake()
        {
            if (mapManager == null)
            {
                mapManager = FindFirstObjectByType<MapManager>();
            }
            
            if (useUIManagerSystem && uiManager == null)
            {
                uiManager = FindFirstObjectByType<UIManager>();
                if (uiManager == null)
                {
                    Debug.Log("No UIManager found. Creating one...");
                    GameObject uiManagerObj = new GameObject("UIManager");
                    uiManager = uiManagerObj.AddComponent<UIManager>();
                }
            }
        }

        [ContextMenu("Setup Simulation UI")]
        public void SetupUI()
        {
            if (useUIManagerSystem && uiManager != null)
            {
                SetupUIWithManager();
            }
            else
            {
                SetupUIStandalone();
            }
        }
        
        /// <summary>
        /// Sets up UI using the UIManager system
        /// </summary>
        private void SetupUIWithManager()
        {
            // Initialize UIManager if not already initialized
            if (uiManager.gameObject.GetComponentsInChildren<RectTransform>().Length <= 1)
            {
                uiManager.Initialize();
            }
            
            // Create or get SimulationUIModule
            simulationUIModule = uiManager.GetModule<SimulationUIModule>();
            if (simulationUIModule == null)
            {
                Debug.Log("Creating SimulationUIModule through UIManager");
                simulationUIModule = uiManager.CreateModule<SimulationUIModule>(UIPosition.Bottom);
            }
            
            // Create or get VisualizationUIModule
            visualizationUIModule = uiManager.GetModule<VisualizationUIModule>();
            if (visualizationUIModule == null)
            {
                Debug.Log("Creating VisualizationUIModule through UIManager");
                visualizationUIModule = uiManager.CreateModule<VisualizationUIModule>(UIPosition.Bottom);
            }
            
            // Initialize UI modules
            simulationUIModule.Initialize();
            visualizationUIModule.Initialize();
            
            // Store references to UI elements for backwards compatibility
            if (simulationUIModule != null)
            {
                System.Type type = simulationUIModule.GetType();
                playButton = (Button)type.GetField("playButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(simulationUIModule);
                pauseButton = (Button)type.GetField("pauseButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(simulationUIModule);
                stepButton = (Button)type.GetField("stepButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(simulationUIModule);
                speedSlider = (Slider)type.GetField("speedSlider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(simulationUIModule);
                statusText = (TextMeshProUGUI)type.GetField("statusText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(simulationUIModule);
            }
            
            Debug.Log("UI Setup complete with UIManager integration!");
        }
        
        /// <summary>
        /// Sets up UI in standalone mode (original behavior)
        /// </summary>
        private void SetupUIStandalone()
        {
            // Make sure we have a Canvas component
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                Debug.Log("Added Canvas component");

                // Add Canvas Scaler
                CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                Debug.Log("Added CanvasScaler component");

                // Add Graphics Raycaster
                gameObject.AddComponent<GraphicRaycaster>();
                Debug.Log("Added GraphicRaycaster component");
            }

            // Create a panel for the controls
            GameObject controlPanel = CreateOrGetGameObject("ControlPanel");
            RectTransform panelRect = controlPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 0.1f);
            panelRect.offsetMin = new Vector2(10, 10);
            panelRect.offsetMax = new Vector2(-10, 10);

            // Add an image component to the panel for background
            Image panelImage = controlPanel.GetComponent<Image>();
            if (panelImage == null)
            {
                panelImage = controlPanel.AddComponent<Image>();
                panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            }

            // Create simulation buttons
            playButton = CreateButton("PlayButton", "Play", new Vector2(0.1f, 0.5f));
            pauseButton = CreateButton("PauseButton", "Pause", new Vector2(0.2f, 0.5f));
            stepButton = CreateButton("StepButton", "Step", new Vector2(0.3f, 0.5f));

            // Create speed slider
            speedSlider = CreateSlider("SpeedSlider", "Speed", new Vector2(0.5f, 0.5f), 0.2f);

            // Create status text
            statusText = CreateText("StatusText", "Status: Ready", new Vector2(0.8f, 0.5f));

            // Add SimulationController component if needed
            simulationController = GetComponent<SimulationController>();
            if (simulationController == null)
            {
                simulationController = gameObject.AddComponent<SimulationController>();
                Debug.Log("Added SimulationController component");
            }

            // Assign UI elements to the controller
            simulationController.playButton = playButton;
            simulationController.pauseButton = pauseButton;
            simulationController.stepButton = stepButton;
            simulationController.speedSlider = speedSlider;
            simulationController.statusText = statusText;

            Debug.Log("UI Canvas setup complete in standalone mode!");
        }

        [ContextMenu("Setup UI Canvas")]
        public void SetupUICanvas()
        {
            if (useUIManagerSystem && uiManager != null)
            {
                // Initialize UIManager
                uiManager.Initialize();
                
                // Create visualization UI module if it doesn't exist
                visualizationUIModule = uiManager.GetModule<VisualizationUIModule>();
                if (visualizationUIModule == null)
                {
                    visualizationUIModule = uiManager.CreateModule<VisualizationUIModule>(UIPosition.Bottom);
                    visualizationUIModule.Initialize();
                }
                
                Debug.Log("UI Canvas setup complete with UIManager!");
            }
            else
            {
                // Use traditional setup
                CreateUICanvas();
                CreateControlPanel();
                CreateLegendPanel();
                CreateControllerObject();
            }
        }

        private void CreateUICanvas()
        {
            // Check if canvas already exists
            Canvas existingCanvas = FindFirstObjectByType<Canvas>();
            if (existingCanvas != null && existingCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                mainCanvas = existingCanvas;
                Debug.Log("Using existing Canvas");
            }
            else
            {
                // Create new canvas
                GameObject canvasObj = new GameObject("UI_Canvas");
                mainCanvas = canvasObj.AddComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                // Add canvas scaler
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                // Add graphic raycaster
                canvasObj.AddComponent<GraphicRaycaster>();

                Debug.Log("Created new Canvas");
            }

            // Check if event system exists
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("Created new EventSystem");
            }
        }

        private void CreateControlPanel()
        {
            // Create control panel at the bottom of the screen
            controlPanel = new GameObject("VisualizationControls");
            controlPanel.transform.SetParent(mainCanvas.transform, false);

            // Add panel components
            Image panelImage = controlPanel.AddComponent<Image>();
            panelImage.color = panelBackgroundColor;

            // Set panel size and position
            RectTransform panelRect = controlPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 0);
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.sizeDelta = new Vector2(0, controlPanelHeight);
            panelRect.anchoredPosition = Vector2.zero;

            // Create horizontal layout group for buttons
            HorizontalLayoutGroup layout = controlPanel.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = buttonSpacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Create buttons
            string[] buttonLabels = { "Default", "Position", "Wealth", "Production" };
            string[] buttonNames = { "DefaultColorButton", "PositionColorButton", "WealthColorButton", "ProductionColorButton" };

            for (int i = 0; i < 4; i++)
            {
                // Create button
                GameObject buttonObj = new GameObject(buttonNames[i]);
                buttonObj.transform.SetParent(controlPanel.transform, false);

                // Add button components
                Image buttonImage = buttonObj.AddComponent<Image>();
                buttonImage.color = buttonColor;

                Button button = buttonObj.AddComponent<Button>();
                button.targetGraphic = buttonImage;
                ColorBlock colors = button.colors;
                colors.highlightedColor = new Color(buttonColor.r + 0.1f, buttonColor.g + 0.1f, buttonColor.b + 0.1f, 1f);
                colors.pressedColor = new Color(buttonColor.r - 0.1f, buttonColor.g - 0.1f, buttonColor.b - 0.1f, 1f);
                button.colors = colors;

                // Set button size
                RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
                buttonRect.sizeDelta = buttonSize;

                // Add text
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(buttonObj.transform, false);

                TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
                text.text = buttonLabels[i];
                text.color = buttonTextColor;
                text.fontSize = 16;
                text.alignment = TextAlignmentOptions.Center;

                // Set text size
                RectTransform textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;

                // Store button reference
                colorButtons[i] = button;
            }

            Debug.Log("Created control panel with buttons");
        }

        private void CreateLegendPanel()
        {
            // Create legend panel at the right side of the screen
            legendPanel = new GameObject("LegendPanel");
            legendPanel.transform.SetParent(mainCanvas.transform, false);

            // Add panel components
            Image panelImage = legendPanel.AddComponent<Image>();
            panelImage.color = panelBackgroundColor;

            // Set panel size and position
            RectTransform panelRect = legendPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 0.5f);
            panelRect.anchorMax = new Vector2(1, 0.5f);
            panelRect.pivot = new Vector2(1, 0.5f);
            panelRect.sizeDelta = new Vector2(legendPanelWidth, legendPanelHeight);
            panelRect.anchoredPosition = new Vector2(-20, 0);

            // Create vertical layout
            VerticalLayoutGroup layout = legendPanel.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10;
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Add legend title
            GameObject titleObj = new GameObject("LegendTitle");
            titleObj.transform.SetParent(legendPanel.transform, false);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Legend";
            titleText.fontSize = 18;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;

            // Set title height
            LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = 30;

            // Add min color image and label
            GameObject minColorContainer = new GameObject("MinColorContainer");
            minColorContainer.transform.SetParent(legendPanel.transform, false);

            // Add horizontal layout for min color
            HorizontalLayoutGroup minLayout = minColorContainer.AddComponent<HorizontalLayoutGroup>();
            minLayout.spacing = 5;
            minLayout.childAlignment = TextAnchor.MiddleLeft;
            minLayout.childForceExpandWidth = false;

            // Min color image
            GameObject minColorObj = new GameObject("MinColorImage");
            minColorObj.transform.SetParent(minColorContainer.transform, false);

            Image minColorImage = minColorObj.AddComponent<Image>();
            minColorImage.color = Color.red;

            LayoutElement minColorLayout = minColorObj.AddComponent<LayoutElement>();
            minColorLayout.preferredWidth = 20;
            minColorLayout.preferredHeight = 20;

            // Min color label
            GameObject minLabelObj = new GameObject("MinValueText");
            minLabelObj.transform.SetParent(minColorContainer.transform, false);

            TextMeshProUGUI minLabel = minLabelObj.AddComponent<TextMeshProUGUI>();
            minLabel.text = "Minimum";
            minLabel.fontSize = 14;
            minLabel.alignment = TextAlignmentOptions.Left;

            // Add max color image and label
            GameObject maxColorContainer = new GameObject("MaxColorContainer");
            maxColorContainer.transform.SetParent(legendPanel.transform, false);

            // Add horizontal layout for max color
            HorizontalLayoutGroup maxLayout = maxColorContainer.AddComponent<HorizontalLayoutGroup>();
            maxLayout.spacing = 5;
            maxLayout.childAlignment = TextAnchor.MiddleLeft;
            maxLayout.childForceExpandWidth = false;

            // Max color image
            GameObject maxColorObj = new GameObject("MaxColorImage");
            maxColorObj.transform.SetParent(maxColorContainer.transform, false);

            Image maxColorImage = maxColorObj.AddComponent<Image>();
            maxColorImage.color = Color.green;

            LayoutElement maxColorLayout = maxColorObj.AddComponent<LayoutElement>();
            maxColorLayout.preferredWidth = 20;
            maxColorLayout.preferredHeight = 20;

            // Max color label
            GameObject maxLabelObj = new GameObject("MaxValueText");
            maxLabelObj.transform.SetParent(maxColorContainer.transform, false);

            TextMeshProUGUI maxLabel = maxLabelObj.AddComponent<TextMeshProUGUI>();
            maxLabel.text = "Maximum";
            maxLabel.fontSize = 14;
            maxLabel.alignment = TextAlignmentOptions.Left;

            // Initially hide the legend
            legendPanel.SetActive(false);

            Debug.Log("Created legend panel");
        }

        private void CreateControllerObject()
        {
            // Create controller object
            GameObject controllerObj = new GameObject("UI_Controllers");
            colorController = controllerObj.AddComponent<MapColorController>();

            // Set controller references for buttons
            colorController.defaultColorButton = colorButtons[0];
            colorController.positionColorButton = colorButtons[1];
            colorController.wealthColorButton = colorButtons[2];
            colorController.productionColorButton = colorButtons[3];

            // Set legend references
            colorController.legendPanel = legendPanel;
            colorController.legendTitle = legendPanel.transform.Find("LegendTitle").GetComponent<TextMeshProUGUI>();
            colorController.minColorImage = legendPanel.transform.Find("MinColorContainer/MinColorImage").GetComponent<Image>();
            colorController.maxColorImage = legendPanel.transform.Find("MaxColorContainer/MaxColorImage").GetComponent<Image>();
            colorController.minValueText = legendPanel.transform.Find("MinColorContainer/MinValueText").GetComponent<TextMeshProUGUI>();
            colorController.maxValueText = legendPanel.transform.Find("MaxColorContainer/MaxValueText").GetComponent<TextMeshProUGUI>();

            Debug.Log("Created and configured UI controller");
        }

        private GameObject CreateOrGetGameObject(string name)
        {
            Transform existingTransform = transform.Find(name);
            if (existingTransform != null)
                return existingTransform.gameObject;

            GameObject newObject = new GameObject(name);
            newObject.transform.SetParent(transform);
            newObject.AddComponent<RectTransform>();

            RectTransform rectTransform = newObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;

            return newObject;
        }

        private Button CreateButton(string name, string text, Vector2 relativePosition)
        {
            // Try to find existing button
            Transform existingTransform = transform.Find("ControlPanel/" + name);
            if (existingTransform != null && existingTransform.GetComponent<Button>() != null)
                return existingTransform.GetComponent<Button>();

            // Create new button
            GameObject buttonObj;
            Button button;

            if (buttonPrefab != null)
            {
                buttonObj = Instantiate(buttonPrefab.gameObject);
                button = buttonObj.GetComponent<Button>();
            }
            else
            {
                buttonObj = new GameObject(name);
                button = buttonObj.AddComponent<Button>();

                // Add image component
                Image image = buttonObj.AddComponent<Image>();
                image.color = new Color(0.3f, 0.3f, 0.3f);

                // Set up button colors
                ColorBlock colors = button.colors;
                colors.normalColor = new Color(0.3f, 0.3f, 0.3f);
                colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f);
                colors.pressedColor = new Color(0.2f, 0.2f, 0.2f);
                button.colors = colors;

                // Add text
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(buttonObj.transform);

                TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
                textComponent.text = text;
                textComponent.color = Color.white;
                textComponent.alignment = TextAlignmentOptions.Center;
                textComponent.fontSize = 24;

                // Position text
                RectTransform textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
            }

            // Set parent and name
            buttonObj.name = name;
            buttonObj.transform.SetParent(transform.Find("ControlPanel"));

            // Position button
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(relativePosition.x - 0.05f, 0.2f);
            buttonRect.anchorMax = new Vector2(relativePosition.x + 0.05f, 0.8f);
            buttonRect.anchoredPosition = Vector2.zero;
            buttonRect.sizeDelta = Vector2.zero;

            return button;
        }

        private Slider CreateSlider(string name, string labelText, Vector2 relativePosition, float width)
        {
            // Try to find existing slider
            Transform existingTransform = transform.Find("ControlPanel/" + name);
            if (existingTransform != null && existingTransform.GetComponent<Slider>() != null)
                return existingTransform.GetComponent<Slider>();

            // Create new slider
            GameObject sliderObj;
            Slider slider;

            if (sliderPrefab != null)
            {
                sliderObj = Instantiate(sliderPrefab.gameObject);
                slider = sliderObj.GetComponent<Slider>();
            }
            else
            {
                sliderObj = new GameObject(name);
                slider = sliderObj.AddComponent<Slider>();

                // Create background
                GameObject backgroundObj = new GameObject("Background");
                backgroundObj.transform.SetParent(sliderObj.transform);
                Image backgroundImage = backgroundObj.AddComponent<Image>();
                backgroundImage.color = new Color(0.2f, 0.2f, 0.2f);

                // Create fill area
                GameObject fillAreaObj = new GameObject("Fill Area");
                fillAreaObj.transform.SetParent(sliderObj.transform);
                RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();

                // Create fill
                GameObject fillObj = new GameObject("Fill");
                fillObj.transform.SetParent(fillAreaObj.transform);
                Image fillImage = fillObj.AddComponent<Image>();
                fillImage.color = new Color(0.0f, 0.7f, 1.0f);

                // Create handle area
                GameObject handleAreaObj = new GameObject("Handle Slide Area");
                handleAreaObj.transform.SetParent(sliderObj.transform);
                RectTransform handleAreaRect = handleAreaObj.AddComponent<RectTransform>();

                // Create handle
                GameObject handleObj = new GameObject("Handle");
                handleObj.transform.SetParent(handleAreaObj.transform);
                Image handleImage = handleObj.AddComponent<Image>();
                handleImage.color = new Color(1.0f, 1.0f, 1.0f);

                // Set up references
                slider.fillRect = fillObj.GetComponent<RectTransform>();
                slider.handleRect = handleObj.GetComponent<RectTransform>();
                slider.targetGraphic = handleImage;

                // Position elements
                RectTransform backgroundRect = backgroundObj.GetComponent<RectTransform>();
                backgroundRect.anchorMin = new Vector2(0, 0.25f);
                backgroundRect.anchorMax = new Vector2(1, 0.75f);
                backgroundRect.sizeDelta = Vector2.zero;

                fillAreaRect.anchorMin = new Vector2(0, 0.25f);
                fillAreaRect.anchorMax = new Vector2(1, 0.75f);
                fillAreaRect.offsetMin = new Vector2(5, 0);
                fillAreaRect.offsetMax = new Vector2(-5, 0);

                RectTransform fillRect = fillObj.GetComponent<RectTransform>();
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = new Vector2(0.5f, 1f);
                fillRect.sizeDelta = Vector2.zero;

                handleAreaRect.anchorMin = Vector2.zero;
                handleAreaRect.anchorMax = Vector2.one;
                handleAreaRect.offsetMin = new Vector2(10, 0);
                handleAreaRect.offsetMax = new Vector2(-10, 0);

                RectTransform handleRect = handleObj.GetComponent<RectTransform>();
                handleRect.anchorMin = new Vector2(0.5f, 0);
                handleRect.anchorMax = new Vector2(0.5f, 1);
                handleRect.sizeDelta = new Vector2(20, 0);

                // Add label
                GameObject labelObj = new GameObject("Label");
                labelObj.transform.SetParent(sliderObj.transform);
                TextMeshProUGUI labelComponent = labelObj.AddComponent<TextMeshProUGUI>();
                labelComponent.text = labelText;
                labelComponent.color = Color.white;
                labelComponent.alignment = TextAlignmentOptions.Center;
                labelComponent.fontSize = 16;

                RectTransform labelRect = labelObj.GetComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0, 0.8f);
                labelRect.anchorMax = new Vector2(1, 1);
                labelRect.sizeDelta = Vector2.zero;
            }

            // Set parent and name
            sliderObj.name = name;
            sliderObj.transform.SetParent(transform.Find("ControlPanel"));

            // Position slider
            RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(relativePosition.x - width / 2, 0.2f);
            sliderRect.anchorMax = new Vector2(relativePosition.x + width / 2, 0.8f);
            sliderRect.anchoredPosition = Vector2.zero;
            sliderRect.sizeDelta = Vector2.zero;

            // Set default values
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 0.5f;

            return slider;
        }

        private TextMeshProUGUI CreateText(string name, string content, Vector2 relativePosition)
        {
            // Try to find existing text
            Transform existingTransform = transform.Find("ControlPanel/" + name);
            if (existingTransform != null && existingTransform.GetComponent<TextMeshProUGUI>() != null)
                return existingTransform.GetComponent<TextMeshProUGUI>();

            // Create new text
            GameObject textObj;
            TextMeshProUGUI textComponent;

            if (textPrefab != null)
            {
                textObj = Instantiate(textPrefab.gameObject);
                textComponent = textObj.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                textObj = new GameObject(name);
                textComponent = textObj.AddComponent<TextMeshProUGUI>();
                textComponent.alignment = TextAlignmentOptions.Right;
                textComponent.fontSize = 20;
                textComponent.color = Color.white;
            }

            // Set parent and name
            textObj.name = name;
            textObj.transform.SetParent(transform.Find("ControlPanel"));

            // Set text content
            textComponent.text = content;

            // Position text
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(relativePosition.x - 0.15f, 0.2f);
            textRect.anchorMax = new Vector2(relativePosition.x + 0.15f, 0.8f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = Vector2.zero;

            return textComponent;
        }
        
        /// <summary>
        /// Converts this standalone UI to use the UIManager system
        /// </summary>
        [ContextMenu("Convert To UIManager System")]
        public void ConvertToUIManagerSystem()
        {
            if (uiManager == null)
            {
                GameObject uiManagerObj = new GameObject("UIManager");
                uiManager = uiManagerObj.AddComponent<UIManager>();
            }
            
            // Initialize the UI Manager
            uiManager.Initialize();
            
            // Create a SimulationUIModule and transfer existing UI elements
            SimulationUIModule simModule = uiManager.CreateModule<SimulationUIModule>(UIPosition.Bottom);
            
            // Set SimulationUIModule properties using reflection
            if (simModule != null && playButton != null)
            {
                System.Type type = simModule.GetType();
                type.GetField("playButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(simModule, playButton);
                type.GetField("pauseButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(simModule, pauseButton);
                type.GetField("stepButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(simModule, stepButton);
                type.GetField("speedSlider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(simModule, speedSlider);
                type.GetField("statusText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(simModule, statusText);
                
                // Reparent the UI elements to the module
                playButton.transform.SetParent(simModule.transform, true);
                pauseButton.transform.SetParent(simModule.transform, true);
                stepButton.transform.SetParent(simModule.transform, true);
                speedSlider.transform.SetParent(simModule.transform, true);
                statusText.transform.SetParent(simModule.transform, true);
            }
            
            // Create a VisualizationUIModule and transfer existing UI elements
            if (colorButtons[0] != null)
            {
                VisualizationUIModule visModule = uiManager.CreateModule<VisualizationUIModule>(UIPosition.Bottom);
                
                // Set VisualizationUIModule properties using reflection
                System.Type type = visModule.GetType();
                type.GetField("defaultColorButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(visModule, colorButtons[0]);
                type.GetField("positionColorButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(visModule, colorButtons[1]);
                type.GetField("wealthColorButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(visModule, colorButtons[2]);
                type.GetField("productionColorButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(visModule, colorButtons[3]);
                
                if (legendPanel != null)
                {
                    type.GetField("legendPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(visModule, legendPanel);
                    
                    // Transfer legend panel elements
                    Transform legendTitle = legendPanel.transform.Find("LegendTitle");
                    Transform minColorImage = legendPanel.transform.Find("MinColorContainer/MinColorImage");
                    Transform maxColorImage = legendPanel.transform.Find("MaxColorContainer/MaxColorImage");
                    Transform minValueText = legendPanel.transform.Find("MinColorContainer/MinValueText");
                    Transform maxValueText = legendPanel.transform.Find("MaxColorContainer/MaxValueText");
                    
                    if (legendTitle != null && minColorImage != null && maxColorImage != null && 
                        minValueText != null && maxValueText != null)
                    {
                        type.GetField("legendTitle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            .SetValue(visModule, legendTitle.GetComponent<TextMeshProUGUI>());
                        type.GetField("minColorImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            .SetValue(visModule, minColorImage.GetComponent<Image>());
                        type.GetField("maxColorImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            .SetValue(visModule, maxColorImage.GetComponent<Image>());
                        type.GetField("minValueText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            .SetValue(visModule, minValueText.GetComponent<TextMeshProUGUI>());
                        type.GetField("maxValueText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            .SetValue(visModule, maxValueText.GetComponent<TextMeshProUGUI>());
                    }
                    
                    // Reparent the legend panel
                    legendPanel.transform.SetParent(visModule.transform, true);
                }
                
                // Reparent the color buttons
                foreach (Button button in colorButtons)
                {
                    if (button != null)
                    {
                        button.transform.SetParent(visModule.transform, true);
                    }
                }
            }
            
            useUIManagerSystem = true;
            Debug.Log("Successfully converted to UIManager system!");
        }
    }
}