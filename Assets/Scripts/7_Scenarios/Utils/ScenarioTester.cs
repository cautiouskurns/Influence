using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Scenarios;
using Managers;
using Systems;
using Core;

/// <summary>
/// Helper script to quickly set up and test the scenario system.
/// Add this to any GameObject in your scene.
/// </summary>
public class ScenarioTester : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool autoGenerateUI = true;
    [SerializeField] private bool autoCreateTestScenario = true;
    
    [Header("References")]
    [SerializeField] private TestScenario testScenario;
    [SerializeField] private ScenarioManager scenarioManager;
    [SerializeField] private EconomicSystem economicSystem;
    [SerializeField] private NationManager nationManager;
    [SerializeField] private TurnManager turnManager;
    
    private void Start()
    {
        // Find or create the systems we need
        if (scenarioManager == null)
            scenarioManager = FindOrCreateComponent<ScenarioManager>("ScenarioManager");
            
        if (economicSystem == null)
            economicSystem = FindOrCreateComponent<EconomicSystem>("EconomicSystem");
            
        if (nationManager == null)
            nationManager = FindOrCreateComponent<NationManager>("NationManager");
            
        if (turnManager == null)
            turnManager = FindOrCreateComponent<TurnManager>("TurnManager");
        
        // Create a test scenario if needed
        if (autoCreateTestScenario && testScenario == null)
        {
            testScenario = CreateTestScenario();
        }
        
        // Generate UI if needed
        if (autoGenerateUI && FindFirstObjectByType<ScenarioUI>() == null)
        {
            StartCoroutine(GenerateUIAfterDelay());
        }
        
        // Connect systems
        ConnectSystems();
        
        // Start the test scenario
        if (testScenario != null)
        {
            StartCoroutine(StartScenarioAfterDelay());
        }
    }
    
    private void ConnectSystems()
    {
        if (scenarioManager != null)
        {
            // Assign references in the ScenarioManager
            var so = new UnityEditor.SerializedObject(scenarioManager);
            
            if (economicSystem != null)
                so.FindProperty("economicSystem").objectReferenceValue = economicSystem;
                
            if (nationManager != null)
                so.FindProperty("nationManager").objectReferenceValue = nationManager;
                
            if (turnManager != null)
                so.FindProperty("turnManager").objectReferenceValue = turnManager;
                
            ScenarioUI ui = FindFirstObjectByType<ScenarioUI>();
            if (ui != null)
                so.FindProperty("scenarioUI").objectReferenceValue = ui;
                
            so.ApplyModifiedProperties();
            
            Debug.Log("Connected systems to ScenarioManager");
        }
    }
    
    private IEnumerator GenerateUIAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        
        // Find Canvas or create one
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create simple ScenarioUI with just text
        GameObject scenarioUIObj = new GameObject("ScenarioUI");
        scenarioUIObj.transform.SetParent(canvas.transform, false);
        ScenarioUI scenarioUI = scenarioUIObj.AddComponent<ScenarioUI>();
        
        // The ScenarioUI will create its own text in Awake if not assigned
        
        Debug.Log("Created simplified ScenarioUI");
    }
    
    private TestScenario CreateTestScenario()
    {
        // Create a simple test scenario asset
        TestScenario scenario = ScriptableObject.CreateInstance<TestScenario>();
        scenario.scenarioName = "Test Scenario";
        scenario.description = "This is a test scenario to verify that the system is working correctly.";
        scenario.turnLimit = 10;
        
        // Create a region
        RegionStartCondition region = new RegionStartCondition
        {
            regionId = "region1",
            regionName = "Test Region 1",
            initialWealth = 200,
            initialProduction = 50,
            initialInfrastructureLevel = 10,
            initialPopulation = 1000,
            initialSatisfaction = 0.7f
        };
        
        // Create another region
        RegionStartCondition region2 = new RegionStartCondition
        {
            regionId = "region2",
            regionName = "Test Region 2",
            initialWealth = 150,
            initialProduction = 40,
            initialInfrastructureLevel = 8,
            initialPopulation = 800,
            initialSatisfaction = 0.6f
        };
        
        // Create a nation
        NationStartCondition nation = new NationStartCondition
        {
            nationId = "nation1",
            nationName = "Test Nation 1",
            controlledRegionIds = new System.Collections.Generic.List<string> { "region1" }
        };
        
        // Create another nation
        NationStartCondition nation2 = new NationStartCondition
        {
            nationId = "nation2",
            nationName = "Test Nation 2",
            controlledRegionIds = new System.Collections.Generic.List<string> { "region2" }
        };
        
        // Add to scenario
        scenario.regionStartConditions.Add(region);
        scenario.regionStartConditions.Add(region2);
        scenario.nationStartConditions.Add(nation);
        scenario.nationStartConditions.Add(nation2);
        
        // Create victory condition
        scenario.victoryCondition = new VictoryCondition
        {
            type = VictoryCondition.VictoryType.Economic,
            requiredWealth = 300,
            requiredConsecutiveTurns = 1
        };
        
        // Save the asset
        string path = "Assets/Resources/Scenarios/TestScenario.asset";
        UnityEditor.AssetDatabase.CreateAsset(scenario, path);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        
        Debug.Log("Created test scenario at " + path);
        
        return scenario;
    }
    
    private T FindOrCreateComponent<T>(string name) where T : Component
    {
        // Try to find an existing component
        T component = FindFirstObjectByType<T>();
        if (component != null)
            return component;
            
        // Create a new GameObject with the component
        GameObject obj = new GameObject(name);
        component = obj.AddComponent<T>();
        Debug.Log($"Created {typeof(T).Name} on GameObject '{name}'");
        
        return component;
    }
    
    private IEnumerator StartScenarioAfterDelay()
    {
        // Give a small delay to allow other systems to initialize
        yield return new WaitForSeconds(0.5f);
        
        if (scenarioManager != null && testScenario != null)
        {
            Debug.Log("Starting test scenario: " + testScenario.scenarioName);
            scenarioManager.StartScenario(testScenario);
        }
    }
    
    public void AdvanceTurn()
    {
        if (turnManager != null)
        {
            // Use reflection to call the EndTurn method since it might be private
            System.Reflection.MethodInfo method = typeof(TurnManager).GetMethod("EndTurn", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic);
                
            if (method != null)
            {
                method.Invoke(turnManager, null);
                Debug.Log("Advanced to next turn");
            }
            else
            {
                Debug.LogWarning("Could not find EndTurn method on TurnManager");
            }
        }
    }
}