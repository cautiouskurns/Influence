using UnityEngine;
using Systems;
using Managers;
using UI;
using Core;

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
    
    [Header("Simulation Settings")]
    [SerializeField] private int startingTurn = 1;
    [SerializeField] private float baseSimulationSpeed = 1.0f;
    [SerializeField] private bool startPaused = true;
    
    [Header("Game State")]
    [SerializeField] private int currentTurn;
    [SerializeField] private int totalPopulation;
    [SerializeField] private float populationGrowthRate = 0.02f;
    [SerializeField] private float migrationRate = 0.01f;
    
    // private bool _isInitialized = false;
    
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
        
        // Initialize game state
        currentTurn = startingTurn;
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
        if (turnManager != null)
        {
            if (startPaused)
            {
                turnManager.Pause();
            }
            else
            {
                turnManager.Resume();
            }
            
            turnManager.SetTimeScale(baseSimulationSpeed);
        }
        
        // _isInitialized = true;
        Debug.Log("GameManager initialized successfully");
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
        
        // Initialize UI Manager
        if (uiManager != null)
        {
            // This will be called in Start() as well, but it's safe to call multiple times
            uiManager.Initialize();
        }
        
        // The EconomicSystem doesn't have an Initialize method, so we'll just proceed
        // with calculating the total population
        CalculateTotalPopulation();
    }
    
    /// <summary>
    /// Calculate the total population from all regions
    /// </summary>
    private void CalculateTotalPopulation()
    {
        totalPopulation = 0;
        
        if (economicSystem != null)
        {
            foreach (string regionId in economicSystem.GetAllRegionIds())
            {
                var region = economicSystem.GetRegion(regionId);
                if (region != null)
                {
                    totalPopulation += Mathf.RoundToInt(region.LaborAvailable);
                }
            }
        }
    }
    
    /// <summary>
    /// Handler for economic tick events
    /// </summary>
    private void OnEconomicTick(object data)
    {
        currentTurn++;
        CalculateTotalPopulation();
        
        // Trigger population growth
        ApplyPopulationGrowth();
        
        Debug.Log($"Turn {currentTurn} completed. Total population: {totalPopulation}");
    }
    
    /// <summary>
    /// Apply population growth to all regions
    /// </summary>
    private void ApplyPopulationGrowth()
    {
        if (economicSystem == null) return;
        
        foreach (string regionId in economicSystem.GetAllRegionIds())
        {
            var region = economicSystem.GetRegion(regionId);
            if (region != null)
            {
                // Calculate growth based on current population and growth rate
                int growthAmount = Mathf.FloorToInt(region.LaborAvailable * populationGrowthRate);
                
                // Apply growth
                region.LaborAvailable += growthAmount;
                
                // Update the region
                economicSystem.UpdateRegion(region);
            }
        }
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
    /// </summary>
    public int GetTotalPopulation()
    {
        return totalPopulation;
    }
    
    /// <summary>
    /// Get the current population growth rate
    /// </summary>
    public float GetPopulationGrowthRate()
    {
        return populationGrowthRate;
    }
    
    /// <summary>
    /// Get the current migration rate
    /// </summary>
    public float GetMigrationRate()
    {
        return migrationRate;
    }
    
    /// <summary>
    /// Set the simulation speed
    /// </summary>
    public void SetSimulationSpeed(float speed)
    {
        if (turnManager != null)
        {
            turnManager.SetTimeScale(speed);
        }
    }
    
    /// <summary>
    /// Pause the simulation
    /// </summary>
    public void PauseSimulation()
    {
        if (turnManager != null)
        {
            turnManager.Pause();
        }
    }
    
    /// <summary>
    /// Resume the simulation
    /// </summary>
    public void ResumeSimulation()
    {
        if (turnManager != null)
        {
            turnManager.Resume();
        }
    }
    
    /// <summary>
    /// Check if the simulation is paused
    /// </summary>
    public bool IsSimulationPaused()
    {
        return turnManager != null && turnManager.IsPaused;
    }
}