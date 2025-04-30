using UnityEngine;
using System.Collections.Generic;
using Entities.Components;

namespace Entities.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject that defines parameters for PopulationComponent
    /// </summary>
    [CreateAssetMenu(fileName = "PopulationConfig", menuName = "Influence/Region/Population Configuration")]
    public class PopulationComponentConfig : ScriptableObject
    {
        [System.Serializable]
        public class ResourceNeedData
        {
            public string resourceType;
            public float amountPerPopulationUnit = 0.1f;
        }
        
        [System.Serializable]
        public class SatisfactionData
        {
            public string needType;
            public float initialSatisfaction = 0.7f;
        }
        
        [Tooltip("Initial population size")]
        public int initialPopulation = 1000;
        
        [Tooltip("Initial labor available")]
        public float initialLabor = 100f;
        
        [Tooltip("Default growth rate per turn")]
        public float defaultGrowthRate = 0.01f;
        
        [Tooltip("Initial satisfaction level")]
        public float initialSatisfaction = 0.7f;
        
        [Tooltip("Resource needs per population unit")]
        public List<ResourceNeedData> resourceNeeds = new List<ResourceNeedData>();
        
        [Tooltip("Initial satisfaction levels for different needs")]
        public List<SatisfactionData> needsSatisfaction = new List<SatisfactionData>();
        
        /// <summary>
        /// Creates and configures a PopulationComponent with the parameters from this configuration
        /// </summary>
        public PopulationComponent CreateComponent()
        {
            PopulationComponent component = new PopulationComponent(initialPopulation, initialLabor);
            
            // Configure resource needs
            foreach (var need in resourceNeeds)
            {
                component.SetResourceNeed(need.resourceType, need.amountPerPopulationUnit);
            }
            
            // Configure satisfaction levels
            foreach (var satisfaction in needsSatisfaction)
            {
                component.UpdateNeedSatisfaction(satisfaction.needType, satisfaction.initialSatisfaction);
            }
            
            return component;
        }
    }
}