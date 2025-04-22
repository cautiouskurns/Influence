using System.Collections.Generic;
using UnityEngine;

namespace Editor.DebugWindow.Data
{
    /// <summary>
    /// Container for all graph data
    /// </summary>
    public class GraphData
    {
        // Constants
        public const int MAX_HISTORY_LENGTH = 100;
        
        // Basic graphs
        public List<int> wealthHistory = new List<int>();
        public List<int> productionHistory = new List<int>();
        public List<float> priceHistory = new List<float>();
        public List<float> efficiencyHistory = new List<float>();
        public List<float> unmetDemandHistory = new List<float>();
        public List<float> cycleEffectHistory = new List<float>();
        
        // Resource-specific graphs
        public Dictionary<string, List<float>> resourcePriceHistory = new Dictionary<string, List<float>>();
        
        // UI state
        public float graphHeight = 200f;
        public Vector2 graphScrollPosition = UnityEngine.Vector2.zero;
        
        // Turn tracking
        public int currentTurn = 1;
        
        /// <summary>
        /// Add a data point to a history list and enforce the maximum length
        /// </summary>
        public void AddDataPoint<T>(List<T> history, T value)
        {
            history.Add(value);
            
            if (history.Count > MAX_HISTORY_LENGTH)
            {
                history.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// Reset all graph data
        /// </summary>
        public void Reset()
        {
            wealthHistory.Clear();
            productionHistory.Clear();
            priceHistory.Clear();
            efficiencyHistory.Clear();
            unmetDemandHistory.Clear();
            cycleEffectHistory.Clear();
            resourcePriceHistory.Clear();
            currentTurn = 1;
        }
        
        /// <summary>
        /// Initialize empty history lists with initial values
        /// </summary>
        public void Initialize(int initialWealth, int initialProduction)
        {
            wealthHistory.Add(initialWealth);
            productionHistory.Add(initialProduction);
            priceHistory.Add(100f); // Base price
            efficiencyHistory.Add(1.0f); // Base efficiency
            unmetDemandHistory.Add(0f); // No unmet demand initially
            cycleEffectHistory.Add(100f); // Base cycle effect
        }
    }
}