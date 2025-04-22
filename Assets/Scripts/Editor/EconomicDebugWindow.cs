using UnityEngine;
using UnityEditor;
using Systems;
using Entities;
using Core;
using Systems.Economics;
using System.Collections.Generic;
using System;

namespace Editor
{
    /// <summary>
    /// Editor window for debugging and tuning the modular economic simulation parameters
    /// </summary>
    public class EconomicDebugWindow : EditorWindow
    {
        // Constants
        private const int MAX_HISTORY_LENGTH = 100;

        // References to components
        private SerializedObject serializedEconomicSystem;
        private EconomicSystem economicSystem;
        
        // Calculator instances for direct testing
        private ProductionCalculator productionCalculator;
        private InfrastructureCalculator infrastructureCalculator;
        private ConsumptionCalculator consumptionCalculator;
        private PriceCalculator priceCalculator;
        private EconomicCycleCalculator cycleCalculator;
        
        // Simulation settings
        private bool autoRunEnabled = false;
        private float autoRunInterval = 1.0f;
        private double lastAutoRunTime;
        private bool simulationActive = false;
        
        // Production Calculator parameters
        private float productivityFactor = 1.0f;
        private float laborElasticity = 0.5f;
        private float capitalElasticity = 0.5f;
        
        // Infrastructure Calculator parameters
        private float efficiencyModifier = 0.1f;
        private float decayRate = 0.02f;
        private float maintenanceCostFactor = 0.05f;
        
        // Consumption Calculator parameters
        private float baseConsumptionRate = 0.2f;
        private float wealthConsumptionExponent = 0.8f;
        private float unmetDemandUnrestFactor = 0.05f;
        
        // Price Calculator parameters
        private Dictionary<string, float> resourceElasticities = new Dictionary<string, float>
        {
            { "Food", 0.5f },
            { "Luxury", 1.5f },
            { "RawMaterial", 0.8f },
            { "Manufacturing", 1.2f }
        };
        private float volatilityFactor = 0.2f;
        
        // Economic Cycle parameters
        private int cycleLength = 12;
        private bool enableEconomicCycles = true;
        private Dictionary<string, float> cycleEffects = new Dictionary<string, float>
        {
            { "Production", 1.0f },
            { "Consumption", 1.0f },
            { "Investment", 1.0f },
            { "PriceInflation", 1.0f },
            { "Unrest", 1.0f }
        };
        private EconomicCycleCalculator.CyclePhase currentCyclePhase = EconomicCycleCalculator.CyclePhase.Expansion;
        private float cyclePhaseProgress = 0f;
        
        // Region values
        private int laborAvailable = 100;
        private int infrastructureLevel = 5;
        private float maintenanceInvestment = 10;
        private float developmentInvestment = 20;
        
        // Graph data
        private List<int> wealthHistory = new List<int>();
        private List<int> productionHistory = new List<int>();
        private List<float> priceHistory = new List<float>();
        private List<float> efficiencyHistory = new List<float>();
        private List<float> unmetDemandHistory = new List<float>();
        private Vector2 graphScrollPosition = Vector2.zero;
        private float graphHeight = 200f;
        private string selectedGraph = "Wealth & Production";
        private string[] graphOptions = new string[] { 
            "Wealth & Production", 
            "Prices", 
            "Infrastructure & Efficiency",
            "Supply & Demand",
            "Economic Cycle Effects",
            "Resource Prices"
        };
        private List<float> cycleEffectHistory = new List<float>();
        private Dictionary<string, List<float>> resourcePriceHistory = new Dictionary<string, List<float>>();
        
        // Current turn tracking
        private int currentTurn = 1;
        
        // Foldout states
        private bool showProductionParams = true;
        private bool showInfrastructureParams = true;
        private bool showConsumptionParams = true;
        private bool showPriceParams = true;
        private bool showCycleParams = true;
        private bool showRegionControls = true;
        private bool showResults = true;
        private bool showGraphs = true;
        private bool showPriceResourceParams = false;
        private bool showTestCalculations = true;
        private bool showParameterEffects = true;
        
        // Parameter change tracking
        private Dictionary<string, float> previousValues = new Dictionary<string, float>();
        private Dictionary<string, string> parameterChangeEffects = new Dictionary<string, string>();
        private bool showParameterChangeFeedback = false;
        private float parameterFeedbackDisplayTime = 0f;
        private const float PARAMETER_FEEDBACK_DURATION = 4.0f;
        
        // Parameter descriptions and effects
        private Dictionary<string, string> parameterDescriptions = new Dictionary<string, string>()
        {
            // Production parameters
            { "productivityFactor", "Controls the overall productivity multiplier of the economy\n\nEFFECT: Higher values increase production output (blue line in Wealth & Production graph). Significant impact on economic growth." },
            { "laborElasticity", "How much labor affects production (Cobb-Douglas function)\n\nEFFECT: Higher values make changes in labor force more impactful on production. Balance with capitalElasticity (sum should be around 1.0)." },
            { "capitalElasticity", "How much capital/infrastructure affects production (Cobb-Douglas function)\n\nEFFECT: Higher values make infrastructure investments more effective. Balance with laborElasticity (sum should be around 1.0)." },
            
            // Infrastructure parameters
            { "efficiencyModifier", "Modifier for infrastructure efficiency\n\nEFFECT: Higher values increase the efficiency boost from infrastructure. Visible in the turquoise line in Infrastructure & Efficiency graph." },
            { "decayRate", "Rate at which infrastructure decays\n\nEFFECT: Higher values cause faster infrastructure deterioration if not maintained. Creates downward pressure on efficiency and production." },
            { "maintenanceCostFactor", "Factor for maintenance costs of infrastructure\n\nEFFECT: Higher values increase maintenance costs. Affects wealth indirectly as more wealth is needed for infrastructure upkeep." },
            
            // Consumption parameters
            { "baseConsumptionRate", "Base rate of consumption\n\nEFFECT: Higher values increase consumption across all wealth levels. Can lead to more unmet demand if production doesn't keep up." },
            { "wealthConsumptionExponent", "Exponent for wealth-based consumption\n\nEFFECT: Controls how consumption scales with wealth. Lower values create more equal consumption across wealth levels." },
            { "unmetDemandUnrestFactor", "Factor for unrest due to unmet demand\n\nEFFECT: Higher values create more unrest when demand is not met. Affects simulation stability." },
            
            // Price parameters
            { "volatilityFactor", "Factor for price volatility\n\nEFFECT: Higher values create more dramatic price fluctuations. Visible as sharper peaks and valleys in the Prices graph." },
            
            // Cycle parameters
            { "cycleLength", "Length of economic cycle in turns\n\nEFFECT: Longer cycles stretch out economic phases. Creates slower oscillations in the Economic Cycle Effects graph." },
            { "enableEconomicCycles", "Toggle economic cycles on/off\n\nEFFECT: When enabled, economic conditions fluctuate through expansion, peak, contraction, and trough phases." }
        };
        
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
                
                // Initialize calculator instances for direct testing
                InitializeCalculators();
            }
        }
        
        private void InitializeCalculators()
        {
            // Create calculator instances with current parameters
            productionCalculator = new ProductionCalculator(
                productivityFactor, 
                laborElasticity, 
                capitalElasticity);
                
            infrastructureCalculator = new InfrastructureCalculator(
                efficiencyModifier,
                decayRate,
                maintenanceCostFactor);
                
            consumptionCalculator = new ConsumptionCalculator(
                baseConsumptionRate,
                wealthConsumptionExponent,
                unmetDemandUnrestFactor);
                
            priceCalculator = new PriceCalculator();
            foreach (var entry in resourceElasticities)
            {
                priceCalculator.SetResourceElasticity(entry.Key, entry.Value);
            }
            
            cycleCalculator = new EconomicCycleCalculator(cycleLength);
            // Initialize cycle effects
            foreach (var effect in cycleEffects)
            {
                EconomicCycleCalculator.CyclePhase[] phases = 
                    (EconomicCycleCalculator.CyclePhase[])Enum.GetValues(typeof(EconomicCycleCalculator.CyclePhase));
                    
                foreach (var phase in phases)
                {
                    cycleCalculator.SetPhaseCoefficient(phase, effect.Key, effect.Value);
                }
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
            
            // Handle parameter change feedback timer
            if (showParameterChangeFeedback)
            {
                parameterFeedbackDisplayTime -= Time.deltaTime;
                if (parameterFeedbackDisplayTime <= 0)
                {
                    showParameterChangeFeedback = false;
                    parameterChangeEffects.Clear();
                }
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
            // Parameter effects help box
            showParameterEffects = EditorGUILayout.Foldout(showParameterEffects, "Parameter Effects Guide", true);
            if (showParameterEffects)
            {
                EditorGUILayout.HelpBox(
                    "Parameter Effect Guide:\n\n" +
                    "PRODUCTION PARAMETERS affect the Wealth & Production graph:\n" +
                    "• Productivity Factor: Higher = more production (blue line rises faster)\n" +
                    "• Labor/Capital Elasticity: Controls balance between labor/infrastructure impact\n\n" +
                    
                    "INFRASTRUCTURE PARAMETERS affect the Infrastructure & Efficiency graph:\n" +
                    "• Efficiency Modifier: Higher = more efficiency boost from infrastructure\n" +
                    "• Decay Rate: Higher = faster infrastructure deterioration\n" +
                    "• Maintenance Cost: Higher = more expense to maintain infrastructure\n\n" +
                    
                    "CONSUMPTION PARAMETERS affect the Supply & Demand graph:\n" +
                    "• Base Rate: Higher = more consumption across all wealth levels\n" +
                    "• Wealth Exponent: Higher = wealthy regions consume proportionally more\n" +
                    "• Unmet Demand Factor: Higher = more unrest from unfulfilled demand\n\n" +
                    
                    "PRICE PARAMETERS affect the Prices graph:\n" +
                    "• Volatility: Higher = more dramatic price fluctuations\n" +
                    "• Resource Elasticities: Higher = resource prices more sensitive to supply/demand\n\n" +
                    
                    "CYCLE PARAMETERS affect the Economic Cycle Effects graph:\n" +
                    "• Cycle Length: Longer = slower oscillations between phases\n" +
                    "• Effect Modifiers: Higher = more dramatic boom/bust cycles",
                    MessageType.Info);
            }
            
            // Show feedback from parameter changes
            if (showParameterChangeFeedback)
            {
                foreach (var effect in parameterChangeEffects)
                {
                    EditorGUILayout.HelpBox(effect.Value, MessageType.Warning);
                }
            }
            
            showProductionParams = EditorGUILayout.Foldout(showProductionParams, "Production Parameters", true);
            
            if (showProductionParams)
            {
                EditorGUI.indentLevel++;
                
                // Productivity Factor
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Productivity Factor:", parameterDescriptions["productivityFactor"]), GUILayout.Width(150));
                
                // Store previous value for comparison
                float oldProductivityFactor = productivityFactor;
                
                float newProductivityFactor = EditorGUILayout.Slider(productivityFactor, 0.1f, 5.0f);
                if (newProductivityFactor != productivityFactor)
                {
                    productivityFactor = newProductivityFactor;
                    ApplyToSystem();
                    RecordParameterChange("productivityFactor", oldProductivityFactor, productivityFactor);
                }
                EditorGUILayout.EndHorizontal();
                
                // Labor Elasticity
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Labor Elasticity:", parameterDescriptions["laborElasticity"]), GUILayout.Width(150));
                
                float oldLaborElasticity = laborElasticity;
                
                float newLaborElasticity = EditorGUILayout.Slider(laborElasticity, 0.1f, 1.0f);
                if (newLaborElasticity != laborElasticity)
                {
                    laborElasticity = newLaborElasticity;
                    ApplyToSystem();
                    RecordParameterChange("laborElasticity", oldLaborElasticity, laborElasticity);
                }
                EditorGUILayout.EndHorizontal();
                
                // Capital Elasticity
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Capital Elasticity:", parameterDescriptions["capitalElasticity"]), GUILayout.Width(150));
                
                float oldCapitalElasticity = capitalElasticity;
                
                float newCapitalElasticity = EditorGUILayout.Slider(capitalElasticity, 0.1f, 1.0f);
                if (newCapitalElasticity != capitalElasticity)
                {
                    capitalElasticity = newCapitalElasticity;
                    ApplyToSystem();
                    RecordParameterChange("capitalElasticity", oldCapitalElasticity, capitalElasticity);
                }
                EditorGUILayout.EndHorizontal();
                
                // Display elasticity sum warning if not close to 1.0
                float elasticitySum = laborElasticity + capitalElasticity;
                if (Mathf.Abs(elasticitySum - 1.0f) > 0.1f)
                {
                    EditorGUILayout.HelpBox(
                        $"Warning: Labor + Capital elasticity = {elasticitySum:F2}. For optimal Cobb-Douglas function, this sum should be close to 1.0.", 
                        elasticitySum > 1.1f ? MessageType.Warning : MessageType.Info);
                }
                
                EditorGUI.indentLevel--;
            }
            
            showInfrastructureParams = EditorGUILayout.Foldout(showInfrastructureParams, "Infrastructure Parameters", true);
            
            if (showInfrastructureParams)
            {
                EditorGUI.indentLevel++;
                
                // Efficiency Modifier
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Efficiency Modifier:", parameterDescriptions["efficiencyModifier"]), GUILayout.Width(150));
                
                float oldEfficiencyModifier = efficiencyModifier;
                
                float newEfficiencyModifier = EditorGUILayout.Slider(efficiencyModifier, 0.01f, 0.5f);
                if (newEfficiencyModifier != efficiencyModifier)
                {
                    efficiencyModifier = newEfficiencyModifier;
                    ApplyToSystem();
                    RecordParameterChange("efficiencyModifier", oldEfficiencyModifier, efficiencyModifier);
                }
                EditorGUILayout.EndHorizontal();
                
                // Decay Rate
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Decay Rate:", parameterDescriptions["decayRate"]), GUILayout.Width(150));
                
                float oldDecayRate = decayRate;
                
                float newDecayRate = EditorGUILayout.Slider(decayRate, 0.01f, 0.1f);
                if (newDecayRate != decayRate)
                {
                    decayRate = newDecayRate;
                    ApplyToSystem();
                    RecordParameterChange("decayRate", oldDecayRate, decayRate);
                }
                EditorGUILayout.EndHorizontal();
                
                // Maintenance Cost Factor
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Maintenance Cost Factor:", parameterDescriptions["maintenanceCostFactor"]), GUILayout.Width(150));
                
                float oldMaintenanceCostFactor = maintenanceCostFactor;
                
                float newMaintenanceCostFactor = EditorGUILayout.Slider(maintenanceCostFactor, 0.01f, 0.1f);
                if (newMaintenanceCostFactor != maintenanceCostFactor)
                {
                    maintenanceCostFactor = newMaintenanceCostFactor;
                    ApplyToSystem();
                    RecordParameterChange("maintenanceCostFactor", oldMaintenanceCostFactor, maintenanceCostFactor);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUI.indentLevel--;
            }
            
            showConsumptionParams = EditorGUILayout.Foldout(showConsumptionParams, "Consumption Parameters", true);
            
            if (showConsumptionParams)
            {
                EditorGUI.indentLevel++;
                
                // Base Consumption Rate
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Base Consumption Rate:", parameterDescriptions["baseConsumptionRate"]), GUILayout.Width(150));
                
                float oldBaseConsumptionRate = baseConsumptionRate;
                
                float newBaseConsumptionRate = EditorGUILayout.Slider(baseConsumptionRate, 0.1f, 1.0f);
                if (newBaseConsumptionRate != baseConsumptionRate)
                {
                    baseConsumptionRate = newBaseConsumptionRate;
                    ApplyToSystem();
                    RecordParameterChange("baseConsumptionRate", oldBaseConsumptionRate, baseConsumptionRate);
                }
                EditorGUILayout.EndHorizontal();
                
                // Wealth Consumption Exponent
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Wealth Consumption Exponent:", parameterDescriptions["wealthConsumptionExponent"]), GUILayout.Width(150));
                
                float oldWealthConsumptionExponent = wealthConsumptionExponent;
                
                float newWealthConsumptionExponent = EditorGUILayout.Slider(wealthConsumptionExponent, 0.1f, 1.0f);
                if (newWealthConsumptionExponent != wealthConsumptionExponent)
                {
                    wealthConsumptionExponent = newWealthConsumptionExponent;
                    ApplyToSystem();
                    RecordParameterChange("wealthConsumptionExponent", oldWealthConsumptionExponent, wealthConsumptionExponent);
                }
                EditorGUILayout.EndHorizontal();
                
                // Unmet Demand Unrest Factor
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Unmet Demand Unrest Factor:", parameterDescriptions["unmetDemandUnrestFactor"]), GUILayout.Width(150));
                
                float oldUnmetDemandUnrestFactor = unmetDemandUnrestFactor;
                
                float newUnmetDemandUnrestFactor = EditorGUILayout.Slider(unmetDemandUnrestFactor, 0.01f, 0.1f);
                if (newUnmetDemandUnrestFactor != unmetDemandUnrestFactor)
                {
                    unmetDemandUnrestFactor = newUnmetDemandUnrestFactor;
                    ApplyToSystem();
                    RecordParameterChange("unmetDemandUnrestFactor", oldUnmetDemandUnrestFactor, unmetDemandUnrestFactor);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUI.indentLevel--;
            }
            
            showPriceParams = EditorGUILayout.Foldout(showPriceParams, "Price Parameters", true);
            
            if (showPriceParams)
            {
                EditorGUI.indentLevel++;
                
                // Volatility Factor
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Volatility Factor:", parameterDescriptions["volatilityFactor"]), GUILayout.Width(150));
                
                float oldVolatilityFactor = volatilityFactor;
                
                float newVolatilityFactor = EditorGUILayout.Slider(volatilityFactor, 0.1f, 1.0f);
                if (newVolatilityFactor != volatilityFactor)
                {
                    volatilityFactor = newVolatilityFactor;
                    ApplyToSystem();
                    RecordParameterChange("volatilityFactor", oldVolatilityFactor, volatilityFactor);
                }
                EditorGUILayout.EndHorizontal();
                
                // Resource elasticities section
                showPriceResourceParams = EditorGUILayout.Foldout(showPriceResourceParams, "Resource Elasticities", true);
                if (showPriceResourceParams)
                {
                    EditorGUI.indentLevel++;
                    
                    List<string> resourceKeys = new List<string>(resourceElasticities.Keys);
                    foreach (var resource in resourceKeys)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(new GUIContent($"{resource} Elasticity:", $"Elasticity for {resource}\n\nEFFECT: Higher values make {resource} prices more sensitive to supply/demand changes. Watch in Resource Prices graph."), GUILayout.Width(150));
                        
                        float oldElasticity = resourceElasticities[resource];
                        
                        float newElasticity = EditorGUILayout.Slider(resourceElasticities[resource], 0.1f, 2.0f);
                        if (newElasticity != resourceElasticities[resource])
                        {
                            resourceElasticities[resource] = newElasticity;
                            if (priceCalculator != null)
                            {
                                priceCalculator.SetResourceElasticity(resource, newElasticity);
                            }
                            ApplyToSystem();
                            RecordParameterChange($"{resource}Elasticity", oldElasticity, newElasticity);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    // Add new resource elasticity
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Add Resource:", GUILayout.Width(150));
                    if (GUILayout.Button("Add", GUILayout.Width(60)))
                    {
                        // Show a popup to enter resource name
                        // AddResourcePopup.ShowWindow(this, resourceName => {
                        //     if (!string.IsNullOrEmpty(resourceName) && !resourceElasticities.ContainsKey(resourceName))
                        //     {
                        //         resourceElasticities.Add(resourceName, 1.0f);
                        //         if (priceCalculator != null)
                        //         {
                        //             priceCalculator.SetResourceElasticity(resourceName, 1.0f);
                        //         }
                        //         ApplyToSystem();
                        //         Repaint();
                        //     }
                        // });
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUI.indentLevel--;
                }
                
                EditorGUI.indentLevel--;
            }
            
            showCycleParams = EditorGUILayout.Foldout(showCycleParams, "Economic Cycle Parameters", true);
            
            if (showCycleParams)
            {
                EditorGUI.indentLevel++;
                
                // Cycle Length
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Cycle Length:", parameterDescriptions["cycleLength"]), GUILayout.Width(150));
                
                int oldCycleLength = cycleLength;
                
                int newCycleLength = EditorGUILayout.IntSlider(cycleLength, 6, 24);
                if (newCycleLength != cycleLength)
                {
                    cycleLength = newCycleLength;
                    if (cycleCalculator != null)
                    {
                        // Recreate the calculator with new cycle length
                        cycleCalculator = new EconomicCycleCalculator(cycleLength);
                    }
                    ApplyToSystem();
                    RecordParameterChange("cycleLength", oldCycleLength, cycleLength);
                }
                EditorGUILayout.EndHorizontal();
                
                // Enable Economic Cycles
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Enable Economic Cycles:", parameterDescriptions["enableEconomicCycles"]), GUILayout.Width(150));
                
                bool oldEnableEconomicCycles = enableEconomicCycles;
                
                bool newEnableEconomicCycles = EditorGUILayout.Toggle(enableEconomicCycles);
                if (newEnableEconomicCycles != enableEconomicCycles)
                {
                    enableEconomicCycles = newEnableEconomicCycles;
                    ApplyToSystem();
                    
                    if (enableEconomicCycles != oldEnableEconomicCycles)
                    {
                        parameterChangeEffects["enableEconomicCycles"] = enableEconomicCycles 
                            ? "Economic cycles enabled: Economy will now fluctuate through boom and bust phases over time. Watch the 'Economic Cycle Effects' graph."
                            : "Economic cycles disabled: Economy will now grow more consistently without cyclic fluctuations.";
                        showParameterChangeFeedback = true;
                        parameterFeedbackDisplayTime = PARAMETER_FEEDBACK_DURATION;
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                // Current Phase Display
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Current Phase:", "Current economic cycle phase"), GUILayout.Width(150));
                GUIStyle phaseStyle = new GUIStyle(EditorStyles.boldLabel);
                switch (currentCyclePhase)
                {
                    case EconomicCycleCalculator.CyclePhase.Expansion:
                        phaseStyle.normal.textColor = new Color(0.0f, 0.7f, 0.0f); // Green for growth
                        break;
                    case EconomicCycleCalculator.CyclePhase.Peak:
                        phaseStyle.normal.textColor = new Color(0.7f, 0.7f, 0.0f); // Yellow for peak
                        break;
                    case EconomicCycleCalculator.CyclePhase.Contraction:
                        phaseStyle.normal.textColor = new Color(0.7f, 0.4f, 0.0f); // Orange for contraction
                        break;
                    case EconomicCycleCalculator.CyclePhase.Trough:
                        phaseStyle.normal.textColor = new Color(0.7f, 0.0f, 0.0f); // Red for trough
                        break;
                }
                EditorGUILayout.LabelField(currentCyclePhase.ToString(), phaseStyle);
                EditorGUILayout.EndHorizontal();
                
                // Phase Progress
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Phase Progress:", "Progress through current phase"), GUILayout.Width(150));
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(false, 18f), cyclePhaseProgress, $"{Mathf.Round(cyclePhaseProgress * 100)}%");
                EditorGUILayout.EndHorizontal();
                
                // Cycle Effect Modifiers
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Cycle Effect Modifiers", EditorStyles.boldLabel);
                
                List<string> effectKeys = new List<string>(cycleEffects.Keys);
                foreach (var effect in effectKeys)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent($"{effect} Effect:", $"Effect of {effect} during economic cycle\n\nEFFECT: Higher values create more pronounced effects during economic cycles. Impacts amplitude of cycle fluctuations."), GUILayout.Width(150));
                    
                    float oldEffect = cycleEffects[effect]; 
                    
                    float newEffect = EditorGUILayout.Slider(cycleEffects[effect], 0.5f, 1.5f);
                    if (newEffect != cycleEffects[effect])
                    {
                        cycleEffects[effect] = newEffect;
                        if (cycleCalculator != null)
                        {
                            // Update the effect for all phases
                            EconomicCycleCalculator.CyclePhase[] phases = 
                                (EconomicCycleCalculator.CyclePhase[])Enum.GetValues(typeof(EconomicCycleCalculator.CyclePhase));
                                
                            foreach (var phase in phases)
                            {
                                cycleCalculator.SetPhaseCoefficient(phase, effect, newEffect);
                            }
                        }
                        ApplyToSystem();
                        RecordParameterChange($"{effect}Effect", oldEffect, newEffect);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
            
            // Add Test Calculations Section
            showTestCalculations = EditorGUILayout.Foldout(showTestCalculations, "Test Calculations", true);
            
            if (showTestCalculations)
            {
                EditorGUI.indentLevel++;
                
                // Production Test
                EditorGUILayout.LabelField("Production Calculator Test", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Input: Labor={laborAvailable}, Capital={infrastructureLevel}", GUILayout.Width(250));
                
                if (productionCalculator != null)
                {
                    float production = productionCalculator.CalculateOutput(laborAvailable, infrastructureLevel);
                    GUI.color = new Color(0.2f, 0.2f, 0.8f);
                    EditorGUILayout.LabelField($"Output: {production:F1}", EditorStyles.boldLabel);
                    GUI.color = Color.white;
                }
                EditorGUILayout.EndHorizontal();
                
                // Infrastructure Test
                EditorGUILayout.LabelField("Infrastructure Calculator Test", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Efficiency Boost (Level {infrastructureLevel}):", GUILayout.Width(250));
                
                if (infrastructureCalculator != null)
                {
                    float efficiency = infrastructureCalculator.CalculateEfficiencyBoost(infrastructureLevel);
                    GUI.color = new Color(0.2f, 0.8f, 0.8f);
                    EditorGUILayout.LabelField($"{efficiency:F2}x", EditorStyles.boldLabel);
                    GUI.color = Color.white;
                }
                EditorGUILayout.EndHorizontal();
                
                // Consumption Test
                EditorGUILayout.LabelField("Consumption Calculator Test", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                
                var regions = economicSystem?.GetAllRegionIds();
                int wealth = 100;
                if (regions != null && regions.Count > 0)
                {
                    var region = economicSystem.GetRegion(regions[0]);
                    if (region != null)
                    {
                        wealth = region.Wealth;
                    }
                }
                
                EditorGUILayout.LabelField($"Expected Consumption (Wealth={wealth}):", GUILayout.Width(250));
                
                if (consumptionCalculator != null)
                {
                    float consumption = consumptionCalculator.CalculateExpectedConsumption(wealth);
                    GUI.color = new Color(0.8f, 0.5f, 0.2f);
                    EditorGUILayout.LabelField($"{consumption:F1}", EditorStyles.boldLabel);
                    GUI.color = Color.white;
                }
                EditorGUILayout.EndHorizontal();
                
                // Price Test
                EditorGUILayout.LabelField("Price Calculator Test", EditorStyles.boldLabel);
                
                // Supply and demand sliders for price testing
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Test Supply:", GUILayout.Width(100));
                float testSupply = EditorGUILayout.Slider(100f, 10f, 200f);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Test Demand:", GUILayout.Width(100));
                float testDemand = EditorGUILayout.Slider(100f, 10f, 200f);
                EditorGUILayout.EndHorizontal();
                
                if (priceCalculator != null && resourceElasticities.Count > 0)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Price Results:");
                    EditorGUI.indentLevel++;
                    
                    foreach (var resource in resourceElasticities.Keys)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"{resource}:", GUILayout.Width(100));
                        float price = priceCalculator.CalculatePrice(100f, testSupply, testDemand, resource);
                        GUI.color = new Color(0.8f, 0.2f, 0.2f);
                        EditorGUILayout.LabelField($"{price:F1}", EditorStyles.boldLabel);
                        GUI.color = Color.white;
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    EditorGUI.indentLevel--;
                }
                
                // Economic Cycle Test
                EditorGUILayout.LabelField("Economic Cycle Calculator Test", EditorStyles.boldLabel);
                if (cycleCalculator != null)
                {
                    string phaseDescription = cycleCalculator.GetEconomicConditionDescription();
                    EditorGUILayout.HelpBox(phaseDescription, MessageType.Info);
                    
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Current Cycle Effects:");
                    EditorGUI.indentLevel++;
                    
                    foreach (var effect in cycleEffects.Keys)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"{effect}:", GUILayout.Width(100));
                        float effectValue = cycleCalculator.ApplyCycleEffect(100f, effect);
                        
                        // Color based on whether the effect is positive or negative
                        if (effectValue > 100f)
                        {
                            GUI.color = new Color(0.2f, 0.8f, 0.2f); // Green for positive
                        }
                        else if (effectValue < 100f)
                        {
                            GUI.color = new Color(0.8f, 0.2f, 0.2f); // Red for negative
                        }
                        
                        EditorGUILayout.LabelField($"{effectValue:F1} ({(effectValue - 100f):F1}%)", EditorStyles.boldLabel);
                        GUI.color = Color.white;
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    EditorGUI.indentLevel--;
                }
                
                EditorGUI.indentLevel--;
            }
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
                
                // Maintenance Investment
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Maintenance Investment:", "Investment in infrastructure maintenance"), GUILayout.Width(150));
                float newMaintenanceInvestment = EditorGUILayout.Slider(maintenanceInvestment, 0, 100);
                if (newMaintenanceInvestment != maintenanceInvestment)
                {
                    maintenanceInvestment = newMaintenanceInvestment;
                    ApplyToSystem();
                }
                EditorGUILayout.EndHorizontal();
                
                // Development Investment
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Development Investment:", "Investment in infrastructure development"), GUILayout.Width(150));
                float newDevelopmentInvestment = EditorGUILayout.Slider(developmentInvestment, 0, 100);
                if (newDevelopmentInvestment != developmentInvestment)
                {
                    developmentInvestment = newDevelopmentInvestment;
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
                        
                        // Add economic cycle information
                        if (enableEconomicCycles)
                        {
                            EditorGUILayout.Space(10);
                            EditorGUILayout.LabelField("Economic Cycle", EditorStyles.boldLabel);
                            
                            // Current phase
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("Current Phase:", GUILayout.Width(120));
                            GUIStyle cycleStyle = new GUIStyle(EditorStyles.boldLabel);
                            switch (currentCyclePhase)
                            {
                                case EconomicCycleCalculator.CyclePhase.Expansion:
                                    cycleStyle.normal.textColor = new Color(0.0f, 0.7f, 0.0f); // Green 
                                    break;
                                case EconomicCycleCalculator.CyclePhase.Peak:
                                    cycleStyle.normal.textColor = new Color(0.7f, 0.7f, 0.0f); // Yellow
                                    break;
                                case EconomicCycleCalculator.CyclePhase.Contraction:
                                    cycleStyle.normal.textColor = new Color(0.7f, 0.4f, 0.0f); // Orange
                                    break;
                                case EconomicCycleCalculator.CyclePhase.Trough:
                                    cycleStyle.normal.textColor = new Color(0.7f, 0.0f, 0.0f); // Red
                                    break;
                            }
                            GUILayout.Label(currentCyclePhase.ToString(), cycleStyle);
                            EditorGUILayout.EndHorizontal();
                            
                            // Display phase progress bar
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("Phase Progress:", GUILayout.Width(120));
                            Rect progressRect = EditorGUILayout.GetControlRect(false, 18f);
                            EditorGUI.ProgressBar(progressRect, cyclePhaseProgress, $"{Mathf.Round(cyclePhaseProgress * 100)}%");
                            EditorGUILayout.EndHorizontal();
                            
                            // If we have a cycle calculator, show its description
                            if (cycleCalculator != null)
                            {
                                EditorGUILayout.HelpBox(cycleCalculator.GetEconomicConditionDescription(), MessageType.Info);
                            }
                        }
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
                
                // Begin scroll view for all graphs
                graphScrollPosition = EditorGUILayout.BeginScrollView(graphScrollPosition, GUILayout.Height(graphHeight * 3 + 100));
                
                // Draw all graphs in sequence
                if (wealthHistory.Count > 1)
                {
                    // 1. Wealth & Production Graph
                    EditorGUILayout.LabelField("Wealth & Production", EditorStyles.boldLabel);
                    Rect wealthProductionRect = EditorGUILayout.GetControlRect(GUILayout.Height(graphHeight));
                    GUI.Box(wealthProductionRect, "");
                    DrawLineGraph(wealthHistory, wealthProductionRect, new Color(0.2f, 0.8f, 0.2f), "Wealth");
                    DrawLineGraph(productionHistory, wealthProductionRect, new Color(0.2f, 0.2f, 0.8f), "Production");
                    DrawLegend(wealthProductionRect, "Wealth & Production", new Dictionary<string, Color> {
                        { "Wealth", new Color(0.2f, 0.8f, 0.2f) },
                        { "Production", new Color(0.2f, 0.2f, 0.8f) }
                    });
                    EditorGUILayout.Space(10);
                    
                    // 2. Prices Graph
                    EditorGUILayout.LabelField("Prices", EditorStyles.boldLabel);
                    Rect pricesRect = EditorGUILayout.GetControlRect(GUILayout.Height(graphHeight));
                    GUI.Box(pricesRect, "");
                    DrawLineGraph(priceHistory, pricesRect, new Color(0.8f, 0.2f, 0.2f), "Prices");
                    DrawLegend(pricesRect, "Prices", new Dictionary<string, Color> {
                        { "Price Index", new Color(0.8f, 0.2f, 0.2f) }
                    });
                    EditorGUILayout.Space(10);
                    
                    // 3. Infrastructure & Efficiency Graph
                    EditorGUILayout.LabelField("Infrastructure & Efficiency", EditorStyles.boldLabel);
                    Rect efficiencyRect = EditorGUILayout.GetControlRect(GUILayout.Height(graphHeight));
                    GUI.Box(efficiencyRect, "");
                    DrawLineGraph(efficiencyHistory, efficiencyRect, new Color(0.2f, 0.8f, 0.8f), "Efficiency");
                    DrawLegend(efficiencyRect, "Infrastructure & Efficiency", new Dictionary<string, Color> {
                        { "Efficiency", new Color(0.2f, 0.8f, 0.8f) }
                    });
                    EditorGUILayout.Space(10);
                    
                    // 4. Supply & Demand Graph
                    EditorGUILayout.LabelField("Supply & Demand", EditorStyles.boldLabel);
                    Rect supplyDemandRect = EditorGUILayout.GetControlRect(GUILayout.Height(graphHeight));
                    GUI.Box(supplyDemandRect, "");
                    DrawLineGraph(unmetDemandHistory, supplyDemandRect, new Color(0.8f, 0.8f, 0.2f), "Unmet Demand");
                    DrawLegend(supplyDemandRect, "Supply & Demand", new Dictionary<string, Color> {
                        { "Unmet Demand", new Color(0.8f, 0.8f, 0.2f) }
                    });
                    EditorGUILayout.Space(10);
                    
                    // 5. Economic Cycle Effects Graph
                    EditorGUILayout.LabelField("Economic Cycle Effects", EditorStyles.boldLabel);
                    Rect cycleEffectsRect = EditorGUILayout.GetControlRect(GUILayout.Height(graphHeight));
                    GUI.Box(cycleEffectsRect, "");
                    DrawLineGraph(cycleEffectHistory, cycleEffectsRect, new Color(0.8f, 0.5f, 0.8f), "Cycle Effect");
                    DrawLegend(cycleEffectsRect, "Economic Cycle Effects", new Dictionary<string, Color> {
                        { "Cycle Factor", new Color(0.8f, 0.5f, 0.8f) }
                    });
                    EditorGUILayout.Space(10);
                    
                    // 6. Resource Prices Graph
                    EditorGUILayout.LabelField("Resource Prices", EditorStyles.boldLabel);
                    Rect resourcePricesRect = EditorGUILayout.GetControlRect(GUILayout.Height(graphHeight));
                    GUI.Box(resourcePricesRect, "");
                    
                    // Draw all resource price histories
                    Color[] resourceColors = new Color[] {
                        new Color(0.8f, 0.2f, 0.2f), // Red
                        new Color(0.2f, 0.8f, 0.2f), // Green
                        new Color(0.2f, 0.2f, 0.8f), // Blue
                        new Color(0.8f, 0.8f, 0.2f), // Yellow
                        new Color(0.8f, 0.2f, 0.8f), // Magenta
                        new Color(0.2f, 0.8f, 0.8f)  // Cyan
                    };
                    
                    Dictionary<string, Color> resourceLegendEntries = new Dictionary<string, Color>();
                    int colorIndex = 0;
                    foreach (var resource in resourcePriceHistory)
                    {
                        if (resource.Value.Count > 1)
                        {
                            Color resourceColor = resourceColors[colorIndex % resourceColors.Length];
                            DrawLineGraph(resource.Value, resourcePricesRect, resourceColor, resource.Key);
                            resourceLegendEntries[resource.Key] = resourceColor;
                            colorIndex++;
                        }
                    }
                    
                    DrawLegend(resourcePricesRect, "Resource Prices", resourceLegendEntries);
                }
                else
                {
                    // Show message if no data
                    string message = "No data available.\nRun simulation to record data.";
                    EditorGUILayout.HelpBox(message, MessageType.Info);
                }
                
                EditorGUILayout.EndScrollView();
                
                EditorGUILayout.HelpBox("All economic data is displayed in graphs above. Scroll to view all graphs.", MessageType.Info);
            }
            
            EditorGUILayout.Space(5);
        }
        
        private void DrawLegend(Rect graphRect, string title, Dictionary<string, Color> legendEntries)
        {
            float legendWidth = 120;
            float legendHeight = 30 + (legendEntries.Count * 20); // Height based on number of entries
            
            Rect legendRect = new Rect(
                graphRect.x + graphRect.width - legendWidth - 10,
                graphRect.y + 10,
                legendWidth,
                legendHeight
            );
            
            // Draw semi-transparent background
            EditorGUI.DrawRect(legendRect, new Color(0.1f, 0.1f, 0.1f, 0.7f));
            
            GUIStyle legendStyle = new GUIStyle(EditorStyles.label);
            legendStyle.normal.textColor = Color.white;
            
            // Draw title at the bottom
            GUI.Label(new Rect(legendRect.x + 5, legendRect.y + legendHeight - 20, legendWidth - 10, 20), 
                      title, EditorStyles.boldLabel);
            
            // Draw each legend entry
            int index = 0;
            foreach (var entry in legendEntries)
            {
                Rect colorRect = new Rect(legendRect.x + 10, legendRect.y + 10 + (index * 20), 10, 10);
                EditorGUI.DrawRect(colorRect, entry.Value);
                GUI.Label(new Rect(colorRect.x + 15, colorRect.y - 2, legendWidth - 35, 20), entry.Key, legendStyle);
                index++;
            }
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
        
        private void DrawLineGraph(List<float> data, Rect rect, Color color, string label)
        {
            if (data == null || data.Count < 2) return;
            
            // Find max value for scaling
            float maxValue = 10f; // Minimum to ensure visibility
            foreach (float value in data)
            {
                maxValue = Mathf.Max(maxValue, value);
            }
            
            // Add headroom to the top
            maxValue = Mathf.Ceil(maxValue * 1.1f);
            
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
                float value = Mathf.Round(maxValue * (1 - (i / 4.0f)));
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
                
                // If cycles are enabled, advance the cycle
                if (enableEconomicCycles && cycleCalculator != null)
                {
                    cycleCalculator.AdvanceCycle();
                    currentCyclePhase = cycleCalculator.GetCurrentPhase();
                    cyclePhaseProgress = cycleCalculator.GetPhaseProgress();
                    
                    // Add cycle effect to history
                    float cycleEffect = cycleCalculator.ApplyCycleEffect(100f, "Production");
                    if (cycleEffectHistory.Count > MAX_HISTORY_LENGTH)
                        cycleEffectHistory.RemoveAt(0);
                    const int maxHistoryLength = 100;
                    if (cycleEffectHistory.Count > maxHistoryLength)
                        cycleEffectHistory.RemoveAt(0);
                }
                
                // Manually record data from regions to ensure graphs update
                SyncRegionDataForGraphs();
                
                // Log data for debugging
                var regions = economicSystem.GetAllRegionIds();
                if (regions.Count > 0)
                {
                    var region = economicSystem.GetRegion(regions[0]);
                    if (region != null)
                    {
                        Debug.Log($"Turn {currentTurn}: Wealth={region.Wealth}, Production={region.Production}, Phase={currentCyclePhase}");
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
                
                // Reset cycle calculator
                if (cycleCalculator != null)
                {
                    cycleCalculator = new EconomicCycleCalculator(cycleLength);
                    foreach (var effect in cycleEffects)
                    {
                        EconomicCycleCalculator.CyclePhase[] phases = 
                            (EconomicCycleCalculator.CyclePhase[])Enum.GetValues(typeof(EconomicCycleCalculator.CyclePhase));
                            
                        foreach (var phase in phases)
                        {
                            cycleCalculator.SetPhaseCoefficient(phase, effect.Key, effect.Value);
                        }
                    }
                    
                    currentCyclePhase = cycleCalculator.GetCurrentPhase();
                    cyclePhaseProgress = cycleCalculator.GetPhaseProgress();
                }
                
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
                        priceHistory.Clear();
                        efficiencyHistory.Clear();
                        unmetDemandHistory.Clear();
                        cycleEffectHistory.Clear();
                        resourcePriceHistory.Clear();
                        
                        // Add the first data point to start the graph
                        wealthHistory.Add(region.Wealth);
                        productionHistory.Add(region.Production);
                        cycleEffectHistory.Add(100f); // Starting value is 100%
                        
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
            
            // Sync Production Calculator parameters
            productivityFactor = economicSystem.productivityFactor;
            laborElasticity = economicSystem.laborElasticity;
            capitalElasticity = economicSystem.capitalElasticity;
            
            // Sync Infrastructure Calculator parameters
            efficiencyModifier = economicSystem.efficiencyModifier;
            decayRate = economicSystem.decayRate;
            maintenanceCostFactor = economicSystem.maintenanceCostFactor;
            
            // Sync Consumption Calculator parameters
            baseConsumptionRate = economicSystem.baseConsumptionRate;
            wealthConsumptionExponent = economicSystem.wealthConsumptionExponent;
            unmetDemandUnrestFactor = economicSystem.unmetDemandUnrestFactor;
            
            // Sync Economic Cycle parameters
            cycleLength = economicSystem.cycleLength;
            enableEconomicCycles = economicSystem.enableEconomicCycles;
            
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
            
            // Update calculator instances with new parameters
            InitializeCalculators();
        }
        
        private void ApplyToSystem()
        {
            if (economicSystem == null) return;
            
            // Apply Production Calculator parameters
            economicSystem.productivityFactor = productivityFactor;
            economicSystem.laborElasticity = laborElasticity;
            economicSystem.capitalElasticity = capitalElasticity;
            
            // Apply Infrastructure Calculator parameters
            economicSystem.efficiencyModifier = efficiencyModifier;
            economicSystem.decayRate = decayRate;
            economicSystem.maintenanceCostFactor = maintenanceCostFactor;
            
            // Apply Consumption Calculator parameters
            economicSystem.baseConsumptionRate = baseConsumptionRate;
            economicSystem.wealthConsumptionExponent = wealthConsumptionExponent;
            economicSystem.unmetDemandUnrestFactor = unmetDemandUnrestFactor;
            
            // Apply Economic Cycle parameters
            economicSystem.cycleLength = cycleLength;
            economicSystem.enableEconomicCycles = enableEconomicCycles;
            
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
            
            // Apply resource elasticities
            // if (economicSystem.priceCalculator != null)
            // {
            //     foreach (var entry in resourceElasticities)
            //     {
            //         economicSystem.priceCalculator.SetResourceElasticity(entry.Key, entry.Value);
            //     }
            // }
            
            // Mark the object as dirty to ensure values persist
            if (serializedEconomicSystem != null)
            {
                serializedEconomicSystem.ApplyModifiedProperties();
                EditorUtility.SetDirty(economicSystem);
            }
            
            // Update calculator instances with new parameters
            InitializeCalculators();
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
                    
                    // Trim history if too long
                    if (wealthHistory.Count > MAX_HISTORY_LENGTH)
                    {
                        wealthHistory.RemoveAt(0);
                    }
                    if (productionHistory.Count > MAX_HISTORY_LENGTH)
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
                    
                    // Add other metrics
                    if (infrastructureCalculator != null)
                    {
                        float efficiency = infrastructureCalculator.CalculateEfficiencyBoost(infrastructureLevel);
                        efficiencyHistory.Add(efficiency);
                    }
                    
                    if (consumptionCalculator != null)
                    {
                        float expectedConsumption = consumptionCalculator.CalculateExpectedConsumption(region.Wealth);
                        float unmetDemand = 0.3f; // Simulated value - you'd calculate this from actual game state
                        unmetDemandHistory.Add(unmetDemand);
                    }
                    
                    // Add price data
                    float price = 100f; // Base price
                    if (priceCalculator != null)
                    {
                        price = priceCalculator.CalculatePrice(100f, region.Production, region.Wealth * 0.5f);
                    }
                    priceHistory.Add(price);
                    
                    // Add resource-specific prices
                    foreach (var resource in resourceElasticities.Keys)
                    {
                        if (!resourcePriceHistory.ContainsKey(resource))
                        {
                            resourcePriceHistory[resource] = new List<float>();
                        }
                        
                        float resourcePrice = 100f;
                        if (priceCalculator != null)
                        {
                            // Simulate different supply/demand for each resource
                            float supply = region.Production * (0.8f + UnityEngine.Random.value * 0.4f);
                            float demand = region.Wealth * 0.5f * (0.8f + UnityEngine.Random.value * 0.4f);
                            resourcePrice = priceCalculator.CalculatePrice(100f, supply, demand, resource);
                        }
                        
                        resourcePriceHistory[resource].Add(resourcePrice);
                        
                        if (resourcePriceHistory[resource].Count > MAX_HISTORY_LENGTH)
                            resourcePriceHistory[resource].RemoveAt(0);
                            resourcePriceHistory[resource].RemoveAt(0);
                    }
                    
                    // Trim other history lists if too long
                    if (wealthHistory.Count > MAX_HISTORY_LENGTH)
                        wealthHistory.RemoveAt(0);
                    if (productionHistory.Count > MAX_HISTORY_LENGTH)
                        productionHistory.RemoveAt(0);
                    if (efficiencyHistory.Count > MAX_HISTORY_LENGTH)
                        efficiencyHistory.RemoveAt(0);
                    if (unmetDemandHistory.Count > MAX_HISTORY_LENGTH)
                        unmetDemandHistory.RemoveAt(0);
                    if (priceHistory.Count > MAX_HISTORY_LENGTH)
                        priceHistory.RemoveAt(0);
                        
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
                priceHistory.Add(100f); // Base price as initial value
                efficiencyHistory.Add(1.0f); // Base efficiency
                unmetDemandHistory.Add(0f); // No unmet demand initially
                cycleEffectHistory.Add(100f); // Base cycle effect
                
                // Initialize resource price histories
                foreach (var resource in resourceElasticities.Keys)
                {
                    if (!resourcePriceHistory.ContainsKey(resource))
                    {
                        resourcePriceHistory[resource] = new List<float>();
                    }
                    resourcePriceHistory[resource].Add(100f);
                }
                
                Debug.Log($"Initialized graph data: Wealth={region.Wealth}, Production={region.Production}");
            }
        }
        
        private void RecordParameterChange(string paramName, float oldValue, float newValue)
        {
            // Skip if the change is very small
            if (Mathf.Abs(oldValue - newValue) < 0.01f) return;
            
            float percentChange = ((newValue - oldValue) / Mathf.Max(0.1f, Mathf.Abs(oldValue))) * 100f;
            string direction = newValue > oldValue ? "increased" : "decreased";
            
            string feedback = "";
            
            switch (paramName)
            {
                case "productivityFactor":
                    feedback = $"Productivity Factor {direction} by {Mathf.Abs(percentChange):F0}%\n" +
                               $"EFFECT: Production output should {direction} significantly. Watch the blue line in the Wealth & Production graph.";
                    break;
                case "laborElasticity":
                    feedback = $"Labor Elasticity {direction} to {newValue:F2}\n" +
                               $"EFFECT: Labor force changes will have {(newValue > oldValue ? "more" : "less")} impact on production. Labor now has {newValue/(laborElasticity+capitalElasticity):P0} weight in production.";
                    break;
                case "capitalElasticity":
                    feedback = $"Capital Elasticity {direction} to {newValue:F2}\n" +
                               $"EFFECT: Infrastructure investment will have {(newValue > oldValue ? "more" : "less")} impact on production. Capital now has {newValue/(laborElasticity+capitalElasticity):P0} weight in production.";
                    break;
                case "efficiencyModifier":
                    feedback = $"Efficiency Modifier {direction} by {Mathf.Abs(percentChange):F0}%\n" +
                               $"EFFECT: Infrastructure efficiency boost should {direction}. Watch the turquoise line in the Infrastructure & Efficiency graph.";
                    break;
                case "decayRate":
                    feedback = $"Decay Rate {direction} by {Mathf.Abs(percentChange):F0}%\n" +
                               $"EFFECT: Infrastructure will deteriorate {(newValue > oldValue ? "faster" : "slower")}. May require higher maintenance investment.";
                    break;
                case "baseConsumptionRate":
                    feedback = $"Base Consumption Rate {direction} by {Mathf.Abs(percentChange):F0}%\n" +
                               $"EFFECT: Overall consumption will {direction}. May lead to {(newValue > oldValue ? "higher" : "lower")} unmet demand if production doesn't adjust.";
                    break;
                case "wealthConsumptionExponent":
                    feedback = $"Wealth Consumption Exponent {direction} to {newValue:F2}\n" +
                               $"EFFECT: Consumption will scale {(newValue > oldValue ? "more" : "less")} steeply with wealth. Wealthy regions will consume {(newValue > oldValue ? "more" : "less")}.";
                    break;
                case "volatilityFactor":
                    feedback = $"Price Volatility {direction} by {Mathf.Abs(percentChange):F0}%\n" +
                               $"EFFECT: Price fluctuations should become {(newValue > oldValue ? "more dramatic" : "more stable")}. Watch for {(newValue > oldValue ? "sharper" : "smoother")} peaks and valleys in the Prices graph.";
                    break;
                case "cycleLength":
                    feedback = $"Economic Cycle Length {direction} to {newValue} turns\n" +
                               $"EFFECT: Economic phases will last {(newValue > oldValue ? "longer" : "shorter")}. Full cycle (boom to bust and back) will take about {newValue} turns.";
                    break;
                default:
                    // For resource elasticities and other parameters
                    if (paramName.Contains("Elasticity") && !paramName.Contains("labor") && !paramName.Contains("capital"))
                    {
                        string resource = paramName.Replace("Elasticity", "");
                        feedback = $"{resource} Price Elasticity {direction} to {newValue:F2}\n" +
                                   $"EFFECT: {resource} prices will be {(newValue > oldValue ? "more" : "less")} sensitive to supply/demand changes. Watch in the Resource Prices graph.";
                    }
                    else if (paramName.Contains("Effect"))
                    {
                        string effect = paramName.Replace("Effect", "");
                        feedback = $"{effect} Cycle Effect {direction} to {newValue:F2}\n" +
                                   $"EFFECT: {effect} will be {(newValue > oldValue ? "more" : "less")} strongly affected by economic cycles. Expect {(newValue > oldValue ? "larger" : "smaller")} fluctuations.";
                    }
                    break;
            }
            
            if (!string.IsNullOrEmpty(feedback))
            {
                parameterChangeEffects[paramName] = feedback;
                showParameterChangeFeedback = true;
                parameterFeedbackDisplayTime = PARAMETER_FEEDBACK_DURATION;
            }
        }
    }
}