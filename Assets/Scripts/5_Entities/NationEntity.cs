using System.Collections.Generic;
using UnityEngine;
using Entities.Components;

namespace Entities
{
    public class NationEntity
    {
        // Basic properties
        public string Id { get; private set; }
        public string Name { get; private set; }
        public Color Color { get; private set; }
        
        // List of regions that belong to this nation
        private List<string> regionIds = new List<string>();
        
        // Components
        public NationEconomyComponent Economy { get; private set; }
        
        // Legacy properties - redirected to components for backward compatibility
        public float TotalWealth { 
            get => Economy.TotalWealth; 
            set => Debug.LogWarning("TotalWealth is now managed by Economy component") ; 
        }
        public float TotalProduction { 
            get => Economy.TotalProduction; 
            set => Debug.LogWarning("TotalProduction is now managed by Economy component"); 
        }
        public float Stability { get; set; } = 0.5f; // This will move to a StabilityComponent later
        
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
            
            // Initialize components
            Economy = new NationEconomyComponent();
            
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
                   $"Wealth: {Economy.TotalWealth}\n" +
                   $"Production: {Economy.TotalProduction}\n" +
                   $"GDP: {Economy.GDP:F0}\n" +
                   $"Growth: {Economy.GDPGrowthRate:P1}\n" +
                   $"Treasury: {Economy.TreasuryBalance:F0}\n" +
                   $"Stability: {Stability:P0}\n" +
                   $"Diplomatic Relations: {diplomaticRelations.Count}";
        }
    }
}