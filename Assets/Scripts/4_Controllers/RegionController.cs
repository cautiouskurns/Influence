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
            
            // Update economic display - ensure production colors are updated every tick
            // This ensures the color logic in RegionView is consistently applied
            int production = regionEntity.Production;
            int wealth = regionEntity.Wealth;
            
            // Forcing the values to be re-applied during every update
            view.UpdateEconomicDisplay(wealth, production);
            
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
            
            // Ensure view is updated with latest data after each tick
            if (regionEntity != null)
            {
                UpdateViewWithEntityData();
            }
        }
        
        /// <summary>
        /// Handle region nation changed event
        /// </summary>
        private void OnRegionNationChanged(object data)
        {
            if (data is RegionNationChangedData changeData && changeData.RegionId == view.RegionName)
            {
                Debug.Log($"Region {view.RegionName} nation changed to {changeData.NationId}");
                
                // Update the entity's NationId property directly
                if (regionEntity != null)
                {
                    regionEntity.NationId = changeData.NationId;
                    
                    // Force an update of the view with the new nation
                    UpdateViewWithEntityData();
                }
                else
                {
                    // If regionEntity is null, try to get it from the system
                    TryGetRegionEntityFromSystem();
                    
                    // If we now have an entity, update its NationId
                    if (regionEntity != null)
                    {
                        regionEntity.NationId = changeData.NationId;
                        UpdateViewWithEntityData();
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
                    
                    // Print detailed region information to console
                    if (regionEntity != null)
                    {
                        PrintRegionDetails();
                    }
                }
                else
                {
                    // Another region was selected, deselect this one
                    view.SetHighlighted(false);
                }
            }
        }
        
        /// <summary>
        /// Print detailed region information to the console
        /// </summary>
        private void PrintRegionDetails()
        {
            if (regionEntity == null) return;
            
            // Create a formatted string with all the important region details
            string details = 
                $"=== REGION DETAILS: {regionEntity.Name} ===\n" +
                $"ID: {regionEntity.Id}\n" +
                $"Nation: {(string.IsNullOrEmpty(regionEntity.NationId) ? "Independent" : regionEntity.GetNation()?.Name ?? "Unknown")}\n\n" +
                
                $"--- ECONOMY ---\n" +
                $"Wealth: {regionEntity.Wealth}\n" +
                $"Production: {regionEntity.Production}\n\n" +
                
                $"--- POPULATION ---\n" +
                $"Population: {regionEntity.Population}\n" +
                $"Labor Available: {regionEntity.LaborAvailable}\n\n" +
                
                $"--- INFRASTRUCTURE ---\n" +
                $"Level: {regionEntity.InfrastructureLevel}\n" +
                $"Quality: {regionEntity.InfrastructureQuality:F2}\n\n" +
                
                $"--- RESOURCES ---\n";
            
            // Add resources if available
            if (regionEntity.Resources != null)
            {
                // Get common resource types and add them to the details
                string[] resourceTypes = { "Food", "Materials", "Fuel" };
                foreach (string resourceType in resourceTypes)
                {
                    float amount = regionEntity.Resources.GetResourceAmount(resourceType);
                    float rate = regionEntity.Resources.GetProductionRate(resourceType);
                    details += $"{resourceType}: {amount:F1} (Producing: {rate:F1}/turn)\n";
                }
            }
            
            // Print to console with a distinctive format
            Debug.Log(details);
        }
        
        #endregion
    }
}