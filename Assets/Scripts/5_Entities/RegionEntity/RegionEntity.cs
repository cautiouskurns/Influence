using UnityEngine;
using System.Collections.Generic;
using Managers;
using Entities.Components;
using Entities.ScriptableObjects;

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
        
        // Basic economic properties - Redirected to components
        public int Wealth { 
            get => Economy.Wealth; 
            set => Debug.LogWarning("Direct setting of Wealth is deprecated. Use Economy component instead."); 
        }
        
        // Production - Now redirected to ProductionComponent
        public int Production { 
            get => ProductionComp.Production; 
            set => Debug.LogWarning("Direct setting of Production is deprecated. Use ProductionComp component instead.");
        }
        
        // Core resources - Now redirecting to components
        public float LaborAvailable { 
            get => PopulationComp.LaborAvailable; 
            set => Debug.LogWarning("Direct setting of LaborAvailable is deprecated. Use PopulationComp component instead.");
        }
        
        // Infrastructure - Now redirected to InfrastructureComponent
        public float InfrastructureLevel { 
            get => Infrastructure.Level; 
            set => Debug.LogWarning("Direct setting of InfrastructureLevel is deprecated. Use Infrastructure component instead.");
        }
        public float InfrastructureQuality { 
            get => Infrastructure.Quality; 
            set => Debug.LogWarning("Direct setting of InfrastructureQuality is deprecated. Use Infrastructure component instead.");
        }        
        
        // Population - Now redirected to PopulationComponent
        public int Population { 
            get => PopulationComp.Population; 
            set => Debug.LogWarning("Direct setting of Population is deprecated. Use PopulationComp component instead.");
        }
        
        // Component references
        public ResourceComponent Resources { get; private set; }
        public ProductionComponent ProductionComp { get; private set; }
        public RegionEconomyComponent Economy { get; private set; }
        public PopulationComponent PopulationComp { get; private set; }
        public InfrastructureComponent Infrastructure { get; private set; }
        
        // Default constructor - use minimal initialization and rely on ConfigureFromAssets
        public RegionEntity(string id, string name)
        {
            Id = id;
            Name = name;
            
            // Initialize with empty components that will be replaced by configuration
            Resources = new ResourceComponent();
            ProductionComp = new ProductionComponent(0);
            Economy = new RegionEconomyComponent(0, 0);
            PopulationComp = new PopulationComponent(0, 0);
            Infrastructure = new InfrastructureComponent(0, 0);
        }

        // Backwards compatibility constructor
        public RegionEntity(string id, string name, int initialWealth, int initialProduction) 
            : this(id, name)
        {
            // If called directly, set up some basic values
            // This would typically be overridden by ConfigureFromAssets
            Economy = new RegionEconomyComponent(initialWealth + initialProduction * 2, initialWealth);
            ProductionComp = new ProductionComponent(initialProduction);
            PopulationComp = new PopulationComponent(1000, 100);
            Infrastructure = new InfrastructureComponent(5.0f, 0.5f);
        }
        
        // Additional constructor with default values
        public RegionEntity(string name) : this(name, name)
        {
            // This calls the main constructor - config will need to be applied separately
        }

        /// <summary>
        /// Configure this region entity with components from configuration assets
        /// </summary>
        public void ConfigureFromAssets(RegionConfig config)
        {
            if (config == null)
            {
                Debug.LogWarning($"No RegionConfig provided for {Name}. Using default values.");
                return;
            }
                
            // Replace components with configured versions
            if (config.resourceConfig != null)
                Resources = config.resourceConfig.CreateComponent();
            else
                Debug.LogWarning($"No ResourceConfig provided for {Name}. Using default values.");
                
            if (config.productionConfig != null)
                ProductionComp = config.productionConfig.CreateComponent();
            else
                Debug.LogWarning($"No ProductionConfig provided for {Name}. Using default values.");
                
            if (config.economyConfig != null) 
                Economy = config.economyConfig.CreateComponent();
            else
                Debug.LogWarning($"No EconomyConfig provided for {Name}. Using default values.");
                
            if (config.populationConfig != null)
                PopulationComp = config.populationConfig.CreateComponent();
            else
                Debug.LogWarning($"No PopulationConfig provided for {Name}. Using default values.");
                
            if (config.infrastructureConfig != null)
                Infrastructure = config.infrastructureConfig.CreateComponent();
            else
                Debug.LogWarning($"No InfrastructureConfig provided for {Name}. Using default values.");
        }
        
        /// <summary>
        /// Creates a new RegionEntity from a configuration asset
        /// </summary>
        public static RegionEntity CreateFromConfig(string id, string name, RegionConfig config)
        {
            if (config == null)
            {
                Debug.LogError($"Attempted to create RegionEntity {name} without a configuration. Using default values.");
                return new RegionEntity(id, name, 100, 50); // Fallback to defaults
            }
            
            // Create a minimal region
            RegionEntity region = new RegionEntity(id, name);
            
            // Configure it from the assets
            region.ConfigureFromAssets(config);
            
            return region;
        }

        // Basic summary of the region
        public string GetSummary()
        {
            string summary = $"Region: {Name}\n" +
                   $"Nation ID: {NationId ?? "None"}\n";
                   
            // Add component summaries
            summary += Resources.GetSummary();
            summary += ProductionComp.GetSummary();
            summary += Economy.GetSummary();
            summary += PopulationComp.GetSummary();
            summary += Infrastructure.GetSummary();
            
            return summary;
        }
        
        // Process all production for one turn
        public void ProcessTurn()
        {
            // Process resource production
            Resources.ProcessProduction();
            
            // Calculate maintenance funding
            float maintenanceFunding = Economy.Wealth * 0.05f; // 5% of wealth for infrastructure
            
            // Process infrastructure changes
            Infrastructure.ProcessTurn(maintenanceFunding);
            
            // Update production modifiers based on current state
            ProductionComp.SetProductionModifier("Infrastructure", Infrastructure.GetProductionModifier());
            ProductionComp.SetProductionModifier("Workforce", PopulationComp.LaborAvailable / 100f);
            ProductionComp.SetProductionModifier("Efficiency", PopulationComp.LaborEfficiency);
            
            // Update production (now handled by the component itself)
            ProductionComp.ProcessTurn();
            
            // Get all resource amounts for population needs calculation
            Dictionary<string, float> availableResources = new Dictionary<string, float>
            {
                { "Food", Resources.GetResourceAmount("Food") },
                { "Materials", Resources.GetResourceAmount("Materials") },
                { "Fuel", Resources.GetResourceAmount("Fuel") }
            };
            
            // Update population based on available resources and infrastructure
            PopulationComp.ProcessTurn(availableResources, Infrastructure.Level);
            
            // Calculate total resource production for economy calculations
            float totalResourceProduction = 0;
            totalResourceProduction += Resources.GetProductionRate("Food");
            totalResourceProduction += Resources.GetProductionRate("Materials");
            totalResourceProduction += Resources.GetProductionRate("Fuel");
            
            // Update economy with current production and infrastructure
            float taxRate = GetNation()?.Economy?.TaxRate ?? 0.1f;
            Economy.ProcessTurn(totalResourceProduction, Infrastructure.Level, taxRate);
            
            // Consume resources based on population needs
            foreach (var resource in availableResources.Keys)
            {
                float needed = PopulationComp.GetTotalResourceNeed(resource);
                float available = Resources.GetResourceAmount(resource);
                float consumed = Mathf.Min(needed, available);
                
                // Update resource amount by removing consumed amount
                float newAmount = available - consumed;
                Resources.SetResourceAmount(resource, newAmount);
            }
        }
        
        // Invest in infrastructure
        public float InvestInInfrastructure(float amount)
        {
            if (amount <= 0 || amount > Economy.Wealth)
                return 0;
                
            // Calculate actual investment amount
            float actualAmount = Mathf.Min(amount, Economy.Wealth);
            
            // Apply the investment to infrastructure
            float infrastructureImprovement = Infrastructure.Invest(actualAmount);
            
            // Deduct the investment from the region's wealth
            Economy.UpdateWealth(-Mathf.RoundToInt(actualAmount));
            
            return infrastructureImprovement;
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
