using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Systems;
using System.Collections;
using System.Collections.Generic;

namespace UI
{
    /// <summary>
    /// UI Module for adjusting key economic parameters
    /// </summary>
    public class EconomicControlUIModule : UIModuleBase
    {
        [Header("Economic System")]
        [SerializeField] private EconomicSystem economicSystem;
        
        [Header("UI Settings")]
        [SerializeField] private Vector2 panelSize = new Vector2(400f, 350f);
        [SerializeField] private Color panelColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        [SerializeField] private Color headerColor = new Color(0.3f, 0.5f, 0.8f, 1f);
        [SerializeField] private Color buttonColor = new Color(0.2f, 0.6f, 0.3f);
        
        // UI Elements
        private GameObject controlPanel;
        private Dictionary<string, Slider> sliders = new Dictionary<string, Slider>();
        private Dictionary<string, TextMeshProUGUI> valueTexts = new Dictionary<string, TextMeshProUGUI>();
        private Dictionary<string, Button> buttons = new Dictionary<string, Button>();
        private Dictionary<string, Image> buttonImages = new Dictionary<string, Image>();

        // Add to track which parameters have been applied
        private Dictionary<string, bool> parameterApplied = new Dictionary<string, bool>();

        // Parameter ranges
        private Dictionary<string, Vector2> parameterRanges = new Dictionary<string, Vector2>()
        {
            { "ProductivityFactor", new Vector2(0.5f, 2.0f) },
            { "LaborElasticity", new Vector2(0.1f, 0.9f) },
            { "EfficiencyModifier", new Vector2(0.05f, 0.3f) },
            { "BaseConsumptionRate", new Vector2(0.1f, 0.4f) },
            { "DecayRate", new Vector2(0.01f, 0.1f) }
        };

        public override void Initialize()
        {
            base.Initialize();
            
            // Find EconomicSystem if not set in inspector
            if (economicSystem == null)
                economicSystem = FindFirstObjectByType<EconomicSystem>();
                
            if (economicSystem == null)
            {
                Debug.LogError("EconomicControlUIModule: Failed to find EconomicSystem!");
                return;
            }
            
            CreateUIElements();
            
            // Ensure we check button states periodically
            InvokeRepeating(nameof(CheckButtonStates), 1f, 1f);
        }
        
        // Check button states periodically
        private void CheckButtonStates()
        {
            foreach (var button in buttons.Values)
            {
                if (button != null && !button.interactable)
                {
                    button.interactable = true;
                    Debug.Log("Restored interactability for button");
                }
            }
        }
        
        private void CreateUIElements()
        {
            // Create main panel if it doesn't exist
            if (controlPanel != null)
                return;
                
            // Create panel
            controlPanel = new GameObject("EconomicControlPanel");
            controlPanel.transform.SetParent(transform, false);
            
            // Add panel image
            Image panelImage = controlPanel.AddComponent<Image>();
            panelImage.color = panelColor;
            panelImage.type = Image.Type.Sliced; // Use sliced image for better borders
            
            // Configure panel size and position - top right
            RectTransform panelRect = controlPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 1);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.pivot = new Vector2(1, 1);
            panelRect.sizeDelta = panelSize;
            panelRect.anchoredPosition = new Vector2(-20, -20);
            
            // Add layout group for content
            VerticalLayoutGroup layout = controlPanel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            
            // Create header
            CreateHeader(controlPanel.transform);
            
            // Create divider
            CreateDivider(controlPanel.transform);
            
            // Create sliders for each parameter
            CreateParameterControl("ProductivityFactor", "Production Factor", controlPanel.transform);
            CreateParameterControl("LaborElasticity", "Labor Factor", controlPanel.transform);
            CreateParameterControl("EfficiencyModifier", "Infrastructure Efficiency", controlPanel.transform); 
            CreateParameterControl("BaseConsumptionRate", "Consumption Rate", controlPanel.transform);
            CreateParameterControl("DecayRate", "Decay Rate", controlPanel.transform);
            
            // Create divider
            CreateDivider(controlPanel.transform);
            
            // Create a reset button
            CreateResetButton(controlPanel.transform);
            
            // Set initial slider values
            UpdateSlidersFromSystem();
            
            Debug.Log("Economic Control UI Module initialized with " + buttons.Count + " buttons");
        }
        
        private void CreateHeader(Transform parent)
        {
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(parent, false);
            
            TextMeshProUGUI headerText = headerObj.AddComponent<TextMeshProUGUI>();
            headerText.text = "ECONOMIC CONTROLS";
            headerText.fontSize = 20;
            headerText.fontStyle = FontStyles.Bold;
            headerText.alignment = TextAlignmentOptions.Center;
            headerText.color = headerColor;
            
            LayoutElement headerLayout = headerObj.AddComponent<LayoutElement>();
            headerLayout.preferredHeight = 30;
            headerLayout.minHeight = 30;
        }
        
        private void CreateDivider(Transform parent)
        {
            GameObject dividerObj = new GameObject("Divider");
            dividerObj.transform.SetParent(parent, false);
            
            Image dividerImage = dividerObj.AddComponent<Image>();
            dividerImage.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
            
            LayoutElement dividerLayout = dividerObj.AddComponent<LayoutElement>();
            dividerLayout.preferredHeight = 2;
            dividerLayout.minHeight = 2;
            dividerLayout.flexibleWidth = 1;
        }
        
        private void CreateParameterControl(string paramName, string displayName, Transform parent)
        {
            GameObject containerObj = new GameObject(paramName + "Container");
            containerObj.transform.SetParent(parent, false);
            
            // Setup horizontal layout for this parameter
            HorizontalLayoutGroup containerLayout = containerObj.AddComponent<HorizontalLayoutGroup>();
            containerLayout.childAlignment = TextAnchor.MiddleLeft;
            containerLayout.childControlWidth = true;
            containerLayout.childForceExpandWidth = false;
            containerLayout.spacing = 10;
            
            LayoutElement containerElement = containerObj.AddComponent<LayoutElement>();
            containerElement.preferredHeight = 40;
            containerElement.minHeight = 30;
            containerElement.flexibleWidth = 1;
            
            // Add label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(containerObj.transform, false);
            
            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = displayName + ":";
            labelText.fontSize = 14;
            labelText.alignment = TextAlignmentOptions.Left;
            
            LayoutElement labelElement = labelObj.AddComponent<LayoutElement>();
            labelElement.preferredWidth = 120;
            labelElement.minWidth = 100;
            labelElement.flexibleWidth = 0;
            
            // Create slider
            GameObject sliderObj = new GameObject("Slider");
            sliderObj.transform.SetParent(containerObj.transform, false);
            
            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 0.5f;
            
            LayoutElement sliderElement = sliderObj.AddComponent<LayoutElement>();
            sliderElement.preferredWidth = 120;
            sliderElement.flexibleWidth = 1;
            
            // Add required slider parts
            CreateSliderParts(sliderObj);
            
            // Add value text
            GameObject valueObj = new GameObject("Value");
            valueObj.transform.SetParent(containerObj.transform, false);
            
            TextMeshProUGUI valueText = valueObj.AddComponent<TextMeshProUGUI>();
            valueText.text = "0.00";
            valueText.fontSize = 14;
            valueText.alignment = TextAlignmentOptions.Left;
            
            LayoutElement valueElement = valueObj.AddComponent<LayoutElement>();
            valueElement.preferredWidth = 50;
            valueElement.minWidth = 50;
            valueElement.flexibleWidth = 0;
            
            // Add apply button with improved setup - this is the key change
            GameObject buttonObj = new GameObject("ApplyButton");
            buttonObj.transform.SetParent(containerObj.transform, false);
            
            // Add button image first - required for proper button setup
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = buttonColor;
            
            // Set layout constraints with specific size to avoid scaling issues
            LayoutElement buttonElement = buttonObj.AddComponent<LayoutElement>();
            buttonElement.preferredWidth = 60;
            buttonElement.preferredHeight = 30;
            buttonElement.flexibleWidth = 0;
            buttonElement.minWidth = 60;
            
            // Add button component after establishing layout
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            
            // Set button colors with more pronounced feedback
            ColorBlock colors = button.colors;
            colors.normalColor = buttonColor;
            colors.highlightedColor = new Color(buttonColor.r + 0.15f, buttonColor.g + 0.15f, buttonColor.b + 0.15f);
            colors.pressedColor = new Color(buttonColor.r - 0.15f, buttonColor.g - 0.15f, buttonColor.b - 0.15f);
            colors.selectedColor = new Color(buttonColor.r + 0.1f, buttonColor.g + 0.1f, buttonColor.b + 0.1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            
            // Add text to button
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Apply";
            buttonText.fontSize = 12;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.raycastTarget = false; // Important - prevent text from blocking button clicks
            
            // Set text to fill button area
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
            
            // Store references
            sliders[paramName] = slider;
            valueTexts[paramName] = valueText;
            buttons[paramName] = button;
            buttonImages[paramName] = buttonImage;
            parameterApplied[paramName] = false;
            
            // Setup slider value change event
            slider.onValueChanged.AddListener((value) => {
                // Convert normalized value to actual parameter value
                float actualValue = Mathf.Lerp(parameterRanges[paramName].x, parameterRanges[paramName].y, value);
                // Update the display
                valueText.text = actualValue.ToString("F2");
                // Mark parameter as not applied
                parameterApplied[paramName] = false;
                // Update button color
                buttonImage.color = buttonColor;
            });
            
            // Improved button click event with direct reference
            string capturedParamName = paramName; // Capture for closure
            button.onClick.AddListener(delegate {
                OnApplyButtonClicked(capturedParamName, buttonObj, valueText);
            });
        }
        
        // Separated button click handler to improve debugging
        private void OnApplyButtonClicked(string paramName, GameObject buttonObj, TextMeshProUGUI valueText)
        {
            Debug.Log($"Apply button clicked for {paramName}");
            
            // Apply the parameter value to the system
            ApplyValueToSystem(paramName);
            
            // Show visual feedback
            ShowButtonFeedback(buttonObj, valueText);
            
            // Mark parameter as applied
            parameterApplied[paramName] = true;
            
            // Update button color
            if (buttonImages.ContainsKey(paramName))
            {
                buttonImages[paramName].color = new Color(0.5f, 0.5f, 0.5f); // Change to a different color to indicate applied
            }
        }
        
        private void CreateSliderParts(GameObject sliderObj)
        {
            // Create background
            GameObject background = new GameObject("Background");
            background.transform.SetParent(sliderObj.transform, false);
            
            Image backgroundImage = background.AddComponent<Image>();
            backgroundImage.color = new Color(0.1f, 0.1f, 0.1f);
            
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.sizeDelta = Vector2.zero;
            
            // Create fill area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            if (fillAreaRect == null)
                fillAreaRect = fillArea.AddComponent<RectTransform>();
                
            fillAreaRect.anchorMin = new Vector2(0, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1, 0.75f);
            fillAreaRect.offsetMin = new Vector2(5, 0);
            fillAreaRect.offsetMax = new Vector2(-5, 0);
            
            // Create fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.2f, 0.6f, 1);
            
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            
            // Create handle area
            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderObj.transform, false);
            
            RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
            if (handleAreaRect == null)
                handleAreaRect = handleArea.AddComponent<RectTransform>();
                
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(10, 0);
            handleAreaRect.offsetMax = new Vector2(-10, 0);
            
            // Create handle
            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            
            Image handleImage = handle.AddComponent<Image>();
            handleImage.color = Color.white;
            
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 0);
            
            // Connect the slider to its parts
            Slider slider = sliderObj.GetComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
        }
        
        private void CreateResetButton(Transform parent)
        {
            GameObject buttonObj = new GameObject("ResetButton");
            buttonObj.transform.SetParent(parent, false);
            
            // Add button image
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.8f, 0.2f, 0.2f);
            
            // Set button size and layout first
            LayoutElement buttonLayout = buttonObj.AddComponent<LayoutElement>();
            buttonLayout.preferredHeight = 35;
            buttonLayout.flexibleWidth = 1;
            buttonLayout.minHeight = 35;
            
            // Add button component after layout
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            
            // Set button colors with stronger feedback
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.8f, 0.2f, 0.2f);
            colors.highlightedColor = new Color(1.0f, 0.3f, 0.3f);
            colors.pressedColor = new Color(0.6f, 0.1f, 0.1f);
            colors.selectedColor = new Color(0.9f, 0.25f, 0.25f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            
            // Add text to button
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "RESET TO DEFAULTS";
            buttonText.fontSize = 14;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.raycastTarget = false; // Prevent text from blocking clicks
            
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
            
            // Improved click handler
            button.onClick.AddListener(delegate {
                OnResetButtonClicked(buttonObj, buttonText);
            });
        }
        
        // Separate reset button handler
        private void OnResetButtonClicked(GameObject buttonObj, TextMeshProUGUI buttonText)
        {
            Debug.Log("Reset button clicked");
            
            // Reset parameters
            ResetAllParameters();
            
            // Visual feedback
            ShowButtonFeedback(buttonObj, buttonText);
        }
        
        private void ApplyValueToSystem(string paramName)
        {
            if (economicSystem == null || !sliders.ContainsKey(paramName)) {
                Debug.LogError($"Cannot apply parameter {paramName}: economicSystem or slider not found");
                return;
            }
            
            // Get slider value and map to parameter range
            Slider slider = sliders[paramName];
            Vector2 range = parameterRanges[paramName];
            float actualValue = Mathf.Lerp(range.x, range.y, slider.value);
            
            // Set the value directly on the economic system
            Debug.Log($"Setting {paramName} to {actualValue}");
            
            try {
                switch (paramName)
                {
                    case "ProductivityFactor":
                        economicSystem.productivityFactor = actualValue;
                        break;
                    case "LaborElasticity":
                        economicSystem.laborElasticity = actualValue;
                        break;
                    case "EfficiencyModifier":
                        economicSystem.efficiencyModifier = actualValue;
                        break;
                    case "BaseConsumptionRate":
                        economicSystem.baseConsumptionRate = actualValue;
                        break;
                    case "DecayRate":
                        economicSystem.decayRate = actualValue;
                        break;
                    default:
                        Debug.LogWarning($"Unknown parameter: {paramName}");
                        return;
                }
                
                // Confirm success
                Debug.Log($"Successfully updated {paramName} = {actualValue}");
            }
            catch (System.Exception e) {
                Debug.LogError($"Error setting {paramName}: {e.Message}");
            }
        }
        
        // Improved visual feedback with better isolation
        private void ShowButtonFeedback(GameObject buttonObj, TextMeshProUGUI valueText)
        {
            if (buttonObj == null) {
                Debug.LogError("Cannot show feedback: button object is null");
                return;
            }
            
            // Create a standalone feedback object
            GameObject feedbackObj = new GameObject("ButtonFeedback");
            feedbackObj.transform.SetParent(buttonObj.transform, false);
            
            // Clear any existing feedback
            foreach (Transform child in buttonObj.transform) {
                if (child.name == "ButtonFeedback" && child.gameObject != feedbackObj) {
                    Destroy(child.gameObject);
                }
            }
            
            // Add flash effect with raycast targets disabled
            Image feedbackImage = feedbackObj.AddComponent<Image>();
            feedbackImage.color = new Color(1f, 1f, 1f, 0.5f);
            feedbackImage.raycastTarget = false;
            
            // Position feedback to cover the button
            RectTransform feedbackRect = feedbackObj.GetComponent<RectTransform>();
            feedbackRect.anchorMin = Vector2.zero;
            feedbackRect.anchorMax = Vector2.one;
            feedbackRect.offsetMin = Vector2.zero;
            feedbackRect.offsetMax = Vector2.zero;
            
            // Add checkmark
            GameObject checkObj = new GameObject("Checkmark");
            checkObj.transform.SetParent(feedbackObj.transform, false);
            
            TextMeshProUGUI checkText = checkObj.AddComponent<TextMeshProUGUI>();
            checkText.text = "✓";
            checkText.fontSize = 20;
            checkText.fontStyle = FontStyles.Bold;
            checkText.color = Color.green;
            checkText.alignment = TextAlignmentOptions.Center;
            checkText.raycastTarget = false;
            
            RectTransform checkRect = checkObj.GetComponent<RectTransform>();
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.offsetMin = Vector2.zero;
            checkRect.offsetMax = Vector2.zero;
            
            // Make value text bold temporarily if provided
            if (valueText != null) {
                FontStyles originalStyle = valueText.fontStyle;
                valueText.fontStyle = FontStyles.Bold;
                
                // Reset text style after delay using a unique coroutine name
                StartCoroutine(ResetTextStyleAfterDelay(valueText, originalStyle, 0.5f));
            }
            
            // Destroy feedback after a delay
            Destroy(feedbackObj, 0.5f);
        }
        
        // Renamed to avoid any potential conflicts
        private IEnumerator ResetTextStyleAfterDelay(TextMeshProUGUI text, FontStyles originalStyle, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (text != null)
            {
                text.fontStyle = originalStyle;
            }
        }
        
        private void ResetAllParameters()
        {
            if (economicSystem == null) {
                Debug.LogError("Cannot reset parameters: economicSystem is null");
                return;
            }
            
            try {
                // Reset all parameters to their default values
                economicSystem.productivityFactor = 1.0f;
                economicSystem.laborElasticity = 0.5f;
                economicSystem.efficiencyModifier = 0.1f;
                economicSystem.baseConsumptionRate = 0.2f;
                economicSystem.decayRate = 0.02f;
                economicSystem.enableEconomicCycles = true;
                
                // Update UI
                UpdateSlidersFromSystem();
                
                Debug.Log("Successfully reset all economic parameters to defaults");
            }
            catch (System.Exception e) {
                Debug.LogError($"Error resetting parameters: {e.Message}");
            }
        }
        
        private void UpdateSlidersFromSystem()
        {
            if (economicSystem == null) return;
            
            // Update sliders to match current system values
            UpdateParameterSlider("ProductivityFactor", economicSystem.productivityFactor);
            UpdateParameterSlider("LaborElasticity", economicSystem.laborElasticity);
            UpdateParameterSlider("EfficiencyModifier", economicSystem.efficiencyModifier);
            UpdateParameterSlider("BaseConsumptionRate", economicSystem.baseConsumptionRate);
            UpdateParameterSlider("DecayRate", economicSystem.decayRate);
        }
        
        private void UpdateParameterSlider(string paramName, float actualValue)
        {
            if (!sliders.ContainsKey(paramName) || !parameterRanges.ContainsKey(paramName)) return;
            
            try {
                // Map the actual value to the 0-1 slider range
                Vector2 range = parameterRanges[paramName];
                float sliderValue = Mathf.InverseLerp(range.x, range.y, actualValue);
                sliders[paramName].value = sliderValue;
                
                // Update text display
                if (valueTexts.ContainsKey(paramName))
                {
                    valueTexts[paramName].text = actualValue.ToString("F2");
                }
            }
            catch (System.Exception e) {
                Debug.LogError($"Error updating slider {paramName}: {e.Message}");
            }
        }
        
        // Public methods for manual debugging and fixes
        
        // Add debug log method that can be called from the editor
        [ContextMenu("Log Button States")]
        public void LogButtonStates()
        {
            Debug.Log("=== Button States ===");
            foreach (var kvp in buttons)
            {
                if (kvp.Value != null)
                {
                    Debug.Log($"Button {kvp.Key}: interactable={kvp.Value.interactable}, enabled={kvp.Value.enabled}, gameObject.active={kvp.Value.gameObject.activeSelf}");
                }
                else
                {
                    Debug.Log($"Button {kvp.Key}: NULL REFERENCE");
                }
            }
        }
        
        // Manually force all buttons to be interactable
        [ContextMenu("Force Enable All Buttons")]
        public void ForceEnableAllButtons()
        {
            foreach (var button in buttons.Values)
            {
                if (button != null)
                {
                    button.interactable = true;
                }
            }
            Debug.Log("Forced all buttons to be enabled");
        }
        
        // Make Awake/Start force set interactability
        private void Start()
        {
            // Double-check that all buttons are interactable
            ForceEnableAllButtons();
        }
        
        // Also check buttons on enable
        private void OnEnable()
        {
            ForceEnableAllButtons();
        }
    }
}