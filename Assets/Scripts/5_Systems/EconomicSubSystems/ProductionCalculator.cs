using UnityEngine;
using Entities;

namespace Systems.Economics
{
    /// <summary>
    /// Handles production calculations using economic models like Cobb-Douglas
    /// </summary>
    public class ProductionCalculator
    {
        // Production model parameters
        private float productivityFactor;
        private float laborElasticity;
        private float capitalElasticity;
        
        public ProductionCalculator(float productivityFactor, float laborElasticity, float capitalElasticity)
        {
            this.productivityFactor = productivityFactor;
            this.laborElasticity = laborElasticity;
            this.capitalElasticity = capitalElasticity;
        }
        
        /// <summary>
        /// Calculates production using the Cobb-Douglas production function
        /// Y = A * L^α * K^β
        /// </summary>
        /// <param name="labor">Labor input (L)</param>
        /// <param name="capital">Capital input (K)</param>
        /// <returns>Production output value</returns>
        public float CalculateOutput(float labor, float capital)
        {
            // Guard against zero values to prevent NaN results
            labor = Mathf.Max(1f, labor);
            capital = Mathf.Max(1f, capital);
            
            // Cobb-Douglas production function: Y = A * L^α * K^β
            return productivityFactor * 
                Mathf.Pow(labor, laborElasticity) * 
                Mathf.Pow(capital, capitalElasticity);
        }
        
        /// <summary>
        /// Applies production calculation to a region entity
        /// </summary>
        /// <param name="region">The region to calculate production for</param>
        /// <returns>The calculated production value</returns>
        public int CalculateRegionProduction(RegionEntity region)
        {
            if (region == null) return 0;
            
            // Get inputs for production calculation
            float labor = region.LaborAvailable;
            float capital = region.InfrastructureLevel;
            
            // Calculate using Cobb-Douglas function
            float production = CalculateOutput(labor, capital);
            
            // Return as integer for gameplay purposes
            return Mathf.RoundToInt(production);
        }
    }
}