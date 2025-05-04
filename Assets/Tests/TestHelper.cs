using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Entities;
using Entities.Components;
using Entities.ScriptableObjects;

namespace Tests
{
    /// <summary>
    /// Helper methods and utilities for testing the economic simulation.
    /// 
    /// PURPOSE:
    /// This utility class provides standard test objects and helper methods to make tests 
    /// more consistent and reduce code duplication. It centralizes the creation of mock entities,
    /// configurations, and test utilities.
    /// 
    /// MOTIVATION:
    /// In a component-based architecture with complex data relationships, having consistent test
    /// objects is essential for reliable and maintainable tests. This helper class ensures that
    /// all tests use consistently configured mock objects with predictable values.
    /// 
    /// USAGE:
    /// - Use CreateMockRegion() to get a fully configured RegionEntity for testing
    /// - Use CreateMockRegionConfig() if you need just the configuration
    /// - Use WaitForCondition() in PlayMode tests for asynchronous operations
    /// </summary>
    public static class TestHelper
    {
        /// <summary>
        /// Create a mock RegionConfig for testing.
        /// 
        /// Motivation: RegionConfig is a complex scriptable object that requires several sub-components.
        /// This method creates a complete configuration with predictable test values, removing the need
        /// for tests to manually create and configure all sub-components.
        /// 
        /// Returns: A fully configured RegionConfig with mock data for all component configurations.
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
        /// Create a mock ResourceComponentConfig for testing.
        /// 
        /// Motivation: ResourceComponentConfig defines the initial resources and production rates
        /// for a region. This method creates a consistent configuration with standard test values
        /// for food, materials, and fuel resources.
        /// 
        /// Returns: A ResourceComponentConfig with predefined test values.
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
        /// Create a mock ProductionComponentConfig for testing.
        /// 
        /// Motivation: ProductionComponentConfig defines the base production capabilities
        /// of a region. This method creates a standard test configuration with a predictable
        /// base production value.
        /// 
        /// Returns: A ProductionComponentConfig with a base production value of 100.
        /// </summary>
        private static ProductionComponentConfig CreateMockProductionConfig()
        {
            ProductionComponentConfig config = ScriptableObject.CreateInstance<ProductionComponentConfig>();
            
            // Set test values
            config.baseProduction = 100;
            
            return config;
        }
        
        /// <summary>
        /// Create a mock EconomyComponentConfig for testing.
        /// 
        /// Motivation: EconomyComponentConfig defines the initial economic state of a region.
        /// This method creates a standard test configuration with predictable GDP and wealth values.
        /// 
        /// Returns: An EconomyComponentConfig with an initial GDP of 200 and wealth of 500.
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
        /// Create a mock PopulationComponentConfig for testing.
        /// 
        /// Motivation: PopulationComponentConfig defines the initial population state of a region.
        /// This method creates a standard test configuration with predictable population size and
        /// available labor.
        /// 
        /// Returns: A PopulationComponentConfig with an initial population of 1000 and labor of 250.
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
        /// Create a mock InfrastructureComponentConfig for testing.
        /// 
        /// Motivation: InfrastructureComponentConfig defines the initial infrastructure state of a region.
        /// This method creates a standard test configuration with predictable level and quality values.
        /// 
        /// Returns: An InfrastructureComponentConfig with an initial level of 5.0 and quality of 0.7.
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
        /// Create a fully populated mock RegionEntity for testing.
        /// 
        /// Motivation: This is the primary method most tests will use. It creates a complete
        /// RegionEntity with all components properly initialized using standard test values.
        /// This ensures tests have a consistent starting point and reduces the need for
        /// each test to set up its own entity.
        /// 
        /// Parameters:
        /// - id: The unique identifier for the region (defaults to "test_region")
        /// - name: The display name for the region (defaults to "Test Region")
        /// 
        /// Returns: A fully initialized RegionEntity with standardized component values.
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
        /// Wait for a specified condition to be true with timeout.
        /// 
        /// Motivation: PlayMode tests often need to wait for asynchronous operations to complete.
        /// This utility provides a standardized way to wait for a condition with a timeout,
        /// avoiding infinite loops and making tests more robust.
        /// 
        /// Parameters:
        /// - condition: A function that returns true when the desired condition is met
        /// - timeoutSeconds: Maximum time to wait before timing out (defaults to 5 seconds)
        /// 
        /// Returns: A coroutine that can be yielded in a UnityTest to wait for the condition.
        /// 
        /// Example Usage:
        /// [UnityTest]
        /// public IEnumerator MyTest()
        /// {
        ///     // Setup test
        ///     bool operationComplete = false;
        ///     StartAsyncOperation(() => { operationComplete = true; });
        ///     
        ///     // Wait for async operation to complete
        ///     yield return TestHelper.WaitForCondition(() => operationComplete);
        ///     
        ///     // Assert results
        ///     Assert.IsTrue(someExpectedResult);
        /// }
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

        /// <summary>
        /// Sets a private field on an object using reflection.
        /// 
        /// Motivation: Tests often need to configure private fields that aren't exposed publicly.
        /// This utility method provides a standardized way to access and set private fields for testing.
        /// 
        /// Parameters:
        /// - target: The object instance containing the field to set
        /// - fieldName: Name of the private field to set
        /// - value: Value to set on the field
        /// 
        /// Example Usage:
        /// TestHelper.SetPrivateField(myObject, "privateField", 42);
        /// </summary>
        public static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, 
                BindingFlags.Instance | BindingFlags.NonPublic);
            
            if (field == null)
            {
                Debug.LogError($"Field '{fieldName}' not found on type {target.GetType().Name}");
                return;
            }
            
            field.SetValue(target, value);
        }
    }
}