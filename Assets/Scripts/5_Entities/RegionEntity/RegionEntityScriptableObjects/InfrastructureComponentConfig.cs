using UnityEngine;
using System.Collections.Generic;
using Entities.Components;

namespace Entities.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject that defines parameters for InfrastructureComponent
    /// </summary>
    [CreateAssetMenu(fileName = "InfrastructureConfig", menuName = "Influence/Region/Infrastructure Configuration")]
    public class InfrastructureComponentConfig : ScriptableObject
    {
        [System.Serializable]
        public class InfrastructureAspectData
        {
            public string aspectName;
            public float initialValue = 0.5f;
        }
        
        [Tooltip("Initial infrastructure level")]
        public float initialLevel = 5.0f;
        
        [Tooltip("Initial infrastructure quality (0-1)")]
        [Range(0, 1)]
        public float initialQuality = 0.5f;
        
        [Tooltip("Initial infrastructure aspects")]
        public List<InfrastructureAspectData> aspects = new List<InfrastructureAspectData>();
        
        /// <summary>
        /// Creates and configures an InfrastructureComponent with the parameters from this configuration
        /// </summary>
        public InfrastructureComponent CreateComponent()
        {
            InfrastructureComponent component = new InfrastructureComponent(initialLevel, initialQuality);
            
            // Additional configuration can be added here as InfrastructureComponent grows
            
            return component;
        }
    }
}