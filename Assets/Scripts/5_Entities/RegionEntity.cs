using UnityEngine;
using System.Collections.Generic;
using Managers;
using Entities.Components;

namespace Entities
{
    /// <summary>
    /// CLASS PURPOSE:
    /// RegionEntity represents a single economic unit in the simulation. It now follows 
    /// the Single Responsibility Principle by focusing purely on being a data container.
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Store basic region identity and properties
    /// - Maintain references to associated models (economic, infrastructure)
    /// - Provide access to basic data
    /// </summary>
    public class RegionEntity
    {
        // Identity
        public string Id { get; private set; }
        public string Name { get; private set; }
        
        // Nation ownership
        public string NationId { get; set; }
        
        // Basic economic properties
        public int Wealth { get; set; }
        public int Production { get; set; }
        
        // Core resources
        public float LaborAvailable { get; set; }
        public float InfrastructureLevel { get; set; }

        public float InfrastructureQuality { get; set; }        
        
        // Population
        public int Population { get; set; }
        
        // Component references
        public ResourceComponent Resources { get; private set; }
        
        // Constructor with required parameters
        public RegionEntity(string id, string name, int initialWealth, int initialProduction)
        {
            Id = id;
            Name = name;
            Wealth = initialWealth;
            Production = initialProduction;
            LaborAvailable = 100;  // Default labor value
            InfrastructureLevel = 5;  // Default infrastructure level
            Population = 1000;     // Default population
            
            // Initialize components
            Resources = new ResourceComponent();
        }

        // Additional constructor with default values
        public RegionEntity(string name) : this(name, name, 100, 50)
        {
            // This calls the main constructor with default values
        }
        
        // Backwards compatibility constructor
        public RegionEntity(string name, int initialWealth, int initialProduction) 
            : this(name, name, initialWealth, initialProduction)
        {
            // This provides compatibility with existing code using the old constructor
        }

        // Basic summary of the region
        public string GetSummary()
        {
            string summary = $"Region: {Name}\n" +
                   $"Nation ID: {NationId ?? "None"}\n" +
                   $"Wealth: {Wealth}\n" +
                   $"Production: {Production}\n" +
                   $"Infrastructure: {InfrastructureLevel:F1}\n";
                   
            // Add resource summary
            summary += Resources.GetSummary();
            
            return summary;
        }
        
        // Process all production for one turn
        public void ProcessTurn()
        {
            // Process resource production
            Resources.ProcessProduction();
            
            // Rest of turn processing logic can be added here
        }
        
        // Convenience method for backward compatibility
        public NationEntity GetNation()
        {
            if (string.IsNullOrEmpty(NationId))
                return null;
                
            return NationManager.Instance?.GetNation(NationId);
        }
    }
}
