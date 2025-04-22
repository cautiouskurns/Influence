using UnityEngine;
using Entities;
using System.Collections.Generic;

namespace Systems.Economics
{
    /// <summary>
    /// Handles price calculations based on supply and demand dynamics
    /// </summary>
    public class PriceCalculator
    {
        // Default price elasticity if not specified
        private const float DEFAULT_ELASTICITY = 1.0f;
        
        // Dictionary to store elasticity values for different resources
        private Dictionary<string, float> elasticityByResource = new Dictionary<string, float>();
        
        public PriceCalculator()
        {
            InitializeDefaultElasticities();
        }
        
        private void InitializeDefaultElasticities()
        {
            // Set default elasticity values for common resources
            // Higher elasticity means price is less sensitive to supply/demand changes
            elasticityByResource.Add("Food", 0.5f);      // Basic necessity, price sensitive
            elasticityByResource.Add("Luxury", 1.5f);    // Luxury goods, less price sensitive
            elasticityByResource.Add("RawMaterial", 0.8f); // Raw materials, moderately sensitive
            elasticityByResource.Add("Manufacturing", 1.2f); // Manufactured goods, less sensitive
        }
        
        /// <summary>
        /// Calculates dynamic price based on supply and demand
        /// </summary>
        /// <param name="basePrice">Base price of the resource</param>
        /// <param name="supply">Current supply level</param>
        /// <param name="demand">Current demand level</param>
        /// <param name="resourceType">Type of resource for elasticity determination</param>
        /// <returns>Calculated price</returns>
        public float CalculatePrice(float basePrice, float supply, float demand, string resourceType = null)
        {
            // Get elasticity value for this resource type, or use default
            float elasticity = DEFAULT_ELASTICITY;
            if (!string.IsNullOrEmpty(resourceType) && elasticityByResource.TryGetValue(resourceType, out float resourceElasticity))
            {
                elasticity = resourceElasticity;
            }
            
            // Prevent division by zero or negative elasticity
            elasticity = Mathf.Max(0.1f, elasticity);
            
            // Guard against zero supply (prevents extremely high prices)
            supply = Mathf.Max(0.1f, supply);
            
            // Calculate dynamic price: BasePrice × (1 + (Demand - Supply) / Elasticity)
            float dynamicFactor = 1.0f + ((demand - supply) / (supply * elasticity));
            
            // Ensure price doesn't go negative or too extreme
            dynamicFactor = Mathf.Clamp(dynamicFactor, 0.1f, 10.0f);
            
            return basePrice * dynamicFactor;
        }
        
        /// <summary>
        /// Applies income elasticity adjustments to demand based on wealth
        /// </summary>
        /// <param name="baseDemand">Base demand level</param>
        /// <param name="wealth">Region's wealth level</param>
        /// <param name="resourceType">Type of resource</param>
        /// <returns>Adjusted demand level</returns>
        public float AdjustDemandByIncome(float baseDemand, float wealth, string resourceType)
        {
            // Income elasticity factors for different resource types
            Dictionary<string, float> incomeElasticityFactors = new Dictionary<string, float>()
            {
                { "Food", 0.3f },         // Basic necessity, less affected by wealth
                { "Luxury", 1.8f },       // Luxury goods, strongly affected by wealth
                { "RawMaterial", 0.5f },  // Raw materials, moderately affected
                { "Manufacturing", 1.2f }  // Manufactured goods, significantly affected
            };
            
            // Get income elasticity factor for this resource type, or use default (1.0 - proportional)
            float incomeElasticityFactor = 1.0f;
            if (!string.IsNullOrEmpty(resourceType) && incomeElasticityFactors.TryGetValue(resourceType, out float factor))
            {
                incomeElasticityFactor = factor;
            }
            
            // Basic wealth modifier - scale wealth to reasonable range 
            float wealthModifier = Mathf.Log10(Mathf.Max(10, wealth)) * 0.5f;
            
            // Apply income elasticity
            return baseDemand * (1 + (wealthModifier * incomeElasticityFactor));
        }
        
        /// <summary>
        /// Calculates cross-price elasticity (substitution) effects
        /// </summary>
        /// <param name="baseDemand">Base demand for target resource</param>
        /// <param name="substitutePrice">Price of substitute resource</param>
        /// <param name="normalizedBasePrice">Normalized base price of target resource</param>
        /// <param name="crossPriceElasticity">Cross-price elasticity factor</param>
        /// <returns>Adjusted demand considering substitution effects</returns>
        public float CalculateSubstitutionEffect(float baseDemand, float substitutePrice, float normalizedBasePrice, float crossPriceElasticity)
        {
            // Positive value for substitutes, negative for complements
            // Higher substitute price increases demand for this good (if they're substitutes)
            float priceRatio = substitutePrice / normalizedBasePrice;
            float substitutionEffect = 1.0f + ((priceRatio - 1.0f) * crossPriceElasticity);
            
            // Clamp the effect to reasonable bounds
            substitutionEffect = Mathf.Clamp(substitutionEffect, 0.5f, 2.0f);
            
            return baseDemand * substitutionEffect;
        }
        
        /// <summary>
        /// Calculates price volatility and applies market shocks
        /// </summary>
        /// <param name="currentPrice">Current price</param>
        /// <param name="supplyShock">Supply shock factor (-1 to 1)</param>
        /// <param name="consumptionTrend">Consumption trend factor (-1 to 1)</param>
        /// <param name="volatilityFactor">Volatility coefficient (0-1)</param>
        /// <returns>New price after shock</returns>
        public float CalculatePriceShock(float currentPrice, float supplyShock, float consumptionTrend, float volatilityFactor = 0.2f)
        {
            // Ensure the volatility factor is within bounds
            volatilityFactor = Mathf.Clamp01(volatilityFactor);
            
            // Calculate price adjustment: α × (SupplyShock - ConsumptionTrend)
            float adjustment = volatilityFactor * (supplyShock - consumptionTrend);
            
            // Apply adjustment as a percentage of current price
            float newPrice = currentPrice * (1 + adjustment);
            
            // Ensure the price doesn't change too drastically
            return Mathf.Clamp(newPrice, currentPrice * 0.75f, currentPrice * 1.5f);
        }
        
        /// <summary>
        /// Set a custom elasticity value for a specific resource type
        /// </summary>
        public void SetResourceElasticity(string resourceType, float elasticity)
        {
            if (string.IsNullOrEmpty(resourceType)) return;
            
            if (elasticityByResource.ContainsKey(resourceType))
            {
                elasticityByResource[resourceType] = elasticity;
            }
            else
            {
                elasticityByResource.Add(resourceType, elasticity);
            }
        }
    }
}