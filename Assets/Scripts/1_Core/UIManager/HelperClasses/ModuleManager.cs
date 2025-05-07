using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI.UIManager.HelperClasses
{
    /// <summary>
    /// Helper class that handles the registration and management of UI modules
    /// </summary>
    public class ModuleManager
    {
        // List of all registered UI modules
        private List<UIModuleBase> _uiModules = new List<UIModuleBase>();
        
        // Dictionary for fast lookup of modules by type
        private Dictionary<Type, UIModuleBase> _moduleCache = new Dictionary<Type, UIModuleBase>();
        
        // Event to notify when a module is registered
        public event Action<UIModuleBase> OnModuleRegistered;
        
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
        /// <param name="panelManager">Reference to the panel manager to get panel references</param>
        /// <returns>The created UI module instance</returns>
        public T CreateModule<T>(UIPosition position, PanelManager panelManager) where T : UIModuleBase
        {
            RectTransform parent = panelManager.GetPanelForPosition(position);
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
        
        /// <summary>
        /// Initialize all registered modules
        /// </summary>
        public void InitializeModules()
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
        /// Discover UI modules already present in the scene
        /// </summary>
        public void DiscoverUIModules()
        {
            // Find all UI modules in the scene
            UIModuleBase[] modules = GameObject.FindObjectsByType<UIModuleBase>(FindObjectsSortMode.None);
            
            foreach (UIModuleBase module in modules)
            {
                if (module != null)
                {
                    RegisterModule(module);
                }
            }
        }
        
        /// <summary>
        /// Clear all registered modules
        /// </summary>
        public void ClearModules()
        {
            _uiModules.Clear();
            _moduleCache.Clear();
        }
    }
}