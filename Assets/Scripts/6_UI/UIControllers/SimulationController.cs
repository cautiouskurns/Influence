using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core;
using Systems;

namespace UI
{
    /// <summary>
    /// SimulationController provides UI controls for the economic simulation
    /// </summary>
    public class SimulationController : MonoBehaviour
    {
        [Header("Core Systems")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private EconomicSystem economicSystem;
        
        [Header("UI Controls")]
        public Button playButton;
        public Button pauseButton;
        public Button stepButton;
        public Slider speedSlider;
        public TextMeshProUGUI statusText;
        
        private void Start()
        {
            // Find systems if not set in inspector
            if (turnManager == null)
                turnManager = FindFirstObjectByType<TurnManager>();
                
            if (economicSystem == null)
                economicSystem = FindFirstObjectByType<EconomicSystem>();
                
            // Setup button callbacks
            if (playButton != null)
                playButton.onClick.AddListener(ResumeSimulation);
                
            if (pauseButton != null)
                pauseButton.onClick.AddListener(PauseSimulation);
                
            if (stepButton != null)
                stepButton.onClick.AddListener(StepSimulation);
                
            if (speedSlider != null)
                speedSlider.onValueChanged.AddListener(ChangeSpeed);
                
            UpdateStatusText();
        }
        
        private void ResumeSimulation()
        {
            if (turnManager != null)
            {
                turnManager.Resume();
                UpdateStatusText();
            }
        }
        
        private void PauseSimulation()
        {
            if (turnManager != null)
            {
                turnManager.Pause();
                UpdateStatusText();
            }
        }
        
        private void StepSimulation()
        {
            if (economicSystem != null)
            {
                economicSystem.ProcessEconomicTick();
                UpdateStatusText();
            }
        }
        
        private void ChangeSpeed(float value)
        {
            if (turnManager != null)
            {
                // Map slider 0-1 to meaningful time scale between 0.5x and 3x
                float speed = 0.5f + (value * 2.5f);
                turnManager.SetTimeScale(speed);
                UpdateStatusText();
            }
        }
        
        private void UpdateStatusText()
        {
            if (statusText != null)
            {
                string status = "Status: ";
                
                if (turnManager != null)
                {
                    status += turnManager.IsPaused ? "PAUSED" : "RUNNING";
                    status += $" (Speed: {turnManager.GetTimeScale():F1}x)";
                }
                else
                {
                    status += "No TurnManager found";
                }
                
                statusText.text = status;
            }
        }
    }
}