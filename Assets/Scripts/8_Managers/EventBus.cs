using System;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    /// <summary>
    /// EventBus provides a centralized messaging system for loosely coupling different parts
    /// of the application.
    /// </summary>
    public static class EventBus
    {
        private static Dictionary<string, Action<object>> eventDictionary = new Dictionary<string, Action<object>>();
        
        public static void Subscribe(string eventName, Action<object> listener)
        {
            if (!eventDictionary.ContainsKey(eventName))
                eventDictionary[eventName] = null;

            eventDictionary[eventName] += listener;
        }

        public static void Unsubscribe(string eventName, Action<object> listener)
        {
            if (!eventDictionary.ContainsKey(eventName))
                return;

            eventDictionary[eventName] -= listener;
        }

        public static void Trigger(string eventName, object eventData = null)
        {
            if (eventDictionary.TryGetValue(eventName, out var callbacks) && callbacks != null)
            {
                callbacks(eventData);
            }
        }
    }
}