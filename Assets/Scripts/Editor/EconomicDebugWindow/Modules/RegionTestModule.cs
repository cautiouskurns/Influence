using UnityEngine;
using UnityEditor;
using Systems;
using Editor.DebugWindow.Data;
using System.Linq; // Add LINQ namespace for IEnumerable extension methods

namespace Editor.DebugWindow.Modules
{
    /// <summary>
    /// Module for handling region test data in the debug window
    /// </summary>
    public class RegionTestModule : IEconomicDebugModule
    {
        // Region control data
        private bool showRegionControls = true;
        private RegionParameters regionParams = new RegionParameters();
        
        // Track if parameters have changed since last apply/sync
        private bool parametersChanged = false;
        
        /// <summary>
        /// Draw the region test controls UI
        /// </summary>
        public void Draw()
        {
            showRegionControls = EditorGUILayout.Foldout(showRegionControls, "Region Controls", true);
            
            if (showRegionControls)
            {
                EditorGUI.indentLevel++;
                
                // Labor
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Available Labor:", "Amount of labor available in the region"), GUILayout.Width(150));
                int newLabor = EditorGUILayout.IntSlider(regionParams.laborAvailable, 10, 500);
                if (newLabor != regionParams.laborAvailable)
                {
                    regionParams.laborAvailable = newLabor;
                    parametersChanged = true;
                }
                EditorGUILayout.EndHorizontal();
                
                // Infrastructure Level
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Infrastructure Level:", "Current level of infrastructure"), GUILayout.Width(150));
                int newInfrastructure = EditorGUILayout.IntSlider(regionParams.infrastructureLevel, 1, 20);
                if (newInfrastructure != regionParams.infrastructureLevel)
                {
                    regionParams.infrastructureLevel = newInfrastructure;
                    parametersChanged = true;
                }
                EditorGUILayout.EndHorizontal();
                
                // Maintenance Investment
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Maintenance Investment:", "Investment in infrastructure maintenance"), GUILayout.Width(150));
                float newMaintenance = EditorGUILayout.Slider(regionParams.maintenanceInvestment, 0f, 50f);
                if (newMaintenance != regionParams.maintenanceInvestment)
                {
                    regionParams.maintenanceInvestment = newMaintenance;
                    parametersChanged = true;
                }
                EditorGUILayout.EndHorizontal();
                
                // Development Investment
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Development Investment:", "Investment in infrastructure development"), GUILayout.Width(150));
                float newDevelopment = EditorGUILayout.Slider(regionParams.developmentInvestment, 0f, 100f);
                if (newDevelopment != regionParams.developmentInvestment)
                {
                    regionParams.developmentInvestment = newDevelopment;
                    parametersChanged = true;
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.HelpBox(
                    "These values control the test region for the simulation. Adjust them to see how different region parameters affect economic outcomes.", 
                    MessageType.Info);
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
        }
        
        /// <summary>
        /// Sync region data from the economic system
        /// </summary>
        public void SyncFromSystem(EconomicSystem economicSystem)
        {
            if (economicSystem == null) return;
            
            // Check if we have regions
            var regions = economicSystem.GetAllRegionIds();
            if (!regions.Any()) return; // Changed Count to Any()
            
            // Get the first region to sync
            var region = economicSystem.GetRegion(regions.First()); // Changed indexer to First()
            if (region == null) return;
            
            // Sync values
            regionParams.laborAvailable = region.Population;
            regionParams.infrastructureLevel = Mathf.RoundToInt(region.InfrastructureQuality * 10);
            // Other values don't typically sync from the system
            parametersChanged = false;
        }
        
        /// <summary>
        /// Apply region test data to the economic system
        /// </summary>
        public void ApplyToSystem(EconomicSystem economicSystem)
        {
            if (economicSystem == null) return;
            
            // Check if we have regions
            var regions = economicSystem.GetAllRegionIds();
            if (!regions.Any()) return; // Changed Count to Any()
            
            // Get the first region to apply changes to
            var region = economicSystem.GetRegion(regions.First()); // Changed indexer to First()
            if (region == null) return;
            
            // Apply values
            // region.Population = regionParams.laborAvailable;
            // region.InfrastructureQuality = regionParams.infrastructureLevel / 10f;
            
            // // Apply investments
            // economicSystem.ApplyInfrastructureInvestment(
            //     regions[0], 
            //     regionParams.maintenanceInvestment, 
            //     regionParams.developmentInvestment
            // );
            parametersChanged = false;
        }
        
        /// <summary>
        /// Reset the module data
        /// </summary>
        public void Reset()
        {
            regionParams = new RegionParameters();
            parametersChanged = false;
        }
        
        /// <summary>
        /// Handle editor update
        /// </summary>
        public void OnEditorUpdate(float deltaTime)
        {
            // No updates needed for region controls
        }
        
        /// <summary>
        /// Get current region parameters
        /// </summary>
        public RegionParameters GetRegionParameters()
        {
            return regionParams;
        }
    }
}