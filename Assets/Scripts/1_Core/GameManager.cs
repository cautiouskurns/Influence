using UnityEngine;
using Systems;
using Managers;
using UI;
using Core;
using Core.Interfaces;
using System.Collections.Generic;

/// <summary>
/// Core GameManager class that acts as the central controller for initializing and managing the simulation loop.
/// It holds and manages references to key simulation objects and responds to global game events.
/// Implemented as a singleton to allow global access throughout the application.
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager _instance;
    
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameManager>();
                
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                }
            }
            
            return _instance;
        }
    }
    #endregion
    
    [Header("Core Systems")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private EconomicSystem economicSystem;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private PopulationManager populationManager;
    
    [Header("Configuration")]
    [Tooltip("ScriptableObject containing game settings")]
    [SerializeField] private GameSettings gameSettings;
    
    [Header("Game State")]
    [SerializeField] private int currentTurn;
    
    // Interface references for better coupling
    private ITurnManager _turnManagerInterface;
    private IEconomicSystem _economicSystemInterface;
    private IPopulationManager _populationManagerInterface;
    
    // Public properties for accessing managers
    /// <summary>
    /// Access to the population manager for detailed population controls
    /// </summary>
    public IPopulationManager PopulationManager => _populationManagerInterface;
    
    /// <summary>
    /// Validates all settings on editor changes
    /// </summary>
    private void OnValidate()
    {
        if (gameSettings != null)
        {
            gameSettings.ValidateSettings();
        }
    }
    
    private void Awake()
    {
        // Singleton pattern setup
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("More than one GameManager instance found. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Load default settings if none provided
        if (gameSettings == null)
        {
            gameSettings = Resources.Load<GameSettings>("DefaultGameSettings");
            if (gameSettings == null)
            {
                Debug.LogError("No GameSettings found! Please create one in the Resources folder named 'DefaultGameSettings'.");
            }
        }
        
        // Initialize game state from settings
        currentTurn = gameSettings != null ? gameSettings.startingTurn : 1;
    }
    
    private void Start()
    {
        InitializeSystems();
        
        // Subscribe to events
        EventBus.Subscribe("EconomicTick", OnEconomicTick);
        EventBus.Subscribe("SimulationStateChanged", OnSimulationStateChanged);
        
        // Initialize UI
        if (uiManager != null)
        {
            uiManager.Initialize();
        }
        
        // Set initial simulation state
        if (_turnManagerInterface != null)
        {
            if (gameSettings.startPaused)
            {
                _turnManagerInterface.Pause();
            }
            else
            {
                _turnManagerInterface.Resume();
            }
            
            _turnManagerInterface.SetTimeScale(gameSettings.baseSimulationSpeed);
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        EventBus.Unsubscribe("EconomicTick", OnEconomicTick);
        EventBus.Unsubscribe("SimulationStateChanged", OnSimulationStateChanged);
    }
    
    /// <summary>
    /// Initialize all core systems and find references if not set
    /// </summary>
    private void InitializeSystems()
    {
        // Find systems if not assigned in inspector
        if (turnManager == null)
        {
            turnManager = FindFirstObjectByType<TurnManager>();
            if (turnManager == null)
            {
                Debug.LogWarning("No TurnManager found. Creating one...");
                GameObject turnManagerObj = new GameObject("TurnManager");
                turnManager = turnManagerObj.AddComponent<TurnManager>();
            }
        }
        
        if (economicSystem == null)
        {
            economicSystem = FindFirstObjectByType<EconomicSystem>();
            if (economicSystem == null)
            {
                Debug.LogWarning("No EconomicSystem found. Creating one...");
                GameObject economicSystemObj = new GameObject("EconomicSystem");
                economicSystem = economicSystemObj.AddComponent<EconomicSystem>();
            }
        }
        
        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<UIManager>();
            if (uiManager == null)
            {
                Debug.LogWarning("No UIManager found. Creating one...");
                GameObject uiManagerObj = new GameObject("UIManager");
                uiManager = uiManagerObj.AddComponent<UIManager>();
            }
        }
        
        if (mapManager == null)
        {
            mapManager = FindFirstObjectByType<MapManager>();
            if (mapManager == null)
            {
                Debug.LogWarning("No MapManager found. UI visualization features may be limited.");
            }
        }
        
        if (populationManager == null)
        {
            populationManager = FindFirstObjectByType<PopulationManager>();
            if (populationManager == null)
            {
                Debug.LogWarning("No PopulationManager found. Creating one...");
                GameObject populationManagerObj = new GameObject("PopulationManager");
                populationManager = populationManagerObj.AddComponent<PopulationManager>();
            }
        }
        
        // Set up interface references
        _turnManagerInterface = turnManager as ITurnManager;
        _economicSystemInterface = economicSystem as IEconomicSystem;
        _populationManagerInterface = populationManager as IPopulationManager;
        
        if (_turnManagerInterface == null)
        {
            Debug.LogError("TurnManager does not implement ITurnManager interface!");
        }
        
        if (_economicSystemInterface == null)
        {
            Debug.LogError("EconomicSystem does not implement IEconomicSystem interface!");
        }
        
        if (_populationManagerInterface == null)
        {
            Debug.LogError("PopulationManager does not implement IPopulationManager interface!");
        }
        
        // Initialize UI Manager
        if (uiManager != null)
        {
            // This will be called in Start() as well, but it's safe to call multiple times
            uiManager.Initialize();
        }
    }
    
    /// <summary>
    /// Handler for economic tick events
    /// </summary>
    private void OnEconomicTick(object data)
    {
        currentTurn++;
        Debug.Log($"Turn {currentTurn} completed. Total population: {GetTotalPopulation()}");
    }
    
    /// <summary>
    /// Event definitions for the GameManager
    /// </summary>
    public static class GameEvents
    {
        /// <summary>
        /// Event raised when a new turn begins
        /// </summary>
        public const string NewTurn = "NewTurn";
        
        /// <summary>
        /// Event raised when population changes significantly
        /// </summary>
        public const string PopulationChanged = "PopulationChanged";
        
        /// <summary>
        /// Event raised when simulation state changes (pause/resume)
        /// </summary>
        public const string SimulationStateChanged = "SimulationStateChanged";
        
        /// <summary>
        /// Event raised when a significant economic change occurs
        /// </summary>
        public const string EconomicTick = "EconomicTick";
    }
    
    /// <summary>
    /// Handler for simulation state changes
    /// </summary>
    private void OnSimulationStateChanged(object data)
    {
        if (data is bool isPaused)
        {
            Debug.Log($"Simulation state changed: {(isPaused ? "Paused" : "Running")}");
        }
    }
    
    /// <summary>
    /// Get the current turn number
    /// </summary>
    public int GetCurrentTurn()
    {
        return currentTurn;
    }
    
    /// <summary>
    /// Get the total population across all regions
    /// Uses null conditional operator for null safety
    /// </summary>
    public int GetTotalPopulation() => _populationManagerInterface?.GetTotalPopulation() ?? 0;
    
    /// <summary>
    /// Get the current population growth rate
    /// Uses null conditional operator for null safety
    /// </summary>
    public float GetPopulationGrowthRate() => _populationManagerInterface?.GetPopulationGrowthRate() ?? 
        (gameSettings?.populationGrowthRate ?? 0.02f);
    
    /// <summary>
    /// Get the current migration rate
    /// Uses null conditional operator for null safety
    /// </summary>
    public float GetMigrationRate() => _populationManagerInterface?.GetMigrationRate() ?? 
        (gameSettings?.migrationRate ?? 0.01f);
    
    /// <summary>
    /// Set the simulation speed
    /// </summary>
    public void SetSimulationSpeed(float speed)
    {
        if (_turnManagerInterface != null)
        {
            _turnManagerInterface.SetTimeScale(speed);
        }
    }
    
    /// <summary>
    /// Pause the simulation
    /// </summary>
    public void PauseSimulation()
    {
        if (_turnManagerInterface != null)
        {
            _turnManagerInterface.Pause();
        }
    }
    
    /// <summary>
    /// Resume the simulation
    /// </summary>
    public void ResumeSimulation()
    {
        if (_turnManagerInterface != null)
        {
            _turnManagerInterface.Resume();
        }
    }
    
    /// <summary>
    /// Check if the simulation is paused
    /// </summary>
    public bool IsSimulationPaused()
    {
        return _turnManagerInterface != null && _turnManagerInterface.IsPaused;
    }
    
    /// <summary>
    /// Get the current game settings
    /// </summary>
    public GameSettings GetGameSettings()
    {
        return gameSettings;
    }
}