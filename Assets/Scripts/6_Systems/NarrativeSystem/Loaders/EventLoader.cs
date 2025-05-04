using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace NarrativeSystem
{
    public class EventLoader
    {
        // Path to JSON file(s) within Resources folder
        private const string EVENT_JSON_PATH = "Events/game_events";
        
        /// <summary>
        /// Load all game events from JSON files in the Resources folder
        /// </summary>
        public static List<GameEvent> LoadEventsFromJSON()
        {
            List<GameEvent> loadedEvents = new List<GameEvent>();
            
            try
            {
                // Load the JSON text asset from Resources
                TextAsset jsonFile = Resources.Load<TextAsset>(EVENT_JSON_PATH);
                
                if (jsonFile != null)
                {
                    // Parse the JSON into an EventCollection
                    EventCollection eventCollection = JsonUtility.FromJson<EventCollection>(jsonFile.text);
                    
                    if (eventCollection != null && eventCollection.events != null)
                    {
                        loadedEvents.AddRange(eventCollection.events);
                        Debug.Log($"Successfully loaded {eventCollection.events.Count} events from JSON");
                    }
                    else
                    {
                        Debug.LogWarning("Failed to parse events from JSON file");
                    }
                }
                else
                {
                    Debug.LogWarning($"Event JSON file not found at 'Resources/{EVENT_JSON_PATH}'");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading events from JSON: {e.Message}");
            }
            
            return loadedEvents;
        }
        
        /// <summary>
        /// Save events to a JSON file (useful for editing/creating events in the editor)
        /// </summary>
        public static void SaveEventsToJSON(List<GameEvent> events)
        {
            try
            {
                // Create an EventCollection to hold the events
                EventCollection eventCollection = new EventCollection { events = events };
                
                // Convert to JSON
                string json = JsonUtility.ToJson(eventCollection, true); // true for pretty print
                
                // Get the full path (outside of Resources for editor-time saving)
                string directoryPath = Application.dataPath + "/Resources/Events";
                string filePath = directoryPath + "/game_events.json";
                
                // Create directory if it doesn't exist
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                
                // Write to file
                File.WriteAllText(filePath, json);
                Debug.Log($"Saved {events.Count} events to JSON at {filePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error saving events to JSON: {e.Message}");
            }
        }
    }
    
    // Wrapper class needed for JsonUtility to deserialize a list
    [System.Serializable]
    public class EventCollection
    {
        public List<GameEvent> events = new List<GameEvent>();
    }
}