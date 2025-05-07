using System.Collections.Generic;
using UnityEngine;
using Entities;
using Systems;
using System.Collections;
using System.Linq; // Added for LINQ extension methods

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
        
        // Dictionary to store region-to-nation assignments explicitly
        private Dictionary<string, string> regionNationMap = new Dictionary<string, string>();
        
        // Reference to the economic system for region access
        private EconomicSystem economicSystem;
        
        [SerializeField] private bool debugLogging = true;
        [SerializeField] private int maxRetries = 5;
        [SerializeField] private float retryDelay = 0.5f;
        
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
            TryFindEconomicSystem();
            
            // if (debugLogging)
            //     Debug.Log("[NationManager] Awake completed - EconomicSystem found: " + (economicSystem != null));
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
//            Debug.Log($"[NationManager] Nation system initialized with {nations.Count} nations");
            
            // Start coroutine to ensure we have economic system and regions
            StartCoroutine(EnsureEconomicSystemAndAssignRegions());
        }

        // New method to try to find the economic system
        private bool TryFindEconomicSystem()
        {
            if (economicSystem == null)
            {
                economicSystem = FindFirstObjectByType<EconomicSystem>();
            }
            return economicSystem != null;
        }
        
        // Coroutine to ensure we have economic system and regions
        private IEnumerator EnsureEconomicSystemAndAssignRegions()
        {
            int attempts = 0;
            
            while (economicSystem == null && attempts < maxRetries)
            {
                Debug.Log($"[NationManager] Trying to find EconomicSystem (attempt {attempts + 1}/{maxRetries})");
                
                if (TryFindEconomicSystem())
                {
                    Debug.Log("[NationManager] EconomicSystem found!");
                    break;
                }
                
                attempts++;
                yield return new WaitForSeconds(retryDelay);
            }
            
            if (economicSystem == null)
            {
                Debug.LogError("[NationManager] Failed to find EconomicSystem after multiple attempts. Region assignment will not work.");
                yield break;
            }
            
            // Now try to assign regions
            var regions = economicSystem.GetAllRegionIds();
            if (regions.Any()) // Changed from Count > 0 to Any()
            {
                Debug.Log($"[NationManager] Found {regions.Count()} regions, assigning to nations"); // Changed from Count to Count()
                AssignRegionsToNations();
            }
            else
            {
                // Debug.Log("[NationManager] No regions found yet, will wait for RegionsCreated event");
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
            
            // Debug.Log($"[NationManager] Received RegionsCreated event with {regionCount} regions");
            
            // Ensure we have an economic system reference
            if (!TryFindEconomicSystem())
            {
                Debug.LogError("[NationManager] Received RegionsCreated event but still can't find EconomicSystem");
                StartCoroutine(EnsureEconomicSystemAndAssignRegions());
                return;
            }
            
            // Assign regions to nations
            CheckAndAssignRegions();
        }
        
        // Check if regions exist and assign them
        private void CheckAndAssignRegions()
        {
            if (economicSystem == null)
            {
                Debug.LogError("[NationManager] Can't check regions - EconomicSystem not found");
                StartCoroutine(EnsureEconomicSystemAndAssignRegions());
                return;
            }
            
            var regions = economicSystem.GetAllRegionIds();
//            Debug.Log($"[NationManager] Regions found: {regions.Count}");
            
            if (regions.Any()) // Changed from Count > 0 to Any()
            {
                // Make sure regions are assigned to nations
                AssignRegionsToNations();
                
                // Trigger event for nation system to process
                EventBus.Trigger("RegionsAssignedToNations", null);
                
                // Additional debug logging to confirm nations have regions
                foreach (var nation in nations.Values)
                {
                    // Debug.Log($"[NationManager] Nation {nation.Name} has {nation.GetRegionIds().Count} regions assigned");
                }
            }
            else
            {
                // Debug.LogWarning("[NationManager] No regions found yet, will wait for RegionsCreated event");
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
            
            // Debug.Log($"Created new nation: {name} (ID: {id})");
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
        
        // Get the nation that owns a specific region
        public NationEntity GetRegionNation(string regionId)
        {
            // First check the direct mapping
            if (regionNationMap.TryGetValue(regionId, out string nationId))
            {
                if (nations.TryGetValue(nationId, out NationEntity mappedNation))
                {
                    if (debugLogging)
                    return mappedNation;
                }
            }
            
            // Fallback to traditional search if mapping fails
            foreach (var nation in nations.Values)
            {
                if (nation.GetRegionIds().Contains(regionId))
                {
                    if (debugLogging)
                        Debug.Log($"[NationManager] Region {regionId} belongs to nation: {nation.Name}");
                    
                    // Update the map for future lookups
                    regionNationMap[regionId] = nation.Id;
                    return nation;
                }
            }
            
            // If no nation owns this region, return null (it's "Independent")
            if (debugLogging)
                Debug.Log($"[NationManager] Region {regionId} belongs to no nation (Independent)");
            return null;
        }
        
        // Get all nation IDs
        public List<string> GetAllNationIds()
        {
            return new List<string>(nations.Keys);
        }
        
        // Get all nations
        public List<NationEntity> GetAllNations()
        {
            return new List<NationEntity>(nations.Values);
        }
        
        // Assign a region to a nation
        public void AssignRegionToNation(string regionId, string nationId)
        {
            // First, remove the region from any existing nation
            foreach (var existingNation in nations.Values)
            {
                existingNation.RemoveRegion(regionId);
            }
            
            // Remove from the map as well
            regionNationMap.Remove(regionId);
            
            // Then add it to the specified nation
            if (nations.TryGetValue(nationId, out NationEntity nation))
            {
                nation.AddRegion(regionId);
                
                // Update the map for quick lookups
                regionNationMap[regionId] = nationId;

                
                // Trigger an event for visualization/processing
                EventBus.Trigger("RegionNationChanged", new RegionNationChangedData { 
                    RegionId = regionId, 
                    NationId = nationId 
                });
            }
            else
            {
                Debug.LogError($"[NationManager] Failed to assign region {regionId} - nation {nationId} not found!");
            }
        }
        
        // Method to ensure regions are assigned to nations
        public void AssignRegionsToNations()
        {
            if (economicSystem == null)
            {
                Debug.LogError("[NationManager] Can't assign regions - EconomicSystem not found");
                return;
            }
            
            var regions = economicSystem.GetAllRegionIds().ToList(); // Convert to List to use Count and indexing
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
            
            // Debug.Log($"[NationManager] Assigning {regions.Count} regions to {nationIds.Count} nations");
            
            // Simple distribution - assign regions to nations based on a simple pattern
            for (int i = 0; i < regions.Count; i++)
            {
                int nationIndex = i % nationIds.Count;
                string nationId = nationIds[nationIndex];
                
                AssignRegionToNation(regions[i], nationId);
            }
        }
        
        // Create some sample nations for testing
        private void CreateSampleNations()
        {
            CreateNation("empire", "Northern Empire", new Color(0.8f, 0.2f, 0.2f));
            CreateNation("republic", "Coastal Republic", new Color(0.2f, 0.4f, 0.8f));
            CreateNation("kingdom", "Southern Kingdom", new Color(0.1f, 0.6f, 0.3f));
            
            // Debug.Log("[NationManager] Created sample nations");
        }
    }
    
    // Data structure for region assignment events
    public class RegionNationChangedData
    {
        public string RegionId { get; set; }
        public string NationId { get; set; }
    }
}