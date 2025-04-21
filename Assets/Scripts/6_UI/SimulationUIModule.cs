using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core;
using Systems;

namespace UI
{
    /// <summary>
    /// UI Module for simulation controls (play, pause, step, speed)
    /// </summary>
    public class SimulationUIModule : UIModuleBase
    {
        [Header("Core Systems")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private EconomicSystem economicSystem;
        
        [Header("UI Controls")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button stepButton;
        [SerializeField] private Slider speedSlider;
        [SerializeField] private TextMeshProUGUI statusText;
        
        public override void Initialize()
        {
            base.Initialize();
            
            // Find systems if not set in inspector
            if (turnManager == null)
                turnManager = FindFirstObjectByType<TurnManager>();
                
            if (economicSystem == null)
                economicSystem = FindFirstObjectByType<EconomicSystem>();
            
            SetupUI();
        }
        
        private void SetupUI()
        {
            // Create UI elements if they don't exist
            if (playButton == null || pauseButton == null || stepButton == null || 
                speedSlider == null || statusText == null)
            {
                CreateUIElements();
            }
            
            // Setup button callbacks
            playButton.onClick.AddListener(ResumeSimulation);
            pauseButton.onClick.AddListener(PauseSimulation);
            stepButton.onClick.AddListener(StepSimulation);
            speedSlider.onValueChanged.AddListener(ChangeSpeed);
            
            UpdateStatusText();
        }
        
        private void CreateUIElements()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }
            
            // Add a horizontal layout group
            HorizontalLayoutGroup layout = GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
            {
                layout = gameObject.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 10;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
                layout.padding = new RectOffset(10, 10, 5, 5);
            }
            
            // Create play button if it doesn't exist
            if (playButton == null)
            {
                playButton = CreateButton("PlayButton", "Play");
            }
            
            // Create pause button if it doesn't exist
            if (pauseButton == null)
            {
                pauseButton = CreateButton("PauseButton", "Pause");
            }
            
            // Create step button if it doesn't exist
            if (stepButton == null)
            {
                stepButton = CreateButton("StepButton", "Step");
            }
            
            // Create speed slider if it doesn't exist
            if (speedSlider == null)
            {
                GameObject sliderObj = new GameObject("SpeedSlider");
                sliderObj.transform.SetParent(transform, false);
                
                speedSlider = sliderObj.AddComponent<Slider>();
                speedSlider.minValue = 0;
                speedSlider.maxValue = 1;
                speedSlider.value = 0.5f;
                
                // Add required slider parts
                CreateSliderParts(sliderObj);
                
                // Set slider size
                RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
                sliderRect.sizeDelta = new Vector2(120, 30);
                
                // Add a layout element
                LayoutElement sliderLayout = sliderObj.AddComponent<LayoutElement>();
                sliderLayout.preferredWidth = 120;
                sliderLayout.preferredHeight = 30;
                
                // Add label above slider
                GameObject labelObj = new GameObject("Label");
                labelObj.transform.SetParent(sliderObj.transform, false);
                
                TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
                label.text = "Speed";
                label.fontSize = 12;
                label.alignment = TextAlignmentOptions.Center;
                
                RectTransform labelRect = labelObj.GetComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0, 1);
                labelRect.anchorMax = new Vector2(1, 1);
                labelRect.pivot = new Vector2(0.5f, 0);
                labelRect.sizeDelta = new Vector2(0, 20);
                labelRect.anchoredPosition = new Vector2(0, 0);
            }
            
            // Create status text if it doesn't exist
            if (statusText == null)
            {
                GameObject textObj = new GameObject("StatusText");
                textObj.transform.SetParent(transform, false);
                
                statusText = textObj.AddComponent<TextMeshProUGUI>();
                statusText.text = "Status: Ready";
                statusText.fontSize = 16;
                statusText.alignment = TextAlignmentOptions.Left;
                
                RectTransform textRect = textObj.GetComponent<RectTransform>();
                textRect.sizeDelta = new Vector2(150, 30);
                
                LayoutElement textLayout = textObj.AddComponent<LayoutElement>();
                textLayout.preferredWidth = 150;
                textLayout.preferredHeight = 30;
            }
        }
        
        private Button CreateButton(string name, string text)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(transform, false);
            
            // Add image component for background
            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.3f);
            
            // Add button component
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = image;
            
            // Set button colors
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.2f, 0.3f);
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.4f);
            colors.pressedColor = new Color(0.15f, 0.15f, 0.25f);
            button.colors = colors;
            
            // Add text child
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = 16;
            textComponent.alignment = TextAlignmentOptions.Center;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            // Set button size
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(80, 30);
            
            // Add layout element
            LayoutElement buttonLayout = buttonObj.AddComponent<LayoutElement>();
            buttonLayout.preferredWidth = 80;
            buttonLayout.preferredHeight = 30;
            
            return button;
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
        
        private void ResumeSimulation()
        {
            if (turnManager != null)
            {
                turnManager.Resume();
                UpdateStatusText();
            }
        }
        
        private void PauseSimulation()
        {
            if (turnManager != null)
            {
                turnManager.Pause();
                UpdateStatusText();
            }
        }
        
        private void StepSimulation()
        {
            if (economicSystem != null)
            {
                economicSystem.ProcessEconomicTick();
                UpdateStatusText();
            }
        }
        
        private void ChangeSpeed(float value)
        {
            if (turnManager != null)
            {
                // Map slider 0-1 to meaningful time scale between 0.5x and 3x
                float speed = 0.5f + (value * 2.5f);
                turnManager.SetTimeScale(speed);
                UpdateStatusText();
            }
        }
        
        private void UpdateStatusText()
        {
            if (statusText != null)
            {
                string status = "Status: ";
                
                if (turnManager != null)
                {
                    status += turnManager.IsPaused ? "PAUSED" : "RUNNING";
                    status += $" (Speed: {turnManager.GetTimeScale():F1}x)";
                }
                else
                {
                    status += "No TurnManager found";
                }
                
                statusText.text = status;
            }
        }
    }
}