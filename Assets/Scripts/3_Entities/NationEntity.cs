using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
    public class NationEntity
    {
        // Basic properties
        public string Id { get; set; }
        public string Name { get; set; }
        public Color NationColor { get; set; }
        
        // List of regions that belong to this nation
        private List<string> regionIds = new List<string>();
        
        // Nation-level statistics
        public float TotalWealth { get; private set; }
        public float TotalProduction { get; private set; }
        public float Stability { get; private set; } = 0.5f; // 0.0 to 1.0 scale, default is neutral
        
        // Basic diplomatic status (can be expanded later)
        public enum DiplomaticStatus { Allied, Neutral, Hostile }
        
        // Basic policy types
        public enum PolicyType { Economic, Diplomatic, Military, Social }
        
        // Dictionary to track diplomatic relations with other nations
        private Dictionary<string, DiplomaticStatus> diplomaticRelations = new Dictionary<string, DiplomaticStatus>();
        
        // Dictionary to store policy settings
        private Dictionary<PolicyType, float> policySettings = new Dictionary<PolicyType, float>();

        public NationEntity(string id, string name, Color color)
        {
            Id = id;
            Name = name;
            NationColor = color;
            
            // Initialize default policy settings (0.5 is neutral/balanced)
            policySettings[PolicyType.Economic] = 0.5f;
            policySettings[PolicyType.Diplomatic] = 0.5f;
            policySettings[PolicyType.Military] = 0.5f;
            policySettings[PolicyType.Social] = 0.5f;
        }
        
        // Region management
        public void AddRegion(string regionId)
        {
            if (!regionIds.Contains(regionId))
            {
                regionIds.Add(regionId);
                Debug.Log($"Added region {regionId} to nation {Name}");
            }
        }
        
        public void RemoveRegion(string regionId)
        {
            if (regionIds.Contains(regionId))
            {
                regionIds.Remove(regionId);
                Debug.Log($"Removed region {regionId} from nation {Name}");
            }
        }
        
        public List<string> GetRegionIds()
        {
            return new List<string>(regionIds);
        }
        
        // Diplomacy methods
        public void SetDiplomaticStatus(string otherNationId, DiplomaticStatus status)
        {
            diplomaticRelations[otherNationId] = status;
            Debug.Log($"Set diplomatic status with {otherNationId} to {status}");
        }
        
        public DiplomaticStatus GetDiplomaticStatus(string otherNationId)
        {
            if (diplomaticRelations.TryGetValue(otherNationId, out DiplomaticStatus status))
            {
                return status;
            }
            return DiplomaticStatus.Neutral; // Default to neutral
        }
        
        // Policy methods
        public void SetPolicy(PolicyType policyType, float value)
        {
            // Clamp value between 0 and 1
            value = Mathf.Clamp01(value);
            policySettings[policyType] = value;
            Debug.Log($"Set {policyType} policy for {Name} to {value:F2}");
        }
        
        public float GetPolicy(PolicyType policyType)
        {
            if (policySettings.TryGetValue(policyType, out float value))
            {
                return value;
            }
            return 0.5f; // Default to balanced
        }
        
        // Method to update nation statistics based on owned regions
        public void UpdateStatistics(Dictionary<string, RegionEntity> regions)
        {
            if (regionIds.Count == 0)
            {
                TotalWealth = 0;
                TotalProduction = 0;
                return;
            }
            
            float wealth = 0;
            float production = 0;
            
            foreach (string regionId in regionIds)
            {
                if (regions.TryGetValue(regionId, out RegionEntity region))
                {
                    wealth += region.Wealth;
                    production += region.Production;
                }
            }
            
            TotalWealth = wealth;
            TotalProduction = production;
            
            // Very simple stability calculation - can be expanded later
            CalculateStability();
            
            Debug.Log($"Updated statistics for {Name}: Wealth={TotalWealth}, Production={TotalProduction}, Stability={Stability:F2}");
        }
        
        // Simple stability calculation
        private void CalculateStability()
        {
            // Base stability
            float baseStability = 0.5f;
            
            // Simple factors that affect stability
            float wealthFactor = TotalWealth > 0 ? Mathf.Min(0.2f, TotalWealth / 1000f) : -0.2f;
            float regionFactor = 0.1f * Mathf.Min(1, regionIds.Count / 5f); // More regions = more stability, up to a point
            
            // Policy impact
            float policyImpact = (GetPolicy(PolicyType.Social) - 0.5f) * 0.2f; // Social policy impacts stability
            
            // Calculate final stability
            Stability = Mathf.Clamp01(baseStability + wealthFactor + regionFactor + policyImpact);
        }
        
        // Very basic nation summary
        public string GetSummary()
        {
            return $"Nation: {Name}\n" +
                   $"Regions: {regionIds.Count}\n" +
                   $"Wealth: {TotalWealth}\n" +
                   $"Production: {TotalProduction}\n" +
                   $"Stability: {Stability:P0}\n" +
                   $"Diplomatic Relations: {diplomaticRelations.Count}";
        }
    }
}