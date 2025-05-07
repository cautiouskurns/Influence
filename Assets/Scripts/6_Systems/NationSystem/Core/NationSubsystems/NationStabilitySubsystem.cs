using UnityEngine;
using Entities;
using Managers;

namespace Systems
{
    public class NationStabilitySubsystem : MonoBehaviour
    {
        private NationManager nationManager;
        
        private void Start()
        {
            nationManager = NationManager.Instance;
        }
        
        // Process stability factors for all nations
        public void ProcessTurn()
        {
            var nationIds = nationManager.GetAllNationIds();
            
            foreach (var nationId in nationIds)
            {
                NationEntity nation = nationManager.GetNation(nationId);
                if (nation == null) continue;
                
                // For now, stability calculation is already handled in NationEntity
                // This is a placeholder for more complex stability calculations in the future
            }
            
            Debug.Log("[NationStabilitySubsystem] Processed stability for all nations");
        }
    }
}