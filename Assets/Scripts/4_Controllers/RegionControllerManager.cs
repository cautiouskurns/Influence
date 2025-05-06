using UnityEngine;
using System.Collections.Generic;
using UI;
using Controllers;

namespace Managers
{
    /// <summary>
    /// CLASS PURPOSE:
    /// RegionControllerManager creates and manages RegionController instances
    /// for all RegionView components, ensuring proper separation of concerns.
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Create controllers for all RegionView instances
    /// - Manage controller lifecycle
    /// - Handle runtime registration of new RegionViews
    /// </summary>
    public class RegionControllerManager : MonoBehaviour
    {
        // Track controllers by region name
        private Dictionary<string, RegionController> controllers = new Dictionary<string, RegionController>();
        
        private void Awake()
        {
            // Ensure this component persists
            if (FindObjectsByType<RegionControllerManager>(FindObjectsSortMode.None).Length > 1)
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void Start()
        {
            // Find and create controllers for all RegionViews
            InitializeControllersForExistingViews();
        }
        
        private void OnDestroy()
        {
            // Clean up all controllers
            foreach (var controller in controllers.Values)
            {
                controller.Dispose();
            }
            controllers.Clear();
        }
        
        /// <summary>
        /// Initialize controllers for all existing RegionView components
        /// </summary>
        public void InitializeControllersForExistingViews()
        {
            // Find all RegionViews in the scene
            RegionView[] views = FindObjectsByType<RegionView>(FindObjectsSortMode.None);
                        
            // Create a controller for each view
            foreach (RegionView view in views)
            {
                if (!string.IsNullOrEmpty(view.RegionName))
                {
                    CreateControllerForView(view);
                }
                else
                {
                    Debug.LogWarning($"RegionControllerManager: RegionView has no RegionName, skipping controller creation");
                }
            }
        }
        
        /// <summary>
        /// Register a new RegionView (for runtime-created regions)
        /// </summary>
        public void RegisterRegionView(RegionView view)
        {
            if (view == null || string.IsNullOrEmpty(view.RegionName)) return;
            
            // Create a controller if one doesn't exist
            if (!controllers.ContainsKey(view.RegionName))
            {
                CreateControllerForView(view);
            }
        }
        
        /// <summary>
        /// Create a controller for a specific view
        /// </summary>
        private RegionController CreateControllerForView(RegionView view)
        {
            if (view == null) return null;
            
            string regionName = view.RegionName;
            
            // Skip if controller already exists
            if (controllers.TryGetValue(regionName, out RegionController existing))
            {
                return existing;
            }
            
            // Create a new controller
            RegionController controller = new RegionController(view);
            controllers[regionName] = controller;
            
            return controller;
        }
    }
}