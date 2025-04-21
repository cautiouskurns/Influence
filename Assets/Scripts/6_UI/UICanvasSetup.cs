using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace UI
{
    /// <summary>
    /// Helper class to set up a UI Canvas with simulation controls
    /// </summary>
    public class UICanvasSetup : MonoBehaviour
    {
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
        
        private SimulationController simulationController;
        
        [ContextMenu("Setup Simulation UI")]
        public void SetupUI()
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
            
            Debug.Log("UI Canvas setup complete!");
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
    }
}