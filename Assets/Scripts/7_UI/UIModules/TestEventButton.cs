using UnityEngine;
using UnityEngine.UI;
using NarrativeSystem;

public class TestEventButton : MonoBehaviour
{
    private Button button;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(TriggerTestEvent);
        }
        else
        {
            Debug.LogError("TestEventButton: No Button component found!");
        }
    }
    
    private void TriggerTestEvent()
    {
        if (EventManager.Instance != null)
        {
            Debug.Log("TestEventButton: Triggering random event for testing...");
            EventManager.Instance.TriggerRandomEvent();
        }
        else
        {
            Debug.LogError("TestEventButton: EventManager.Instance is null!");
        }
    }
}