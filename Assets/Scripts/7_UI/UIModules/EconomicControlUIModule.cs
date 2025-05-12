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
        [SerializeField] private EconomicSystem economicSystem;
        
        [SerializeField] private Vector2 panelSize = new Vector2(400f, 350f);
        [SerializeField] private Color panelColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        [SerializeField] private Color headerColor = new Color(0.3f, 0.5f, 0.8f, 1f);
        [SerializeField] private Color buttonColor = new Color(0.2f, 0.6f, 0.3f);
        
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
            
            CreateUIPanel();
        }
        
        private void CreateUIPanel()
        {
            // Create main panel
            controlPanel = new GameObject("EconomicControlPanel");
            controlPanel.transform.SetParent(transform, false);
            
            // Set it as the last child to be drawn on top
            controlPanel.transform.SetAsLastSibling();
            
            // Add panel image
            Image panelImage = controlPanel.AddComponent<Image>();
            panelImage.color = panelColor;
            
            // Add a nested Canvas to ensure it renders on top of other UI elements
            Canvas panelCanvas = controlPanel.AddComponent<Canvas>();
            panelCanvas.overrideSorting = true;
            panelCanvas.sortingOrder = 100; // High sorting order to be on top
            
            // Add a GraphicRaycaster to make sure clicks are detected
            controlPanel.AddComponent<GraphicRaycaster>();
            
            // Configure panel position - top right as original
            RectTransform panelRect = controlPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 1);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.pivot = new Vector2(1, 1);
            panelRect.sizeDelta = panelSize;
            panelRect.anchoredPosition = new Vector2(-20, -20);
            
            // Add vertical layout for content
            VerticalLayoutGroup layout = controlPanel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            
            // Create header
            CreateHeader(controlPanel.transform);
            
            // Create divider
            CreateDivider(controlPanel.transform);
            
            // Create sliders for each parameter
            CreateParameterControl("ProductivityFactor", "Productivity", controlPanel.transform);
            CreateParameterControl("LaborElasticity", "Labor", controlPanel.transform);
            CreateParameterControl("EfficiencyModifier", "Efficiency", controlPanel.transform); 
            CreateParameterControl("BaseConsumptionRate", "Consumption", controlPanel.transform);
            CreateParameterControl("DecayRate", "Decay", controlPanel.transform);
            
            // Create divider
            CreateDivider(controlPanel.transform);
            
            // Create a reset button
            CreateResetButton(controlPanel.transform);
            
            // Set initial slider values
            UpdateDisplayValues();
            
            Debug.Log("Economic Control UI created with proper layering");
        }
        
        private void CreateHeader(Transform parent)
        {
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(parent, false);
            
            TextMeshProUGUI headerText = headerObj.AddComponent<TextMeshProUGUI>();
            headerText.text = "ECONOMIC CONTROLS";
            headerText.fontSize = 18;
            headerText.fontStyle = FontStyles.Bold;
            headerText.alignment = TextAlignmentOptions.Center;
            headerText.color = headerColor;
            
            LayoutElement headerLayout = headerObj.AddComponent<LayoutElement>();
            headerLayout.preferredHeight = 30;
            headerLayout.flexibleWidth = 1;
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
            // Create container
            GameObject containerObj = new GameObject(paramName + "Container");
            containerObj.transform.SetParent(parent, false);
            
            // Add layout
            HorizontalLayoutGroup layout = containerObj.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleLeft;
            
            LayoutElement containerElement = containerObj.AddComponent<LayoutElement>();
            containerElement.preferredHeight = 30;
            containerElement.flexibleWidth = 1;
            
            // Create label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(containerObj.transform, false);
            
            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = displayName + ":";
            labelText.fontSize = 14;
            labelText.alignment = TextAlignmentOptions.Left;
            
            LayoutElement labelElement = labelObj.AddComponent<LayoutElement>();
            labelElement.preferredWidth = 100;
            labelElement.flexibleWidth = 0;
            
            // Create slider
            GameObject sliderObj = new GameObject("Slider");
            sliderObj.transform.SetParent(containerObj.transform, false);
            
            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 1;
            
            LayoutElement sliderElement = sliderObj.AddComponent<LayoutElement>();
            sliderElement.preferredWidth = 120;
            sliderElement.flexibleWidth = 1;
            
            CreateSliderBackground(sliderObj);
            CreateSliderFillArea(sliderObj);
            CreateSliderHandle(sliderObj);
            
            // Create value display
            GameObject valueObj = new GameObject("Value");
            valueObj.transform.SetParent(containerObj.transform, false);
            
            TextMeshProUGUI valueText = valueObj.AddComponent<TextMeshProUGUI>();
            valueText.text = "0.00";
            valueText.fontSize = 14;
            valueText.alignment = TextAlignmentOptions.Center;
            
            LayoutElement valueElement = valueObj.AddComponent<LayoutElement>();
            valueElement.preferredWidth = 50;
            valueElement.flexibleWidth = 0;
            
            // Create apply button
            GameObject buttonObj = new GameObject("ApplyButton");
            buttonObj.transform.SetParent(containerObj.transform, false);
            
            // Create a button that visually looks like a button
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = buttonColor;
            
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            
            // Set button colors
            ColorBlock colors = button.colors;
            colors.normalColor = buttonColor;
            colors.highlightedColor = new Color(buttonColor.r + 0.1f, buttonColor.g + 0.1f, buttonColor.b + 0.1f);
            colors.pressedColor = new Color(buttonColor.r - 0.1f, buttonColor.g - 0.1f, buttonColor.b - 0.1f);
            button.colors = colors;
            
            LayoutElement buttonElement = buttonObj.AddComponent<LayoutElement>();
            buttonElement.preferredWidth = 60;
            buttonElement.preferredHeight = 25;
            buttonElement.flexibleWidth = 0;
            
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Apply";
            buttonText.fontSize = 12;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.raycastTarget = false; // Important - prevents text from blocking clicks
            
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
            
            // Store references
            sliders[paramName] = slider;
            valueTexts[paramName] = valueText;
            buttons[paramName] = button;
            
            // Setup slider event
            slider.onValueChanged.AddListener((float value) => {
                UpdateParameterValueDisplay(paramName, value);
            });
            
            // Setup button click handler using a concrete method reference for reliability
            // This is the exact same pattern used by the Reset button that works
            switch (paramName)
            {
                case "ProductivityFactor":
                    button.onClick.AddListener(OnApplyProductivityFactor);
                    break;
                case "LaborElasticity":
                    button.onClick.AddListener(OnApplyLaborElasticity);
                    break;
                case "EfficiencyModifier":
                    button.onClick.AddListener(OnApplyEfficiencyModifier);
                    break;
                case "BaseConsumptionRate":
                    button.onClick.AddListener(OnApplyBaseConsumptionRate);
                    break;
                case "DecayRate":
                    button.onClick.AddListener(OnApplyDecayRate);
                    break;
            }
        }
        
        // Individual parameter application methods for direct button binding
        public void OnApplyProductivityFactor() { 
            ApplyParameter("ProductivityFactor");
            Debug.Log("ProductivityFactor Apply button clicked!");
        }
        
        public void OnApplyLaborElasticity() { 
            ApplyParameter("LaborElasticity"); 
            Debug.Log("LaborElasticity Apply button clicked!");
        }
        
        public void OnApplyEfficiencyModifier() { 
            ApplyParameter("EfficiencyModifier"); 
            Debug.Log("EfficiencyModifier Apply button clicked!");
        }
        
        public void OnApplyBaseConsumptionRate() { 
            ApplyParameter("BaseConsumptionRate"); 
            Debug.Log("BaseConsumptionRate Apply button clicked!");
        }
        
        public void OnApplyDecayRate() { 
            ApplyParameter("DecayRate"); 
            Debug.Log("DecayRate Apply button clicked!");
        }
        
        private void CreateSliderBackground(GameObject sliderObj)
        {
            GameObject background = new GameObject("Background");
            background.transform.SetParent(sliderObj.transform, false);
            
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f);
            
            RectTransform bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            
            Slider slider = sliderObj.GetComponent<Slider>();
            slider.targetGraphic = bgImage;
        }
        
        private void CreateSliderFillArea(GameObject sliderObj)
        {
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1, 0.75f);
            fillAreaRect.offsetMin = new Vector2(5, 0);
            fillAreaRect.offsetMax = new Vector2(-5, 0);
            
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.2f, 0.6f, 1f);
            
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            
            Slider slider = sliderObj.GetComponent<Slider>();
            slider.fillRect = fillRect;
        }
        
        private void CreateSliderHandle(GameObject sliderObj)
        {
            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderObj.transform, false);
            
            RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(10, 0);
            handleAreaRect.offsetMax = new Vector2(-10, 0);
            
            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            
            Image handleImage = handle.AddComponent<Image>();
            handleImage.color = Color.white;
            
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 0);
            
            Slider slider = sliderObj.GetComponent<Slider>();
            slider.handleRect = handleRect;
        }
        
        private void CreateResetButton(Transform parent)
        {
            GameObject buttonObj = new GameObject("ResetButton");
            buttonObj.transform.SetParent(parent, false);
            
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.8f, 0.2f, 0.2f);
            
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            
            LayoutElement buttonElement = buttonObj.AddComponent<LayoutElement>();
            buttonElement.preferredHeight = 30;
            buttonElement.flexibleWidth = 1;
            
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
            
            button.onClick.AddListener(ResetParameters);
        }
        
        private void UpdateDisplayValues()
        {
            if (economicSystem == null) return;
            
            UpdateSliderFromSystem("ProductivityFactor", economicSystem.productivityFactor);
            UpdateSliderFromSystem("LaborElasticity", economicSystem.laborElasticity);
            UpdateSliderFromSystem("EfficiencyModifier", economicSystem.efficiencyModifier);
            UpdateSliderFromSystem("BaseConsumptionRate", economicSystem.baseConsumptionRate);
            UpdateSliderFromSystem("DecayRate", economicSystem.decayRate);
        }
        
        private void UpdateSliderFromSystem(string paramName, float value)
        {
            if (!sliders.ContainsKey(paramName) || !parameterRanges.ContainsKey(paramName))
                return;
                
            Vector2 range = parameterRanges[paramName];
            float normalized = Mathf.InverseLerp(range.x, range.y, value);
            sliders[paramName].value = normalized;
            
            UpdateParameterValueDisplay(paramName, normalized);
        }
        
        private void UpdateParameterValueDisplay(string paramName, float normalized)
        {
            if (!valueTexts.ContainsKey(paramName) || !parameterRanges.ContainsKey(paramName))
                return;
                
            Vector2 range = parameterRanges[paramName];
            float actual = Mathf.Lerp(range.x, range.y, normalized);
            valueTexts[paramName].text = actual.ToString("F2");
        }
        
        private void ApplyParameter(string paramName)
        {
            if (economicSystem == null || !sliders.ContainsKey(paramName) || !parameterRanges.ContainsKey(paramName))
                return;
                
            // Get actual parameter value from slider
            Vector2 range = parameterRanges[paramName];
            float normalized = sliders[paramName].value;
            float value = Mathf.Lerp(range.x, range.y, normalized);
            
            // Apply to economic system based on parameter name
            switch (paramName)
            {
                case "ProductivityFactor":
                    economicSystem.productivityFactor = value;
                    break;
                case "LaborElasticity":
                    economicSystem.laborElasticity = value;
                    break;
                case "EfficiencyModifier":
                    economicSystem.efficiencyModifier = value;
                    break;
                case "BaseConsumptionRate":
                    economicSystem.baseConsumptionRate = value;
                    break;
                case "DecayRate":
                    economicSystem.decayRate = value;
                    break;
            }
            
            Debug.Log($"Applied {paramName} = {value}");
            
            // Provide visual feedback
            if (valueTexts.ContainsKey(paramName))
            {
                // Make text temporarily bold and green
                TextMeshProUGUI text = valueTexts[paramName];
                StartCoroutine(FlashValueFeedback(text));
            }
            
            if (buttons.ContainsKey(paramName))
            {
                // Flash button color
                Button button = buttons[paramName];
                if (button.targetGraphic as Image != null)
                {
                    StartCoroutine(FlashButtonFeedback(button));
                }
            }
        }
        
        private IEnumerator FlashValueFeedback(TextMeshProUGUI text)
        {
            Color originalColor = text.color;
            FontStyles originalStyle = text.fontStyle;
            
            text.color = Color.green;
            text.fontStyle = FontStyles.Bold;
            
            yield return new WaitForSeconds(0.5f);
            
            text.color = originalColor;
            text.fontStyle = originalStyle;
        }
        
        private IEnumerator FlashButtonFeedback(Button button)
        {
            Image buttonImage = button.targetGraphic as Image;
            if (buttonImage == null) yield break;
            
            Color originalColor = buttonImage.color;
            
            buttonImage.color = Color.yellow;
            
            yield return new WaitForSeconds(0.5f);
            
            buttonImage.color = originalColor;
        }
        
        private void ResetParameters()
        {
            if (economicSystem == null) return;
            
            economicSystem.productivityFactor = 1.0f;
            economicSystem.laborElasticity = 0.5f;
            economicSystem.efficiencyModifier = 0.1f;
            economicSystem.baseConsumptionRate = 0.2f;
            economicSystem.decayRate = 0.02f;
            
            // Update UI to match
            UpdateDisplayValues();
            
            Debug.Log("Reset all parameters to defaults");
        }
        
        private void Update()
        {
            // Ensure all buttons remain interactive
            foreach (var kvp in buttons)
            {
                if (kvp.Value != null && !kvp.Value.interactable)
                {
                    kvp.Value.interactable = true;
                }
            }
        }
    }
}