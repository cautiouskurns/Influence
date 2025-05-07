using System.Collections.Generic;
using UnityEngine;
using UI.Configuration;
using UI.MapComponents;

namespace UI.Generation
{
    /// <summary>
    /// Standard grid map generation strategy that creates a uniform hex grid
    /// </summary>
    public class StandardGridStrategy : IMapGenerationStrategy
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
            
            // Generate cells in a grid pattern
            for (int q = 0; q < config.gridWidth; q++)
            {
                for (int r = 0; r < config.gridHeight; r++)
                {
                    string id = $"hex_{q}_{r}";
                    string name = $"Region {q}-{r}";
                    Vector3 position = gridGenerator.GetHexPosition(q, r);
                    Quaternion rotation = Quaternion.identity;
                    
                    cells.Add(new HexCellData(id, name, position, rotation, q, r));
                }
            }
            
            return cells;
        }
        
        public string GetStrategyName()
        {
            return "Standard Grid";
        }
    }
}