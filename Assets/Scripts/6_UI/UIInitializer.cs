using UnityEngine;
using UI;

/// <summary>
/// Helper class to initialize the UI system via UIManager at runtime.
/// Attach this to a GameObject in your scene to automatically create UI elements.
/// </summary>
public class UIInitializer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool initializeOnStart = true;
    [SerializeField] private bool createSimulationControls = true;
    [SerializeField] private bool createVisualizationControls = true;
    [SerializeField] private bool createStatsDisplay = true;
    
    [Header("UI Positions")]
    [SerializeField] private UIPosition simulationControlsPosition = UIPosition.Bottom;
    [SerializeField] private UIPosition visualizationControlsPosition = UIPosition.Bottom;
    [SerializeField] private UIPosition statsDisplayPosition = UIPosition.Right;
    
    // Reference to the UIManager
    private UIManager uiManager;
    
    private void Start()
    {
        if (initializeOnStart)
        {
            InitializeUI();
        }
    }
    
    /// <summary>
    /// Initialize the UI system and create all selected UI modules
    /// </summary>
    [ContextMenu("Initialize UI")]
    public void InitializeUI()
    {
        // Find or create a UIManager
        uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager == null)
        {
            GameObject uiManagerObj = new GameObject("UIManager");
            uiManager = uiManagerObj.AddComponent<UIManager>();
            Debug.Log("Created new UIManager");
        }
        
        // Initialize the UIManager (create canvas and panels)
        uiManager.Initialize();
        Debug.Log("UIManager initialized");
        
        // Create requested UI modules
        CreateUIModules();
    }
    
    /// <summary>
    /// Create all selected UI modules
    /// </summary>
    private void CreateUIModules()
    {
        if (createSimulationControls)
        {
            CreateSimulationControlsModule();
        }
        
        if (createVisualizationControls)
        {
            CreateVisualizationControlsModule();
        }
        
        if (createStatsDisplay)
        {
            CreateStatsDisplayModule();
        }
    }
    
    /// <summary>
    /// Create the simulation controls module if it doesn't exist
    /// </summary>
    private void CreateSimulationControlsModule()
    {
        // Check if module already exists
        SimulationUIModule existingModule = uiManager.GetModule<SimulationUIModule>();
        if (existingModule != null)
        {
            Debug.Log("SimulationUIModule already exists");
            return;
        }
        
        // Create the module
        SimulationUIModule module = uiManager.CreateModule<SimulationUIModule>(simulationControlsPosition);
        Debug.Log("Created SimulationUIModule");
    }
    
    /// <summary>
    /// Create the visualization controls module if it doesn't exist
    /// </summary>
    private void CreateVisualizationControlsModule()
    {
        // Check if module already exists
        VisualizationUIModule existingModule = uiManager.GetModule<VisualizationUIModule>();
        if (existingModule != null)
        {
            Debug.Log("VisualizationUIModule already exists");
            return;
        }
        
        // Create the module
        VisualizationUIModule module = uiManager.CreateModule<VisualizationUIModule>(visualizationControlsPosition);
        Debug.Log("Created VisualizationUIModule");
    }
    
    /// <summary>
    /// Create the stats display module if it doesn't exist
    /// </summary>
    private void CreateStatsDisplayModule()
    {
        // Check if module already exists
        StatsDisplayUIModule existingModule = uiManager.GetModule<StatsDisplayUIModule>();
        if (existingModule != null)
        {
            Debug.Log("StatsDisplayUIModule already exists");
            return;
        }
        
        // Create the module
        StatsDisplayUIModule module = uiManager.CreateModule<StatsDisplayUIModule>(statsDisplayPosition);
        Debug.Log("Created StatsDisplayUIModule");
    }
}