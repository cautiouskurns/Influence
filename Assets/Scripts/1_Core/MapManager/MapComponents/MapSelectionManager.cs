using UnityEngine;
using System.Collections.Generic;
using UI;

namespace UI.MapComponents
{
    /// <summary>
    /// CLASS PURPOSE:
    /// Handle region selection and highlighting on the map.
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Track the currently selected region
    /// - Handle selection and deselection of regions
    /// - Maintain visual highlighting of selected regions
    /// </summary>
    public class MapSelectionManager
    {
        // State tracking
        private string selectedRegionId;
        private readonly Dictionary<string, RegionView> regionViews;
        
        /// <summary>
        /// Constructor with region views collection
        /// </summary>
        public MapSelectionManager(Dictionary<string, RegionView> regionViews)
        {
            this.regionViews = regionViews;
        }
        
        /// <summary>
        /// Get the currently selected region ID
        /// </summary>
        public string GetSelectedRegionId()
        {
            return selectedRegionId;
        }
        
        /// <summary>
        /// Handle selection of a region
        /// </summary>
        public void SelectRegion(string regionId)
        {
            // Do nothing if the region is already selected
            if (regionId == selectedRegionId) return;
            
            // Deselect previous region if there was one
            if (!string.IsNullOrEmpty(selectedRegionId) && 
                regionViews.TryGetValue(selectedRegionId, out var previousRegion))
            {
                previousRegion.SetHighlighted(false);
                Debug.Log($"Deselected region: {selectedRegionId}");
            }
            
            // Select new region
            selectedRegionId = regionId;
            if (regionViews.TryGetValue(regionId, out var currentRegion))
            {
                currentRegion.SetHighlighted(true);
                Debug.Log($"Selected region: {regionId}");
            }
        }
        
        /// <summary>
        /// Clear all selections
        /// </summary>
        public void DeselectAll()
        {
            // Deselect current region if there is one
            if (!string.IsNullOrEmpty(selectedRegionId) && 
                regionViews.TryGetValue(selectedRegionId, out var currentRegion))
            {
                currentRegion.SetHighlighted(false);
            }
            
            // Clear the selected region ID
            selectedRegionId = null;
            Debug.Log("Cleared all region selections");
        }
        
        /// <summary>
        /// Handle region selection event
        /// </summary>
        public void OnRegionSelected(object data)
        {
            if (data is string regionId)
            {
                SelectRegion(regionId);
            }
        }
    }
}