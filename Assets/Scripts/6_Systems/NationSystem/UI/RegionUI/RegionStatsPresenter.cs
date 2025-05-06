using UnityEngine;
using Entities;
using Core;
using Managers;

namespace Systems.UI
{
    /// <summary>
    /// Presenter component that handles region selection and coordinates updates for the region stats UI.
    /// This is a simplified version that only shows region name and wealth.
    /// </summary>
    public class RegionStatsPresenter : MonoBehaviour
    {
        // Singleton implementation
        private static RegionStatsPresenter _instance;
        public static RegionStatsPresenter Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<RegionStatsPresenter>();
                    
                    if (_instance == null)
                    {
                        GameObject presenterObj = new GameObject("RegionStatsPresenter");
                        _instance = presenterObj.AddComponent<RegionStatsPresenter>();
                        Debug.Log("Created new RegionStatsPresenter instance");
                    }
                }
                
                return _instance;
            }
        }
        
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        private RegionStatsViewModel viewModel;
        
        // Region selection state
        private RegionEntity currentSelectedRegion;
        
        private void Awake()
        {
            // Ensure singleton behavior
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("Duplicate RegionStatsPresenter found. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            Debug.Log("RegionStatsPresenter Awake");
        }
        
        private void Start()
        {
            // Find references if not set
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
            }
            
            // Get the view model
            viewModel = RegionStatsViewModel.Instance;
            if (viewModel == null)
            {
                Debug.LogError("Failed to find RegionStatsViewModel instance!");
            }
            else
            {
                Debug.Log("RegionStatsPresenter found ViewModel successfully");
            }
            
            // Subscribe to turn change events to update the UI
            EventBus.Subscribe("EconomicTick", OnTurnChanged);
            Debug.Log("RegionStatsPresenter subscribed to EconomicTick events");
        }
        
        private void OnDestroy()
        {
            // Unsubscribe when destroyed
            EventBus.Unsubscribe("EconomicTick", OnTurnChanged);
            Debug.Log("RegionStatsPresenter unsubscribed from EconomicTick events");
        }
        
        /// <summary>
        /// Handle turn changes by refreshing the displayed region stats
        /// </summary>
        private void OnTurnChanged(object data)
        {
            Debug.Log($"RegionStatsPresenter: OnTurnChanged event received");
            
            // Refresh the currently displayed region stats if there is one
            if (currentSelectedRegion != null && viewModel != null)
            {
                Debug.Log($"Refreshing stats for region: {currentSelectedRegion.Name}");
                viewModel.RefreshDisplay();
            }
        }
        
        /// <summary>
        /// Force a refresh of the region display
        /// </summary>
        public void RefreshDisplay()
        {
            Debug.Log("RefreshDisplay called on RegionStatsPresenter");
            if (viewModel != null && currentSelectedRegion != null)
            {
                viewModel.RefreshDisplay();
                Debug.Log($"Region stats refreshed for: {currentSelectedRegion.Name}");
            }
            else
            {
                Debug.LogWarning($"Cannot refresh display: {(viewModel == null ? "ViewModel is null" : "No region selected")}");
            }
        }
        
        /// <summary>
        /// Display region stats - can be called from UI buttons or input events
        /// </summary>
        public void DisplayRegionStats(RegionEntity region)
        {
            if (region != null && viewModel != null)
            {
                Debug.Log($"DisplayRegionStats called for region: {region.Name}");
                currentSelectedRegion = region;
                viewModel.DisplayRegion(region);
            }
            else
            {
                Debug.LogWarning("Cannot display region stats: " + 
                    (region == null ? "Region is null" : "ViewModel is null"));
            }
        }
        
        /// <summary>
        /// Set the currently selected region
        /// </summary>
        public void SetSelectedRegion(RegionEntity region)
        {
            Debug.Log($"SetSelectedRegion called with region: {(region != null ? region.Name : "null")}");
            currentSelectedRegion = region;
            DisplayRegionStats(region);
        }
        
        /// <summary>
        /// Get the currently selected region
        /// </summary>
        public RegionEntity GetSelectedRegion()
        {
            return currentSelectedRegion;
        }
        
        /// <summary>
        /// Hide the region stats UI
        /// </summary>
        public void HideRegionStats()
        {
            if (viewModel != null)
            {
                viewModel.DisplayRegion(null);
                currentSelectedRegion = null;
            }
        }
    }
}