using UnityEngine;
using UnityEditor;
using Systems;
using Systems.Economics;
using Editor.DebugWindow.Data;
using System.Collections.Generic;

namespace Editor.DebugWindow.Modules
{
    /// <summary>
    /// Module for handling economic parameter editing in the debug window
    /// </summary>
    public class ParametersModule : IEconomicDebugModule
    {
        // Parameters data
        private EconomicParameters parameters = new EconomicParameters();
        
        // UI state
        private bool showProductionParams = true;
        private bool showInfrastructureParams = true;
        private bool showConsumptionParams = true;
        private bool showPriceParams = true;
        private bool showCycleParams = true;
        private bool showPriceResourceParams = false;
        private bool showParameterEffects = true;
        
        // Parameter change tracking
        private Dictionary<string, string> parameterChangeEffects = new Dictionary<string, string>();
        private bool showParameterChangeFeedback = false;
        private float parameterFeedbackDisplayTime = 0f;
        private const float PARAMETER_FEEDBACK_DURATION = 4.0f;
        
        // Track if parameters have changed since last apply/sync
        private bool parametersChanged = false;
        
        // Reference to calculators for previewing effects
        private ProductionCalculator productionCalculator;
        private InfrastructureCalculator infrastructureCalculator;
        private ConsumptionCalculator consumptionCalculator;
        private PriceCalculator priceCalculator;
        private EconomicCycleCalculator cycleCalculator;
        
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

        /// <summary>
        /// Draw the parameters section UI
        /// </summary>
        public void Draw()
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
            
            DrawProductionParameters();
            DrawInfrastructureParameters();
            DrawConsumptionParameters();
            DrawPriceParameters();
            DrawCycleParameters();
            
            EditorGUILayout.Space(5);
        }
        
        /// <summary>
        /// Draw production parameters
        /// </summary>
        private void DrawProductionParameters()
        {
            showProductionParams = EditorGUILayout.Foldout(showProductionParams, "Production Parameters", true);
            
            if (showProductionParams)
            {
                EditorGUI.indentLevel++;
                
                // Productivity Factor
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Productivity Factor:", parameterDescriptions["productivityFactor"]), GUILayout.Width(150));
                
                // Store previous value for comparison
                float oldProductivityFactor = parameters.production.productivityFactor;
                
                float newProductivityFactor = EditorGUILayout.Slider(parameters.production.productivityFactor, 0.1f, 5.0f);
                if (newProductivityFactor != parameters.production.productivityFactor)
                {
                    parameters.production.productivityFactor = newProductivityFactor;
                    RecordParameterChange("productivityFactor", oldProductivityFactor, parameters.production.productivityFactor);
                }
                EditorGUILayout.EndHorizontal();
                
                // Labor Elasticity
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Labor Elasticity:", parameterDescriptions["laborElasticity"]), GUILayout.Width(150));
                
                float oldLaborElasticity = parameters.production.laborElasticity;
                
                float newLaborElasticity = EditorGUILayout.Slider(parameters.production.laborElasticity, 0.1f, 1.0f);
                if (newLaborElasticity != parameters.production.laborElasticity)
                {
                    parameters.production.laborElasticity = newLaborElasticity;
                    RecordParameterChange("laborElasticity", oldLaborElasticity, parameters.production.laborElasticity);
                }
                EditorGUILayout.EndHorizontal();
                
                // Capital Elasticity
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Capital Elasticity:", parameterDescriptions["capitalElasticity"]), GUILayout.Width(150));
                
                float oldCapitalElasticity = parameters.production.capitalElasticity;
                
                float newCapitalElasticity = EditorGUILayout.Slider(parameters.production.capitalElasticity, 0.1f, 1.0f);
                if (newCapitalElasticity != parameters.production.capitalElasticity)
                {
                    parameters.production.capitalElasticity = newCapitalElasticity;
                    RecordParameterChange("capitalElasticity", oldCapitalElasticity, parameters.production.capitalElasticity);
                }
                EditorGUILayout.EndHorizontal();
                
                // Display elasticity sum warning if not close to 1.0
                float elasticitySum = parameters.production.laborElasticity + parameters.production.capitalElasticity;
                if (Mathf.Abs(elasticitySum - 1.0f) > 0.1f)
                {
                    EditorGUILayout.HelpBox(
                        $"Warning: Labor + Capital elasticity = {elasticitySum:F2}. For optimal Cobb-Douglas function, this sum should be close to 1.0.", 
                        elasticitySum > 1.1f ? MessageType.Warning : MessageType.Info);
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// <summary>
        /// Draw infrastructure parameters
        /// </summary>
        private void DrawInfrastructureParameters()
        {
            showInfrastructureParams = EditorGUILayout.Foldout(showInfrastructureParams, "Infrastructure Parameters", true);
            
            if (showInfrastructureParams)
            {
                EditorGUI.indentLevel++;
                
                // Efficiency Modifier
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Efficiency Modifier:", parameterDescriptions["efficiencyModifier"]), GUILayout.Width(150));
                
                float oldEfficiencyModifier = parameters.infrastructure.efficiencyModifier;
                
                float newEfficiencyModifier = EditorGUILayout.Slider(parameters.infrastructure.efficiencyModifier, 0.01f, 0.5f);
                if (newEfficiencyModifier != parameters.infrastructure.efficiencyModifier)
                {
                    parameters.infrastructure.efficiencyModifier = newEfficiencyModifier;
                    RecordParameterChange("efficiencyModifier", oldEfficiencyModifier, parameters.infrastructure.efficiencyModifier);
                }
                EditorGUILayout.EndHorizontal();
                
                // Decay Rate
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Decay Rate:", parameterDescriptions["decayRate"]), GUILayout.Width(150));
                
                float oldDecayRate = parameters.infrastructure.decayRate;
                
                float newDecayRate = EditorGUILayout.Slider(parameters.infrastructure.decayRate, 0.01f, 0.1f);
                if (newDecayRate != parameters.infrastructure.decayRate)
                {
                    parameters.infrastructure.decayRate = newDecayRate;
                    RecordParameterChange("decayRate", oldDecayRate, parameters.infrastructure.decayRate);
                }
                EditorGUILayout.EndHorizontal();
                
                // Maintenance Cost Factor
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Maintenance Cost Factor:", parameterDescriptions["maintenanceCostFactor"]), GUILayout.Width(150));
                
                float oldMaintenanceCostFactor = parameters.infrastructure.maintenanceCostFactor;
                
                float newMaintenanceCostFactor = EditorGUILayout.Slider(parameters.infrastructure.maintenanceCostFactor, 0.01f, 0.1f);
                if (newMaintenanceCostFactor != parameters.infrastructure.maintenanceCostFactor)
                {
                    parameters.infrastructure.maintenanceCostFactor = newMaintenanceCostFactor;
                    RecordParameterChange("maintenanceCostFactor", oldMaintenanceCostFactor, parameters.infrastructure.maintenanceCostFactor);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// <summary>
        /// Draw consumption parameters
        /// </summary>
        private void DrawConsumptionParameters()
        {
            showConsumptionParams = EditorGUILayout.Foldout(showConsumptionParams, "Consumption Parameters", true);
            
            if (showConsumptionParams)
            {
                EditorGUI.indentLevel++;
                
                // Base Consumption Rate
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Base Consumption Rate:", parameterDescriptions["baseConsumptionRate"]), GUILayout.Width(150));
                
                float oldBaseConsumptionRate = parameters.consumption.baseConsumptionRate;
                
                float newBaseConsumptionRate = EditorGUILayout.Slider(parameters.consumption.baseConsumptionRate, 0.1f, 1.0f);
                if (newBaseConsumptionRate != parameters.consumption.baseConsumptionRate)
                {
                    parameters.consumption.baseConsumptionRate = newBaseConsumptionRate;
                    RecordParameterChange("baseConsumptionRate", oldBaseConsumptionRate, parameters.consumption.baseConsumptionRate);
                }
                EditorGUILayout.EndHorizontal();
                
                // Wealth Consumption Exponent
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Wealth Consumption Exponent:", parameterDescriptions["wealthConsumptionExponent"]), GUILayout.Width(150));
                
                float oldWealthConsumptionExponent = parameters.consumption.wealthConsumptionExponent;
                
                float newWealthConsumptionExponent = EditorGUILayout.Slider(parameters.consumption.wealthConsumptionExponent, 0.1f, 1.0f);
                if (newWealthConsumptionExponent != parameters.consumption.wealthConsumptionExponent)
                {
                    parameters.consumption.wealthConsumptionExponent = newWealthConsumptionExponent;
                    RecordParameterChange("wealthConsumptionExponent", oldWealthConsumptionExponent, parameters.consumption.wealthConsumptionExponent);
                }
                EditorGUILayout.EndHorizontal();
                
                // Unmet Demand Unrest Factor
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Unmet Demand Unrest Factor:", parameterDescriptions["unmetDemandUnrestFactor"]), GUILayout.Width(150));
                
                float oldUnmetDemandUnrestFactor = parameters.consumption.unmetDemandUnrestFactor;
                
                float newUnmetDemandUnrestFactor = EditorGUILayout.Slider(parameters.consumption.unmetDemandUnrestFactor, 0.01f, 0.1f);
                if (newUnmetDemandUnrestFactor != parameters.consumption.unmetDemandUnrestFactor)
                {
                    parameters.consumption.unmetDemandUnrestFactor = newUnmetDemandUnrestFactor;
                    RecordParameterChange("unmetDemandUnrestFactor", oldUnmetDemandUnrestFactor, parameters.consumption.unmetDemandUnrestFactor);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// <summary>
        /// Draw price parameters
        /// </summary>
        private void DrawPriceParameters()
        {
            showPriceParams = EditorGUILayout.Foldout(showPriceParams, "Price Parameters", true);
            
            if (showPriceParams)
            {
                EditorGUI.indentLevel++;
                
                // Volatility Factor
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Volatility Factor:", parameterDescriptions["volatilityFactor"]), GUILayout.Width(150));
                
                float oldVolatilityFactor = parameters.price.volatilityFactor;
                
                float newVolatilityFactor = EditorGUILayout.Slider(parameters.price.volatilityFactor, 0.1f, 1.0f);
                if (newVolatilityFactor != parameters.price.volatilityFactor)
                {
                    parameters.price.volatilityFactor = newVolatilityFactor;
                    RecordParameterChange("volatilityFactor", oldVolatilityFactor, parameters.price.volatilityFactor);
                }
                EditorGUILayout.EndHorizontal();
                
                // Resource elasticities section
                showPriceResourceParams = EditorGUILayout.Foldout(showPriceResourceParams, "Resource Elasticities", true);
                if (showPriceResourceParams)
                {
                    EditorGUI.indentLevel++;
                    
                    List<string> resourceKeys = new List<string>(parameters.price.resourceElasticities.Keys);
                    foreach (var resource in resourceKeys)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(new GUIContent($"{resource} Elasticity:", $"Elasticity for {resource}\n\nEFFECT: Higher values make {resource} prices more sensitive to supply/demand changes. Watch in Resource Prices graph."), GUILayout.Width(150));
                        
                        float oldElasticity = parameters.price.resourceElasticities[resource];
                        
                        float newElasticity = EditorGUILayout.Slider(parameters.price.resourceElasticities[resource], 0.1f, 2.0f);
                        if (newElasticity != parameters.price.resourceElasticities[resource])
                        {
                            parameters.price.resourceElasticities[resource] = newElasticity;
                            if (priceCalculator != null)
                            {
                                priceCalculator.SetResourceElasticity(resource, newElasticity);
                            }
                            
                            RecordParameterChange($"{resource}Elasticity", oldElasticity, newElasticity);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    // Add new resource elasticity
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Add Resource:", GUILayout.Width(150));
                    if (GUILayout.Button("Add", GUILayout.Width(60)))
                    {
                        // Show a popup to enter resource name - implementation skipped for brevity
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUI.indentLevel--;
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// <summary>
        /// Draw economic cycle parameters
        /// </summary>
        private void DrawCycleParameters()
        {
            showCycleParams = EditorGUILayout.Foldout(showCycleParams, "Economic Cycle Parameters", true);
            
            if (showCycleParams)
            {
                EditorGUI.indentLevel++;
                
                // Cycle Length
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Cycle Length:", parameterDescriptions["cycleLength"]), GUILayout.Width(150));
                
                int oldCycleLength = parameters.economicCycle.cycleLength;
                
                int newCycleLength = EditorGUILayout.IntSlider(parameters.economicCycle.cycleLength, 6, 24);
                if (newCycleLength != parameters.economicCycle.cycleLength)
                {
                    parameters.economicCycle.cycleLength = newCycleLength;
                    if (cycleCalculator != null)
                    {
                        // Recreate the calculator with new cycle length
                        cycleCalculator = new EconomicCycleCalculator(newCycleLength);
                    }
                    
                    RecordParameterChange("cycleLength", oldCycleLength, parameters.economicCycle.cycleLength);
                }
                EditorGUILayout.EndHorizontal();
                
                // Enable Economic Cycles
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Enable Economic Cycles:", parameterDescriptions["enableEconomicCycles"]), GUILayout.Width(150));
                
                bool oldEnableEconomicCycles = parameters.economicCycle.enableEconomicCycles;
                
                bool newEnableEconomicCycles = EditorGUILayout.Toggle(parameters.economicCycle.enableEconomicCycles);
                if (newEnableEconomicCycles != parameters.economicCycle.enableEconomicCycles)
                {
                    parameters.economicCycle.enableEconomicCycles = newEnableEconomicCycles;
                    
                    if (parameters.economicCycle.enableEconomicCycles != oldEnableEconomicCycles)
                    {
                        parameterChangeEffects["enableEconomicCycles"] = parameters.economicCycle.enableEconomicCycles 
                            ? "Economic cycles enabled: Economy will now fluctuate through boom and bust phases over time. Watch the 'Economic Cycle Effects' graph."
                            : "Economic cycles disabled: Economy will now grow more consistently without cyclic fluctuations.";
                        showParameterChangeFeedback = true;
                        parameterFeedbackDisplayTime = PARAMETER_FEEDBACK_DURATION;
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                // Display current phase information if we have an active cycle calculator
                if (cycleCalculator != null && parameters.economicCycle.enableEconomicCycles)
                {
                    // Current Phase Display
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Current Phase:", "Current economic cycle phase"), GUILayout.Width(150));
                    GUIStyle phaseStyle = new GUIStyle(EditorStyles.boldLabel);
                    var currentPhase = cycleCalculator.GetCurrentPhase();
                    switch (currentPhase)
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
                    EditorGUILayout.LabelField(currentPhase.ToString(), phaseStyle);
                    EditorGUILayout.EndHorizontal();
                    
                    // Phase Progress
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Phase Progress:", "Progress through current phase"), GUILayout.Width(150));
                    float phaseProgress = cycleCalculator.GetPhaseProgress();
                    EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(false, 18f), phaseProgress, $"{Mathf.Round(phaseProgress * 100)}%");
                    EditorGUILayout.EndHorizontal();
                }
                
                // Cycle Effect Modifiers
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Cycle Effect Modifiers", EditorStyles.boldLabel);
                
                List<string> effectKeys = new List<string>(parameters.economicCycle.cycleEffects.Keys);
                foreach (var effect in effectKeys)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent($"{effect} Effect:", $"Effect of {effect} during economic cycle\n\nEFFECT: Higher values create more pronounced effects during economic cycles. Impacts amplitude of cycle fluctuations."), GUILayout.Width(150));
                    
                    float oldEffect = parameters.economicCycle.cycleEffects[effect]; 
                    
                    float newEffect = EditorGUILayout.Slider(parameters.economicCycle.cycleEffects[effect], 0.5f, 1.5f);
                    if (newEffect != parameters.economicCycle.cycleEffects[effect])
                    {
                        parameters.economicCycle.cycleEffects[effect] = newEffect;
                        if (cycleCalculator != null)
                        {
                            // Update the effect for all phases
                            EconomicCycleCalculator.CyclePhase[] phases = 
                                (EconomicCycleCalculator.CyclePhase[])System.Enum.GetValues(typeof(EconomicCycleCalculator.CyclePhase));
                                
                            foreach (var phase in phases)
                            {
                                cycleCalculator.SetPhaseCoefficient(phase, effect, newEffect);
                            }
                        }
                        
                        RecordParameterChange($"{effect}Effect", oldEffect, newEffect);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// <summary>
        /// Record a parameter change and display feedback
        /// </summary>
        private void RecordParameterChange(string paramName, float oldValue, float newValue)
        {
            // Skip if the change is very small
            if (Mathf.Abs(oldValue - newValue) < 0.01f) return;
            
            // Set flag that parameters have changed
            parametersChanged = true;
            
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
                               $"EFFECT: Labor force changes will have {(newValue > oldValue ? "more" : "less")} impact on production. " +
                               $"Labor now has {newValue/(parameters.production.laborElasticity + parameters.production.capitalElasticity):P0} weight in production.";
                    break;
                case "capitalElasticity":
                    feedback = $"Capital Elasticity {direction} to {newValue:F2}\n" +
                               $"EFFECT: Infrastructure investment will have {(newValue > oldValue ? "more" : "less")} impact on production. " +
                               $"Capital now has {newValue/(parameters.production.laborElasticity + parameters.production.capitalElasticity):P0} weight in production.";
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

        /// <summary>
        /// Initialize calculator instances with current parameters
        /// </summary>
        private void InitializeCalculators()
        {
            productionCalculator = new ProductionCalculator(
                parameters.production.productivityFactor,
                parameters.production.laborElasticity,
                parameters.production.capitalElasticity);
                
            infrastructureCalculator = new InfrastructureCalculator(
                parameters.infrastructure.efficiencyModifier,
                parameters.infrastructure.decayRate,
                parameters.infrastructure.maintenanceCostFactor);
                
            consumptionCalculator = new ConsumptionCalculator(
                parameters.consumption.baseConsumptionRate,
                parameters.consumption.wealthConsumptionExponent,
                parameters.consumption.unmetDemandUnrestFactor);
                
            priceCalculator = new PriceCalculator();
            foreach (var entry in parameters.price.resourceElasticities)
            {
                priceCalculator.SetResourceElasticity(entry.Key, entry.Value);
            }
            
            cycleCalculator = new EconomicCycleCalculator(parameters.economicCycle.cycleLength);
            // Initialize cycle effects
            foreach (var effect in parameters.economicCycle.cycleEffects)
            {
                EconomicCycleCalculator.CyclePhase[] phases = 
                    (EconomicCycleCalculator.CyclePhase[])System.Enum.GetValues(typeof(EconomicCycleCalculator.CyclePhase));
                    
                foreach (var phase in phases)
                {
                    cycleCalculator.SetPhaseCoefficient(phase, effect.Key, effect.Value);
                }
            }
        }
        
        /// <summary>
        /// Sync parameters from the economic system
        /// </summary>
        public virtual void SyncFromSystem(EconomicSystem economicSystem)
        {
            if (economicSystem == null) return;
            
            // Sync Production Calculator parameters
            parameters.production.productivityFactor = economicSystem.productivityFactor;
            parameters.production.laborElasticity = economicSystem.laborElasticity;
            parameters.production.capitalElasticity = economicSystem.capitalElasticity;
            
            // Sync Infrastructure Calculator parameters
            parameters.infrastructure.efficiencyModifier = economicSystem.efficiencyModifier;
            parameters.infrastructure.decayRate = economicSystem.decayRate;
            parameters.infrastructure.maintenanceCostFactor = economicSystem.maintenanceCostFactor;
            
            // Sync Consumption Calculator parameters
            parameters.consumption.baseConsumptionRate = economicSystem.baseConsumptionRate;
            parameters.consumption.wealthConsumptionExponent = economicSystem.wealthConsumptionExponent;
            parameters.consumption.unmetDemandUnrestFactor = economicSystem.unmetDemandUnrestFactor;
            
            // Sync Economic Cycle parameters
            parameters.economicCycle.cycleLength = economicSystem.cycleLength;
            parameters.economicCycle.enableEconomicCycles = economicSystem.enableEconomicCycles;
            
            // Update calculator instances with new parameters
            InitializeCalculators();
        }
        
        /// <summary>
        /// Apply parameters to the economic system
        /// </summary>
        public virtual void ApplyToSystem(EconomicSystem economicSystem)
        {
            if (economicSystem == null) return;
            
            // Apply Production Calculator parameters
            economicSystem.productivityFactor = parameters.production.productivityFactor;
            economicSystem.laborElasticity = parameters.production.laborElasticity;
            economicSystem.capitalElasticity = parameters.production.capitalElasticity;
            
            // Apply Infrastructure Calculator parameters
            economicSystem.efficiencyModifier = parameters.infrastructure.efficiencyModifier;
            economicSystem.decayRate = parameters.infrastructure.decayRate;
            economicSystem.maintenanceCostFactor = parameters.infrastructure.maintenanceCostFactor;
            
            // Apply Consumption Calculator parameters
            economicSystem.baseConsumptionRate = parameters.consumption.baseConsumptionRate;
            economicSystem.wealthConsumptionExponent = parameters.consumption.wealthConsumptionExponent;
            economicSystem.unmetDemandUnrestFactor = parameters.consumption.unmetDemandUnrestFactor;
            
            // Apply Economic Cycle parameters
            economicSystem.cycleLength = parameters.economicCycle.cycleLength;
            economicSystem.enableEconomicCycles = parameters.economicCycle.enableEconomicCycles;
            
            // Reset the change flag after applying changes
            parametersChanged = false;
        }
        
        /// <summary>
        /// Handle per-frame updates
        /// </summary>
        public void OnEditorUpdate(float deltaTime)
        {
            // Handle parameter change feedback timer
            if (showParameterChangeFeedback)
            {
                parameterFeedbackDisplayTime -= deltaTime;
                if (parameterFeedbackDisplayTime <= 0)
                {
                    showParameterChangeFeedback = false;
                    parameterChangeEffects.Clear();
                }
            }
            
            // Update cycle calculator if applicable
            if (cycleCalculator != null && parameters.economicCycle.enableEconomicCycles)
            {
                // We don't advance the cycle here, that's done in the simulation tick
            }
        }
        
        /// <summary>
        /// Reset the module data
        /// </summary>
        public void Reset()
        {
            // Reset to default values if needed
            parameters = new EconomicParameters();
            
            // Reset UI state
            showParameterChangeFeedback = false;
            parameterChangeEffects.Clear();
            
            // Reinitialize calculators
            InitializeCalculators();
        }
        
        /// <summary>
        /// Get the current parameters
        /// </summary>
        public EconomicParameters GetParameters()
        {
            return parameters;
        }
        
        /// <summary>
        /// Get the current production calculator for preview calculations
        /// </summary>
        public ProductionCalculator GetProductionCalculator()
        {
            return productionCalculator;
        }
        
        /// <summary>
        /// Get the current infrastructure calculator for preview calculations
        /// </summary>
        public InfrastructureCalculator GetInfrastructureCalculator()
        {
            return infrastructureCalculator;
        }
        
        /// <summary>
        /// Get the current consumption calculator for preview calculations
        /// </summary>
        public ConsumptionCalculator GetConsumptionCalculator()
        {
            return consumptionCalculator;
        }
        
        /// <summary>
        /// Get the current price calculator for preview calculations
        /// </summary>
        public PriceCalculator GetPriceCalculator()
        {
            return priceCalculator;
        }
        
        /// <summary>
        /// Get the current cycle calculator for preview calculations
        /// </summary>
        public EconomicCycleCalculator GetCycleCalculator()
        {
            return cycleCalculator;
        }
    }
}