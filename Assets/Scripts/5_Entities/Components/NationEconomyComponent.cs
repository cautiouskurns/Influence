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
    }
}