using UnityEngine;

namespace Scenarios
{
    [System.Serializable]
    public class VictoryCondition
    {
        public enum VictoryType
        {
            Economic,
            Development,
            Stability
        }
        
        public VictoryType type = VictoryType.Economic;
        public string targetRegionId = ""; // Empty means "all regions"
        
        // Economic victory condition
        public int requiredWealth = 500;
        
        // Development victory condition
        public float requiredInfrastructure = 20f;
        
        // Stability victory condition
        public float requiredSatisfaction = 0.8f;
        
        // Number of consecutive turns the condition must be met
        public int requiredConsecutiveTurns = 1;
    }
}