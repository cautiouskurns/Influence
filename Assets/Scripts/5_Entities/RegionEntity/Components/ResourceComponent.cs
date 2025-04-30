using UnityEngine;
using System.Collections.Generic;

namespace Entities.Components
{
    /// <summary>
    /// CLASS PURPOSE:
    /// Handles resource production and management for a region
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Track resource types and amounts
    /// - Calculate resource production and consumption
    /// - Provide resource-related data
    /// </summary>
    public class ResourceComponent
    {
        // Dictionary to store resources and their amounts
        private Dictionary<string, float> resources = new Dictionary<string, float>();
        
        // Production rates for each resource
        private Dictionary<string, float> productionRates = new Dictionary<string, float>();
        
        // Initialize with default resources
        public ResourceComponent()
        {
            // Add some default resources
            resources.Add("Food", 100);
            resources.Add("Materials", 100);
            resources.Add("Fuel", 50);
            
            // Set default production rates
            productionRates.Add("Food", 10);
            productionRates.Add("Materials", 8);
            productionRates.Add("Fuel", 5);
        }
        
        /// <summary>
        /// Get current amount of a specific resource
        /// </summary>
        public float GetResourceAmount(string resourceType)
        {
            if (resources.ContainsKey(resourceType))
                return resources[resourceType];
            return 0;
        }
        
        /// <summary>
        /// Get production rate of a specific resource
        /// </summary>
        public float GetProductionRate(string resourceType)
        {
            if (productionRates.ContainsKey(resourceType))
                return productionRates[resourceType];
            return 0;
        }
        
        /// <summary>
        /// Set production rate for a resource
        /// </summary>
        public void SetProductionRate(string resourceType, float rate)
        {
            if (productionRates.ContainsKey(resourceType))
                productionRates[resourceType] = rate;
            else
                productionRates.Add(resourceType, rate);
        }
        
        /// <summary>
        /// Process resource production for one turn
        /// </summary>
        public void ProcessProduction()
        {
            // Update each resource based on its production rate
            foreach (var resource in productionRates.Keys)
            {
                if (resources.ContainsKey(resource))
                    resources[resource] += productionRates[resource];
                else
                    resources.Add(resource, productionRates[resource]);
            }
        }
        
        /// <summary>
        /// Get a summary of resources
        /// </summary>
        public string GetSummary()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Resources:");
            
            foreach (var resource in resources)
            {
                float production = GetProductionRate(resource.Key);
                string trend = production > 0 ? "↑" : production < 0 ? "↓" : "→";
                
                sb.AppendLine($"  {resource.Key}: {resource.Value:F0} {trend} ({production:F1}/turn)");
            }
            
            return sb.ToString();
        }
    }
}