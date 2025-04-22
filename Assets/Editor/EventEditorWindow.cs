using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Managers;

#if UNITY_EDITOR
namespace UI.Editor
{
    public class EventEditorWindow : EditorWindow
    {
        private List<GameEvent> events = new List<GameEvent>();
        private Vector2 scrollPosition;
        private bool showEvents = true;
        private int selectedEventIndex = -1;
        private int selectedChoiceIndex = -1;
        private bool isDirty = false;
        
        [MenuItem("Influence/Event Editor")]
        public static void ShowWindow()
        {
            GetWindow<EventEditorWindow>("Event Editor");
        }
        
        private void OnEnable()
        {
            LoadEvents();
        }
        
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            
            // Toolbar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("New Event", EditorStyles.toolbarButton))
            {
                CreateNewEvent();
            }
            if (GUILayout.Button("Save Events", EditorStyles.toolbarButton))
            {
                SaveEvents();
            }
            if (GUILayout.Button("Load Events", EditorStyles.toolbarButton))
            {
                LoadEvents();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            // Event List and Editor
            EditorGUILayout.BeginHorizontal();
            
            // Left panel - Event list
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            showEvents = EditorGUILayout.Foldout(showEvents, "Events", true);
            
            if (showEvents)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                for (int i = 0; i < events.Count; i++)
                {
                    GameEvent evt = events[i];
                    EditorGUILayout.BeginHorizontal();
                    
                    bool isSelected = i == selectedEventIndex;
                    bool newIsSelected = EditorGUILayout.ToggleLeft(evt.title, isSelected);
                    
                    if (newIsSelected && !isSelected)
                    {
                        selectedEventIndex = i;
                        selectedChoiceIndex = -1;
                    }
                    else if (!newIsSelected && isSelected)
                    {
                        selectedEventIndex = -1;
                    }
                    
                    if (GUILayout.Button("×", GUILayout.Width(20)))
                    {
                        if (EditorUtility.DisplayDialog("Delete Event", 
                            $"Are you sure you want to delete the event '{evt.title}'?", 
                            "Delete", "Cancel"))
                        {
                            events.RemoveAt(i);
                            isDirty = true;
                            i--;
                        }
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
            }
            
            EditorGUILayout.EndVertical();
            
            // Divider
            EditorGUILayout.BeginVertical(GUILayout.Width(1));
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(GUILayout.Width(1), GUILayout.ExpandHeight(true)), 
                new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.EndVertical();
            
            // Right panel - Event editor
            EditorGUILayout.BeginVertical();
            
            if (selectedEventIndex >= 0 && selectedEventIndex < events.Count)
            {
                EditEvent(events[selectedEventIndex]);
            }
            else
            {
                EditorGUILayout.LabelField("Select an event to edit", EditorStyles.centeredGreyMiniLabel);
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            // Check for changes and mark dirty
            if (GUI.changed)
            {
                isDirty = true;
            }
            
            // Show warning when unsaved changes
            if (isDirty)
            {
                EditorGUILayout.HelpBox("You have unsaved changes.", MessageType.Warning);
            }
        }
        
        private void CreateNewEvent()
        {
            GameEvent newEvent = new GameEvent
            {
                id = "event_" + System.DateTime.Now.Ticks,
                title = "New Event",
                description = "Event description goes here",
                conditionType = ConditionType.TurnNumber,
                conditionValue = 1,
                choices = new List<EventChoice>()
            };
            
            // Add a default choice
            newEvent.choices.Add(new EventChoice
            {
                text = "Default choice",
                result = "Result of this choice",
                wealthEffect = 0,
                productionEffect = 0,
                laborEffect = 0
            });
            
            events.Add(newEvent);
            selectedEventIndex = events.Count - 1;
            isDirty = true;
        }
        
        private void EditEvent(GameEvent evt)
        {
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.LabelField("Event Properties", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            
            // Basic properties
            evt.id = EditorGUILayout.TextField("ID", evt.id);
            evt.title = EditorGUILayout.TextField("Title", evt.title);
            
            // Description with text area
            EditorGUILayout.LabelField("Description");
            evt.description = EditorGUILayout.TextArea(evt.description, GUILayout.Height(60));
            
            // Condition
            evt.conditionType = (ConditionType)EditorGUILayout.EnumPopup("Condition Type", evt.conditionType);
            evt.conditionValue = EditorGUILayout.FloatField("Condition Value", evt.conditionValue);
            
            // Has triggered
            evt.hasTriggered = EditorGUILayout.Toggle("Has Triggered", evt.hasTriggered);
            
            GUILayout.Space(10);
            
            // Choices
            EditorGUILayout.LabelField("Choices", EditorStyles.boldLabel);
            
            for (int i = 0; i < evt.choices.Count; i++)
            {
                EventChoice choice = evt.choices[i];
                bool isExpanded = i == selectedChoiceIndex;
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.BeginHorizontal();
                bool newIsExpanded = EditorGUILayout.Foldout(isExpanded, $"Choice {i + 1}", true);
                
                if (newIsExpanded != isExpanded)
                {
                    selectedChoiceIndex = newIsExpanded ? i : -1;
                }
                
                if (GUILayout.Button("×", GUILayout.Width(20)))
                {
                    if (EditorUtility.DisplayDialog("Delete Choice", 
                        "Are you sure you want to delete this choice?", 
                        "Delete", "Cancel"))
                    {
                        evt.choices.RemoveAt(i);
                        i--;
                        if (selectedChoiceIndex == i)
                        {
                            selectedChoiceIndex = -1;
                        }
                        isDirty = true;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        continue;
                    }
                }
                
                EditorGUILayout.EndHorizontal();
                
                // Display choice properties when expanded
                if (isExpanded)
                {
                    EditorGUI.indentLevel++;
                    
                    // Choice text
                    EditorGUILayout.LabelField("Text");
                    choice.text = EditorGUILayout.TextArea(choice.text, GUILayout.Height(40));
                    
                    // Result text
                    EditorGUILayout.LabelField("Result");
                    choice.result = EditorGUILayout.TextArea(choice.result, GUILayout.Height(40));
                    
                    // Effects
                    EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);
                    choice.wealthEffect = EditorGUILayout.IntField("Wealth Effect", choice.wealthEffect);
                    choice.productionEffect = EditorGUILayout.IntField("Production Effect", choice.productionEffect);
                    choice.laborEffect = EditorGUILayout.IntField("Labor Effect", choice.laborEffect);
                    
                    // Next event
                    choice.nextEventId = EditorGUILayout.TextField("Next Event ID", choice.nextEventId);
                    
                    // Show available event IDs for reference
                    if (GUILayout.Button("Select Next Event"))
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("<None>"), string.IsNullOrEmpty(choice.nextEventId), 
                            () => { choice.nextEventId = ""; isDirty = true; });
                        
                        foreach (GameEvent e in events)
                        {
                            if (e != evt) // Don't link to self
                            {
                                menu.AddItem(new GUIContent(e.title + " (" + e.id + ")"), 
                                    choice.nextEventId == e.id, 
                                    () => { choice.nextEventId = e.id; isDirty = true; });
                            }
                        }
                        
                        menu.ShowAsContext();
                    }
                    
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.EndVertical();
            }
            
            if (GUILayout.Button("Add Choice"))
            {
                evt.choices.Add(new EventChoice
                {
                    text = "New choice",
                    result = "Result of this choice",
                    wealthEffect = 0,
                    productionEffect = 0,
                    laborEffect = 0
                });
                
                selectedChoiceIndex = evt.choices.Count - 1;
                isDirty = true;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void LoadEvents()
        {
            events = EventLoader.LoadEventsFromJSON();
            
            if (events.Count > 0)
            {
                Debug.Log($"Loaded {events.Count} events from JSON");
            }
            else
            {
                Debug.LogWarning("No events loaded from JSON, editor will start with an empty list");
                events = new List<GameEvent>();
            }
            
            selectedEventIndex = -1;
            selectedChoiceIndex = -1;
            isDirty = false;
        }
        
        private void SaveEvents()
        {
            if (events.Count > 0)
            {
                EventLoader.SaveEventsToJSON(events);
                isDirty = false;
                Debug.Log($"Saved {events.Count} events to JSON");
            }
            else
            {
                Debug.LogWarning("No events to save");
            }
        }
        
        private void OnDestroy()
        {
            if (isDirty)
            {
                if (EditorUtility.DisplayDialog("Unsaved Changes", 
                    "You have unsaved changes. Would you like to save before closing?", 
                    "Save", "Discard"))
                {
                    SaveEvents();
                }
            }
        }
    }
}
#endif