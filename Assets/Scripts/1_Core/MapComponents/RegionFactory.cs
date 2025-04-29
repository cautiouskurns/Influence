using UnityEngine;
using UI;
using Entities;
using Systems;
using Controllers;
using Managers;

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
            
            // Create a new region entity with random initial values
            int initialWealth = Random.Range(100, 300);
            int initialProduction = Random.Range(50, 100);
            RegionEntity regionEntity = new RegionEntity(regionId, initialWealth, initialProduction);
            
            // Set additional properties
            regionEntity.LaborAvailable = Random.Range(50, 150);
            regionEntity.InfrastructureLevel = Random.Range(1, 5);
            
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
    }
}