using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Managers;
using Entities;
using Systems;
using UI.MapComponents;
using Controllers;
using Scenarios; // Added this namespace for RegionStartCondition

namespace UI
{
    /// <summary>
    /// CLASS PURPOSE:
    /// MapManager coordinates the different components of the map system.
    /// It follows the Single Responsibility Principle by delegating specific tasks
    /// to specialized components.
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Initialize and coordinate map components
    /// - Handle global events related to the map
    /// - Manage high-level map state and configuration
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        [Header("Map Settings")]
        [SerializeField] private GameObject regionPrefab;
        [SerializeField] private Transform regionsContainer;
        [SerializeField] private int gridWidth = 8;
        [SerializeField] private int gridHeight = 8;
        [SerializeField] private float hexSize = 1.0f;
        
        [Header("Hex Grid Settings")]
        [SerializeField] private bool pointyTopHexes = false;
        [SerializeField] [Range(0.7f, 1.3f)] private float horizontalSpacingAdjust = 1.0f;
        [SerializeField] [Range(0.7f, 1.3f)] private float verticalSpacingAdjust = 1.0f;
        
        // Component references
        private HexGridGenerator gridGenerator;
        private RegionFactory regionFactory;
        private MapSelectionManager selectionManager;
        
        // State
        private Dictionary<string, RegionView> regionViews = new Dictionary<string, RegionView>();
        
        // Dependencies
        private EconomicSystem economicSystem;
        private RegionControllerManager controllerManager;
        private RegionColorService colorService;
        
        private void Awake()
        {
            // Get dependencies
            economicSystem = FindFirstObjectByType<EconomicSystem>();
            colorService = RegionColorService.Instance;
            
            // Initialize the controller manager
            InitializeControllerManager();
            
            // Initialize components
            InitializeComponents();
        }
        
        private void Start()
        {
            // Only create the default hex grid if not being controlled by a scenario
            if (!gameObject.TryGetComponent<MapScenarioController>(out _))
            {
                CreateHexGrid();
            }
        }
        
        private void OnEnable()
        {
            EventBus.Subscribe("RegionSelected", OnRegionSelected);
        }
        
        private void OnDisable()
        {
            EventBus.Unsubscribe("RegionSelected", OnRegionSelected);
        }
        
        /// <summary>
        /// Initialize specialized component classes
        /// </summary>
        private void InitializeComponents()
        {
            // Create the hex grid generator
            gridGenerator = new HexGridGenerator(
                gridWidth, 
                gridHeight, 
                hexSize, 
                pointyTopHexes, 
                horizontalSpacingAdjust, 
                verticalSpacingAdjust
            );
            
            // Create the region factory
            regionFactory = new RegionFactory(
                regionPrefab,
                regionsContainer,
                economicSystem,
                controllerManager
            );
            
            // Create the selection manager
            selectionManager = new MapSelectionManager(regionViews);
            
            // Initialize the color service with our region views
            if (colorService != null)
            {
                colorService.Initialize(regionViews, gridWidth, gridHeight);
            }
            else 
            {
                Debug.LogError("MapManager: RegionColorService is null! Colors will not be properly managed.");
            }
            
            Debug.Log("MapManager: Initialized all components");
        }
        
        /// <summary>
        /// Create the hexagonal grid of regions
        /// </summary>
        private void CreateHexGrid()
        {
            ClearExistingRegions();
            
            // Generate the grid cells
            var cells = gridGenerator.GenerateGrid();
            
            // Create regions for each cell
            foreach (var cell in cells)
            {
                // Get the color for this region
                Color regionColor = colorService != null 
                    ? colorService.GetRegionColor(cell.Id, cell.Q, cell.R, gridWidth, gridHeight, colorService.GetColorMode())
                    : Color.white;
                
                // Create the region with entity
                RegionView regionView = regionFactory.CreateRegionWithEntity(
                    cell.Id, cell.Name, cell.Position, cell.Rotation, regionColor);
                
                // Store reference
                regionViews[cell.Id] = regionView;
            }
            
            Debug.Log($"MapManager created {regionViews.Count} hexagonal regions in a tightly packed hex grid");
            
            // Update the color service with the new region views
            if (colorService != null)
            {
                colorService.Initialize(regionViews, gridWidth, gridHeight);
            }
            
            // Trigger the RegionsCreated event to notify NationManager that regions have been created
            EventBus.Trigger("RegionsCreated", regionViews.Count);
        }
        
        /// <summary>
        /// Creates custom regions based on scenario data instead of the default hex grid
        /// </summary>
        public void CreateCustomMap(List<Scenarios.RegionStartCondition> regionStartConditions)
        {
            ClearExistingRegions();
            
            Debug.Log($"MapManager: Creating custom map with {regionStartConditions.Count} predefined regions");
            
            // First, get the nation manager
            NationManager nationManager = NationManager.Instance;
            if (nationManager == null)
            {
                Debug.LogError("MapManager: NationManager instance not found. Cannot organize regions by nation.");
                return;
            }
            
            // Group regions by nation for better organization
            Dictionary<string, List<RegionStartCondition>> regionsByNation = new Dictionary<string, List<RegionStartCondition>>();
            
            // First pass - identify which nation each region belongs to
            foreach (var regionCondition in regionStartConditions)
            {
                string nationId = "unassigned"; // Default if no nation is assigned
                
                // Try to get the nation ID for this region from scenarios or other data
                foreach (var nation in nationManager.GetAllNations())
                {
                    if (nation.GetRegionIds().Contains(regionCondition.regionId))
                    {
                        nationId = nation.Id;
                        break;
                    }
                }
                
                // Add to the group for this nation
                if (!regionsByNation.ContainsKey(nationId))
                {
                    regionsByNation[nationId] = new List<RegionStartCondition>();
                }
                
                regionsByNation[nationId].Add(regionCondition);
            }
            
            // Keep track of already used positions
            HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();
            
            // Create starting positions for each nation as an anchor point
            Dictionary<string, Vector2Int> nationStartPositions = new Dictionary<string, Vector2Int>();
            int nationSpacing = 3; // Space between nation clusters
            int currentX = 0;
            
            foreach (string nationId in regionsByNation.Keys)
            {
                nationStartPositions[nationId] = new Vector2Int(currentX, 0);
                currentX += nationSpacing;
            }
            
            // Now create regions nation by nation, placing them in clusters
            foreach (var nationGroup in regionsByNation)
            {
                string nationId = nationGroup.Key;
                List<RegionStartCondition> nationRegions = nationGroup.Value;
                
                Vector2Int nationAnchor = nationStartPositions[nationId];
                Queue<Vector2Int> availablePositions = new Queue<Vector2Int>();
                
                // Start with the anchor position
                Vector2Int startPos = nationAnchor;
                if (!occupiedPositions.Contains(startPos))
                {
                    availablePositions.Enqueue(startPos);
                    occupiedPositions.Add(startPos);
                }
                
                // Process each region in this nation
                foreach (var regionCondition in nationRegions)
                {
                    // If we ran out of pre-computed positions, find new ones
                    if (availablePositions.Count == 0)
                    {
                        // Search for positions adjacent to any existing region from this nation
                        foreach (Vector2Int pos in occupiedPositions)
                        {
                            AddAdjacentPositions(pos, occupiedPositions, availablePositions);
                        }
                        
                        // If still no positions, create a new one further out
                        if (availablePositions.Count == 0)
                        {
                            Vector2Int newPos = new Vector2Int(nationAnchor.x + Random.Range(-5, 5), 
                                                              nationAnchor.y + Random.Range(-5, 5));
                            while (occupiedPositions.Contains(newPos))
                            {
                                newPos = new Vector2Int(nationAnchor.x + Random.Range(-5, 5), 
                                                      nationAnchor.y + Random.Range(-5, 5));
                            }
                            availablePositions.Enqueue(newPos);
                            occupiedPositions.Add(newPos);
                        }
                    }
                    
                    // Get next available position
                    Vector2Int coords = availablePositions.Dequeue();
                    
                    // Convert grid coordinates to hex coordinates (q, r)
                    int q = coords.x;
                    int r = coords.y;
                    
                    // Calculate position using hex grid layout
                    Vector3 position = gridGenerator.GetHexPosition(q, r);
                    Quaternion rotation = Quaternion.identity;
                    
                    // Get appropriate color
                    Color regionColor = colorService != null 
                        ? colorService.GetRegionColor(regionCondition.regionId, q, r, gridWidth, gridHeight, colorService.GetColorMode())
                        : Color.white;
                    
                    // Create the region with entity and custom properties
                    RegionView regionView = regionFactory.CreateCustomRegion(
                        regionCondition.regionId,
                        regionCondition.regionName,
                        position, 
                        rotation,
                        regionColor,
                        regionCondition.initialWealth,
                        regionCondition.initialProduction,
                        regionCondition.initialPopulation,
                        regionCondition.initialSatisfaction,
                        regionCondition.initialInfrastructureLevel);
                    
                    // Store reference
                    regionViews[regionCondition.regionId] = regionView;
                    
                    // Queue up adjacent positions for future regions from same nation
                    AddAdjacentPositions(coords, occupiedPositions, availablePositions);
                }
            }
            
            Debug.Log($"MapManager created {regionViews.Count} custom regions from scenario data");
            
            // Update the color service with the new region views
            if (colorService != null)
            {
                colorService.Initialize(regionViews, gridWidth, gridHeight);
            }
            
            // Trigger the RegionsCreated event to notify NationManager that regions have been created
            EventBus.Trigger("RegionsCreated", regionViews.Count);
        }
        
        /// <summary>
        /// Adds adjacent hex grid positions to the queue if they aren't already occupied
        /// </summary>
        private void AddAdjacentPositions(Vector2Int center, HashSet<Vector2Int> occupied, Queue<Vector2Int> available)
        {
            // Define the six adjacent hex positions
            // For odd-q hex grid:
            Vector2Int[] adjacentOffsets;
            
            // For even rows, the offsets are different than odd rows
            if (center.y % 2 == 0)
            {
                adjacentOffsets = new Vector2Int[] {
                    new Vector2Int(0, 1),   // North
                    new Vector2Int(1, 1),   // Northeast
                    new Vector2Int(1, 0),   // Southeast
                    new Vector2Int(0, -1),  // South
                    new Vector2Int(-1, 0),  // Southwest
                    new Vector2Int(-1, 1)   // Northwest
                };
            }
            else
            {
                adjacentOffsets = new Vector2Int[] {
                    new Vector2Int(0, 1),   // North
                    new Vector2Int(1, 0),   // Northeast
                    new Vector2Int(1, -1),  // Southeast
                    new Vector2Int(0, -1),  // South
                    new Vector2Int(-1, -1), // Southwest
                    new Vector2Int(-1, 0)   // Northwest
                };
            }
            
            // Randomize the order to avoid predictable layouts
            System.Random rng = new System.Random();
            Vector2Int[] shuffledOffsets = adjacentOffsets.OrderBy(x => rng.Next()).ToArray();
            
            // Check each adjacent position
            foreach (var offset in shuffledOffsets)
            {
                Vector2Int adjacentPos = new Vector2Int(center.x + offset.x, center.y + offset.y);
                
                // If this position isn't already taken, add it to available positions
                if (!occupied.Contains(adjacentPos))
                {
                    available.Enqueue(adjacentPos);
                    occupied.Add(adjacentPos);
                }
            }
        }
        
        /// <summary>
        /// Clear existing region game objects
        /// </summary>
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
        
        /// <summary>
        /// Set the color mode for the map via the RegionColorService
        /// </summary>
        public void SetColorMode(int modeValue)
        {
            if (colorService != null)
            {
                colorService.SetColorMode(modeValue);
            }
        }
        
        /// <summary>
        /// Initialize or find the RegionControllerManager
        /// </summary>
        private void InitializeControllerManager()
        {
            // Find or create the RegionControllerManager
            controllerManager = FindFirstObjectByType<RegionControllerManager>();
            if (controllerManager == null)
            {
                GameObject managerObj = new GameObject("RegionControllerManager");
                controllerManager = managerObj.AddComponent<RegionControllerManager>();
                Debug.Log("MapManager: Created new RegionControllerManager");
            }
        }
        
        #region Event Handlers
        
        private void OnRegionSelected(object data)
        {
            selectionManager.OnRegionSelected(data);
        }
        
        #endregion
        
        // Add a reset method that can be called from editor/inspector
        [ContextMenu("Reset Hex Grid")]
        public void ResetHexGrid()
        {
            CreateHexGrid();
        }
    }
}