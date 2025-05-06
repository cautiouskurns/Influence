using UnityEngine;
using System.Collections.Generic;
using Entities;
using Systems;
using Managers;
using Controllers;

namespace UI.MapComponents
{
    /// <summary>
    /// Defines the different visualization modes for region colors on the map
    /// </summary>
    public enum RegionColorMode
    {
        Default,
        Position,
        Wealth,
        Production,
        Nation,
        Terrain
    }
    
    /// <summary>
    /// Contains all the necessary configuration data for displaying map color legends
    /// </summary>
    [System.Serializable]
    public class LegendConfiguration
    {
        public string Title;
        public bool ShowLegend;
        public Color MinColor;
        public Color MaxColor;
        public string MinLabel;
        public string MaxLabel;
        
        public LegendConfiguration()
        {
            // Default values
            Title = "Default";
            ShowLegend = true;
            MinColor = Color.white;
            MaxColor = Color.white;
            MinLabel = "Min";
            MaxLabel = "Max";
        }
    }

    /// <summary>
    /// CLASS PURPOSE:
    /// Centralized service for managing the colors of map regions based on different visualization modes.
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Calculate colors based on economic data (wealth, production)
    /// - Calculate colors based on nation ownership
    /// - Calculate colors based on position/coordinate data
    /// - Calculate colors based on terrain type
    /// - Manage color mode switching and updates
    /// - Handle region color-related events
    /// </summary>
    public class RegionColorService : MonoBehaviour
    {
        #region Singleton Pattern
        private static RegionColorService _instance;
        public static RegionColorService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<RegionColorService>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("RegionColorService");
                        _instance = go.AddComponent<RegionColorService>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        [Header("Color Settings")]
        [SerializeField] private RegionColorMode colorMode = RegionColorMode.Default;
        [SerializeField] private Color defaultRegionColor = new Color(0.5f, 0.5f, 0.7f);
        [SerializeField] private Color wealthMinColor = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private Color wealthMaxColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color productionMinColor = new Color(0.2f, 0.2f, 0.8f);
        [SerializeField] private Color productionMaxColor = new Color(0.8f, 0.8f, 0.2f);
        [SerializeField] private Color nationDefaultColor = new Color(0.6f, 0.6f, 0.6f);

        // Dependencies
        private EconomicSystem economicSystem;
        private NationManager nationManager;
        
        // Cache for grid dimensions - updated when Initialize is called
        private int gridWidth = 8; 
        private int gridHeight = 8;
        
        // References to map objects
        private Dictionary<string, RegionView> regionViews = new Dictionary<string, RegionView>();

        private void Awake()
        {
            // Singleton pattern check
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            
            // Find dependencies
            economicSystem = FindFirstObjectByType<EconomicSystem>();
            nationManager = NationManager.Instance;
        }

        private void OnEnable()
        {
            // Subscribe to events
            EventBus.Subscribe("RegionNationChanged", OnRegionNationChanged);
            EventBus.Subscribe("UpdateMapColors", OnUpdateMapColors);
            EventBus.Subscribe("RegionsAssignedToNations", OnRegionsAssignedToNations);
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            EventBus.Unsubscribe("RegionNationChanged", OnRegionNationChanged);
            EventBus.Unsubscribe("UpdateMapColors", OnUpdateMapColors);
            EventBus.Unsubscribe("RegionsAssignedToNations", OnRegionsAssignedToNations);
        }

        /// <summary>
        /// Initialize the service with references to map objects
        /// </summary>
        public void Initialize(Dictionary<string, RegionView> regionViews, int gridWidth, int gridHeight)
        {
            this.regionViews = regionViews;
            this.gridWidth = gridWidth;
            this.gridHeight = gridHeight;
            Debug.Log($"RegionColorService: Initialized with {regionViews.Count} region views");
        }

        /// <summary>
        /// Set the color mode for the map and update all region colors
        /// </summary>
        public void SetColorMode(int modeValue)
        {
            // Validate the input mode value
            if (modeValue < 0 || modeValue > System.Enum.GetValues(typeof(RegionColorMode)).Length - 1)
            {
                Debug.LogError($"RegionColorService: Invalid color mode value: {modeValue}");
                return;
            }
            
            // Convert to enum and assign
            RegionColorMode newMode = (RegionColorMode)modeValue;
            Debug.Log($"RegionColorService: Changing color mode from {colorMode} to {newMode}");
            colorMode = newMode;
            
            // Update all region colors immediately
            UpdateAllRegionColors();
        }

        /// <summary>
        /// Set the color mode directly with the enum value
        /// </summary>
        public void SetColorMode(RegionColorMode mode)
        {
            colorMode = mode;
            UpdateAllRegionColors();
        }

        /// <summary>
        /// Get the current color mode
        /// </summary>
        public RegionColorMode GetColorMode()
        {
            return colorMode;
        }

        /// <summary>
        /// Update colors for all regions based on current color mode
        /// </summary>
        public void UpdateAllRegionColors()
        {
            if (regionViews == null || regionViews.Count == 0)
            {
                Debug.LogWarning("RegionColorService: No region views to update colors on!");
                return;
            }
            
            Debug.Log($"RegionColorService: Updating {regionViews.Count} region colors to {colorMode} mode");
            
            foreach (var kvp in regionViews)
            {
                UpdateRegionColor(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Update the color for a specific region
        /// </summary>
        public void UpdateRegionColor(string regionId, RegionView view)
        {
            if (view == null || view.gameObject == null)
            {
                return;
            }
            
            // Extract coordinates from region ID (format: "Region_X_Y" or "region_X_Y")
            string[] parts = regionId.Split('_');
            if (parts.Length < 3 || !int.TryParse(parts[1], out int q) || !int.TryParse(parts[2], out int r))
            {
                Debug.LogWarning($"RegionColorService: Could not parse coordinates from region ID: {regionId}. Using default color.");
                view.SetColor(defaultRegionColor);
                return;
            }
            
            // Calculate new color based on current mode
            Color newColor = GetRegionColor(regionId, q, r, gridWidth, gridHeight, colorMode);
            
            // Force immediate update on the view
            view.SetColor(newColor);
        }

        /// <summary>
        /// Get a color for a region based on the current color mode
        /// </summary>
        public Color GetRegionColor(string regionId, int q, int r, int gridWidth, int gridHeight, RegionColorMode colorMode)
        {
            switch (colorMode)
            {
                case RegionColorMode.Position:
                    return GetPositionBasedColor(q, r, gridWidth, gridHeight);
                    
                case RegionColorMode.Wealth:
                    return GetWealthBasedColor(regionId);
                    
                case RegionColorMode.Production:
                    return GetProductionBasedColor(regionId);
                    
                case RegionColorMode.Nation:
                    return GetNationBasedColor(regionId);
                    
                case RegionColorMode.Terrain:
                    return GetTerrainBasedColor(regionId);
                    
                case RegionColorMode.Default:
                default:
                    return defaultRegionColor;
            }
        }
        
        /// <summary>
        /// Get a color based on the region's position in the grid
        /// </summary>
        private Color GetPositionBasedColor(int q, int r, int gridWidth, int gridHeight)
        {
            // Calculate color based on normalized position in grid
            return new Color(
                0.4f + (float)q/gridWidth * 0.6f,
                0.4f + (float)r/gridHeight * 0.6f,
                0.5f
            );
        }
        
        /// <summary>
        /// Get a color based on the region's wealth value
        /// </summary>
        private Color GetWealthBasedColor(string regionId)
        {
            if (economicSystem == null) return defaultRegionColor;
            
            var region = economicSystem.GetRegion(regionId);
            if (region == null) return defaultRegionColor;
            
            // Get min/max wealth values in the economy
            int minWealth = int.MaxValue;
            int maxWealth = int.MinValue;
            
            foreach (var entityId in economicSystem.GetAllRegionIds())
            {
                var entity = economicSystem.GetRegion(entityId);
                if (entity != null)
                {
                    minWealth = Mathf.Min(minWealth, entity.Wealth);
                    maxWealth = Mathf.Max(maxWealth, entity.Wealth);
                }
            }
            
            // Safeguard against division by zero
            if (minWealth == maxWealth) return Color.Lerp(wealthMinColor, wealthMaxColor, 0.5f);
            
            // Normalize the value between 0 and 1
            float normalizedValue = (float)(region.Wealth - minWealth) / (maxWealth - minWealth);
            
            // Return color gradient based on wealth
            return Color.Lerp(wealthMinColor, wealthMaxColor, normalizedValue);
        }
        
        /// <summary>
        /// Get a color based on the region's production value
        /// </summary>
        private Color GetProductionBasedColor(string regionId)
        {
            if (economicSystem == null) return defaultRegionColor;
            
            var region = economicSystem.GetRegion(regionId);
            if (region == null) return defaultRegionColor;
            
            // Get min/max production values in the economy
            int minProduction = int.MaxValue;
            int maxProduction = int.MinValue;
            
            foreach (var entityId in economicSystem.GetAllRegionIds())
            {
                var entity = economicSystem.GetRegion(entityId);
                if (entity != null)
                {
                    minProduction = Mathf.Min(minProduction, entity.Production);
                    maxProduction = Mathf.Max(maxProduction, entity.Production);
                }
            }
            
            // Safeguard against division by zero
            if (minProduction == maxProduction) return Color.Lerp(productionMinColor, productionMaxColor, 0.5f);
            
            // Normalize the value between 0 and 1
            float normalizedValue = (float)(region.Production - minProduction) / (maxProduction - minProduction);
            
            // Return color gradient based on production
            return Color.Lerp(productionMinColor, productionMaxColor, normalizedValue);
        }
        
        /// <summary>
        /// Get a color based on the region's nation ownership
        /// </summary>
        private Color GetNationBasedColor(string regionId)
        {
            if (nationManager == null) return nationDefaultColor;
            
            // Get the nation that owns this region
            NationEntity nation = nationManager.GetRegionNation(regionId);
            if (nation == null) return nationDefaultColor;
            
            // Return the nation's color
            return nation.Color;
        }
        
        /// <summary>
        /// Get a color based on the region's terrain type
        /// </summary>
        private Color GetTerrainBasedColor(string regionId)
        {
            // Extract coordinates from region ID (format: "Region_X_Y")
            string[] parts = regionId.Split('_');
            if (parts.Length < 3 || !int.TryParse(parts[1], out int q) || !int.TryParse(parts[2], out int r))
            {
                return defaultRegionColor;
            }
            
            // Use a combination of coordinates to create terrain "zones"
            // This creates a natural-looking terrain distribution pattern
            float noiseValue = Mathf.PerlinNoise(q * 0.3f, r * 0.3f);
            
            // Map noise value to terrain types
            if (noiseValue < 0.3f)
            {
                // Desert
                return new Color(0.95f, 0.85f, 0.6f);
            }
            else if (noiseValue < 0.5f)
            {
                // Plains
                return new Color(0.7f, 0.85f, 0.5f);
            }
            else if (noiseValue < 0.7f)
            {
                // Forest
                return new Color(0.2f, 0.55f, 0.3f);
            }
            else if (noiseValue < 0.85f)
            {
                // Mountains
                return new Color(0.6f, 0.6f, 0.6f);
            }
            else
            {
                // Tundra
                return new Color(0.9f, 0.95f, 0.95f);
            }
        }

        /// <summary>
        /// Get legend configuration data for a specific color mode
        /// </summary>
        /// <param name="mode">The color mode to get legend data for</param>
        /// <returns>A structure containing all necessary legend information</returns>
        public LegendConfiguration GetLegendConfiguration(RegionColorMode mode)
        {
            LegendConfiguration config = new LegendConfiguration();
            
            // Set legend attributes based on color mode
            switch (mode)
            {
                case RegionColorMode.Default:
                    config.Title = "Default";
                    config.ShowLegend = false;
                    config.MinColor = defaultRegionColor;
                    config.MaxColor = defaultRegionColor;
                    config.MinLabel = "Uniform";
                    config.MaxLabel = "Uniform";
                    break;
                
                case RegionColorMode.Position:
                    config.Title = "Position";
                    config.ShowLegend = true;
                    config.MinColor = new Color(0.4f, 0.4f, 0.5f);
                    config.MaxColor = new Color(1.0f, 1.0f, 0.5f);
                    config.MinLabel = "Top-Left";
                    config.MaxLabel = "Bottom-Right";
                    break;
                
                case RegionColorMode.Wealth:
                    config.Title = "Wealth";
                    config.ShowLegend = true;
                    config.MinColor = wealthMinColor;
                    config.MaxColor = wealthMaxColor;
                    config.MinLabel = "Poor";
                    config.MaxLabel = "Wealthy";
                    break;
                
                case RegionColorMode.Production:
                    config.Title = "Production";
                    config.ShowLegend = true;
                    config.MinColor = productionMinColor;
                    config.MaxColor = productionMaxColor;
                    config.MinLabel = "Low";
                    config.MaxLabel = "High";
                    break;
                    
                case RegionColorMode.Nation:
                    config.Title = "Nation";
                    config.ShowLegend = true;
                    config.MinColor = nationDefaultColor;
                    config.MaxColor = new Color(1.0f, 0.5f, 0.5f);
                    config.MinLabel = "Nation A";
                    config.MaxLabel = "Nation B";
                    break;
                    
                case RegionColorMode.Terrain:
                    config.Title = "Terrain Type";
                    config.ShowLegend = true;
                    config.MinColor = new Color(0.7f, 0.85f, 0.5f); // Plains
                    config.MaxColor = new Color(0.95f, 0.85f, 0.6f); // Desert
                    config.MinLabel = "Plains";
                    config.MaxLabel = "Desert";
                    break;
            }
            
            return config;
        }

        #region Event Handlers

        private void OnUpdateMapColors(object data)
        {
            UpdateAllRegionColors();
        }

        private void OnRegionNationChanged(object data)
        {
            if (colorMode == RegionColorMode.Nation)
            {
                // If we have the specific region data, update just that region
                if (data is RegionNationChangedData changeData)
                {
                    if (regionViews.TryGetValue(changeData.RegionId, out RegionView view))
                    {
                        UpdateRegionColor(changeData.RegionId, view);
                    }
                }
                else
                {
                    // Otherwise update all regions
                    UpdateAllRegionColors();
                }
            }
        }

        private void OnRegionsAssignedToNations(object data)
        {
            Debug.Log("RegionColorService: Regions were assigned to nations, updating to nation color mode");
            SetColorMode(RegionColorMode.Nation);
        }

        #endregion
    }
}