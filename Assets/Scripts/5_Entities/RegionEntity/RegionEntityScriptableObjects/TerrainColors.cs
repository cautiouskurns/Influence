using UnityEngine;
using System.Collections.Generic;

namespace Entities.ScriptableObjects
{
    /// <summary>
    /// Static class that defines terrain colors for different region types
    /// </summary>
    public static class TerrainColors
    {
        private static readonly Dictionary<RegionConfig.RegionType, Color> colors = 
            new Dictionary<RegionConfig.RegionType, Color>()
            {
                { RegionConfig.RegionType.Desert, new Color(0.95f, 0.85f, 0.6f) },   // Sandy yellow
                { RegionConfig.RegionType.Plains, new Color(0.7f, 0.85f, 0.5f) },    // Light green
                { RegionConfig.RegionType.Forest, new Color(0.2f, 0.55f, 0.3f) },    // Forest green
                { RegionConfig.RegionType.Mountains, new Color(0.6f, 0.6f, 0.6f) },  // Gray
                { RegionConfig.RegionType.Coastal, new Color(0.4f, 0.7f, 0.9f) },    // Light blue
                { RegionConfig.RegionType.Tundra, new Color(0.9f, 0.95f, 0.95f) },   // White-blue
                { RegionConfig.RegionType.Jungle, new Color(0.0f, 0.45f, 0.15f) }    // Dark green
            };
            
        /// <summary>
        /// Get the color for a specific region/terrain type
        /// </summary>
        public static Color GetColor(RegionConfig.RegionType type)
        {
            return colors.TryGetValue(type, out Color color) ? color : Color.gray;
        }
    }
}