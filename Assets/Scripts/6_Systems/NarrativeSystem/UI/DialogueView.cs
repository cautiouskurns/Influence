using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace NarrativeSystem
{
    /// <summary>
    /// DialogueView manages the dialogue UI window and handles player interaction with dialogue events
    /// </summary>
    public class DialogueView : MonoBehaviour
    {
        [Header("UI References - Set via DialoguePrefabSetup")]
        [Tooltip("This should be automatically set by DialoguePrefabSetup")]
        public RectTransform dialoguePanel;
        
        [Tooltip("This should be automatically set by DialoguePrefabSetup")]
        public TextMeshProUGUI eventTitleText;
        
        [Tooltip("This should be automatically set by DialoguePrefabSetup")]
        public TextMeshProUGUI eventDescriptionText;
        
        [Tooltip("This should be automatically set by DialoguePrefabSetup")]
        public RectTransform responseContainer;
        
        [Tooltip("This should be automatically set by DialoguePrefabSetup")]
        public Button responseButtonTemplate;
        
        [Header("Outcome Indicators")]
        [Tooltip("Optional prefab for wealth effect indicator")]
        public GameObject wealthEffectIndicatorPrefab;
        
        [Tooltip("Optional prefab for production effect indicator")]
        public GameObject productionEffectIndicatorPrefab;
        
        [Tooltip("Optional prefab for labor effect indicator")]
        public GameObject laborEffectIndicatorPrefab;
        
        [Header("Indicator Colors")]
        public Color positiveEffectColor = new Color(0.3f, 0.8f, 0.3f);
        public Color negativeEffectColor = new Color(0.8f, 0.3f, 0.3f);
        public Color neutralEffectColor = new Color(0.7f, 0.7f, 0.7f);
        
        // Dialogue data
        private string currentEventId;
        private List<GameObject> activeResponseButtons = new List<GameObject>();
        private bool componentsChecked = false;
        
        // Events
        public event Action<string, int> OnResponseSelected;
        
        /// <summary>
        /// Data class to hold effects for a response option
        /// </summary>
        public class ResponseEffects
        {
            public int wealthEffect;
            public int productionEffect;
            public int laborEffect;
        }
        
        private void Awake()
        {
            // We'll check components during Start instead to ensure everything is initialized
        }

        private void Start()
        {
            // Check components are set up properly
            if (!componentsChecked)
            {
                CheckComponents();
                componentsChecked = true;
            }
            
            // Initially hide the dialogue panel
            if (dialoguePanel != null)
            {
                dialoguePanel.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Force a components check - call this if you've set up the dialogue view components at runtime
        /// </summary>
        public void ForceComponentCheck()
        {
            CheckComponents();
            componentsChecked = true;

            Debug.Log("DialogueView: Components check forced");
        }
        
        /// <summary>
        /// Validates the required components and logs errors for any missing ones
        /// </summary>
        private void CheckComponents()
        {
            if (dialoguePanel == null)
            {
                Debug.LogWarning("DialogueView: dialoguePanel is null! Attempting to find it automatically...");
                
                // Try to find the panel as a child
                Transform dialoguePanelTransform = transform.Find("DialoguePanel");
                if (dialoguePanelTransform != null)
                {
                    dialoguePanel = dialoguePanelTransform.GetComponent<RectTransform>();
                    Debug.Log($"DialogueView: Found dialoguePanel as child: {dialoguePanelTransform.name}");
                    
                    // Try to find the other objects as children of the panel
                    TryFindComponents();
                }
                else
                {
                    // Look for any child with RectTransform that might be the dialogue panel
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        Transform child = transform.GetChild(i);
                        RectTransform rect = child.GetComponent<RectTransform>();
                        
                        if (rect != null && (child.name.Contains("Panel") || child.name.Contains("Dialogue") || child.name.Contains("Window")))
                        {
                            Debug.Log($"DialogueView: Found potential dialogue panel: {child.name}");
                            dialoguePanel = rect;
                            TryFindComponents();
                            
                            // If we found all components after trying this panel, break out
                            if (eventTitleText != null && eventDescriptionText != null && 
                                responseContainer != null && responseButtonTemplate != null)
                            {
                                Debug.Log("DialogueView: Successfully found all components");
                                break;
                            }
                        }
                    }
                    
                    // If still not found, check if we ARE the dialogue panel
                    if (dialoguePanel == null)
                    {
                        dialoguePanel = GetComponent<RectTransform>();
                        if (dialoguePanel != null)
                        {
                            Debug.Log("DialogueView: Using this GameObject as the dialoguePanel.");
                            TryFindComponents();
                        }
                        else
                        {
                            // Last resort: just create a new dialogue panel
                            Debug.LogWarning("DialogueView: Creating a new dialogue panel as last resort");
                            GameObject panelObj = new GameObject("DialoguePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                            panelObj.transform.SetParent(transform, false);
                            
                            dialoguePanel = panelObj.GetComponent<RectTransform>();
                            dialoguePanel.anchorMin = new Vector2(0.5f, 0.5f);
                            dialoguePanel.anchorMax = new Vector2(0.5f, 0.5f);
                            dialoguePanel.pivot = new Vector2(0.5f, 0.5f);
                            dialoguePanel.sizeDelta = new Vector2(600, 400);
                            
                            // Add a background image
                            Image panelImage = panelObj.GetComponent<Image>();
                            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
                            
                            // Create basic components
                            CreateBasicDialogueComponents();
                        }
                    }
                }
            }
            else
            {
                // If we have a panel but other components are missing, try to find them
                if (eventTitleText == null || eventDescriptionText == null || 
                    responseContainer == null || responseButtonTemplate == null)
                {
                    TryFindComponents();
                }
            }
                
            if (eventTitleText == null)
                Debug.LogWarning("DialogueView: eventTitleText is null! Please set it up using DialoguePrefabSetup.");
                
            if (eventDescriptionText == null)
                Debug.LogWarning("DialogueView: eventDescriptionText is null! Please set it up using DialoguePrefabSetup.");
                
            if (responseContainer == null)
                Debug.LogWarning("DialogueView: responseContainer is null! Please set it up using DialoguePrefabSetup.");
                
            if (responseButtonTemplate == null)
                Debug.LogWarning("DialogueView: responseButtonTemplate is null! Please set it up using DialoguePrefabSetup.");
                
            // Log the final status
            Debug.Log($"DialogueView setup status: Panel={dialoguePanel != null}, Title={eventTitleText != null}, " +
                      $"Description={eventDescriptionText != null}, Container={responseContainer != null}, " +
                      $"ButtonTemplate={responseButtonTemplate != null}");
        }
        
        /// <summary>
        /// Creates basic dialogue UI components when none are found
        /// </summary>
        private void CreateBasicDialogueComponents()
        {
            if (dialoguePanel == null) return;
            
            Debug.Log("DialogueView: Creating basic dialogue UI components");
            
            // Create header panel with title
            GameObject headerObj = new GameObject("HeaderPanel", typeof(RectTransform));
            headerObj.transform.SetParent(dialoguePanel, false);
            RectTransform headerRect = headerObj.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.pivot = new Vector2(0.5f, 1);
            headerRect.sizeDelta = new Vector2(0, 50);
            headerRect.anchoredPosition = Vector2.zero;
            
            // Add title text
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(headerObj.transform, false);
            eventTitleText = titleObj.GetComponent<TextMeshProUGUI>();
            eventTitleText.text = "Dialogue Title";
            eventTitleText.fontSize = 24;
            eventTitleText.alignment = TextAlignmentOptions.Center;
            eventTitleText.color = Color.white;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = new Vector2(10, 0);
            titleRect.offsetMax = new Vector2(-10, 0);
            
            // Create content panel with description
            GameObject contentObj = new GameObject("ContentPanel", typeof(RectTransform));
            contentObj.transform.SetParent(dialoguePanel, false);
            RectTransform contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0.3f);
            contentRect.anchorMax = new Vector2(1, 0.9f);
            contentRect.offsetMin = new Vector2(10, 0);
            contentRect.offsetMax = new Vector2(-10, -10);
            
            // Add description text
            GameObject descObj = new GameObject("DescriptionText", typeof(RectTransform), typeof(TextMeshProUGUI));
            descObj.transform.SetParent(contentObj.transform, false);
            eventDescriptionText = descObj.GetComponent<TextMeshProUGUI>();
            eventDescriptionText.text = "Dialogue description text goes here.";
            eventDescriptionText.fontSize = 18;
            eventDescriptionText.alignment = TextAlignmentOptions.TopLeft;
            eventDescriptionText.color = Color.white;
            eventDescriptionText.textWrappingMode = TextWrappingModes.Normal;
            
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = Vector2.zero;
            descRect.anchorMax = Vector2.one;
            descRect.offsetMin = new Vector2(10, 10);
            descRect.offsetMax = new Vector2(-10, -10);
            
            // Create response container
            GameObject responseObj = new GameObject("ResponseContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
            responseObj.transform.SetParent(dialoguePanel, false);
            responseContainer = responseObj.GetComponent<RectTransform>();
            responseContainer.anchorMin = new Vector2(0, 0);
            responseContainer.anchorMax = new Vector2(1, 0.25f);
            responseContainer.offsetMin = new Vector2(20, 20);
            responseContainer.offsetMax = new Vector2(-20, 0);
            
            // Configure layout group
            VerticalLayoutGroup layout = responseObj.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            
            // Create button template
            GameObject buttonObj = new GameObject("ResponseButtonTemplate", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObj.transform.SetParent(responseContainer, false);
            buttonObj.SetActive(false); // Template should be inactive
            
            // Configure button
            responseButtonTemplate = buttonObj.GetComponent<Button>();
            Image buttonImage = buttonObj.GetComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.3f, 1f);
            
            // Add layout element for sizing
            LayoutElement buttonLayout = buttonObj.AddComponent<LayoutElement>();
            buttonLayout.minHeight = 40;
            buttonLayout.preferredHeight = 40;
            
            // Add button text
            GameObject buttonTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            TextMeshProUGUI buttonText = buttonTextObj.GetComponent<TextMeshProUGUI>();
            buttonText.text = "Response option";
            buttonText.fontSize = 16;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = new Vector2(10, 5);
            buttonTextRect.offsetMax = new Vector2(-10, -5);
            
            Debug.Log("DialogueView: Basic components created successfully");
        }
        
        /// <summary>
        /// Attempts to find UI components by traversing the hierarchy
        /// </summary>
        private void TryFindComponents()
        {
            if (dialoguePanel != null)
            {
                // Try to find title text in HeaderPanel
                Transform headerPanel = dialoguePanel.transform.Find("HeaderPanel");
                if (headerPanel != null)
                {
                    Transform titleText = headerPanel.Find("TitleText");
                    if (titleText != null && eventTitleText == null)
                    {
                        eventTitleText = titleText.GetComponent<TextMeshProUGUI>();
                    }
                }
                
                // Try to find description text in ContentPanel
                Transform contentPanel = dialoguePanel.transform.Find("ContentPanel");
                if (contentPanel != null)
                {
                    Transform descText = contentPanel.Find("DescriptionText");
                    if (descText != null && eventDescriptionText == null)
                    {
                        eventDescriptionText = descText.GetComponent<TextMeshProUGUI>();
                    }
                }
                
                // Try to find response container
                Transform responseContainerTrans = dialoguePanel.transform.Find("ResponseContainer");
                if (responseContainerTrans != null && responseContainer == null)
                {
                    // Check if it's a scroll rect setup
                    ScrollRect scrollRect = responseContainerTrans.GetComponent<ScrollRect>();
                    if (scrollRect != null && scrollRect.content != null)
                    {
                        responseContainer = scrollRect.content;
                    }
                    else
                    {
                        responseContainer = responseContainerTrans.GetComponent<RectTransform>();
                    }
                    
                    // Try to find response button template in the direct container
                    Transform buttonTemplate = responseContainer.Find("ResponseButtonTemplate");
                    if (buttonTemplate != null && responseButtonTemplate == null)
                    {
                        responseButtonTemplate = buttonTemplate.GetComponent<Button>();
                    }
                    else if (responseContainerTrans != null)
                    {
                        // Try to find in the viewport/parent structure
                        buttonTemplate = responseContainerTrans.Find("ResponseButtonTemplate");
                        if (buttonTemplate != null && responseButtonTemplate == null)
                        {
                            responseButtonTemplate = buttonTemplate.GetComponent<Button>();
                        }
                        else
                        {
                            Transform viewportTrans = responseContainerTrans.Find("Viewport");
                            if (viewportTrans != null)
                            {
                                buttonTemplate = viewportTrans.Find("ResponseButtonTemplate");
                                if (buttonTemplate != null && responseButtonTemplate == null)
                                {
                                    responseButtonTemplate = buttonTemplate.GetComponent<Button>();
                                }
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Ensures required components are available for showing dialogues
        /// </summary>
        private bool EnsureComponents()
        {
            if (!componentsChecked)
            {
                CheckComponents();
                componentsChecked = true;
            }
            
            if (dialoguePanel == null || eventTitleText == null || eventDescriptionText == null || 
                responseContainer == null || responseButtonTemplate == null)
            {
                // Try one more time
                TryFindComponents();
                
                // Check if we now have all components
                if (dialoguePanel == null || eventTitleText == null || eventDescriptionText == null || 
                    responseContainer == null || responseButtonTemplate == null)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Show a dialogue event with title, description and response options
        /// </summary>
        public void ShowDialogue(string eventId, string title, string description, List<string> responses)
        {
            // Check if components are properly set up
            if (!EnsureComponents())
            {
                Debug.LogError($"DialogueView: Cannot show dialogue - UI components are missing. Event: {title}");
                return;
            }
            
            currentEventId = eventId;
            
            // Set up the texts
            eventTitleText.text = title;
            eventDescriptionText.text = description;
            
            // Clear any existing response buttons
            ClearResponseButtons();
            
            // Create new response buttons
            if (responses != null)
            {
                for (int i = 0; i < responses.Count; i++)
                {
                    int responseIndex = i; // Capture the index for the lambda
                    GameObject responseButton = CreateResponseButton(responses[i], () => {
                        HandleResponseSelected(responseIndex);
                    });
                    
                    if (responseButton != null)
                        activeResponseButtons.Add(responseButton);
                }
            }
            
            // Show the dialogue panel
            dialoguePanel.gameObject.SetActive(true);
            Debug.Log($"DialogueView: Showing dialogue '{title}'");
        }
        
        /// <summary>
        /// Show a dialogue event with title, description and response options with effects
        /// </summary>
        public void ShowDialogueWithEffects(string eventId, string title, string description, 
            List<string> responses, List<ResponseEffects> effects)
        {
            // Check if components are properly set up
            if (!EnsureComponents())
            {
                Debug.LogError($"DialogueView: Cannot show dialogue - UI components are missing. Event: {title}");
                return;
            }
            
            currentEventId = eventId;
            
            // Set up the texts
            eventTitleText.text = title;
            eventDescriptionText.text = description;
            
            // Clear any existing response buttons
            ClearResponseButtons();
            
            // Create new response buttons
            if (responses != null)
            {
                for (int i = 0; i < responses.Count; i++)
                {
                    int responseIndex = i; // Capture the index for the lambda
                    
                    // Get effects for this response if available
                    ResponseEffects responseEffects = null;
                    if (effects != null && i < effects.Count)
                    {
                        responseEffects = effects[i];
                    }
                    
                    GameObject responseButton = CreateResponseButtonWithEffects(
                        responses[i], 
                        responseEffects,
                        () => { HandleResponseSelected(responseIndex); }
                    );
                    
                    if (responseButton != null)
                        activeResponseButtons.Add(responseButton);
                }
            }
            
            // Show the dialogue panel
            dialoguePanel.gameObject.SetActive(true);
            Debug.Log($"DialogueView: Showing dialogue '{title}' with effects");
        }
        
        /// <summary>
        /// Hides the dialogue panel
        /// </summary>
        public void HideDialogue()
        {
            if (dialoguePanel != null && dialoguePanel.gameObject != null)
                dialoguePanel.gameObject.SetActive(false);
                
            currentEventId = null;
            ClearResponseButtons();
        }
        
        /// <summary>
        /// Creates a response button with the given text and callback
        /// </summary>
        private GameObject CreateResponseButton(string responseText, Action onClick)
        {
            if (responseButtonTemplate == null || responseContainer == null)
            {
                Debug.LogError("DialogueView: Cannot create response button - template or container is null");
                return null;
            }
            
            // Instantiate a new button from the template
            GameObject buttonObj = Instantiate(responseButtonTemplate.gameObject, responseContainer);
            if (buttonObj == null)
            {
                Debug.LogError("DialogueView: Failed to instantiate button from template");
                return null;
            }
            
            buttonObj.SetActive(true);
            
            // Set the button text
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = responseText;
            }
            else
            {
                Debug.LogWarning("DialogueView: Button text component not found in response button");
            }
            
            // Add click handler
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => {
                    onClick?.Invoke();
                });
            }
            else
            {
                Debug.LogError("DialogueView: Button component not found in instantiated button");
            }
            
            return buttonObj;
        }
        
        /// <summary>
        /// Creates a response button with the given text, effects, and callback
        /// </summary>
        private GameObject CreateResponseButtonWithEffects(string responseText, ResponseEffects effects, Action onClick)
        {
            if (responseButtonTemplate == null || responseContainer == null)
            {
                Debug.LogError("DialogueView: Cannot create response button - template or container is null");
                return null;
            }
            
            // Instantiate a new button from the template
            GameObject buttonObj = Instantiate(responseButtonTemplate.gameObject, responseContainer);
            if (buttonObj == null)
            {
                Debug.LogError("DialogueView: Failed to instantiate button from template");
                return null;
            }
            
            buttonObj.SetActive(true);
            
            // Set the button text
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                // Format the response text with outcome indicators if effects are provided
                if (effects != null)
                {
                    // Keep the original response text, we'll add indicators separately
                    buttonText.text = responseText;
                    
                    // Create a horizontal layout for the indicators
                    GameObject indicatorsObj = new GameObject("EffectIndicators", typeof(RectTransform));
                    indicatorsObj.transform.SetParent(buttonObj.transform, false);
                    
                    RectTransform indicatorsRect = indicatorsObj.GetComponent<RectTransform>();
                    indicatorsRect.anchorMin = new Vector2(1, 0.5f);
                    indicatorsRect.anchorMax = new Vector2(1, 0.5f);
                    indicatorsRect.pivot = new Vector2(1, 0.5f);
                    indicatorsRect.anchoredPosition = new Vector2(-10, 0);
                    indicatorsRect.sizeDelta = new Vector2(120, 30);
                    
                    HorizontalLayoutGroup layout = indicatorsObj.AddComponent<HorizontalLayoutGroup>();
                    layout.spacing = 5;
                    layout.childAlignment = TextAnchor.MiddleRight;
                    layout.childForceExpandWidth = false;
                    layout.childForceExpandHeight = false;
                    
                    // Add the wealth indicator
                    if (effects.wealthEffect != 0)
                    {
                        CreateEffectIndicator(indicatorsObj.transform, "W", effects.wealthEffect, 
                            effects.wealthEffect > 0 ? positiveEffectColor : negativeEffectColor);
                    }
                    
                    // Add the production indicator
                    if (effects.productionEffect != 0)
                    {
                        CreateEffectIndicator(indicatorsObj.transform, "P", effects.productionEffect,
                            effects.productionEffect > 0 ? positiveEffectColor : negativeEffectColor);
                    }
                    
                    // Add the labor indicator
                    if (effects.laborEffect != 0)
                    {
                        CreateEffectIndicator(indicatorsObj.transform, "L", effects.laborEffect,
                            effects.laborEffect > 0 ? positiveEffectColor : negativeEffectColor);
                    }
                    
                    // Adjust the text rect to make room for indicators
                    RectTransform textRect = buttonText.GetComponent<RectTransform>();
                    if (textRect != null)
                    {
                        textRect.offsetMax = new Vector2(-130, textRect.offsetMax.y);
                    }
                }
                else
                {
                    buttonText.text = responseText;
                }
            }
            else
            {
                Debug.LogWarning("DialogueView: Button text component not found in response button");
            }
            
            // Add click handler
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => {
                    onClick?.Invoke();
                });
            }
            else
            {
                Debug.LogError("DialogueView: Button component not found in instantiated button");
            }
            
            return buttonObj;
        }
        
        /// <summary>
        /// Creates an effect indicator with the given letter, value, and color
        /// </summary>
        private void CreateEffectIndicator(Transform parent, string letter, int value, Color color)
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
        
        /// <summary>
        /// Clears all active response buttons
        /// </summary>
        private void ClearResponseButtons()
        {
            foreach (GameObject button in activeResponseButtons)
            {
                if (button != null)
                    Destroy(button);
            }
            
            activeResponseButtons.Clear();
        }
        
        /// <summary>
        /// Handles when a response is selected
        /// </summary>
        private void HandleResponseSelected(int responseIndex)
        {
            // Invoke the response event
            OnResponseSelected?.Invoke(currentEventId, responseIndex);
            
            // Hide the dialogue
            HideDialogue();
        }

        /// <summary>
        /// Static helper method to create a fully set up DialogueView in the scene
        /// </summary>
        public static DialogueView CreateDialogueSystem()
        {
            Debug.Log("Creating a new DialogueView system from scratch");
            
            // First, find or create a canvas
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("DialogueCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                Debug.Log("Created new canvas for dialogue system");
            }
            
            // Create the dialogue view object
            GameObject dialogueObj = new GameObject("DialogueView");
            dialogueObj.transform.SetParent(canvas.transform, false);
            
            // Add the DialogueView component
            DialogueView dialogueView = dialogueObj.AddComponent<DialogueView>();
            
            // Create the actual dialogue panel
            GameObject panelObj = new GameObject("DialoguePanel");
            panelObj.transform.SetParent(dialogueObj.transform, false);
            
            // Add required components
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            Image panelImage = panelObj.AddComponent<Image>();
            
            // Configure the panel
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(800, 500);
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            
            // Set the dialoguePanel reference
            dialogueView.dialoguePanel = panelRect;
            
            // Create title area
            GameObject titleObj = new GameObject("HeaderPanel");
            titleObj.transform.SetParent(panelObj.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.sizeDelta = new Vector2(0, 60);
            titleRect.anchoredPosition = Vector2.zero;
            
            // Create title text
            GameObject titleTextObj = new GameObject("TitleText");
            titleTextObj.transform.SetParent(titleObj.transform, false);
            RectTransform titleTextRect = titleTextObj.AddComponent<RectTransform>();
            TextMeshProUGUI titleText = titleTextObj.AddComponent<TextMeshProUGUI>();
            titleTextRect.anchorMin = Vector2.zero;
            titleTextRect.anchorMax = Vector2.one;
            titleTextRect.offsetMin = new Vector2(20, 10);
            titleTextRect.offsetMax = new Vector2(-20, -10);
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontSize = 24;
            titleText.color = Color.white;
            titleText.text = "Event Title";
            
            // Set the title text reference
            dialogueView.eventTitleText = titleText;
            
            // Create description area
            GameObject descObj = new GameObject("ContentPanel");
            descObj.transform.SetParent(panelObj.transform, false);
            RectTransform descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0.3f);
            descRect.anchorMax = new Vector2(1, 0.9f);
            descRect.offsetMin = new Vector2(20, 10);
            descRect.offsetMax = new Vector2(-20, -10);
            
            // Create description text
            GameObject descTextObj = new GameObject("DescriptionText");
            descTextObj.transform.SetParent(descObj.transform, false);
            RectTransform descTextRect = descTextObj.AddComponent<RectTransform>();
            TextMeshProUGUI descText = descTextObj.AddComponent<TextMeshProUGUI>();
            descTextRect.anchorMin = Vector2.zero;
            descTextRect.anchorMax = Vector2.one;
            descTextRect.offsetMin = Vector2.zero;
            descTextRect.offsetMax = Vector2.zero;
            descText.alignment = TextAlignmentOptions.TopLeft;
            descText.fontSize = 18;
            descText.color = Color.white;
            descText.text = "Event description goes here...";
            
            // Set description text reference
            dialogueView.eventDescriptionText = descText;
            
            // Create response container
            GameObject responseObj = new GameObject("ResponseContainer");
            responseObj.transform.SetParent(panelObj.transform, false);
            RectTransform responseRect = responseObj.AddComponent<RectTransform>();
            VerticalLayoutGroup responseLayout = responseObj.AddComponent<VerticalLayoutGroup>();
            ContentSizeFitter sizeFitter = responseObj.AddComponent<ContentSizeFitter>();
            responseRect.anchorMin = new Vector2(0, 0);
            responseRect.anchorMax = new Vector2(1, 0.3f);
            responseRect.offsetMin = new Vector2(40, 20);
            responseRect.offsetMax = new Vector2(-40, -10);
            responseLayout.spacing = 10;
            responseLayout.padding = new RectOffset(5, 5, 5, 5);
            responseLayout.childAlignment = TextAnchor.UpperCenter;
            responseLayout.childControlHeight = true;
            responseLayout.childControlWidth = true;
            responseLayout.childForceExpandWidth = true;
            responseLayout.childForceExpandHeight = false;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Set response container reference
            dialogueView.responseContainer = responseRect;
            
            // Create response button template
            GameObject buttonObj = new GameObject("ResponseButtonTemplate");
            buttonObj.transform.SetParent(responseObj.transform, false);
            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            Image buttonImage = buttonObj.AddComponent<Image>();
            Button button = buttonObj.AddComponent<Button>();
            buttonRect.anchorMin = new Vector2(0, 0);
            buttonRect.anchorMax = new Vector2(1, 0);
            buttonRect.sizeDelta = new Vector2(0, 50);
            buttonRect.pivot = new Vector2(0.5f, 0);
            buttonImage.color = new Color(0.2f, 0.3f, 0.4f, 1);
            
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.3f, 0.4f, 1);
            colors.highlightedColor = new Color(0.3f, 0.4f, 0.5f, 1);
            colors.pressedColor = new Color(0.15f, 0.25f, 0.35f, 1);
            colors.selectedColor = new Color(0.3f, 0.4f, 0.5f, 1);
            button.colors = colors;
            
            // Create button text
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = new Vector2(15, 5);
            buttonTextRect.offsetMax = new Vector2(-15, -5);
            buttonText.alignment = TextAlignmentOptions.Left;
            buttonText.fontSize = 16;
            buttonText.color = Color.white;
            buttonText.text = "Response Option";
            
            // Set button reference
            dialogueView.responseButtonTemplate = button;
            
            // Deactivate button template
            buttonObj.SetActive(false);
            
            // Hide dialogue panel by default
            panelObj.SetActive(false);
            
            Debug.Log("DialogueView system created successfully");
            
            return dialogueView;
        }
    }
}