using UnityEngine;
using Entities;
using Managers;
using System.Collections.Generic;

namespace Systems
{
    public class NationDiplomaticSubsystem : MonoBehaviour
    {
        private NationManager nationManager;
        
        private void Start()
        {
            nationManager = NationManager.Instance;
        }
        
        // Process diplomatic relations between nations
        public void ProcessTurn()
        {
            var nationIds = nationManager.GetAllNationIds();
            
            // Very simple diplomatic processing - just ensure all nations have some relation
            foreach (var nationId in nationIds)
            {
                foreach (var otherNationId in nationIds)
                {
                    if (nationId == otherNationId) continue;
                    
                    // Ensure nations have a diplomatic relation (defaults to Neutral)
                    NationEntity nation = nationManager.GetNation(nationId);
                    if (nation != null)
                    {
                        nation.GetDiplomaticStatus(otherNationId);
                    }
                }
            }
            
            Debug.Log("[NationDiplomaticSubsystem] Processed diplomatic relations for all nations");
        }
    }
}