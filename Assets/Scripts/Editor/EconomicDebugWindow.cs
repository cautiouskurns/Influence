using UnityEngine;
using UnityEditor;
using Systems;
using Entities;
using Core;
using System.Collections.Generic;

namespace Editor
{
    /// <summary>
    /// Simple editor window for debugging and tuning economic simulation parameters
    /// </summary>
    public class EconomicDebugWindow : EditorWindow
    {
        // References to components
        private SerializedObject serializedEconomicSystem;
        private EconomicSystem economicSystem;
        
        // Simulation settings
        private bool autoRunEnabled = false;
        private float autoRunInterval = 1.0f;
        private double lastAutoRunTime;
        private bool simulationActive = false;
        
        // Parameter settings
        private float productivityFactor = 1.0f;
        private float laborElasticity = 0.5f;
        private float capitalElasticity = 0.5f;
        
        // Region values
        private int laborAvailable = 100;
        private int infrastructureLevel = 5;
        
        // Graph data
        private List<int> wealthHistory = new List<int>();
        private List<int> productionHistory = new List<int>();
        private Vector2 graphScrollPosition = Vector2.zero;
        private float graphHeight = 200f;
        
        // Current turn tracking
        private int currentTurn = 1;
        
        // Foldout states
        private bool showParameters = true;
        private bool showRegionControls = true;
        private bool showResults = true;
        private bool showGraphs = true;
        
        [MenuItem("Window/Economic Debug")]
        public static void ShowWindow()
        {
            var window = GetWindow<EconomicDebugWindow>("Economic Debug");
            window.minSize = new Vector2(500, 700);
        }
        
        private void OnEnable()
        {
            // Try to find the economic system
            FindEconomicSystem();
            
            // Start editor update for auto-run
            EditorApplication.update += OnEditorUpdate;
        }
        
        private void OnDisable()
        {
            // Clean up editor update
            EditorApplication.update -= OnEditorUpdate;
        }
        
        private void FindEconomicSystem()
        {
            // Try to find an economic system in the scene
            economicSystem = FindFirstObjectByType<EconomicSystem>();
            
            if (economicSystem != null)
            {
                serializedEconomicSystem = new SerializedObject(economicSystem);
                SyncFromSystem();
            }
        }
        
        // Modify this method to directly update the graph after each economic tick
        private void OnEditorUpdate()
        {
            // Handle auto-run functionality
            if (simulationActive && autoRunEnabled && economicSystem != null)
            {
                double currentTime = EditorApplication.timeSinceStartup;
                if (currentTime - lastAutoRunTime >= autoRunInterval)
                {
                    lastAutoRunTime = currentTime;
                    RunSimulationTick();
                }
            }
            
            // Update simulation active state
            bool wasActive = simulationActive;
            simulationActive = EditorApplication.isPlaying;
            
            // If we just entered play mode, reset some data
            if (!wasActive && simulationActive)
            {
                ResetSimulation();
            }
            
            // Check for external changes to region data (in case region values were modified outside this window)
            if (simulationActive && economicSystem != null)
            {
                SyncRegionDataForGraphs();
            }
            
            // Force repaint every frame when in Play Mode to keep UI fresh
            if (simulationActive)
            {
                Repaint();
            }
        }
        
        private void OnGUI()
        {
            // Check if we have lost our reference
            if (economicSystem == null)
            {
                FindEconomicSystem();
            }

            if (economicSystem == null)
            {
                EditorGUILayout.HelpBox("No EconomicSystem found in the scene. Open a scene with an EconomicSystem component or enter Play Mode.", MessageType.Warning);
                if (GUILayout.Button("Find EconomicSystem"))
                {
                    FindEconomicSystem();
                }
                return;
            }

            // Debug information at the top to help diagnose issues
            EditorGUILayout.LabelField($"Data points: Wealth={wealthHistory.Count}, Production={productionHistory.Count}");
            EditorGUILayout.LabelField($"Current Turn: {currentTurn}");

            // Main sections
            DrawPlayModeControls();
            DrawControlsSection();
            DrawParametersSection();
            DrawRegionControlsSection();
            DrawResultsSection();
            DrawGraphsSection();
        }
        
        private void DrawPlayModeControls()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Play Mode Controls", EditorStyles.boldLabel);
            
            // Display current state
            GUI.color = simulationActive ? Color.green : Color.red;
            EditorGUILayout.LabelField("Simulation Status: " + (simulationActive ? "ACTIVE" : "INACTIVE"));
            GUI.color = Color.white;
            
            EditorGUILayout.HelpBox(
                simulationActive 
                    ? "Simulation is running. You can test parameters in real-time." 
                    : "Enter Play Mode to run the simulation. You can still adjust parameters.",
                simulationActive ? MessageType.Info : MessageType.Warning);
            
            EditorGUILayout.Space(5);
            
            // Enter/Exit play mode buttons
            EditorGUILayout.BeginHorizontal();
            
            if (!simulationActive)
            {
                if (GUILayout.Button("Enter Play Mode", GUILayout.Height(30)))
                {
                    EditorApplication.isPlaying = true;
                }
            }
            else
            {
                if (GUILayout.Button("Exit Play Mode", GUILayout.Height(30)))
                {
                    EditorApplication.isPlaying = false;
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
        }
        
        private void DrawControlsSection()
        {
            EditorGUILayout.LabelField("Simulation Controls", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = simulationActive;
            
            if (GUILayout.Button("Run Single Tick", GUILayout.Height(30)))
            {
                RunSimulationTick();
            }
            
            if (GUILayout.Button("Reset Simulation", GUILayout.Height(30)))
            {
                ResetSimulation();
            }
            
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Auto-run section
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = simulationActive;
            
            // Auto-run toggle
            bool newAutoRunEnabled = EditorGUILayout.Toggle("Auto Run", autoRunEnabled, GUILayout.Width(100));
            if (newAutoRunEnabled != autoRunEnabled)
            {
                autoRunEnabled = newAutoRunEnabled;
                lastAutoRunTime = EditorApplication.timeSinceStartup;
            }
            
            // Auto-run interval
            EditorGUILayout.LabelField("Interval (sec):", GUILayout.Width(100));
            autoRunInterval = EditorGUILayout.Slider(autoRunInterval, 0.1f, 5.0f);
            
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            // Sync buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Sync from System"))
            {
                SyncFromSystem();
            }
            
            if (GUILayout.Button("Apply to System"))
            {
                ApplyToSystem();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
        }
        
        private void DrawParametersSection()
        {
            showParameters = EditorGUILayout.Foldout(showParameters, "Economic Parameters", true);
            
            if (showParameters)
            {
                EditorGUI.indentLevel++;
                
                // Production Parameters
                EditorGUILayout.LabelField("Production Parameters", EditorStyles.boldLabel);
                
                // Productivity Factor
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Productivity Factor:", "Controls the overall productivity multiplier of the economy"), GUILayout.Width(150));
                float newProductivityFactor = EditorGUILayout.Slider(productivityFactor, 0.1f, 5.0f);
                if (newProductivityFactor != productivityFactor)
                {
                    productivityFactor = newProductivityFactor;
                    ApplyToSystem();
                }
                EditorGUILayout.EndHorizontal();
                
                // Labor Elasticity
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Labor Elasticity:", "How much labor affects production (Cobb-Douglas function)"), GUILayout.Width(150));
                float newLaborElasticity = EditorGUILayout.Slider(laborElasticity, 0.1f, 1.0f);
                if (newLaborElasticity != laborElasticity)
                {
                    laborElasticity = newLaborElasticity;
                    ApplyToSystem();
                }
                EditorGUILayout.EndHorizontal();
                
                // Capital Elasticity
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Capital Elasticity:", "How much capital/infrastructure affects production (Cobb-Douglas function)"), GUILayout.Width(150));
                float newCapitalElasticity = EditorGUILayout.Slider(capitalElasticity, 0.1f, 1.0f);
                if (newCapitalElasticity != capitalElasticity)
                {
                    capitalElasticity = newCapitalElasticity;
                    ApplyToSystem();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
        }
        
        private void DrawRegionControlsSection()
        {
            showRegionControls = EditorGUILayout.Foldout(showRegionControls, "Region Controls", true);
            
            if (showRegionControls)
            {
                EditorGUI.indentLevel++;
                
                // Population controls
                EditorGUILayout.LabelField("Population", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Labor Available:", "Amount of labor force available in the region"), GUILayout.Width(150));
                int newLaborAvailable = EditorGUILayout.IntSlider(laborAvailable, 10, 500);
                if (newLaborAvailable != laborAvailable)
                {
                    laborAvailable = newLaborAvailable;
                    ApplyToSystem();
                }
                EditorGUILayout.EndHorizontal();
                
                // Infrastructure controls
                EditorGUILayout.LabelField("Infrastructure", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Infrastructure Level:", "Current level of infrastructure development"), GUILayout.Width(150));
                int newInfrastructureLevel = EditorGUILayout.IntSlider(infrastructureLevel, 1, 10);
                if (newInfrastructureLevel != infrastructureLevel)
                {
                    infrastructureLevel = newInfrastructureLevel;
                    ApplyToSystem();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
        }
        
        private void DrawResultsSection()
        {
            showResults = EditorGUILayout.Foldout(showResults, "Simulation Results", true);
            
            if (showResults && economicSystem != null)
            {
                EditorGUI.indentLevel++;
                
                var regions = economicSystem.GetAllRegionIds();
                if (regions.Count > 0)
                {
                    var region = economicSystem.GetRegion(regions[0]);
                    if (region != null)
                    {
                        // Current turn
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("Current Turn:", GUILayout.Width(100));
                        GUIStyle turnStyle = new GUIStyle(EditorStyles.boldLabel);
                        turnStyle.normal.textColor = new Color(1f, 0.7f, 0.2f);
                        GUILayout.Label(currentTurn.ToString(), turnStyle);
                        EditorGUILayout.EndHorizontal();
                        
                        // Wealth
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("Wealth:", GUILayout.Width(100));
                        GUI.color = new Color(0.2f, 0.8f, 0.2f);
                        GUILayout.Label(region.Wealth.ToString(), EditorStyles.boldLabel);
                        GUI.color = Color.white;
                        EditorGUILayout.EndHorizontal();
                        
                        // Production
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("Production:", GUILayout.Width(100));
                        GUI.color = new Color(0.2f, 0.2f, 0.8f);
                        GUILayout.Label(region.Production.ToString(), EditorStyles.boldLabel);
                        GUI.color = Color.white;
                        EditorGUILayout.EndHorizontal();
                        
                        // Labor
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("Labor:", GUILayout.Width(100));
                        GUILayout.Label(region.LaborAvailable.ToString(), EditorStyles.boldLabel);
                        EditorGUILayout.EndHorizontal();
                        
                        // Infrastructure
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("Infrastructure:", GUILayout.Width(100));
                        GUILayout.Label(region.InfrastructureLevel.ToString(), EditorStyles.boldLabel);
                        EditorGUILayout.EndHorizontal();
                        
                        // Total wealth across all regions
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("Total Wealth:", GUILayout.Width(100));
                        GUI.color = new Color(0.8f, 0.5f, 0.2f);
                        GUILayout.Label(economicSystem.GetTotalWealth().ToString(), EditorStyles.boldLabel);
                        GUI.color = Color.white;
                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No regions found in the economic system.", MessageType.Warning);
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
        }
        
        private void DrawGraphsSection()
        {
            showGraphs = EditorGUILayout.Foldout(showGraphs, "Economic Graphs", true);
            
            if (showGraphs)
            {
                // Graph height control
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Graph Height:", GUILayout.Width(100));
                graphHeight = EditorGUILayout.Slider(graphHeight, 100f, 400f);
                EditorGUILayout.EndHorizontal();
                
                // Begin scroll view for graphs
                graphScrollPosition = EditorGUILayout.BeginScrollView(graphScrollPosition, GUILayout.Height(graphHeight + 50));
                
                Rect graphRect = EditorGUILayout.GetControlRect(GUILayout.Height(graphHeight));
                
                // Draw graph background
                GUI.Box(graphRect, "");
                
                // Draw data if we have at least two points
                if (wealthHistory.Count > 1)
                {
                    DrawLineGraph(wealthHistory, graphRect, new Color(0.2f, 0.8f, 0.2f), "Wealth");
                    DrawLineGraph(productionHistory, graphRect, new Color(0.2f, 0.2f, 0.8f), "Production");
                    
                    // Draw legend
                    DrawLegend(graphRect);
                }
                else
                {
                    // Show message if no data
                    string message = "No data available.\nRun simulation to record data.";
                    GUI.Label(new Rect(graphRect.x + 20, graphRect.y + 50, graphRect.width - 40, 60), message);
                }
                
                EditorGUILayout.EndScrollView();
                
                EditorGUILayout.HelpBox("Graph shows wealth (green) and production (blue) over time.", MessageType.Info);
            }
            
            EditorGUILayout.Space(5);
        }
        
        private void DrawLineGraph(List<int> data, Rect rect, Color color, string label)
        {
            if (data == null || data.Count < 2) return;
            
            // Find max value for scaling
            int maxValue = 10; // Minimum to ensure visibility
            foreach (int value in data)
            {
                maxValue = Mathf.Max(maxValue, value);
            }
            
            // Add headroom to the top
            maxValue = Mathf.CeilToInt(maxValue * 1.1f);
            
            // Adjust the rect to leave more space for axis labels
            Rect adjustedRect = new Rect(
                rect.x + 100, // Move graph right to make room for y-axis labels
                rect.y + 20, // Move graph down to make room for top labels
                rect.width - 80, // Reduce width to keep right margin
                rect.height - 40  // Reduce height to keep bottom margin for x-axis labels
            );
            
            // Add a dark background to the graph area to improve contrast
            EditorGUI.DrawRect(adjustedRect, new Color(0.15f, 0.15f, 0.15f, 0.4f));
            
            // Calculate points
            Vector3[] points = new Vector3[data.Count];
            
            for (int i = 0; i < data.Count; i++)
            {
                float x = adjustedRect.x + (i * adjustedRect.width) / (data.Count - 1);
                float y = adjustedRect.y + adjustedRect.height - (data[i] * adjustedRect.height) / maxValue;
                points[i] = new Vector3(x, y, 0);
            }
            
            // Draw lines
            Handles.color = color;
            Handles.DrawAAPolyLine(4f, points);
            
            // Create better styles for labels
            GUIStyle axisLabelStyle = new GUIStyle(EditorStyles.boldLabel);
            axisLabelStyle.normal.textColor = Color.white;
            axisLabelStyle.fontSize = 12;
            
            GUIStyle tickLabelStyle = new GUIStyle(EditorStyles.miniLabel);
            tickLabelStyle.normal.textColor = Color.white;
            tickLabelStyle.fontSize = 10;
            tickLabelStyle.alignment = TextAnchor.MiddleRight;
            
            // Draw outer box around the entire graph area
            Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            Handles.DrawLine(
                new Vector3(adjustedRect.x - 5, adjustedRect.y - 5, 0),
                new Vector3(adjustedRect.x + adjustedRect.width + 5, adjustedRect.y - 5, 0)
            );
            Handles.DrawLine(
                new Vector3(adjustedRect.x + adjustedRect.width + 5, adjustedRect.y - 5, 0),
                new Vector3(adjustedRect.x + adjustedRect.width + 5, adjustedRect.y + adjustedRect.height + 5, 0)
            );
            Handles.DrawLine(
                new Vector3(adjustedRect.x + adjustedRect.width + 5, adjustedRect.y + adjustedRect.height + 5, 0),
                new Vector3(adjustedRect.x - 5, adjustedRect.y + adjustedRect.height + 5, 0)
            );
            Handles.DrawLine(
                new Vector3(adjustedRect.x - 5, adjustedRect.y + adjustedRect.height + 5, 0),
                new Vector3(adjustedRect.x - 5, adjustedRect.y - 5, 0)
            );
            
            // Draw y-axis labels with improved style and positioning
            // Max value at top
            GUI.Label(new Rect(adjustedRect.x - 50, adjustedRect.y - 10, 45, 20), maxValue.ToString(), tickLabelStyle);
            
            // Mid-values
            for (int i = 1; i < 4; i++)
            {
                float yPos = adjustedRect.y + (adjustedRect.height / 4) * i;
                int value = Mathf.RoundToInt(maxValue * (1 - (i / 4.0f)));
                GUI.Label(new Rect(adjustedRect.x - 50, yPos - 10, 45, 20), value.ToString(), tickLabelStyle);
            }
            
            // Zero at bottom
            GUI.Label(new Rect(adjustedRect.x - 50, adjustedRect.y + adjustedRect.height - 10, 45, 20), "0", tickLabelStyle);
            
            // Draw axis labels
            GUI.Label(new Rect(adjustedRect.x - 70, adjustedRect.y + (adjustedRect.height / 2) - 30, 20, 60), "Value", axisLabelStyle);
            
            // Draw x-axis (time)
            Handles.color = Color.gray;
            Handles.DrawLine(
                new Vector3(adjustedRect.x, adjustedRect.y + adjustedRect.height, 0),
                new Vector3(adjustedRect.x + adjustedRect.width, adjustedRect.y + adjustedRect.height, 0)
            );
            
            // Draw y-axis
            Handles.DrawLine(
                new Vector3(adjustedRect.x, adjustedRect.y, 0),
                new Vector3(adjustedRect.x, adjustedRect.y + adjustedRect.height, 0)
            );
            
            // Draw interval lines for y-axis
            for (int i = 1; i < 5; i++)
            {
                float y = adjustedRect.y + (adjustedRect.height / 5) * i;
                Handles.DrawDottedLine(
                    new Vector3(adjustedRect.x, y, 0),
                    new Vector3(adjustedRect.x + adjustedRect.width, y, 0),
                    2f
                );
            }
            
            // Draw x-axis tick marks and turn numbers with improved styling
            int tickCount = Mathf.Min(10, data.Count); // Show at most 10 ticks to avoid overcrowding
            if (data.Count > 0)
            {
                GUIStyle turnLabelStyle = new GUIStyle(EditorStyles.miniLabel);
                turnLabelStyle.normal.textColor = Color.white;
                turnLabelStyle.fontSize = 10;
                turnLabelStyle.alignment = TextAnchor.UpperCenter;
                
                for (int i = 0; i < tickCount; i++)
                {
                    // Calculate position evenly spaced along x-axis
                    int dataIndex = i * (data.Count - 1) / (tickCount - 1);
                    if (tickCount == 1) dataIndex = 0;
                    
                    float x = adjustedRect.x + (dataIndex * adjustedRect.width) / (data.Count - 1);
                    
                    // Draw tick mark
                    Handles.DrawLine(
                        new Vector3(x, adjustedRect.y + adjustedRect.height, 0),
                        new Vector3(x, adjustedRect.y + adjustedRect.height + 5, 0)
                    );
                    
                    // Calculate turn number based on current turn and history length
                    int turnNumber = currentTurn - (data.Count - 1 - dataIndex);
                    
                    // Only show turn number if it's positive (actual turns that have happened)
                    if (turnNumber > 0)
                    {
                        // Draw turn number with better styling and alignment
                        GUI.Label(new Rect(x - 15, adjustedRect.y + adjustedRect.height + 5, 30, 20), turnNumber.ToString(), turnLabelStyle);
                    }
                }
            }
            
            // Draw X-axis label with better styling
            GUI.Label(
                new Rect(adjustedRect.x + adjustedRect.width/2 - 20, adjustedRect.y + adjustedRect.height + 25, 40, 20), 
                "Turn", 
                axisLabelStyle
            );
        }
        
        private void DrawLegend(Rect graphRect)
        {
            float legendWidth = 100;
            float legendHeight = 50;
            Rect legendRect = new Rect(
                graphRect.x + graphRect.width - legendWidth - 10,
                graphRect.y + 10,
                legendWidth,
                legendHeight
            );
            
            // Draw legend background
            GUI.Box(legendRect, "");
            
            // Draw wealth legend item
            Rect wealthLegendRect = new Rect(legendRect.x + 10, legendRect.y + 10, 10, 10);
            EditorGUI.DrawRect(wealthLegendRect, new Color(0.2f, 0.8f, 0.2f));
            GUI.Label(new Rect(wealthLegendRect.x + 15, wealthLegendRect.y - 2, 80, 20), "Wealth");
            
            // Draw production legend item
            Rect productionLegendRect = new Rect(legendRect.x + 10, legendRect.y + 30, 10, 10);
            EditorGUI.DrawRect(productionLegendRect, new Color(0.2f, 0.2f, 0.8f));
            GUI.Label(new Rect(productionLegendRect.x + 15, productionLegendRect.y - 2, 80, 20), "Production");
        }
        
        private void RunSimulationTick()
        {
            if (economicSystem != null && simulationActive)
            {
                // Apply current values before running
                ApplyToSystem();
                
                // Run the simulation tick
                economicSystem.ProcessEconomicTick();
                
                // Update turn counter
                currentTurn++;
                
                // Manually record data from regions to ensure graphs update
                var regions = economicSystem.GetAllRegionIds();
                if (regions.Count > 0)
                {
                    var region = economicSystem.GetRegion(regions[0]);
                    if (region != null)
                    {
                        // Directly add data to history lists
                        wealthHistory.Add(region.Wealth);
                        productionHistory.Add(region.Production);
                        
                        // Keep graphs to a reasonable length
                        const int maxHistoryLength = 100;
                        if (wealthHistory.Count > maxHistoryLength)
                            wealthHistory.RemoveAt(0);
                        if (productionHistory.Count > maxHistoryLength)
                            productionHistory.RemoveAt(0);
                        
                        // Log data for debugging
                        Debug.Log($"Turn {currentTurn}: Wealth={region.Wealth}, Production={region.Production}");
                    }
                }
                
                // Resync values
                SyncFromSystem();
                
                // Force immediate repaint of editor window to update graphs
                Repaint();
            }
        }
        
        private void ResetSimulation()
        {
            if (economicSystem != null && simulationActive)
            {
                // Reset turn counter
                currentTurn = 1;
                
                // Reset test region in economic system
                var regions = economicSystem.GetAllRegionIds();
                if (regions.Count > 0)
                {
                    var region = economicSystem.GetRegion(regions[0]);
                    if (region != null)
                    {
                        // Set initial values
                        region.Wealth = 100;
                        region.Production = 50;
                        region.LaborAvailable = laborAvailable;
                        region.InfrastructureLevel = infrastructureLevel;
                        economicSystem.UpdateRegion(region);
                        
                        // Clear history
                        wealthHistory.Clear();
                        productionHistory.Clear();
                        
                        // Add the first data point to start the graph
                        wealthHistory.Add(region.Wealth);
                        productionHistory.Add(region.Production);
                        
                        Debug.Log($"Simulation reset. Initial values - Wealth: {region.Wealth}, Production: {region.Production}");
                    }
                }
                
                // Resync values
                SyncFromSystem();
                
                // Force immediate repaint
                Repaint();
                
                // Log confirmation
                Debug.Log("Simulation reset completed.");
            }
        }
        
        private void SyncFromSystem()
        {
            if (economicSystem == null) return;
            
            // Update the serialized object
            if (serializedEconomicSystem != null)
            {
                serializedEconomicSystem.Update();
            }
            
            // Sync economic parameters
            productivityFactor = economicSystem.productivityFactor;
            laborElasticity = economicSystem.laborElasticity;
            capitalElasticity = economicSystem.capitalElasticity;
            
            // Sync region values from the first region
            var regions = economicSystem.GetAllRegionIds();
            if (regions.Count > 0)
            {
                var region = economicSystem.GetRegion(regions[0]);
                if (region != null)
                {
                    laborAvailable = (int)region.LaborAvailable;
                    infrastructureLevel = (int)region.InfrastructureLevel;
                    
                    // Record initial history if empty
                    if (wealthHistory.Count == 0)
                    {
                        RecordHistory();
                    }
                }
            }
        }
        
        private void ApplyToSystem()
        {
            if (economicSystem == null) return;
            
            // Apply economic parameters
            economicSystem.productivityFactor = productivityFactor;
            economicSystem.laborElasticity = laborElasticity;
            economicSystem.capitalElasticity = capitalElasticity;
            
            // Apply region values if in play mode
            if (simulationActive)
            {
                var regions = economicSystem.GetAllRegionIds();
                if (regions.Count > 0)
                {
                    var region = economicSystem.GetRegion(regions[0]);
                    if (region != null)
                    {
                        region.LaborAvailable = laborAvailable;
                        region.InfrastructureLevel = infrastructureLevel;
                        economicSystem.UpdateRegion(region);
                    }
                }
            }
            
            // Mark the object as dirty to ensure values persist
            if (serializedEconomicSystem != null)
            {
                serializedEconomicSystem.ApplyModifiedProperties();
                EditorUtility.SetDirty(economicSystem);
            }
        }
        
        private void RecordHistory()
        {
            if (economicSystem == null) return;
            
            var regions = economicSystem.GetAllRegionIds();
            if (regions.Count > 0)
            {
                var region = economicSystem.GetRegion(regions[0]);
                if (region != null)
                {
                    // Record wealth and production
                    wealthHistory.Add(region.Wealth);
                    productionHistory.Add(region.Production);
                    
                    // Trim history if too long (keep last 100 entries)
                    const int maxHistoryLength = 100;
                    if (wealthHistory.Count > maxHistoryLength)
                    {
                        wealthHistory.RemoveAt(0);
                    }
                    if (productionHistory.Count > maxHistoryLength)
                    {
                        productionHistory.RemoveAt(0);
                    }
                    
                    // Force repaint to update the graph immediately
                    Repaint();
                }
            }
        }

        private void SyncRegionDataForGraphs()
        {
            if (economicSystem == null) return;
            
            // Check if we have regions
            var regions = economicSystem.GetAllRegionIds();
            if (regions.Count == 0) return;
            
            // Get the first region to track
            var region = economicSystem.GetRegion(regions[0]);
            if (region == null) return;
            
            // Check if we have history data already
            if (wealthHistory.Count > 0)
            {
                // If the last data point doesn't match the current values, add a new point
                int lastWealthPoint = wealthHistory[wealthHistory.Count - 1];
                int lastProductionPoint = productionHistory[productionHistory.Count - 1];
                
                if (lastWealthPoint != region.Wealth || lastProductionPoint != region.Production)
                {
                    // Values have changed externally, record them
                    wealthHistory.Add(region.Wealth);
                    productionHistory.Add(region.Production);
                    
                    // Trim history if too long
                    const int maxHistoryLength = 100;
                    if (wealthHistory.Count > maxHistoryLength)
                        wealthHistory.RemoveAt(0);
                    if (productionHistory.Count > maxHistoryLength)
                        productionHistory.RemoveAt(0);
                        
                    Debug.Log($"Detected external change: Wealth={region.Wealth}, Production={region.Production}");
                    
                    // Update turn counter if it wasn't updated elsewhere
                    if (wealthHistory.Count > currentTurn)
                        currentTurn = wealthHistory.Count;
                }
            }
            else
            {
                // No history yet, initialize with first data point
                wealthHistory.Add(region.Wealth);
                productionHistory.Add(region.Production);
                Debug.Log($"Initialized graph data: Wealth={region.Wealth}, Production={region.Production}");
            }
        }
    }
}