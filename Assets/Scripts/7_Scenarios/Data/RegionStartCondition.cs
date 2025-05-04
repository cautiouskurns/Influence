using UnityEngine;

namespace Scenarios
{
    [System.Serializable]
    public class RegionStartCondition
    {
        public string regionId;
        public string regionName;
        
        public int initialWealth = 100;
        public int initialProduction = 50;
        public float initialInfrastructureLevel = 10;
        
        public int initialPopulation = 1000;
        public float initialSatisfaction = 0.7f;
    }
}