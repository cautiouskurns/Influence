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
        public float InfrastructureQuality => InfrastructureLevel / 10f; // Example calculation
        public float InfrastructureDecayRate => 0.02f; // Example decay rate
        public float InfrastructureMaintenanceCost => 0.05f; // Example maintenance cost factor
        public float ConsumptionRate => 0.2f; // Example consumption rate
        public float WealthConsumptionExponent => 0.8f; // Example exponent for wealth consumption
        public float UnmetDemandUnrestFactor => 0.05f; // Example unrest factor for unmet demand
        public float ResourcePriceVolatility => 0.2f; // Example volatility factor for resource prices
        public Dictionary<string, float> ResourceElasticities { get; set; } = new Dictionary<string, float>
        {
            { "Food", 0.5f },
            { "Luxury", 1.5f },
            { "RawMaterial", 0.8f },
            { "Manufacturing", 1.2f }
        };
        public int Population { get; set; } = 1000; // Example population value
        public int UnmetDemand { get; set; } = 0; // Example unmet demand value
        public int CycleLength { get; set; } = 12; // Example cycle length
        public bool EnableEconomicCycles { get; set; } = true; // Example cycle enable flag
        public Dictionary<string, float> CycleEffects { get; set; } = new Dictionary<string, float>
        {
            { "Production", 1.0f },
            { "Consumption", 1.0f }
        };
        public float PriceElasticity { get; set; } = 0.5f; // Example price elasticity
        public float PriceVolatility { get; set; } = 0.2f; // Example price volatility
        public float PriceSensitivity { get; set; } = 0.5f; // Example price sensitivity
        public float PriceElasticityOfDemand { get; set; } = 0.5f; // Example price elasticity of demand
        public float PriceElasticityOfSupply { get; set; } = 0.5f; // Example price elasticity of supply
        public float PriceElasticityOfSubstitution { get; set; } = 0.5f; // Example price elasticity of substitution
        public float PriceElasticityOfComplement { get; set; } = 0.5f; // Example price elasticity of complement
        public float PriceElasticityOfIncome { get; set; } = 0.5f; // Example price elasticity of income
        public float PriceElasticityOfCross { get; set; } = 0.5f; // Example price elasticity of cross
        public float PriceElasticityOfDemandForLuxury { get; set; } = 0.5f; // Example price elasticity of demand for luxury
        public float PriceElasticityOfDemandForNecessity { get; set; } = 0.5f; // Example price elasticity of demand for necessity
        public float PriceElasticityOfDemandForInferior { get; set; } = 0.5f; // Example price elasticity of demand for inferior
        public float PriceElasticityOfDemandForNormal { get; set; } = 0.5f; // Example price elasticity of demand for normal
        public float PriceElasticityOfDemandForGiffen { get; set; } = 0.5f; // Example price elasticity of demand for Giffen
        public float PriceElasticityOfDemandForVeblen { get; set; } = 0.5f; // Example price elasticity of demand for Veblen
        public float PriceElasticityOfDemandForSnob { get; set; } = 0.5f; // Example price elasticity of demand for snob
        public float PriceElasticityOfDemandForConspicuous { get; set; } = 0.5f; // Example price elasticity of demand for conspicuous
        public float PriceElasticityOfDemandForConspicuousLuxury { get; set; } = 0.5f; // Example price elasticity of demand for conspicuous luxury
        public float PriceElasticityOfDemandForConspicuousNecessity { get; set; } = 0.5f; // Example price elasticity of demand for conspicuous necessity
        public float PriceElasticityOfDemandForConspicuousInferior { get; set; } = 0.5f; // Example price elasticity of demand for conspicuous
        public float PriceElasticityOfDemandForConspicuousNormal { get; set; } = 0.5f; // Example price elasticity of demand for conspicuous normal
        public float PriceElasticityOfDemandForConspicuousGiffen { get; set; } = 0.5f; // Example price elasticity of demand for conspicuous Giffen
        public float PriceElasticityOfDemandForConspicuousVeblen { get; set; } = 0.5f; // Example price elasticity of demand for conspicuous Veblen

        // Constructor with all parameters
        public RegionEntity(string name, int initialWealth, int initialProduction)
        {
            Name = name;
            Wealth = initialWealth;
            Production = initialProduction;
            LaborAvailable = 100;  // Default labor value
            InfrastructureLevel = 5;  // Default infrastructure level
//            Debug.Log($"Created region: {Name} with Wealth: {Wealth}, Production: {Production}");
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
