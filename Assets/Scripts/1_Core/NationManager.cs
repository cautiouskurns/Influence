using System.Collections.Generic;
using UnityEngine;
using Entities;
using Systems;
using Managers;
using System.Collections;

namespace Managers
{
    public class NationManager : MonoBehaviour
    {
        #region Singleton
        private static NationManager _instance;
        
        public static NationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<NationManager>();
                    
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("NationManager");
                        _instance = go.AddComponent<NationManager>();
                    }
                }
                
                return _instance;
            }
        }
        #endregion

        // Dictionary of nations by ID
        private Dictionary<string, NationEntity> nations = new Dictionary<string, NationEntity>();
        
        // Reference to the economic system for region access
        private EconomicSystem economicSystem;
        
        private void Awake()
        {
            // Singleton pattern
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize references
            economicSystem = FindFirstObjectByType<EconomicSystem>();
        }
        
        private void OnEnable()
        {
            // Subscribe to the RegionsCreated event
            EventBus.Subscribe("RegionsCreated", OnRegionsCreated);
        }
        
        private void OnDisable()
        {
            // Unsubscribe from the RegionsCreated event
            EventBus.Unsubscribe("RegionsCreated", OnRegionsCreated);
        }
        
        private void Start()
        {
            // Create some sample nations if none exist
            if (nations.Count == 0)
            {
                CreateSampleNations();
            }
            
            // Debug output to check the setup
            Debug.Log($"[NationManager] Nation system initialized with {nations.Count} nations");
            
            // Check if we have access to the economic system
            if (economicSystem == null)
            {
                economicSystem = FindObjectOfType<EconomicSystem>();
                Debug.Log($"[NationManager] EconomicSystem found: {(economicSystem != null ? "Yes" : "No")}");
            }
            
            // Check if regions already exist
            if (economicSystem != null)
            {
                CheckAndAssignRegions();
            }
            else
            {
                // No economic system found, try to find it after a short delay
                StartCoroutine(FindEconomicSystemWithDelay());
            }
        }

        // Event handler for when regions are created
        private void OnRegionsCreated(object data)
        {
            int regionCount = 0;
            if (data is int count)
            {
                regionCount = count;
            }
            
            Debug.Log($"[NationManager] Received RegionsCreated event with {regionCount} regions");
            
            // Try to assign regions after a short delay
            StartCoroutine(AssignRegionsWithDelay(0.5f));
        }
        
        // Coroutine to wait and find EconomicSystem
        private IEnumerator FindEconomicSystemWithDelay()
        {
            Debug.Log("[NationManager] Waiting to find EconomicSystem...");
            
            yield return new WaitForSeconds(0.5f);
            
            economicSystem = FindObjectOfType<EconomicSystem>();
            Debug.Log($"[NationManager] EconomicSystem found after delay: {(economicSystem != null ? "Yes" : "No")}");
            
            if (economicSystem != null)
            {
                CheckAndAssignRegions();
            }
        }
        
        // Coroutine to wait and assign regions
        private IEnumerator AssignRegionsWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            Debug.Log("[NationManager] Assigning regions after delay...");
            CheckAndAssignRegions();
        }
        
        // Check if regions exist and assign them
        private void CheckAndAssignRegions()
        {
            if (economicSystem == null)
            {
                Debug.LogError("[NationManager] Can't check regions - EconomicSystem not found");
                return;
            }
            
            var regions = economicSystem.GetAllRegionIds();
            Debug.Log($"[NationManager] Regions found: {regions.Count}");
            
            if (regions.Count > 0)
            {
                // Make sure regions are assigned to nations
                AssignRegionsToNations();
            }
            else
            {
                Debug.LogWarning("[NationManager] No regions found yet, will wait for RegionsCreated event");
            }
            
            // Output the final state of nations and regions
            foreach (var nation in nations.Values)
            {
                Debug.Log($"[NationManager] Nation: {nation.Name}, Regions: {nation.GetRegionIds().Count}");
            }
        }

        // Create and register a new nation
        public NationEntity CreateNation(string id, string name, Color color)
        {
            if (nations.ContainsKey(id))
            {
                Debug.LogWarning($"Nation with ID {id} already exists!");
                return nations[id];
            }
            
            NationEntity newNation = new NationEntity(id, name, color);
            nations.Add(id, newNation);
            
            Debug.Log($"Created new nation: {name} (ID: {id})");
            return newNation;
        }
        
        // Get a nation by ID
        public NationEntity GetNation(string nationId)
        {
            if (nations.TryGetValue(nationId, out NationEntity nation))
            {
                return nation;
            }
            
            Debug.LogWarning($"Nation with ID {nationId} not found!");
            return null;
        }
        
        // Get all nation IDs
        public List<string> GetAllNationIds()
        {
            return new List<string>(nations.Keys);
        }
        
        // Assign a region to a nation
        public void AssignRegionToNation(string regionId, string nationId)
        {
            // First, remove the region from any existing nation
            foreach (var existingNation in nations.Values)
            {
                existingNation.RemoveRegion(regionId);
            }
            
            // Then add it to the specified nation
            if (nations.TryGetValue(nationId, out NationEntity nation))
            {
                nation.AddRegion(regionId);
                
                // Update the region's display if needed
                // This could trigger an event that MapManager listens to
                EventBus.Trigger("RegionNationChanged", new RegionNationChangedData { RegionId = regionId, NationId = nationId });
            }
        }
        
        // Get the nation a region belongs to
        public NationEntity GetRegionNation(string regionId)
        {
            foreach (var nation in nations.Values)
            {
                if (nation.GetRegionIds().Contains(regionId))
                {
                    return nation;
                }
            }
            
            return null;
        }
        
        // Method to ensure regions are assigned to nations
        public void AssignRegionsToNations()
        {
            if (economicSystem == null)
            {
                Debug.LogError("[NationManager] Can't assign regions - EconomicSystem not found");
                return;
            }
            
            var regions = economicSystem.GetAllRegionIds();
            if (regions.Count == 0)
            {
                Debug.LogWarning("[NationManager] No regions found to assign to nations");
                return;
            }
            
            var nationIds = new List<string>(nations.Keys);
            if (nationIds.Count == 0)
            {
                Debug.LogWarning("[NationManager] No nations available to assign regions to");
                return;
            }
            
            Debug.Log($"[NationManager] Assigning {regions.Count} regions to {nationIds.Count} nations");
            
            // Simple distribution - assign regions to nations based on a simple pattern
            for (int i = 0; i < regions.Count; i++)
            {
                int nationIndex = i % nationIds.Count;
                string nationId = nationIds[nationIndex];
                
                AssignRegionToNation(regions[i], nationId);
                Debug.Log($"[NationManager] Assigned region {regions[i]} to nation {nationId}");
            }
        }
        
        // Create some sample nations for testing
        private void CreateSampleNations()
        {
            CreateNation("empire", "Northern Empire", new Color(0.8f, 0.2f, 0.2f));
            CreateNation("republic", "Coastal Republic", new Color(0.2f, 0.4f, 0.8f));
            CreateNation("kingdom", "Southern Kingdom", new Color(0.1f, 0.6f, 0.3f));
            
            Debug.Log("[NationManager] Created sample nations");
        }
    }
    
    // Data structure for region assignment events
    public class RegionNationChangedData
    {
        public string RegionId { get; set; }
        public string NationId { get; set; }
    }
}