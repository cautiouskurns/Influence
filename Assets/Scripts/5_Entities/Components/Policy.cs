using System.Collections.Generic;
using UnityEngine;

namespace Entities.Components
{
    /// <summary>
    /// Represents a specific policy or reform that can be implemented by a nation
    /// </summary>
    public class Policy
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int Cost { get; private set; }
        public int TurnDuration { get; private set; }
        public int TurnsRemaining { get; private set; }
        
        // Effects of this policy on various aspects
        public float WealthEffect { get; private set; }
        public float ProductionEffect { get; private set; }
        public float StabilityEffect { get; private set; }
        
        public Policy(string name, string description, int cost, int duration, 
                      float wealthEffect = 0, float productionEffect = 0, float stabilityEffect = 0)
        {
            Name = name;
            Description = description;
            Cost = cost;
            TurnDuration = duration;
            TurnsRemaining = duration;
            
            WealthEffect = wealthEffect;
            ProductionEffect = productionEffect;
            StabilityEffect = stabilityEffect;
        }
        
        /// <summary>
        /// Update the remaining duration of this policy
        /// </summary>
        /// <returns>True if the policy is still active, false if it has expired</returns>
        public bool UpdateTurn()
        {
            if (TurnsRemaining <= 0)
                return false;
                
            TurnsRemaining--;
            return TurnsRemaining > 0;
        }
    }
}