using UnityEngine;
using UI;
using Managers;

namespace Scenarios
{
    /// <summary>
    /// Helper component to automatically set up and connect scenario and map systems
    /// Add this to a GameObject in your scene to handle automatic setup of scenario systems
    /// </summary>
    public class ScenarioSetupHelper : MonoBehaviour
    {
        [SerializeField] private TestScenario defaultScenario;
        [SerializeField] private bool autoLoadScenario = true;
        [SerializeField] private bool attachMapScenarioController = true;
        
        [Header("References")]
        [SerializeField] private ScenarioManager scenarioManager;
        [SerializeField] private MapManager mapManager;
        
        private void Awake()
        {
            if (scenarioManager == null)
                scenarioManager = FindFirstObjectByType<ScenarioManager>();
                
            if (mapManager == null)
                mapManager = FindFirstObjectByType<MapManager>();
                
            // Set up the connections between components
            if (mapManager != null && attachMapScenarioController)
            {
                // Only add the controller if it doesn't already exist
                if (!mapManager.gameObject.TryGetComponent<MapScenarioController>(out _))
                {
                    var controller = mapManager.gameObject.AddComponent<MapScenarioController>();
                    
                    // Set references using reflection to bypass private fields
                    var controllerSO = new UnityEditor.SerializedObject(controller);
                    controllerSO.FindProperty("mapManager").objectReferenceValue = mapManager;
                    controllerSO.FindProperty("scenarioManager").objectReferenceValue = scenarioManager;
                    controllerSO.ApplyModifiedProperties();
                    
                }
                else
                {
                    Debug.Log("MapScenarioController already exists on MapManager");
                }
            }
        }
        
        private void Start()
        {
            // Auto-load scenario after all components are initialized
            if (autoLoadScenario && scenarioManager != null && defaultScenario != null)
            {
                // Small delay to ensure all systems are ready
                Invoke(nameof(LoadDefaultScenario), 0.2f);
            }
        }
        
        private void LoadDefaultScenario()
        {
            scenarioManager.StartScenario(defaultScenario);
            Debug.Log($"Auto-loaded scenario: {defaultScenario.scenarioName}");
        }
    }
}