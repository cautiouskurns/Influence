using UnityEngine;
using Managers;
using System;
using System.Collections;
using Core.Interfaces;

namespace Core
{
    /// <summary>
    /// Manages turn-based progression in the simulation with support for pausing and customizable tick speed.
    /// 
    /// This class serves as the central clock for the turn-based systems, providing:
    /// - Regular tick intervals with adjustable speed
    /// - Pause/resume functionality
    /// - Turn counting and event broadcasts
    /// - Time scaling for simulation speed control
    /// </summary>
    public class TurnManager : MonoBehaviour, ITurnManager
    {
        #region Inspector Fields
        [Header("Turn Configuration")]
        [Tooltip("Time in seconds between each simulation turn/tick")]
        [SerializeField] private float tickIntervalSeconds = 1.0f;
        
        [Tooltip("Multiplier for simulation speed (1.0 = normal speed)")]
        [Range(0.1f, 10f)]
        [SerializeField] private float timeScale = 1.0f;
        
        [Tooltip("Whether the simulation starts in a paused state")]
        [SerializeField] private bool startPaused = false;
        #endregion

        #region Public Properties
        /// <summary>
        /// Whether the simulation is currently paused
        /// </summary>
        public bool IsPaused { get; private set; }
        
        /// <summary>
        /// Current turn number in the simulation
        /// </summary>
        public int CurrentTurn { get; private set; } = 0;
        
        /// <summary>
        /// Current time scale factor (speed multiplier)
        /// </summary>
        public float TimeScale => timeScale;
        #endregion

        #region Events
        /// <summary>
        /// Event triggered when a turn completes
        /// </summary>
        public static event Action<int> OnTurnCompleted;
        
        /// <summary>
        /// Event triggered when simulation is paused or resumed
        /// </summary>
        public static event Action<bool> OnSimulationPausedChanged;
        #endregion

        #region Private Fields
        private Coroutine _tickRoutine;
        private const float MIN_TIME_SCALE = 0.1f;
        private const string TURN_ENDED_EVENT = "TurnEnded";
        #endregion

        #region Unity Lifecycle Methods
        /// <summary>
        /// Sets up initial component state
        /// </summary>
        private void Awake()
        {
            IsPaused = startPaused;
        }

        /// <summary>
        /// Starts the turn progression system
        /// </summary>
        private void OnEnable()
        {
            // Start the turn cycle when enabled
            if (_tickRoutine == null)
            {
                _tickRoutine = StartCoroutine(TickLoop());
            }
        }

        /// <summary>
        /// Stops the turn progression
        /// </summary>
        private void OnDisable()
        {
            // Stop the turn cycle when disabled
            if (_tickRoutine != null)
            {
                StopCoroutine(_tickRoutine);
                _tickRoutine = null;
            }
        }

        /// <summary>
        /// Validates inspector values when they change in editor
        /// </summary>
        private void OnValidate()
        {
            // Ensure timeScale stays above minimum value
            timeScale = Mathf.Max(MIN_TIME_SCALE, timeScale);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// The main turn progression loop
        /// </summary>
        private IEnumerator TickLoop()
        {
            while (true)
            {
                if (!IsPaused)
                {
                    ProcessTurn();
                }

                yield return new WaitForSeconds(tickIntervalSeconds / timeScale);
            }
        }

        /// <summary>
        /// Processes a single turn/tick of the simulation
        /// </summary>
        private void ProcessTurn()
        {
            // Increment turn counter
            CurrentTurn++;
            
            // Log the turn if debugging is enabled
            #if UNITY_EDITOR
            Debug.Log($"Turn {CurrentTurn}");
            #endif
            
            // Trigger turn events - C# event first
            OnTurnCompleted?.Invoke(CurrentTurn);
            
            // Legacy event bus for backward compatibility
            EventBus.Trigger(TURN_ENDED_EVENT);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Pauses the simulation
        /// </summary>
        public void Pause()
        {
            if (!IsPaused)
            {
                IsPaused = true;
                #if UNITY_EDITOR
                Debug.Log("Simulation paused");
                #endif
                
                OnSimulationPausedChanged?.Invoke(true);
            }
        }

        /// <summary>
        /// Resumes the simulation from a paused state
        /// </summary>
        public void Resume()
        {
            if (IsPaused)
            {
                IsPaused = false;
                #if UNITY_EDITOR
                Debug.Log("Simulation resumed");
                #endif
                
                OnSimulationPausedChanged?.Invoke(false);
            }
        }
        
        /// <summary>
        /// Sets the time scale (simulation speed)
        /// </summary>
        /// <param name="newTimeScale">The new time scale value (minimum 0.1)</param>
        public void SetTimeScale(float newTimeScale)
        {
            float oldTimeScale = timeScale;
            timeScale = Mathf.Max(MIN_TIME_SCALE, newTimeScale);
            
            if (!Mathf.Approximately(oldTimeScale, timeScale))
            {
                #if UNITY_EDITOR
                Debug.Log($"Time scale set to {timeScale:F1}x");
                #endif
            }
        }
        
        /// <summary>
        /// Gets the current time scale (simulation speed)
        /// </summary>
        /// <returns>The current time scale value</returns>
        [Obsolete("Use TimeScale property instead")]
        public float GetTimeScale()
        {
            return timeScale;
        }
        
        /// <summary>
        /// Sets the current turn number for scenarios where manual adjustment is needed
        /// </summary>
        /// <param name="turnNumber">New turn number to set</param>
        public void SetTurnNumber(int turnNumber)
        {
            if (turnNumber >= 0)
            {
                CurrentTurn = turnNumber;
            }
            else
            {
                Debug.LogWarning("Attempted to set turn number to a negative value");
            }
        }
        #endregion

        #region Editor Methods
        /// <summary>
        /// Reset to default values when the component is first added
        /// </summary>
        private void Reset()
        {
            tickIntervalSeconds = 1.0f;
            timeScale = 1.0f;
            startPaused = false;
        }
        #endregion
    }
}