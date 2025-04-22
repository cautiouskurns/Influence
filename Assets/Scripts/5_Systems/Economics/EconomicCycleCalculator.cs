using UnityEngine;
using Entities;
using System.Collections.Generic;

namespace Systems.Economics
{
    /// <summary>
    /// Handles economic cycle calculations and effects
    /// </summary>
    public class EconomicCycleCalculator
    {
        // Cycle phases
        public enum CyclePhase
        {
            Expansion,      // Growth phase
            Peak,           // Temporary prosperity
            Contraction,    // Decline phase
            Trough          // Bottom of cycle
        }
        
        // Current cycle state
        private CyclePhase currentPhase = CyclePhase.Expansion;
        private float phaseProgress = 0f; // 0 to 1, progress through current phase
        private int cycleLength = 12;     // How many turns a full cycle takes
        private int currentTurn = 0;
        
        // Phase coefficients (modifiers for different economic aspects)
        private Dictionary<CyclePhase, Dictionary<string, float>> phaseCoefficients = new Dictionary<CyclePhase, Dictionary<string, float>>();
        
        public EconomicCycleCalculator(int cycleLength = 12)
        {
            this.cycleLength = Mathf.Max(4, cycleLength);
            InitializeDefaultCoefficients();
        }
        
        private void InitializeDefaultCoefficients()
        {
            // Set up default coefficients for different phases
            
            // Expansion phase - growth in all sectors
            var expansionCoefficients = new Dictionary<string, float>
            {
                { "Production", 1.15f },      // Production bonus
                { "Consumption", 1.10f },     // Increased consumption
                { "Investment", 1.25f },      // High investment
                { "PriceInflation", 1.05f },  // Slight inflation
                { "Unrest", 0.9f }            // Reduced unrest
            };
            
            // Peak phase - prosperity but with potential instability
            var peakCoefficients = new Dictionary<string, float>
            {
                { "Production", 1.2f },       // Strong production
                { "Consumption", 1.3f },      // High consumption
                { "Investment", 1.1f },       // Slowing investment
                { "PriceInflation", 1.15f },  // Higher inflation
                { "Unrest", 0.95f }           // Slightly reduced unrest
            };
            
            // Contraction phase - decline in economy
            var contractionCoefficients = new Dictionary<string, float>
            {
                { "Production", 0.9f },       // Reduced production
                { "Consumption", 0.85f },     // Reduced consumption
                { "Investment", 0.7f },       // Low investment
                { "PriceInflation", 0.95f },  // Slight deflation
                { "Unrest", 1.2f }            // Increased unrest
            };
            
            // Trough phase - bottom of cycle
            var troughCoefficients = new Dictionary<string, float>
            {
                { "Production", 0.8f },       // Low production
                { "Consumption", 0.75f },     // Low consumption
                { "Investment", 0.8f },       // Beginning of recovery investments
                { "PriceInflation", 0.9f },   // Deflation
                { "Unrest", 1.4f }            // High unrest
            };
            
            // Add to the phase coefficients dictionary
            phaseCoefficients.Add(CyclePhase.Expansion, expansionCoefficients);
            phaseCoefficients.Add(CyclePhase.Peak, peakCoefficients);
            phaseCoefficients.Add(CyclePhase.Contraction, contractionCoefficients);
            phaseCoefficients.Add(CyclePhase.Trough, troughCoefficients);
        }
        
        /// <summary>
        /// Advance the economic cycle by one turn
        /// </summary>
        public void AdvanceCycle()
        {
            currentTurn++;
            
            // Calculate phase (each phase takes 1/4 of the cycle)
            int phaseLength = cycleLength / 4;
            int cyclePosition = currentTurn % cycleLength;
            
            // Determine current phase
            if (cyclePosition < phaseLength)
            {
                currentPhase = CyclePhase.Expansion;
            }
            else if (cyclePosition < phaseLength * 2)
            {
                currentPhase = CyclePhase.Peak;
            }
            else if (cyclePosition < phaseLength * 3)
            {
                currentPhase = CyclePhase.Contraction;
            }
            else
            {
                currentPhase = CyclePhase.Trough;
            }
            
            // Calculate progress within the phase (0 to 1)
            phaseProgress = (cyclePosition % phaseLength) / (float)phaseLength;
        }
        
        /// <summary>
        /// Apply economic cycle effects to a given value
        /// </summary>
        /// <param name="baseValue">The base value to modify</param>
        /// <param name="effectType">Type of effect to apply (e.g., "Production", "Consumption")</param>
        /// <returns>Modified value with cycle effects applied</returns>
        public float ApplyCycleEffect(float baseValue, string effectType)
        {
            // Get coefficient for current phase and effect type
            float coefficient = 1.0f; // Default: no change
            
            if (phaseCoefficients.TryGetValue(currentPhase, out var effects))
            {
                if (effects.TryGetValue(effectType, out float value))
                {
                    coefficient = value;
                }
            }
            
            // CycleEffect = PhaseModifier × Coefficient
            return baseValue * coefficient;
        }
        
        /// <summary>
        /// Get the current economic cycle phase
        /// </summary>
        public CyclePhase GetCurrentPhase()
        {
            return currentPhase;
        }
        
        /// <summary>
        /// Get progress through the current cycle phase (0-1)
        /// </summary>
        public float GetPhaseProgress()
        {
            return phaseProgress;
        }
        
        /// <summary>
        /// Get the current turn number in the cycle
        /// </summary>
        public int GetCurrentTurn()
        {
            return currentTurn;
        }
        
        /// <summary>
        /// Get a descriptive string about the current economic conditions
        /// </summary>
        public string GetEconomicConditionDescription()
        {
            switch (currentPhase)
            {
                case CyclePhase.Expansion:
                    return "Economic Expansion: The economy is growing steadily with increasing production and investment.";
                    
                case CyclePhase.Peak:
                    return "Economic Peak: The economy is at its strongest point, with high consumption but rising inflation.";
                    
                case CyclePhase.Contraction:
                    return "Economic Contraction: The economy is slowing down with falling production and investment.";
                    
                case CyclePhase.Trough:
                    return "Economic Trough: The economy is at its weakest point, with low consumption and high unrest.";
                    
                default:
                    return "Normal Economic Conditions";
            }
        }
        
        /// <summary>
        /// Modify coefficients for a specific phase and effect
        /// </summary>
        public void SetPhaseCoefficient(CyclePhase phase, string effectType, float value)
        {
            if (!phaseCoefficients.ContainsKey(phase))
            {
                phaseCoefficients[phase] = new Dictionary<string, float>();
            }
            
            phaseCoefficients[phase][effectType] = value;
        }
    }
}