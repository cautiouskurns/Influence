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
        [SerializeField] private int gridWidth = 8;
        [SerializeField] private int gridHeight = 8;
        [SerializeField] private float hexSize = 1.0f; // Set to 1.0 as per current settings
        
        [Header("Hex Grid Settings")]
        [SerializeField] private bool pointyTopHexes = false; // False = flat-top, True = pointy-top orientation
        [SerializeField] [Range(0.7f, 1.3f)] private float horizontalSpacingAdjust = 1.0f; // Adjust x-spacing
        [SerializeField] [Range(0.7f, 1.3f)] private float verticalSpacingAdjust = 1.0f; // Adjust y-spacing
        
        // Tracking variables
        private Dictionary<string, RegionView> regionViews = new Dictionary<string, RegionView>();
        private string selectedRegionId;
        private EconomicSystem economicSystem;
        
        // Constants for hex grid calculations
        private readonly float SQRT_3 = Mathf.Sqrt(3);
        
        private void Awake()
        {
            economicSystem = FindFirstObjectByType<EconomicSystem>();
        }
        
        private void Start()
        {
            CreateHexGrid();
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
        
        private void CreateHexGrid()
        {
            ClearExistingRegions();
            
            // Calculate the exact size of a hex sprite in the grid
            float width, height, horizontalSpacing, verticalSpacing, xStart, yStart;
            
            if (pointyTopHexes)
            {
                // Pointy-top hex grid layout (hexes pointing up/down)
                width = hexSize * 2.0f; // Width of a hex (point to point)
                height = hexSize * SQRT_3; // Height of a hex (flat to flat)
                
                // For a perfectly tight grid with pointy-top hexes:
                horizontalSpacing = width * 0.75f * horizontalSpacingAdjust; // Can adjust to fix overlap
                verticalSpacing = height * verticalSpacingAdjust; // Can adjust to fix spacing
                
                xStart = -(gridWidth * horizontalSpacing) / 2 + horizontalSpacing/2;
                yStart = -(gridHeight * verticalSpacing) / 2 + verticalSpacing/2;
                
                for (int r = 0; r < gridHeight; r++)
                {
                    for (int q = 0; q < gridWidth; q++)
                    {
                        // Calculate position based on hex grid coordinates
                        float xPos = xStart + q * horizontalSpacing;
                        float yPos = yStart + r * verticalSpacing;
                        
                        // Offset every other row
                        if (r % 2 != 0)
                        {
                            xPos += horizontalSpacing / 2;
                        }
                        
                        Vector3 position = new Vector3(xPos, yPos, 0);
                        // Create the hex with no rotation for pointy-top
                        CreateHexWithRotation(q, r, position, Quaternion.identity);
                    }
                }
            }
            else
            {
                // Flat-top hex grid layout (hexes pointing sideways)
                width = hexSize * SQRT_3; // Width of a hex (flat to flat)
                height = hexSize * 2.0f; // Height of a hex (point to point)
                
                // For a perfectly tight grid with flat-top hexes:
                horizontalSpacing = width * horizontalSpacingAdjust;
                verticalSpacing = height * 0.75f * verticalSpacingAdjust;
                
                xStart = -(gridWidth * horizontalSpacing) / 2 + horizontalSpacing/2;
                yStart = -(gridHeight * verticalSpacing) / 2 + verticalSpacing/2;
                
                for (int r = 0; r < gridHeight; r++)
                {
                    for (int q = 0; q < gridWidth; q++)
                    {
                        // Calculate position based on hex grid coordinates
                        float xPos = xStart + q * horizontalSpacing;
                        float yPos = yStart + r * verticalSpacing;
                        
                        // Offset every other column
                        if (q % 2 != 0)
                        {
                            yPos += verticalSpacing / 2;
                        }
                        
                        Vector3 position = new Vector3(xPos, yPos, 0);
                        // Create the hex with 30-degree rotation for flat-top
                        CreateHexWithRotation(q, r, position, Quaternion.Euler(0, 0, 30));
                    }
                }
            }
            
            Debug.Log($"MapManager created {regionViews.Count} hexagonal regions in a tightly packed hex grid");
        }
        
        private void CreateHexWithRotation(int q, int r, Vector3 position, Quaternion rotation)
        {
            // Create the hex with the proper orientation
            GameObject regionObj = Instantiate(regionPrefab, position, rotation, regionsContainer);
            
            string regionId = $"Region_{q}_{r}";
            string regionName = $"R{q},{r}";
            
            // Create color based on position (for visual distinction)
            Color regionColor = new Color(
                0.4f + (float)q/gridWidth * 0.6f,
                0.4f + (float)r/gridHeight * 0.6f,
                0.5f
            );
            
            // Initialize the region
            RegionView regionView = regionObj.GetComponent<RegionView>();
            regionView.Initialize(regionId, regionName, regionColor);
            
            // Store reference
            regionViews[regionId] = regionView;
            
            // Create an economic entity for this region
            CreateRegionEntity(regionId);
        }
        
        private void ClearExistingRegions()
        {
            // Clear any existing regions before creating new ones
            foreach (var view in regionViews.Values)
            {
                if (view != null && view.gameObject != null)
                {
                    Destroy(view.gameObject);
                }
            }
            regionViews.Clear();
            
            // Alternative approach to clear child objects
            for (int i = regionsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(regionsContainer.GetChild(i).gameObject);
            }
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
                    previousRegion.Deselect(); // Use the new Deselect method
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
        
        // Add a reset method that can be called from editor/inspector
        [ContextMenu("Reset Hex Grid")]
        public void ResetHexGrid()
        {
            CreateHexGrid();
        }
    }
}