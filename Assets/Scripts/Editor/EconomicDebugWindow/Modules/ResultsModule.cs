using UnityEngine;
using UnityEditor;
using Systems;
using System.Linq; // Add LINQ namespace for IEnumerable extension methods

namespace Editor.DebugWindow.Modules
{
    /// <summary>
    /// Module for displaying current economic simulation results
    /// </summary>
    public class ResultsModule : IEconomicDebugModule
    {
        // Results data
        private bool showResults = true;
        private int currentWealth = 0;
        private int currentProduction = 0;
        private int currentPopulation = 0;
        private float currentInfrastructure = 0;
        private float currentEfficiency = 0;
        private float currentUnmetDemand = 0;
        private int currentTurn = 1;
        
        // Test calculation display
        private bool showTestCalculations = true;
        
        /// <summary>
        /// Draw the results UI
        /// </summary>
        public void Draw()
        {
            showResults = EditorGUILayout.Foldout(showResults, "Current Results", true);
            
            if (showResults)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.LabelField($"Turn: {currentTurn}", EditorStyles.boldLabel);
                EditorGUILayout.Space(5);
                
                // Draw main results with colors and layout
                DrawResultItem("Wealth:", currentWealth.ToString(), Color.green);
                DrawResultItem("Production:", currentProduction.ToString(), Color.blue);
                DrawResultItem("Population:", currentPopulation.ToString(), Color.cyan);
                
                EditorGUILayout.Space(5);
                
                // Draw secondary metrics
                DrawResultItem("Infrastructure Quality:", (currentInfrastructure * 100).ToString("F1") + "%", Color.yellow);
                DrawResultItem("Efficiency:", (currentEfficiency * 100).ToString("F1") + "%", new Color(0.2f, 0.8f, 0.8f));
                DrawResultItem("Unmet Demand:", (currentUnmetDemand * 100).ToString("F1") + "%", new Color(0.8f, 0.4f, 0.2f));
                
                EditorGUILayout.EndVertical();
            }
            
            // Test calculation display
            showTestCalculations = EditorGUILayout.Foldout(showTestCalculations, "Test Calculations", true);
            
            if (showTestCalculations)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.LabelField("Preview calculations based on current parameter values:", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "The values below show how the economic calculations would work with the current parameters. " +
                    "They are calculated directly using the formulas, independent of the actual simulation.", 
                    MessageType.Info);
                
                // Production calculation test
                EditorGUILayout.LabelField("Production Formula:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("P = Productivity × Labor^LaborElasticity × Capital^CapitalElasticity");
                
                // Example values
                int exampleLabor = 100;
                int exampleCapital = 5;
                
                float productionResult = CalculateProduction(1.0f, 0.5f, 0.5f, exampleLabor, exampleCapital);
                
                EditorGUILayout.LabelField($"With Labor={exampleLabor}, Capital={exampleCapital}:");
                EditorGUILayout.LabelField($"Production = {productionResult:F1}");
                
                EditorGUILayout.Space(10);
                
                // Infrastructure efficiency calculation test
                EditorGUILayout.LabelField("Infrastructure Efficiency Formula:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("E = 1.0 + (Quality × EfficiencyModifier)");
                
                float quality = 0.5f;
                float efficiencyResult = CalculateEfficiency(0.1f, quality);
                
                EditorGUILayout.LabelField($"With Quality={quality:F1}:");
                EditorGUILayout.LabelField($"Efficiency = {efficiencyResult:F2} ({(efficiencyResult - 1) * 100:F0}% bonus)");
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Space(5);
        }
        
        /// <summary>
        /// Draw a result item with a colored value
        /// </summary>
        private void DrawResultItem(string label, string value, Color valueColor)
        {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField(label, GUILayout.Width(150));
            
            GUIStyle valueStyle = new GUIStyle(EditorStyles.boldLabel);
            valueStyle.normal.textColor = valueColor;
            
            EditorGUILayout.LabelField(value, valueStyle);
            
            EditorGUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// Calculate production for test calculations
        /// </summary>
        private float CalculateProduction(float productivity, float laborElasticity, float capitalElasticity,
                                         int labor, float capital)
        {
            // Cobb-Douglas production function
            return productivity * Mathf.Pow(labor, laborElasticity) * Mathf.Pow(capital, capitalElasticity);
        }
        
        /// <summary>
        /// Calculate efficiency for test calculations
        /// </summary>
        private float CalculateEfficiency(float efficiencyModifier, float quality)
        {
            return 1.0f + (quality * efficiencyModifier);
        }
        
        /// <summary>
        /// Update the displayed results
        /// </summary>
        public void UpdateResults(int wealth, int production, int population, float infrastructure,
                                 float efficiency, float unmetDemand, int turn)
        {
            currentWealth = wealth;
            currentProduction = production;
            currentPopulation = population;
            currentInfrastructure = infrastructure;
            currentEfficiency = efficiency;
            currentUnmetDemand = unmetDemand;
            currentTurn = turn;
        }
        
        /// <summary>
        /// Sync from the economic system
        /// </summary>
        public void SyncFromSystem(EconomicSystem economicSystem)
        {
            if (economicSystem == null) return;
            
            // Check if we have regions
            var regions = economicSystem.GetAllRegionIds();
            if (!regions.Any()) return; // Changed Count to Any()
            
            // Get the first region to display
            var region = economicSystem.GetRegion(regions.First()); // Changed indexer to First()
            if (region == null) return;
            
            // Update values
            currentWealth = region.Wealth;
            currentProduction = region.Production;
            currentPopulation = region.Population;
            currentInfrastructure = region.InfrastructureQuality;
            
            // Values not directly available from region
            currentEfficiency = 1.0f + (region.InfrastructureQuality * 0.1f);
            currentUnmetDemand = 0.2f; // Sample value
        }
        
        /// <summary>
        /// Apply to the economic system
        /// </summary>
        public void ApplyToSystem(EconomicSystem economicSystem)
        {
            // Results module doesn't apply changes back to the system
        }
        
        /// <summary>
        /// Reset the module
        /// </summary>
        public void Reset()
        {
            currentWealth = 0;
            currentProduction = 0;
            currentPopulation = 0;
            currentInfrastructure = 0;
            currentEfficiency = 0;
            currentUnmetDemand = 0;
            currentTurn = 1;
        }
        
        /// <summary>
        /// Handle editor update
        /// </summary>
        public void OnEditorUpdate(float deltaTime)
        {
            // No updates needed for results display
        }
    }
}