using UnityEngine;
using Entities;
using UI;
using Core;
using Managers;

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
    }
}