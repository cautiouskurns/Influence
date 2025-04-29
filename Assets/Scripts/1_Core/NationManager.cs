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
        
        // Dictionary to cache regions from the economic system
        private Dictionary<string, RegionEntity> regionCache = new Dictionary<string, RegionEntity>();
        
        // Policy modifier defaults
        private const float DEFAULT_POLICY_VALUE = 0.5f;
        
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
            
            // Subscribe to turn processing events
            EventBus.Subscribe("TurnProcessed", OnTurnProcessed);
        }
        
        private void OnDisable()
        {
            // Unsubscribe from the RegionsCreated event
            EventBus.Unsubscribe("RegionsCreated", OnRegionsCreated);
            
            // Unsubscribe from turn processing events
            EventBus.Unsubscribe("TurnProcessed", OnTurnProcessed);
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
                economicSystem = FindFirstObjectByType<EconomicSystem>();
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
        
        // Event handler for when a turn is processed
        private void OnTurnProcessed(object data)
        {
            Debug.Log("[NationManager] Turn processed, updating nation statistics");
            UpdateAllNationStatistics();
        }
        
        // Coroutine to wait and find EconomicSystem
        private IEnumerator FindEconomicSystemWithDelay()
        {
            Debug.Log("[NationManager] Waiting to find EconomicSystem...");
            
            yield return new WaitForSeconds(0.5f);
            
            economicSystem = FindFirstObjectByType<EconomicSystem>();
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
                
                // Update the region cache
                UpdateRegionCache();
                
                // After region assignment, update nation statistics
                UpdateAllNationStatistics();
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
        
        // Update all nation statistics based on their owned regions
        public void UpdateAllNationStatistics()
        {
            if (regionCache.Count == 0)
            {
                UpdateRegionCache();
            }
            
            foreach (var nation in nations.Values)
            {
                nation.UpdateStatistics(regionCache);
            }
            
            Debug.Log("[NationManager] Updated statistics for all nations");
            
            // Trigger an event for UI updates
            EventBus.Trigger("NationStatisticsUpdated", null);
        }
        
        // Update the cache of regions from the economic system
        private void UpdateRegionCache()
        {
            if (economicSystem == null)
            {
                Debug.LogError("[NationManager] Can't update region cache - EconomicSystem not found");
                return;
            }
            
            // Clear the existing cache
            regionCache.Clear();
            
            // Get all regions from the economic system
            var regions = economicSystem.GetAllRegions();
            foreach (var region in regions)
            {
                regionCache[region.Name] = region;
            }
            
            Debug.Log($"[NationManager] Updated region cache with {regionCache.Count} regions");
        }
        
        // Get basic information about all nations for UI display
        public List<string> GetNationSummaries()
        {
            List<string> summaries = new List<string>();
            
            foreach (var nation in nations.Values)
            {
                summaries.Add(nation.GetSummary());
            }
            
            return summaries;
        }
        
        // Set a policy for a nation
        public void SetNationPolicy(string nationId, NationEntity.PolicyType policyType, float value)
        {
            if (nations.TryGetValue(nationId, out NationEntity nation))
            {
                nation.SetPolicy(policyType, value);
                Debug.Log($"[NationManager] Set {policyType} policy for {nationId} to {value:F2}");
                
                // Trigger an event for UI updates
                EventBus.Trigger("NationPolicyUpdated", new NationPolicyChangedData { 
                    NationId = nationId, 
                    PolicyType = policyType, 
                    Value = value 
                });
            }
            else
            {
                Debug.LogWarning($"[NationManager] Nation with ID {nationId} not found!");
            }
        }
    }
    
    // Data structure for region assignment events
    public class RegionNationChangedData
    {
        public string RegionId { get; set; }
        public string NationId { get; set; }
    }
    
    // Data structure for nation policy change events
    public class NationPolicyChangedData
    {
        public string NationId { get; set; }
        public NationEntity.PolicyType PolicyType { get; set; }
        public float Value { get; set; }
    }
}