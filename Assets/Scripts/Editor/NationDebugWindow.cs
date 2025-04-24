using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Entities;
using Managers;
using Systems;

namespace Editor
{
    public class NationDebugWindow : EditorWindow
    {
        // Reference to managers
        private NationManager nationManager;
        private EconomicSystem economicSystem;
        
        // UI state
        private Vector2 scrollPosition;
        private string newNationId = "new_nation";
        private string newNationName = "New Nation";
        private Color newNationColor = Color.gray;
        
        // Selected nation for editing
        private string selectedNationId;
        
        [MenuItem("Economic Cycles/Nation Debug")]
        public static void ShowWindow()
        {
            GetWindow<NationDebugWindow>("Nation Debug");
        }
        
        private void OnEnable()
        {
            // Initialize references
            nationManager = FindFirstObjectByType<NationManager>();
            economicSystem = FindFirstObjectByType<EconomicSystem>();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Nations Debug Window", EditorStyles.boldLabel);
            
            // Initialize manager references if needed
            if (nationManager == null || economicSystem == null)
            {
                if (GUILayout.Button("Initialize References"))
                {
                    nationManager = FindFirstObjectByType<NationManager>();
                    economicSystem = FindFirstObjectByType<EconomicSystem>();
                }
                
                EditorGUILayout.HelpBox("Nation Manager or Economic System not found in scene.", MessageType.Warning);
                return;
            }
            
            // Section for creating a new nation
            DrawCreateNationSection();
            
            EditorGUILayout.Space();
            
            // Section for nation list and details
            DrawNationListSection();
            
            EditorGUILayout.Space();
            
            // Section for economic data
            DrawEconomicDataSection();
        }
        
        private void DrawCreateNationSection()
        {
            GUILayout.Label("Create New Nation", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Nation ID:", GUILayout.Width(80));
            newNationId = EditorGUILayout.TextField(newNationId);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name:", GUILayout.Width(80));
            newNationName = EditorGUILayout.TextField(newNationName);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Color:", GUILayout.Width(80));
            newNationColor = EditorGUILayout.ColorField(newNationColor);
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Create Nation"))
            {
                if (string.IsNullOrEmpty(newNationId) || string.IsNullOrEmpty(newNationName))
                {
                    EditorUtility.DisplayDialog("Error", "Nation ID and Name cannot be empty.", "OK");
                }
                else
                {
                    nationManager.CreateNation(newNationId, newNationName, newNationColor);
                    newNationId = "new_nation";
                    newNationName = "New Nation";
                    newNationColor = Color.gray;
                }
            }
        }
        
        private void DrawNationListSection()
        {
            GUILayout.Label("Nations", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            
            List<string> nationIds = nationManager.GetAllNationIds();
            
            if (nationIds.Count == 0)
            {
                EditorGUILayout.HelpBox("No nations created yet.", MessageType.Info);
            }
            else
            {
                foreach (string nationId in nationIds)
                {
                    NationEntity nation = nationManager.GetNation(nationId);
                    
                    if (nation == null) continue;
                    
                    EditorGUILayout.BeginHorizontal(GUI.skin.box);
                    
                    // Nation color indicator
                    GUILayout.Box("", GUILayout.Width(20), GUILayout.Height(20));
                    Rect colorRect = GUILayoutUtility.GetLastRect();
                    EditorGUI.DrawRect(colorRect, nation.NationColor);
                    
                    // Nation name
                    GUILayout.Label($"{nation.Name} (ID: {nation.Id})", EditorStyles.boldLabel);
                    
                    // Region count
                    GUILayout.Label($"Regions: {nation.GetRegionIds().Count}", GUILayout.Width(100));
                    
                    // Select button
                    if (GUILayout.Button("Select", GUILayout.Width(80)))
                    {
                        selectedNationId = nation.Id;
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            EditorGUILayout.EndScrollView();
            
            // Display selected nation details
            if (!string.IsNullOrEmpty(selectedNationId))
            {
                NationEntity selectedNation = nationManager.GetNation(selectedNationId);
                
                if (selectedNation != null)
                {
                    EditorGUILayout.LabelField("Selected Nation Details", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Summary:", EditorStyles.boldLabel);
                    EditorGUILayout.HelpBox(selectedNation.GetSummary(), MessageType.None);
                    
                    EditorGUILayout.Space();
                    
                    EditorGUILayout.LabelField("Regions:", EditorStyles.boldLabel);
                    
                    List<string> regionIds = selectedNation.GetRegionIds();
                    
                    if (regionIds.Count == 0)
                    {
                        EditorGUILayout.HelpBox("No regions in this nation.", MessageType.Info);
                    }
                    else
                    {
                        foreach (string regionId in regionIds)
                        {
                            RegionEntity region = economicSystem.GetRegion(regionId);
                            
                            if (region != null)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(region.Name);
                                EditorGUILayout.LabelField($"Wealth: {region.Wealth}", GUILayout.Width(100));
                                EditorGUILayout.LabelField($"Production: {region.Production}", GUILayout.Width(120));
                                EditorGUILayout.EndHorizontal();
                            }
                            else
                            {
                                EditorGUILayout.LabelField($"Region {regionId} not found in Economic System");
                            }
                        }
                    }
                }
            }
        }
        
        private void DrawEconomicDataSection()
        {
            GUILayout.Label("National Economic Data", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Refresh Economic Data"))
            {
                Repaint();
            }
            
            Dictionary<string, int> nationWealth = economicSystem.GetNationWealthData();
            Dictionary<string, int> nationProduction = economicSystem.GetNationProductionData();
            Dictionary<string, float> nationInfrastructure = economicSystem.GetNationAverageInfrastructureData();
            
            EditorGUILayout.LabelField("Nation", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Nation", EditorStyles.boldLabel, GUILayout.Width(150));
            EditorGUILayout.LabelField("Wealth", EditorStyles.boldLabel, GUILayout.Width(100));
            EditorGUILayout.LabelField("Production", EditorStyles.boldLabel, GUILayout.Width(100));
            EditorGUILayout.LabelField("Avg. Infrastructure", EditorStyles.boldLabel, GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();
            
            foreach (string nationId in nationManager.GetAllNationIds())
            {
                NationEntity nation = nationManager.GetNation(nationId);
                
                if (nation == null) continue;
                
                EditorGUILayout.BeginHorizontal();
                
                // Color indicator
                EditorGUILayout.LabelField(new GUIContent("■"), GUILayout.Width(20));
                GUI.color = nation.NationColor;
                GUILayout.Box("", GUILayout.Width(10), GUILayout.Height(10));
                GUI.color = Color.white;
                
                // Nation name
                EditorGUILayout.LabelField(nation.Name, GUILayout.Width(120));
                
                // Economic data
                int wealth = nationWealth.ContainsKey(nationId) ? nationWealth[nationId] : 0;
                int production = nationProduction.ContainsKey(nationId) ? nationProduction[nationId] : 0;
                float infrastructure = nationInfrastructure.ContainsKey(nationId) ? nationInfrastructure[nationId] : 0;
                
                EditorGUILayout.LabelField(wealth.ToString(), GUILayout.Width(100));
                EditorGUILayout.LabelField(production.ToString(), GUILayout.Width(100));
                EditorGUILayout.LabelField(infrastructure.ToString("F1"), GUILayout.Width(120));
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space();
            
            // Strongest nation summary
            EditorGUILayout.LabelField("Strongest Nation Summary", EditorStyles.boldLabel);
            string summary = economicSystem.GetStrongestNationSummary();
            EditorGUILayout.HelpBox(summary, MessageType.None);
        }
    }
}