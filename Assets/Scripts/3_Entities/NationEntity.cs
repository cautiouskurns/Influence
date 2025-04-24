using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
    public class NationEntity
    {
        // Basic properties
        public string Id { get; set; }
        public string Name { get; set; }
        public Color NationColor { get; set; }
        
        // List of regions that belong to this nation
        private List<string> regionIds = new List<string>();
        
        // Basic diplomatic status (can be expanded later)
        public enum DiplomaticStatus { Allied, Neutral, Hostile }
        
        // Dictionary to track diplomatic relations with other nations
        private Dictionary<string, DiplomaticStatus> diplomaticRelations = new Dictionary<string, DiplomaticStatus>();

        public NationEntity(string id, string name, Color color)
        {
            Id = id;
            Name = name;
            NationColor = color;
        }
        
        // Region management
        public void AddRegion(string regionId)
        {
            if (!regionIds.Contains(regionId))
            {
                regionIds.Add(regionId);
                Debug.Log($"Added region {regionId} to nation {Name}");
            }
        }
        
        public void RemoveRegion(string regionId)
        {
            if (regionIds.Contains(regionId))
            {
                regionIds.Remove(regionId);
                Debug.Log($"Removed region {regionId} from nation {Name}");
            }
        }
        
        public List<string> GetRegionIds()
        {
            return new List<string>(regionIds);
        }
        
        // Diplomacy methods
        public void SetDiplomaticStatus(string otherNationId, DiplomaticStatus status)
        {
            diplomaticRelations[otherNationId] = status;
            Debug.Log($"Set diplomatic status with {otherNationId} to {status}");
        }
        
        public DiplomaticStatus GetDiplomaticStatus(string otherNationId)
        {
            if (diplomaticRelations.TryGetValue(otherNationId, out DiplomaticStatus status))
            {
                return status;
            }
            return DiplomaticStatus.Neutral; // Default to neutral
        }
        
        // Very basic nation summary
        public string GetSummary()
        {
            return $"Nation: {Name}\nRegions: {regionIds.Count}\nDiplomatic Relations: {diplomaticRelations.Count}";
        }
    }
}