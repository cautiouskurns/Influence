using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Entities;
using Entities.Components;
using Managers;

namespace Tests.EditMode
{
    /// <summary>
    /// Tests for the NationEntity class to ensure proper nation management, policy implementation,
    /// and diplomatic relationship handling.
    /// 
    /// PURPOSE:
    /// NationEntity represents a major political entity in our simulation that manages multiple regions
    /// and handles international relations. These tests verify that nations properly initialize,
    /// apply policies to their regions, and correctly manage diplomatic relationships.
    ///
    /// MOTIVATION:
    /// Nations serve as a higher-level organizational unit in our simulation. Ensuring their correct
    /// behavior is critical for the economic and political aspects of the model. These tests focus on
    /// validating the creation process, policy implementation effects, and diplomatic relationship management.
    /// </summary>
    public class NationEntityTests
    {
        private NationEntity testNation;
        private RegionEntity testRegion1;
        private RegionEntity testRegion2;
        private NationEntity foreignNation;
        
        [SetUp]
        public void SetUp()
        {
            // Create test nations with distinct properties
            testNation = new NationEntity("nation1", "Test Nation", Color.blue);
            foreignNation = new NationEntity("nation2", "Foreign Nation", Color.red);
            
            // Create test regions to be managed by the nation
            testRegion1 = new RegionEntity("region1", "Test Region 1", 200, 100);
            testRegion2 = new RegionEntity("region2", "Test Region 2", 300, 150);
            
            // Assign regions to the test nation
            testNation.AddRegion(testRegion1.Id);
            testNation.AddRegion(testRegion2.Id);
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up after each test
            testNation = null;
            foreignNation = null;
            testRegion1 = null;
            testRegion2 = null;
        }
        
        #region Nation Creation and Initialization Tests
        
        /// <summary>
        /// Test: Verifies that the NationEntity constructor correctly sets basic properties.
        /// 
        /// Motivation: This test ensures that the fundamental properties like ID, name, and color
        /// are correctly initialized when creating a nation. If this fails, it indicates a problem
        /// with the constructor or property accessors.
        /// 
        /// Expected outcome: All properties match the values provided to the constructor.
        /// </summary>
        [Test]
        public void NationEntity_Constructor_SetsPropertiesCorrectly()
        {
            // Assert
            Assert.AreEqual("nation1", testNation.Id);
            Assert.AreEqual("Test Nation", testNation.Name);
            Assert.AreEqual(Color.blue, testNation.Color);
        }
        
        /// <summary>
        /// Test: Verifies that all component references are properly initialized.
        /// 
        /// Motivation: The component-based architecture requires that all sub-components
        /// are correctly initialized during entity creation. Missing components would
        /// cause NullReferenceExceptions during simulation processing.
        /// 
        /// Expected outcome: No component references are null after initialization.
        /// </summary>
        [Test]
        public void NationEntity_ComponentsInitialized_NotNull()
        {
            // Assert all components are initialized
            Assert.IsNotNull(testNation.Economy);
            Assert.IsNotNull(testNation.Policy);
            Assert.IsNotNull(testNation.Diplomacy);
            Assert.IsNotNull(testNation.Stability);
        }
        
        /// <summary>
        /// Test: Verifies that default policy values are correctly initialized.
        /// 
        /// Motivation: Policy settings are crucial for nation behavior, and should start with
        /// balanced default values (0.5) to ensure predictable initial simulation state.
        /// 
        /// Expected outcome: All policy values should be initialized to 0.5 (balanced).
        /// </summary>
        [Test]
        public void NationEntity_DefaultPolicyValues_AreBalanced()
        {
            // Check all policy types have balanced default values
            Assert.AreEqual(0.5f, testNation.GetPolicy(NationEntity.PolicyType.Economic));
            Assert.AreEqual(0.5f, testNation.GetPolicy(NationEntity.PolicyType.Diplomatic));
            Assert.AreEqual(0.5f, testNation.GetPolicy(NationEntity.PolicyType.Military));
            Assert.AreEqual(0.5f, testNation.GetPolicy(NationEntity.PolicyType.Social));
        }
        
        /// <summary>
        /// Test: Verifies that regions can be added and retrieved properly.
        /// 
        /// Motivation: Nations manage collections of regions, and this relationship must
        /// be correctly maintained. This test ensures regions can be added to a nation
        /// and retrieved accurately.
        /// 
        /// Expected outcome: The list of region IDs should match what was added.
        /// </summary>
        [Test]
        public void NationEntity_RegionManagement_AddsAndRetrievesRegions()
        {
            // Act
            List<string> regionIds = testNation.GetRegionIds();
            
            // Assert
            Assert.AreEqual(2, regionIds.Count);
            Assert.Contains("region1", regionIds);
            Assert.Contains("region2", regionIds);
        }
        
        /// <summary>
        /// Test: Verifies that regions can be properly removed from a nation.
        /// 
        /// Motivation: Nations must be able to both add and remove regions to handle
        /// territory changes. This test ensures the removal functionality works correctly.
        /// 
        /// Expected outcome: The region should no longer be in the nation's list after removal.
        /// </summary>
        [Test]
        public void NationEntity_RegionManagement_RemovesRegions()
        {
            // Act
            testNation.RemoveRegion("region1");
            List<string> regionIds = testNation.GetRegionIds();
            
            // Assert
            Assert.AreEqual(1, regionIds.Count);
            Assert.IsFalse(regionIds.Contains("region1"));
            Assert.IsTrue(regionIds.Contains("region2"));
        }
        
        /// <summary>
        /// Test: Verifies that GetSummary produces meaningful textual output.
        /// 
        /// Motivation: The GetSummary method provides a human-readable overview of the nation state,
        /// which is used in UI displays and debug information. This test ensures the method
        /// generates useful output containing the nation's name and essential metrics.
        /// 
        /// Expected outcome: A non-empty string that contains the nation name is returned.
        /// </summary>
        [Test]
        public void NationEntity_GetSummary_ReturnsNonEmptyString()
        {
            // Act
            string summary = testNation.GetSummary();
            
            // Assert
            Assert.IsNotEmpty(summary);
            Assert.IsTrue(summary.Contains(testNation.Name), 
                "Summary should contain the nation name");
            Assert.IsTrue(summary.Contains("ECONOMY"), 
                "Summary should include economic information");
            Assert.IsTrue(summary.Contains("DIPLOMACY"), 
                "Summary should include diplomatic information");
        }
        
        #endregion
        
        #region National Policy Effects Tests
        
        /// <summary>
        /// Test: Verifies that policy slider values can be set and retrieved.
        /// 
        /// Motivation: Policy settings represent core decision points for nations and must
        /// be correctly stored and retrievable for UI display and simulation processing.
        /// 
        /// Expected outcome: The policy value retrieved should match what was set.
        /// </summary>
        [Test]
        public void NationEntity_PolicySlider_SetsAndGetsCorrectly()
        {
            // Arrange
            float newEconomicPolicy = 0.75f;
            
            // Act
            testNation.SetPolicy(NationEntity.PolicyType.Economic, newEconomicPolicy);
            
            // Assert
            Assert.AreEqual(newEconomicPolicy, testNation.GetPolicy(NationEntity.PolicyType.Economic));
        }
        
        /// <summary>
        /// Test: Verifies that a policy can be implemented when there are sufficient funds.
        /// 
        /// Motivation: Policy implementation is a key nation action that should only succeed
        /// when there are sufficient treasury funds to cover the cost.
        /// 
        /// Expected outcome: Policy implementation succeeds with adequate funding.
        /// </summary>
        [Test]
        public void NationEntity_ImplementPolicy_SucceedsWithSufficientFunds()
        {
            // Arrange
            Policy economicReform = NationEntity.CreateStandardPolicy(
                "Economic Reform", 
                "Boosts economic output", 
                50, // cost
                5,  // duration
                0.1f, // wealth effect
                0.2f, // production effect
                0.0f  // stability effect
            );
            
            // Set sufficient treasury balance
            testNation.Economy.SetTreasuryBalance(100f);
            
            // Act
            bool result = testNation.ImplementPolicy(economicReform);
            
            // Assert
            Assert.IsTrue(result, "Policy implementation should succeed with sufficient funds");
        }
        
        /// <summary>
        /// Test: Verifies that policy implementation fails when there are insufficient funds.
        /// 
        /// Motivation: Policy implementation should have a cost constraint to make economic
        /// decisions meaningful in the simulation.
        /// 
        /// Expected outcome: Policy implementation fails with inadequate funding.
        /// </summary>
        [Test]
        public void NationEntity_ImplementPolicy_FailsWithInsufficientFunds()
        {
            // Arrange
            Policy economicReform = NationEntity.CreateStandardPolicy(
                "Economic Reform", 
                "Boosts economic output", 
                200, // cost - higher than treasury balance
                5,   // duration
                0.1f, // wealth effect
                0.2f, // production effect
                0.0f  // stability effect
            );
            
            // Set insufficient treasury balance
            testNation.Economy.SetTreasuryBalance(50f);
            
            // Act
            bool result = testNation.ImplementPolicy(economicReform);
            
            // Assert
            Assert.IsFalse(result, "Policy implementation should fail with insufficient funds");
        }
        
        /// <summary>
        /// Test: Verifies that processing a turn updates the nation's economic state.
        /// 
        /// Motivation: The turn-based simulation depends on the ProcessTurn method to update
        /// all components with new values. This test ensures that treasury balance changes after processing,
        /// which indicates that the economic calculations are being applied.
        /// 
        /// Expected outcome: Economic metrics should change after processing a turn.
        /// </summary>
        [Test]
        public void NationEntity_ProcessTurn_UpdatesEconomicMetrics()
        {
            // Arrange
            List<RegionEntity> regions = new List<RegionEntity> { testRegion1, testRegion2 };
            float initialTreasuryBalance = testNation.Economy.TreasuryBalance;
            
            // Act
            testNation.ProcessTurn(regions);
            
            // Assert
            Assert.AreNotEqual(initialTreasuryBalance, testNation.Economy.TreasuryBalance,
                "Treasury balance should change after processing a turn");
        }
        
        /// <summary>
        /// Test: Verifies that high economic policy increases production in member regions.
        /// 
        /// Motivation: Policy settings should have tangible effects on member regions, with
        /// economic policies directly influencing production capabilities.
        /// 
        /// Expected outcome: Regions should have higher production modifiers with higher
        /// economic policy settings.
        /// </summary>
        [Test]
        public void NationEntity_HighEconomicPolicy_IncreasesRegionProduction()
        {
            // Arrange
            List<RegionEntity> regions = new List<RegionEntity> { testRegion1, testRegion2 };
            float initialProduction = testRegion1.Production;
            
            // Set high economic policy
            testNation.SetPolicy(NationEntity.PolicyType.Economic, 0.9f);
            
            // Apply policy effects to regions
            testNation.ApplyPolicyEffects(regions);
            
            // Process turn for each region to see effects
            testRegion1.ProcessTurn();
            
            // Assert
            Assert.Greater(testRegion1.Production, initialProduction,
                "Region production should increase with high economic policy");
        }
        
        #endregion
        
        #region Diplomatic Relation Tests
        
        /// <summary>
        /// Test: Verifies that diplomatic status can be set and retrieved correctly.
        /// 
        /// Motivation: Diplomatic relations are a core aspect of inter-nation dynamics
        /// and must be correctly maintained between nation entities.
        /// 
        /// Expected outcome: The diplomatic status retrieved should match what was set.
        /// </summary>
        [Test]
        public void NationEntity_DiplomaticStatus_SetsAndGetsCorrectly()
        {
            // Arrange
            NationEntity.DiplomaticStatus newStatus = NationEntity.DiplomaticStatus.Allied;
            
            // Act
            testNation.SetDiplomaticStatus(foreignNation.Id, newStatus);
            NationEntity.DiplomaticStatus retrievedStatus = testNation.GetDiplomaticStatus(foreignNation.Id);
            
            // Assert
            Assert.AreEqual(newStatus, retrievedStatus);
        }
        
        /// <summary>
        /// Test: Verifies that diplomatic events affect relationship scores.
        /// 
        /// Motivation: Diplomatic events should have quantifiable impacts on nation relationships.
        /// This test ensures that a positive event improves relations as expected.
        /// 
        /// Expected outcome: The relationship score should increase after a positive diplomatic event.
        /// </summary>
        [Test]
        public void NationEntity_PositiveDiplomaticEvent_ImprovesDiplomaticRelations()
        {
            // Arrange - Get initial relation
            DiplomaticRelation initialRelation = testNation.GetDiplomaticRelation(foreignNation.Id);
            float initialScore = initialRelation.RelationScore;
            
            // Act - Apply a positive diplomatic event
            testNation.ApplyDiplomaticEvent(foreignNation.Id, 10.0f, "Trade agreement");
            
            // Assert
            DiplomaticRelation updatedRelation = testNation.GetDiplomaticRelation(foreignNation.Id);
            Assert.Greater(updatedRelation.RelationScore, initialScore,
                "Diplomatic relation score should increase after positive event");
        }
        
        /// <summary>
        /// Test: Verifies that negative diplomatic events reduce relationship scores.
        /// 
        /// Motivation: Diplomatic events should have appropriate negative consequences when
        /// they represent conflicts or disagreements between nations.
        /// 
        /// Expected outcome: The relationship score should decrease after a negative diplomatic event.
        /// </summary>
        [Test]
        public void NationEntity_NegativeDiplomaticEvent_HarmsDiplomaticRelations()
        {
            // Arrange - Get initial relation
            DiplomaticRelation initialRelation = testNation.GetDiplomaticRelation(foreignNation.Id);
            float initialScore = initialRelation.RelationScore;
            
            // Act - Apply a negative diplomatic event
            testNation.ApplyDiplomaticEvent(foreignNation.Id, -10.0f, "Border dispute");
            
            // Assert
            DiplomaticRelation updatedRelation = testNation.GetDiplomaticRelation(foreignNation.Id);
            Assert.Less(updatedRelation.RelationScore, initialScore,
                "Diplomatic relation score should decrease after negative event");
        }
        
        /// <summary>
        /// Test: Verifies that diplomatic action permissions are based on relationship status.
        /// 
        /// Motivation: Certain diplomatic actions should only be possible with appropriate
        /// relationship levels, enforcing meaningful progression in international relations.
        /// 
        /// Expected outcome: Actions should be permitted or denied based on relationship status.
        /// </summary>
        [Test]
        public void NationEntity_DiplomaticActionPermission_DependsOnRelationshipStatus()
        {
            // Arrange - Set up allied relationship
            testNation.SetDiplomaticStatus(foreignNation.Id, NationEntity.DiplomaticStatus.Allied);
            
            // Act & Assert - Check if diplomatic action can be performed
            Assert.IsTrue(testNation.CanPerformDiplomaticAction(foreignNation.Id, DiplomaticActionType.Trade),
                "Trade agreements should be possible with allied nations");
                
            // Change to hostile relationship
            testNation.SetDiplomaticStatus(foreignNation.Id, NationEntity.DiplomaticStatus.Hostile);
            
            Assert.IsFalse(testNation.CanPerformDiplomaticAction(foreignNation.Id, DiplomaticActionType.Trade),
                "Trade agreements shouldn't be possible with hostile nations");
        }
        
        /// <summary>
        /// Test: Verifies that diplomatic relations can be established with multiple nations.
        /// 
        /// Motivation: Nations should be able to maintain relationships with many other nations
        /// simultaneously, each with their own status and metrics.
        /// 
        /// Expected outcome: Nation should successfully maintain distinct relationships.
        /// </summary>
        [Test]
        public void NationEntity_MultipleDiplomaticRelations_MaintainsDistinctRelationships()
        {
            // Arrange
            NationEntity thirdNation = new NationEntity("nation3", "Third Nation", Color.green);
            
            // Act - Set different diplomatic statuses
            testNation.SetDiplomaticStatus(foreignNation.Id, NationEntity.DiplomaticStatus.Allied);
            testNation.SetDiplomaticStatus(thirdNation.Id, NationEntity.DiplomaticStatus.Hostile);
            
            // Apply different events
            testNation.ApplyDiplomaticEvent(foreignNation.Id, 15.0f, "Military alliance");
            testNation.ApplyDiplomaticEvent(thirdNation.Id, -15.0f, "Trade sanctions");
            
            // Assert - Each relationship should maintain its own status
            Assert.AreEqual(NationEntity.DiplomaticStatus.Allied, 
                testNation.GetDiplomaticStatus(foreignNation.Id));
            Assert.AreEqual(NationEntity.DiplomaticStatus.Hostile, 
                testNation.GetDiplomaticStatus(thirdNation.Id));
            
            // Verify relation scores are different
            float allyScore = testNation.GetDiplomaticRelation(foreignNation.Id).RelationScore;
            float enemyScore = testNation.GetDiplomaticRelation(thirdNation.Id).RelationScore;
            Assert.Greater(allyScore, enemyScore, 
                "Allied nation should have higher relation score than hostile nation");
        }
        
        #endregion
    }
}