using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace Core
{
    /// <summary>
    /// ScriptableObject that holds all global game settings and configuration values.
    /// Centralizes game parameters for easy tuning and modification.
    /// </summary>
    [CreateAssetMenu(fileName = "DefaultGameSettings", menuName = "Game/Settings/GameSettings")]
    public class GameSettings : ScriptableObject
    {
        [Header("Simulation Settings")]
        [Tooltip("Starting turn number for new games")]
        public int startingTurn = 1;
        
        [Tooltip("Whether simulation should start paused")]
        public bool startPaused = true;
        
        [Tooltip("Base simulation speed (1 = normal)")]
        [Range(0.1f, 10f)]
        public float baseSimulationSpeed = 1f;
        
        [Tooltip("Turn duration in seconds")]
        [Range(0.1f, 5f)]
        public float turnDuration = 1f;
        
        [Header("Population Settings")]
        [Tooltip("Base population growth rate per turn")]
        [Range(0.001f, 0.1f)]
        public float populationGrowthRate = 0.02f;
        
        [Tooltip("Base migration rate between regions per turn")]
        [Range(0.001f, 0.1f)]
        public float migrationRate = 0.01f;
        
        [Tooltip("Maximum population density per unit area")]
        public float maxPopulationDensity = 100f;
        
        [Tooltip("Infrastructure level needed for max growth bonus")]
        public float infrastructureGrowthThreshold = 10f;
        
        [Header("Economic Settings")]
        [Tooltip("Base production efficiency multiplier")]
        [Range(0.5f, 2f)]
        public float baseProductionEfficiency = 1.0f;
        
        [Tooltip("Base trade efficiency multiplier")]
        [Range(0.5f, 2f)]
        public float baseTradeEfficiency = 1.0f;
        
        [Tooltip("Base consumption rate per population unit")]
        public float baseConsumptionRate = 0.5f;
        
        [Header("Resource Settings")]
        [Tooltip("Base resource regeneration rate")]
        [Range(0.001f, 0.1f)]
        public float resourceRegenerationRate = 0.005f;
        
        [Tooltip("Maximum resource extraction efficiency")]
        [Range(0.1f, 1f)]
        public float maxExtractionEfficiency = 0.8f;
        
        [Header("UI Settings")]
        [Tooltip("How long notifications stay visible (in seconds)")]
        public float notificationDuration = 3f;
        
        [Tooltip("Maximum zoom level for map camera")]
        public float maxZoomLevel = 10f;
        
        [Tooltip("Minimum zoom level for map camera")]
        public float minZoomLevel = 2f;

        /// <summary>
        /// Validates all settings to ensure they're within reasonable bounds
        /// Called during OnValidate in the editor
        /// </summary>
        public void ValidateSettings()
        {
            // Ensure turns never start below 1
            startingTurn = Mathf.Max(1, startingTurn);
            
            // Ensure speeds are positive
            baseSimulationSpeed = Mathf.Max(0.1f, baseSimulationSpeed);
            turnDuration = Mathf.Max(0.1f, turnDuration);
            
            // Ensure population rates are positive but not too high
            populationGrowthRate = Mathf.Clamp(populationGrowthRate, 0.001f, 0.1f);
            migrationRate = Mathf.Clamp(migrationRate, 0.001f, 0.1f);
            maxPopulationDensity = Mathf.Max(1f, maxPopulationDensity);
            
            // Ensure economic settings are reasonable
            baseProductionEfficiency = Mathf.Max(0.1f, baseProductionEfficiency);
            baseTradeEfficiency = Mathf.Max(0.1f, baseTradeEfficiency);
            baseConsumptionRate = Mathf.Max(0.01f, baseConsumptionRate);
            
            // Ensure resource settings are positive
            resourceRegenerationRate = Mathf.Clamp(resourceRegenerationRate, 0.001f, 0.1f);
            maxExtractionEfficiency = Mathf.Clamp(maxExtractionEfficiency, 0.1f, 1f);
            
            // Validate UI settings
            notificationDuration = Mathf.Max(1f, notificationDuration);
            maxZoomLevel = Mathf.Max(minZoomLevel + 1f, maxZoomLevel);
        }
    }
}