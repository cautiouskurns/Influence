using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
    public class NationEntity
    {
        // Basic properties
        public string Id { get; private set; }
        public string Name { get; private set; }
        public Color Color { get; private set; } // Changed from NationColor to match usage in NationView
        
        // List of regions that belong to this nation
        private List<string> regionIds = new List<string>();
        
        // Nation-level statistics - these are stored here but calculated by NationSystem
        public float TotalWealth { get; set; }
        public float TotalProduction { get; set; }
        public float Stability { get; set; } = 0.5f; // 0.0 to 1.0 scale, default is neutral
        
        // Policy system
        public enum PolicyType { Economic, Diplomatic, Military, Social }
        private Dictionary<PolicyType, float> policies = new Dictionary<PolicyType, float>();
        
        // Basic diplomatic status (can be expanded later)
        public enum DiplomaticStatus { Allied, Neutral, Hostile }
        
        // Dictionary to track diplomatic relations with other nations
        private Dictionary<string, DiplomaticStatus> diplomaticRelations = new Dictionary<string, DiplomaticStatus>();

        public NationEntity(string id, string name, Color color)
        {
            Id = id;
            Name = name;
            Color = color;
            
            // Initialize policies with default values (0.5 = balanced)
            SetPolicy(PolicyType.Economic, 0.5f);
            SetPolicy(PolicyType.Diplomatic, 0.5f);
            SetPolicy(PolicyType.Military, 0.5f);
            SetPolicy(PolicyType.Social, 0.5f);
        }
        
        // Region management
        public void AddRegion(string regionId)
        {
            if (!regionIds.Contains(regionId))
            {
                regionIds.Add(regionId);
            }
        }
        
        public void RemoveRegion(string regionId)
        {
            if (regionIds.Contains(regionId))
            {
                regionIds.Remove(regionId);
            }
        }
        
        public List<string> GetRegionIds()
        {
            return new List<string>(regionIds);
        }
        
        // Policy methods
        public void SetPolicy(PolicyType type, float value)
        {
            // Clamp value between 0 and 1
            value = Mathf.Clamp01(value);
            policies[type] = value;
        }
        
        public float GetPolicy(PolicyType type)
        {
            if (policies.TryGetValue(type, out float value))
            {
                return value;
            }
            return 0.5f; // Default to balanced
        }
        
        // Diplomacy methods
        public void SetDiplomaticStatus(string otherNationId, DiplomaticStatus status)
        {
            diplomaticRelations[otherNationId] = status;
        }
        
        public DiplomaticStatus GetDiplomaticStatus(string otherNationId)
        {
            if (diplomaticRelations.TryGetValue(otherNationId, out DiplomaticStatus status))
            {
                return status;
            }
            return DiplomaticStatus.Neutral; // Default to neutral
        }
        
        // Basic nation summary
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