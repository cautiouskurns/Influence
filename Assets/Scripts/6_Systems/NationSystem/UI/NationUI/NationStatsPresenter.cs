using UnityEngine;
using Entities;
using UI;
using Core;
using Managers;
using System.Collections.Generic;

/// <summary>
/// Presenter component that handles nation selection and coordinates updates for the nation stats UI.
/// This class acts as the bridge between game systems and the nation stats UI.
/// </summary>
/// 
namespace Systems.UI
{
    public class NationStatsPresenter : MonoBehaviour
    {
        // Singleton implementation
        private static NationStatsPresenter _instance;
        public static NationStatsPresenter Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<NationStatsPresenter>();
                    
                    if (_instance == null)
                    {
                        GameObject presenterObj = new GameObject("NationStatsPresenter");
                        _instance = presenterObj.AddComponent<NationStatsPresenter>();
                        Debug.Log("Created new NationStatsPresenter instance");
                    }
                }
                
                return _instance;
            }
        }
        
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        private NationStatsViewModel viewModel;
        
        // Nation selection state
        private NationEntity currentSelectedNation;
        
        private void Awake()
        {
            // Ensure singleton behavior
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("Duplicate NationStatsPresenter found. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            Debug.Log("NationStatsPresenter Awake");
        }
        
        private void Start()
        {
            // Find references if not set
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
            }
            
            // Get the view model
            viewModel = NationStatsViewModel.Instance;
            if (viewModel == null)
            {
                Debug.LogError("Failed to find NationStatsViewModel instance!");
            }
            else
            {
                Debug.Log("NationStatsPresenter found ViewModel successfully");
            }
            
            // Display initial nation if set
            if (currentSelectedNation != null)
            {
                Debug.Log($"Displaying initial nation: {currentSelectedNation.Name}");
                DisplayNationStats(currentSelectedNation);
            }
            
            // Subscribe to turn change events
            EventBus.Subscribe("EconomicTick", OnTurnChanged);
            Debug.Log("NationStatsPresenter subscribed to EconomicTick events");
        }
        
        private void OnDestroy()
        {
            // Unsubscribe when destroyed
            EventBus.Unsubscribe("EconomicTick", OnTurnChanged);
            Debug.Log("NationStatsPresenter unsubscribed from EconomicTick events");
        }
        
        /// <summary>
        /// Handle turn changes by refreshing the displayed nation stats
        /// </summary>
        private void OnTurnChanged(object data)
        {
            Debug.Log($"NationStatsPresenter: OnTurnChanged event received with data: {data}");
            
            // Refresh the currently displayed nation stats if there is one
            if (currentSelectedNation != null && viewModel != null)
            {
                Debug.Log($"Refreshing stats for nation: {currentSelectedNation.Name}");
                
                // Check if we need to refresh the nation's economy data based on its regions
                RefreshNationEconomyFromRegions();
                
                // Log debug info
                LogEconomyDebugInfo("BEFORE refresh", currentSelectedNation);
                
                // Refresh the display
                viewModel.RefreshDisplay();
                
                // Log debug info again after refresh
                LogEconomyDebugInfo("AFTER refresh", currentSelectedNation);
            }
            else
            {
                Debug.LogWarning("Cannot refresh nation stats: " + 
                    (currentSelectedNation == null ? "No nation selected" : "ViewModel is null"));
            }
        }
        
        /// <summary>
        /// Refresh nation economy data by aggregating from its regions
        /// </summary>
        private void RefreshNationEconomyFromRegions()
        {
            if (currentSelectedNation == null || currentSelectedNation.Economy == null)
                return;

            // Get all regions belonging to this nation
            List<RegionEntity> nationRegions = GetNationRegions();
            if (nationRegions == null || nationRegions.Count == 0) 
            {
                Debug.LogWarning($"No regions found for nation: {currentSelectedNation.Name}");
                return;
            }

            // Aggregate economy values from all regions
            float totalWealth = 0f;
            float totalProduction = 0f;
            
            // Sum up values from all regions
            foreach (var region in nationRegions)
            {
                // Add region wealth and production to nation totals
                totalWealth += region.Wealth;
                totalProduction += region.Production;
            }
            
            // Update the nation's economy with aggregated values
            if (currentSelectedNation.Economy != null)
            {
                // Only update if values have changed
                if (totalWealth != currentSelectedNation.Economy.TotalWealth)
                {
                    float oldWealth = currentSelectedNation.Economy.TotalWealth;
                    currentSelectedNation.Economy.SetTotalWealth(totalWealth);
                    Debug.Log($"Updated {currentSelectedNation.Name} TotalWealth: {oldWealth} -> {totalWealth}");
                }
                
                if (totalProduction != currentSelectedNation.Economy.TotalProduction)
                {
                    float oldProduction = currentSelectedNation.Economy.TotalProduction;
                    currentSelectedNation.Economy.SetTotalProduction(totalProduction);
                    Debug.Log($"Updated {currentSelectedNation.Name} TotalProduction: {oldProduction} -> {totalProduction}");
                }
            }
        }
        
        /// <summary>
        /// Get all regions belonging to the current nation
        /// </summary>
        private List<RegionEntity> GetNationRegions()
        {
            if (currentSelectedNation == null)
                return null;
                
            // Try to get region entities via Economic System
            EconomicSystem economicSystem = FindFirstObjectByType<EconomicSystem>();
            if (economicSystem != null)
            {
                return economicSystem.GetRegionsForNation(currentSelectedNation.Id);
            }
            
            // Fallback method
            List<RegionEntity> regions = new List<RegionEntity>();
            List<string> regionIds = currentSelectedNation.GetRegionIds();
            
            if (regionIds != null && regionIds.Count > 0)
            {
                foreach (var regionId in regionIds)
                {
                    RegionEntity region = economicSystem?.GetRegion(regionId);
                    if (region != null)
                    {
                        regions.Add(region);
                    }
                }
            }
            
            return regions;
        }
        
        /// <summary>
        /// Test method to manually trigger a refresh of the displayed nation stats
        /// For debugging when we suspect the event system is not working
        /// </summary>
        public void TestRefresh()
        {
            Debug.Log("TestRefresh called - manually triggering nation stats refresh");
            
            if (currentSelectedNation != null && viewModel != null)
            {
                Debug.Log($"Manually refreshing stats for nation: {currentSelectedNation.Name}");
                
                // Modify economy data using the proper method
                if (currentSelectedNation.Economy != null)
                {
                    // Use the provided simulation method to modify economic values
                    currentSelectedNation.Economy.SimulateEconomicChanges();
                    Debug.Log($"Simulated economic changes for {currentSelectedNation.Name}");
                    
                    // Log the updated values
                    LogEconomyDebugInfo("After simulation", currentSelectedNation);
                }
                
                // Refresh the display
                viewModel.RefreshDisplay();
            }
            else
            {
                Debug.LogError($"TestRefresh failed: Selected Nation={currentSelectedNation != null}, ViewModel={viewModel != null}");
            }
        }
        
        /// <summary>
        /// Log debug information about nation economy values
        /// </summary>
        private void LogEconomyDebugInfo(string stage, NationEntity nation)
        {
            if (nation?.Economy != null)
            {
                Debug.Log($"{stage}: Nation {nation.Name} - " +
                        $"Treasury: {nation.Economy.TreasuryBalance}, " +
                        $"Wealth: {nation.Economy.TotalWealth}, " +
                        $"Production: {nation.Economy.TotalProduction}");
            }
        }
        
        /// <summary>
        /// Display nation stats - can be called directly from UI buttons or events
        /// </summary>
        public void DisplayNationStats(NationEntity nation)
        {
            if (nation != null && viewModel != null)
            {
                Debug.Log($"DisplayNationStats called for nation: {nation.Name}");
                currentSelectedNation = nation;
                
                // Refresh economy data from regions before displaying
                RefreshNationEconomyFromRegions();
                
                viewModel.DisplayNation(nation);
            }
            else
            {
                Debug.LogWarning("Cannot display nation stats: " + 
                    (nation == null ? "Nation is null" : "ViewModel is null"));
            }
        }
        
        /// <summary>
        /// Set the currently selected nation
        /// </summary>
        public void SetSelectedNation(NationEntity nation)
        {
            Debug.Log($"SetSelectedNation called with nation: {(nation != null ? nation.Name : "null")}");
            currentSelectedNation = nation;
            DisplayNationStats(nation);
        }
        
        /// <summary>
        /// Get the currently selected nation
        /// </summary>
        public NationEntity GetSelectedNation()
        {
            return currentSelectedNation;
        }
        
        /// <summary>
        /// Refresh the current display without changing the selected nation
        /// </summary>
        public void RefreshDisplay()
        {
            Debug.Log("RefreshDisplay called on NationStatsPresenter");
            if (viewModel != null && currentSelectedNation != null)
            {
                // First refresh the nation economy from its regions
                RefreshNationEconomyFromRegions();
                
                // Then update the UI display
                viewModel.RefreshDisplay();
                Debug.Log($"Nation stats refreshed for: {currentSelectedNation.Name}");
            }
            else
            {
                Debug.LogWarning($"Cannot refresh display: {(viewModel == null ? "ViewModel is null" : "No nation selected")}");
            }
        }
    }
}