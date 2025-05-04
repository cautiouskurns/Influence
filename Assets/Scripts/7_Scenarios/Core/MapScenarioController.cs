using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scenarios;
using UI;
using Managers;

namespace Scenarios
{
    /// <summary>
    /// Bridge between ScenarioManager and MapManager to create custom maps from scenario data
    /// </summary>
    public class MapScenarioController : MonoBehaviour
    {
        [SerializeField] private MapManager mapManager;
        [SerializeField] private ScenarioManager scenarioManager;
        
        private void Awake()
        {
            // Find references if not set in inspector
            if (mapManager == null)
                mapManager = GetComponent<MapManager>();
            if (mapManager == null)
                mapManager = FindFirstObjectByType<MapManager>();
                
            if (scenarioManager == null)
                scenarioManager = FindFirstObjectByType<ScenarioManager>();
        }
        
        private void Start()
        {
            // Listen for scenario start events
            EventBus.Subscribe("ScenarioStarted", OnScenarioStarted);
        }
        
        private void OnDestroy()
        {
            // Clean up event subscription
            EventBus.Unsubscribe("ScenarioStarted", OnScenarioStarted);
        }
        
        private void OnScenarioStarted(object data)
        {
            if (data is TestScenario scenario)
            {
                Debug.Log("MapScenarioController received ScenarioStarted event, creating custom map");
                CreateMapFromScenario(scenario);
            }
        }
        
        private void CreateMapFromScenario(TestScenario scenario)
        {
            if (mapManager != null && scenario != null)
            {
                Debug.Log($"Creating map with {scenario.regionStartConditions.Count} regions from scenario: {scenario.scenarioName}");
                
                // Cast the list to remove any ambiguity about the type
                List<RegionStartCondition> regionConditions = scenario.regionStartConditions;
                mapManager.CreateCustomMap(regionConditions);
            }
            else
            {
                Debug.LogError("Cannot create scenario map: MapManager or scenario is null");
            }
        }
    }
}