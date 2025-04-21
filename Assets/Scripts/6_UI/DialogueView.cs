using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace UI
{
    /// <summary>
    /// DialogueView manages the dialogue UI window and handles player interaction with dialogue events
    /// </summary>
    public class DialogueView : MonoBehaviour
    {
        // UI References
        public RectTransform dialoguePanel;
        public TextMeshProUGUI eventTitleText;
        public TextMeshProUGUI eventDescriptionText;
        public RectTransform responseContainer;
        public Button responseButtonTemplate;
        
        // Dialogue data
        private string currentEventId;
        private List<GameObject> activeResponseButtons = new List<GameObject>();
        
        // Events
        public event Action<string, int> OnResponseSelected;
        
        private void Awake()
        {
            // Check components are set up properly
            CheckComponents();
            
            // Initially hide the dialogue panel
            if (dialoguePanel != null)
            {
                dialoguePanel.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Validates the required components and logs errors for any missing ones
        /// </summary>
        private void CheckComponents()
        {
            if (dialoguePanel == null)
                Debug.LogError("DialogueView: dialoguePanel is null! Please set it up using DialoguePrefabSetup.");
                
            if (eventTitleText == null)
                Debug.LogError("DialogueView: eventTitleText is null! Please set it up using DialoguePrefabSetup.");
                
            if (eventDescriptionText == null)
                Debug.LogError("DialogueView: eventDescriptionText is null! Please set it up using DialoguePrefabSetup.");
                
            if (responseContainer == null)
                Debug.LogError("DialogueView: responseContainer is null! Please set it up using DialoguePrefabSetup.");
                
            if (responseButtonTemplate == null)
                Debug.LogError("DialogueView: responseButtonTemplate is null! Please set it up using DialoguePrefabSetup.");
        }
        
        /// <summary>
        /// Show a dialogue event with title, description and response options
        /// </summary>
        /// <param name="eventId">Unique identifier for the event</param>
        /// <param name="title">Title of the event</param>
        /// <param name="description">Detailed description of the event</param>
        /// <param name="responses">List of possible responses</param>
        public void ShowDialogue(string eventId, string title, string description, List<string> responses)
        {
            // Check if components are properly set up
            if (dialoguePanel == null || eventTitleText == null || eventDescriptionText == null || 
                responseContainer == null || responseButtonTemplate == null)
            {
                Debug.LogError($"DialogueView: Cannot show dialogue - UI components are missing. Event: {title}");
                return;
            }
            
            currentEventId = eventId;
            
            // Set up the texts
            if (eventTitleText != null)
                eventTitleText.text = title;
                
            if (eventDescriptionText != null)
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
            if (dialoguePanel != null && dialoguePanel.gameObject != null)
            {
                dialoguePanel.gameObject.SetActive(true);
                Debug.Log($"DialogueView: Showing dialogue '{title}'");
            }
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