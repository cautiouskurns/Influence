using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Editor.DebugWindow.Data;
using Editor.DebugWindow.Utilities;
using Systems.Economics;
using Systems;
using Entities;
using Core;

namespace Editor.DebugWindow.Modules
{
    /// <summary>
    /// Module for handling economic graph display in the debug window
    /// </summary>
    public class GraphModule : IEconomicDebugModule
    {
        // Graph data
        private GraphData graphData = new GraphData();
        
        // UI state
        private bool showGraphs = true;
        
        /// <summary>
        /// Draw the graphs section UI
        /// </summary>
        public void Draw()
        {
            showGraphs = EditorGUILayout.Foldout(showGraphs, "Economic Graphs", true);
            
            if (showGraphs)
            {
                // Graph height control
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Graph Height:", GUILayout.Width(100));
                graphData.graphHeight = EditorGUILayout.Slider(graphData.graphHeight, 100f, 400f);
                EditorGUILayout.EndHorizontal();
                
                // Begin scroll view for all graphs
                graphData.graphScrollPosition = EditorGUILayout.BeginScrollView(graphData.graphScrollPosition, GUILayout.Height(graphData.graphHeight * 3 + 100));
                
                // Draw all graphs in sequence
                if (graphData.wealthHistory.Count > 1)
                {
                    DrawWealthProductionGraph();
                    EditorGUILayout.Space(10);
                    
                    DrawPricesGraph();
                    EditorGUILayout.Space(10);
                    
                    DrawEfficiencyGraph();
                    EditorGUILayout.Space(10);
                    
                    DrawSupplyDemandGraph();
                    EditorGUILayout.Space(10);
                    
                    DrawCycleEffectsGraph();
                    EditorGUILayout.Space(10);
                    
                    DrawResourcePricesGraph();
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
        
        /// <summary>
        /// Draw the wealth and production graph
        /// </summary>
        private void DrawWealthProductionGraph()
        {
            EditorGUILayout.LabelField("Wealth & Production", EditorStyles.boldLabel);
            Rect wealthProductionRect = EditorGUILayout.GetControlRect(GUILayout.Height(graphData.graphHeight));
            GUI.Box(wealthProductionRect, "");
            
            GraphRenderer.DrawLineGraph(graphData.wealthHistory, wealthProductionRect, new Color(0.2f, 0.8f, 0.2f), "Wealth", graphData.currentTurn);
            GraphRenderer.DrawLineGraph(graphData.productionHistory, wealthProductionRect, new Color(0.2f, 0.2f, 0.8f), "Production", graphData.currentTurn);
            
            GraphRenderer.DrawLegend(wealthProductionRect, "Wealth & Production", new Dictionary<string, Color> {
                { "Wealth", new Color(0.2f, 0.8f, 0.2f) },
                { "Production", new Color(0.2f, 0.2f, 0.8f) }
            });
        }
        
        /// <summary>
        /// Draw the prices graph
        /// </summary>
        private void DrawPricesGraph()
        {
            EditorGUILayout.LabelField("Prices", EditorStyles.boldLabel);
            Rect pricesRect = EditorGUILayout.GetControlRect(GUILayout.Height(graphData.graphHeight));
            GUI.Box(pricesRect, "");
            
            GraphRenderer.DrawLineGraph(graphData.priceHistory, pricesRect, new Color(0.8f, 0.2f, 0.2f), "Prices", graphData.currentTurn);
            
            GraphRenderer.DrawLegend(pricesRect, "Prices", new Dictionary<string, Color> {
                { "Price Index", new Color(0.8f, 0.2f, 0.2f) }
            });
        }
        
        /// <summary>
        /// Draw the infrastructure efficiency graph
        /// </summary>
        private void DrawEfficiencyGraph()
        {
            EditorGUILayout.LabelField("Infrastructure & Efficiency", EditorStyles.boldLabel);
            Rect efficiencyRect = EditorGUILayout.GetControlRect(GUILayout.Height(graphData.graphHeight));
            GUI.Box(efficiencyRect, "");
            
            GraphRenderer.DrawLineGraph(graphData.efficiencyHistory, efficiencyRect, new Color(0.2f, 0.8f, 0.8f), "Efficiency", graphData.currentTurn);
            
            GraphRenderer.DrawLegend(efficiencyRect, "Infrastructure & Efficiency", new Dictionary<string, Color> {
                { "Efficiency", new Color(0.2f, 0.8f, 0.8f) }
            });
        }
        
        /// <summary>
        /// Draw the supply and demand graph
        /// </summary>
        private void DrawSupplyDemandGraph()
        {
            EditorGUILayout.LabelField("Supply & Demand", EditorStyles.boldLabel);
            Rect supplyDemandRect = EditorGUILayout.GetControlRect(GUILayout.Height(graphData.graphHeight));
            GUI.Box(supplyDemandRect, "");
            
            GraphRenderer.DrawLineGraph(graphData.unmetDemandHistory, supplyDemandRect, new Color(0.8f, 0.8f, 0.2f), "Unmet Demand", graphData.currentTurn);
            
            GraphRenderer.DrawLegend(supplyDemandRect, "Supply & Demand", new Dictionary<string, Color> {
                { "Unmet Demand", new Color(0.8f, 0.8f, 0.2f) }
            });
        }
        
        /// <summary>
        /// Draw the economic cycle effects graph
        /// </summary>
        private void DrawCycleEffectsGraph()
        {
            EditorGUILayout.LabelField("Economic Cycle Effects", EditorStyles.boldLabel);
            Rect cycleEffectsRect = EditorGUILayout.GetControlRect(GUILayout.Height(graphData.graphHeight));
            GUI.Box(cycleEffectsRect, "");
            
            GraphRenderer.DrawLineGraph(graphData.cycleEffectHistory, cycleEffectsRect, new Color(0.8f, 0.5f, 0.8f), "Cycle Effect", graphData.currentTurn);
            
            GraphRenderer.DrawLegend(cycleEffectsRect, "Economic Cycle Effects", new Dictionary<string, Color> {
                { "Cycle Factor", new Color(0.8f, 0.5f, 0.8f) }
            });
        }
        
        /// <summary>
        /// Draw the resource prices graph
        /// </summary>
        private void DrawResourcePricesGraph()
        {
            EditorGUILayout.LabelField("Resource Prices", EditorStyles.boldLabel);
            Rect resourcePricesRect = EditorGUILayout.GetControlRect(GUILayout.Height(graphData.graphHeight));
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
            foreach (var resource in graphData.resourcePriceHistory)
            {
                if (resource.Value.Count > 1)
                {
                    Color resourceColor = resourceColors[colorIndex % resourceColors.Length];
                    GraphRenderer.DrawLineGraph(resource.Value, resourcePricesRect, resourceColor, resource.Key, graphData.currentTurn);
                    resourceLegendEntries[resource.Key] = resourceColor;
                    colorIndex++;
                }
            }
            
            GraphRenderer.DrawLegend(resourcePricesRect, "Resource Prices", resourceLegendEntries);
        }
        
        /// <summary>
        /// Record economic data history for graphing
        /// </summary>
        public void RecordHistory(int wealth, int production, float price, float efficiency, float unmetDemand, float cycleEffect)
        {
            graphData.AddDataPoint(graphData.wealthHistory, wealth);
            graphData.AddDataPoint(graphData.productionHistory, production);
            graphData.AddDataPoint(graphData.priceHistory, price);
            graphData.AddDataPoint(graphData.efficiencyHistory, efficiency);
            graphData.AddDataPoint(graphData.unmetDemandHistory, unmetDemand);
            graphData.AddDataPoint(graphData.cycleEffectHistory, cycleEffect);
        }
        
        /// <summary>
        /// Record resource-specific price history
        /// </summary>
        public void RecordResourcePrice(string resource, float price)
        {
            if (!graphData.resourcePriceHistory.ContainsKey(resource))
            {
                graphData.resourcePriceHistory[resource] = new List<float>();
            }
            
            graphData.AddDataPoint(graphData.resourcePriceHistory[resource], price);
        }
        
        /// <summary>
        /// Increment the current turn counter
        /// </summary>
        public void IncrementTurn()
        {
            graphData.currentTurn++;
        }
        
        /// <summary>
        /// Get the current turn
        /// </summary>
        public int GetCurrentTurn()
        {
            return graphData.currentTurn;
        }
        
        /// <summary>
        /// Sync data from the economic system for graph display
        /// </summary>
        public void SyncFromSystem(EconomicSystem economicSystem)
        {
            if (economicSystem == null) return;
            
            // Check if we have regions
            var regions = economicSystem.GetAllRegionIds();
            if (regions.Count == 0) return;
            
            // Get the first region to track
            var region = economicSystem.GetRegion(regions[0]);
            if (region == null) return;
            
            // Check if we have history data already
            if (graphData.wealthHistory.Count > 0)
            {
                // If the last data point doesn't match the current values, add a new point
                int lastWealthPoint = graphData.wealthHistory[graphData.wealthHistory.Count - 1];
                int lastProductionPoint = graphData.productionHistory[graphData.productionHistory.Count - 1];
                
                if (lastWealthPoint != region.Wealth || lastProductionPoint != region.Production)
                {
                    // Values have changed externally, record them
                    RecordHistory(
                        region.Wealth,
                        region.Production,
                        100f, // Simulated price
                        1.0f, // Simulated efficiency
                        0.3f, // Simulated unmet demand
                        100f  // Simulated cycle effect
                    );
                    
                    // Generate simulated resource prices for different resources
                    Dictionary<string, float> resourceElasticities = new Dictionary<string, float>
                    {
                        { "Food", 0.5f },
                        { "Luxury", 1.5f },
                        { "RawMaterial", 0.8f },
                        { "Manufacturing", 1.2f }
                    };
                    
                    foreach (var resource in resourceElasticities.Keys)
                    {
                        // Simulate different supply/demand for each resource
                        float resourcePrice = 100f * (0.8f + Random.value * 0.4f);
                        RecordResourcePrice(resource, resourcePrice);
                    }
                    
                    // Update turn counter if it wasn't updated elsewhere
                    if (graphData.wealthHistory.Count > graphData.currentTurn)
                        graphData.currentTurn = graphData.wealthHistory.Count;
                }
            }
            else
            {
                // No history yet, initialize with first data point
                graphData.Initialize(region.Wealth, region.Production);
                
                // Initialize resource price histories
                Dictionary<string, float> resourceElasticities = new Dictionary<string, float>
                {
                    { "Food", 0.5f },
                    { "Luxury", 1.5f },
                    { "RawMaterial", 0.8f },
                    { "Manufacturing", 1.2f }
                };
                
                foreach (var resource in resourceElasticities.Keys)
                {
                    RecordResourcePrice(resource, 100f);
                }
            }
        }
        
        /// <summary>
        /// Apply changes to the economic system
        /// </summary>
        public void ApplyToSystem(EconomicSystem economicSystem)
        {
            // No changes to apply from graph module to system
        }
        
        /// <summary>
        /// Reset the graph data
        /// </summary>
        public void Reset()
        {
            graphData.Reset();
        }
        
        /// <summary>
        /// Handle editor update
        /// </summary>
        public void OnEditorUpdate(float deltaTime)
        {
            // No update needed for graphs
        }
        
        /// <summary>
        /// Get the graph data
        /// </summary>
        public GraphData GetGraphData()
        {
            return graphData;
        }
    }
}