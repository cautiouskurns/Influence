using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Entities
{
    /// <summary>
    /// CLASS PURPOSE:
    /// RegionEntity represents a single economic unit in the simulation. It encapsulates basic
    /// economic state (wealth, production) and delegates resource and production behavior
    /// to internal components.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Maintain core simulation variables: wealth and production
    /// - Delegate resource generation and production processing to internal components
    /// - React to global turn-based events via `ProcessTurn`
    /// - Emit region updates using the EventBus
    /// </summary>
    public class RegionEntity
    {
        // Identity
        public string Name { get; set; }
        
        // Basic economic properties
        public int Wealth { get; set; }
        public int Production { get; set; }
        public float LaborAvailable { get; set; }
        public float InfrastructureLevel { get; set; }

        // Constructor with all parameters
        public RegionEntity(string name, int initialWealth, int initialProduction)
        {
            Name = name;
            Wealth = initialWealth;
            Production = initialProduction;
            LaborAvailable = 100;  // Default labor value
            InfrastructureLevel = 5;  // Default infrastructure level
            Debug.Log($"Created region: {Name} with Wealth: {Wealth}, Production: {Production}");
        }

        // Additional constructor with default values
        public RegionEntity(string name) : this(name, 100, 50)
        {
            // This calls the main constructor with default values
            // Default wealth: 100
            // Default production: 50
        }

        public void ProcessTurn()
        {
            Debug.Log($"[Region: {Name}] Processing Turn...");
        }

        public string GetSummary()
        {
            return $"[{Name}] Wealth: {Wealth}, Production: {Production}, Labor: {LaborAvailable}, Infrastructure: {InfrastructureLevel}";
        }
    }
}
