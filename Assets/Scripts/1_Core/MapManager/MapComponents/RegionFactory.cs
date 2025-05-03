using UnityEngine;
using UI;
using Entities;
using Systems;
using Controllers;
using Managers;
using Entities.ScriptableObjects;
using Entities.Components; // Added this namespace for PopulationComponent

namespace UI.MapComponents
{
    /// <summary>
    /// CLASS PURPOSE:
    /// Create and initialize region GameObjects and their associated data entities.
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Instantiate region GameObjects with proper configuration
    /// - Create economic entities for regions
    /// - Register regions with appropriate systems
    /// </summary>
    public class RegionFactory
    {
        // Dependencies
        private readonly GameObject regionPrefab;
        private readonly Transform regionsContainer;
        private readonly EconomicSystem economicSystem;
        private readonly RegionControllerManager controllerManager;
        private RegionConfig defaultRegionConfig;
        
        /// <summary>
        /// Constructor with required dependencies
        /// </summary>
        public RegionFactory(
            GameObject regionPrefab,
            Transform regionsContainer,
            EconomicSystem economicSystem,
            RegionControllerManager controllerManager)
        {
            this.regionPrefab = regionPrefab;
            this.regionsContainer = regionsContainer;
            this.economicSystem = economicSystem;
            this.controllerManager = controllerManager;
            
            // Try to load a default region config from Resources
            defaultRegionConfig = Resources.Load<RegionConfig>("DefaultConfigs/DefaultRegionConfig");
            
            // If no default config is found, debug a warning
            if (defaultRegionConfig == null)
            {
                Debug.LogWarning("No default RegionConfig found in Resources/DefaultConfigs/. Using fallback values.");
            }
        }
        
        /// <summary>
        /// Create a region GameObject with proper initialization
        /// </summary>
        public RegionView CreateRegion(string id, string name, Vector3 position, Quaternion rotation, Color color)
        {
            // Instantiate the region prefab
            GameObject regionObj = Object.Instantiate(regionPrefab, position, rotation, regionsContainer);
            
            // Get the RegionView component
            RegionView regionView = regionObj.GetComponent<RegionView>();
            
            // Initialize with data
            regionView.Initialize(id, name, color);
            
            return regionView;
        }
        
        /// <summary>
        /// Create an economic entity for a region and register it with systems
        /// </summary>
        public RegionEntity CreateRegionEntity(string regionId)
        {
            if (economicSystem == null) return null;
            
            // Only create if the region doesn't already have an entity
            RegionEntity existingEntity = economicSystem.GetRegion(regionId);
            if (existingEntity != null) return existingEntity;
            
            RegionEntity regionEntity;
            
            // If we have a default config, use it
            if (defaultRegionConfig != null)
            {
                regionEntity = RegionEntity.CreateFromConfig(regionId, regionId, defaultRegionConfig);
            }
            else
            {
                // Fallback to direct constructor with random values
                int initialWealth = Random.Range(100, 300);
                int initialProduction = Random.Range(50, 100);
                regionEntity = new RegionEntity(regionId, regionId, initialWealth, initialProduction);
            }
            
            // Register with the economic system
            economicSystem.RegisterRegion(regionEntity);
            
            return regionEntity;
        }
        
        /// <summary>
        /// Create an economic entity for a region and register it with systems
        /// </summary>
        public RegionEntity CreateRegionEntityWithConfig(string regionId, RegionConfig config)
        {
            if (economicSystem == null) return null;
            
            // Only create if the region doesn't already have an entity
            RegionEntity existingEntity = economicSystem.GetRegion(regionId);
            if (existingEntity != null) return existingEntity;
            
            // Create entity from config
            RegionEntity regionEntity = RegionEntity.CreateFromConfig(regionId, regionId, config);
            
            // Register with the economic system
            economicSystem.RegisterRegion(regionEntity);
            
            return regionEntity;
        }
        
        /// <summary>
        /// Create both a region view and its entity, and connect them via controller
        /// </summary>
        public RegionView CreateRegionWithEntity(string id, string name, Vector3 position, Quaternion rotation, Color color)
        {
            // Create the view
            RegionView view = CreateRegion(id, name, position, rotation, color);
            
            // Create the entity
            CreateRegionEntity(id);
            
            // Register with controller manager if available
            if (controllerManager != null)
            {
                controllerManager.RegisterRegionView(view);
            }
            
            return view;
        }
        
        /// <summary>
        /// Create both a region view and its entity with specific configuration
        /// </summary>
        public RegionView CreateRegionWithConfig(string id, string name, Vector3 position, Quaternion rotation, Color color, RegionConfig config)
        {
            // Use the region's terrain color from the config instead of the passed parameter if available
            Color regionColor = config != null ? config.TerrainColor : color;
            
            // Create the view
            RegionView view = CreateRegion(id, name, position, rotation, regionColor);
            
            // Create the entity with config
            CreateRegionEntityWithConfig(id, config);
            
            // Register with controller manager if available
            if (controllerManager != null)
            {
                controllerManager.RegisterRegionView(view);
            }
            
            return view;
        }

        /// <summary>
        /// Create a region view and entity with specific custom properties from scenario
        /// </summary>
        public RegionView CreateCustomRegion(string id, string name, Vector3 position, Quaternion rotation, 
                                           Color color, int wealth, int production, 
                                           int population = 100, float satisfaction = 0.5f, float infrastructure = 1f)
        {
            // Create the view
            RegionView view = CreateRegion(id, name, position, rotation, color);
            
            // Create custom entity with specific properties instead of random or config values
            if (economicSystem != null)
            {
                // Only create if the region doesn't already have an entity
                RegionEntity existingEntity = economicSystem.GetRegion(id);
                if (existingEntity == null)
                {
                    // Create entity with custom properties
                    RegionEntity regionEntity = new RegionEntity(id, name, wealth, production);
                    
                    // Set additional properties if available
                    // Use Invest() method to set infrastructure level instead of direct assignment
                    if (infrastructure > 1f) {
                        float currentLevel = regionEntity.Infrastructure.Level;
                        float levelDifference = infrastructure - currentLevel;
                        if (levelDifference > 0) {
                            // Use a high enough investment amount to reach the desired level
                            // The exact formula will depend on the Invest() implementation
                            regionEntity.Infrastructure.Invest(levelDifference * 100);
                        }
                    }
                    
                    // For population, we need to create a new PopulationComponent with the desired initial population
                    // since there's no direct setter method for Population
                    if (population > 0) {
                        // Preserve the existing labor available ratio when recreating the component
                        float laborRatio = regionEntity.PopulationComp.LaborAvailable / 
                                        (regionEntity.PopulationComp.Population > 0 ? 
                                         regionEntity.PopulationComp.Population : 1);
                        float newLaborAvailable = population * laborRatio;
                        
                        // Create new component with desired population
                        PopulationComponent newPopComp = new PopulationComponent(population, newLaborAvailable);
                        
                        // Replace the existing component (assuming this is allowed in the architecture)
                        // If not possible, we may need to use reflection or another approach
                        typeof(RegionEntity)
                            .GetProperty("PopulationComp")
                            .SetValue(regionEntity, newPopComp);
                    }
                    
                    // For satisfaction, use UpdateNeedSatisfaction to set satisfaction values
                    // Set a general "Overall" need with the desired satisfaction
                    regionEntity.PopulationComp.UpdateNeedSatisfaction("Overall", satisfaction);
                    
                    // Register with the economic system
                    economicSystem.RegisterRegion(regionEntity);
                }
            }
            
            // Register with controller manager if available
            if (controllerManager != null)
            {
                controllerManager.RegisterRegionView(view);
            }
            
            return view;
        }
    }
}