using UnityEngine;
using UnityEditor;
using Scenarios;
using System.IO;

namespace Scenarios.Editor
{
    public class ProceduralScenarioWindow : EditorWindow
    {
        // Reference to the procedural generator
        private ProceduralScenarioGenerator generator;
        private TestScenario generatedScenario;
        
        private Vector2 scrollPosition;
        private bool showGeneralSettings = true;
        private bool showRegionSettings = true;
        private bool showNationSettings = true;
        
        [MenuItem("Influence/Procedural Scenario Generator")]
        public static void ShowWindow()
        {
            GetWindow<ProceduralScenarioWindow>("Procedural Scenarios");
        }
        
        private void OnEnable()
        {
            // Find or create a procedural generator
            string[] guids = AssetDatabase.FindAssets("t:ProceduralScenarioGenerator");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                generator = AssetDatabase.LoadAssetAtPath<ProceduralScenarioGenerator>(path);
            }
            
            if (generator == null)
            {
                // Create a default generator if none exists
                generator = CreateInstance<ProceduralScenarioGenerator>();
            }
        }
        
        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            EditorGUILayout.LabelField("Procedural Scenario Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            
            // Generator asset reference
            EditorGUI.BeginChangeCheck();
            generator = (ProceduralScenarioGenerator)EditorGUILayout.ObjectField(
                "Generator Asset", generator, typeof(ProceduralScenarioGenerator), false);
            if (EditorGUI.EndChangeCheck() && generator == null)
            {
                // If user removed the reference, create a default one again
                generator = CreateInstance<ProceduralScenarioGenerator>();
            }
            
            // Create asset button (if it's not already an asset)
            string assetPath = AssetDatabase.GetAssetPath(generator);
            if (string.IsNullOrEmpty(assetPath))
            {
                EditorGUILayout.Space(5);
                if (GUILayout.Button("Save Generator as Asset"))
                {
                    string path = EditorUtility.SaveFilePanelInProject(
                        "Save Generator Asset",
                        "ProceduralScenarioGenerator",
                        "asset",
                        "Save the procedural generator as an asset"
                    );
                    
                    if (!string.IsNullOrEmpty(path))
                    {
                        AssetDatabase.CreateAsset(generator, path);
                        AssetDatabase.SaveAssets();
                    }
                }
            }
            
            EditorGUILayout.Space(10);
            
            // General settings
            showGeneralSettings = EditorGUILayout.Foldout(showGeneralSettings, "General Settings", true);
            if (showGeneralSettings)
            {
                EditorGUI.indentLevel++;
                generator.regionCount = EditorGUILayout.IntSlider("Region Count", generator.regionCount, 3, 50);
                generator.nationCount = EditorGUILayout.IntSlider("Nation Count", generator.nationCount, 1, 10);
                
                // Make sure we don't have more nations than regions
                if (generator.nationCount > generator.regionCount)
                {
                    generator.nationCount = generator.regionCount;
                }
                
                generator.seed = EditorGUILayout.IntField("Random Seed (0 for random)", generator.seed);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
            
            // Region settings
            showRegionSettings = EditorGUILayout.Foldout(showRegionSettings, "Region Settings", true);
            if (showRegionSettings)
            {
                EditorGUI.indentLevel++;
                generator.minRegionWealth = EditorGUILayout.IntField("Min Wealth", generator.minRegionWealth);
                generator.maxRegionWealth = EditorGUILayout.IntField("Max Wealth", generator.maxRegionWealth);
                generator.minRegionProduction = EditorGUILayout.IntField("Min Production", generator.minRegionProduction);
                generator.maxRegionProduction = EditorGUILayout.IntField("Max Production", generator.maxRegionProduction);
                generator.minInfrastructureLevel = EditorGUILayout.FloatField("Min Infrastructure", generator.minInfrastructureLevel);
                generator.maxInfrastructureLevel = EditorGUILayout.FloatField("Max Infrastructure", generator.maxInfrastructureLevel);
                generator.minPopulation = EditorGUILayout.IntField("Min Population", generator.minPopulation);
                generator.maxPopulation = EditorGUILayout.IntField("Max Population", generator.maxPopulation);
                generator.minSatisfaction = EditorGUILayout.Slider("Min Satisfaction", generator.minSatisfaction, 0f, 1f);
                generator.maxSatisfaction = EditorGUILayout.Slider("Max Satisfaction", generator.maxSatisfaction, 0f, 1f);
                generator.clusterWealthyRegions = EditorGUILayout.Toggle("Cluster Wealthy Regions", generator.clusterWealthyRegions);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
            
            // Nation settings
            showNationSettings = EditorGUILayout.Foldout(showNationSettings, "Nation Settings", true);
            if (showNationSettings)
            {
                EditorGUI.indentLevel++;
                generator.nationNameStyle = (ProceduralScenarioGenerator.NationNameStyle)EditorGUILayout.EnumPopup(
                    "Nation Name Style", generator.nationNameStyle);
                generator.balanceNations = EditorGUILayout.Toggle("Balance Nation Strength", generator.balanceNations);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            // Generate buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Preview", GUILayout.Height(30)))
            {
                generatedScenario = generator.GenerateScenario();
            }
            
            if (GUILayout.Button("Generate & Save", GUILayout.Height(30)))
            {
                generatedScenario = generator.GenerateScenario();
                
                string path = EditorUtility.SaveFilePanelInProject(
                    "Save Generated Scenario",
                    "GeneratedScenario",
                    "asset",
                    "Save the procedurally generated scenario as an asset"
                );
                
                if (!string.IsNullOrEmpty(path))
                {
                    AssetDatabase.CreateAsset(generatedScenario, path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Selection.activeObject = generatedScenario;
                    EditorGUIUtility.PingObject(generatedScenario);
                    Debug.Log($"Saved procedurally generated scenario to {path}");
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            // Preview generated scenario
            if (generatedScenario != null)
            {
                EditorGUILayout.LabelField("Generated Scenario Preview", EditorStyles.boldLabel);
                
                EditorGUI.indentLevel++;
                
                EditorGUILayout.LabelField("Name", generatedScenario.scenarioName);
                EditorGUILayout.LabelField("Description", generatedScenario.description);
                EditorGUILayout.LabelField("Turn Limit", generatedScenario.turnLimit.ToString());
                
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"Regions: {generatedScenario.regionStartConditions.Count}", EditorStyles.boldLabel);
                
                foreach (var region in generatedScenario.regionStartConditions)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField($"{region.regionName} (ID: {region.regionId})");
                    EditorGUILayout.LabelField($"Wealth: {region.initialWealth}, Production: {region.initialProduction}");
                    EditorGUILayout.LabelField($"Population: {region.initialPopulation}, Satisfaction: {region.initialSatisfaction:F2}");
                    EditorGUILayout.EndVertical();
                }
                
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"Nations: {generatedScenario.nationStartConditions.Count}", EditorStyles.boldLabel);
                
                foreach (var nation in generatedScenario.nationStartConditions)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField($"{nation.nationName} (ID: {nation.nationId})");
                    EditorGUILayout.LabelField($"Controls {nation.controlledRegionIds.Count} regions");
                    EditorGUILayout.EndVertical();
                }
                
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Victory Condition", EditorStyles.boldLabel);
                
                string victoryTypeString = "Unknown";
                string victoryValueString = "";
                
                switch (generatedScenario.victoryCondition.type)
                {
                    case VictoryCondition.VictoryType.Economic:
                        victoryTypeString = "Economic";
                        victoryValueString = $"Required Wealth: {generatedScenario.victoryCondition.requiredWealth}";
                        break;
                        
                    case VictoryCondition.VictoryType.Development:
                        victoryTypeString = "Development";
                        victoryValueString = $"Required Infrastructure: {generatedScenario.victoryCondition.requiredInfrastructure}";
                        break;
                        
                    case VictoryCondition.VictoryType.Stability:
                        victoryTypeString = "Stability";
                        victoryValueString = $"Required Satisfaction: {generatedScenario.victoryCondition.requiredSatisfaction:F2}";
                        break;
                }
                
                EditorGUILayout.LabelField($"Type: {victoryTypeString}");
                EditorGUILayout.LabelField(victoryValueString);
                EditorGUILayout.LabelField($"Required Consecutive Turns: {generatedScenario.victoryCondition.requiredConsecutiveTurns}");
                
                if (!string.IsNullOrEmpty(generatedScenario.victoryCondition.targetRegionId))
                {
                    EditorGUILayout.LabelField($"Target Region: {generatedScenario.victoryCondition.targetRegionId}");
                }
                else
                {
                    EditorGUILayout.LabelField("Target: All Regions");
                }
                
                EditorGUI.indentLevel--;
                
                EditorGUILayout.Space(10);
                if (GUILayout.Button("Open in Scenario Editor"))
                {
                    // Open the generated scenario in the regular scenario editor
                    ScenarioEditorWindow window = GetWindow<ScenarioEditorWindow>("Test Scenario Editor");
                    var field = typeof(ScenarioEditorWindow).GetField("currentScenario", 
                        System.Reflection.BindingFlags.Instance | 
                        System.Reflection.BindingFlags.NonPublic);
                    
                    if (field != null)
                    {
                        field.SetValue(window, generatedScenario);
                    }
                }
            }
            
            EditorGUILayout.EndScrollView();
        }
    }
}