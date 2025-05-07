using System.Collections.Generic;
using UnityEngine;
using UI.Configuration;
using UI.MapComponents;

namespace UI.Generation
{
    /// <summary>
    /// Map generation strategy that creates island-like formations
    /// </summary>
    public class IslandStrategy : IMapGenerationStrategy
    {
        public List<HexCellData> GenerateMap(MapGenerationConfig config)
        {
            List<HexCellData> cells = new List<HexCellData>();
            HexGridGenerator gridGenerator = new HexGridGenerator(
                config.gridWidth, 
                config.gridHeight, 
                config.hexSize, 
                config.pointyTopHexes, 
                config.horizontalSpacingAdjust, 
                config.verticalSpacingAdjust
            );
            
            // Use terrain seed or generate random one
            int seed = config.terrainSeed == 0 ? 
                Random.Range(1, 100000) : config.terrainSeed;
            
            // Set random seed for consistent generation
            Random.InitState(seed);
            
            // Center of the island
            Vector2 center = new Vector2(config.gridWidth / 2f, config.gridHeight / 2f);
            
            // Maximum distance from center (used for falloff calculation)
            float maxDistance = Mathf.Max(config.gridWidth, config.gridHeight) / 2f;
            
            // Generate cells with distance-based falloff for island shape
            for (int q = 0; q < config.gridWidth; q++)
            {
                for (int r = 0; r < config.gridHeight; r++)
                {
                    string id = $"hex_{q}_{r}";
                    string name = $"Region {q}-{r}";
                    
                    // Calculate distance from center
                    float distanceFromCenter = Vector2.Distance(new Vector2(q, r), center);
                    
                    // Normalize distance (0 to 1 range)
                    float normalizedDistance = distanceFromCenter / maxDistance;
                    
                    // Apply falloff function to create island shape
                    // Higher falloffStrength makes the island smaller and more compact
                    float falloffStrength = 2.5f;
                    float falloff = Mathf.Pow(normalizedDistance, falloffStrength);
                    
                    // Add some noise to make edges irregular
                    float noise = Mathf.PerlinNoise(
                        (q * config.noiseScale) + seed, 
                        (r * config.noiseScale) + seed
                    ) * 0.3f;
                    
                    // Skip cells that fall outside the island boundary
                    if (falloff - noise > 0.5f)
                    {
                        continue;
                    }
                    
                    // Base position
                    Vector3 position = gridGenerator.GetHexPosition(q, r);
                    
                    // Elevation based on distance from center and noise
                    // Centers are higher, gradually sloping down to edges
                    position.y = (0.5f - falloff) * 0.8f;
                    
                    Quaternion rotation = Quaternion.identity;
                    cells.Add(new HexCellData(id, name, position, rotation, q, r));
                }
            }
            
            return cells;
        }
        
        public string GetStrategyName()
        {
            return "Island";
        }
    }
}