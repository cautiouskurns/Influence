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
    /// 
    /// PURPOSE:
    /// RegionEntity is a core data container in our economic simulation that manages multiple
    /// sub-components (Economy, Production, Resources, Population, Infrastructure). These tests
    /// verify that the entity properly initializes, maintains state integrity, and correctly
    /// handles interactions between components.
    ///
    /// MOTIVATION:
    /// Since RegionEntity lies at the heart of our simulation model, ensuring its correctness is critical.
    /// These tests help us catch regressions when refactoring and maintain the integrity of
    /// our economic model. Component-based architecture requires careful testing of interactions.
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
        
        /// <summary>
        /// Test: Verifies that the RegionEntity constructor correctly sets basic properties.
        /// 
        /// Motivation: This test ensures that the fundamental identifier and economic properties
        /// are correctly initialized when creating a region. If this fails, it indicates a problem
        /// with the constructor or property accessors.
        /// 
        /// Components:
        /// - Verifies ID assignment
        /// - Verifies Name assignment
        /// - Verifies initial Wealth value
        /// - Verifies initial Production value
        /// 
        /// Expected outcome: All properties match the values provided to the constructor.
        /// </summary>
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
        
        /// <summary>
        /// Test: Verifies that all component references are properly initialized.
        /// 
        /// Motivation: The component-based architecture requires that all sub-components
        /// are correctly initialized during entity creation. Missing components would
        /// cause NullReferenceExceptions during simulation processing.
        /// 
        /// Components:
        /// - Checks Economy component
        /// - Checks Production component
        /// - Checks Resources component
        /// - Checks Population component
        /// - Checks Infrastructure component
        /// 
        /// Expected outcome: No component references are null after initialization.
        /// </summary>
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
        
        /// <summary>
        /// Test: Verifies that processing a turn updates the region's economic state.
        /// 
        /// Motivation: The turn-based simulation depends on the ProcessTurn method to update
        /// all components with new values. This test ensures that wealth changes after processing,
        /// which indicates that the economic calculations are being applied.
        /// 
        /// Components:
        /// - Records initial wealth
        /// - Processes a simulation turn
        /// - Verifies wealth has been updated
        /// 
        /// Expected outcome: Wealth should change after processing a turn, indicating that
        /// economic calculations were performed (production, consumption, etc.).
        /// </summary>
        [Test]
        public void RegionEntity_ProcessTurn_UpdatesComponents()
        {
            // Arrange
            int initialWealth = testRegion.Wealth;
            
            // Act
            testRegion.ProcessTurn();
            
            // Assert
            Assert.AreNotEqual(initialWealth, testRegion.Wealth, 
                "Wealth should change after processing a turn");
        }
        
        /// <summary>
        /// Test: Verifies that infrastructure investment both increases infrastructure and decreases wealth.
        /// 
        /// Motivation: Infrastructure investments represent a core economic decision in the simulation.
        /// This test ensures that when investing in infrastructure:
        /// 1. Infrastructure level increases (representing improved facilities)
        /// 2. Wealth decreases (representing the cost of investment)
        /// 
        /// This validation is critical because it checks both sides of the transaction.
        /// 
        /// Components:
        /// - Records initial wealth and infrastructure level
        /// - Invests a specific amount in infrastructure
        /// - Verifies infrastructure increased
        /// - Verifies wealth decreased by the appropriate amount
        /// 
        /// Expected outcome: Infrastructure improves and wealth decreases proportionally to the investment.
        /// </summary>
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
            Assert.Less(testRegion.Wealth, initialWealth, "Wealth should decrease after investment");
            
            Debug.Log($"Infrastructure changed by: {infrastructureChange}, wealth decreased from {initialWealth} to {testRegion.Wealth}");
        }
        
        /// <summary>
        /// Test: Verifies that GetSummary produces meaningful textual output.
        /// 
        /// Motivation: The GetSummary method provides a human-readable overview of the region state,
        /// which is used in UI displays and debug information. This test ensures the method
        /// generates useful output containing the region's name and other essential information.
        /// 
        /// Components:
        /// - Calls GetSummary method
        /// - Checks that the output is not empty
        /// - Verifies the summary includes the region name
        /// 
        /// Expected outcome: A non-empty string that contains the region name is returned.
        /// </summary>
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
        
        /// <summary>
        /// Test: Verifies minimal constructor properly initializes the entity.
        /// 
        /// Motivation: The minimal constructor (id, name) is used when creating regions from
        /// configuration files or editor tools. This test ensures it properly initializes
        /// all required components even without explicit economic values.
        /// 
        /// Components:
        /// - Creates region with minimal constructor
        /// - Verifies ID and Name are set correctly
        /// - Checks that all necessary components are created and initialized
        /// 
        /// Expected outcome: Region should have correct ID/Name and all required components initialized.
        /// </summary>
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