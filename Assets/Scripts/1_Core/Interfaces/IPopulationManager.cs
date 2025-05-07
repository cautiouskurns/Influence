using UnityEngine;
using System.Collections.Generic;

namespace Core.Interfaces
{
    /// <summary>
    /// Interface for managing global population statistics and policies
    /// Decouples population management for better testing and modularity
    /// </summary>
    public interface IPopulationManager
    {
        /// <summary>
        /// Get the current population growth rate
        /// </summary>
        float GetPopulationGrowthRate();
        
        /// <summary>
        /// Get the current migration rate
        /// </summary>
        float GetMigrationRate();
        
        /// <summary>
        /// Get the total population across all regions
        /// </summary>
        int GetTotalPopulation();
        
        /// <summary>
        /// Get average satisfaction level across all regions (0-1)
        /// </summary>
        float GetAverageSatisfaction();
        
        /// <summary>
        /// Apply growth calculations to all regions
        /// </summary>
        void ApplyPopulationGrowth();
        
        /// <summary>
        /// Force recalculation of total population
        /// </summary>
        /// <param name="rebuildCache">Whether to rebuild the population cache</param>
        void CalculateTotalPopulation(bool rebuildCache);
    }
}