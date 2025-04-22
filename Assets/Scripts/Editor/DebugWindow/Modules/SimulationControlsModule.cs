using UnityEngine;
using UnityEditor;
using Systems;

namespace Editor.DebugWindow.Modules
{
    /// <summary>
    /// Module for handling simulation controls in the debug window
    /// </summary>
    public class SimulationControlsModule : IEconomicDebugModule
    {
        // Simulation settings
        private bool autoRunEnabled = false;
        private float autoRunInterval = 1.0f;
        private double lastAutoRunTime;
        private bool simulationActive = false;
        
        // Reference to main window
        private EconomicDebugWindow window;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public SimulationControlsModule(EconomicDebugWindow window)
        {
            this.window = window;
        }
        
        /// <summary>
        /// Draw the simulation controls UI
        /// </summary>
        public void Draw()
        {
            // Header area with system status
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 14;
            EditorGUILayout.LabelField("Economic Debug", headerStyle);
            
            string statusMessage = simulationActive 
                ? "Simulation ACTIVE - Play Mode" 
                : "Simulation INACTIVE - Edit Mode";
            
            GUIStyle statusStyle = new GUIStyle(EditorStyles.boldLabel);
            statusStyle.normal.textColor = simulationActive 
                ? new Color(0.0f, 0.7f, 0.0f) 
                : new Color(0.7f, 0.0f, 0.0f);
                
            EditorGUILayout.LabelField(statusMessage, statusStyle);
            
            // Show separator before controls
            EditorGUILayout.Space(5);
            
            // Simulation controls
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = simulationActive && !autoRunEnabled;
            
            if (GUILayout.Button("Run Turn", GUILayout.Height(30), GUILayout.Width(100)))
            {
                window.RunSimulationTick();
            }
            
            GUI.enabled = simulationActive;
            
            // Auto-run toggle and speed
            bool newAutoRunEnabled = EditorGUILayout.Toggle("Auto-Run:", autoRunEnabled, GUILayout.Width(80));
            if (newAutoRunEnabled != autoRunEnabled)
            {
                autoRunEnabled = newAutoRunEnabled;
                if (autoRunEnabled)
                {
                    lastAutoRunTime = EditorApplication.timeSinceStartup;
                }
            }
            
            EditorGUILayout.LabelField("Speed:", GUILayout.Width(45));
            float newAutoRunInterval = EditorGUILayout.Slider(autoRunInterval, 0.1f, 3.0f, GUILayout.Width(150));
            if (newAutoRunInterval != autoRunInterval)
            {
                autoRunInterval = newAutoRunInterval;
            }
            
            GUI.enabled = simulationActive;
            if (GUILayout.Button("Reset", GUILayout.Height(30), GUILayout.Width(80)))
            {
                window.ResetSimulation();
            }
            
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            string simulationInstructions = "Enter Play Mode to activate simulation. ";
            if (simulationActive)
            {
                simulationInstructions = autoRunEnabled 
                    ? $"Auto-running every {autoRunInterval:F1} seconds." 
                    : "Press 'Run Turn' to advance simulation one turn.";
            }
            
            EditorGUILayout.HelpBox(simulationInstructions, MessageType.Info);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
        }
        
        /// <summary>
        /// Handle editor update
        /// </summary>
        public void OnEditorUpdate(float deltaTime)
        {
            // Update simulation active state
            bool wasActive = simulationActive;
            simulationActive = EditorApplication.isPlaying;
            
            // If we just entered play mode, reset some data
            if (!wasActive && simulationActive)
            {
                window.ResetSimulation();
            }
            
            // Handle auto-run functionality
            if (simulationActive && autoRunEnabled)
            {
                double currentTime = EditorApplication.timeSinceStartup;
                if (currentTime - lastAutoRunTime >= autoRunInterval)
                {
                    lastAutoRunTime = currentTime;
                    window.RunSimulationTick();
                }
            }
        }
        
        /// <summary>
        /// Sync from the economic system
        /// </summary>
        public void SyncFromSystem(EconomicSystem economicSystem)
        {
            // Nothing to sync for simulation controls
        }
        
        /// <summary>
        /// Apply to the economic system
        /// </summary>
        public void ApplyToSystem(EconomicSystem economicSystem)
        {
            // Nothing to apply for simulation controls
        }
        
        /// <summary>
        /// Reset the module
        /// </summary>
        public void Reset()
        {
            autoRunEnabled = false;
        }
        
        /// <summary>
        /// Get whether the simulation is active
        /// </summary>
        public bool IsSimulationActive()
        {
            return simulationActive;
        }
        
        /// <summary>
        /// Get whether auto-run is enabled
        /// </summary>
        public bool IsAutoRunEnabled()
        {
            return autoRunEnabled;
        }
    }
}