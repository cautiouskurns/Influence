using UnityEngine;

namespace Core.Interfaces
{
    /// <summary>
    /// Interface for the TurnManager component, allowing for looser coupling
    /// and easier testing/mocking.
    /// </summary>
    public interface ITurnManager
    {
        /// <summary>
        /// Current turn number
        /// </summary>
        int CurrentTurn { get; }
        
        /// <summary>
        /// Whether the simulation is currently paused
        /// </summary>
        bool IsPaused { get; }
        
        /// <summary>
        /// Current time scale (simulation speed)
        /// </summary>
        float TimeScale { get; }
        
        /// <summary>
        /// Pause the simulation
        /// </summary>
        void Pause();
        
        /// <summary>
        /// Resume the simulation from a paused state
        /// </summary>
        void Resume();
        
        /// <summary>
        /// Set the time scale (simulation speed)
        /// </summary>
        /// <param name="scale">The new time scale value</param>
        void SetTimeScale(float scale);
    }
}