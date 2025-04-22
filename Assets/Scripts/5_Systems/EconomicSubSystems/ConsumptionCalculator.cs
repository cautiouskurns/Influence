using UnityEngine;
using Entities;
using System.Collections.Generic;

namespace Systems.Economics
{
    /// <summary>
    /// Handles consumption calculations for economic systems
    /// </summary>
    public class ConsumptionCalculator
    {
        // Base consumption parameters
        private float baseConsumptionRate = 0.2f;
        private float wealthConsumptionExponent = 0.8f;  // <1 means diminishing consumption rate as wealth increases
        private float unmetDemandUnrestFactor = 0.05f;   // How much unmet demand contributes to unrest
        
        public ConsumptionCalculator(float baseConsumptionRate = 0.2f, 
                                     float wealthConsumptionExponent = 0.8f,
                                     float unmetDemandUnrestFactor = 0.05f)
        {
            this.baseConsumptionRate = baseConsumptionRate;
            this.wealthConsumptionExponent = wealthConsumptionExponent;
            this.unmetDemandUnrestFactor = unmetDemandUnrestFactor;
        }
        
        /// <summary>
        /// Calculates consumption based on wealth
        /// </summary>
        /// <param name="wealth">Current wealth level</param>
        /// <returns>Expected consumption</returns>
        public float CalculateExpectedConsumption(float wealth)
        {
            // Formula: BaseRate * Wealth^Exponent
            // As wealth increases, consumption increases but at a decreasing rate
            return baseConsumptionRate * Mathf.Pow(wealth, wealthConsumptionExponent);
        }
        
        /// <summary>
        /// Calculate consumption for specific resource types
        /// </summary>
        /// <param name="totalConsumption">Total expected consumption</param>
        /// <param name="resourceAllocation">Dictionary of resource type to proportion</param>
        /// <returns>Dictionary of resource type to consumption amount</returns>
        public Dictionary<string, float> CalculateResourceConsumption(float totalConsumption, Dictionary<string, float> resourceAllocation)
        {
            Dictionary<string, float> resourceConsumption = new Dictionary<string, float>();
            
            // Ensure proportions sum to 1.0
            float totalProportion = 0f;
            foreach (var proportion in resourceAllocation.Values)
            {
                totalProportion += proportion;
            }
            
            if (totalProportion <= 0f) return resourceConsumption;
            
            // Calculate consumption for each resource type
            foreach (var resource in resourceAllocation)
            {
                float normalizedProportion = resource.Value / totalProportion;
                float consumption = totalConsumption * normalizedProportion;
                resourceConsumption.Add(resource.Key, consumption);
            }
            
            return resourceConsumption;
        }
        
        /// <summary>
        /// Calculate unmet demand when consumption can't be fully satisfied
        /// </summary>
        /// <param name="expectedConsumption">Expected consumption</param>
        /// <param name="actualConsumption">Actual consumption based on available resources</param>
        /// <returns>Unmet demand ratio (0-1)</returns>
        public float CalculateUnmetDemand(float expectedConsumption, float actualConsumption)
        {
            if (expectedConsumption <= 0f) return 0f;
            
            float unmetDemand = expectedConsumption - actualConsumption;
            float unmetDemandRatio = Mathf.Clamp01(unmetDemand / expectedConsumption);
            
            return unmetDemandRatio;
        }
        
        /// <summary>
        /// Calculate unrest from unmet demand
        /// </summary>
        /// <param name="unmetDemandRatio">Ratio of unmet demand (0-1)</param>
        /// <returns>Unrest increase</returns>
        public float CalculateUnrestFromUnmetDemand(float unmetDemandRatio)
        {
            // Non-linear relationship: small shortages have minimal impact,
            // but large shortages cause disproportionately more unrest
            return unmetDemandRatio * unmetDemandRatio * unmetDemandUnrestFactor * 100f;
        }
        
        /// <summary>
        /// Apply consumption calculations to a region entity
        /// </summary>
        /// <param name="region">The region entity</param>
        /// <param name="availableResources">Dictionary of available resources</param>
        /// <param name="resourceAllocation">Dictionary of resource consumption preferences</param>
        /// <returns>Applied consumption, unmet demand, and unrest</returns>
        public (float consumption, float unmetDemand, float unrest) ProcessRegionConsumption(
            RegionEntity region, 
            Dictionary<string, float> availableResources,
            Dictionary<string, float> resourceAllocation)
        {
            if (region == null) return (0f, 0f, 0f);
            
            // Calculate expected consumption based on wealth
            float expectedConsumption = CalculateExpectedConsumption(region.Wealth);
            
            // Distribute consumption across resource types
            Dictionary<string, float> expectedResourceConsumption = 
                CalculateResourceConsumption(expectedConsumption, resourceAllocation);
            
            // Calculate actual consumption based on available resources
            float actualConsumption = 0f;
            foreach (var resource in expectedResourceConsumption)
            {
                string resourceType = resource.Key;
                float expected = resource.Value;
                
                // Check if this resource is available
                if (availableResources.TryGetValue(resourceType, out float available))
                {
                    // Consume up to available amount
                    float consumed = Mathf.Min(expected, available);
                    actualConsumption += consumed;
                    
                    // Update available resources
                    availableResources[resourceType] = available - consumed;
                }
            }
            
            // Calculate unmet demand
            float unmetDemandRatio = CalculateUnmetDemand(expectedConsumption, actualConsumption);
            
            // Calculate unrest from unmet demand
            float unrestIncrease = CalculateUnrestFromUnmetDemand(unmetDemandRatio);
            
            return (actualConsumption, unmetDemandRatio, unrestIncrease);
        }
    }
}