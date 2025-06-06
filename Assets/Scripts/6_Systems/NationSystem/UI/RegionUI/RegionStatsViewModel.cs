using UnityEngine;
using Entities;
using System.Collections.Generic;
using Managers;

namespace Systems.UI
{
    /// <summary>
    /// View model component that prepares data for the region statistics UI.
    /// Contains expanded region statistics for display.
    /// </summary>
    public class RegionStatsViewModel : MonoBehaviour
    {
        // Singleton implementation
        private static RegionStatsViewModel _instance;
        public static RegionStatsViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<RegionStatsViewModel>();
                    
                    if (_instance == null)
                    {
                        // Create it in a Canvas
                        Canvas canvas = FindFirstObjectByType<Canvas>();
                        if (canvas == null)
                        {
                            // Create canvas if none exists
                            GameObject canvasObj = new GameObject("UICanvas");
                            canvas = canvasObj.AddComponent<Canvas>();
                            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                            Debug.Log("Created new Canvas for RegionStatsViewModel");
                        }
                        
                        GameObject viewModelObj = new GameObject("RegionStatsViewModel");
                        viewModelObj.transform.SetParent(canvas.transform, false);
                        
                        _instance = viewModelObj.AddComponent<RegionStatsViewModel>();
                        Debug.Log("Created new RegionStatsViewModel instance");
                    }
                }
                
                return _instance;
            }
        }
        
        [SerializeField] private RegionStatsUIView uiView;
        
        // Current region that's being displayed
        private RegionEntity currentRegion;
        
        // Data to be displayed in the UI, prepared by the view model
        public struct RegionDisplayData
        {
            public string regionName;
            public string wealth;
            public string production;
            public string population;
            public string infrastructure;
            public string resources;
            public string growth;
            public string nationName;
        }
        
        private void Awake()
        {
            // Ensure singleton behavior
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("Multiple RegionStatsViewModel instances found. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            Debug.Log("RegionStatsViewModel Awake");
            
            SetupUIView();
        }
        
        /// <summary>
        /// Set the region to display and prepare its data
        /// </summary>
        public void DisplayRegion(RegionEntity region)
        {
            Debug.Log($"DisplayRegion called with region: {(region != null ? region.Name : "null")}");
            
            if (uiView != null)
            {
                currentRegion = region;
                
                if (region != null)
                {
                    // Create display data and send to view
                    RegionDisplayData displayData = PrepareDisplayData(region);
                    uiView.DisplayRegion(displayData);
                    Debug.Log($"Region {region.Name} display data sent to UI view");
                }
                else
                {
                    // Hide the UI if no region
                    uiView.Hide();
                    Debug.Log("No region to display, UI view hidden");
                }
            }
            else
            {
                Debug.LogError("UI view not found in RegionStatsViewModel.DisplayRegion");
            }
        }
        
        /// <summary>
        /// Force refresh the currently displayed region
        /// </summary>
        public void RefreshDisplay()
        {
            Debug.Log($"RefreshDisplay called for current region: {(currentRegion != null ? currentRegion.Name : "null")}");
            
            if (uiView != null && currentRegion != null)
            {
                // Re-prepare the data, then update the view
                RegionDisplayData displayData = PrepareDisplayData(currentRegion);
                uiView.UpdateDisplay(displayData);
                
                Debug.Log($"Refreshed display for region: {currentRegion.Name} with Wealth: {displayData.wealth}");
            }
            else
            {
                Debug.LogWarning($"Cannot refresh display: {(uiView == null ? "UI view is null" : "No region selected")}");
            }
        }
        
        /// <summary>
        /// Process raw region data into display-ready values
        /// </summary>
        private RegionDisplayData PrepareDisplayData(RegionEntity region)
        {
            RegionDisplayData data = new RegionDisplayData();
            
            // Basic region info
            data.regionName = region.Name.ToUpper();
            
            // Format the economic values
            data.wealth = "W: " + FormatNumber(region.Wealth);
            data.production = "P: " + FormatNumber(region.Production);
            
            // Population data
            data.population = FormatNumber(region.Population) + " people";
            
            // Infrastructure data
            data.infrastructure = $"Level {FormatNumber(region.InfrastructureLevel)} ({(region.InfrastructureQuality * 100):F0}%)";
            
            // Resource data
            if (region.Resources != null)
            {
                data.resources = $"F:{FormatNumber(region.Resources.GetResourceAmount("Food"))}, " + 
                                $"M:{FormatNumber(region.Resources.GetResourceAmount("Materials"))}, " +
                                $"Fu:{FormatNumber(region.Resources.GetResourceAmount("Fuel"))}";
            }
            else
            {
                data.resources = "No resources";
            }
            
            // Growth rate from economy component
            if (region.Economy != null)
            {
                data.growth = $"{(region.Economy.GrowthRate * 100):F1}%";
            }
            else
            {
                data.growth = "0%";
            }
            
            // Nation ownership information
            if (!string.IsNullOrEmpty(region.NationId))
            {
                NationEntity nation = GetNationForRegion(region);
                data.nationName = nation != null ? nation.Name : "Unknown Nation";
            }
            else
            {
                data.nationName = "Independent";
            }
            
            return data;
        }
        
        // Get the nation entity that owns this region
        private NationEntity GetNationForRegion(RegionEntity region)
        {
            if (string.IsNullOrEmpty(region.NationId))
                return null;
                
            NationManager nationManager = NationManager.Instance;
            return nationManager?.GetNation(region.NationId);
        }
        
        // Helper formatting method
        private string FormatNumber(float value)
        {
            if (value >= 1000000)
                return (value / 1000000f).ToString("F2") + "M";
            else if (value >= 1000)
                return (value / 1000f).ToString("F1") + "K";
            else
                return value.ToString("F0");
        }
        
        /// <summary>
        /// Setup the UI view component
        /// </summary>
        public void SetupUIView()
        {
            // If the view reference is not set, try to find it
            if (uiView == null)
            {
                uiView = GetComponentInChildren<RegionStatsUIView>();
            }
            
            // If still not found, create it
            if (uiView == null)
            {
                GameObject viewObj = new GameObject("RegionStatsUIView");
                viewObj.transform.SetParent(transform, false);
                
                uiView = viewObj.AddComponent<RegionStatsUIView>();
                Debug.Log("Created new RegionStatsUIView component");
            }
            else
            {
                Debug.Log("Found existing RegionStatsUIView component");
            }
            
            // Initialize the view
            uiView.Initialize();
            Debug.Log("RegionStatsUIView initialized");
        }
    }
}