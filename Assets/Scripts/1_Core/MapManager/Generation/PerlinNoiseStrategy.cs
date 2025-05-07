using System.Collections.Generic;
using UnityEngine;
using UI.Configuration;
using UI.MapComponents;

namespace UI.Generation
{
    /// <summary>
    /// Map generation strategy that uses Perlin noise to create more natural, varied terrain
    /// </summary>
    public class PerlinNoiseStrategy : IMapGenerationStrategy
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
            
            // Random offsets to make each map unique
            float xOffset = Random.Range(0f, 1000f);
            float yOffset = Random.Range(0f, 1000f);
            
            // Generate cells with Perlin noise variation
            for (int q = 0; q < config.gridWidth; q++)
            {
                for (int r = 0; r < config.gridHeight; r++)
                {
                    string id = $"hex_{q}_{r}";
                    string name = $"Region {q}-{r}";
                    
                    // Base position
                    Vector3 position = gridGenerator.GetHexPosition(q, r);
                    
                    // Sample Perlin noise using coordinates
                    float perlinValue = Mathf.PerlinNoise(
                        (q * config.noiseScale) + xOffset,
                        (r * config.noiseScale) + yOffset
                    );
                    
                    // Skip cells with very low Perlin values to create natural patterns
                    if (perlinValue < 0.3f)
                    {
                        continue;
                    }
                    
                    // Vary height based on Perlin value
                    position.y = (perlinValue - 0.5f) * 0.5f;
                    
                    Quaternion rotation = Quaternion.identity;
                    cells.Add(new HexCellData(id, name, position, rotation, q, r));
                }
            }
            
            return cells;
        }
        
        public string GetStrategyName()
        {
            return "Perlin Noise";
        }
    }
}