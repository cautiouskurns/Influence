using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Entities.Components
{
    /// <summary>
    /// CLASS PURPOSE:
    /// Handles production calculations and management for a region
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Calculate total production capacity
    /// - Track production modifiers
    /// - Allocate production to different sectors
    /// </summary>
    public class ProductionComponent
    {
        // Base production value
        private float baseProduction;
        
        // Current production - now properly encapsulated
        public int Production { get; private set; }
        
        // Modifiers that affect production
        private Dictionary<string, float> productionModifiers = new Dictionary<string, float>();
        
        // Production allocation to different sectors (percentages)
        private Dictionary<string, float> sectorAllocation = new Dictionary<string, float>();
        
        /// <summary>
        /// Initialize with default values
        /// </summary>
        public ProductionComponent(float initialProduction = 50)
        {
            baseProduction = initialProduction;
            Production = Mathf.RoundToInt(initialProduction);
            
            // Add default modifiers
            productionModifiers.Add("Infrastructure", 1.0f);
            productionModifiers.Add("Technology", 1.0f);
            productionModifiers.Add("Workforce", 1.0f);
            
            // Default sector allocation (must sum to 1.0)
            sectorAllocation.Add("Agriculture", 0.4f);
            sectorAllocation.Add("Industry", 0.4f);
            sectorAllocation.Add("Commerce", 0.2f);
        }
        
        /// <summary>
        /// Set the production value directly (for use by the EconomicSystem)
        /// </summary>
        public void SetProduction(int value)
        {
            Production = Mathf.Max(0, value);
            baseProduction = value; // Update base production to match
        }
        
        /// <summary>
        /// Calculate total production with all modifiers applied
        /// </summary>
        public float CalculateTotalProduction()
        {
            float totalModifier = 1.0f;
            
            // Apply all modifiers
            foreach (var modifier in productionModifiers.Values)
            {
                totalModifier *= modifier;
            }
            
            return baseProduction * totalModifier;
        }
        
        /// <summary>
        /// Update the Production property based on current modifiers
        /// </summary>
        public void UpdateProduction()
        {
            Production = Mathf.RoundToInt(CalculateTotalProduction());
        }
        
        /// <summary>
        /// Update production by adding or subtracting a delta value
        /// </summary>
        public void UpdateProduction(int delta)
        {
            baseProduction = Mathf.Max(0, baseProduction + delta);
            UpdateProduction(); // Recalculate production with new base value
        }
        
        /// <summary>
        /// Set the base production value
        /// </summary>
        public void SetBaseProduction(float value)
        {
            baseProduction = Mathf.Max(0, value);
            UpdateProduction();
        }
        
        /// <summary>
        /// Get production amount allocated to a specific sector
        /// </summary>
        public float GetSectorProduction(string sector)
        {
            if (sectorAllocation.ContainsKey(sector))
            {
                return CalculateTotalProduction() * sectorAllocation[sector];
            }
            return 0;
        }
        
        /// <summary>
        /// Set production modifier
        /// </summary>
        public void SetProductionModifier(string modifierName, float value)
        {
            if (productionModifiers.ContainsKey(modifierName))
                productionModifiers[modifierName] = value;
            else
                productionModifiers.Add(modifierName, value);
            
            // Update production after modifiers change
            UpdateProduction();
        }
        
        /// <summary>
        /// Set sector allocation percentage
        /// </summary>
        public void SetSectorAllocation(string sector, float percentage)
        {
            // Ensure percentage is between 0 and 1
            percentage = Mathf.Clamp01(percentage);
            
            if (sectorAllocation.ContainsKey(sector))
                sectorAllocation[sector] = percentage;
            else
                sectorAllocation.Add(sector, percentage);
                
            // Normalize all allocations to ensure sum is 1.0
            NormalizeSectorAllocations();
        }
        
        /// <summary>
        /// Ensure all sector allocations sum to 1.0
        /// </summary>
        private void NormalizeSectorAllocations()
        {
            float total = 0;
            
            // Calculate current total
            foreach (var allocation in sectorAllocation.Values)
            {
                total += allocation;
            }
            
            // Skip if total is already 1.0 or no allocations exist
            if (total == 0 || Mathf.Approximately(total, 1.0f))
                return;
                
            // Normalize all values
            foreach (var sector in sectorAllocation.Keys.ToArray())
            {
                sectorAllocation[sector] /= total;
            }
        }
        
        /// <summary>
        /// Get a detailed summary of production
        /// </summary>
        public string GetSummary()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Production:");
            sb.AppendLine($"  Base Production: {baseProduction:F1}");
            sb.AppendLine($"  Current Production: {Production}");
            sb.AppendLine($"  Total Production: {CalculateTotalProduction():F1}");
            
            sb.AppendLine("Modifiers:");
            foreach (var modifier in productionModifiers)
            {
                sb.AppendLine($"  {modifier.Key}: x{modifier.Value:F2}");
            }
            
            sb.AppendLine("Sector Allocation:");
            foreach (var allocation in sectorAllocation)
            {
                float sectorProduction = GetSectorProduction(allocation.Key);
                sb.AppendLine($"  {allocation.Key}: {allocation.Value:P0} ({sectorProduction:F1})");
            }
            
            return sb.ToString();
        }
        
        /// <summary>
        /// Process production for one turn
        /// </summary>
        public void ProcessTurn()
        {
            // Update the production value based on current modifiers
            UpdateProduction();
            
            // Additional turn processing logic can be added here
        }
    }
}