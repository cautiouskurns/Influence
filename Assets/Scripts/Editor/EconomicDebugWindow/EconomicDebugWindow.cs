using UnityEngine;
using UnityEditor;
using Systems;
using Entities;
using Core;
using Systems.Economics;
using System;
using System.Collections.Generic;
using System.Linq; // Add LINQ namespace for IEnumerable extension methods
using Editor.DebugWindow.Modules;
using Editor.DebugWindow.Data;

namespace Editor.DebugWindow
{
    /// <summary>
    /// Editor window for debugging and tuning the modular economic simulation parameters
    /// </summary>
    public class EconomicDebugWindow : EditorWindow
    {
        // Reference to economic system
        private EconomicSystem economicSystem;
        
        // Modules
        private SimulationControlsModule simulationControls;
        private ParametersModule parametersModule;
        private GraphModule graphModule;
        
        // Region test data
        private RegionTestModule regionTestModule;
        
        // Results view
        private ResultsModule resultsModule;
        
        // UI state for apply buttons
        private bool showApplyButtons = true;
        
        [MenuItem("Window/Economic Debug")]
        public static void ShowWindow()
        {
            var window = GetWindow<EconomicDebugWindow>("Economic Debug");
            window.minSize = new Vector2(500, 700);
        }
        
        private void OnEnable()
        {
            // Initialize modules
            simulationControls = new SimulationControlsModule(this);
            parametersModule = new ParametersModule();
            graphModule = new GraphModule();
            regionTestModule = new RegionTestModule();
            resultsModule = new ResultsModule();
            
            // Try to find the economic system
            FindEconomicSystem();
            
            // Set up editor update callback
            EditorApplication.update += OnEditorUpdate;
        }
        
        private void OnDisable()
        {
            // Clean up editor update
            EditorApplication.update -= OnEditorUpdate;
        }
        
        private void OnGUI()
        {
            // Draw modules
            simulationControls.Draw();
            
            // Show info if we don't have the system yet
            if (economicSystem == null)
            {
                EditorGUILayout.HelpBox(
                    "No EconomicSystem found. Enter Play Mode to use the debugger with the active scene, or configure test values below.", 
                    MessageType.Warning);
            }
            
            // Draw parameter controls
            parametersModule.Draw();
            
            // Draw region test controls
            regionTestModule.Draw();
            
            // Draw results section
            resultsModule.Draw();
            
            // Draw graphs
            graphModule.Draw();
            
            // Force repaint every frame when in Play Mode to keep UI fresh
            if (simulationControls.IsSimulationActive())
            {
                Repaint();
            }
        }
        
        /// <summary>
        /// Find the economic system in the scene
        /// </summary>
        private void FindEconomicSystem()
        {
            if (!EditorApplication.isPlaying) return;
            
            // Try to find in scene
            economicSystem = GameObject.FindFirstObjectByType<EconomicSystem>();
            
            if (economicSystem != null)
            {
                // Sync all modules with the found system
                SyncAllModules();
            }
        }
        
        /// <summary>
        /// Handle editor updates
        /// </summary>
        private void OnEditorUpdate()
        {
            // Pass update to all modules
            simulationControls.OnEditorUpdate(Time.deltaTime);
            parametersModule.OnEditorUpdate(Time.deltaTime);
            graphModule.OnEditorUpdate(Time.deltaTime);
            regionTestModule.OnEditorUpdate(Time.deltaTime);
            resultsModule.OnEditorUpdate(Time.deltaTime);
            
            // Check if we need to find the economic system (e.g., if we just entered play mode)
            if (economicSystem == null && EditorApplication.isPlaying)
            {
                FindEconomicSystem();
            }
            
            // Check for external changes to region data (in case region values were modified outside this window)
            if (simulationControls.IsSimulationActive() && economicSystem != null)
            {
                SyncFromSystem();
            }
        }
        
        /// <summary>
        /// Reset the simulation
        /// </summary>
        public void ResetSimulation()
        {
            // Reset all modules
            parametersModule.Reset();
            graphModule.Reset();
            regionTestModule.Reset();
            resultsModule.Reset();
            
            // Re-sync with the system
            SyncAllModules();
        }
        
        /// <summary>
        /// Run a simulation tick/turn
        /// </summary>
        public void RunSimulationTick()
        {
            if (economicSystem == null) return;
            
            // Apply all module settings to the system
            ApplyAllModules();
            
            // Run economic calculations
            economicSystem.ProcessEconomicTick();
            
            // Process region updates
            var regions = economicSystem.GetAllRegionIds();
            if (regions.Any()) // Changed from Count > 0 to Any()
            {
                // Get the first region for simplicity
                var region = economicSystem.GetRegion(regions.First()); // Changed from [0] to First()
                if (region != null)
                {
                    // Record data for graphs
                    graphModule.RecordHistory(
                        region.Wealth,
                        region.Production,
                        100f, // Basic price indicator
                        1.0f + region.InfrastructureQuality * parametersModule.GetParameters().infrastructure.efficiencyModifier,
                        0.2f, // Sample unmet demand
                        1.0f  // Sample cycle effect
                    );
                    
                    // Record resource price data
                    foreach (var resource in parametersModule.GetParameters().price.resourceElasticities.Keys)
                    {
                        float resourcePrice = 100f * (0.8f + UnityEngine.Random.value * 0.4f);
                        graphModule.RecordResourcePrice(resource, resourcePrice);
                    }
                    
                    // Update turn counter
                    graphModule.IncrementTurn();
                    
                    // Update results with latest values
                    resultsModule.UpdateResults(
                        region.Wealth,
                        region.Production,
                        region.Population,
                        region.InfrastructureQuality,
                        0.05f, // Sample efficiency
                        0.2f,  // Sample unmet demand
                        graphModule.GetCurrentTurn()
                    );
                }
            }
            
            // Sync back any updates
            SyncFromSystem();
        }
        
        /// <summary>
        /// Sync parameters from economic system to the UI
        /// </summary>
        private void SyncFromSystem()
        {
            // Pass through to modules
            parametersModule.SyncFromSystem(economicSystem);
            graphModule.SyncFromSystem(economicSystem);
            regionTestModule.SyncFromSystem(economicSystem);
            resultsModule.SyncFromSystem(economicSystem);
        }
        
        /// <summary>
        /// Apply UI parameters to the economic system
        /// </summary>
        private void ApplyAllModules()
        {
            // Pass through to modules
            parametersModule.ApplyToSystem(economicSystem);
            regionTestModule.ApplyToSystem(economicSystem);
        }
        
        /// <summary>
        /// Sync all modules with the system
        /// </summary>
        private void SyncAllModules()
        {
            simulationControls.SyncFromSystem(economicSystem);
            parametersModule.SyncFromSystem(economicSystem);
            graphModule.SyncFromSystem(economicSystem);
            regionTestModule.SyncFromSystem(economicSystem);
            resultsModule.SyncFromSystem(economicSystem);
        }
    }
}