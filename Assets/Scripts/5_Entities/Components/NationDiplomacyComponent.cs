using System.Collections.Generic;
using UnityEngine;

namespace Entities.Components
{
    /// <summary>
    /// CLASS PURPOSE:
    /// Manages diplomatic relations between nations and handles diplomatic events.
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Store and manage diplomatic relationships with other nations
    /// - Track diplomatic metrics like global reputation
    /// - Handle diplomatic events and their effects
    /// </summary>
    public class NationDiplomacyComponent
    {
        // Diplomatic relationships with other nations
        private Dictionary<string, DiplomaticRelation> diplomaticRelations = new Dictionary<string, DiplomaticRelation>();
        
        // Diplomatic metrics
        public float GlobalReputation { get; private set; } = 0.5f;      // 0.0 to 1.0
        public float DiplomaticInfluence { get; private set; } = 50f;    // Influence points
        
        // Event history
        private List<DiplomaticEvent> recentEvents = new List<DiplomaticEvent>();
        private readonly int maxRecentEvents = 10;
        
        /// <summary>
        /// Set diplomatic status with another nation
        /// </summary>
        public void SetDiplomaticStatus(string nationId, NationEntity.DiplomaticStatus status)
        {
            if (string.IsNullOrEmpty(nationId))
                return;
            
            // Get or create the diplomatic relation
            DiplomaticRelation relation = GetOrCreateRelation(nationId);
            
            // Set the new status
            relation.Status = status;
            relation.TurnsInCurrentStatus = 0;
            
            // Update relation score based on new status
            switch (status)
            {
                case NationEntity.DiplomaticStatus.Allied:
                    // Ensure good relations for allies
                    relation.RelationScore = Mathf.Max(relation.RelationScore, 60f);
                    break;
                    
                case NationEntity.DiplomaticStatus.Hostile:
                    // Ensure negative relations for hostile nations
                    relation.RelationScore = Mathf.Min(relation.RelationScore, -60f);
                    break;
                    
                // For neutral, leave the score as is
            }
            
            Debug.Log($"Diplomatic status with {nationId} set to {status}");
        }
        
        /// <summary>
        /// Get the full diplomatic relation data with another nation
        /// </summary>
        public DiplomaticRelation GetDiplomaticRelation(string nationId)
        {
            if (string.IsNullOrEmpty(nationId))
                return null;
                
            return GetOrCreateRelation(nationId);
        }
        
        /// <summary>
        /// Apply a diplomatic event that affects relations with another nation
        /// </summary>
        public void ApplyDiplomaticEvent(string nationId, float reputationChange, string eventDescription = "")
        {
            if (string.IsNullOrEmpty(nationId))
                return;
                
            // Get or create the diplomatic relation
            DiplomaticRelation relation = GetOrCreateRelation(nationId);
            
            // Apply the change to the relation score
            relation.UpdateRelationScore(reputationChange);
            
            // Create and store the event
            DiplomaticEvent newEvent = new DiplomaticEvent(
                nationId,
                reputationChange,
                eventDescription,
                relation.Status
            );
            
            recentEvents.Add(newEvent);
            
            // Trim the events list if it gets too long
            if (recentEvents.Count > maxRecentEvents)
            {
                recentEvents.RemoveAt(0);
            }
            
            // Update global reputation based on event
            UpdateGlobalReputation(reputationChange * 0.01f);
            
            Debug.Log($"Diplomatic event with {nationId}: {eventDescription} ({reputationChange:+0.0;-0.0})");
        }
        
        /// <summary>
        /// Apply a global diplomatic event that affects relations with all nations
        /// </summary>
        public void ApplyGlobalDiplomaticEvent(float reputationChange, string eventDescription = "")
        {
            // Update global reputation
            UpdateGlobalReputation(reputationChange * 0.02f);
            
            // Apply the change to all relations
            foreach (var relation in diplomaticRelations.Values)
            {
                relation.UpdateRelationScore(reputationChange);
            }
            
            Debug.Log($"Global diplomatic event: {eventDescription} ({reputationChange:+0.0;-0.0})");
        }
        
        /// <summary>
        /// Get a simple diplomatic status for another nation
        /// </summary>
        public NationEntity.DiplomaticStatus GetDiplomaticStatus(string nationId)
        {
            if (string.IsNullOrEmpty(nationId))
                return NationEntity.DiplomaticStatus.Neutral;
                
            DiplomaticRelation relation = GetOrCreateRelation(nationId);
            return relation.Status;
        }
        
        /// <summary>
        /// Check if a diplomatic action can be performed with a nation
        /// </summary>
        public bool CanPerformAction(string nationId, DiplomaticActionType actionType)
        {
            if (string.IsNullOrEmpty(nationId))
                return false;
                
            DiplomaticRelation relation = GetOrCreateRelation(nationId);
            return relation.CanPerformAction(actionType);
        }
        
        /// <summary>
        /// Process diplomatic turn for all relations
        /// </summary>
        public void ProcessTurn()
        {
            foreach (var relation in diplomaticRelations.Values)
            {
                relation.ProcessTurn();
            }
            
            // Gradually normalize global reputation
            if (GlobalReputation > 0.5f)
            {
                GlobalReputation = Mathf.Max(GlobalReputation - 0.01f, 0.5f);
            }
            else if (GlobalReputation < 0.5f)
            {
                GlobalReputation = Mathf.Min(GlobalReputation + 0.01f, 0.5f);
            }
            
            // Generate new diplomatic influence
            DiplomaticInfluence += 5f * GlobalReputation;
        }
        
        /// <summary>
        /// Update global reputation based on events
        /// </summary>
        private void UpdateGlobalReputation(float change)
        {
            GlobalReputation = Mathf.Clamp01(GlobalReputation + change);
        }
        
        /// <summary>
        /// Spend diplomatic influence points
        /// </summary>
        public bool SpendInfluence(float amount)
        {
            if (amount > DiplomaticInfluence)
                return false;
                
            DiplomaticInfluence -= amount;
            return true;
        }
        
        /// <summary>
        /// Get or create a diplomatic relation with another nation
        /// </summary>
        private DiplomaticRelation GetOrCreateRelation(string nationId)
        {
            // If a relation already exists, return it
            if (diplomaticRelations.TryGetValue(nationId, out DiplomaticRelation relation))
            {
                return relation;
            }
            
            // Otherwise create a new one with default neutral status
            relation = new DiplomaticRelation(nationId);
            diplomaticRelations.Add(nationId, relation);
            
            return relation;
        }
        
        /// <summary>
        /// Get count of diplomatic relations
        /// </summary>
        public int GetDiplomaticRelationCount()
        {
            return diplomaticRelations.Count;
        }
        
        /// <summary>
        /// Get summary of all diplomatic relations
        /// </summary>
        public string GetSummary()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            sb.AppendLine($"Global Reputation: {GlobalReputation:P0}");
            sb.AppendLine($"Diplomatic Influence: {DiplomaticInfluence:F0} points");
            sb.AppendLine($"Relations with {diplomaticRelations.Count} nations:");
            
            foreach (var relation in diplomaticRelations.Values)
            {
                sb.AppendLine($"- {relation.NationId}: {relation.Status} ({relation.RelationScore:F0})");
            }
            
            if (recentEvents.Count > 0)
            {
                sb.AppendLine("\nRecent Diplomatic Events:");
                for (int i = recentEvents.Count - 1; i >= 0; i--)
                {
                    var evt = recentEvents[i];
                    sb.AppendLine($"- {evt.Description} with {evt.NationId} ({evt.ReputationChange:+0.0;-0.0})");
                }
            }
            
            return sb.ToString();
        }
    }
    
    /// <summary>
    /// Structure to track diplomatic events
    /// </summary>
    public class DiplomaticEvent
    {
        public string NationId { get; private set; }
        public float ReputationChange { get; private set; }
        public string Description { get; private set; }
        public NationEntity.DiplomaticStatus StatusAfterEvent { get; private set; }
        public int TurnOccurred { get; private set; }
        
        public DiplomaticEvent(
            string nationId, 
            float reputationChange, 
            string description, 
            NationEntity.DiplomaticStatus statusAfterEvent)
        {
            NationId = nationId;
            ReputationChange = reputationChange;
            Description = description;
            StatusAfterEvent = statusAfterEvent;
            TurnOccurred = 0; // This would be set by a turn system
        }
    }
}