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
                    
                    // Try to find the other objects as children of the panel
                    TryFindComponents();
                }
                else
                {
                    // Check if we ARE the dialogue panel
                    dialoguePanel = GetComponent<RectTransform>();
                    if (dialoguePanel != null)
                    {
                        Debug.Log("DialogueView: Using this GameObject as the dialoguePanel.");
                        TryFindComponents();
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
    }
}