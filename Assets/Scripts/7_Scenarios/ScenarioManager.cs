using UnityEngine;
using System.Collections.Generic;
using Systems;
using Managers;
using Entities;
using Core;
using UI;   

namespace Scenarios
{
    public class ScenarioManager : MonoBehaviour
    {
        [SerializeField] private TestScenario activeScenario;
        
        // System references
        [SerializeField] private EconomicSystem economicSystem;
        [SerializeField] private NationManager nationManager;
        [SerializeField] private TurnManager turnManager;
        
        // UI reference
        [SerializeField] private ScenarioUI scenarioUI;
        
        private int currentTurn = 0;
        private bool scenarioActive = false;
        private int consecutiveVictoryTurns = 0;
        private Dictionary<string, RegionEntity> scenarioRegions = new Dictionary<string, RegionEntity>();
        private Dictionary<string, NationEntity> scenarioNations = new Dictionary<string, NationEntity>();
        
        // Public properties
        public int CurrentTurn => currentTurn;
        public int TurnLimit => activeScenario != null ? activeScenario.turnLimit : 0;
        public bool IsScenarioActive => scenarioActive;
        
        private void Start()
        {
            FindSystemReferences();
            
            // Subscribe to turn events
            EventBus.Subscribe("TurnEnded", OnTurnEnded);
            
            // Find UI if not assigned
            if (scenarioUI == null)
                scenarioUI = FindFirstObjectByType<ScenarioUI>();
        }
        
        private void OnDestroy()
        {
            EventBus.Unsubscribe("TurnEnded", OnTurnEnded);
        }
        
        private void FindSystemReferences()
        {
            if (economicSystem == null)
                economicSystem = FindFirstObjectByType<EconomicSystem>();
                
            if (nationManager == null)
                nationManager = FindFirstObjectByType<NationManager>();
                
            if (turnManager == null)
                turnManager = FindFirstObjectByType<TurnManager>();
        }
        
        // Handle turn ended events
        private void OnTurnEnded(object _)
        {
            if (!scenarioActive || activeScenario == null)
                return;
                
            currentTurn++;
            UpdateUI();
            
            // Check victory conditions
            if (CheckVictoryConditions())
            {
                consecutiveVictoryTurns++;
                
                if (consecutiveVictoryTurns >= activeScenario.victoryCondition.requiredConsecutiveTurns)
                {
                    EndScenario(true);
                    return;
                }
            }
            else
            {
                consecutiveVictoryTurns = 0;
            }
            
            // Check if turn limit reached
            if (currentTurn >= activeScenario.turnLimit)
            {
                EndScenario(false);
            }
        }
        
        // Start a test scenario
        public void StartScenario(TestScenario scenario)
        {
            if (scenario == null)
                return;
                
            // Clean up any existing scenario
            if (scenarioActive)
                CleanupScenario();
                
            activeScenario = scenario;
            scenarioActive = true;
            currentTurn = 0;
            consecutiveVictoryTurns = 0;
            
            // Setup regions and nations
            SetupRegions();
            SetupNations();
            
            // Update UI
            UpdateUI();
            
            Debug.Log($"Started scenario: {scenario.scenarioName}");
        }
        
        // Basic setup for regions
        private void SetupRegions()
        {
            scenarioRegions.Clear();
            
            foreach (var regionCondition in activeScenario.regionStartConditions)
            {
                // Create region with basic properties
                RegionEntity region = new RegionEntity(
                    regionCondition.regionId,
                    regionCondition.regionName,
                    regionCondition.initialWealth, 
                    regionCondition.initialProduction
                );
                
                // Store and register the region
                scenarioRegions[region.Id] = region;
                if (economicSystem != null)
                    economicSystem.RegisterRegion(region);
            }
        }
        
        // Basic setup for nations
        private void SetupNations()
        {
            scenarioNations.Clear();
            
            foreach (var nationCondition in activeScenario.nationStartConditions)
            {
                // Create nation with basic properties
                NationEntity nation = null;
                
                // Register with nation manager
                if (nationManager != null)
                {
                    try {
                        // Create and register the nation using the proper method
                        nation = nationManager.CreateNation(
                            nationCondition.nationId,
                            nationCondition.nationName,
                            new Color(Random.value, Random.value, Random.value)
                        );
                        
                        // Store the nation
                        if (nation != null) {
                            scenarioNations[nation.Id] = nation;
                            Debug.Log($"Created nation: {nation.Id}");
                        }
                    }
                    catch (System.Exception ex) {
                        Debug.LogWarning($"Could not create nation using nationManager.CreateNation: {ex.Message}");
                    }
                    
                    // Assign regions to nation
                    if (nation != null) {
                        foreach (string regionId in nationCondition.controlledRegionIds)
                        {
                            try {
                                nationManager.AssignRegionToNation(regionId, nation.Id);
                            }
                            catch (System.Exception ex) {
                                Debug.LogWarning($"Could not assign region {regionId} to nation {nation.Id}: {ex.Message}");
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("NationManager is null! Cannot register nations.");
                }
            }
        }
        
        // Check if victory conditions are met
        private bool CheckVictoryConditions()
        {
            if (activeScenario == null || activeScenario.victoryCondition == null)
                return false;
                
            VictoryCondition condition = activeScenario.victoryCondition;
            
            // Get regions to check
            List<RegionEntity> regionsToCheck = new List<RegionEntity>();
            if (string.IsNullOrEmpty(condition.targetRegionId))
            {
                // Check all regions
                regionsToCheck.AddRange(economicSystem.GetAllRegions());
            }
            else
            {
                // Check specific region
                RegionEntity targetRegion = economicSystem.GetRegion(condition.targetRegionId);
                if (targetRegion != null)
                    regionsToCheck.Add(targetRegion);
            }
            
            if (regionsToCheck.Count == 0)
                return false;
                
            // Check based on victory type
            switch (condition.type)
            {
                case VictoryCondition.VictoryType.Economic:
                    // Check if all regions meet economic conditions
                    foreach (var region in regionsToCheck)
                    {
                        if (region.Economy.Wealth < condition.requiredWealth)
                            return false;
                    }
                    return true;
                    
                case VictoryCondition.VictoryType.Development:
                    // Check if all regions meet infrastructure conditions
                    foreach (var region in regionsToCheck)
                    {
                        if (region.Infrastructure.Level < condition.requiredInfrastructure)
                            return false;
                    }
                    return true;
                    
                case VictoryCondition.VictoryType.Stability:
                    // Check if all regions meet satisfaction conditions
                    foreach (var region in regionsToCheck)
                    {
                        if (region.PopulationComp.Satisfaction < condition.requiredSatisfaction)
                            return false;
                    }
                    return true;
                    
                default:
                    return false;
            }
        }
        
        // End the current scenario
        private void EndScenario(bool victory)
        {
            scenarioActive = false;
            
            if (scenarioUI != null)
            {
                if (victory)
                    scenarioUI.ShowVictoryPanel();
                else
                    scenarioUI.ShowDefeatPanel();
            }
            
            Debug.Log($"Scenario ended: {activeScenario.scenarioName}, Victory: {victory}");
        }
        
        // Update the scenario UI
        private void UpdateUI()
        {
            if (activeScenario == null || scenarioUI == null)
                return;
                
            scenarioUI.SetScenarioInfo(
                activeScenario.scenarioName,
                activeScenario.description,
                GetObjectiveDescription()
            );
            
            scenarioUI.UpdateTurnCounter(currentTurn);
        }
        
        // Get formatted objective description
        private string GetObjectiveDescription()
        {
            if (activeScenario == null || activeScenario.victoryCondition == null)
                return "No objectives";
                
            VictoryCondition condition = activeScenario.victoryCondition;
            string target = string.IsNullOrEmpty(condition.targetRegionId) ? 
                "all regions" : $"region {condition.targetRegionId}";
                
            switch (condition.type)
            {
                case VictoryCondition.VictoryType.Economic:
                    return $"Reach {condition.requiredWealth} wealth in {target}";
                    
                case VictoryCondition.VictoryType.Development:
                    return $"Develop infrastructure to level {condition.requiredInfrastructure} in {target}";
                    
                case VictoryCondition.VictoryType.Stability:
                    return $"Maintain {condition.requiredSatisfaction:P0} satisfaction in {target}";
                    
                default:
                    return "Unknown objective";
            }
        }
        
        // Clean up scenario resources
        private void CleanupScenario()
        {
            scenarioRegions.Clear();
            scenarioNations.Clear();
            currentTurn = 0;
            consecutiveVictoryTurns = 0;
            scenarioActive = false;
        }
    }
}