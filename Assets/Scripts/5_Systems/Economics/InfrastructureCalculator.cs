using UnityEngine;
using Entities;

namespace Systems.Economics
{
    /// <summary>
    /// Handles infrastructure-related calculations for economic systems
    /// </summary>
    public class InfrastructureCalculator
    {
        // Default values
        private float efficiencyModifier = 0.1f;
        private float decayRate = 0.02f;
        private float maintenanceCostFactor = 0.05f;
        
        public InfrastructureCalculator(float efficiencyModifier = 0.1f, float decayRate = 0.02f, float maintenanceCostFactor = 0.05f)
        {
            this.efficiencyModifier = efficiencyModifier;
            this.decayRate = decayRate;
            this.maintenanceCostFactor = maintenanceCostFactor;
        }
        
        /// <summary>
        /// Calculates the efficiency boost provided by infrastructure
        /// </summary>
        /// <param name="infrastructureLevel">Current infrastructure level</param>
        /// <returns>Efficiency multiplier (1.0 = no boost)</returns>
        public float CalculateEfficiencyBoost(float infrastructureLevel)
        {
            // Efficiency formula: 1 + level × modifier
            return 1f + (infrastructureLevel * efficiencyModifier);
        }
        
        /// <summary>
        /// Calculates how much infrastructure decays over time if not maintained
        /// </summary>
        /// <param name="infrastructureLevel">Current infrastructure level</param>
        /// <param name="maintenanceInvested">Amount invested in maintenance</param>
        /// <returns>New infrastructure level after decay</returns>
        public float CalculateInfrastructureDecay(float infrastructureLevel, float maintenanceInvested)
        {
            // Calculate required maintenance
            float requiredMaintenance = CalculateMaintenanceCost(infrastructureLevel);
            
            // Calculate maintenance ratio (capped at 1.0)
            float maintenanceRatio = Mathf.Min(1.0f, maintenanceInvested / requiredMaintenance);
            
            // Calculate actual decay (reduced by maintenance)
            float actualDecayRate = decayRate * (1f - maintenanceRatio);
            
            // Apply decay
            float newInfrastructureLevel = infrastructureLevel * (1f - actualDecayRate);
            
            // Ensure we don't go below zero
            return Mathf.Max(0f, newInfrastructureLevel);
        }
        
        /// <summary>
        /// Calculates the cost to maintain infrastructure at its current level
        /// </summary>
        /// <param name="infrastructureLevel">Current infrastructure level</param>
        /// <returns>Maintenance cost</returns>
        public float CalculateMaintenanceCost(float infrastructureLevel)
        {
            // Maintenance increases non-linearly with infrastructure level
            return infrastructureLevel * infrastructureLevel * maintenanceCostFactor;
        }
        
        /// <summary>
        /// Calculates how much infrastructure is added per unit of investment
        /// </summary>
        /// <param name="investmentAmount">Amount to invest in infrastructure</param>
        /// <param name="currentInfrastructureLevel">Current infrastructure level</param>
        /// <returns>Infrastructure level increase</returns>
        public float CalculateInfrastructureGrowth(float investmentAmount, float currentInfrastructureLevel)
        {
            // Diminishing returns formula - higher levels require more investment
            float diminishingFactor = 1.0f / (1.0f + (currentInfrastructureLevel * 0.1f));
            
            // Calculate growth - more efficient at lower infrastructure levels
            return investmentAmount * diminishingFactor * 0.05f;
        }
        
        /// <summary>
        /// Applies infrastructure calculations to a region entity
        /// </summary>
        /// <param name="region">The region entity</param>
        /// <param name="maintenanceInvested">Amount invested in maintenance</param>
        /// <param name="developmentInvested">Amount invested in new development</param>
        /// <returns>Updated infrastructure level</returns>
        public float UpdateRegionInfrastructure(RegionEntity region, float maintenanceInvested, float developmentInvested)
        {
            if (region == null) return 0f;
            
            float currentLevel = region.InfrastructureLevel;
            
            // Calculate decay based on maintenance
            float levelAfterDecay = CalculateInfrastructureDecay(currentLevel, maintenanceInvested);
            
            // Calculate growth from new investment
            float growth = CalculateInfrastructureGrowth(developmentInvested, levelAfterDecay);
            
            // Update to new level
            float newLevel = levelAfterDecay + growth;
            
            return newLevel;
        }
    }
}