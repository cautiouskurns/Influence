using System.Collections.Generic;
using UnityEngine;
using Managers;
using Entities;
using Systems;

namespace UI
{
    public class MapManager : MonoBehaviour
    {
        [Header("Map Settings")]
        [SerializeField] private GameObject regionPrefab;
        [SerializeField] private Transform regionsContainer;
        [SerializeField] private int gridWidth = 3;
        [SerializeField] private int gridHeight = 3;
        [SerializeField] private float regionSpacing = 2.2f;
        
        // Tracking
        private Dictionary<string, RegionView> regionViews = new Dictionary<string, RegionView>();
        private string selectedRegionId;
        private EconomicSystem economicSystem;
        
        private void Awake()
        {
            economicSystem = FindFirstObjectByType<EconomicSystem>();
        }
        
        private void Start()
        {
            CreateRegionGrid();
        }
        
        private void OnEnable()
        {
            EventBus.Subscribe("RegionSelected", OnRegionSelected);
            EventBus.Subscribe("RegionUpdated", OnRegionUpdated);
        }
        
        private void OnDisable()
        {
            EventBus.Unsubscribe("RegionSelected", OnRegionSelected);
            EventBus.Unsubscribe("RegionUpdated", OnRegionUpdated);
        }
        
        private void CreateRegionGrid()
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    // Calculate position
                    Vector3 position = new Vector3(
                        (x - gridWidth/2) * regionSpacing, 
                        (y - gridHeight/2) * regionSpacing, 
                        0
                    );
                    
                    // Generate region ID and name
                    string regionId = $"Region_{x}_{y}";
                    string regionName = $"R{x},{y}";
                    
                    // Create color based on position (for visual distinction)
                    Color regionColor = new Color(
                        0.4f + (float)x/gridWidth * 0.6f,
                        0.4f + (float)y/gridHeight * 0.6f,
                        0.5f
                    );
                    
                    // Create region
                    CreateRegion(regionId, regionName, position, regionColor);
                    
                    // Create an economic entity for this region
                    CreateRegionEntity(regionId);
                }
            }
            
            Debug.Log($"MapManager created {regionViews.Count} regions");
        }
        
        private void CreateRegionEntity(string regionId)
        {
            if (economicSystem == null) return;
            
            // Only create if the region doesn't already have an entity
            RegionEntity existingEntity = economicSystem.GetRegion(regionId);
            if (existingEntity == null)
            {
                // Create a new region entity with random initial values
                int initialWealth = Random.Range(100, 300);
                int initialProduction = Random.Range(50, 100);
                RegionEntity regionEntity = new RegionEntity(regionId, initialWealth, initialProduction);
                
                // Set additional properties
                regionEntity.LaborAvailable = Random.Range(50, 150);
                regionEntity.InfrastructureLevel = Random.Range(1, 5);
                
                // Register with the economic system
                economicSystem.RegisterRegion(regionEntity);
                
                // Update the region view if available
                if (regionViews.TryGetValue(regionId, out RegionView regionView))
                {
                    regionView.SetRegionEntity(regionEntity);
                }
            }
        }
        
        private void CreateRegion(string id, string name, Vector3 position, Color color)
        {
            // Instantiate the region prefab
            GameObject regionObj = Instantiate(regionPrefab, position, Quaternion.identity, regionsContainer);
            
            // Get the RegionView component
            RegionView regionView = regionObj.GetComponent<RegionView>();
            
            // Initialize with data
            regionView.Initialize(id, name, color);
            
            // Store reference
            regionViews[id] = regionView;
        }
        
        private void OnRegionSelected(object data)
        {
            if (data is string regionId)
            {
                // Deselect previous region
                if (!string.IsNullOrEmpty(selectedRegionId) && 
                    regionViews.TryGetValue(selectedRegionId, out var previousRegion))
                {
                    previousRegion.SetHighlighted(false);
                }
                
                // Select new region
                selectedRegionId = regionId;
                if (regionViews.TryGetValue(regionId, out var currentRegion))
                {
                    currentRegion.SetHighlighted(true);
                    Debug.Log($"Selected region: {regionId}");
                }
            }
        }
        
        private void OnRegionUpdated(object data)
        {
            // This is handled directly by the RegionView components
        }
    }
}