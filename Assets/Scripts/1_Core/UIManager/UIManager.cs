using System;
using UnityEngine;
using UI.UIManager.HelperClasses;
using UI.Config;

namespace UI.Core
{
    /// <summary>
    /// Central manager for all UI modules in the game interface.
    /// 
    /// This class manages UI layout, module creation, registration, and provides
    /// utilities for positioning and organizing UI elements across standardized panels.
    /// It follows a Singleton pattern for global access while maintaining encapsulation.
    /// 
    /// Responsibilities:
    /// - Coordinating UI systems and components
    /// - Managing module lifecycle
    /// - Providing access to UI panels and layouts
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
        
        #region Events
        /// <summary>
        /// Event raised when the UI layout is initialized or changed significantly
        /// </summary>
        public event Action OnUILayoutInitialized;
        #endregion
        
        #region Private Fields
        // Helper classes
        private PanelManager _panelManager;
        private ModuleManager _moduleManager;
        
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
            
            // Initialize helpers
            InitializeHelpers();
            
            if (autoInitialize)
            {
                Initialize();
            }
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
            }
        }
        
        /// <summary>
        /// Initialize helper classes
        /// </summary>
        private void InitializeHelpers()
        {
            // Initialize panel manager with configuration settings
            _panelManager = new PanelManager(
                config.topPanelHeight,
                config.bottomPanelHeight,
                config.sidePanelWidth,
                config.centerPanelSize,
                config.defaultElementSpacing,
                config.panelPadding
            );
            
            // Set debug visualization options
            _panelManager.SetDebugVisualization(config.showPanelDebugBorders, config.panelDebugBorderColor);
            
            // Initialize panel manager with existing panel references
            _panelManager.Initialize(mainCanvas, topPanel, bottomPanel, leftPanel, rightPanel, centerPanel);
            
            // Initialize module manager
            _moduleManager = new ModuleManager();
            
            // Forward module registration events
            _moduleManager.OnModuleRegistered += (module) => {
                // You could add additional logic here when modules are registered
            };
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
                // Create UI layout if not set in inspector
                _panelManager.CreateUILayoutIfNeeded();
                
                // Discover UI modules in the scene
                _moduleManager.DiscoverUIModules();
                
                // Create essential UI modules if they don't exist and it's enabled in config
                if (config.autoCreateEssentialModules)
                {
                    CreateEssentialModules();
                }
                
                // Initialize all modules
                _moduleManager.InitializeModules();
                
                _initialized = true;
                OnUILayoutInitialized?.Invoke();
                
                Debug.Log($"UIManager initialized with {_moduleManager.GetAllModules().Count} modules");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error initializing UIManager: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// Create essential UI modules if they don't exist yet
        /// </summary>
        private void CreateEssentialModules()
        {
            // Check if we already have a GameStatsUIModule
            if (_moduleManager.GetModule<GameStatsUIModule>() == null)
            {
                // Create the turn counter in the bottom panel
                _moduleManager.CreateModule<GameStatsUIModule>(UIPosition.Bottom, _panelManager);
            }
        }
        #endregion
        
        #region Public Module Management Methods
        /// <summary>
        /// Register a UI module with the manager.
        /// Once registered, the module will be initialized and can be accessed through GetModule.
        /// </summary>
        /// <param name="module">The UI module to register</param>
        public void RegisterModule(UIModuleBase module)
        {
            _moduleManager.RegisterModule(module);
        }
        
        /// <summary>
        /// Create a new UI module of the specified type and position it in the given panel.
        /// </summary>
        /// <typeparam name="T">The type of UI module to create</typeparam>
        /// <param name="position">Which panel to place the module in</param>
        /// <returns>The created UI module instance</returns>
        public T CreateModule<T>(UIPosition position = UIPosition.Center) where T : UIModuleBase
        {
            return _moduleManager.CreateModule<T>(position, _panelManager);
        }
        
        /// <summary>
        /// Find a UI module of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of UI module to find</typeparam>
        /// <returns>The UI module if found, null otherwise</returns>
        public T GetModule<T>() where T : UIModuleBase
        {
            return _moduleManager.GetModule<T>();
        }
        
        /// <summary>
        /// Show all registered UI modules
        /// </summary>
        public void ShowAllModules()
        {
            _moduleManager.ShowAllModules();
        }
        
        /// <summary>
        /// Hide all registered UI modules
        /// </summary>
        public void HideAllModules()
        {
            _moduleManager.HideAllModules();
        }
        #endregion
        
        #region Public Panel Management Methods
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
            
            RectTransform parent = _panelManager.GetPanelForPosition(position);
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
                        _panelManager.ConfigureHorizontalLayout(parent);
                        break;
                        
                    case UIPosition.Left:
                    case UIPosition.Right:
                        _panelManager.ConfigureVerticalLayout(parent);
                        break;
                        
                    case UIPosition.Center:
                        _panelManager.ConfigureGridLayout(parent);
                        break;
                }
            }
            else
            {
                Debug.LogWarning($"GameObject {obj.name} doesn't have a RectTransform component");
            }
        }
        
        /// <summary>
        /// Resize a panel to the specified size
        /// </summary>
        /// <param name="position">The panel to resize</param>
        /// <param name="size">The new size (for side panels: width, for top/bottom: height)</param>
        public void ResizePanel(UIPosition position, float size)
        {
            _panelManager.ResizePanel(position, size);
        }
        
        /// <summary>
        /// Resize the center panel with specific width and height
        /// </summary>
        /// <param name="width">The new width</param>
        /// <param name="height">The new height</param>
        public void ResizeCenterPanel(float width, float height)
        {
            _panelManager.ResizeCenterPanel(width, height);
        }
        #endregion
        
        #region Debug and Testing Methods
        #if UNITY_EDITOR
        /// <summary>
        /// Reset the entire UI layout
        /// </summary>
        public void ResetUILayout()
        {
            // Get all panel references
            var (top, bottom, left, right, center) = _panelManager.GetAllPanels();
            
            // Helper method to destroy panel contents
            void DestroyPanelContents(RectTransform panel)
            {
                if (panel == null) return;
                
                for (int i = panel.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(panel.GetChild(i).gameObject);
                }
                
                // Remove layout components
                var layout = panel.GetComponent<UnityEngine.UI.LayoutGroup>();
                if (layout != null)
                {
                    DestroyImmediate(layout);
                }
            }
            
            // Destroy all panel children
            DestroyPanelContents(top);
            DestroyPanelContents(bottom);
            DestroyPanelContents(left);
            DestroyPanelContents(right);
            DestroyPanelContents(center);
            
            // Clear module lists
            _moduleManager.ClearModules();
            
            // Re-initialize
            _initialized = false;
            Initialize();
        }
        #endif
        #endregion
    }
}