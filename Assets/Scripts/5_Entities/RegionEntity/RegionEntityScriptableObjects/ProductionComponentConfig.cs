using UnityEngine;
using System.Collections.Generic;
using Entities.Components;

namespace Entities.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject that defines parameters for ProductionComponent
    /// </summary>
    [CreateAssetMenu(fileName = "ProductionConfig", menuName = "Influence/Region/Production Configuration")]
    public class ProductionComponentConfig : ScriptableObject
    {
        [System.Serializable]
        public class ModifierData
        {
            public string modifierName;
            public float initialValue = 1.0f;
        }
        
        [System.Serializable]
        public class SectorData
        {
            public string sectorName;
            public float allocation = 0.33f;
        }
        
        [Tooltip("Base production value")]
        public float baseProduction = 50f;
        
        [Tooltip("Initial production modifiers")]
        public List<ModifierData> initialModifiers = new List<ModifierData>();
        
        [Tooltip("Initial sector allocations")]
        public List<SectorData> initialSectors = new List<SectorData>();
        
        /// <summary>
        /// Creates and configures a ProductionComponent with the parameters from this configuration
        /// </summary>
        public ProductionComponent CreateComponent()
        {
            ProductionComponent component = new ProductionComponent(baseProduction);
            
            // Configure modifiers
            foreach (var modifier in initialModifiers)
            {
                component.SetProductionModifier(modifier.modifierName, modifier.initialValue);
            }
            
            // Configure sector allocations
            foreach (var sector in initialSectors)
            {
                component.SetSectorAllocation(sector.sectorName, sector.allocation);
            }
            
            return component;
        }
    }
}