using System.Collections.Generic;
using UnityEngine;
using UI.Configuration;

namespace UI.Generation
{
    /// <summary>
    /// Factory for creating map generation strategies
    /// </summary>
    public class MapGenerationFactory
    {
        public enum StrategyType
        {
            StandardGrid,
            PerlinNoise,
            Island
        }
        
        /// <summary>
        /// Get a map generation strategy based on the type
        /// </summary>
        /// <param name="type">The type of strategy to create</param>
        /// <returns>The instantiated strategy</returns>
        public IMapGenerationStrategy GetStrategy(StrategyType type)
        {
            switch (type)
            {
                case StrategyType.StandardGrid:
                    return new StandardGridStrategy();
                case StrategyType.PerlinNoise:
                    return new PerlinNoiseStrategy();
                case StrategyType.Island:
                    return new IslandStrategy();
                default:
                    Debug.LogWarning($"Unknown strategy type: {type}. Using StandardGrid as fallback.");
                    return new StandardGridStrategy();
            }
        }
    }
}