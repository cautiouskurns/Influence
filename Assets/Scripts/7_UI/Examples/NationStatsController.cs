using UnityEngine;
using Entities;
using UI;
using Core;
using Managers;

/// <summary>
/// Example controller that demonstrates how to display nation stats
/// when a nation is selected in the game.
/// </summary>
public class NationStatsController : MonoBehaviour
{
    // Singleton implementation
    private static NationStatsController _instance;
    public static NationStatsController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<NationStatsController>();
                
                if (_instance == null)
                {
                    GameObject controllerObj = new GameObject("NationStatsController");
                    _instance = controllerObj.AddComponent<NationStatsController>();
                }
            }
            
            return _instance;
        }
    }
    
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private NationStatsPanel statsPanel;
    [SerializeField] private NationEntity currentSelectedNation; // Manual reference to the selected nation
    
    private NationEntity currentlyDisplayedNation;
    
    private void Awake()
    {
        // Ensure singleton behavior
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
    }
    
    private void Start()
    {
        // Find references if not set
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
        
        // Always use the singleton instance of NationStatsPanel
        statsPanel = NationStatsPanel.Instance;
        
        // Display initial nation if set
        if (currentSelectedNation != null)
        {
            DisplayNationStats(currentSelectedNation);
        }
        
        // Subscribe to turn change events
        EventBus.Subscribe("EconomicTick", OnTurnChanged);
    }
    
    private void OnDestroy()
    {
        // Unsubscribe when destroyed
        EventBus.Unsubscribe("EconomicTick", OnTurnChanged);
    }
    
    // Handle turn changes
    private void OnTurnChanged(object data)
    {
        // Refresh the currently displayed nation stats if there is one
        if (currentlyDisplayedNation != null)
        {
            DisplayNationStats(currentlyDisplayedNation);
            Debug.Log($"Updated nation stats for {currentlyDisplayedNation.Name} after turn change");
        }
    }
    
    // You can call this method directly from UI buttons or events
    public void DisplayNationStats(NationEntity nation)
    {
        if (nation != null && statsPanel != null)
        {
            currentSelectedNation = nation;
            currentlyDisplayedNation = nation;
            statsPanel.SetNation(nation);
        }
    }
    
    // Set the currently selected nation
    public void SetSelectedNation(NationEntity nation)
    {
        currentSelectedNation = nation;
        DisplayNationStats(nation);
    }
}