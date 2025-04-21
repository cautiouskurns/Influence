using UnityEngine;
using Managers;
using System.Collections;

namespace Core
{
    /// <summary>
    /// TurnManager handles turn-based progression with support for pausing and customizable tick speed.
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        [SerializeField] private float tickIntervalSeconds = 1.0f;
        [SerializeField] private float timeScale = 1.0f;
        [SerializeField] private bool isPaused = false;
        
        private Coroutine tickRoutine;
        private int currentTurn = 0;
        
        // Public properties for external access
        public bool IsPaused => isPaused;
        public int CurrentTurn => currentTurn;

        private void Start()
        {
            tickRoutine = StartCoroutine(TickLoop());
        }

        private IEnumerator TickLoop()
        {
            while (true)
            {
                if (!isPaused)
                {
                    currentTurn++;
                    Debug.Log($"Turn {currentTurn}");
                    EventBus.Trigger("TurnEnded");
                }

                yield return new WaitForSeconds(tickIntervalSeconds / timeScale);
            }
        }

        public void Pause()
        {
            isPaused = true;
            Debug.Log("Simulation paused");
        }

        public void Resume()
        {
            isPaused = false;
            Debug.Log("Simulation resumed");
        }
        
        public void SetTimeScale(float newTimeScale)
        {
            timeScale = Mathf.Max(0.1f, newTimeScale);
            Debug.Log($"Time scale set to {timeScale:F1}x");
        }
        
        public float GetTimeScale()
        {
            return timeScale;
        }
    }
}