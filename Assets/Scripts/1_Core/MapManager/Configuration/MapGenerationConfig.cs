using UnityEngine;

namespace UI.Configuration
{
    /// <summary>
    /// ScriptableObject that holds configuration settings for map generation
    /// </summary>
    [CreateAssetMenu(fileName = "MapGenerationConfig", menuName = "Influence/Map Generation Config", order = 1)]
    public class MapGenerationConfig : ScriptableObject
    {
        [Header("Grid Settings")]
        [Tooltip("Width of the hex grid in cells")]
        public int gridWidth = 8;
        
        [Tooltip("Height of the hex grid in cells")]
        public int gridHeight = 8;
        
        [Tooltip("Size of each hex cell")]
        public float hexSize = 1.0f;
        
        [Tooltip("Whether hexes should be oriented with the point at the top")]
        public bool pointyTopHexes = false;
        
        [Tooltip("Horizontal spacing adjustment factor")]
        [Range(0.5f, 2.0f)]
        public float horizontalSpacingAdjust = 1.0f;
        
        [Tooltip("Vertical spacing adjustment factor")]
        [Range(0.5f, 2.0f)]
        public float verticalSpacingAdjust = 1.0f;
        
        [Header("Generation Settings")]
        [Tooltip("Scale of Perlin noise (lower = more gradual changes)")]
        [Range(0.05f, 1.0f)]
        public float noiseScale = 0.3f;
        
        [Tooltip("Seed for terrain generation (0 for random)")]
        public int terrainSeed = 0;
        
        [Header("Nation Layout Settings")]
        [Tooltip("Spacing between nation clusters")]
        [Range(1, 10)]
        public int nationSpacing = 3;
        
        [Tooltip("Whether nations should be placed in clusters")]
        public bool clusterNations = true;
    }
}