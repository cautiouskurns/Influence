using UnityEngine;
using Entities;
using System.Collections.Generic;

namespace Systems.UI
{
    /// <summary>
    /// View model component that prepares data for the nation statistics UI.
    /// This class acts as an intermediary between the presenter and the view.
    /// </summary>
    public class NationStatsViewModel : MonoBehaviour
    {
        // Singleton implementation
        private static NationStatsViewModel _instance;
        public static NationStatsViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<NationStatsViewModel>();
                    
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
                            Debug.Log("Created new Canvas for NationStatsViewModel");
                        }
                        
                        GameObject viewModelObj = new GameObject("NationStatsViewModel");
                        viewModelObj.transform.SetParent(canvas.transform, false);
                        
                        _instance = viewModelObj.AddComponent<NationStatsViewModel>();
                        Debug.Log("Created new NationStatsViewModel instance");
                    }
                }
                
                return _instance;
            }
        }
        
        [SerializeField] private NationStatsUIView uiView;
        
        // Current nation that's being displayed
        private NationEntity currentNation;
        
        // Keep track of nations we've already populated with data to avoid duplicate logs
        private HashSet<string> populatedNations = new HashSet<string>();
        
        // Data to be displayed in the UI, prepared by the view model
        public struct NationDisplayData
        {
            public string nationName;
            public Color nationColor;
            public string regionsCount;
            public string treasury;
            public string gdp;
            public string growthRate;
            public string totalWealth;
            public string production;
            public string stability;
            public string unrest;
            public string economicPolicy;
            public string diplomaticPolicy;
            public string militaryPolicy;
            public string socialPolicy;
        }
        
        private void Awake()
        {
            // Ensure singleton behavior
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("Multiple NationStatsViewModel instances found. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            Debug.Log("NationStatsViewModel Awake");
            
            SetupUIView();
        }
        
        /// <summary>
        /// Set the nation to display and prepare its data
        /// </summary>
        public void DisplayNation(NationEntity nation)
        {
            Debug.Log($"DisplayNation called with nation: {(nation != null ? nation.Name : "null")}");
            
            if (uiView != null)
            {
                currentNation = nation;
                
                // Make sure the nation has valid economy data for display
                if (nation != null)
                {
                    PrepareNationData(nation);
                    
                    // Create display data and send to view
                    NationDisplayData displayData = PrepareDisplayData(nation);
                    uiView.DisplayNation(displayData);
                    Debug.Log($"Nation {nation.Name} display data sent to UI view");
                }
                else
                {
                    // Hide the UI if no nation
                    uiView.Hide();
                    Debug.Log("No nation to display, UI view hidden");
                }
            }
            else
            {
                Debug.LogError("UI view not found in NationStatsViewModel.DisplayNation");
            }
        }
        
        /// <summary>
        /// Force refresh the currently displayed nation
        /// </summary>
        public void RefreshDisplay()
        {
            Debug.Log($"RefreshDisplay called for current nation: {(currentNation != null ? currentNation.Name : "null")}");
            
            if (uiView != null && currentNation != null)
            {
                // Re-prepare the data, then update the view
                NationDisplayData displayData = PrepareDisplayData(currentNation);
                uiView.UpdateDisplay(displayData);
                
                Debug.Log($"Refreshed display for nation: {currentNation.Name} with Treasury: {displayData.treasury}");
            }
            else
            {
                Debug.LogWarning($"Cannot refresh display: {(uiView == null ? "UI view is null" : "No nation selected")}");
            }
        }
        
        /// <summary>
        /// Process raw nation data into display-ready values
        /// </summary>
        private NationDisplayData PrepareDisplayData(NationEntity nation)
        {
            NationDisplayData data = new NationDisplayData();
            
            // Basic nation info
            data.nationName = nation.Name.ToUpper();
            data.nationColor = nation.Color;
            data.regionsCount = nation.GetRegionIds()?.Count.ToString() ?? "0";
            
            // Economy stats with null checks
            if (nation.Economy != null)
            {
                data.treasury = FormatNumber(nation.Economy.TreasuryBalance);
                data.gdp = FormatNumber(nation.Economy.GDP);
                data.growthRate = FormatPercent(nation.Economy.GDPGrowthRate);
                data.totalWealth = FormatNumber(nation.Economy.TotalWealth);
                data.production = FormatNumber(nation.Economy.TotalProduction);
                
                Debug.Log($"Prepared economic data for {nation.Name} - Treasury: {data.treasury}, GDP: {data.gdp}");
            }
            else
            {
                data.treasury = "-";
                data.gdp = "-";
                data.growthRate = "-";
                data.totalWealth = "-";
                data.production = "-";
                Debug.LogWarning($"Nation {nation.Name} has no Economy component");
            }
            
            // Stability stats with null check
            if (nation.Stability != null)
            {
                data.stability = FormatPercent(nation.Stability.Stability);
                data.unrest = FormatPercent(nation.Stability.UnrestLevel);
            }
            else
            {
                data.stability = "-";
                data.unrest = "-";
            }
            
            // Prepare policy values with error handling
            try { data.economicPolicy = FormatPolicyValue(nation.GetPolicy(NationEntity.PolicyType.Economic)); } 
            catch { data.economicPolicy = "N/A"; }
            
            try { data.diplomaticPolicy = FormatPolicyValue(nation.GetPolicy(NationEntity.PolicyType.Diplomatic)); } 
            catch { data.diplomaticPolicy = "N/A"; }
            
            try { data.militaryPolicy = FormatPolicyValue(nation.GetPolicy(NationEntity.PolicyType.Military)); } 
            catch { data.militaryPolicy = "N/A"; }
            
            try { data.socialPolicy = FormatPolicyValue(nation.GetPolicy(NationEntity.PolicyType.Social)); } 
            catch { data.socialPolicy = "N/A"; }
            
            return data;
        }
        
        /// <summary>
        /// Ensure nation has valid data for UI display
        /// </summary>
        private void PrepareNationData(NationEntity nation)
        {
            // Only prepare data if we haven't seen this nation before
            // or if this is running in editor (to allow for testing/refresh)
            if (!Application.isPlaying || !populatedNations.Contains(nation.Id))
            {
                bool firstTime = !populatedNations.Contains(nation.Id);
                
                // Calculate base values from region count - gives some variety
                float baseValue = 1000 + (nation.GetRegionIds().Count * 250);
                
                // Set initial economy values if they appear to be missing or invalid
                EnsureValidEconomyData(nation, baseValue, firstTime);
                
                // Ensure stability values are available
                EnsureValidStabilityData(nation, firstTime);
                
                // Mark this nation as populated
                if (firstTime)
                {
                    populatedNations.Add(nation.Id);
                }
            }
        }
        
        // Helper formatting methods
        private string FormatNumber(float value)
        {
            if (value >= 1000000)
                return (value / 1000000f).ToString("F2") + "M";
            else if (value >= 1000)
                return (value / 1000f).ToString("F1") + "K";
            else
                return value.ToString("F1");
        }
        
        private string FormatPercent(float value)
        {
            return (value * 100).ToString("F1") + "%";
        }
        
        private string FormatPolicyValue(float value)
        {
            if (value < 0.25f)
                return "Very Low";
            else if (value < 0.4f)
                return "Low";
            else if (value <= 0.6f)
                return "Balanced";
            else if (value <= 0.75f)
                return "High";
            else
                return "Very High";
        }
        
        /// <summary>
        /// Ensure nation has valid economy data
        /// </summary>
        private void EnsureValidEconomyData(NationEntity nation, float baseValue, bool logInfo)
        {
            if (nation.Economy == null)
            {
                Debug.LogWarning($"Nation {nation.Name} lacks Economy component");
                return;
            }
            
            // Check if values appear to be default/empty
            bool needsData = 
                nation.Economy.TotalWealth < 1 && 
                nation.Economy.TotalProduction < 1 && 
                nation.Economy.TreasuryBalance < 1;
                
            if (needsData || Application.isEditor)
            {
                if (logInfo) Debug.Log($"Setting economic values for nation: {nation.Name}");
                
                // Initialize economy values only if they appear to be missing
                if (nation.Economy.TotalWealth <= 0) 
                    SetSafely(nation.Economy, "TotalWealth", baseValue * 5f);
                    
                if (nation.Economy.TotalProduction <= 0)
                    SetSafely(nation.Economy, "TotalProduction", baseValue * 0.65f);
                    
                if (nation.Economy.TreasuryBalance <= 0)
                    SetSafely(nation.Economy, "TreasuryBalance", baseValue * 0.5f);
                    
                if (nation.Economy.GDP <= 0)
                    SetSafely(nation.Economy, "GDP", baseValue * 2.5f);
                    
                if (nation.Economy.GDPGrowthRate == 0)
                    SetSafely(nation.Economy, "GDPGrowthRate", 0.05f);
            }
        }
        
        /// <summary>
        /// Ensure nation has valid stability data
        /// </summary>
        private void EnsureValidStabilityData(NationEntity nation, bool logInfo)
        {
            if (nation.Stability == null)
            {
                Debug.LogWarning($"Nation {nation.Name} lacks Stability component");
                return;
            }
            
            // Initialize stability values only if they appear to be empty
            bool needsData = nation.Stability.Stability <= 0;
            
            if (needsData || Application.isEditor)
            {
                if (logInfo) Debug.Log($"Setting stability values for nation: {nation.Name}");
                
                if (nation.Stability.Stability <= 0)
                    SetSafely(nation.Stability, "Stability", 0.75f);
                    
                if (nation.Stability.UnrestLevel <= 0)
                    SetSafely(nation.Stability, "UnrestLevel", 0.15f);
            }
        }
        
        /// <summary>
        /// Safely set a property value using reflection with error handling 
        /// </summary>
        private void SetSafely(object component, string propertyName, object value)
        {
            try {
                var property = component.GetType().GetProperty(propertyName);
                if (property != null && property.CanWrite) {
                    property.SetValue(component, value);
                }
            }
            catch (System.Exception e) {
                Debug.LogError($"Failed to set {propertyName}: {e.Message}");
            }
        }
        
        /// <summary>
        /// Setup the UI view component
        /// </summary>
        public void SetupUIView()
        {
            // If the view reference is not set, try to find it
            if (uiView == null)
            {
                uiView = GetComponentInChildren<NationStatsUIView>();
            }
            
            // If still not found, create it
            if (uiView == null)
            {
                GameObject viewObj = new GameObject("NationStatsUIView");
                viewObj.transform.SetParent(transform, false);
                
                uiView = viewObj.AddComponent<NationStatsUIView>();
                Debug.Log("Created new NationStatsUIView component");
            }
            else
            {
                Debug.Log("Found existing NationStatsUIView component");
            }
            
            // Initialize the view
            uiView.Initialize();
            Debug.Log("NationStatsUIView initialized");
        }
    }
}
