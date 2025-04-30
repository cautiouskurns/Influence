using System.Collections.Generic;
using UnityEngine;

namespace Entities.Components
{
    /// <summary>
    /// CLASS PURPOSE:
    /// Manages the stability and internal affairs of a nation.
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Track overall stability and unrest
    /// - Handle stability-affecting events
    /// - Manage unrest factors and their effects
    /// </summary>
    public class NationStabilityComponent
    {
        // Stability metrics
        public float Stability { get; private set; } = 0.5f;        // 0.0 to 1.0 scale (0 = collapse, 1 = perfect)
        public float UnrestLevel { get; private set; } = 0.0f;      // 0.0 to 1.0 scale (0 = none, 1 = revolt)
        
        // Unrest factors
        private Dictionary<string, UnrestFactor> unrestFactors = new Dictionary<string, UnrestFactor>();
        
        // Event tracking
        private List<StabilityEvent> recentEvents = new List<StabilityEvent>();
        private readonly int maxRecentEvents = 10;
        
        // Constants
        private const float RevoltThreshold = 0.8f;             // Unrest level that triggers revolt
        private const float NaturalStabilityRecovery = 0.01f;   // How much stability recovers per turn naturally
        
        /// <summary>
        /// Constructor with optional initial stability value
        /// </summary>
        public NationStabilityComponent(float initialStability = 0.5f)
        {
            Stability = Mathf.Clamp01(initialStability);
        }
        
        /// <summary>
        /// Apply a stability impact from an event
        /// </summary>
        public void ApplyStabilityEvent(float impact, string source, string description = "")
        {
            // Apply the impact to stability
            ModifyStability(impact);
            
            // Create an event record
            StabilityEvent newEvent = new StabilityEvent(
                impact,
                source,
                description
            );
            
            recentEvents.Add(newEvent);
            
            // Trim events list if needed
            if (recentEvents.Count > maxRecentEvents)
            {
                recentEvents.RemoveAt(0);
            }
            
            Debug.Log($"Stability Event: {description} from {source} ({impact:+0.00;-0.00})");
        }
        
        /// <summary>
        /// Add or update an unrest factor
        /// </summary>
        public void SetUnrestFactor(string factorId, string description, float severity)
        {
            if (string.IsNullOrEmpty(factorId))
                return;
                
            severity = Mathf.Clamp01(severity);
            
            // If factor already exists, update it
            if (unrestFactors.TryGetValue(factorId, out UnrestFactor factor))
            {
                factor.Severity = severity;
                factor.Description = description;
            }
            else
            {
                // Create a new factor
                factor = new UnrestFactor(factorId, description, severity);
                unrestFactors.Add(factorId, factor);
            }
            
            // Recalculate unrest level
            RecalculateUnrestLevel();
            
            Debug.Log($"Unrest Factor: {description} set to {severity:F2}");
        }
        
        /// <summary>
        /// Remove an unrest factor
        /// </summary>
        public void RemoveUnrestFactor(string factorId)
        {
            if (unrestFactors.Remove(factorId))
            {
                // Recalculate unrest level
                RecalculateUnrestLevel();
                Debug.Log($"Unrest Factor: {factorId} removed");
            }
        }
        
        /// <summary>
        /// Check if a revolt is occurring
        /// </summary>
        public bool IsRevoltOccurring()
        {
            return UnrestLevel >= RevoltThreshold;
        }
        
        /// <summary>
        /// Process a turn for the stability component
        /// </summary>
        public void ProcessTurn()
        {
            // Natural stability recovery (if unrest is low)
            if (UnrestLevel < 0.3f)
            {
                ModifyStability(NaturalStabilityRecovery);
            }
            
            // Decay each unrest factor slightly
            List<string> factorsToRemove = new List<string>();
            foreach (var factor in unrestFactors.Values)
            {
                factor.Severity = Mathf.Max(0, factor.Severity - 0.02f);
                
                // If factor has decayed to insignificance, mark for removal
                if (factor.Severity < 0.01f)
                {
                    factorsToRemove.Add(factor.Id);
                }
            }
            
            // Remove decayed factors
            foreach (var id in factorsToRemove)
            {
                unrestFactors.Remove(id);
            }
            
            // Recalculate unrest
            RecalculateUnrestLevel();
        }
        
        /// <summary>
        /// Apply stability effects from policies
        /// </summary>
        public void ApplyPolicyEffects(Dictionary<NationEntity.PolicyType, float> policies)
        {
            if (policies == null)
                return;
                
            // Example: Social policy affects stability
            if (policies.TryGetValue(NationEntity.PolicyType.Social, out float socialPolicy))
            {
                // High social spending increases stability
                if (socialPolicy > 0.7f)
                {
                    ModifyStability(0.01f);
                }
            }
        }
        
        /// <summary>
        /// Apply stability effects from economic conditions
        /// </summary>
        public void ApplyEconomicEffects(float gdpGrowth, float wealthPerCapita)
        {
            // Economic growth improves stability
            if (gdpGrowth > 0.05f)  // 5% growth
            {
                ModifyStability(0.02f);
            }
            else if (gdpGrowth < -0.05f)  // 5% contraction
            {
                ModifyStability(-0.03f);
            }
            
            // Wealth per capita affects stability
            if (wealthPerCapita < 10)  // Very poor
            {
                SetUnrestFactor("poverty", "Widespread Poverty", 0.5f);
            }
            else if (wealthPerCapita > 50)  // Wealthy
            {
                RemoveUnrestFactor("poverty");
            }
        }
        
        /// <summary>
        /// Modify stability value with clamping
        /// </summary>
        private void ModifyStability(float change)
        {
            Stability = Mathf.Clamp01(Stability + change);
        }
        
        /// <summary>
        /// Recalculate the overall unrest level based on active factors
        /// </summary>
        private void RecalculateUnrestLevel()
        {
            if (unrestFactors.Count == 0)
            {
                UnrestLevel = 0;
                return;
            }
            
            // Calculate weighted unrest
            float totalUnrest = 0;
            float highestUnrest = 0;
            
            foreach (var factor in unrestFactors.Values)
            {
                totalUnrest += factor.Severity;
                highestUnrest = Mathf.Max(highestUnrest, factor.Severity);
            }
            
            // Formula: 70% of the highest unrest + 30% of the average unrest
            float averageUnrest = totalUnrest / unrestFactors.Count;
            UnrestLevel = (highestUnrest * 0.7f) + (averageUnrest * 0.3f);
            
            // Stability reduces unrest
            UnrestLevel *= (1f - (Stability * 0.5f));
            
            // Ensure unrest is in valid range
            UnrestLevel = Mathf.Clamp01(UnrestLevel);
        }
        
        /// <summary>
        /// Get a summary of the stability component
        /// </summary>
        public string GetSummary()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            string stabilityDesc = Stability > 0.8f ? "Excellent" :
                                   Stability > 0.6f ? "Good" :
                                   Stability > 0.4f ? "Average" :
                                   Stability > 0.2f ? "Poor" : "Critical";
                                   
            string unrestDesc = UnrestLevel < 0.2f ? "Minimal" :
                               UnrestLevel < 0.4f ? "Minor" :
                               UnrestLevel < 0.6f ? "Significant" :
                               UnrestLevel < 0.8f ? "Severe" : "Revolutionary";
            
            sb.AppendLine($"Stability: {stabilityDesc} ({Stability:P0})");
            sb.AppendLine($"Unrest: {unrestDesc} ({UnrestLevel:P0})");
            
            if (unrestFactors.Count > 0)
            {
                sb.AppendLine("\nUnrest Factors:");
                foreach (var factor in unrestFactors.Values)
                {
                    string severityDesc = factor.Severity > 0.7f ? "Critical" :
                                         factor.Severity > 0.4f ? "Serious" : "Minor";
                    sb.AppendLine($"- {factor.Description}: {severityDesc} ({factor.Severity:P0})");
                }
            }
            
            if (recentEvents.Count > 0)
            {
                sb.AppendLine("\nRecent Events:");
                for (int i = recentEvents.Count - 1; i >= 0; i--)
                {
                    var evt = recentEvents[i];
                    string impactStr = evt.StabilityImpact >= 0 ? "+" + evt.StabilityImpact.ToString("F2") : evt.StabilityImpact.ToString("F2");
                    sb.AppendLine($"- {evt.Description} ({impactStr})");
                }
            }
            
            return sb.ToString();
        }
    }
    
    /// <summary>
    /// Represents a factor contributing to unrest
    /// </summary>
    public class UnrestFactor
    {
        public string Id { get; private set; }
        public string Description { get; set; }
        public float Severity { get; set; }  // 0.0 to 1.0
        
        public UnrestFactor(string id, string description, float severity)
        {
            Id = id;
            Description = description;
            Severity = Mathf.Clamp01(severity);
        }
    }
    
    /// <summary>
    /// Represents a stability-affecting event
    /// </summary>
    public class StabilityEvent
    {
        public float StabilityImpact { get; private set; }
        public string Source { get; private set; }
        public string Description { get; private set; }
        public int TurnOccurred { get; private set; }
        
        public StabilityEvent(float impact, string source, string description)
        {
            StabilityImpact = impact;
            Source = source;
            Description = description;
            TurnOccurred = 0;  // This would be set by a turn system
        }
    }
}