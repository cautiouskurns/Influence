using System.Collections.Generic;
using UnityEngine;
using Entities;
using Managers;

namespace Systems
{
    public class NationSystem : MonoBehaviour
    {
        #region Singleton
        private static NationSystem _instance;
        
        public static NationSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<NationSystem>();
                    
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("NationSystem");
                        _instance = go.AddComponent<NationSystem>();
                    }
                }
                
                return _instance;
            }
        }
        #endregion
        
        // References
        private NationManager nationManager;
        private EconomicSystem economicSystem;
        
        // Subsystems
        private NationEconomicSubsystem economicSubsystem;
        private NationDiplomaticSubsystem diplomaticSubsystem;
        private NationStabilitySubsystem stabilitySubsystem;
        private NationEventSubsystem eventSubsystem;
        private NationPolicySubsystem policySubsystem;
        
        // Cache of regions
        private Dictionary<string, RegionEntity> regionCache = new Dictionary<string, RegionEntity>();
        
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
            
            // Initialize subsystems
            InitializeSubsystems();
        }
        
        private void Start()
        {
            // Get references
            nationManager = NationManager.Instance;
            economicSystem = FindFirstObjectByType<EconomicSystem>();
            
            if (nationManager == null || economicSystem == null)
            {
                Debug.LogError("[NationSystem] Failed to find required dependencies.");
                return;
            }
            
            // Initialize the region cache
            UpdateRegionCache();
            
        }
        
        private void OnEnable()
        {
            // Subscribe to events
            EventBus.Subscribe("TurnProcessed", OnTurnProcessed);
            EventBus.Subscribe("RegionsAssignedToNations", OnRegionsAssignedToNations);
            EventBus.Subscribe("RegionNationChanged", OnRegionNationChanged);
        }
        
        private void OnDisable()
        {
            // Unsubscribe from events
            EventBus.Unsubscribe("TurnProcessed", OnTurnProcessed);
            EventBus.Unsubscribe("RegionsAssignedToNations", OnRegionsAssignedToNations);
            EventBus.Unsubscribe("RegionNationChanged", OnRegionNationChanged);
        }
        
        // Event handlers
        private void OnTurnProcessed(object data)
        {
            Debug.Log("[NationSystem] Turn processed, updating nation statistics");
            
            // Update region cache
            UpdateRegionCache();
            
            // Update core nation data
            UpdateAllNationData();
            
            // Process subsystems in the correct order
            economicSubsystem.ProcessTurn();
            diplomaticSubsystem.ProcessTurn();
            stabilitySubsystem.ProcessTurn();
            eventSubsystem.ProcessTurn();
            policySubsystem.ApplyPolicies();
            
            // Notify that nation processing is complete
            EventBus.Trigger("NationStatisticsUpdated", null);
        }
        
        private void OnRegionsAssignedToNations(object data)
        {
            Debug.Log("[NationSystem] Regions assigned to nations, updating statistics");
            UpdateRegionCache();
            UpdateAllNationData();
        }
        
        private void OnRegionNationChanged(object data)
        {
            // A single region changed ownership, update statistics for affected nations
            if (data is RegionNationChangedData changeData)
            {
                string regionId = changeData.RegionId;
                string nationId = changeData.NationId;
                
                // Update the statistics for the nation
                UpdateNationData(nationId);
                
                // Debug.Log($"[NationSystem] Region {regionId} changed to nation {nationId}, updated statistics");
            }
        }
        
        // Core functions
        
        // Initialize subsystems
        private void InitializeSubsystems()
        {
            // Create gameobjects for each subsystem
            GameObject economicGO = new GameObject("NationEconomicSubsystem");
            economicGO.transform.parent = this.transform;
            economicSubsystem = economicGO.AddComponent<NationEconomicSubsystem>();
            
            GameObject diplomaticGO = new GameObject("NationDiplomaticSubsystem");
            diplomaticGO.transform.parent = this.transform;
            diplomaticSubsystem = diplomaticGO.AddComponent<NationDiplomaticSubsystem>();
            
            GameObject stabilityGO = new GameObject("NationStabilitySubsystem");
            stabilityGO.transform.parent = this.transform;
            stabilitySubsystem = stabilityGO.AddComponent<NationStabilitySubsystem>();
            
            GameObject eventGO = new GameObject("NationEventSubsystem");
            eventGO.transform.parent = this.transform;
            eventSubsystem = eventGO.AddComponent<NationEventSubsystem>();
            
            GameObject policyGO = new GameObject("NationPolicySubsystem");
            policyGO.transform.parent = this.transform;
            policySubsystem = policyGO.AddComponent<NationPolicySubsystem>();
        }
        
        // Update the cache of regions from the economic system
        private void UpdateRegionCache()
        {
            if (economicSystem == null)
            {
                Debug.LogError("[NationSystem] Can't update region cache - EconomicSystem not found");
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
            
        }
        
        // Update data for all nations
        private void UpdateAllNationData()
        {
            var nationIds = nationManager.GetAllNationIds();
            foreach (var nationId in nationIds)
            {
                UpdateNationData(nationId);
            }
        }
        
        // Update data for a specific nation
        private void UpdateNationData(string nationId)
        {
            NationEntity nation = nationManager.GetNation(nationId);
            if (nation == null) return;
            
            // Update basic nation data
            // This will be enhanced by the subsystems later
        }
        
        // Public API methods
        
        // Get access to the region cache
        public Dictionary<string, RegionEntity> GetRegionCache()
        {
            return regionCache;
        }
        
        // Interface with policy subsystem
        public void SetNationPolicy(string nationId, NationEntity.PolicyType policyType, float value)
        {
            policySubsystem.SetNationPolicy(nationId, policyType, value);
        }
        
        public float GetNationPolicy(string nationId, NationEntity.PolicyType policyType)
        {
            return policySubsystem.GetNationPolicy(nationId, policyType);
        }
    }
}