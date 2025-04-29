using UnityEngine;
using Entities;
using Systems;
using Managers;
using UI;

namespace Controllers
{
    /// <summary>
    /// CLASS PURPOSE:
    /// RegionController handles the business logic for regions, following the MVC pattern.
    /// It bridges between data entities (RegionEntity) and visual components (RegionView).
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Fetch data from EconomicSystem
    /// - Process event subscriptions
    /// - Update the view when data changes
    /// - Handle region selection and deselection logic
    /// </summary>
    public class RegionController
    {
        // References
        private readonly RegionView view;
        private RegionEntity regionEntity;
        
        // Cache for systems and managers
        private EconomicSystem economicSystem;
        private NationManager nationManager;
        
        public RegionController(RegionView view)
        {
            this.view = view;
            
            // Find references
            economicSystem = Object.FindFirstObjectByType<EconomicSystem>();
            nationManager = NationManager.Instance;
            
            // Subscribe to events
            EventBus.Subscribe("RegionUpdated", OnRegionUpdated);
            EventBus.Subscribe("EconomicTick", OnEconomicTick);
            EventBus.Subscribe("RegionNationChanged", OnRegionNationChanged);
            EventBus.Subscribe("RegionSelected", OnRegionSelected);
            
            // Initialize with data
            TryGetRegionEntityFromSystem();
        }
        
        /// <summary>
        /// Clean up event subscriptions
        /// </summary>
        public void Dispose()
        {
            EventBus.Unsubscribe("RegionUpdated", OnRegionUpdated);
            EventBus.Unsubscribe("EconomicTick", OnEconomicTick);
            EventBus.Unsubscribe("RegionNationChanged", OnRegionNationChanged);
            EventBus.Unsubscribe("RegionSelected", OnRegionSelected);
        }
        
        /// <summary>
        /// Try to find the region entity in the economic system
        /// </summary>
        private void TryGetRegionEntityFromSystem()
        {
            if (economicSystem != null && !string.IsNullOrEmpty(view.RegionName))
            {
                RegionEntity existingEntity = economicSystem.GetRegion(view.RegionName);
                if (existingEntity != null)
                {
                    regionEntity = existingEntity;
                    UpdateViewWithEntityData();
                }
            }
        }
        
        /// <summary>
        /// Update the view with current entity data
        /// </summary>
        private void UpdateViewWithEntityData()
        {
            if (regionEntity == null) return;
            
            // Update economic display
            view.UpdateEconomicDisplay(regionEntity.Wealth, regionEntity.Production);
            
            // Update nation information if available
            if (!string.IsNullOrEmpty(regionEntity.NationId) && nationManager != null)
            {
                NationEntity nation = nationManager.GetNation(regionEntity.NationId);
                if (nation != null)
                {
                    view.UpdateNationInfo(nation.Name);
                }
            }
        }
        
        /// <summary>
        /// Set a new region entity
        /// </summary>
        public void SetRegionEntity(RegionEntity entity)
        {
            if (entity == null) return;
            
            regionEntity = entity;
            UpdateViewWithEntityData();
        }
        
        #region Event Handlers
        
        /// <summary>
        /// Handle region updated event
        /// </summary>
        private void OnRegionUpdated(object data)
        {
            if (data is RegionEntity entity && entity.Id == view.RegionName)
            {
                regionEntity = entity;
                UpdateViewWithEntityData();
            }
        }
        
        /// <summary>
        /// Handle economic tick event
        /// </summary>
        private void OnEconomicTick(object data)
        {
            // Refresh entity data from system
            TryGetRegionEntityFromSystem();
        }
        
        /// <summary>
        /// Handle region nation changed event
        /// </summary>
        private void OnRegionNationChanged(object data)
        {
            if (data is RegionNationChangedData changeData && changeData.RegionId == view.RegionName)
            {
                // Refresh entity data
                TryGetRegionEntityFromSystem();
                
                // Update nation info if available
                if (nationManager != null && !string.IsNullOrEmpty(changeData.NationId))
                {
                    NationEntity nation = nationManager.GetNation(changeData.NationId);
                    if (nation != null)
                    {
                        view.UpdateNationInfo(nation.Name);
                    }
                }
            }
        }
        
        /// <summary>
        /// Handle region selected event
        /// </summary>
        private void OnRegionSelected(object data)
        {
            if (data is string regionId)
            {
                if (regionId == view.RegionName)
                {
                    // This region was selected
                    view.SetHighlighted(true);
                    
                    // Log debug info
                    if (regionEntity != null)
                    {
                        Debug.Log($"Region {view.RegionName}: {regionEntity.GetSummary()}");
                    }
                }
                else
                {
                    // Another region was selected, deselect this one
                    view.SetHighlighted(false);
                }
            }
        }
        
        #endregion
    }
}