using UnityEngine;
using System.Collections.Generic;
using Entities;
using Systems;

// Define a simple game event
[System.Serializable]
public class GameEvent
{
    public string id;
    public string title;
    public string description;
    public bool hasTriggered = false;
    
    // Simple condition for when this event should trigger
    public ConditionType conditionType;
    public float conditionValue;
    
    // Choices available to the player
    public List<EventChoice> choices = new List<EventChoice>();
    
    // Check if the condition is met
    public bool IsConditionMet(EconomicSystem economicSystem, RegionEntity region)
    {
        if (region == null) return false;
        
        switch (conditionType)
        {
            case ConditionType.TurnNumber:
                // Trigger on specific turn number
                return GameManager.Instance.GetCurrentTurn() == (int)conditionValue;
                
            case ConditionType.MinimumWealth:
                // Trigger when region's wealth exceeds the value
                return region.Wealth >= conditionValue;
                
            case ConditionType.MaximumWealth:
                // Trigger when region's wealth is below the value
                return region.Wealth <= conditionValue;
                
            case ConditionType.MinimumProduction:
                // Trigger when region's production exceeds the value
                return region.Production >= conditionValue;
                
            case ConditionType.MaximumProduction:
                // Trigger when region's production is below the value
                return region.Production <= conditionValue;
                
            default:
                return false;
        }
    }
}

// Simple enum for condition types
public enum ConditionType
{
    TurnNumber,
    MinimumWealth,
    MaximumWealth,
    MinimumProduction,
    MaximumProduction
}

// Define a choice for an event
[System.Serializable]
public class EventChoice
{
    public string text;
    public string result;
    
    // Simple effects
    public int wealthEffect;
    public int productionEffect;
    public int laborEffect;
    
    // Optional link to next event
    public string nextEventId;
}