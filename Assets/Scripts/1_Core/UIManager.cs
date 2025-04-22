using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Central manager for all UI modules
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Canvas References")]
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private RectTransform topPanel;
        [SerializeField] private RectTransform bottomPanel;
        [SerializeField] private RectTransform leftPanel;
        [SerializeField] private RectTransform rightPanel;
        [SerializeField] private RectTransform centerPanel;
        
        [Header("Initialization")]
        [SerializeField] private bool autoInitialize = true;
        
        // List of all registered UI modules
        private List<UIModuleBase> uiModules = new List<UIModuleBase>();
        
        private void Awake()
        {
            if (autoInitialize)
            {
                Initialize();
            }
        }
        
        /// <summary>
        /// Initialize the UI Manager and all UI modules
        /// </summary>
        public void Initialize()
        {
            // Create UI layout if not set in inspector
            CreateUILayoutIfNeeded();
            
            // Discover UI modules in the scene
            DiscoverUIModules();
            
            // Initialize all modules
            InitializeModules();
        }
        
        /// <summary>
        /// Register a UI module with the manager
        /// </summary>
        public void RegisterModule(UIModuleBase module)
        {
            if (!uiModules.Contains(module))
            {
                uiModules.Add(module);
                module.Initialize();
            }
        }
        
        /// <summary>
        /// Create a new UI module of the specified type
        /// </summary>
        public T CreateModule<T>(UIPosition position = UIPosition.Center) where T : UIModuleBase
        {
            RectTransform parent = GetPanelForPosition(position);
            
            GameObject moduleObj = new GameObject(typeof(T).Name);
            moduleObj.transform.SetParent(parent, false);
            
            // Add rect transform
            RectTransform rectTransform = moduleObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            
            // Add module component
            T module = moduleObj.AddComponent<T>();
            
            // Register the module
            RegisterModule(module);
            
            return module;
        }
        
        /// <summary>
        /// Find a UI module of the specified type
        /// </summary>
        public T GetModule<T>() where T : UIModuleBase
        {
            foreach (UIModuleBase module in uiModules)
            {
                if (module is T typedModule)
                {
                    return typedModule;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Resize a panel to the specified size
        /// </summary>
        /// <param name="position">The panel to resize</param>
        /// <param name="size">The new size (for side panels: width, for top/bottom: height)</param>
        public void ResizePanel(UIPosition position, float size)
        {
            RectTransform panel = GetPanelForPosition(position);
            
            switch (position)
            {
                case UIPosition.Top:
                case UIPosition.Bottom:
                    // For top/bottom panels, change the height
                    panel.sizeDelta = new Vector2(panel.sizeDelta.x, size);
                    break;
                    
                case UIPosition.Left:
                case UIPosition.Right:
                    // For left/right panels, change the width
                    panel.sizeDelta = new Vector2(size, panel.sizeDelta.y);
                    break;
                    
                case UIPosition.Center:
                    // For center panel, both dimensions
                    Debug.LogWarning("Use ResizeCenterPanel for center panel to specify both dimensions");
                    break;
            }
        }
        
        /// <summary>
        /// Resize the center panel with specific width and height
        /// </summary>
        /// <param name="width">The new width</param>
        /// <param name="height">The new height</param>
        public void ResizeCenterPanel(float width, float height)
        {
            if (centerPanel != null)
            {
                centerPanel.sizeDelta = new Vector2(width, height);
            }
        }
        
        /// <summary>
        /// Get the current size of a panel
        /// </summary>
        /// <param name="position">The panel position</param>
        /// <returns>The size of the panel (for top/bottom: height, for left/right: width, for center: Vector2 with both dimensions)</returns>
        public Vector2 GetPanelSize(UIPosition position)
        {
            RectTransform panel = GetPanelForPosition(position);
            return panel.sizeDelta;
        }
        
        /// <summary>
        /// Show all registered UI modules
        /// </summary>
        public void ShowAllModules()
        {
            foreach (UIModuleBase module in uiModules)
            {
                module.Show();
            }
        }
        
        /// <summary>
        /// Hide all registered UI modules
        /// </summary>
        public void HideAllModules()
        {
            foreach (UIModuleBase module in uiModules)
            {
                module.Hide();
            }
        }
        
        /// <summary>
        /// Position a game object in a specified panel
        /// </summary>
        public void PositionInPanel(GameObject obj, UIPosition position)
        {
            RectTransform parent = GetPanelForPosition(position);
            obj.transform.SetParent(parent, false);
            
            RectTransform rectTransform = obj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Reset positioning
                rectTransform.anchoredPosition = Vector2.zero;
                
                // Apply layout behavior based on position
                switch (position)
                {
                    case UIPosition.Top:
                    case UIPosition.Bottom:
                        // Horizontal layout
                        HorizontalLayoutGroup layout = parent.GetComponent<HorizontalLayoutGroup>();
                        if (layout == null)
                        {
                            layout = parent.gameObject.AddComponent<HorizontalLayoutGroup>();
                            layout.childAlignment = TextAnchor.MiddleCenter;
                            layout.spacing = 10;
                            layout.childForceExpandWidth = false;
                            layout.childForceExpandHeight = true;
                            layout.padding = new RectOffset(10, 10, 5, 5);
                        }
                        break;
                        
                    case UIPosition.Left:
                    case UIPosition.Right:
                        // Vertical layout
                        VerticalLayoutGroup vertLayout = parent.GetComponent<VerticalLayoutGroup>();
                        if (vertLayout == null)
                        {
                            vertLayout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
                            vertLayout.childAlignment = TextAnchor.MiddleCenter;
                            vertLayout.spacing = 10;
                            vertLayout.childForceExpandWidth = true;
                            vertLayout.childForceExpandHeight = false;
                            vertLayout.padding = new RectOffset(5, 5, 10, 10);
                        }
                        break;
                        
                    case UIPosition.Center:
                        // Grid layout
                        GridLayoutGroup gridLayout = parent.GetComponent<GridLayoutGroup>();
                        if (gridLayout == null)
                        {
                            gridLayout = parent.gameObject.AddComponent<GridLayoutGroup>();
                            gridLayout.cellSize = new Vector2(100, 100);
                            gridLayout.spacing = new Vector2(10, 10);
                            gridLayout.childAlignment = TextAnchor.MiddleCenter;
                        }
                        break;
                }
            }
        }
        
        /// <summary>
        /// Create the basic UI layout if it doesn't already exist
        /// </summary>
        private void CreateUILayoutIfNeeded()
        {
            // Check for canvas
            if (mainCanvas == null)
            {
                GameObject canvasObj = new GameObject("MainCanvas");
                mainCanvas = canvasObj.AddComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            // Create panels if they don't exist
            if (topPanel == null)
            {
                topPanel = CreatePanel("TopPanel", mainCanvas.transform);
                topPanel.anchorMin = new Vector2(0, 1);
                topPanel.anchorMax = new Vector2(1, 1);
                topPanel.pivot = new Vector2(0.5f, 1);
                topPanel.sizeDelta = new Vector2(0, 80);
                topPanel.anchoredPosition = Vector2.zero;
            }
            
            if (bottomPanel == null)
            {
                bottomPanel = CreatePanel("BottomPanel", mainCanvas.transform);
                bottomPanel.anchorMin = new Vector2(0, 0);
                bottomPanel.anchorMax = new Vector2(1, 0);
                bottomPanel.pivot = new Vector2(0.5f, 0);
                bottomPanel.sizeDelta = new Vector2(0, 150);
                bottomPanel.anchoredPosition = Vector2.zero;
            }
            
            if (leftPanel == null)
            {
                leftPanel = CreatePanel("LeftPanel", mainCanvas.transform);
                leftPanel.anchorMin = new Vector2(0, 0);
                leftPanel.anchorMax = new Vector2(0, 1);
                leftPanel.pivot = new Vector2(0, 0.5f);
                leftPanel.sizeDelta = new Vector2(200, 0);
                leftPanel.anchoredPosition = Vector2.zero;
            }
            
            if (rightPanel == null)
            {
                rightPanel = CreatePanel("RightPanel", mainCanvas.transform);
                rightPanel.anchorMin = new Vector2(1, 0);
                rightPanel.anchorMax = new Vector2(1, 1);
                rightPanel.pivot = new Vector2(1, 0.5f);
                rightPanel.sizeDelta = new Vector2(200, 0);
                rightPanel.anchoredPosition = Vector2.zero;
            }
            
            if (centerPanel == null)
            {
                centerPanel = CreatePanel("CenterPanel", mainCanvas.transform);
                centerPanel.anchorMin = new Vector2(0.5f, 0.5f);
                centerPanel.anchorMax = new Vector2(0.5f, 0.5f);
                centerPanel.pivot = new Vector2(0.5f, 0.5f);
                centerPanel.sizeDelta = new Vector2(400, 300);
                centerPanel.anchoredPosition = Vector2.zero;
            }
        }
        
        /// <summary>
        /// Create a panel with the given name
        /// </summary>
        private RectTransform CreatePanel(string name, Transform parent)
        {
            GameObject panelObj = new GameObject(name);
            panelObj.transform.SetParent(parent, false);
            
            // Add components
            RectTransform rectTransform = panelObj.AddComponent<RectTransform>();
            
            // Initially invisible
            Image image = panelObj.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0);
            
            return rectTransform;
        }
        
        /// <summary>
        /// Get the panel for a specific position
        /// </summary>
        private RectTransform GetPanelForPosition(UIPosition position)
        {
            switch (position)
            {
                case UIPosition.Top:
                    return topPanel;
                case UIPosition.Bottom:
                    return bottomPanel;
                case UIPosition.Left:
                    return leftPanel;
                case UIPosition.Right:
                    return rightPanel;
                case UIPosition.Center:
                default:
                    return centerPanel;
            }
        }
        
        /// <summary>
        /// Discover UI modules in the scene
        /// </summary>
        private void DiscoverUIModules()
        {
            // Find all UI modules in the scene
            UIModuleBase[] modules = FindObjectsByType<UIModuleBase>(FindObjectsSortMode.None);
            
            foreach (UIModuleBase module in modules)
            {
                RegisterModule(module);
            }
        }
        
        /// <summary>
        /// Initialize all registered modules
        /// </summary>
        private void InitializeModules()
        {
            foreach (UIModuleBase module in uiModules)
            {
                module.Initialize();
            }
        }
    }
}