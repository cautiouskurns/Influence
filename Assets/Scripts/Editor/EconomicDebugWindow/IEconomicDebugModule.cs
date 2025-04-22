using UnityEngine;
using UnityEditor;
using Systems.Economics;
using Systems;

namespace Editor.DebugWindow
{
    /// <summary>
    /// Interface for all economic debug window modules
    /// </summary>
    public interface IEconomicDebugModule
    {
        /// <summary>
        /// Draw the module UI
        /// </summary>
        void Draw();
        
        /// <summary>
        /// Sync data from the economic system to the module
        /// </summary>
        /// <param name="economicSystem">The economic system to sync from</param>
        void SyncFromSystem(EconomicSystem economicSystem);
        
        /// <summary>
        /// Apply module data to the economic system
        /// </summary>
        /// <param name="economicSystem">The economic system to apply to</param>
        void ApplyToSystem(EconomicSystem economicSystem);
        
        /// <summary>
        /// Reset the module data
        /// </summary>
        void Reset();
        
        /// <summary>
        /// Update the module on editor update
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        void OnEditorUpdate(float deltaTime);
    }
}