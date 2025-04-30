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
        public NationPolicyComponent Policy { get; private set; }
        public NationDiplomacyComponent Diplomacy { get; private set; }
        
        // Legacy properties - redirected to components for backward compatibility
        public float TotalWealth { 
            get => Economy.TotalWealth; 
            set => Debug.LogWarning("TotalWealth is now managed by Economy component"); 
        }
        public float TotalProduction { 
            get => Economy.TotalProduction; 
            set => Debug.LogWarning("TotalProduction is now managed by Economy component"); 
        }
        public float Stability { get; set; } = 0.5f; // This will move to a StabilityComponent later
        
        // Basic diplomatic status (can be expanded later)
        public enum PolicyType { Economic, Diplomatic, Military, Social }
        public enum DiplomaticStatus { Allied, Neutral, Hostile }

        public NationEntity(string id, string name, Color color)
        {
            Id = id;
            Name = name;
            Color = color;
            
            // Initialize components
            Economy = new NationEconomyComponent();
            Policy = new NationPolicyComponent();
            Diplomacy = new NationDiplomacyComponent();
            
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
        
        // Policy methods - now delegated to Policy component
        public void SetPolicy(PolicyType type, float value)
        {
            Policy.SetPolicy(type, value);
        }
        
        public float GetPolicy(PolicyType type)
        {
            return Policy.GetPolicy(type);
        }
        
        /// <summary>
        /// Implement a new policy/reform
        /// </summary>
        public bool ImplementPolicy(Policy policy)
        {
            return Policy.ImplementPolicy(policy, Economy.TreasuryBalance);
        }
        
        /// <summary>
        /// Apply policy effects to all regions
        /// </summary>
        public void ApplyPolicyEffects(List<RegionEntity> regions)
        {
            Policy.ApplyPolicyEffects(regions);
        }
        
        // Diplomacy methods - delegated to Diplomacy component
        public void SetDiplomaticStatus(string otherNationId, DiplomaticStatus status)
        {
            Diplomacy.SetDiplomaticStatus(otherNationId, status);
        }
        
        public DiplomaticStatus GetDiplomaticStatus(string otherNationId)
        {
            return Diplomacy.GetDiplomaticStatus(otherNationId);
        }
        
        public DiplomaticRelation GetDiplomaticRelation(string otherNationId)
        {
            return Diplomacy.GetDiplomaticRelation(otherNationId);
        }
        
        public void ApplyDiplomaticEvent(string otherNationId, float reputationChange, string description = "")
        {
            Diplomacy.ApplyDiplomaticEvent(otherNationId, reputationChange, description);
        }
        
        public bool CanPerformDiplomaticAction(string otherNationId, DiplomaticActionType actionType)
        {
            return Diplomacy.CanPerformAction(otherNationId, actionType);
        }
        
        /// <summary>
        /// Process a turn for this nation
        /// </summary>
        public void ProcessTurn(List<RegionEntity> regions)
        {
            // Process economy
            Economy.ProcessTurn(regions);
            
            // Process policy effects
            Policy.ProcessTurn();
            Policy.ApplyPolicyEffects(regions);
            
            // Process diplomacy
            Diplomacy.ProcessTurn();
        }
        
        // Basic nation summary
        public string GetSummary()
        {
            string summary = $"Nation: {Name}\n" +
                   $"Regions: {regionIds.Count}\n" +
                   $"Wealth: {Economy.TotalWealth}\n" +
                   $"Production: {Economy.TotalProduction}\n" +
                   $"GDP: {Economy.GDP:F0}\n" +
                   $"Growth: {Economy.GDPGrowthRate:P1}\n" +
                   $"Treasury: {Economy.TreasuryBalance:F0}\n" +
                   $"Stability: {Stability:P0}\n" +
                   $"Diplomatic Relations: {Diplomacy.GetDiplomaticRelationCount()}\n\n";
            
            // Add policy summary   
            summary += Policy.GetSummary() + "\n\n";
            
            // Add diplomacy summary
            summary += Diplomacy.GetSummary();
            
            return summary;
        }
        
        /// <summary>
        /// Create a standard policy
        /// </summary>
        public static Policy CreateStandardPolicy(string name, string description, int cost, int duration,
                                                float wealthEffect, float productionEffect, float stabilityEffect)
        {
            return NationPolicyComponent.CreateStandardPolicy(name, description, cost, duration, 
                                                            wealthEffect, productionEffect, stabilityEffect);
        }
    }
}