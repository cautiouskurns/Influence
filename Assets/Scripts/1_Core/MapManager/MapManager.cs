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
    public enum RegionColorMode
    {
        Default,
        Position,
        Wealth,
        Production,
        Nation,
        Terrain  // New terrain view mode
    }

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
        
        [Header("Color Settings")]
        [SerializeField] private RegionColorMode colorMode = RegionColorMode.Default;
        [SerializeField] private Color defaultRegionColor = new Color(0.5f, 0.5f, 0.7f);
        [SerializeField] private Color wealthMinColor = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private Color wealthMaxColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color productionMinColor = new Color(0.2f, 0.2f, 0.8f);
        [SerializeField] private Color productionMaxColor = new Color(0.8f, 0.8f, 0.2f);
        
        [Header("Nation Colors")]
        [SerializeField] private Color nationDefaultColor = new Color(0.6f, 0.6f, 0.6f);
        
        // Component references
        private HexGridGenerator gridGenerator;
        private RegionColorCalculator colorCalculator;
        private RegionFactory regionFactory;
        private MapSelectionManager selectionManager;
        
        // State
        private Dictionary<string, RegionView> regionViews = new Dictionary<string, RegionView>();
        
        // Dependencies
        private EconomicSystem economicSystem;
        private RegionControllerManager controllerManager;
        
        private void Awake()
        {
            // Get dependencies
            economicSystem = FindFirstObjectByType<EconomicSystem>();
            
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
            EventBus.Subscribe("RegionUpdated", OnRegionUpdated);
            EventBus.Subscribe("UpdateMapColors", OnUpdateMapColors);
            EventBus.Subscribe("RegionNationChanged", OnRegionNationChanged);
            EventBus.Subscribe("RegionsAssignedToNations", OnRegionsAssignedToNations);
        }
        
        private void OnDisable()
        {
            EventBus.Unsubscribe("RegionSelected", OnRegionSelected);
            EventBus.Unsubscribe("RegionUpdated", OnRegionUpdated);
            EventBus.Unsubscribe("UpdateMapColors", OnUpdateMapColors);
            EventBus.Unsubscribe("RegionNationChanged", OnRegionNationChanged);
            EventBus.Unsubscribe("RegionsAssignedToNations", OnRegionsAssignedToNations);
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
            
            // Create the color calculator
            colorCalculator = new RegionColorCalculator(
                defaultRegionColor,
                wealthMinColor,
                wealthMaxColor,
                productionMinColor,
                productionMaxColor,
                nationDefaultColor,
                economicSystem,
                NationManager.Instance
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
                Color regionColor = colorCalculator.GetRegionColor(
                    cell.Id, cell.Q, cell.R, gridWidth, gridHeight, colorMode);
                
                // Create the region with entity
                RegionView regionView = regionFactory.CreateRegionWithEntity(
                    cell.Id, cell.Name, cell.Position, cell.Rotation, regionColor);
                
                // Store reference
                regionViews[cell.Id] = regionView;
            }
            
            Debug.Log($"MapManager created {regionViews.Count} hexagonal regions in a tightly packed hex grid");
            
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
            
            // Create regions based on scenario data
            foreach (var regionCondition in regionStartConditions)
            {
                // Calculate position based on ID or other logic
                string[] idParts = regionCondition.regionId.Split('_');
                int q = 0, r = 0;
                
                // Try to extract coordinates from region ID if possible (format "Region_X_Y")
                if (idParts.Length >= 3)
                {
                    int.TryParse(idParts[1], out q);
                    int.TryParse(idParts[2], out r);
                }
                
                // Calculate position using hex grid layout
                Vector3 position = gridGenerator.GetHexPosition(q, r);
                Quaternion rotation = Quaternion.identity;
                
                // Get appropriate color
                Color regionColor = colorCalculator.GetRegionColor(
                    regionCondition.regionId, q, r, gridWidth, gridHeight, colorMode);
                
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
            }
            
            Debug.Log($"MapManager created {regionViews.Count} custom regions from scenario data");
            
            // Trigger the RegionsCreated event to notify NationManager that regions have been created
            EventBus.Trigger("RegionsCreated", regionViews.Count);
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
        /// Update colors for all regions based on current color mode
        /// </summary>
        public void UpdateRegionColors()
        {
            if (regionViews == null || regionViews.Count == 0)
            {
                Debug.LogWarning("MapManager: No region views to update colors on!");
                return;
            }
            
            Debug.Log($"MapManager: Updating {regionViews.Count} region colors to {colorMode} mode");
            
            foreach (var kvp in regionViews)
            {
                string regionId = kvp.Key;
                RegionView view = kvp.Value;
                
                if (view == null || view.gameObject == null)
                {
                    continue;
                }
                
                // Extract coordinates from region ID (format: "Region_X_Y")
                string[] parts = regionId.Split('_');
                if (parts.Length < 3 || !int.TryParse(parts[1], out int q) || !int.TryParse(parts[2], out int r))
                {
                    continue;
                }
                
                // Calculate new color based on current mode
                Color newColor = colorCalculator.GetRegionColor(
                    regionId, q, r, gridWidth, gridHeight, colorMode);
                
                // Force immediate update on the view
                view.SetColor(newColor);
            }
        }
        
        /// <summary>
        /// Set the color mode for the map
        /// </summary>
        public void SetColorMode(int modeValue)
        {
            // Validate the input mode value
            if (modeValue < 0 || modeValue > System.Enum.GetValues(typeof(RegionColorMode)).Length - 1)
            {
                Debug.LogError($"MapManager: Invalid color mode value: {modeValue}");
                return;
            }
            
            // Convert to enum and assign
            RegionColorMode newMode = (RegionColorMode)modeValue;
            Debug.Log($"MapManager: Changing color mode from {colorMode} to {newMode}");
            colorMode = newMode;
            
            // Update all region colors immediately
            UpdateRegionColors();
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
        
        private void OnRegionUpdated(object data)
        {
            // This is handled by RegionControllers now
        }
        
        private void OnUpdateMapColors(object data)
        {
            UpdateRegionColors();
        }
        
        private void OnRegionNationChanged(object data)
        {
            if (colorMode == RegionColorMode.Nation)
            {
                UpdateRegionColors();
            }
        }
        
        private void OnRegionsAssignedToNations(object data)
        {
            Debug.Log("MapManager: Regions were assigned to nations, updating to nation color mode");
            SetColorMode((int)RegionColorMode.Nation);
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