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
        if (autoGenerateUI && FindObjectOfType<ScenarioUI>() == null)
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
                
            ScenarioUI ui = FindObjectOfType<ScenarioUI>();
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
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create basic ScenarioUI
        GameObject scenarioUIObj = new GameObject("ScenarioUI");
        scenarioUIObj.transform.SetParent(canvas.transform, false);
        RectTransform scenarioUIRect = scenarioUIObj.AddComponent<RectTransform>();
        scenarioUIRect.anchorMin = Vector2.zero;
        scenarioUIRect.anchorMax = Vector2.one;
        scenarioUIRect.offsetMin = Vector2.zero;
        scenarioUIRect.offsetMax = Vector2.zero;
        
        ScenarioUI scenarioUI = scenarioUIObj.AddComponent<ScenarioUI>();
        
        // Create info panel
        GameObject infoPanel = new GameObject("ScenarioInfoPanel");
        infoPanel.transform.SetParent(scenarioUIObj.transform, false);
        RectTransform infoPanelRect = infoPanel.AddComponent<RectTransform>();
        infoPanelRect.anchorMin = new Vector2(0, 0.8f);
        infoPanelRect.anchorMax = new Vector2(1, 1);
        infoPanelRect.offsetMin = Vector2.zero;
        infoPanelRect.offsetMax = Vector2.zero;
        
        Image infoBg = infoPanel.AddComponent<Image>();
        infoBg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        // Create name text
        GameObject nameTextObj = new GameObject("ScenarioNameText");
        nameTextObj.transform.SetParent(infoPanel.transform, false);
        RectTransform nameTextRect = nameTextObj.AddComponent<RectTransform>();
        nameTextRect.anchorMin = new Vector2(0, 0.6f);
        nameTextRect.anchorMax = new Vector2(1, 1);
        nameTextRect.offsetMin = new Vector2(20, 0);
        nameTextRect.offsetMax = new Vector2(-20, 0);
        
        TMPro.TextMeshProUGUI nameText = nameTextObj.AddComponent<TMPro.TextMeshProUGUI>();
        nameText.text = "Scenario Name";
        nameText.fontSize = 24;
        nameText.alignment = TMPro.TextAlignmentOptions.Center;
        
        // Create description text
        GameObject descTextObj = new GameObject("DescriptionText");
        descTextObj.transform.SetParent(infoPanel.transform, false);
        RectTransform descTextRect = descTextObj.AddComponent<RectTransform>();
        descTextRect.anchorMin = new Vector2(0, 0);
        descTextRect.anchorMax = new Vector2(1, 0.6f);
        descTextRect.offsetMin = new Vector2(20, 10);
        descTextRect.offsetMax = new Vector2(-20, 0);
        
        TMPro.TextMeshProUGUI descText = descTextObj.AddComponent<TMPro.TextMeshProUGUI>();
        descText.text = "Scenario Description";
        descText.fontSize = 18;
        descText.alignment = TMPro.TextAlignmentOptions.Top;
        
        // Create objective text
        GameObject objTextObj = new GameObject("ObjectiveText");
        objTextObj.transform.SetParent(infoPanel.transform, false);
        RectTransform objTextRect = objTextObj.AddComponent<RectTransform>();
        objTextRect.anchorMin = new Vector2(0, 0);
        objTextRect.anchorMax = new Vector2(1, 0.3f);
        objTextRect.offsetMin = new Vector2(20, 10);
        objTextRect.offsetMax = new Vector2(-20, 0);
        
        TMPro.TextMeshProUGUI objText = objTextObj.AddComponent<TMPro.TextMeshProUGUI>();
        objText.text = "Objectives";
        objText.fontSize = 16;
        objText.color = new Color(1f, 0.8f, 0.2f);
        objText.alignment = TMPro.TextAlignmentOptions.Bottom;
        
        // Create turn counter
        GameObject turnTextObj = new GameObject("TurnCounterText");
        turnTextObj.transform.SetParent(infoPanel.transform, false);
        RectTransform turnTextRect = turnTextObj.AddComponent<RectTransform>();
        turnTextRect.anchorMin = new Vector2(0.8f, 0.8f);
        turnTextRect.anchorMax = new Vector2(1, 1);
        turnTextRect.offsetMin = Vector2.zero;
        turnTextRect.offsetMax = new Vector2(-20, -10);
        
        TMPro.TextMeshProUGUI turnText = turnTextObj.AddComponent<TMPro.TextMeshProUGUI>();
        turnText.text = "Turn: 0 / 10";
        turnText.fontSize = 18;
        turnText.alignment = TMPro.TextAlignmentOptions.Right;
        
        // Create basic victory/defeat panels
        CreateBasicPanel("VictoryPanel", scenarioUIObj.transform, new Color(0.1f, 0.4f, 0.1f, 0.9f), "Victory!");
        CreateBasicPanel("DefeatPanel", scenarioUIObj.transform, new Color(0.4f, 0.1f, 0.1f, 0.9f), "Defeat!");
        
        // Wire up the references
        scenarioUI.scenarioInfoPanel = infoPanel;
        scenarioUI.victoryPanel = scenarioUIObj.transform.Find("VictoryPanel").gameObject;
        scenarioUI.defeatPanel = scenarioUIObj.transform.Find("DefeatPanel").gameObject;
        scenarioUI.scenarioNameText = nameText;
        scenarioUI.descriptionText = descText;
        scenarioUI.objectiveText = objText;
        scenarioUI.turnCounterText = turnText;
        
        // Connect to ScenarioManager
        if (scenarioManager != null)
        {
            var so = new UnityEditor.SerializedObject(scenarioManager);
            so.FindProperty("scenarioUI").objectReferenceValue = scenarioUI;
            so.ApplyModifiedProperties();
        }
        
        // Hide result panels
        scenarioUI.victoryPanel.SetActive(false);
        scenarioUI.defeatPanel.SetActive(false);
        
        Debug.Log("Created basic ScenarioUI");
    }
    
    private void CreateBasicPanel(string name, Transform parent, Color bgColor, string text)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.3f, 0.3f);
        panelRect.anchorMax = new Vector2(0.7f, 0.7f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        Image bg = panel.AddComponent<Image>();
        bg.color = bgColor;
        
        GameObject textObj = new GameObject("ResultText");
        textObj.transform.SetParent(panel.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TMPro.TextMeshProUGUI tmpText = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.fontSize = 36;
        tmpText.alignment = TMPro.TextAlignmentOptions.Center;
        
        GameObject buttonObj = new GameObject("ContinueButton");
        buttonObj.transform.SetParent(panel.transform, false);
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.3f, 0.2f);
        buttonRect.anchorMax = new Vector2(0.7f, 0.3f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 1);
        
        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.5f, 0.5f, 0.5f);
        button.colors = colors;
        
        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;
        
        TMPro.TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TMPro.TextMeshProUGUI>();
        buttonText.text = "Continue";
        buttonText.fontSize = 18;
        buttonText.alignment = TMPro.TextAlignmentOptions.Center;
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
        T component = FindObjectOfType<T>();
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