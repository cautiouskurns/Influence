using System.Collections.Generic;
using Systems;
using Entities;

namespace Core.Interfaces
{
    /// <summary>
    /// Interface for the Economic System, allowing for looser coupling and easier testing.
    /// </summary>
    public interface IEconomicSystem
    {
        /// <summary>
        /// Get a region by its ID
        /// </summary>
        /// <param name="regionId">The ID of the region to retrieve</param>
        /// <returns>The region object, or null if not found</returns>
        RegionEntity GetRegion(string regionId);
        
        /// <summary>
        /// Get all region IDs in the system
        /// </summary>
        /// <returns>Collection of all region IDs</returns>
        IEnumerable<string> GetAllRegionIds();
        
        /// <summary>
        /// Update a region's data in the system
        /// </summary>
        /// <param name="region">The updated region data</param>
        void UpdateRegion(RegionEntity region);
    }
}