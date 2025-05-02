using System.Collections.Generic;
using UnityEngine;

namespace Entities.Components
{
    /// <summary>
    /// CLASS PURPOSE:
    /// Handles all economic aspects of a nation, including wealth tracking,
    /// tax collection, and economic metrics.
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Aggregate economic data from regions
    /// - Manage treasury and taxation
    /// - Calculate economic metrics like GDP and growth
    /// </summary>
    public class NationEconomyComponent
    {
        // Aggregated economic data
        public float TotalWealth { get; private set; }
        public float TotalProduction { get; private set; }
        public float TreasuryBalance { get; private set; }
        
        // Economic policy settings
        public float TaxRate { get; set; } = 0.1f;  // Default 10% tax rate
        public float InfrastructureInvestment { get; set; } = 0.3f;  // Default 30% of treasury goes to infrastructure
        
        // Economic metrics
        public float GDP { get; private set; }
        public float GDPGrowthRate { get; private set; }
        public float PreviousGDP { get; private set; }
        public float Inflation { get; private set; } = 0.02f;  // Default 2% inflation
        
        // Historical data for trends
        private Queue<float> gdpHistory = new Queue<float>();
        private readonly int maxHistoryLength = 10;
        
        /// <summary>
        /// Set the treasury balance - intended for testing purposes
        /// </summary>
        public void SetTreasuryBalance(float value)
        {
            TreasuryBalance = Mathf.Max(0, value); // Ensure balance is never negative
        }
        
        /// <summary>
        /// Update economic metrics based on region data
        /// </summary>
        public void UpdateFromRegions(List<RegionEntity> regions)
        {
            if (regions == null || regions.Count == 0)
            {
                TotalWealth = 0;
                TotalProduction = 0;
                GDP = 0;
                return;
            }
            
            // Save previous GDP for growth calculations
            PreviousGDP = GDP;
            
            // Reset totals
            TotalWealth = 0;
            TotalProduction = 0;
            
            // Aggregate data from all regions
            foreach (var region in regions)
            {
                TotalWealth += region.Wealth;
                TotalProduction += region.Production;
            }
            
            // Calculate GDP (simple formula: wealth + production)
            GDP = TotalWealth + TotalProduction * 2;
            
            // Add to history
            gdpHistory.Enqueue(GDP);
            if (gdpHistory.Count > maxHistoryLength)
            {
                gdpHistory.Dequeue();
            }
            
            // Calculate GDP growth rate
            if (PreviousGDP > 0)
            {
                GDPGrowthRate = (GDP - PreviousGDP) / PreviousGDP;
            }
            else
            {
                GDPGrowthRate = 0;
            }
        }
        
        /// <summary>
        /// Collect taxes from all regions
        /// </summary>
        public float CollectTaxes(List<RegionEntity> regions)
        {
            float taxRevenue = 0;
            
            foreach (var region in regions)
            {
                // Tax is calculated as a percentage of region wealth
                float regionTax = region.Wealth * TaxRate;
                
                // Reduce region wealth by tax amount
                region.Wealth -= Mathf.RoundToInt(regionTax);
                
                // Add to tax revenue
                taxRevenue += regionTax;
            }
            
            // Add tax revenue to treasury
            TreasuryBalance += taxRevenue;
            
            return taxRevenue;
        }
        
        /// <summary>
        /// Distribute treasury funds as subsidies to regions
        /// </summary>
        public float DistributeSubsidies(List<RegionEntity> regions, float amount)
        {
            // Can't distribute more than what's in the treasury
            amount = Mathf.Min(amount, TreasuryBalance);
            
            if (amount <= 0 || regions.Count == 0)
                return 0;
            
            // Calculate per-region subsidy
            float subsidyPerRegion = amount / regions.Count;
            
            foreach (var region in regions)
            {
                // Add subsidy to region wealth
                region.Wealth += Mathf.RoundToInt(subsidyPerRegion);
            }
            
            // Reduce treasury balance
            TreasuryBalance -= amount;
            
            return amount;
        }
        
        /// <summary>
        /// Invest in infrastructure across the nation
        /// </summary>
        public float InvestInInfrastructure(List<RegionEntity> regions)
        {
            // Calculate investment amount as percentage of treasury
            float investmentAmount = TreasuryBalance * InfrastructureInvestment;
            
            if (investmentAmount <= 0 || regions.Count == 0)
                return 0;
            
            // Calculate per-region investment
            float investmentPerRegion = investmentAmount / regions.Count;
            
            foreach (var region in regions)
            {
                // Improve infrastructure (diminishing returns formula)
                float infrastructureGain = investmentPerRegion / (region.InfrastructureLevel * 10 + 1);
                region.InfrastructureLevel += infrastructureGain;
            }
            
            // Reduce treasury balance
            TreasuryBalance -= investmentAmount;
            
            return investmentAmount;
        }
        
        /// <summary>
        /// Get average GDP over recent history
        /// </summary>
        public float GetAverageGDP()
        {
            if (gdpHistory.Count == 0)
                return 0;
            
            float sum = 0;
            foreach (var gdp in gdpHistory)
            {
                sum += gdp;
            }
            
            return sum / gdpHistory.Count;
        }
        
        /// <summary>
        /// Process a fiscal turn for the nation
        /// </summary>
        public void ProcessTurn(List<RegionEntity> regions)
        {
            UpdateFromRegions(regions);
            CollectTaxes(regions);
            InvestInInfrastructure(regions);
            
            // Apply inflation to economy
            ApplyInflation(regions);
        }
        
        /// <summary>
        /// Apply inflation effects
        /// </summary>
        private void ApplyInflation(List<RegionEntity> regions)
        {
            // Simple inflation model: prices rise, effectively reducing wealth value
            foreach (var region in regions)
            {
                // Inflation slightly reduces effective wealth
                float wealthReduction = region.Wealth * Inflation;
                region.Wealth -= Mathf.RoundToInt(wealthReduction);
            }
        }
        
        /// <summary>
        /// Get a summary of the economic situation
        /// </summary>
        public string GetSummary()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            // Economic status descriptions
            string growthDesc = GDPGrowthRate > 0.05f ? "Strong Growth" :
                               GDPGrowthRate > 0.02f ? "Moderate Growth" :
                               GDPGrowthRate > 0.0f ? "Slow Growth" :
                               GDPGrowthRate > -0.02f ? "Stagnation" :
                               GDPGrowthRate > -0.05f ? "Recession" : "Depression";
                               
            string treasuryDesc = TreasuryBalance > 1000 ? "Well-Funded" :
                                 TreasuryBalance > 500 ? "Adequate" :
                                 TreasuryBalance > 200 ? "Limited" :
                                 TreasuryBalance > 50 ? "Strained" : "Empty";
            
            // Basic economic stats
            sb.AppendLine($"GDP: {GDP:F0} ({growthDesc}, {GDPGrowthRate:P1})");
            sb.AppendLine($"Treasury: {TreasuryBalance:F0} ({treasuryDesc})");
            sb.AppendLine($"Tax Rate: {TaxRate:P0}");
            sb.AppendLine($"Inflation: {Inflation:P1}");
            sb.AppendLine($"Infrastructure Investment: {InfrastructureInvestment:P0} of Treasury");
            
            // Wealth and production
            sb.AppendLine($"Total Wealth: {TotalWealth:F0}");
            sb.AppendLine($"Total Production: {TotalProduction:F0}");
            
            // GDP history if available
            if (gdpHistory.Count > 1)
            {
                sb.AppendLine("\nGDP Trend:");
                float[] gdpArray = gdpHistory.ToArray();
                for (int i = Mathf.Max(0, gdpArray.Length - 5); i < gdpArray.Length; i++)
                {
                    string year = (i - gdpArray.Length).ToString();
                    sb.AppendLine($"  Year {year}: {gdpArray[i]:F0}");
                }
            }
            
            return sb.ToString();
        }
    }
}