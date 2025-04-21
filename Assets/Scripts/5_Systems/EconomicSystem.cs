using UnityEngine;
using Entities;
using Managers;
using System.Collections.Generic;
using System.Linq;

namespace Systems
{
    /// <summary>
    /// EconomicSystem processes economic calculations for all regions in the simulation.
    /// </summary>
    public class EconomicSystem : MonoBehaviour
    {
        [Header("Production Settings")]
        public float productionBase = 10.0f;
        public float productivityFactor = 1.0f;
        public float laborElasticity = 0.5f;
        public float capitalElasticity = 0.5f;
        
        // Debug/Testing
        [Header("Debug")]
        public bool autoRunSimulation = true;
        public RegionEntity testRegion;
        
        private Dictionary<string, RegionEntity> regions = new Dictionary<string, RegionEntity>();

        private void OnEnable()
        {
            EventBus.Subscribe("TurnEnded", OnTurnEnded);
            EventBus.Subscribe("RegionUpdated", OnRegionUpdated);
            EventBus.Subscribe("EconomicTick", OnEconomicTick);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe("TurnEnded", OnTurnEnded);
            EventBus.Unsubscribe("RegionUpdated", OnRegionUpdated);
            EventBus.Unsubscribe("EconomicTick", OnEconomicTick);
        }

        private void OnTurnEnded(object _)
        {
            if (autoRunSimulation)
            {
                ProcessEconomicTick();
            }
        }

        private void OnRegionUpdated(object data)
        {
            if (data is RegionEntity region)
            {
                RegisterRegion(region);
            }
        }

        private void OnEconomicTick(object data)
        {
            // Also update all map colors when the economy ticks
            EventBus.Trigger("UpdateMapColors", null);
        }

        [ContextMenu("Process Economic Tick")]
        public void ProcessEconomicTick()
        {
            if (regions.Count > 0)
            {
                List<RegionEntity> processedRegions = new List<RegionEntity>();
                
                foreach (var region in regions.Values)
                {
                    ProcessRegion(region);
                    processedRegions.Add(region);
                }
                
                // Notify that economic processing is complete
                EventBus.Trigger("EconomicTick", regions.Count);
                
                // Notify individual region updates
                foreach (var region in processedRegions)
                {
                    EventBus.Trigger("RegionUpdated", region);
                }
                
                return;
            }
            
            Debug.LogWarning("No regions available for economic processing");
        }

        private void ProcessRegion(RegionEntity region)
        {
            if (region == null) return;
            
            // Calculate production using Cobb-Douglas function
            CalculateProduction(region);
            
            // Update wealth based on production
            region.Wealth += Mathf.RoundToInt(region.Production * 0.1f);
            
//            Debug.Log($"Processed economic tick for {region.Name}: Production = {region.Production}, Wealth = {region.Wealth}");
        }
        
        /// <summary>
        /// Calculates production using the Cobb-Douglas production function
        /// </summary>
        private void CalculateProduction(RegionEntity region)
        {
            // Get inputs for production calculation
            float labor = region.LaborAvailable;
            float capital = region.InfrastructureLevel;
            
            // Guard against zero values to prevent NaN results
            labor = Mathf.Max(1f, labor);
            capital = Mathf.Max(1f, capital);
            
            // Cobb-Douglas production function: Y = A * L^α * K^β
            float production = productivityFactor * 
                Mathf.Pow(labor, laborElasticity) * 
                Mathf.Pow(capital, capitalElasticity);
            
            // Update region's production value
            region.Production = Mathf.RoundToInt(production);
            
//            Debug.Log($"[Production] {region.Name} - Labor: {labor}, Capital: {capital}, " +
//                      $"Output: {region.Production}");
        }
        
        public void RegisterRegion(RegionEntity region)
        {
            if (region == null) return;
            
            if (!regions.ContainsKey(region.Name))
            {
                regions.Add(region.Name, region);
//                Debug.Log($"Region registered: {region.Name}");
            }
        }
        
        public RegionEntity GetRegion(string regionName)
        {
            if (regions.TryGetValue(regionName, out RegionEntity region))
            {
                return region;
            }
            return null;
        }
        
        public List<RegionEntity> GetAllRegions()
        {
            return regions.Values.ToList();
        }

        public List<string> GetAllRegionIds()
        {
            return regions.Keys.ToList();
        }

        /// <summary>
        /// Update an existing region entity
        /// </summary>
        /// <param name="region">The updated region entity</param>
        public void UpdateRegion(RegionEntity region)
        {
            if (region == null) return;
            
            if (regions.ContainsKey(region.Name))
            {
                regions[region.Name] = region;
                
                // Trigger an event to notify listeners of the update
                EventBus.Trigger("RegionUpdated", region);
            }
            else
            {
                // If region doesn't exist yet, register it
                RegisterRegion(region);
            }
        }

        /// <summary>
        /// Gets the total wealth across all regions in the economic system
        /// </summary>
        /// <returns>The sum of wealth from all regions</returns>
        public int GetTotalWealth()
        {
            int totalWealth = 0;
            
            foreach (var region in regions.Values)
            {
                if (region != null)
                {
                    totalWealth += region.Wealth;
                }
            }
            
            return totalWealth;
        }
    }
}