using UnityEngine;

namespace Entities.Components
{
    /// <summary>
    /// Represents the diplomatic relationship between two nations
    /// </summary>
    public class DiplomaticRelation
    {
        // The nation this relationship is with
        public string NationId { get; private set; }
        
        // Current diplomatic status
        public NationEntity.DiplomaticStatus Status { get; set; }
        
        // Relationship metrics
        public float RelationScore { get; set; } = 0f; // -100 to 100
        public float TradeValue { get; set; } = 0f;
        public bool HasTreaty { get; set; } = false;
        public int TurnsInCurrentStatus { get; set; } = 0;
        
        /// <summary>
        /// Constructor with required information
        /// </summary>
        public DiplomaticRelation(string nationId, NationEntity.DiplomaticStatus initialStatus = NationEntity.DiplomaticStatus.Neutral)
        {
            NationId = nationId;
            Status = initialStatus;
            
            // Set default relation score based on initial status
            switch (initialStatus)
            {
                case NationEntity.DiplomaticStatus.Allied:
                    RelationScore = 75f;
                    break;
                case NationEntity.DiplomaticStatus.Hostile:
                    RelationScore = -75f;
                    break;
                case NationEntity.DiplomaticStatus.Neutral:
                default:
                    RelationScore = 0f;
                    break;
            }
        }
        
        /// <summary>
        /// Check if relations are good enough for a specific action
        /// </summary>
        public bool CanPerformAction(DiplomaticActionType actionType)
        {
            switch (actionType)
            {
                case DiplomaticActionType.Trade:
                    return RelationScore >= -25f;
                case DiplomaticActionType.Alliance:
                    return RelationScore >= 50f;
                case DiplomaticActionType.WarDeclaration:
                    return RelationScore <= -50f;
                case DiplomaticActionType.Treaty:
                    return RelationScore >= 25f;
                default:
                    return true; // Basic actions always allowed
            }
        }
        
        /// <summary>
        /// Update the relationship score based on an event
        /// </summary>
        public void UpdateRelationScore(float change)
        {
            RelationScore = Mathf.Clamp(RelationScore + change, -100f, 100f);
            
            // Update status based on relation score
            UpdateStatusFromScore();
        }
        
        /// <summary>
        /// Update the diplomatic status based on the current relation score
        /// </summary>
        private void UpdateStatusFromScore()
        {
            // Only change status if the score is significantly different
            if (RelationScore >= 60f && Status != NationEntity.DiplomaticStatus.Allied)
            {
                Status = NationEntity.DiplomaticStatus.Allied;
                TurnsInCurrentStatus = 0;
            }
            else if (RelationScore <= -60f && Status != NationEntity.DiplomaticStatus.Hostile)
            {
                Status = NationEntity.DiplomaticStatus.Hostile;
                TurnsInCurrentStatus = 0;
            }
            else if (RelationScore > -30f && RelationScore < 30f && Status != NationEntity.DiplomaticStatus.Neutral)
            {
                Status = NationEntity.DiplomaticStatus.Neutral;
                TurnsInCurrentStatus = 0;
            }
        }
        
        /// <summary>
        /// Process a turn for this relationship
        /// </summary>
        public void ProcessTurn()
        {
            TurnsInCurrentStatus++;
            
            // Relations naturally normalize over time
            if (RelationScore > 0)
            {
                RelationScore = Mathf.Max(RelationScore - 0.5f, 0f);
            }
            else if (RelationScore < 0)
            {
                RelationScore = Mathf.Min(RelationScore + 0.5f, 0f);
            }
        }
        
        /// <summary>
        /// Get a summary of this diplomatic relationship
        /// </summary>
        public string GetSummary()
        {
            string relationDesc = RelationScore > 50f ? "Excellent" :
                                  RelationScore > 25f ? "Good" :
                                  RelationScore > -25f ? "Neutral" :
                                  RelationScore > -50f ? "Poor" : "Hostile";
                                  
            return $"Relations with {NationId}: {relationDesc} ({RelationScore:F0})\n" +
                   $"Status: {Status} for {TurnsInCurrentStatus} turns\n" +
                   $"Trade Value: {TradeValue:F0}\n" +
                   $"Treaty: {(HasTreaty ? "Yes" : "No")}";
        }
    }
    
    /// <summary>
    /// Types of diplomatic actions that can be performed
    /// </summary>
    public enum DiplomaticActionType
    {
        Basic,         // Basic diplomatic interaction
        Trade,         // Trade agreement
        Alliance,      // Military alliance
        WarDeclaration,// Declare war
        Treaty         // Sign treaty
    }
}