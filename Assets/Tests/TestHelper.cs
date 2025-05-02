using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Entities;
using Entities.Components;
using Entities.ScriptableObjects;

namespace Tests
{
    /// <summary>
    /// Helper methods and utilities for testing
    /// </summary>
    public static class TestHelper
    {
        /// <summary>
        /// Create a mock RegionConfig for testing
        /// </summary>
        public static RegionConfig CreateMockRegionConfig()
        {
            RegionConfig config = ScriptableObject.CreateInstance<RegionConfig>();
            
            // Create mock config components
            config.resourceConfig = CreateMockResourceConfig();
            config.productionConfig = CreateMockProductionConfig();
            config.economyConfig = CreateMockEconomyConfig();
            config.populationConfig = CreateMockPopulationConfig();
            config.infrastructureConfig = CreateMockInfrastructureConfig();
            
            return config;
        }
        
        /// <summary>
        /// Create a mock ResourceConfig for testing
        /// </summary>
        private static ResourceComponentConfig CreateMockResourceConfig()
        {
            ResourceComponentConfig config = ScriptableObject.CreateInstance<ResourceComponentConfig>();
            
            // Set test values
            config.initialResources = new List<ResourceComponentConfig.ResourceData>
            {
                new ResourceComponentConfig.ResourceData { resourceType = "Food", initialAmount = 100, productionRate = 10 },
                new ResourceComponentConfig.ResourceData { resourceType = "Materials", initialAmount = 50, productionRate = 5 },
                new ResourceComponentConfig.ResourceData { resourceType = "Fuel", initialAmount = 30, productionRate = 3 }
            };
            
            return config;
        }
        
        /// <summary>
        /// Create a mock ProductionConfig for testing
        /// </summary>
        private static ProductionComponentConfig CreateMockProductionConfig()
        {
            ProductionComponentConfig config = ScriptableObject.CreateInstance<ProductionComponentConfig>();
            
            // Set test values
            config.baseProduction = 100;
            
            return config;
        }
        
        /// <summary>
        /// Create a mock EconomyConfig for testing
        /// </summary>
        private static EconomyComponentConfig CreateMockEconomyConfig()
        {
            EconomyComponentConfig config = ScriptableObject.CreateInstance<EconomyComponentConfig>();
            
            // Set test values
            config.initialGDP = 200;
            config.initialWealth = 500;
            
            return config;
        }
        
        /// <summary>
        /// Create a mock PopulationConfig for testing
        /// </summary>
        private static PopulationComponentConfig CreateMockPopulationConfig()
        {
            PopulationComponentConfig config = ScriptableObject.CreateInstance<PopulationComponentConfig>();
            
            // Set test values
            config.initialPopulation = 1000;
            config.initialLabor = 250;
            
            return config;
        }
        
        /// <summary>
        /// Create a mock InfrastructureConfig for testing
        /// </summary>
        private static InfrastructureComponentConfig CreateMockInfrastructureConfig()
        {
            InfrastructureComponentConfig config = ScriptableObject.CreateInstance<InfrastructureComponentConfig>();
            
            // Set test values
            config.initialLevel = 5.0f;
            config.initialQuality = 0.7f;
            
            return config;
        }
        
        /// <summary>
        /// Create a fully populated mock RegionEntity for testing
        /// </summary>
        public static RegionEntity CreateMockRegion(string id = "test_region", string name = "Test Region")
        {
            // Create region with mock config
            RegionEntity region = RegionEntity.CreateFromConfig(id, name, CreateMockRegionConfig());
            
            // Additional setup if needed
            region.NationId = "test_nation";
            
            return region;
        }
        
        /// <summary>
        /// Wait for a specified condition to be true with timeout
        /// </summary>
        public static IEnumerator WaitForCondition(System.Func<bool> condition, float timeoutSeconds = 5.0f)
        {
            float startTime = Time.time;
            while (!condition() && (Time.time - startTime < timeoutSeconds))
            {
                yield return null;
            }
            
            // Ensure condition was met and not timed out
            if (Time.time - startTime >= timeoutSeconds)
            {
                Debug.LogWarning("WaitForCondition timed out after " + timeoutSeconds + " seconds");
            }
        }
    }
}