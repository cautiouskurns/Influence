using UnityEngine;
using System.Collections.Generic;
using Entities;
using Entities.Components;

namespace Entities.ScriptableObjects
{
    /// <summary>
    /// Master ScriptableObject that defines all parameters for a RegionEntity
    /// </summary>
    [CreateAssetMenu(fileName = "RegionConfig", menuName = "Influence/Region/Region Configuration")]
    public class RegionConfig : ScriptableObject
    {
        [Header("Region Identity")]
        [Tooltip("Region type name (Desert, Forest, Plains, etc)")]
        public string regionTypeName;
        
        [Tooltip("Description of this region type")]
        [TextArea(3, 5)]
        public string description;
        
        [Header("Component Configurations")]
        [Tooltip("Resource configuration")]
        public ResourceComponentConfig resourceConfig;
        
        [Tooltip("Production configuration")]
        public ProductionComponentConfig productionConfig;
        
        [Tooltip("Economy configuration")]
        public EconomyComponentConfig economyConfig;
        
        [Tooltip("Population configuration")]
        public PopulationComponentConfig populationConfig;
        
        [Tooltip("Infrastructure configuration")]
        public InfrastructureComponentConfig infrastructureConfig;
        
        /// <summary>
        /// Creates and configures a complete RegionEntity using this configuration
        /// </summary>
        public RegionEntity CreateRegionEntity(string id, string name)
        {
            // Use the static factory method on RegionEntity
            return RegionEntity.CreateFromConfig(id, name, this);
        }
    }
}