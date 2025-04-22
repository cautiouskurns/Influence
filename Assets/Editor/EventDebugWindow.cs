using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace UI.Editor
{
    public class EventDebugWindow : EditorWindow
    {
        private List<GameEvent> events = new List<GameEvent>();
        private Vector2 scrollPosition;
        private bool showEvents = true;
        private string searchQuery = "";
        private EventManager eventManager;
        private bool useJsonEvents = true;
        private bool showTriggered = true;
        private int selectedEventIndex = -1;
        
        [MenuItem("Influence/Event Debug")]
        public static void ShowWindow()
        {
            GetWindow<EventDebugWindow>("Event Debug");
        }
        
        private void OnEnable()
        {
            eventManager = FindFirstObjectByType<EventManager>();
            if (eventManager != null)
            {
                useJsonEvents = eventManager.GetUseJsonEvents();
                LoadEvents();
            }
        }
        
        private void OnGUI()
        {
            if (eventManager == null)
            {
                eventManager = FindFirstObjectByType<EventManager>();
                if (eventManager == null)
                {
                    EditorGUILayout.HelpBox("No EventManager found in the scene. Enter play mode or add an EventManager to your scene.", MessageType.Warning);
                    if (GUILayout.Button("Create EventManager"))
                    {
                        CreateEventManager();
                    }
                    return;
                }
            }
            
            EditorGUILayout.BeginVertical();
            
            // Toolbar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                LoadEvents();
            }
            
            if (GUILayout.Button("Reset All", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog("Reset Events", "Reset all events' triggered state?", "Yes", "No"))
                {
                    ResetAllEvents();
                }
            }
            
            GUILayout.FlexibleSpace();
            
            bool tempUseJsonEvents = EditorGUILayout.ToggleLeft("Use JSON Events", useJsonEvents, GUILayout.Width(120));
            if (tempUseJsonEvents != useJsonEvents)
            {
                useJsonEvents = tempUseJsonEvents;
                if (Application.isPlaying && eventManager != null)
                {
                    eventManager.SetUseJsonEvents(useJsonEvents);
                    LoadEvents();
                }
            }
            
            showTriggered = EditorGUILayout.ToggleLeft("Show Triggered", showTriggered, GUILayout.Width(120));
            
            EditorGUILayout.EndHorizontal();
            
            // Search bar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
            string newSearchQuery = EditorGUILayout.TextField(searchQuery);
            if (newSearchQuery != searchQuery)
            {
                searchQuery = newSearchQuery;
            }
            
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                searchQuery = "";
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Display events count
            EditorGUILayout.LabelField($"Found {events.Count} events. {events.Count(e => e.hasTriggered)} triggered.");
            
            // Events list
            showEvents = EditorGUILayout.Foldout(showEvents, "Events", true);
            
            if (showEvents)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                List<GameEvent> filteredEvents = events;
                
                // Apply search filter
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    filteredEvents = events.Where(e => 
                        e.id.ToLower().Contains(searchQuery.ToLower()) || 
                        e.title.ToLower().Contains(searchQuery.ToLower()) ||
                        e.description.ToLower().Contains(searchQuery.ToLower())
                    ).ToList();
                }
                
                // Apply triggered filter
                if (!showTriggered)
                {
                    filteredEvents = filteredEvents.Where(e => !e.hasTriggered).ToList();
                }
                
                // Display filtered events
                for (int i = 0; i < filteredEvents.Count; i++)
                {
                    GameEvent evt = filteredEvents[i];
                    DrawEventItem(evt, i == selectedEventIndex);
                }
                
                EditorGUILayout.EndScrollView();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawEventItem(GameEvent evt, bool isSelected)
        {
            GUI.backgroundColor = evt.hasTriggered ? new Color(0.8f, 0.6f, 0.6f) : new Color(0.7f, 0.9f, 0.7f);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.BeginHorizontal();
            
            // Title and ID
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(evt.title, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"ID: {evt.id}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            
            // Trigger info
            EditorGUILayout.BeginVertical(GUILayout.Width(150));
            EditorGUILayout.LabelField($"Condition: {evt.conditionType}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Value: {evt.conditionValue}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField(evt.hasTriggered ? "Status: Triggered" : "Status: Ready", 
                evt.hasTriggered ? EditorStyles.boldLabel : EditorStyles.miniBoldLabel);
            EditorGUILayout.EndVertical();
            
            // Buttons
            EditorGUILayout.BeginVertical(GUILayout.Width(140));
            
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Trigger"))
                {
                    TriggerEvent(evt);
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("Trigger (Play Mode Only)");
                EditorGUI.EndDisabledGroup();
            }
            
            if (GUILayout.Button(evt.hasTriggered ? "Mark as Not Triggered" : "Mark as Triggered"))
            {
                evt.hasTriggered = !evt.hasTriggered;
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            
            // Description (collapsible)
            if (isSelected)
            {
                EditorGUILayout.LabelField("Description:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(evt.description, EditorStyles.wordWrappedLabel);
                
                EditorGUILayout.Space();
                
                // Choices
                EditorGUILayout.LabelField($"Choices ({evt.choices.Count}):", EditorStyles.boldLabel);
                
                for (int i = 0; i < evt.choices.Count; i++)
                {
                    EventChoice choice = evt.choices[i];
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    EditorGUILayout.LabelField($"Choice {i + 1}: {choice.text}", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"Result: {choice.result}", EditorStyles.wordWrappedLabel);
                    
                    EditorGUILayout.BeginHorizontal();
                    
                    if (choice.wealthEffect != 0)
                        EditorGUILayout.LabelField($"Wealth: {(choice.wealthEffect > 0 ? "+" : "")}{choice.wealthEffect}", EditorStyles.miniLabel);
                    
                    if (choice.productionEffect != 0)
                        EditorGUILayout.LabelField($"Production: {(choice.productionEffect > 0 ? "+" : "")}{choice.productionEffect}", EditorStyles.miniLabel);
                    
                    if (choice.laborEffect != 0)
                        EditorGUILayout.LabelField($"Labor: {(choice.laborEffect > 0 ? "+" : "")}{choice.laborEffect}", EditorStyles.miniLabel);
                    
                    EditorGUILayout.EndHorizontal();
                    
                    if (!string.IsNullOrEmpty(choice.nextEventId))
                    {
                        EditorGUILayout.LabelField($"Next Event: {choice.nextEventId}", EditorStyles.miniLabel);
                    }
                    
                    EditorGUILayout.EndVertical();
                }
            }
            
            if (GUILayout.Button(isSelected ? "▲ Hide Details" : "▼ Show Details"))
            {
                selectedEventIndex = isSelected ? -1 : events.IndexOf(evt);
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
        }
        
        private void LoadEvents()
        {
            if (eventManager != null)
            {
                events = eventManager.GetAvailableEvents();
            }
            else
            {
                events = new List<GameEvent>();
            }
        }
        
        private void TriggerEvent(GameEvent evt)
        {
            if (eventManager != null && Application.isPlaying)
            {
                eventManager.TriggerSpecificEvent(evt);
                Repaint(); // Refresh the window
            }
        }
        
        private void ResetAllEvents()
        {
            if (eventManager != null)
            {
                if (Application.isPlaying)
                {
                    eventManager.ResetAllEvents();
                }
                else
                {
                    foreach (var evt in events)
                    {
                        evt.hasTriggered = false;
                    }
                }
                
                LoadEvents(); // Refresh events
            }
        }
        
        private void CreateEventManager()
        {
            GameObject managerObj = new GameObject("EventManager");
            eventManager = managerObj.AddComponent<EventManager>();
            Selection.activeGameObject = managerObj;
            EditorGUIUtility.PingObject(managerObj);
        }
    }
}