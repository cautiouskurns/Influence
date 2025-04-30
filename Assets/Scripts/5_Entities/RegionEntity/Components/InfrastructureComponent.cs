using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Entities.Components
{
    /// <summary>
    /// CLASS PURPOSE:
    /// Handles infrastructure management for a region
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Track infrastructure level and quality
    /// - Manage maintenance costs and degradation
    /// - Calculate infrastructure effects on production and satisfaction
    /// </summary>
    public class InfrastructureComponent
    {
        // Core infrastructure metrics
        public float Level { get; private set; }
        public float Quality { get; private set; }
        public float MaintenanceCost { get; private set; }
        
        // Infrastructure aspects and their states
        private Dictionary<string, float> aspects = new Dictionary<string, float>();
        
        /// <summary>
        /// Initialize with default values
        /// </summary>
        public InfrastructureComponent(float initialLevel = 5.0f, float initialQuality = 0.5f)
        {
            Level = initialLevel;
            Quality = Mathf.Clamp01(initialQuality);
            
            // Calculate initial maintenance cost
            MaintenanceCost = CalculateMaintenanceCost();
            
            // Set up default infrastructure aspects
            aspects.Add("Roads", 0.5f);
            aspects.Add("Buildings", 0.5f);
            aspects.Add("Utilities", 0.5f);
        }
        
        /// <summary>
        /// Invest in infrastructure to improve it
        /// </summary>
        public float Invest(float investmentAmount)
        {
            if (investmentAmount <= 0)
                return 0;
                
            // Higher levels require more investment for the same improvement
            float effectiveness = 1.0f / (Level + 1.0f);
            
            // Calculate level increase (diminishing returns)
            float increase = investmentAmount * effectiveness * 0.1f;
            Level += increase;
            
            // Improve quality with investment
            Quality = Mathf.Clamp01(Quality + (increase * 0.05f));
            
            // Update maintenance cost
            MaintenanceCost = CalculateMaintenanceCost();
            
            return increase;
        }
        
        /// <summary>
        /// Calculate maintenance cost based on level and quality
        /// </summary>
        private float CalculateMaintenanceCost()
        {
            // Higher level = higher maintenance cost
            // Higher quality = more efficient (slightly lower cost)
            return Level * (2.0f - Quality * 0.5f);
        }
        
        /// <summary>
        /// Apply maintenance effects to infrastructure
        /// </summary>
        public void ApplyMaintenance(float maintenanceFunding)
        {
            // Calculate what percentage of required maintenance is funded
            float maintenanceRatio = maintenanceFunding / MaintenanceCost;
            
            // Quality decreases if underfunded, improves slightly if fully funded
            if (maintenanceRatio < 0.8f)
            {
                // Underfunded maintenance causes quality deterioration
                Quality = Mathf.Max(0.1f, Quality - (0.05f * (1.0f - maintenanceRatio)));
            }
            else if (maintenanceRatio > 1.2f)
            {
                // Overfunded maintenance improves quality
                Quality = Mathf.Min(1.0f, Quality + (0.02f * (maintenanceRatio - 1.0f)));
            }
            
            // Update maintenance cost based on new quality
            MaintenanceCost = CalculateMaintenanceCost();
        }
        
        /// <summary>
        /// Process infrastructure changes for one turn
        /// </summary>
        public void ProcessTurn(float maintenanceFunding)
        {
            // Apply maintenance effects
            ApplyMaintenance(maintenanceFunding);
            
            // Natural degradation of infrastructure (very slow)
            Level = Mathf.Max(1.0f, Level - 0.01f);
            
            // Update aspects based on current level and quality
            foreach (var aspect in aspects.Keys.ToArray())
            {
                aspects[aspect] = Mathf.Clamp01((Level / 10.0f) * Quality);
            }
        }
        
        /// <summary>
        /// Get the production modifier from infrastructure
        /// </summary>
        public float GetProductionModifier()
        {
            // Infrastructure provides a boost to production based on level and quality
            return 0.5f + ((Level / 10.0f) * Quality);
        }
        
        /// <summary>
        /// Get a summary of the infrastructure status
        /// </summary>
        public string GetSummary()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Infrastructure:");
            sb.AppendLine($"  Level: {Level:F1}");
            
            string qualityDesc = Quality > 0.8f ? "Excellent" :
                                Quality > 0.6f ? "Good" :
                                Quality > 0.4f ? "Average" :
                                Quality > 0.2f ? "Poor" : "Terrible";
            sb.AppendLine($"  Quality: {qualityDesc} ({Quality:P0})");
            sb.AppendLine($"  Maintenance Cost: {MaintenanceCost:F1}");
            
            sb.AppendLine("Aspects:");
            foreach (var aspect in aspects)
            {
                sb.AppendLine($"  {aspect.Key}: {aspect.Value:P0}");
            }
            
            return sb.ToString();
        }
    }
}