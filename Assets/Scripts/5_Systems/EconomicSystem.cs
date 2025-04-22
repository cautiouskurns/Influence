using UnityEngine;
using Entities;
using Managers;
using Systems.Economics;
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
        public float productivityFactor = 1.0f;
        public float laborElasticity = 0.5f;
        public float capitalElasticity = 0.5f;
        
        [Header("Infrastructure Settings")]
        public float efficiencyModifier = 0.1f;
        public float decayRate = 0.02f;
        public float maintenanceCostFactor = 0.05f;
        
        [Header("Consumption Settings")]
        public float baseConsumptionRate = 0.2f;
        public float wealthConsumptionExponent = 0.8f;
        public float unmetDemandUnrestFactor = 0.05f;
        
        [Header("Economic Cycle Settings")]
        public int cycleLength = 12;
        public bool enableEconomicCycles = true;
        
        [Header("Resource Types")]
        public List<string> resourceTypes = new List<string>() { "Food", "Luxury", "RawMaterial", "Manufacturing" };
        
        [Header("Debug")]
        public bool autoRunSimulation = true;
        public bool showDebugLogs = false;
        public RegionEntity testRegion;
        
        // Dictionary to store all region entities
        private Dictionary<string, RegionEntity> regions = new Dictionary<string, RegionEntity>();
        
        // Economic calculators
        private ProductionCalculator productionCalculator;
        private InfrastructureCalculator infrastructureCalculator;
        private PriceCalculator priceCalculator;
        private ConsumptionCalculator consumptionCalculator;
        private EconomicCycleCalculator cycleCalculator;
        
        // Resource data
        private Dictionary<string, float> resourcePrices = new Dictionary<string, float>();
        private Dictionary<string, float> globalSupply = new Dictionary<string, float>();
        private Dictionary<string, float> globalDemand = new Dictionary<string, float>();

        private void Awake()
        {
            InitializeCalculators();
            InitializeResourceData();
        }
        
        private void InitializeCalculators()
        {
            // Initialize all economic calculators
            productionCalculator = new ProductionCalculator(
                productivityFactor,
                laborElasticity,
                capitalElasticity
            );
            
            infrastructureCalculator = new InfrastructureCalculator(
                efficiencyModifier,
                decayRate,
                maintenanceCostFactor
            );
            
            priceCalculator = new PriceCalculator();
            
            consumptionCalculator = new ConsumptionCalculator(
                baseConsumptionRate,
                wealthConsumptionExponent,
                unmetDemandUnrestFactor
            );
            
            cycleCalculator = new EconomicCycleCalculator(cycleLength);
            
            if (showDebugLogs)
                Debug.Log("Economic calculators initialized");
        }
        
        private void InitializeResourceData()
        {
            // Set initial prices and zero out supply/demand
            foreach (string resourceType in resourceTypes)
            {
                resourcePrices[resourceType] = 100f; // Base price of 100
                globalSupply[resourceType] = 0f;
                globalDemand[resourceType] = 0f;
            }
            
            if (showDebugLogs)
                Debug.Log("Resource data initialized");
        }

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
            // Advance economic cycle
            if (enableEconomicCycles)
            {
                cycleCalculator.AdvanceCycle();
                
                if (showDebugLogs)
                {
                    Debug.Log($"Economic Cycle: {cycleCalculator.GetCurrentPhase()} - {cycleCalculator.GetEconomicConditionDescription()}");
                }
            }
            
            // Also update all map colors when the economy ticks
            EventBus.Trigger("UpdateMapColors", null);
        }

        [ContextMenu("Process Economic Tick")]
        public void ProcessEconomicTick()
        {
            if (regions.Count > 0)
            {
                // Reset global supply and demand
                ResetGlobalEconomicTracking();
                
                // First pass: calculate production and supply
                CalculateProduction();
                
                // Second pass: calculate consumption, demand and price effects
                CalculateDemandAndConsumption();
                
                // Third pass: update prices based on supply and demand
                UpdatePrices();
                
                // Notify that economic processing is complete
                EventBus.Trigger("EconomicTick", regions.Count);
                
                // Notify individual region updates
                foreach (var region in regions.Values)
                {
                    EventBus.Trigger("RegionUpdated", region);
                }
                
                return;
            }
            
            Debug.LogWarning("No regions available for economic processing");
        }
        
        private void ResetGlobalEconomicTracking()
        {
            // Reset global supply and demand for all resource types
            foreach (string resourceType in resourceTypes)
            {
                globalSupply[resourceType] = 0f;
                globalDemand[resourceType] = 0f;
            }
        }
        
        private void CalculateProduction()
        {
            foreach (var region in regions.Values)
            {
                // Calculate base production
                float baseProduction = productionCalculator.CalculateRegionProduction(region);
                
                // Apply economic cycle effects if enabled
                float cycleFactor = 1.0f;
                if (enableEconomicCycles)
                {
                    cycleFactor = cycleCalculator.ApplyCycleEffect(1.0f, "Production");
                }
                
                // Apply infrastructure efficiency boost
                float efficiencyBoost = infrastructureCalculator.CalculateEfficiencyBoost(region.InfrastructureLevel);
                
                // Calculate final production with all factors
                float finalProduction = baseProduction * efficiencyBoost * cycleFactor;
                
                // Update region production
                region.Production = Mathf.RoundToInt(finalProduction);
                
                // Contribute to global supply based on production
                // Simplified: assume equal distribution across resource types
                float productionPerResourceType = finalProduction / resourceTypes.Count;
                foreach (string resourceType in resourceTypes)
                {
                    globalSupply[resourceType] += productionPerResourceType;
                }
                
                if (showDebugLogs && region == testRegion)
                {
                    Debug.Log($"Region {region.Name} Production: {region.Production} " +
                        $"(Base: {baseProduction}, Efficiency: {efficiencyBoost}, Cycle: {cycleFactor})");
                }
            }
        }
        
        private void CalculateDemandAndConsumption()
        {
            // Create a preference map for resource consumption
            Dictionary<string, float> defaultResourceAllocation = new Dictionary<string, float>();
            foreach (string resourceType in resourceTypes)
            {
                defaultResourceAllocation[resourceType] = 1.0f / resourceTypes.Count; // Equal distribution by default
            }
            
            foreach (var region in regions.Values)
            {
                // Available resources for consumption
                Dictionary<string, float> availableResources = new Dictionary<string, float>();
                foreach (string resourceType in resourceTypes)
                {
                    // Simplified: proportional share of global supply based on region's wealth
                    float totalWealth = GetTotalWealth();
                    float wealthShare = totalWealth > 0 ? region.Wealth / (float)totalWealth : 0;
                    availableResources[resourceType] = globalSupply[resourceType] * wealthShare;
                }
                
                // Calculate consumption and unmet demand
                var (consumption, unmetDemand, unrest) = consumptionCalculator.ProcessRegionConsumption(
                    region, availableResources, defaultResourceAllocation);
                
                // Apply economic cycle effects to consumption
                if (enableEconomicCycles)
                {
                    consumption = cycleCalculator.ApplyCycleEffect(consumption, "Consumption");
                    unrest = cycleCalculator.ApplyCycleEffect(unrest, "Unrest");
                }
                
                // Update region wealth based on consumption
                region.Wealth -= Mathf.RoundToInt(consumption);
                
                // Ensure wealth doesn't go negative
                region.Wealth = Mathf.Max(0, region.Wealth);
                
                // Add wealth from production (simplified)
                region.Wealth += Mathf.RoundToInt(region.Production * 0.1f);
                
                // Update unrest from unmet demand
                //region.Unrest += Mathf.RoundToInt(unrest);
                
                // Contribute to global demand
                foreach (string resourceType in resourceTypes)
                {
                    // Adjust demand by income for each resource type
                    float baseDemand = consumption * defaultResourceAllocation[resourceType];
                    float adjustedDemand = priceCalculator.AdjustDemandByIncome(
                        baseDemand, region.Wealth, resourceType);
                    
                    globalDemand[resourceType] += adjustedDemand;
                }
                
                if (showDebugLogs && region == testRegion)
                {
                    Debug.Log($"Region {region.Name} Consumption: {consumption}, " +
                        $"Unmet Demand: {unmetDemand * 100:F1}%, Unrest: +{unrest:F1}");
                }
            }
        }
        
        private void UpdatePrices()
        {
            // Random market volatility (simplified)
            System.Random random = new System.Random();
            
            foreach (string resourceType in resourceTypes)
            {
                float supply = globalSupply[resourceType];
                float demand = globalDemand[resourceType];
                float oldPrice = resourcePrices[resourceType];
                
                // Calculate base price using supply and demand
                float newPrice = priceCalculator.CalculatePrice(
                    oldPrice, supply, demand, resourceType);
                
                // Apply economic cycle effects to price
                if (enableEconomicCycles)
                {
                    float inflationFactor = cycleCalculator.ApplyCycleEffect(1.0f, "PriceInflation");
                    newPrice *= inflationFactor;
                }
                
                // Apply random market shock (simplified)
                float supplyShock = (float)(random.NextDouble() * 0.2 - 0.1); // -10% to +10%
                float demandTrend = (float)(random.NextDouble() * 0.1 - 0.05); // -5% to +5%
                
                newPrice = priceCalculator.CalculatePriceShock(
                    newPrice, supplyShock, demandTrend, 0.2f);
                
                // Update resource price
                resourcePrices[resourceType] = newPrice;
                
                if (showDebugLogs)
                {
                    Debug.Log($"Resource {resourceType}: Supply={supply:F1}, Demand={demand:F1}, " +
                        $"Price={newPrice:F1} (changed from {oldPrice:F1})");
                }
            }
        }
        
        /// <summary>
        /// Gets the current price for a resource type
        /// </summary>
        public float GetResourcePrice(string resourceType)
        {
            if (resourcePrices.TryGetValue(resourceType, out float price))
            {
                return price;
            }
            return 100f; // Default price if not found
        }
        
        /// <summary>
        /// Gets the current economic cycle phase
        /// </summary>
        public EconomicCycleCalculator.CyclePhase GetCurrentEconomicPhase()
        {
            return cycleCalculator.GetCurrentPhase();
        }
        
        /// <summary>
        /// Gets a description of current economic conditions
        /// </summary>
        public string GetEconomicConditionDescription()
        {
            return cycleCalculator.GetEconomicConditionDescription();
        }

        // Region management methods
        public void RegisterRegion(RegionEntity region)
        {
            if (region == null) return;
            
            if (!regions.ContainsKey(region.Name))
            {
                regions.Add(region.Name, region);
                if (showDebugLogs)
                    Debug.Log($"Region registered: {region.Name}");
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
        
        // Inspector debug helpers
        [ContextMenu("Debug Current Economic Cycle")]
        public void DebugEconomicCycle()
        {
            Debug.Log($"Current Economic Phase: {cycleCalculator.GetCurrentPhase()}");
            Debug.Log($"Description: {cycleCalculator.GetEconomicConditionDescription()}");
            Debug.Log($"Turn: {cycleCalculator.GetCurrentTurn()}, Progress: {cycleCalculator.GetPhaseProgress() * 100:F1}%");
        }
        
        [ContextMenu("Debug Resource Prices")]
        public void DebugResourcePrices()
        {
            Debug.Log("Current Resource Prices:");
            foreach (var resource in resourcePrices)
            {
                Debug.Log($"- {resource.Key}: {resource.Value:F2}");
            }
        }
    }
}