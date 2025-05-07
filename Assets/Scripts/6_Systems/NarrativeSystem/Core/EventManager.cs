using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Entities;
using Systems;
using Core;
using Managers;
using UI;
using System.Linq; // For LINQ methods

namespace NarrativeSystem
{
    /// <summary>
    /// EventManager is the core controller of the narrative system. It manages the creation, triggering, 
    /// processing, and presentation of narrative events within the game.
    /// 
    /// This class handles:
    /// - Loading events from JSON or creating sample events
    /// - Checking conditions for event triggering
    /// - Managing event flow and sequences
    /// - Processing player choices and their effects
    /// - Interfacing with the UI system to display events
    /// 
    /// The EventManager follows the Singleton pattern to ensure only one instance exists.
    /// </summary>
    public class EventManager : MonoBehaviour
    {
        #region Singleton
        private static EventManager _instance;
        
        /// <summary>
        /// Singleton accessor that creates the EventManager if it doesn't exist
        /// </summary>
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
        [Tooltip("The list of all available events that can be triggered in the game")]
        [SerializeField] private List<GameEvent> availableEvents = new List<GameEvent>();
        
        [Tooltip("How often (in seconds) to check for potential event triggers")]
        [SerializeField] private float checkInterval = 2.0f;
        
        [Header("UI References")]
        [Tooltip("Drag your DialogueUI GameObject with DialogueView component here")]
        [SerializeField] public DialogueView dialogueView;
        
        [Header("Debug")]
        [Tooltip("Enable to show detailed debug logs in the console")]
        [SerializeField] private bool showDebugLogs = true;
        
        [Tooltip("Key to select the first dialogue option")]
        [SerializeField] private KeyCode option1Key = KeyCode.Alpha1;
        
        [Tooltip("Key to select the second dialogue option")]
        [SerializeField] private KeyCode option2Key = KeyCode.Alpha2;
        
        [Tooltip("Key to select the third dialogue option")]
        [SerializeField] private KeyCode option3Key = KeyCode.Alpha3;
        
        [Tooltip("If true, loads events from JSON file. If false, uses hard-coded events")]
        [SerializeField] private bool useJsonEvents = true;
        
        // Core system references
        private EconomicSystem economicSystem; // Reference to the game's economic system for applying effects
        private GameManager gameManager;       // Reference to the game manager for control flow
        
        // Event handling
        private Queue<GameEvent> pendingEvents = new Queue<GameEvent>(); // Queue of events waiting to be displayed
        private GameEvent currentEvent;    // The currently active event being displayed to the player
        private float checkTimer = 0f;     // Timer for the periodic event check
        
        /// <summary>
        /// Initializes the EventManager and sets up required references.
        /// Loads events from JSON if configured, or creates sample events if none exist.
        /// </summary>
        private void Awake()
        {
            // Singleton setup
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            
            // Find references to other systems
            economicSystem = FindFirstObjectByType<EconomicSystem>();
            gameManager = GameManager.Instance;
            
            // Find DialogueView if not set, or create one if none exists
            if (dialogueView == null)
            {
                dialogueView = FindFirstObjectByType<DialogueView>();
                if (dialogueView == null)
                {
                    Debug.Log("No DialogueView found. Creating one automatically.");
                    dialogueView = DialogueView.CreateDialogueSystem();
                }
            }
            
            // Load events from JSON if selected
            if (useJsonEvents)
            {
                LoadEventsFromJson();
            }
            
            // Create sample events if we don't have any after attempting to load
            if (availableEvents.Count == 0)
            {
                CreateSampleEvents();
                
                // Optional: Save the sample events to JSON for future editing
                if (useJsonEvents)
                {
                    SaveEventsToJson();
                }
            }
        }
        
        /// <summary>
        /// Subscribe to necessary events when this component is enabled
        /// </summary>
        private void OnEnable()
        {
            EventBus.Subscribe("EconomicTick", OnEconomicTick);
            
            // Subscribe to dialogue response event
            if (dialogueView != null)
            {
                dialogueView.OnResponseSelected += HandleDialogueResponse;
            }
        }
        
        /// <summary>
        /// Unsubscribe from events when this component is disabled
        /// </summary>
        private void OnDisable()
        {
            EventBus.Unsubscribe("EconomicTick", OnEconomicTick);
            
            // Unsubscribe from dialogue response event
            if (dialogueView != null)
            {
                dialogueView.OnResponseSelected -= HandleDialogueResponse;
            }
        }
        
        /// <summary>
        /// Handles periodic event checking and key input for event choices
        /// </summary>
        private void Update()
        {
            // Regular checking for new events
            checkTimer += Time.deltaTime;
            if (checkTimer >= checkInterval)
            {
                CheckForEvents();
                checkTimer = 0f;
            }
            
            // Handle key presses for choices when an event is active
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
        
        /// <summary>
        /// Loads game events from a JSON file in the Resources folder
        /// </summary>
        private void LoadEventsFromJson()
        {
            List<GameEvent> loadedEvents = EventLoader.LoadEventsFromJSON();
            
            if (loadedEvents.Count > 0)
            {
                availableEvents = loadedEvents;
                
                if (showDebugLogs)
                {
                    // Debug.Log($"Loaded {loadedEvents.Count} events from JSON");
                }
            }
            else
            {
                Debug.LogWarning("No events loaded from JSON, will use sample events instead");
            }
        }
        
        /// <summary>
        /// Saves current events to a JSON file
        /// </summary>
        private void SaveEventsToJson()
        {
            if (availableEvents.Count > 0)
            {
                EventLoader.SaveEventsToJSON(availableEvents);
                
                if (showDebugLogs)
                {
                    Debug.Log($"Saved {availableEvents.Count} events to JSON");
                }
            }
        }
        
        /// <summary>
        /// Handler for the economic tick event from the EventBus
        /// </summary>
        /// <param name="data">Event data (unused)</param>
        private void OnEconomicTick(object data)
        {
            CheckForEvents();
        }
        
        /// <summary>
        /// Checks all available events to see if any should be triggered based on their conditions
        /// </summary>
        public void CheckForEvents()
        {
            if (economicSystem == null) return;
            
            // Get a random region to check (for simplicity)
            var regionIds = economicSystem.GetAllRegionIds().ToList(); // Convert IEnumerable to List
            if (regionIds.Count == 0) return;
            
            string randomRegionId = regionIds[Random.Range(0, regionIds.Count)];
            RegionEntity region = economicSystem.GetRegion(randomRegionId);
            
            // Check all events against current game state
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
        
        /// <summary>
        /// Adds an event to the pending events queue
        /// </summary>
        /// <param name="evt">The event to queue</param>
        private void QueueEvent(GameEvent evt)
        {
            pendingEvents.Enqueue(evt);
        }
        
        /// <summary>
        /// Displays the next event in the queue to the player
        /// Pauses the game while the event is displayed
        /// </summary>
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
            
            // If we have a DialogueView, use it to display the event
            if (dialogueView != null)
            {
                // Convert event choices to string list and effects list for DialogueView
                List<string> choiceTexts = new List<string>();
                List<DialogueView.ResponseEffects> choiceEffects = new List<DialogueView.ResponseEffects>();
                
                foreach (EventChoice choice in currentEvent.choices)
                {
                    choiceTexts.Add(choice.text);
                    
                    // Create a response effects object for each choice
                    DialogueView.ResponseEffects effects = new DialogueView.ResponseEffects
                    {
                        wealthEffect = choice.wealthEffect,
                        productionEffect = choice.productionEffect,
                        laborEffect = choice.laborEffect
                    };
                    
                    choiceEffects.Add(effects);
                }
                
                // Show the dialogue with effects
                dialogueView.ShowDialogueWithEffects(
                    currentEvent.id,
                    currentEvent.title,
                    currentEvent.description,
                    choiceTexts,
                    choiceEffects
                );
                
                if (showDebugLogs)
                {
                    Debug.Log($"Displaying event in UI with effects: {currentEvent.title}");
                }
            }
            else
            {
                // Fallback to console display if DialogueView is not available
                Debug.Log("=================================");
                Debug.Log($"EVENT: {currentEvent.title}");
                Debug.Log($"{currentEvent.description}");
                Debug.Log("---------------------------------");
                
                // Print choices with effects
                for (int i = 0; i < currentEvent.choices.Count; i++)
                {
                    EventChoice choice = currentEvent.choices[i];
                    string effectsText = "";
                    
                    if (choice.wealthEffect != 0)
                        effectsText += $" Wealth: {(choice.wealthEffect > 0 ? "+" : "")}{choice.wealthEffect}";
                    
                    if (choice.productionEffect != 0)
                        effectsText += $" Production: {(choice.productionEffect > 0 ? "+" : "")}{choice.productionEffect}";
                    
                    if (choice.laborEffect != 0)
                        effectsText += $" Labor: {(choice.laborEffect > 0 ? "+" : "")}{choice.laborEffect}";
                    
                    Debug.Log($"[{i + 1}] {choice.text} ({effectsText})");
                }
                
                Debug.Log("=================================");
                Debug.Log("Press the corresponding number key to select an option");
            }
        }
        
        /// <summary>
        /// Callback handler for when the player selects a response in the dialogue UI
        /// </summary>
        /// <param name="eventId">ID of the event the response belongs to</param>
        /// <param name="choiceIndex">Index of the selected choice</param>
        private void HandleDialogueResponse(string eventId, int choiceIndex)
        {
            // Verify this is the current event
            if (currentEvent != null && currentEvent.id == eventId)
            {
                ProcessChoice(choiceIndex);
            }
        }
        
        /// <summary>
        /// Processes the player's choice and applies its effects to the game state
        /// </summary>
        /// <param name="choiceIndex">Index of the chosen option</param>
        public void ProcessChoice(int choiceIndex)
        {
            if (currentEvent == null || choiceIndex < 0 || choiceIndex >= currentEvent.choices.Count)
            {
                return;
            }
            
            EventChoice choice = currentEvent.choices[choiceIndex];
            
            // Show the result
            if (showDebugLogs)
            {
                Debug.Log("=================================");
                Debug.Log($"RESULT: {choice.result}");
            }
            
            // Apply effects to a random region (this could be enhanced to target specific regions)
            if (economicSystem != null)
            {
                var regionIds = economicSystem.GetAllRegionIds().ToList(); // Convert IEnumerable to List
                if (regionIds.Count > 0)
                {
                    string regionId = regionIds[Random.Range(0, regionIds.Count)];
                    RegionEntity region = economicSystem.GetRegion(regionId);
                    
                    if (region != null)
                    {
                        // Apply wealth effect
                        if (choice.wealthEffect != 0)
                        {
                            // Update using the Economy component instead of direct property access
                            region.Economy.UpdateWealth(choice.wealthEffect);
                            
                            if (showDebugLogs)
                            {
                                Debug.Log($"Wealth {(choice.wealthEffect > 0 ? "+" : "")}{choice.wealthEffect} (new total: {region.Economy.Wealth})");
                            }
                        }
                        
                        // Apply production effect
                        if (choice.productionEffect != 0)
                        {
                            // Update using the ProductionComp instead of direct property access
                            region.ProductionComp.UpdateProduction(choice.productionEffect);
                            
                            if (showDebugLogs)
                            {
                                Debug.Log($"Production {(choice.productionEffect > 0 ? "+" : "")}{choice.productionEffect} (new total: {region.ProductionComp.Production})");
                            }
                        }
                        
                        // Apply labor effect
                        if (choice.laborEffect != 0)
                        {
                            // Update using the PopulationComp instead of direct property access
                            region.PopulationComp.UpdateLaborAvailable(choice.laborEffect);
                            
                            if (showDebugLogs)
                            {
                                Debug.Log($"Labor {(choice.laborEffect > 0 ? "+" : "")}{choice.laborEffect} (new total: {region.PopulationComp.LaborAvailable})");
                            }
                        }
                        
                        // Update the region in the economic system
                        economicSystem.UpdateRegion(region);
                    }
                }
            }
            
            if (showDebugLogs)
            {
                Debug.Log("=================================");
            }
            
            // Check for next event in a chain
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

        /// <summary>
        /// Helper method to resume the game simulation if there are no active or pending events
        /// </summary>
        private void ResumeSimulationIfNoEvents()
        {
            if (pendingEvents.Count == 0 && currentEvent == null && gameManager != null)
            {
                gameManager.ResumeSimulation();
                if (showDebugLogs)
                {
                    Debug.Log("No more events, resuming simulation");
                }
            }
        }
        
        /// <summary>
        /// Creates a set of sample narrative events for the game
        /// Used when no events are loaded from JSON or as initial examples
        /// </summary>
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
        
        #region Public Test Methods
        
        /// <summary>
        /// Triggers a random event for testing purposes
        /// Useful for debugging or demonstrating the event system
        /// </summary>
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
        
        /// <summary>
        /// Resets all events to their untriggered state
        /// Useful for testing or starting a new game
        /// </summary>
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
        
        #if UNITY_EDITOR
        /// <summary>
        /// Editor-only method to save current events to JSON
        /// </summary>
        public void SaveCurrentEventsToJson()
        {
            SaveEventsToJson();
        }
        #endif
        #endregion

        #region Debug API
        
        /// <summary>
        /// Gets a copy of all available events for debugging and inspection
        /// </summary>
        /// <returns>A copy of the available events list</returns>
        public List<GameEvent> GetAvailableEvents()
        {
            return new List<GameEvent>(availableEvents);
        }
        
        /// <summary>
        /// Checks if JSON events are being used instead of hardcoded events
        /// </summary>
        /// <returns>True if using JSON events, false otherwise</returns>
        public bool GetUseJsonEvents()
        {
            return useJsonEvents;
        }
        
        /// <summary>
        /// Sets whether to use JSON events or hardcoded events
        /// </summary>
        /// <param name="value">True to use JSON events, false to use hardcoded events</param>
        public void SetUseJsonEvents(bool value)
        {
            if (value != useJsonEvents)
            {
                useJsonEvents = value;
                
                // Clear all events
                availableEvents.Clear();
                pendingEvents.Clear();
                currentEvent = null;
                
                // Load appropriate events
                if (useJsonEvents)
                {
                    LoadEventsFromJson();
                }
                
                // If we have no events after attempting to load, create sample events
                if (availableEvents.Count == 0)
                {
                    CreateSampleEvents();
                    
                    // Optional: Save the sample events to JSON for future editing
                    if (useJsonEvents)
                    {
                        SaveEventsToJson();
                    }
                }
                
                if (showDebugLogs)
                {
                    Debug.Log($"Switched to {(useJsonEvents ? "JSON" : "hardcoded")} events. {availableEvents.Count} events loaded.");
                }
            }
        }
        
        /// <summary>
        /// Triggers a specific event for testing and debugging purposes
        /// </summary>
        /// <param name="evt">The event to trigger</param>
        public void TriggerSpecificEvent(GameEvent evt)
        {
            if (evt == null) return;
            
            // Reset the event's triggered state to make sure it can be displayed
            evt.hasTriggered = false;
            
            // If another event is active, clear it to make way for the debug event
            if (currentEvent != null)
            {
                Debug.Log($"Canceling current event to display debug event: {evt.title}");
                currentEvent = null;
            }
            
            // Add to pending events queue
            QueueEvent(evt);
            
            // Force display the event immediately
            DisplayNextEvent();
            
            if (showDebugLogs)
            {
                Debug.Log($"Manually triggered event: {evt.title} (ID: {evt.id})");
            }
        }
        
        #endregion
    }
}