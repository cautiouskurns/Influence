using UnityEngine;
using System.Collections.Generic;
using Entities.Components;

namespace Entities.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject that defines parameters for ResourceComponent
    /// </summary>
    [CreateAssetMenu(fileName = "ResourceConfig", menuName = "Influence/Region/Resource Configuration")]
    public class ResourceComponentConfig : ScriptableObject
    {
        [System.Serializable]
        public class ResourceData
        {
            public string resourceType;
            public float initialAmount;
            public float productionRate;
        }
        
        [Tooltip("Initial resources and their production rates")]
        public List<ResourceData> initialResources = new List<ResourceData>();
        
        /// <summary>
        /// Creates and configures a ResourceComponent with the parameters from this configuration
        /// </summary>
        public ResourceComponent CreateComponent()
        {
            ResourceComponent component = new ResourceComponent();
            
            // Configure resources based on this configuration
            foreach (var resource in initialResources)
            {
                component.SetResourceAmount(resource.resourceType, resource.initialAmount);
                component.SetProductionRate(resource.resourceType, resource.productionRate);
            }
            
            return component;
        }
    }
}