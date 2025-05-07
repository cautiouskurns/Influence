using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI.Config;

namespace UI
{
    /// <summary>
    /// Central manager for all UI modules in the game interface.
    /// 
    /// This class manages UI layout, module creation, registration, and provides
    /// utilities for positioning and organizing UI elements across standardized panels.
    /// It follows a Singleton pattern for global access while maintaining encapsulation.
    /// 
    /// Responsibilities:
    /// - Maintaining the main UI canvas and panel structure
    /// - Registration and initialization of UI modules
    /// - Dynamic creation of UI modules at runtime
    /// - Panel positioning and layout management
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region Singleton Pattern
        private static UIManager _instance;
        
        /// <summary>
        /// Singleton accessor for global access to the UI manager
        /// </summary>
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<UIManager>();
                    
                    if (_instance == null)
                    {
                        Debug.LogWarning("No UIManager found - creating a new instance");
                        GameObject go = new GameObject("UIManager");
                        _instance = go.AddComponent<UIManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion
        
        #region Events
        /// <summary>
        /// Event raised when a new UI module is registered with the manager
        /// </summary>
        public event Action<UIModuleBase> OnModuleRegistered;
        
        /// <summary>
        /// Event raised when the UI layout is initialized or changed significantly
        /// </summary>
        public event Action OnUILayoutInitialized;
        #endregion
        
        #region Inspector Fields
        [Header("Canvas References")]
        [Tooltip("Main canvas for all UI elements")]
        [SerializeField] private Canvas mainCanvas;
        
        [Tooltip("Panel positioned at the top of the screen")]
        [SerializeField] private RectTransform topPanel;
        
        [Tooltip("Panel positioned at the bottom of the screen")]
        [SerializeField] private RectTransform bottomPanel;
        
        [Tooltip("Panel positioned at the left side of the screen")]
        [SerializeField] private RectTransform leftPanel;
        
        [Tooltip("Panel positioned at the right side of the screen")]
        [SerializeField] private RectTransform rightPanel;
        
        [Tooltip("Panel positioned at the center of the screen")]
        [SerializeField] private RectTransform centerPanel;
        
        [Header("Configuration")]
        [Tooltip("ScriptableObject containing UI layout configuration")]
        [SerializeField] private UIManagerConfig config;
        
        [Tooltip("Whether to automatically initialize UI on Awake")]
        [SerializeField] private bool autoInitialize = true;
        #endregion
        
        #region Private Fields
        // List of all registered UI modules
        private List<UIModuleBase> _uiModules = new List<UIModuleBase>();
        
        // Dictionary for fast lookup of modules by type
        private Dictionary<Type, UIModuleBase> _moduleCache = new Dictionary<Type, UIModuleBase>();
        
        // Initialization state
        private bool _initialized = false;
        #endregion
        
        #region Unity Lifecycle Methods
        /// <summary>
        /// Handles singleton initialization and auto-initialization if enabled
        /// </summary>
        private void Awake()
        {
            // Singleton pattern enforcement
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning($"Multiple UIManager instances detected. Destroying duplicate on {gameObject.name}");
                Destroy(this);
                return;
            }
            
            _instance = this;
            
            // Ensure we have a configuration
            EnsureConfiguration();
            
            if (autoInitialize)
            {
                Initialize();
            }
        }
        
        /// <summary>
        /// Validates serialized field values in the editor
        /// </summary>
        private void OnValidate()
        {
            // Nothing to validate here since values are now in the config
        }
        
        /// <summary>
        /// Clean up resources on destruction
        /// </summary>
        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
            
            // Clear module collections
            _uiModules.Clear();
            _moduleCache.Clear();
        }
        #endregion
        
        #region Configuration Management
        /// <summary>
        /// Ensures a configuration asset is available, creates a runtime instance if needed
        /// </summary>
        private void EnsureConfiguration()
        {
            if (config == null)
            {
                // Try to load from Resources first
                config = Resources.Load<UIManagerConfig>("UIManagerConfig");
                
                // If still null, create a default runtime instance
                if (config == null)
                {
                    config = UIManagerConfig.CreateDefaultConfig();
                    Debug.LogWarning("No UIManagerConfig found. Using default runtime configuration. " +
                                    "Create a configuration asset via 'Create > UI > UIManager Configuration'");
                }
            }
        }
        
        /// <summary>
        /// Gets the current UI configuration
        /// </summary>
        public UIManagerConfig GetConfiguration()
        {
            EnsureConfiguration();
            return config;
        }
        
        /// <summary>
        /// Sets a new configuration
        /// </summary>
        /// <param name="newConfig">The new configuration to use</param>
        public void SetConfiguration(UIManagerConfig newConfig)
        {
            if (newConfig == null)
            {
                Debug.LogError("Attempted to set null UIManagerConfig");
                return;
            }
            
            bool wasInitialized = _initialized;
            
            // Store new config
            config = newConfig;
            
            // Re-initialize if already initialized
            if (wasInitialized)
            {
                ResetUILayout();
            }
        }
        #endregion
        
        #region Initialization Methods
        /// <summary>
        /// Initialize the UI Manager and all UI modules.
        /// Creates the UI layout if needed, discovers existing modules,
        /// and ensures essential modules are available.
        /// </summary>
        public void Initialize()
        {
            if (_initialized)
            {
                Debug.LogWarning("UIManager is already initialized");
                return;
            }
            
            try
            {
                // Ensure we have configuration
                EnsureConfiguration();
                
                // Create UI layout if not set in inspector
                CreateUILayoutIfNeeded();
                
                // Discover UI modules in the scene
                DiscoverUIModules();
                
                // Create essential UI modules if they don't exist and it's enabled in config
                if (config.autoCreateEssentialModules)
                {
                    CreateEssentialModules();
                }
                
                // Initialize all modules
                InitializeModules();
                
                _initialized = true;
                OnUILayoutInitialized?.Invoke();
                
                Debug.Log($"UIManager initialized with {_uiModules.Count} modules");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error initializing UIManager: {e.Message}\n{e.StackTrace}");
            }
        }
        
        /// <summary>
        /// Create the basic UI layout if it doesn't already exist.
        /// Sets up the main canvas and all panels with appropriate anchors and positions.
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
                scaler.referenceResolution = config.referenceResolution;
                
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            // Create panels if they don't exist
            if (topPanel == null)
            {
                topPanel = CreatePanel("TopPanel", mainCanvas.transform);
                topPanel.anchorMin = new Vector2(0, 1);
                topPanel.anchorMax = new Vector2(1, 1);
                topPanel.pivot = new Vector2(0.5f, 1);
                topPanel.sizeDelta = new Vector2(0, config.topPanelHeight);
                topPanel.anchoredPosition = Vector2.zero;
            }
            
            if (bottomPanel == null)
            {
                bottomPanel = CreatePanel("BottomPanel", mainCanvas.transform);
                bottomPanel.anchorMin = new Vector2(0, 0);
                bottomPanel.anchorMax = new Vector2(1, 0);
                bottomPanel.pivot = new Vector2(0.5f, 0);
                bottomPanel.sizeDelta = new Vector2(0, config.bottomPanelHeight);
                bottomPanel.anchoredPosition = Vector2.zero;
            }
            
            if (leftPanel == null)
            {
                leftPanel = CreatePanel("LeftPanel", mainCanvas.transform);
                leftPanel.anchorMin = new Vector2(0, 0);
                leftPanel.anchorMax = new Vector2(0, 1);
                leftPanel.pivot = new Vector2(0, 0.5f);
                leftPanel.sizeDelta = new Vector2(config.sidePanelWidth, 0);
                leftPanel.anchoredPosition = Vector2.zero;
            }
            
            if (rightPanel == null)
            {
                rightPanel = CreatePanel("RightPanel", mainCanvas.transform);
                rightPanel.anchorMin = new Vector2(1, 0);
                rightPanel.anchorMax = new Vector2(1, 1);
                rightPanel.pivot = new Vector2(1, 0.5f);
                rightPanel.sizeDelta = new Vector2(config.sidePanelWidth, 0);
                rightPanel.anchoredPosition = Vector2.zero;
            }
            
            if (centerPanel == null)
            {
                centerPanel = CreatePanel("CenterPanel", mainCanvas.transform);
                centerPanel.anchorMin = new Vector2(0.5f, 0.5f);
                centerPanel.anchorMax = new Vector2(0.5f, 0.5f);
                centerPanel.pivot = new Vector2(0.5f, 0.5f);
                centerPanel.sizeDelta = config.centerPanelSize;
                centerPanel.anchoredPosition = Vector2.zero;
            }
        }
        
        /// <summary>
        /// Discover UI modules already present in the scene
        /// </summary>
        private void DiscoverUIModules()
        {
            // Find all UI modules in the scene
            UIModuleBase[] modules = FindObjectsByType<UIModuleBase>(FindObjectsSortMode.None);
            
            foreach (UIModuleBase module in modules)
            {
                if (module != null)
                {
                    RegisterModule(module);
                }
            }
        }
        
        /// <summary>
        /// Initialize all registered modules
        /// </summary>
        private void InitializeModules()
        {
            foreach (UIModuleBase module in _uiModules)
            {
                if (module != null)
                {
                    try
                    {
                        module.Initialize();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error initializing module {module.GetType().Name}: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Create essential UI modules if they don't exist yet
        /// </summary>
        private void CreateEssentialModules()
        {
            // Check if we already have a GameStatsUIModule
            if (GetModule<GameStatsUIModule>() == null)
            {
                // Create the turn counter in the bottom panel
                CreateModule<GameStatsUIModule>(UIPosition.Bottom);
            }
        }
        #endregion
        
        #region Module Management Methods
        /// <summary>
        /// Register a UI module with the manager.
        /// Once registered, the module will be initialized and can be accessed through GetModule.
        /// </summary>
        /// <param name="module">The UI module to register</param>
        public void RegisterModule(UIModuleBase module)
        {
            if (module == null)
            {
                Debug.LogError("Attempted to register null UI module");
                return;
            }
            
            if (!_uiModules.Contains(module))
            {
                _uiModules.Add(module);
                _moduleCache[module.GetType()] = module;
                
                try
                {
                    module.Initialize();
                    OnModuleRegistered?.Invoke(module);
                    
                    // Debug.Log($"Registered UI module: {module.GetType().Name}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to initialize UI module {module.GetType().Name}: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// Create a new UI module of the specified type and position it in the given panel.
        /// </summary>
        /// <typeparam name="T">The type of UI module to create</typeparam>
        /// <param name="position">Which panel to place the module in</param>
        /// <returns>The created UI module instance</returns>
        public T CreateModule<T>(UIPosition position = UIPosition.Center) where T : UIModuleBase
        {
            RectTransform parent = GetPanelForPosition(position);
            if (parent == null)
            {
                Debug.LogError($"Failed to create UI module {typeof(T).Name}: Panel not found for position {position}");
                return null;
            }
            
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
        /// Find a UI module of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of UI module to find</typeparam>
        /// <returns>The UI module if found, null otherwise</returns>
        public T GetModule<T>() where T : UIModuleBase
        {
            // Try fast lookup first
            if (_moduleCache.TryGetValue(typeof(T), out UIModuleBase cachedModule))
            {
                return cachedModule as T;
            }
            
            // Fall back to linear search
            foreach (UIModuleBase module in _uiModules)
            {
                if (module is T typedModule)
                {
                    // Cache for future lookups
                    _moduleCache[typeof(T)] = module;
                    return typedModule;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Get all registered UI modules
        /// </summary>
        /// <returns>A read-only list of all UI modules</returns>
        public IReadOnlyList<UIModuleBase> GetAllModules()
        {
            return _uiModules.AsReadOnly();
        }
        
        /// <summary>
        /// Show all registered UI modules
        /// </summary>
        public void ShowAllModules()
        {
            foreach (UIModuleBase module in _uiModules)
            {
                if (module != null)
                {
                    module.Show();
                }
            }
        }
        
        /// <summary>
        /// Hide all registered UI modules
        /// </summary>
        public void HideAllModules()
        {
            foreach (UIModuleBase module in _uiModules)
            {
                if (module != null)
                {
                    module.Hide();
                }
            }
        }
        #endregion
        
        #region Panel Management Methods
        /// <summary>
        /// Create a panel with the given name
        /// </summary>
        /// <param name="name">Name of the panel GameObject</param>
        /// <param name="parent">Parent transform</param>
        /// <returns>RectTransform of the created panel</returns>
        private RectTransform CreatePanel(string name, Transform parent)
        {
            GameObject panelObj = new GameObject(name);
            panelObj.transform.SetParent(parent, false);
            
            // Add components
            RectTransform rectTransform = panelObj.AddComponent<RectTransform>();
            
            // Add image component with configurable debug color
            Image image = panelObj.AddComponent<Image>();
            if (config != null && config.showPanelDebugBorders)
            {
                image.color = config.panelDebugBorderColor;
            }
            else
            {
                image.color = new Color(0, 0, 0, 0); // Transparent
            }
            
            return rectTransform;
        }
        
        /// <summary>
        /// Get the panel for a specific position
        /// </summary>
        /// <param name="position">The desired UI position</param>
        /// <returns>RectTransform for the corresponding panel</returns>
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
        /// Resize a panel to the specified size
        /// </summary>
        /// <param name="position">The panel to resize</param>
        /// <param name="size">The new size (for side panels: width, for top/bottom: height)</param>
        public void ResizePanel(UIPosition position, float size)
        {
            RectTransform panel = GetPanelForPosition(position);
            if (panel == null)
            {
                Debug.LogError($"Cannot resize panel: No panel found for position {position}");
                return;
            }
            
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
            else
            {
                Debug.LogError("Cannot resize center panel: Panel not found");
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
            return panel != null ? panel.sizeDelta : Vector2.zero;
        }
        
        /// <summary>
        /// Position a game object in a specified panel and apply appropriate layout
        /// </summary>
        /// <param name="obj">The GameObject to position</param>
        /// <param name="position">Which panel to place it in</param>
        public void PositionInPanel(GameObject obj, UIPosition position)
        {
            if (obj == null)
            {
                Debug.LogError("Cannot position null GameObject in panel");
                return;
            }
            
            RectTransform parent = GetPanelForPosition(position);
            if (parent == null)
            {
                Debug.LogError($"Cannot position GameObject: No panel found for position {position}");
                return;
            }
            
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
                        ConfigureHorizontalLayout(parent);
                        break;
                        
                    case UIPosition.Left:
                    case UIPosition.Right:
                        ConfigureVerticalLayout(parent);
                        break;
                        
                    case UIPosition.Center:
                        ConfigureGridLayout(parent);
                        break;
                }
            }
            else
            {
                Debug.LogWarning($"GameObject {obj.name} doesn't have a RectTransform component");
            }
        }
        
        /// <summary>
        /// Configure horizontal layout on a panel
        /// </summary>
        private void ConfigureHorizontalLayout(RectTransform parent)
        {
            HorizontalLayoutGroup layout = parent.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
            {
                layout = parent.gameObject.AddComponent<HorizontalLayoutGroup>();
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.spacing = config.defaultElementSpacing;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = true;
                layout.padding = new RectOffset(config.panelPadding, config.panelPadding, 5, 5);
            }
        }
        
        /// <summary>
        /// Configure vertical layout on a panel
        /// </summary>
        private void ConfigureVerticalLayout(RectTransform parent)
        {
            VerticalLayoutGroup vertLayout = parent.GetComponent<VerticalLayoutGroup>();
            if (vertLayout == null)
            {
                vertLayout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
                vertLayout.childAlignment = TextAnchor.MiddleCenter;
                vertLayout.spacing = config.defaultElementSpacing;
                vertLayout.childForceExpandWidth = true;
                vertLayout.childForceExpandHeight = false;
                vertLayout.padding = new RectOffset(5, 5, config.panelPadding, config.panelPadding);
            }
        }
        
        /// <summary>
        /// Configure grid layout on a panel
        /// </summary>
        private void ConfigureGridLayout(RectTransform parent)
        {
            GridLayoutGroup gridLayout = parent.GetComponent<GridLayoutGroup>();
            if (gridLayout == null)
            {
                gridLayout = parent.gameObject.AddComponent<GridLayoutGroup>();
                gridLayout.cellSize = new Vector2(100, 100);
                gridLayout.spacing = new Vector2(config.defaultElementSpacing, config.defaultElementSpacing);
                gridLayout.childAlignment = TextAnchor.MiddleCenter;
            }
        }
        #endregion
        
        #region Debug and Testing Methods
        #if UNITY_EDITOR
        /// <summary>
        /// Reset the entire UI layout
        /// </summary>
        public void ResetUILayout()
        {
            // Destroy all panel children first
            DestroyPanelContents(topPanel);
            DestroyPanelContents(bottomPanel);
            DestroyPanelContents(leftPanel);
            DestroyPanelContents(rightPanel);
            DestroyPanelContents(centerPanel);
            
            // Clear module lists
            _uiModules.Clear();
            _moduleCache.Clear();
            
            // Re-initialize
            _initialized = false;
            Initialize();
        }
        
        /// <summary>
        /// Helper to destroy all children of a panel
        /// </summary>
        private void DestroyPanelContents(RectTransform panel)
        {
            if (panel == null) return;
            
            for (int i = panel.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(panel.GetChild(i).gameObject);
            }
            
            // Remove layout components
            LayoutGroup layout = panel.GetComponent<LayoutGroup>();
            if (layout != null)
            {
                DestroyImmediate(layout);
            }
        }
        #endif
        #endregion
    }
    
    /// <summary>
    /// Enum representing UI element positions within the screen layout
    /// </summary>
    // public enum UIPosition
    // {
    //     /// <summary>Top of screen panel</summary>
    //     Top,
    //     /// <summary>Bottom of screen panel</summary>
    //     Bottom,
    //     /// <summary>Left side panel</summary>
    //     Left,
    //     /// <summary>Right side panel</summary>
    //     Right,
    //     /// <summary>Center of screen panel</summary>
    //     Center
    // }
}