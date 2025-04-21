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
        [SerializeField] private Vector2 dialoguePanelSize = new Vector2(800, 400);
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
            
            // Create content area
            GameObject contentObj = SetupChild(dialoguePanel, "ContentPanel", 
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(dialoguePanelSize.x - 40f, dialoguePanelSize.y - 150f), new Vector2(0f, 0f));
            
            // Create event description text if needed
            if (eventDescriptionText == null)
            {
                GameObject descObj = SetupChild(contentObj.GetComponent<RectTransform>(), "DescriptionText", 
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(dialoguePanelSize.x - 60f, dialoguePanelSize.y / 2f - 40f), new Vector2(0f, -20f));
                
                eventDescriptionText = descObj.AddComponent<TextMeshProUGUI>();
                eventDescriptionText.text = "This is where the event description will appear. It can be multiple lines of text explaining the situation that the player needs to respond to.";
                eventDescriptionText.fontSize = descriptionFontSize;
                eventDescriptionText.alignment = TextAlignmentOptions.Top;
                eventDescriptionText.color = descriptionTextColor;
                eventDescriptionText.textWrappingMode = TextWrappingModes.Normal;
                
                Debug.Log("Created event description text");
            }
            
            // Create response container if needed
            if (responseContainer == null)
            {
                GameObject responseObj = SetupChild(dialoguePanel, "ResponseContainer", 
                    new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                    new Vector2(dialoguePanelSize.x - 40f, 200f), new Vector2(0f, 100f));
                
                responseContainer = responseObj.GetComponent<RectTransform>();
                
                // Add vertical layout group for responses
                VerticalLayoutGroup layoutGroup = responseObj.AddComponent<VerticalLayoutGroup>();
                layoutGroup.spacing = buttonSpacing;
                layoutGroup.padding = new RectOffset(20, 20, 0, 20);
                layoutGroup.childAlignment = TextAnchor.UpperCenter;
                layoutGroup.childControlHeight = false;
                layoutGroup.childControlWidth = false;
                layoutGroup.childForceExpandHeight = false;
                layoutGroup.childForceExpandWidth = false;
                
                // Add content size fitter for dynamic sizing
                ContentSizeFitter fitter = responseObj.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                
                Debug.Log("Created response container with layout");
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
                    new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                    Vector2.zero, Vector2.zero);
                
                TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
                buttonText.text = "Response Option";
                buttonText.fontSize = responseFontSize;
                buttonText.alignment = TextAlignmentOptions.Center;
                buttonText.color = responseTextColor;
                buttonText.margin = new Vector4(10, 5, 10, 5);
                buttonText.textWrappingMode = TextWrappingModes.Normal;
                
                Debug.Log("Created response button template");
                
                // Initially hide the template - will be used to instantiate real buttons
                buttonObj.SetActive(false);
            }
            
            // Reference all components in DialogueView
            dialogueView.dialoguePanel = dialoguePanel;
            dialogueView.eventTitleText = eventTitleText;
            dialogueView.eventDescriptionText = eventDescriptionText;
            dialogueView.responseContainer = responseContainer;
            dialogueView.responseButtonTemplate = responseButtonTemplate;
            
            Debug.Log("Dialogue prefab setup complete!");
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
    }
}