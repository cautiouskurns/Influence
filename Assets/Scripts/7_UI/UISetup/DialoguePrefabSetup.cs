using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Helper class to set up a dialogue window prefab with necessary components
    /// </summary>
    public class DialoguePrefabSetup : MonoBehaviour
    {
        [Header("Required Components")]
        public RectTransform dialoguePanel;
        public TextMeshProUGUI eventTitleText;
        public TextMeshProUGUI eventDescriptionText;
        public RectTransform responseContainer;
        public Button responseButtonTemplate;
        
        [Header("Panel Settings")]
        [SerializeField] private Vector2 dialoguePanelSize = new Vector2(800, 500); // Increased height
        [SerializeField] private Color panelColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        [SerializeField] private Color headerColor = new Color(0.2f, 0.3f, 0.4f, 1f);
        
        [Header("Text Settings")]
        [SerializeField] private int titleFontSize = 28;
        [SerializeField] private int descriptionFontSize = 22;
        [SerializeField] private int responseFontSize = 20;
        [SerializeField] private Color titleTextColor = Color.white;
        [SerializeField] private Color descriptionTextColor = new Color(0.9f, 0.9f, 0.9f);
        [SerializeField] private Color responseTextColor = Color.white;
        
        [Header("Response Button Settings")]
        [SerializeField] private Vector2 buttonSize = new Vector2(700, 60);
        [SerializeField] private float buttonSpacing = 10f;
        [SerializeField] private Color buttonColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color buttonHoverColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        
        [Header("Indicator Settings")]
        [SerializeField] private Color positiveEffectColor = new Color(0.2f, 0.7f, 0.2f);
        [SerializeField] private Color negativeEffectColor = new Color(0.7f, 0.2f, 0.2f);
        [SerializeField] private Color neutralEffectColor = new Color(0.7f, 0.7f, 0.7f);

        [ContextMenu("Setup Dialogue Prefab")]
        public void SetupPrefab()
        {
            // Make sure we have a DialogueView component
            DialogueView dialogueView = gameObject.GetComponent<DialogueView>();
            if (dialogueView == null)
            {
                dialogueView = gameObject.AddComponent<DialogueView>();
                Debug.Log("Added DialogueView component to prefab");
            }
            
            // Create main panel if needed
            if (dialoguePanel == null)
            {
                // Create panel game object
                GameObject panelObj = new GameObject("DialoguePanel", typeof(RectTransform));
                panelObj.transform.SetParent(transform);
                panelObj.transform.localPosition = Vector3.zero;
                panelObj.transform.localScale = Vector3.one;
                
                // Set up the RectTransform
                dialoguePanel = panelObj.GetComponent<RectTransform>();
                dialoguePanel.anchorMin = new Vector2(0.5f, 0.5f);
                dialoguePanel.anchorMax = new Vector2(0.5f, 0.5f);
                dialoguePanel.pivot = new Vector2(0.5f, 0.5f);
                dialoguePanel.sizeDelta = dialoguePanelSize;
                
                // Add Image component for background
                Image panelImage = panelObj.AddComponent<Image>();
                panelImage.color = panelColor;
                
                Debug.Log("Created main dialogue panel");
            }
            
            // Create header area
            GameObject headerObj = SetupChild(dialoguePanel, "HeaderPanel", 
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(dialoguePanelSize.x, 60f), new Vector2(0f, -30f));
            
            Image headerImage = headerObj.AddComponent<Image>();
            headerImage.color = headerColor;
            
            // Create event title text if needed
            if (eventTitleText == null)
            {
                GameObject titleObj = SetupChild(headerObj.GetComponent<RectTransform>(), "TitleText", 
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(dialoguePanelSize.x - 40f, 50f), Vector2.zero);
                
                eventTitleText = titleObj.AddComponent<TextMeshProUGUI>();
                eventTitleText.text = "Event Title";
                eventTitleText.fontSize = titleFontSize;
                eventTitleText.alignment = TextAlignmentOptions.Center;
                eventTitleText.color = titleTextColor;
                eventTitleText.fontStyle = FontStyles.Bold;
                
                Debug.Log("Created event title text");
            }
            
            // Create content area with more space for description
            GameObject contentObj = SetupChild(dialoguePanel, "ContentPanel", 
                new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.9f), new Vector2(0.5f, 0.5f),
                new Vector2(dialoguePanelSize.x - 40f, dialoguePanelSize.y * 0.35f), new Vector2(0f, 0f));
            
            // Create event description text if needed
            if (eventDescriptionText == null)
            {
                GameObject descObj = SetupChild(contentObj.GetComponent<RectTransform>(), "DescriptionText", 
                    new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                    Vector2.zero, Vector2.zero);
                
                eventDescriptionText = descObj.AddComponent<TextMeshProUGUI>();
                eventDescriptionText.text = "This is where the event description will appear. It can be multiple lines of text explaining the situation that the player needs to respond to.";
                eventDescriptionText.fontSize = descriptionFontSize;
                eventDescriptionText.alignment = TextAlignmentOptions.Top;
                eventDescriptionText.color = descriptionTextColor;
                eventDescriptionText.textWrappingMode = TextWrappingModes.Normal;
                eventDescriptionText.overflowMode = TextOverflowModes.Overflow;
                
                Debug.Log("Created event description text");
            }
            
            // Create response container in the bottom section, not overlapping the description
            if (responseContainer == null)
            {
                GameObject responseObj = SetupChild(dialoguePanel, "ResponseContainer", 
                    new Vector2(0.5f, 0.1f), new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.5f),
                    new Vector2(dialoguePanelSize.x - 40f, dialoguePanelSize.y * 0.3f), Vector2.zero);
                
                responseContainer = responseObj.GetComponent<RectTransform>();
                
                // Add scroll rect for many responses
                ScrollRect scrollRect = responseObj.AddComponent<ScrollRect>();
                
                // Create viewport
                GameObject viewportObj = SetupChild(responseContainer, "Viewport", 
                    Vector2.zero, Vector2.one, Vector2.one * 0.5f,
                    Vector2.zero, Vector2.zero);
                
                Image viewportImage = viewportObj.AddComponent<Image>();
                viewportImage.color = new Color(0, 0, 0, 0.01f); // Almost transparent
                
                // Create content container for the scroll rect
                GameObject contentContainerObj = SetupChild(viewportObj.GetComponent<RectTransform>(), "Content", 
                    new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                    new Vector2(0, 0), Vector2.zero);
                
                // Setup scroll rect
                scrollRect.viewport = viewportObj.GetComponent<RectTransform>();
                scrollRect.content = contentContainerObj.GetComponent<RectTransform>();
                scrollRect.horizontal = false;
                scrollRect.vertical = true;
                scrollRect.scrollSensitivity = 10;
                
                // Add vertical layout group to content container
                VerticalLayoutGroup layoutGroup = contentContainerObj.AddComponent<VerticalLayoutGroup>();
                layoutGroup.spacing = buttonSpacing;
                layoutGroup.padding = new RectOffset(20, 20, 0, 20);
                layoutGroup.childAlignment = TextAnchor.UpperCenter;
                layoutGroup.childControlHeight = false;
                layoutGroup.childControlWidth = false;
                layoutGroup.childForceExpandHeight = false;
                layoutGroup.childForceExpandWidth = false;
                
                // Add content size fitter to content container
                ContentSizeFitter fitter = contentContainerObj.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                
                // Update response container reference to the content container
                responseContainer = contentContainerObj.GetComponent<RectTransform>();
                
                Debug.Log("Created scrollable response container with layout");
            }
            
            // Create button template if needed
            if (responseButtonTemplate == null)
            {
                GameObject buttonObj = SetupChild(responseContainer, "ResponseButtonTemplate", 
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    buttonSize, new Vector2(0f, -buttonSize.y/2));
                
                // Add button component
                responseButtonTemplate = buttonObj.AddComponent<Button>();
                
                // Add image for the button
                Image buttonImage = buttonObj.AddComponent<Image>();
                buttonImage.color = buttonColor;
                
                // Set up the Selectable colors
                ColorBlock colors = responseButtonTemplate.colors;
                colors.normalColor = buttonColor;
                colors.highlightedColor = buttonHoverColor;
                colors.pressedColor = new Color(buttonColor.r * 0.8f, buttonColor.g * 0.8f, buttonColor.b * 0.8f);
                responseButtonTemplate.colors = colors;
                
                // Create text inside button
                GameObject textObj = SetupChild(buttonObj.GetComponent<RectTransform>(), "ButtonText", 
                    new Vector2(0f, 0f), new Vector2(0.82f, 1f), new Vector2(0.5f, 0.5f),
                    Vector2.zero, Vector2.zero);
                
                TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
                buttonText.text = "Response Option";
                buttonText.fontSize = responseFontSize;
                buttonText.alignment = TextAlignmentOptions.Left;
                buttonText.color = responseTextColor;
                buttonText.margin = new Vector4(15, 5, 10, 5);
                buttonText.textWrappingMode = TextWrappingModes.Normal;
                
                // Create indicator area - MAKE THIS ALWAYS VISIBLE
                GameObject indicatorsObj = SetupChild(buttonObj.GetComponent<RectTransform>(), "EffectIndicators", 
                    new Vector2(0.85f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f),
                    Vector2.zero, Vector2.zero);
                
                HorizontalLayoutGroup indicatorsLayout = indicatorsObj.AddComponent<HorizontalLayoutGroup>();
                indicatorsLayout.spacing = 5;
                indicatorsLayout.padding = new RectOffset(0, 10, 5, 5);
                indicatorsLayout.childAlignment = TextAnchor.MiddleRight;
                indicatorsLayout.childControlHeight = false;
                indicatorsLayout.childControlWidth = false;
                indicatorsLayout.childForceExpandHeight = false;
                indicatorsLayout.childForceExpandWidth = false;
                
                // Add tooltip component to the button to show detailed effect description
                TooltipHandler tooltipHandler = buttonObj.AddComponent<TooltipHandler>();
                tooltipHandler.tooltipText = "This choice will affect:\nWealth: +100\nProduction: -50\nLabor: +25";
                tooltipHandler.tooltipDelay = 0.5f;
                
                // Create sample indicators with better styling
                CreateEffectIndicator(indicatorsObj.transform, "W", 100, positiveEffectColor);
                CreateEffectIndicator(indicatorsObj.transform, "P", -50, negativeEffectColor);
                CreateEffectIndicator(indicatorsObj.transform, "L", 25, positiveEffectColor);
                
                // KEEP INDICATORS VISIBLE in the template so they show up correctly
                // indicatorsObj.SetActive(true);
                
                Debug.Log("Created response button template with effect indicators");
                
                // Initially hide the template - will be used to instantiate real buttons
                buttonObj.SetActive(false);
            }
                    
            // Reference all components in DialogueView
            dialogueView.dialoguePanel = dialoguePanel;
            dialogueView.eventTitleText = eventTitleText;
            dialogueView.eventDescriptionText = eventDescriptionText;
            dialogueView.responseContainer = responseContainer;
            dialogueView.responseButtonTemplate = responseButtonTemplate;
            
            // Set the indicator colors in DialogueView
            dialogueView.positiveEffectColor = positiveEffectColor;
            dialogueView.negativeEffectColor = negativeEffectColor;
            dialogueView.neutralEffectColor = neutralEffectColor;
            
            Debug.Log("Dialogue prefab setup complete!");
        }

        private void CreateEffectIndicator(Transform parent, string letter, int value, Color color)
        {
            // Create indicator object
            GameObject indicator = new GameObject($"{letter}Indicator", typeof(RectTransform));
            indicator.transform.SetParent(parent, false);
            
            // Set up the rect transform with proper size
            RectTransform rect = indicator.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(40, 25); // Slightly wider for better readability
            
            // Add background image with rounded corners look
            Image bg = indicator.AddComponent<Image>();
            // Load a rounded rectangle sprite or use a default one
            // If using default, we'll make it visually distinct with color
            bg.color = new Color(color.r, color.g, color.b, 0.9f); // More opacity
            
            // Add a shadow effect for better visibility
            Shadow shadow = indicator.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(2, -2);
            
            // Add text component
            GameObject textObj = new GameObject("IndicatorText", typeof(RectTransform));
            textObj.transform.SetParent(indicator.transform, false);
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            
            // Format text with appropriate style and sign
            string valueText = value > 0 ? $"+{value}" : value.ToString();
            tmp.text = $"{letter}:{valueText}";
            tmp.fontSize = 14; // Slightly larger for readability
            tmp.fontStyle = FontStyles.Bold; // Make it bold
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
            // Add tooltip component to each indicator
            TooltipHandler tooltipHandler = indicator.AddComponent<TooltipHandler>();
            string effectType = letter == "W" ? "Wealth" : (letter == "P" ? "Production" : "Labor");
            tooltipHandler.tooltipText = $"{effectType}: {valueText}";
            tooltipHandler.tooltipDelay = 0.3f;
        }
                
        private GameObject SetupChild(RectTransform parent, string name, 
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, 
            Vector2 sizeDelta, Vector2 anchoredPosition)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent);
            
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = anchoredPosition;
            rect.localScale = Vector3.one;
            
            return obj;
        }
        
        private void CreateSampleIndicator(Transform parent, string letter, int value, Color color)
        {
            GameObject indicator = new GameObject($"{letter}Indicator", typeof(RectTransform));
            indicator.transform.SetParent(parent, false);
            
            RectTransform rect = indicator.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(35, 25);
            
            // Add background image
            Image bg = indicator.AddComponent<Image>();
            bg.color = new Color(color.r, color.g, color.b, 0.8f);
            
            // Add text component
            GameObject textObj = new GameObject("IndicatorText", typeof(RectTransform));
            textObj.transform.SetParent(indicator.transform, false);
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = $"{letter}:{(value > 0 ? "+" : "")}{value}";
            tmp.fontSize = 12;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }
    }
}