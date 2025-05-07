using System.Collections.Generic;
using UnityEngine;
using UI.MapComponents;
using UI.Configuration;

namespace UI.Generation
{
    /// <summary>
    /// Interface for different map generation strategies
    /// </summary>
    public interface IMapGenerationStrategy
    {
        /// <summary>
        /// Generate a map and return a list of hex cell data
        /// </summary>
        /// <param name="config">The map generation configuration</param>
        /// <returns>A list of HexCellData containing position and other data for each cell</returns>
        List<HexCellData> GenerateMap(MapGenerationConfig config);
        
        /// <summary>
        /// Get the name of this strategy
        /// </summary>
        string GetStrategyName();
    }
    
    /// <summary>
    /// Data class to hold hex cell information
    /// </summary>
    public class HexCellData
    {
        public string Id;
        public string Name;
        public Vector3 Position;
        public Quaternion Rotation;
        public int Q; // Hex grid q coordinate
        public int R; // Hex grid r coordinate
        
        public HexCellData(string id, string name, Vector3 position, Quaternion rotation, int q, int r)
        {
            Id = id;
            Name = name;
            Position = position;
            Rotation = rotation;
            Q = q;
            R = r;
        }
    }
}