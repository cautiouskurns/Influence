using UnityEngine;
using Managers;
using Systems;

namespace Editor
{
    /// <summary>
    /// NationSystemSetup is a helper component to ensure the Nation System is properly
    /// set up in your scene. Add this to any GameObject in your main scene.
    /// </summary>
    public class NationSystemSetup : MonoBehaviour
    {
        [Tooltip("Reference to the EconomicSystem in your scene")]
        public EconomicSystem economicSystem;
        
        private NationManager nationManager;
        
        private void Awake()
        {
            // Check for NationManager and create one if it doesn't exist
            nationManager = FindFirstObjectByType<NationManager>();
            if (nationManager == null)
            {
                GameObject nationManagerObject = new GameObject("NationManager");
                nationManager = nationManagerObject.AddComponent<NationManager>();
                Debug.Log("[NationSystemSetup] Created NationManager GameObject");
            }
            
            // Make sure we have a reference to the EconomicSystem
            if (economicSystem == null)
            {
                economicSystem = FindFirstObjectByType<EconomicSystem>();
                if (economicSystem == null)
                {
                    Debug.LogError("[NationSystemSetup] Could not find EconomicSystem - Nations will not have regions!");
                }
                else
                {
                    Debug.Log("[NationSystemSetup] Found EconomicSystem automatically");
                }
            }
            
            // You can remove this component after setup
            if (Application.isPlaying)
            {
                Destroy(this);
            }
        }

        [ContextMenu("Setup Nation System")]
        public void SetupNationSystem()
        {
            Awake();
            
            // Create sample nations and assign regions if we have an economic system
            if (nationManager != null && economicSystem != null)
            {
                Debug.Log("[NationSystemSetup] Setting up Nation System...");
                
                // Manually trigger assignment of regions to nations
                nationManager.AssignRegionsToNations();
                
                Debug.Log("[NationSystemSetup] Nation System setup complete!");
            }
        }
    }
}