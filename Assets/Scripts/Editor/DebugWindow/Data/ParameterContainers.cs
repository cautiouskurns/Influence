using System.Collections.Generic;

namespace Editor.DebugWindow.Data
{
    /// <summary>
    /// Container for economic production parameters
    /// </summary>
    public class ProductionParameters
    {
        public float productivityFactor = 1.0f;
        public float laborElasticity = 0.5f;
        public float capitalElasticity = 0.5f;
    }
    
    /// <summary>
    /// Container for infrastructure parameters
    /// </summary>
    public class InfrastructureParameters
    {
        public float efficiencyModifier = 0.1f;
        public float decayRate = 0.02f;
        public float maintenanceCostFactor = 0.05f;
    }
    
    /// <summary>
    /// Container for consumption parameters
    /// </summary>
    public class ConsumptionParameters
    {
        public float baseConsumptionRate = 0.2f;
        public float wealthConsumptionExponent = 0.8f;
        public float unmetDemandUnrestFactor = 0.05f;
    }
    
    /// <summary>
    /// Container for price parameters
    /// </summary>
    public class PriceParameters
    {
        public Dictionary<string, float> resourceElasticities = new Dictionary<string, float>
        {
            { "Food", 0.5f },
            { "Luxury", 1.5f },
            { "RawMaterial", 0.8f },
            { "Manufacturing", 1.2f }
        };
        public float volatilityFactor = 0.2f;
    }
    
    /// <summary>
    /// Container for economic cycle parameters
    /// </summary>
    public class EconomicCycleParameters
    {
        public int cycleLength = 12;
        public bool enableEconomicCycles = true;
        public Dictionary<string, float> cycleEffects = new Dictionary<string, float>
        {
            { "Production", 1.0f },
            { "Consumption", 1.0f },
            { "Investment", 1.0f },
            { "PriceInflation", 1.0f },
            { "Unrest", 1.0f }
        };
    }
    
    /// <summary>
    /// Container for region control parameters
    /// </summary>
    public class RegionParameters
    {
        public int laborAvailable = 100;
        public int infrastructureLevel = 5;
        public float maintenanceInvestment = 10;
        public float developmentInvestment = 20;
    }
    
    /// <summary>
    /// Container for all economic parameters
    /// </summary>
    public class EconomicParameters
    {
        public ProductionParameters production = new ProductionParameters();
        public InfrastructureParameters infrastructure = new InfrastructureParameters();
        public ConsumptionParameters consumption = new ConsumptionParameters();
        public PriceParameters price = new PriceParameters();
        public EconomicCycleParameters economicCycle = new EconomicCycleParameters();
        public RegionParameters region = new RegionParameters();
    }
}