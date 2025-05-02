using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Entities;  // This imports the RegionEntity
using Entities.Components;  // This imports components like ResourceComponent
using Entities.ScriptableObjects; // This imports configuration objects

namespace Tests.EditMode
{
    /// <summary>
    /// Tests for the RegionEntity class to ensure proper data management
    /// and component interactions
    /// </summary>
    public class RegionEntityTests
    {
        private RegionEntity testRegion;
        
        [SetUp]
        public void SetUp()
        {
            // Create a new test region before each test
            testRegion = new RegionEntity("test_region", "Test Region", 200, 100);
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up after each test if needed
            testRegion = null;
        }
        
        [Test]
        public void RegionEntity_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange & Act done in SetUp
            
            // Assert
            Assert.AreEqual("test_region", testRegion.Id);
            Assert.AreEqual("Test Region", testRegion.Name);
            Assert.AreEqual(200, testRegion.Wealth);
            Assert.AreEqual(100, testRegion.Production);
        }
        
        [Test]
        public void RegionEntity_ComponentsInitialized_NotNull()
        {
            // Assert all components are initialized
            Assert.IsNotNull(testRegion.Economy);
            Assert.IsNotNull(testRegion.ProductionComp);
            Assert.IsNotNull(testRegion.Resources);
            Assert.IsNotNull(testRegion.PopulationComp);
            Assert.IsNotNull(testRegion.Infrastructure);
        }
        
        [Test]
        public void RegionEntity_ProcessTurn_UpdatesComponents()
        {
            // Arrange
            int initialWealth = testRegion.Wealth;
            
            // Act
            testRegion.ProcessTurn();
            
            // Assert
            // Verify components were updated during process turn
            // This will depend on your exact implementation details
            Assert.AreNotEqual(initialWealth, testRegion.Wealth, 
                "Wealth should change after processing a turn");
        }
        
        [Test]
        public void RegionEntity_InvestInInfrastructure_ReducesWealthAndIncreasesInfrastructure()
        {
            // Arrange
            int initialWealth = testRegion.Wealth;
            float initialInfrastructure = testRegion.InfrastructureLevel;
            float investmentAmount = 50;
            
            // Act
            float infrastructureChange = testRegion.InvestInInfrastructure(investmentAmount);
            
            // Assert
            Assert.Greater(infrastructureChange, 0, "Infrastructure should increase");
            Assert.Greater(testRegion.InfrastructureLevel, initialInfrastructure, 
                "Infrastructure level should increase after investment");
            Assert.Less(testRegion.Wealth, initialWealth, 
                "Wealth should decrease after investment");
        }
        
        [Test]
        public void RegionEntity_GetSummary_ReturnsNonEmptyString()
        {
            // Act
            string summary = testRegion.GetSummary();
            
            // Assert
            Assert.IsNotEmpty(summary);
            Assert.IsTrue(summary.Contains(testRegion.Name), 
                "Summary should contain the region name");
        }
        
        [Test]
        public void RegionEntity_Constructor_InitializesPropertiesCorrectly()
        {
            // Arrange & Act
            string id = "test_region";
            string name = "Test Region";
            RegionEntity region = new RegionEntity(id, name);
            
            // Assert
            Assert.AreEqual(id, region.Id);
            Assert.AreEqual(name, region.Name);
            Assert.IsNotNull(region.Resources);
            Assert.IsNotNull(region.ProductionComp);
            Assert.IsNotNull(region.Economy);
            Assert.IsNotNull(region.PopulationComp);
            Assert.IsNotNull(region.Infrastructure);
        }
        
        // Add more tests specific to your RegionEntity functionality
    }
}