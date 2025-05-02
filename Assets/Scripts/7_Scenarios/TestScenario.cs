using UnityEngine;
using System.Collections.Generic;

namespace Scenarios
{
    [CreateAssetMenu(fileName = "TestScenario", menuName = "Influence/Test Scenario")]
    public class TestScenario : ScriptableObject
    {
        [Header("Scenario Settings")]
        public string scenarioName = "Test Scenario";
        
        [TextArea(3, 5)]
        public string description = "Description of this test scenario";
        
        public int turnLimit = 10;
        
        [Header("Starting Conditions")]
        public List<RegionStartCondition> regionStartConditions = new List<RegionStartCondition>();
        public List<NationStartCondition> nationStartConditions = new List<NationStartCondition>();
        
        [Header("Victory Conditions")]
        public VictoryCondition victoryCondition;
    }
}