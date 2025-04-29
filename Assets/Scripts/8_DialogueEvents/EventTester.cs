using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventTester : MonoBehaviour
{
    [Header("Debug Controls")]
    [SerializeField] private KeyCode triggerRandomEventKey = KeyCode.E;
    [SerializeField] private KeyCode resetEventsKey = KeyCode.R;
    
    private EventManager eventManager;
    
    private void Start()
    {
        eventManager = EventManager.Instance;
        Debug.Log("Event Tester loaded. Press 'E' to trigger a random event, 'R' to reset all events.");
        Debug.Log("When an event appears, press 1, 2, or 3 to select options.");
    }
    
    private void Update()
    {
        // Trigger a random event
        if (Input.GetKeyDown(triggerRandomEventKey))
        {
            if (eventManager != null)
            {
                eventManager.TriggerRandomEvent();
            }
        }
        
        // Reset all events
        if (Input.GetKeyDown(resetEventsKey))
        {
            if (eventManager != null)
            {
                eventManager.ResetAllEvents();
            }
        }
    }
}