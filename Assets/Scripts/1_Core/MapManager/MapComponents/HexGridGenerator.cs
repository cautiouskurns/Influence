using UnityEngine;
using System.Collections.Generic;

namespace UI.MapComponents
{
    /// <summary>
    /// CLASS PURPOSE:
    /// Generate hexagonal grid structures with appropriate spacing and positioning.
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Calculate positions for hex grid cells
    /// - Handle different hex orientations (flat-top vs pointy-top)
    /// - Generate grid data without any GameObject dependencies
    /// </summary>
    public class HexGridGenerator
    {
        // Grid parameters
        private readonly int gridWidth;
        private readonly int gridHeight;
        private readonly float hexSize;
        private readonly bool pointyTopHexes;
        private readonly float horizontalSpacingAdjust;
        private readonly float verticalSpacingAdjust;
        
        // Math constants
        private readonly float SQRT_3 = Mathf.Sqrt(3);
        
        /// <summary>
        /// Structure to hold grid cell data
        /// </summary>
        public struct GridCell
        {
            public int Q; // Grid coordinates
            public int R;
            public Vector3 Position;
            public Quaternion Rotation;
            public string Id;
            public string Name;
            
            public override string ToString()
            {
                return $"Cell {Id} at ({Position.x}, {Position.y})";
            }
        }
        
        /// <summary>
        /// Constructor with all grid parameters
        /// </summary>
        public HexGridGenerator(int width, int height, float size, bool pointyTop, 
                           float hSpacing, float vSpacing)
        {
            gridWidth = width;
            gridHeight = height;
            hexSize = size;
            pointyTopHexes = pointyTop;
            horizontalSpacingAdjust = hSpacing;
            verticalSpacingAdjust = vSpacing;
        }
        
        /// <summary>
        /// Generate positions for all cells in the hex grid
        /// </summary>
        public List<GridCell> GenerateGrid()
        {
            List<GridCell> cells = new List<GridCell>();
            
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
                        
                        // Create the cell with no rotation for pointy-top
                        GridCell cell = new GridCell
                        {
                            Q = q,
                            R = r,
                            Position = position,
                            Rotation = Quaternion.identity,
                            Id = $"Region_{q}_{r}",
                            Name = $"R{q},{r}"
                        };
                        
                        cells.Add(cell);
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
                        
                        // Create the cell with 30-degree rotation for flat-top
                        GridCell cell = new GridCell
                        {
                            Q = q,
                            R = r,
                            Position = position,
                            Rotation = Quaternion.Euler(0, 0, 30),
                            Id = $"Region_{q}_{r}",
                            Name = $"R{q},{r}"
                        };
                        
                        cells.Add(cell);
                    }
                }
            }
            
            return cells;
        }

        /// <summary>
        /// Calculate the position for a single hex cell based on its grid coordinates
        /// </summary>
        public Vector3 GetHexPosition(int q, int r)
        {
            // Calculate hex position based on grid parameters
            float width, height, horizontalSpacing, verticalSpacing, xStart, yStart;
            
            if (pointyTopHexes)
            {
                // Pointy-top hex grid layout (hexes pointing up/down)
                width = hexSize * 2.0f; // Width of a hex (point to point)
                height = hexSize * SQRT_3; // Height of a hex (flat to flat)
                
                // For a perfectly tight grid with pointy-top hexes:
                horizontalSpacing = width * 0.75f * horizontalSpacingAdjust;
                verticalSpacing = height * verticalSpacingAdjust;
                
                xStart = -(gridWidth * horizontalSpacing) / 2 + horizontalSpacing/2;
                yStart = -(gridHeight * verticalSpacing) / 2 + verticalSpacing/2;
                
                float xPos = xStart + q * horizontalSpacing;
                float yPos = yStart + r * verticalSpacing;
                
                // Offset every other row
                if (r % 2 != 0)
                {
                    xPos += horizontalSpacing / 2;
                }
                
                return new Vector3(xPos, yPos, 0);
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
                
                float xPos = xStart + q * horizontalSpacing;
                float yPos = yStart + r * verticalSpacing;
                
                // Offset every other column
                if (q % 2 != 0)
                {
                    yPos += verticalSpacing / 2;
                }
                
                return new Vector3(xPos, yPos, 0);
            }
        }
    }
}