using UnityEngine;
using System.Collections.Generic;

namespace Entities.Components
{
    /// <summary>
    /// CLASS PURPOSE:
    /// Handles economic aspects of a region
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Track economic metrics like GDP, growth, and income
    /// - Calculate tax revenues and expenses
    /// - Manage economic modifiers and factors
    /// </summary>
    public class RegionEconomyComponent
    {
        // Core economic metrics
        public float RegionalGDP { get; private set; }
        public float GrowthRate { get; private set; } = 0.02f; // Default 2% growth
        public float TaxRevenue { get; private set; }
        
        // Wealth is now encapsulated within the economy component
        public int Wealth { get; private set; }
        
        // Economic factors
        private Dictionary<string, float> economicFactors = new Dictionary<string, float>();
        
        // Historical data for trends
        private Queue<float> gdpHistory = new Queue<float>();
        private readonly int maxHistoryLength = 5;
        
        /// <summary>
        /// Initialize with default values
        /// </summary>
        public RegionEconomyComponent(float initialGDP = 100f, int initialWealth = 100)
        {
            RegionalGDP = initialGDP;
            Wealth = initialWealth;
            
            // Default economic factors
            economicFactors.Add("Infrastructure", 1.0f);
            economicFactors.Add("Stability", 1.0f);
            economicFactors.Add("Resources", 1.0f);
            
            // Initialize history
            gdpHistory.Enqueue(RegionalGDP);
        }
        
        /// <summary>
        /// Adjust the wealth value by the specified amount (can be positive or negative)
        /// </summary>
        public void UpdateWealth(int amount)
        {
            Wealth += amount;
            // Ensure wealth doesn't go negative
            Wealth = Mathf.Max(0, Wealth);
        }
        
        /// <summary>
        /// Set wealth to a specific value directly
        /// </summary>
        public void SetWealth(int value)
        {
            Wealth = Mathf.Max(0, value);
        }
        
        /// <summary>
        /// Set economic factor value
        /// </summary>
        public void SetEconomicFactor(string factorName, float value)
        {
            if (economicFactors.ContainsKey(factorName))
                economicFactors[factorName] = value;
            else
                economicFactors.Add(factorName, value);
        }
        
        /// <summary>
        /// Calculate tax revenue based on GDP and tax rate
        /// </summary>
        public float CalculateTaxRevenue(float taxRate)
        {
            TaxRevenue = RegionalGDP * taxRate;
            return TaxRevenue;
        }
        
        /// <summary>
        /// Update economy for a turn
        /// </summary>
        public void ProcessTurn(float resourceProduction, float infrastructure, float taxRate = 0.1f)
        {
            // Update economic factors
            SetEconomicFactor("Resources", 0.5f + (resourceProduction / 100f));
            SetEconomicFactor("Infrastructure", 0.5f + (infrastructure / 10f));
            
            // Calculate combined economic growth multiplier
            float growthMultiplier = 1.0f;
            foreach (var factor in economicFactors.Values)
            {
                growthMultiplier *= factor;
            }
            
            // Apply growth to GDP
            float previousGDP = RegionalGDP;
            RegionalGDP *= (1 + (GrowthRate * growthMultiplier));
            
            // Calculate new growth rate
            GrowthRate = (RegionalGDP - previousGDP) / previousGDP;
            
            // Update wealth based on GDP
            Wealth = Mathf.RoundToInt(RegionalGDP * 0.5f);
            
            // Add to history
            gdpHistory.Enqueue(RegionalGDP);
            if (gdpHistory.Count > maxHistoryLength)
            {
                gdpHistory.Dequeue();
            }
            
            // Calculate tax revenue
            CalculateTaxRevenue(taxRate);
        }
        
        /// <summary>
        /// Get a summary of the economic situation
        /// </summary>
        public string GetSummary()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Regional Economy:");
            sb.AppendLine($"  Wealth: {Wealth}");
            sb.AppendLine($"  GDP: {RegionalGDP:F1}");
            sb.AppendLine($"  Growth Rate: {GrowthRate:P1}");
            sb.AppendLine($"  Tax Revenue: {TaxRevenue:F1}");
            
            sb.AppendLine("Economic Factors:");
            foreach (var factor in economicFactors)
            {
                sb.AppendLine($"  {factor.Key}: x{factor.Value:F2}");
            }
            
            return sb.ToString();
        }
    }
}