using UnityEngine;
using System.Collections.Generic;

namespace Entities.Components
{
    /// <summary>
    /// CLASS PURPOSE:
    /// Handles population management for a region
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Track population size and growth
    /// - Manage labor availability and allocation
    /// - Calculate population satisfaction and needs
    /// </summary>
    public class PopulationComponent
    {
        // Population metrics
        public int Population { get; private set; }
        public float GrowthRate { get; private set; } = 0.01f; // Default 1% growth
        public float Satisfaction { get; private set; } = 0.7f; // 0.0 to 1.0
        
        // Labor related properties
        public float LaborAvailable { get; private set; }
        public float LaborEfficiency { get; private set; } = 1.0f;
        
        // Population needs and consumption
        private Dictionary<string, float> resourceNeeds = new Dictionary<string, float>();
        private Dictionary<string, float> needsSatisfaction = new Dictionary<string, float>();
        
        /// <summary>
        /// Initialize with default values
        /// </summary>
        public PopulationComponent(int initialPopulation = 1000, float initialLaborAvailable = 100)
        {
            Population = initialPopulation;
            LaborAvailable = initialLaborAvailable;
            
            // Default resource needs per population unit
            resourceNeeds.Add("Food", 0.1f);
            resourceNeeds.Add("Fuel", 0.05f);
            resourceNeeds.Add("Materials", 0.02f);
            
            // Initial satisfaction for each need
            needsSatisfaction.Add("Food", 0.8f);
            needsSatisfaction.Add("Fuel", 0.7f);
            needsSatisfaction.Add("Materials", 0.6f);
            needsSatisfaction.Add("Infrastructure", 0.5f);
        }
        
        /// <summary>
        /// Set resource need per population unit
        /// </summary>
        public void SetResourceNeed(string resource, float amountPerPopulation)
        {
            if (resourceNeeds.ContainsKey(resource))
                resourceNeeds[resource] = amountPerPopulation;
            else
                resourceNeeds.Add(resource, amountPerPopulation);
        }
        
        /// <summary>
        /// Calculate total resource needs for the population
        /// </summary>
        public float GetTotalResourceNeed(string resource)
        {
            if (resourceNeeds.ContainsKey(resource))
                return resourceNeeds[resource] * Population;
            return 0;
        }
        
        /// <summary>
        /// Update satisfaction level for a specific need
        /// </summary>
        public void UpdateNeedSatisfaction(string need, float satisfactionLevel)
        {
            satisfactionLevel = Mathf.Clamp01(satisfactionLevel);
            
            if (needsSatisfaction.ContainsKey(need))
                needsSatisfaction[need] = satisfactionLevel;
            else
                needsSatisfaction.Add(need, satisfactionLevel);
                
            // Recalculate overall satisfaction
            RecalculateSatisfaction();
        }
        
        /// <summary>
        /// Update infrastructure satisfaction based on infrastructure level
        /// </summary>
        public void UpdateInfrastructureSatisfaction(float infrastructureLevel)
        {
            float infrastructureSatisfaction = Mathf.Clamp01(infrastructureLevel / 10f);
            UpdateNeedSatisfaction("Infrastructure", infrastructureSatisfaction);
        }
        
        /// <summary>
        /// Calculate overall satisfaction level
        /// </summary>
        private void RecalculateSatisfaction()
        {
            if (needsSatisfaction.Count == 0)
                return;
                
            float totalSatisfaction = 0f;
            foreach (var satisfaction in needsSatisfaction.Values)
            {
                totalSatisfaction += satisfaction;
            }
            
            Satisfaction = totalSatisfaction / needsSatisfaction.Count;
        }
        
        /// <summary>
        /// Process population changes for one turn
        /// </summary>
        public void ProcessTurn(Dictionary<string, float> availableResources, float infrastructureLevel)
        {
            // Update infrastructure satisfaction
            UpdateInfrastructureSatisfaction(infrastructureLevel);
            
            // Calculate satisfaction for resources
            foreach (var need in resourceNeeds.Keys)
            {
                if (availableResources.TryGetValue(need, out float available))
                {
                    float required = GetTotalResourceNeed(need);
                    float satisfactionLevel = required > 0 ? Mathf.Clamp01(available / required) : 1.0f;
                    UpdateNeedSatisfaction(need, satisfactionLevel);
                }
            }
            
            // Calculate growth based on satisfaction
            float adjustedGrowthRate = GrowthRate * (Satisfaction * 2 - 0.5f);
            
            // Apply population growth/decline with a minimum floor
            int newPopulation = Mathf.Max(100, Mathf.RoundToInt(Population * (1 + adjustedGrowthRate)));
            int populationChange = newPopulation - Population;
            Population = newPopulation;
            
            // Update labor availability based on population and satisfaction
            LaborAvailable = Population * 0.1f * Satisfaction;
            
            // Labor efficiency affected by satisfaction
            LaborEfficiency = 0.5f + (Satisfaction * 0.5f);
        }
        
        /// <summary>
        /// Update labor availability by adding or subtracting a delta value
        /// </summary>
        public void UpdateLaborAvailable(float delta)
        {
            LaborAvailable = Mathf.Max(0, LaborAvailable + delta);
        }

        /// <summary>
        /// Get a summary of the population status
        /// </summary>
        public string GetSummary()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Population:");
            sb.AppendLine($"  Total Population: {Population}");
            sb.AppendLine($"  Growth Rate: {GrowthRate:P1}");
            sb.AppendLine($"  Satisfaction: {Satisfaction:P0}");
            sb.AppendLine($"  Labor Available: {LaborAvailable:F0}");
            sb.AppendLine($"  Labor Efficiency: {LaborEfficiency:P0}");
            
            sb.AppendLine("Needs Satisfaction:");
            foreach (var need in needsSatisfaction)
            {
                string satisfactionLevel = need.Value > 0.8f ? "High" :
                                          need.Value > 0.5f ? "Medium" : "Low";
                sb.AppendLine($"  {need.Key}: {satisfactionLevel} ({need.Value:P0})");
            }
            
            return sb.ToString();
        }
    }
}