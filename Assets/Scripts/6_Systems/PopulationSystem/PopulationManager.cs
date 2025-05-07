using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Add this for ToList() extension method
using Core.Interfaces;
using Entities;
using Entities.Components;
using Core;

namespace Managers
{
    /// <summary>
    /// CLASS PURPOSE: 
    /// Manages global population statistics and growth policies across all regions
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Track total population across all regions
    /// - Apply population growth and migration
    /// - Provide centralized growth/migration rates and policies
    /// - Delegate region-specific population management to PopulationComponent
    /// </summary>
    public class PopulationManager : MonoBehaviour, IPopulationManager
    {
        #region Singleton
        private static PopulationManager _instance;
        
        public static PopulationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<PopulationManager>();
                    
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("PopulationManager");
                        _instance = go.AddComponent<PopulationManager>();
                    }
                }
                
                return _instance;
            }
        }
        #endregion
        
        [Header("Dependencies")]
        [SerializeField] private GameSettings gameSettings;
        
        [Header("Statistics")]
        [SerializeField] private int totalPopulation;
        [SerializeField] private float averageGrowthRate;
        [SerializeField] private float averageSatisfaction;
        
        // Reference to the economic system for region access
        private IEconomicSystem _economicSystem;
        
        // Cached region data to avoid recalculation
        private Dictionary<string, float> _previousPopulations = new Dictionary<string, float>();
        
        private void Awake()
        {
            // Singleton pattern setup
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("More than one PopulationManager instance found. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Load default settings if none provided
            if (gameSettings == null)
            {
                gameSettings = Resources.Load<GameSettings>("DefaultGameSettings");
                if (gameSettings == null)
                {
                    Debug.LogError("No GameSettings found! Population systems will use default values.");
                }
            }
        }
        
        private void Start()
        {
            // Find the economic system
            var economicSystem = FindFirstObjectByType<Systems.EconomicSystem>();
            if (economicSystem != null)
            {
                _economicSystem = economicSystem as IEconomicSystem;
                if (_economicSystem == null)
                {
                    Debug.LogError("EconomicSystem does not implement IEconomicSystem interface!");
                }
            }
            
            // Subscribe to events
            EventBus.Subscribe("EconomicTick", OnEconomicTick);
            
            // Initialize population cache
            CalculateTotalPopulation(true);
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            EventBus.Unsubscribe("EconomicTick", OnEconomicTick);
        }
        
        /// <summary>
        /// Handler for economic tick events
        /// </summary>
        private void OnEconomicTick(object data)
        {
            // Apply population changes
            ApplyPopulationGrowth();
            
            // Recalculate total population
            CalculateTotalPopulation();
            
            // Trigger population changed event
            EventBus.Trigger("PopulationChanged", totalPopulation);
        }
        
        /// <summary>
        /// Calculate the total population from all regions, with option to rebuild cache
        /// </summary>
        public void CalculateTotalPopulation(bool rebuildCache = false)
        {
            totalPopulation = 0;
            float totalGrowthRate = 0;
            float totalSatisfaction = 0;
            int regionCount = 0;
            
            if (_economicSystem != null)
            {
                // Convert IEnumerable<string> to List<string> using ToList()
                var regionIds = _economicSystem.GetAllRegionIds().ToList();
                regionCount = regionIds.Count;
                
                foreach (string regionId in regionIds)
                {
                    var region = _economicSystem.GetRegion(regionId);
                    if (region != null && region.PopulationComp != null)
                    {
                        float laborAvailable = region.LaborAvailable;
                        totalPopulation += Mathf.RoundToInt(laborAvailable);
                        
                        // Track growth rates and satisfaction for averages
                        totalGrowthRate += region.PopulationComp.GrowthRate;
                        totalSatisfaction += region.PopulationComp.Satisfaction;
                        
                        // Update cache if rebuilding or if region not in cache
                        if (rebuildCache || !_previousPopulations.ContainsKey(regionId))
                        {
                            _previousPopulations[regionId] = laborAvailable;
                        }
                    }
                }
                
                // Calculate averages
                if (regionCount > 0)
                {
                    averageGrowthRate = totalGrowthRate / regionCount;
                    averageSatisfaction = totalSatisfaction / regionCount;
                }
            }
        }
        
        /// <summary>
        /// Apply population growth to all regions
        /// </summary>
        public void ApplyPopulationGrowth()
        {
            if (_economicSystem == null || gameSettings == null) return;
            
            // Use IEnumerable directly without converting to List
            foreach (string regionId in _economicSystem.GetAllRegionIds())
            {
                var region = _economicSystem.GetRegion(regionId);
                if (region != null && region.PopulationComp != null)
                {
                    // Get terrain area for density calculations if available
                    float? area = null;
                    // TerrainComp doesn't exist, so we'll skip this for now
                    // Could be added in a future version of the region entity
                    
                    // Get infrastructure level if available
                    float infrastructureLevel = 0;
                    if (region.Infrastructure != null) // Fixed: Changed from InfrastructureComp to Infrastructure
                    {
                        infrastructureLevel = region.InfrastructureLevel; // Use the existing property
                    }
                    
                    // Apply growth with all relevant factors
                    region.PopulationComp.ApplyGrowth(
                        gameSettings.populationGrowthRate,
                        infrastructureLevel,
                        gameSettings.infrastructureGrowthThreshold,
                        area,
                        gameSettings.maxPopulationDensity
                    );
                    
                    // Update the region in the economic system
                    _economicSystem.UpdateRegion(region);
                    
                    // Update cache
                    _previousPopulations[regionId] = region.LaborAvailable;
                }
            }
        }
        
        /// <summary>
        /// Get the current population growth rate from settings
        /// </summary>
        public float GetPopulationGrowthRate()
        {
            return gameSettings != null ? gameSettings.populationGrowthRate : 0.02f;
        }
        
        /// <summary>
        /// Get the current migration rate from settings
        /// </summary>
        public float GetMigrationRate()
        {
            return gameSettings != null ? gameSettings.migrationRate : 0.01f;
        }
        
        /// <summary>
        /// Get the total population across all regions
        /// </summary>
        public int GetTotalPopulation()
        {
            return totalPopulation;
        }
        
        /// <summary>
        /// Get average satisfaction level across all regions (0-1)
        /// </summary>
        public float GetAverageSatisfaction()
        {
            return averageSatisfaction;
        }
    }
}