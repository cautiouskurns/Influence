using System.Collections.Generic;
using UnityEngine;

namespace Entities.Components
{
    /// <summary>
    /// CLASS PURPOSE:
    /// Manages the policy system for a nation, including policy settings and active reforms.
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Store policy slider values (economic, military, etc)
    /// - Track active policies/reforms and their effects
    /// - Apply policy effects to regions
    /// </summary>
    public class NationPolicyComponent
    {
        // Policy settings (sliders from 0.0 to 1.0)
        private Dictionary<NationEntity.PolicyType, float> policies = new Dictionary<NationEntity.PolicyType, float>();
        
        // Active policies/reforms
        private List<Policy> activePolicies = new List<Policy>();
        
        /// <summary>
        /// Constructor with optional default policy values
        /// </summary>
        public NationPolicyComponent()
        {
            // Initialize with balanced defaults (0.5)
            foreach (NationEntity.PolicyType policyType in System.Enum.GetValues(typeof(NationEntity.PolicyType)))
            {
                policies[policyType] = 0.5f;
            }
        }
        
        /// <summary>
        /// Set a policy slider value
        /// </summary>
        public void SetPolicy(NationEntity.PolicyType type, float value)
        {
            // Clamp value between 0 and 1
            value = Mathf.Clamp01(value);
            policies[type] = value;
        }
        
        /// <summary>
        /// Get a policy slider value
        /// </summary>
        public float GetPolicy(NationEntity.PolicyType type)
        {
            if (policies.TryGetValue(type, out float value))
            {
                return value;
            }
            return 0.5f; // Default to balanced
        }
        
        /// <summary>
        /// Get all current policy values
        /// </summary>
        public Dictionary<NationEntity.PolicyType, float> GetAllPolicies()
        {
            return new Dictionary<NationEntity.PolicyType, float>(policies);
        }
        
        /// <summary>
        /// Get all active policies/reforms
        /// </summary>
        public List<Policy> GetActivePolicies()
        {
            return new List<Policy>(activePolicies);
        }
        
        /// <summary>
        /// Implement a new policy/reform
        /// </summary>
        public bool ImplementPolicy(Policy policy, float treasuryBalance)
        {
            // Check if we can afford the policy
            if (policy.Cost > treasuryBalance)
            {
                Debug.LogWarning($"Cannot implement policy {policy.Name}: Insufficient funds");
                return false;
            }
            
            // Add the policy to active policies
            activePolicies.Add(policy);
            
            return true;
        }
        
        /// <summary>
        /// Apply the effects of all active policies to regions
        /// </summary>
        public void ApplyPolicyEffects(List<RegionEntity> regions)
        {
            if (regions == null || regions.Count == 0)
                return;
                
            foreach (var region in regions)
            {
                // Apply the effect of each active policy
                foreach (var policy in activePolicies)
                {
                    // Apply wealth effect
                    if (policy.WealthEffect != 0)
                    {
                        float wealthChange = region.Wealth * policy.WealthEffect;
                        region.Wealth += Mathf.RoundToInt(wealthChange);
                    }
                    
                    // Apply production effect
                    if (policy.ProductionEffect != 0)
                    {
                        float productionChange = region.Production * policy.ProductionEffect;
                        region.Production += Mathf.RoundToInt(productionChange);
                    }
                    
                    // Additional effects based on policy sliders
                    ApplyPolicySliderEffects(region);
                }
            }
        }
        
        /// <summary>
        /// Apply effects based on the policy slider values
        /// </summary>
        private void ApplyPolicySliderEffects(RegionEntity region)
        {
            // Economic policy (0 = centralized, 1 = free market)
            float economicPolicy = GetPolicy(NationEntity.PolicyType.Economic);
            if (economicPolicy > 0.7f)
            {
                // Free market boosts production but may reduce stability
                region.Production += Mathf.RoundToInt(region.Production * 0.05f);
            }
            else if (economicPolicy < 0.3f)
            {
                // Centralized economy provides more stability but less growth
                region.Wealth = Mathf.Max(region.Wealth - 2, 0);
            }
            
            // Military policy (0 = pacifist, 1 = militarist)
            float militaryPolicy = GetPolicy(NationEntity.PolicyType.Military);
            if (militaryPolicy > 0.7f)
            {
                // High military spending reduces wealth
                region.Wealth = Mathf.Max(region.Wealth - 5, 0);
            }
            
            // Social policy (0 = conservative, 1 = progressive)
            // Implementation could be expanded in the future
        }
        
        /// <summary>
        /// Process a turn, updating policy durations and removing expired policies
        /// </summary>
        public void ProcessTurn()
        {
            for (int i = activePolicies.Count - 1; i >= 0; i--)
            {
                if (!activePolicies[i].UpdateTurn())
                {
                    // Policy has expired, remove it
                    Debug.Log($"Policy {activePolicies[i].Name} has expired");
                    activePolicies.RemoveAt(i);
                }
            }
        }
        
        /// <summary>
        /// Create a standard policy
        /// </summary>
        public static Policy CreateStandardPolicy(string name, string description, int cost, int duration,
                                                 float wealthEffect, float productionEffect, float stabilityEffect)
        {
            return new Policy(name, description, cost, duration, wealthEffect, productionEffect, stabilityEffect);
        }
        
        /// <summary>
        /// Get a summary of the policy component status
        /// </summary>
        public string GetSummary()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            // Add policy slider values
            sb.AppendLine("Policy Settings:");
            foreach (var policy in policies)
            {
                string policyName = policy.Key.ToString();
                string valueDesc = policy.Value < 0.3f ? "Low" : (policy.Value > 0.7f ? "High" : "Balanced");
                sb.AppendLine($"- {policyName}: {valueDesc} ({policy.Value:F2})");
            }
            
            // Add active policies
            sb.AppendLine("\nActive Policies:");
            if (activePolicies.Count == 0)
            {
                sb.AppendLine("- None");
            }
            else
            {
                foreach (var policy in activePolicies)
                {
                    sb.AppendLine($"- {policy.Name} ({policy.TurnsRemaining} turns remaining)");
                }
            }
            
            return sb.ToString();
        }
    }
}