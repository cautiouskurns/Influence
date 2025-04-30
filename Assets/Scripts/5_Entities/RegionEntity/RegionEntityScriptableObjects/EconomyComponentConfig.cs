using UnityEngine;
using System.Collections.Generic;
using Entities.Components;

namespace Entities.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject that defines parameters for RegionEconomyComponent
    /// </summary>
    [CreateAssetMenu(fileName = "EconomyConfig", menuName = "Influence/Region/Economy Configuration")]
    public class EconomyComponentConfig : ScriptableObject
    {
        [System.Serializable]
        public class EconomicFactorData
        {
            public string factorName;
            public float initialValue = 1.0f;
        }
        
        [Tooltip("Initial GDP value")]
        public float initialGDP = 100f;
        
        [Tooltip("Initial wealth value")]
        public int initialWealth = 100;
        
        [Tooltip("Default growth rate")]
        public float defaultGrowthRate = 0.02f;
        
        [Tooltip("Economic factors that influence growth")]
        public EconomicFactorData[] economicFactors;
        
        /// <summary>
        /// Creates and configures a RegionEconomyComponent with the parameters from this configuration
        /// </summary>
        public RegionEconomyComponent CreateComponent()
        {
            RegionEconomyComponent component = new RegionEconomyComponent(initialGDP, initialWealth);
            
            // Configure economic factors if they exist
            if (economicFactors != null)
            {
                foreach (var factor in economicFactors)
                {
                    component.SetEconomicFactor(factor.factorName, factor.initialValue);
                }
            }
            
            return component;
        }
    }
}