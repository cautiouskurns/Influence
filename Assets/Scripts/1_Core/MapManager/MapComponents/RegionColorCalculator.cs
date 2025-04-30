using UnityEngine;
using System.Collections.Generic;
using Entities;
using Systems;
using Managers;

namespace UI.MapComponents
{
    /// <summary>
    /// CLASS PURPOSE:
    /// Calculate colors for map regions based on different visualization modes.
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Calculate colors based on economic data (wealth, production)
    /// - Calculate colors based on nation ownership
    /// - Calculate colors based on position/coordinate data
    /// </summary>
    public class RegionColorCalculator
    {
        // Color settings
        private readonly Color defaultColor;
        private readonly Color wealthMinColor;
        private readonly Color wealthMaxColor;
        private readonly Color productionMinColor;
        private readonly Color productionMaxColor;
        private readonly Color nationDefaultColor;
        
        // Dependencies
        private readonly EconomicSystem economicSystem;
        private readonly NationManager nationManager;
        
        /// <summary>
        /// Constructor with color settings and dependencies
        /// </summary>
        public RegionColorCalculator(
            Color defaultColor,
            Color wealthMinColor,
            Color wealthMaxColor,
            Color productionMinColor,
            Color productionMaxColor,
            Color nationDefaultColor,
            EconomicSystem economicSystem,
            NationManager nationManager)
        {
            this.defaultColor = defaultColor;
            this.wealthMinColor = wealthMinColor;
            this.wealthMaxColor = wealthMaxColor;
            this.productionMinColor = productionMinColor;
            this.productionMaxColor = productionMaxColor;
            this.nationDefaultColor = nationDefaultColor;
            this.economicSystem = economicSystem;
            this.nationManager = nationManager;
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
                    
                case RegionColorMode.Default:
                default:
                    return defaultColor;
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
            if (economicSystem == null) return defaultColor;
            
            var region = economicSystem.GetRegion(regionId);
            if (region == null) return defaultColor;
            
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
            if (economicSystem == null) return defaultColor;
            
            var region = economicSystem.GetRegion(regionId);
            if (region == null) return defaultColor;
            
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
    }
}