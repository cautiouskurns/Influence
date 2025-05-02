using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Scenarios.Editor
{
    public class ScenarioEditorWindow : EditorWindow
    {
        private TestScenario currentScenario;
        private Vector2 scrollPosition;
        private bool showRegions = true;
        private bool showNations = true;
        private bool showVictory = true;
        
        // Templates for creating new elements
        private RegionStartCondition newRegion = new RegionStartCondition();
        private NationStartCondition newNation = new NationStartCondition();
        
        [MenuItem("Influence/Test Scenario Editor")]
        public static void ShowWindow()
        {
            GetWindow<ScenarioEditorWindow>("Test Scenario Editor");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Test Scenario Editor", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // Scenario selection/creation
            currentScenario = (TestScenario)EditorGUILayout.ObjectField(
                "Current Scenario", currentScenario, typeof(TestScenario), false);
            
            if (currentScenario == null)
            {
                EditorGUILayout.HelpBox("Select an existing scenario or create a new one.", MessageType.Info);
                
                if (GUILayout.Button("Create New Scenario"))
                {
                    CreateNewScenario();
                }
                
                EditorGUILayout.EndScrollView();
                return;
            }
            
            // Basic settings
            EditorGUI.BeginChangeCheck();
            currentScenario.scenarioName = EditorGUILayout.TextField("Name", currentScenario.scenarioName);
            currentScenario.description = EditorGUILayout.TextArea(
                currentScenario.description, GUILayout.Height(60));
            currentScenario.turnLimit = EditorGUILayout.IntField("Turn Limit", currentScenario.turnLimit);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(currentScenario);
            }
            
            // Regions section
            EditorGUILayout.Space(10);
            showRegions = EditorGUILayout.Foldout(showRegions, "Regions", true);
            if (showRegions)
            {
                EditorGUI.indentLevel++;
                DrawRegionsSection();
                EditorGUI.indentLevel--;
            }
            
            // Nations section
            EditorGUILayout.Space(10);
            showNations = EditorGUILayout.Foldout(showNations, "Nations", true);
            if (showNations)
            {
                EditorGUI.indentLevel++;
                DrawNationsSection();
                EditorGUI.indentLevel--;
            }
            
            // Victory conditions section
            EditorGUILayout.Space(10);
            showVictory = EditorGUILayout.Foldout(showVictory, "Victory Conditions", true);
            if (showVictory)
            {
                EditorGUI.indentLevel++;
                DrawVictorySection();
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            // Test scenario button
            if (GUILayout.Button("Run Scenario", GUILayout.Height(30)))
            {
                RunScenario();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void CreateNewScenario()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create New Test Scenario",
                "NewScenario",
                "asset",
                "Create a new test scenario asset"
            );
            
            if (string.IsNullOrEmpty(path))
                return;
                
            TestScenario newScenario = CreateInstance<TestScenario>();
            newScenario.scenarioName = "New Test Scenario";
            newScenario.description = "Description of the test scenario";
            newScenario.turnLimit = 10;
            newScenario.victoryCondition = new VictoryCondition();
            newScenario.regionStartConditions = new List<RegionStartCondition>();
            newScenario.nationStartConditions = new List<NationStartCondition>();
            
            AssetDatabase.CreateAsset(newScenario, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            currentScenario = newScenario;
            Debug.Log($"Created new scenario at {path}");
        }
        
        private void DrawRegionsSection()
        {
            // Draw existing regions
            if (currentScenario.regionStartConditions != null && currentScenario.regionStartConditions.Count > 0)
            {
                for (int i = 0; i < currentScenario.regionStartConditions.Count; i++)
                {
                    RegionStartCondition region = currentScenario.regionStartConditions[i];
                    
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    EditorGUI.BeginChangeCheck();
                    
                    region.regionId = EditorGUILayout.TextField("Region ID", region.regionId);
                    region.regionName = EditorGUILayout.TextField("Region Name", region.regionName);
                    region.initialWealth = EditorGUILayout.IntField("Wealth", region.initialWealth);
                    region.initialProduction = EditorGUILayout.IntField("Production", region.initialProduction);
                    region.initialInfrastructureLevel = EditorGUILayout.FloatField("Infrastructure", region.initialInfrastructureLevel);
                    region.initialPopulation = EditorGUILayout.IntField("Population", region.initialPopulation);
                    region.initialSatisfaction = EditorGUILayout.Slider("Satisfaction", region.initialSatisfaction, 0f, 1f);
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(currentScenario);
                    }
                    
                    // Delete region button
                    if (GUILayout.Button("Remove Region", GUILayout.Width(150)))
                    {
                        currentScenario.regionStartConditions.RemoveAt(i);
                        EditorUtility.SetDirty(currentScenario);
                        i--;
                        EditorGUILayout.EndVertical();
                        continue;
                    }
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(5);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No regions defined yet.", MessageType.Info);
            }
            
            // Add new region section
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Add New Region", EditorStyles.boldLabel);
            
            newRegion.regionId = EditorGUILayout.TextField("Region ID", newRegion.regionId);
            newRegion.regionName = EditorGUILayout.TextField("Region Name", newRegion.regionName);
            
            if (GUILayout.Button("Add Region", GUILayout.Width(150)))
            {
                if (string.IsNullOrEmpty(newRegion.regionId) || string.IsNullOrEmpty(newRegion.regionName))
                {
                    EditorUtility.DisplayDialog("Missing Information", 
                        "Region ID and Name are required.", "OK");
                    return;
                }
                
                // Add the new region to the scenario
                RegionStartCondition newRegionCopy = new RegionStartCondition
                {
                    regionId = newRegion.regionId,
                    regionName = newRegion.regionName,
                    initialWealth = newRegion.initialWealth,
                    initialProduction = newRegion.initialProduction,
                    initialInfrastructureLevel = newRegion.initialInfrastructureLevel,
                    initialPopulation = newRegion.initialPopulation,
                    initialSatisfaction = newRegion.initialSatisfaction
                };
                
                currentScenario.regionStartConditions.Add(newRegionCopy);
                EditorUtility.SetDirty(currentScenario);
                
                // Reset the new region template
                newRegion = new RegionStartCondition();
            }
        }
        
        private void DrawNationsSection()
        {
            // Draw existing nations
            if (currentScenario.nationStartConditions != null && currentScenario.nationStartConditions.Count > 0)
            {
                for (int i = 0; i < currentScenario.nationStartConditions.Count; i++)
                {
                    NationStartCondition nation = currentScenario.nationStartConditions[i];
                    
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    EditorGUI.BeginChangeCheck();
                    
                    nation.nationId = EditorGUILayout.TextField("Nation ID", nation.nationId);
                    nation.nationName = EditorGUILayout.TextField("Nation Name", nation.nationName);
                    
                    // Controlled regions
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Controlled Regions", EditorStyles.boldLabel);
                    
                    if (nation.controlledRegionIds == null)
                    {
                        nation.controlledRegionIds = new List<string>();
                    }
                    
                    // Get available region IDs for selection
                    List<string> availableRegionIds = new List<string>();
                    foreach (var region in currentScenario.regionStartConditions)
                    {
                        availableRegionIds.Add(region.regionId);
                    }
                    
                    for (int j = 0; j < nation.controlledRegionIds.Count; j++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        
                        int selectedIndex = availableRegionIds.IndexOf(nation.controlledRegionIds[j]);
                        if (selectedIndex < 0 && availableRegionIds.Count > 0) selectedIndex = 0;
                        
                        if (availableRegionIds.Count > 0)
                        {
                            selectedIndex = EditorGUILayout.Popup("Region", selectedIndex, availableRegionIds.ToArray());
                            nation.controlledRegionIds[j] = availableRegionIds[selectedIndex];
                        }
                        else
                        {
                            EditorGUILayout.LabelField("Region", "No regions defined");
                        }
                        
                        if (GUILayout.Button("X", GUILayout.Width(20)))
                        {
                            nation.controlledRegionIds.RemoveAt(j);
                            EditorUtility.SetDirty(currentScenario);
                            j--;
                        }
                        
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    if (availableRegionIds.Count > 0 && GUILayout.Button("Add Region", GUILayout.Width(150)))
                    {
                        string regionToAdd = availableRegionIds[0];
                        // Try to add a region that's not already controlled
                        foreach (var regionId in availableRegionIds)
                        {
                            if (!nation.controlledRegionIds.Contains(regionId))
                            {
                                regionToAdd = regionId;
                                break;
                            }
                        }
                        
                        nation.controlledRegionIds.Add(regionToAdd);
                        EditorUtility.SetDirty(currentScenario);
                    }
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(currentScenario);
                    }
                    
                    // Delete nation button
                    if (GUILayout.Button("Remove Nation", GUILayout.Width(150)))
                    {
                        currentScenario.nationStartConditions.RemoveAt(i);
                        EditorUtility.SetDirty(currentScenario);
                        i--;
                        EditorGUILayout.EndVertical();
                        continue;
                    }
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(5);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No nations defined yet.", MessageType.Info);
            }
            
            // Add new nation section
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Add New Nation", EditorStyles.boldLabel);
            
            newNation.nationId = EditorGUILayout.TextField("Nation ID", newNation.nationId);
            newNation.nationName = EditorGUILayout.TextField("Nation Name", newNation.nationName);
            
            if (GUILayout.Button("Add Nation", GUILayout.Width(150)))
            {
                if (string.IsNullOrEmpty(newNation.nationId) || string.IsNullOrEmpty(newNation.nationName))
                {
                    EditorUtility.DisplayDialog("Missing Information", 
                        "Nation ID and Name are required.", "OK");
                    return;
                }
                
                // Add the new nation to the scenario
                NationStartCondition newNationCopy = new NationStartCondition
                {
                    nationId = newNation.nationId,
                    nationName = newNation.nationName,
                    controlledRegionIds = new List<string>()
                };
                
                currentScenario.nationStartConditions.Add(newNationCopy);
                EditorUtility.SetDirty(currentScenario);
                
                // Reset the new nation template
                newNation = new NationStartCondition();
            }
        }
        
        private void DrawVictorySection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Create victory condition if null
            if (currentScenario.victoryCondition == null)
            {
                currentScenario.victoryCondition = new VictoryCondition();
                EditorUtility.SetDirty(currentScenario);
            }
            
            VictoryCondition victory = currentScenario.victoryCondition;
            
            EditorGUI.BeginChangeCheck();
            
            // Victory type
            victory.type = (VictoryCondition.VictoryType)EditorGUILayout.EnumPopup(
                "Victory Type", victory.type);
                
            // Target region
            List<string> regionOptions = new List<string> { "All Regions" };
            foreach (var region in currentScenario.regionStartConditions)
            {
                regionOptions.Add(region.regionId);
            }
            
            int selectedRegionIndex = string.IsNullOrEmpty(victory.targetRegionId) ? 0 : 
                regionOptions.IndexOf(victory.targetRegionId);
            if (selectedRegionIndex < 0) selectedRegionIndex = 0;
            
            selectedRegionIndex = EditorGUILayout.Popup("Target Region", selectedRegionIndex, regionOptions.ToArray());
            victory.targetRegionId = selectedRegionIndex == 0 ? "" : regionOptions[selectedRegionIndex];
            
            // Draw fields based on victory type
            switch (victory.type)
            {
                case VictoryCondition.VictoryType.Economic:
                    victory.requiredWealth = EditorGUILayout.IntField("Required Wealth", victory.requiredWealth);
                    break;
                    
                case VictoryCondition.VictoryType.Development:
                    victory.requiredInfrastructure = EditorGUILayout.FloatField(
                        "Required Infrastructure", victory.requiredInfrastructure);
                    break;
                    
                case VictoryCondition.VictoryType.Stability:
                    victory.requiredSatisfaction = EditorGUILayout.Slider(
                        "Required Satisfaction", victory.requiredSatisfaction, 0f, 1f);
                    break;
            }
            
            // Required consecutive turns
            victory.requiredConsecutiveTurns = EditorGUILayout.IntField(
                "Required Consecutive Turns", victory.requiredConsecutiveTurns);
                
            if (victory.requiredConsecutiveTurns < 1)
                victory.requiredConsecutiveTurns = 1;
                
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(currentScenario);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void RunScenario()
        {
            if (currentScenario == null)
            {
                EditorUtility.DisplayDialog("Error", "No scenario selected.", "OK");
                return;
            }
            
            // Save current changes
            EditorUtility.SetDirty(currentScenario);
            AssetDatabase.SaveAssets();
            
            // Instructions to run manually in play mode
            if (!EditorApplication.isPlaying)
            {
                if (EditorUtility.DisplayDialog("Enter Play Mode", 
                    "To run the scenario:\n1. Enter Play Mode\n2. Find or add ScenarioManager component in scene\n3. Drag this scenario to the ScenarioManager", 
                    "OK", "Cancel"))
                {
                    EditorPrefs.SetString("PendingScenarioPath", AssetDatabase.GetAssetPath(currentScenario));
                    EditorApplication.isPlaying = true;
                }
            }
            else
            {
                // Already in play mode, find manager
                ScenarioManager manager = FindFirstObjectByType<ScenarioManager>();
                if (manager != null)
                {
                    manager.StartScenario(currentScenario);
                }
                else
                {
                    // Create a manager
                    GameObject scenarioObj = new GameObject("ScenarioManager");
                    manager = scenarioObj.AddComponent<ScenarioManager>();
                    manager.StartScenario(currentScenario);
                }
            }
        }
    }
}