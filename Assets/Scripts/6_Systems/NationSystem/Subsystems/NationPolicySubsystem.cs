using UnityEngine;
using Entities;
using Managers;
using System.Collections.Generic;

namespace Systems
{
    public class NationPolicySubsystem : MonoBehaviour
    {
        private NationManager nationManager;
        
        private void Start()
        {
            nationManager = NationManager.Instance;
        }
        
        // Apply all policy effects
        public void ApplyPolicies()
        {
            var nationIds = nationManager.GetAllNationIds();
            
            foreach (var nationId in nationIds)
            {
                NationEntity nation = nationManager.GetNation(nationId);
                if (nation == null) continue;
                
                // Apply policy effects (simple version)
                ApplyPolicyEffects(nation);
            }
            
            Debug.Log("[NationPolicySubsystem] Applied policy effects for all nations");
        }
        
        // Set a policy for a nation
        public void SetNationPolicy(string nationId, NationEntity.PolicyType policyType, float value)
        {
            NationEntity nation = nationManager.GetNation(nationId);
            if (nation == null) return;
            
            // Set the policy in the entity
            nation.SetPolicy(policyType, value);
            Debug.Log($"[NationPolicySubsystem] Set {policyType} policy for {nationId} to {value:F2}");
        }
        
        // Get a policy value for a nation
        public float GetNationPolicy(string nationId, NationEntity.PolicyType policyType)
        {
            NationEntity nation = nationManager.GetNation(nationId);
            if (nation == null) return 0.5f; // Default to balanced
            
            // Get the policy from the entity
            return nation.GetPolicy(policyType);
        }
        
        // Apply effects of policies to a nation
        private void ApplyPolicyEffects(NationEntity nation)
        {
            // This is a simplified implementation
            // In a full system, policy effects would influence many aspects of the nation
            // such as resource production, military capability, diplomatic relations, etc.
            
            // For now, we're just logging that policies were "applied"
            Debug.Log($"[NationPolicySubsystem] Applied policy effects for {nation.Name}");
        }
    }
}