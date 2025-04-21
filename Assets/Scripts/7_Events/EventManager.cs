using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Entities;
using Systems;
using Core;
using Managers;

public class EventManager : MonoBehaviour
{
    #region Singleton
    private static EventManager _instance;
    
    public static EventManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<EventManager>();
                
                if (_instance == null)
                {
                    GameObject go = new GameObject("EventManager");
                    _instance = go.AddComponent<EventManager>();
                }
            }
            
            return _instance;
        }
    }
    #endregion
    
    [Header("Event Settings")]
    [SerializeField] private List<GameEvent> availableEvents = new List<GameEvent>();
    [SerializeField] private float checkInterval = 2.0f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private KeyCode option1Key = KeyCode.Alpha1;
    [SerializeField] private KeyCode option2Key = KeyCode.Alpha2;
    [SerializeField] private KeyCode option3Key = KeyCode.Alpha3;
    
    // Core system references
    private EconomicSystem economicSystem;
    private GameManager gameManager;
    
    // Event handling
    private Queue<GameEvent> pendingEvents = new Queue<GameEvent>();
    private GameEvent currentEvent;
    private float checkTimer = 0f;
    
    private void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        
        // Find references
        economicSystem = FindFirstObjectByType<EconomicSystem>();
        gameManager = GameManager.Instance;
        
        // Create sample events if we don't have any
        if (availableEvents.Count == 0)
        {
            CreateSampleEvents();
        }
    }
    
    private void OnEnable()
    {
        EventBus.Subscribe("EconomicTick", OnEconomicTick);
    }
    
    private void OnDisable()
    {
        EventBus.Unsubscribe("EconomicTick", OnEconomicTick);
    }
    
    private void Update()
    {
        // Regular checking for new events
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            CheckForEvents();
            checkTimer = 0f;
        }
        
        // Handle key presses for choices
        if (currentEvent != null)
        {
            if (Input.GetKeyDown(option1Key) && currentEvent.choices.Count >= 1)
            {
                ProcessChoice(0);
            }
            else if (Input.GetKeyDown(option2Key) && currentEvent.choices.Count >= 2)
            {
                ProcessChoice(1);
            }
            else if (Input.GetKeyDown(option3Key) && currentEvent.choices.Count >= 3)
            {
                ProcessChoice(2);
            }
        }
    }
    
    private void OnEconomicTick(object data)
    {
        CheckForEvents();
    }
    
    public void CheckForEvents()
    {
        if (economicSystem == null) return;
        
        // Get a random region to check (for simplicity)
        List<string> regionIds = economicSystem.GetAllRegionIds();
        if (regionIds.Count == 0) return;
        
        string randomRegionId = regionIds[Random.Range(0, regionIds.Count)];
        RegionEntity region = economicSystem.GetRegion(randomRegionId);
        
        // Check all events
        foreach (GameEvent evt in availableEvents)
        {
            if (!evt.hasTriggered && evt.IsConditionMet(economicSystem, region))
            {
                QueueEvent(evt);
                evt.hasTriggered = true;
                
                if (showDebugLogs)
                {
                    Debug.Log($"Event triggered: {evt.title} (ID: {evt.id})");
                }
            }
        }
        
        // Display pending event if none is active
        if (currentEvent == null && pendingEvents.Count > 0)
        {
            DisplayNextEvent();
        }
    }
    
    private void QueueEvent(GameEvent evt)
    {
        pendingEvents.Enqueue(evt);
    }
    
    private void DisplayNextEvent()
    {
        if (pendingEvents.Count == 0) return;
        
        currentEvent = pendingEvents.Dequeue();
        
        // Pause the game when an event is displayed
        if (gameManager != null)
        {
            gameManager.PauseSimulation();
            Debug.Log("Game paused due to event");
        }
        
        // Print event information to console
        Debug.Log("=================================");
        Debug.Log($"EVENT: {currentEvent.title}");
        Debug.Log($"{currentEvent.description}");
        Debug.Log("---------------------------------");
        
        // Print choices
        for (int i = 0; i < currentEvent.choices.Count; i++)
        {
            EventChoice choice = currentEvent.choices[i];
            Debug.Log($"[{i + 1}] {choice.text}");
        }
        
        Debug.Log("=================================");
        Debug.Log("Press the corresponding number key to select an option");
    }
    
    public void ProcessChoice(int choiceIndex)
    {
        if (currentEvent == null || choiceIndex < 0 || choiceIndex >= currentEvent.choices.Count)
        {
            return;
        }
        
        EventChoice choice = currentEvent.choices[choiceIndex];
        
        // Show the result
        Debug.Log("=================================");
        Debug.Log($"RESULT: {choice.result}");
        
        // Apply effects
        if (economicSystem != null)
        {
            List<string> regionIds = economicSystem.GetAllRegionIds();
            if (regionIds.Count > 0)
            {
                string regionId = regionIds[Random.Range(0, regionIds.Count)];
                RegionEntity region = economicSystem.GetRegion(regionId);
                
                if (region != null)
                {
                    // Apply wealth effect
                    if (choice.wealthEffect != 0)
                    {
                        region.Wealth += choice.wealthEffect;
                        Debug.Log($"Wealth {(choice.wealthEffect > 0 ? "+" : "")}{choice.wealthEffect} (new total: {region.Wealth})");
                    }
                    
                    // Apply production effect
                    if (choice.productionEffect != 0)
                    {
                        region.Production += choice.productionEffect;
                        Debug.Log($"Production {(choice.productionEffect > 0 ? "+" : "")}{choice.productionEffect} (new total: {region.Production})");
                    }
                    
                    // Apply labor effect
                    if (choice.laborEffect != 0)
                    {
                        region.LaborAvailable += choice.laborEffect;
                        Debug.Log($"Labor {(choice.laborEffect > 0 ? "+" : "")}{choice.laborEffect} (new total: {region.LaborAvailable})");
                    }
                    
                    // Update the region in the economic system
                    economicSystem.UpdateRegion(region);
                }
            }
        }
        
        Debug.Log("=================================");
        
        // Check for next event
        string nextEventId = choice.nextEventId;
        currentEvent = null;
        
        // If there's a next event, try to find and queue it
        if (!string.IsNullOrEmpty(nextEventId))
        {
            GameEvent nextEvent = availableEvents.Find(e => e.id == nextEventId);
            if (nextEvent != null)
            {
                nextEvent.hasTriggered = false;  // Reset so it can trigger again
                QueueEvent(nextEvent);
                DisplayNextEvent();
            }
            else
            {
                // If no next event found, resume the simulation
                ResumeSimulationIfNoEvents();
            }
        }
        else if (pendingEvents.Count > 0)
        {
            // If there are other pending events, show the next one
            DisplayNextEvent();
        }
        else
        {
            // If no more events, resume the simulation
            ResumeSimulationIfNoEvents();
        }
    }

    // Helper method to resume the simulation if there are no pending events
    private void ResumeSimulationIfNoEvents()
    {
        if (pendingEvents.Count == 0 && currentEvent == null && gameManager != null)
        {
            gameManager.ResumeSimulation();
            Debug.Log("No more events, resuming simulation");
        }
    }
    
    private void CreateSampleEvents()
    {
        // Event 1: Resource Shortage (triggers when production is below 80)
        GameEvent resourceEvent = new GameEvent
        {
            id = "resource_shortage",
            title = "Resource Shortage",
            description = "Your economic advisor reports a serious shortage of essential resources.",
            conditionType = ConditionType.MaximumProduction,
            conditionValue = 80f
        };
        
        // Add choices
        resourceEvent.choices.Add(new EventChoice
        {
            text = "Import resources from neighboring regions",
            result = "You negotiate favorable import terms with neighboring regions.",
            wealthEffect = -500,
            productionEffect = 20,
            nextEventId = "economic_reform"
        });
        
        resourceEvent.choices.Add(new EventChoice
        {
            text = "Divert labor to resource extraction",
            result = "You order an emergency reallocation of labor to increase resource production.",
            wealthEffect = -10,
            productionEffect = 15,
            laborEffect = -5
        });
        
        resourceEvent.choices.Add(new EventChoice
        {
            text = "Do nothing and hope the market resolves the shortage",
            result = "You decide to let market forces handle the shortage naturally.",
            wealthEffect = -20,
            productionEffect = -10
        });
        
        availableEvents.Add(resourceEvent);
        
        // Event 2: Economic Reform (triggers on turn 5)
        GameEvent economicEvent = new GameEvent
        {
            id = "economic_reform",
            title = "Economic Reform Proposal",
            description = "Your finance minister has presented a series of possible economic reforms aimed at increasing long-term growth.",
            conditionType = ConditionType.TurnNumber,
            conditionValue = 5f
        };
        
        // Add choices
        economicEvent.choices.Add(new EventChoice
        {
            text = "Implement market liberalization reforms",
            result = "You begin a program of market liberalization, reducing regulations and trade barriers.",
            wealthEffect = 30,
            productionEffect = 15,
            laborEffect = -5
        });
        
        economicEvent.choices.Add(new EventChoice
        {
            text = "Focus on industrial modernization",
            result = "You invest heavily in modernizing industrial infrastructure and production methods.",
            wealthEffect = -80,
            productionEffect = 50,
            laborEffect = 10
        });
        
        economicEvent.choices.Add(new EventChoice
        {
            text = "Implement worker protection and welfare programs",
            result = "You strengthen worker protections and expand social safety nets.",
            wealthEffect = -60,
            productionEffect = -10,
            laborEffect = 20
        });
        
        availableEvents.Add(economicEvent);
        
        // Event 3: Population Growth (triggers when wealth exceeds 500)
        GameEvent populationEvent = new GameEvent
        {
            id = "population_growth",
            title = "Population Boom",
            description = "Your region's prosperity has attracted many new residents seeking opportunities.",
            conditionType = ConditionType.MinimumWealth,
            conditionValue = 500f
        };
        
        // Add choices
        populationEvent.choices.Add(new EventChoice
        {
            text = "Expand housing and infrastructure",
            result = "You invest in new housing developments and infrastructure to accommodate growth.",
            wealthEffect = -100,
            productionEffect = 10,
            laborEffect = 30
        });
        
        populationEvent.choices.Add(new EventChoice
        {
            text = "Implement strict migration controls",
            result = "You establish controls to limit the influx of new residents.",
            wealthEffect = -20,
            productionEffect = -5,
            laborEffect = 5
        });
        
        populationEvent.choices.Add(new EventChoice
        {
            text = "Let existing systems handle the growth",
            result = "You allow existing systems to adapt to the population changes.",
            wealthEffect = 0,
            productionEffect = 0,
            laborEffect = 15
        });
        
        availableEvents.Add(populationEvent);
    }
    
    // Public methods for manual testing
    
    public void TriggerRandomEvent()
    {
        if (availableEvents.Count == 0) return;
        
        int index = Random.Range(0, availableEvents.Count);
        GameEvent randomEvent = availableEvents[index];
        randomEvent.hasTriggered = false;
        
        Debug.Log($"Manually triggering random event: {randomEvent.title}");
        QueueEvent(randomEvent);
        
        if (currentEvent == null)
        {
            DisplayNextEvent();
        }
    }
    
    public void ResetAllEvents()
    {
        foreach (var evt in availableEvents)
        {
            evt.hasTriggered = false;
        }
        
        pendingEvents.Clear();
        currentEvent = null;
        
        Debug.Log("All events have been reset.");
    }
}