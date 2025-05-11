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
        
        // UI Elements
        private GameObject controlPanel;
        private Dictionary<string, Slider> sliders = new Dictionary<string, Slider>();
        private Dictionary<string, TextMeshProUGUI> valueTexts = new Dictionary<string, TextMeshProUGUI>();
        private Dictionary<string, Button> buttons = new Dictionary<string, Button>();
        
        // Parameter ranges
        private Dictionary<string, Vector2> parameterRanges = new Dictionary<string, Vector2>()
        {
            { "ProductivityFactor", new Vector2(0.5f, 2.0f) },
            { "LaborElasticity", new Vector2(0.1f, 0.9f) },
            { "EfficiencyModifier", new Vector2(0.05f, 0.3f) },
            { "BaseConsumptionRate", new Vector2(0.1f, 0.4f) },
            { "DecayRate", new Vector2(0.01f, 0.1f) }
        };
        
        // Track button click times for debug purposes
        private Dictionary<string, float> lastClickTime = new Dictionary<string, float>();

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
            
            // Add apply button
            GameObject buttonObj = new GameObject("ApplyButton");
            buttonObj.transform.SetParent(containerObj.transform, false);
            
            // Add button image
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.6f, 0.3f);
            
            // Add button component
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            
            // Set button colors
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.6f, 0.3f);
            colors.highlightedColor = new Color(0.3f, 0.7f, 0.4f);
            colors.pressedColor = new Color(0.1f, 0.5f, 0.2f);
            colors.disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            button.colors = colors;
            
            // Add layout element
            LayoutElement buttonElement = buttonObj.AddComponent<LayoutElement>();
            buttonElement.preferredWidth = 40;
            buttonElement.preferredHeight = 25;
            buttonElement.flexibleWidth = 0;
            
            // Add text to button
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Apply";
            buttonText.fontSize = 12;
            buttonText.alignment = TextAlignmentOptions.Center;
            
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
            
            // Store references
            sliders[paramName] = slider;
            valueTexts[paramName] = valueText;
            buttons[paramName] = button;
            lastClickTime[paramName] = -1f;
            
            // Setup slider value change event
            slider.onValueChanged.AddListener(value => {
                float actualValue = Mathf.Lerp(parameterRanges[paramName].x, parameterRanges[paramName].y, value);
                valueText.text = actualValue.ToString("F2");
            });
            
            // Setup apply button click handler - using Unity's native system
            button.onClick.AddListener(() => {
                // Apply parameter 
                ApplyValueToSystem(paramName);
                
                // Visual feedback
                ShowButtonFeedback(buttonObj, valueText);
                
                // Track for debugging
                lastClickTime[paramName] = Time.time;
                Debug.Log($"Button {paramName} clicked at time: {Time.time}");
            });
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
            
            // Add button component
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            
            // Set button colors
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.8f, 0.2f, 0.2f);
            colors.highlightedColor = new Color(0.9f, 0.3f, 0.3f);
            colors.pressedColor = new Color(0.7f, 0.1f, 0.1f);
            button.colors = colors;
            
            // Set button size and layout
            LayoutElement buttonLayout = buttonObj.AddComponent<LayoutElement>();
            buttonLayout.preferredHeight = 35;
            buttonLayout.flexibleWidth = 1;
            
            // Add text to button
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "RESET TO DEFAULTS";
            buttonText.fontSize = 14;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.fontStyle = FontStyles.Bold;
            
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
            
            // Setup callback - using Unity's native system
            button.onClick.AddListener(() => {
                // Reset all parameters
                ResetAllParameters();
                
                // Visual feedback
                ShowButtonFeedback(buttonObj, buttonText);
                
                Debug.Log("Reset button clicked at time: " + Time.time);
            });
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
            
            Debug.Log($"APPLIED {paramName} = {actualValue}");
        }
        
        private void ShowButtonFeedback(GameObject buttonObj, TextMeshProUGUI valueText)
        {
            // 1. Create temporary visual feedback that sits on top of the button
            GameObject feedbackObj = new GameObject("ButtonFeedback");
            feedbackObj.transform.SetParent(buttonObj.transform, false);
            
            // Add an image that flashes
            Image feedbackImage = feedbackObj.AddComponent<Image>();
            feedbackImage.color = new Color(1f, 1f, 1f, 0.5f); // White semi-transparent flash
            
            // Make sure it covers the entire button
            RectTransform feedbackRect = feedbackObj.GetComponent<RectTransform>();
            feedbackRect.anchorMin = Vector2.zero;
            feedbackRect.anchorMax = Vector2.one;
            feedbackRect.offsetMin = Vector2.zero;
            feedbackRect.offsetMax = Vector2.zero;
            
            // Create a checkmark text on top
            GameObject checkObj = new GameObject("Checkmark");
            checkObj.transform.SetParent(feedbackObj.transform, false);
            
            TextMeshProUGUI checkText = checkObj.AddComponent<TextMeshProUGUI>();
            checkText.text = "✓"; // Checkmark
            checkText.fontSize = 20;
            checkText.fontStyle = FontStyles.Bold;
            checkText.color = Color.green;
            checkText.alignment = TextAlignmentOptions.Center;
            
            RectTransform checkRect = checkObj.GetComponent<RectTransform>();
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.offsetMin = Vector2.zero;
            checkRect.offsetMax = Vector2.zero;
            
            // Make text briefly bold
            FontStyles originalStyle = valueText.fontStyle;
            valueText.fontStyle = FontStyles.Bold;
            
            // Destroy after a short time
            Destroy(feedbackObj, 0.5f);
            
            // Return text to normal style after the feedback
            StartCoroutine(ResetTextStyle(valueText, originalStyle, 0.5f));
        }
        
        private IEnumerator ResetTextStyle(TextMeshProUGUI text, FontStyles originalStyle, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (text != null)
            {
                text.fontStyle = originalStyle;
            }
        }
        
        private void ResetAllParameters()
        {
            if (economicSystem == null) return;
            
            // Reset all parameters to their default values
            economicSystem.productivityFactor = 1.0f;
            economicSystem.laborElasticity = 0.5f;
            economicSystem.efficiencyModifier = 0.1f;
            economicSystem.baseConsumptionRate = 0.2f;
            economicSystem.decayRate = 0.02f;
            economicSystem.enableEconomicCycles = true;
            
            // Update UI
            UpdateSlidersFromSystem();
            
            Debug.Log("Reset all economic parameters to defaults");
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
    }
}