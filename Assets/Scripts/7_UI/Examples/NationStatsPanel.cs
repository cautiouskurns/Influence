using UnityEngine;
using Entities;
using UI;
using System.Collections.Generic;

/// <summary>
/// Example script that shows how to use the NationStatsUIModule in your game
/// </summary>
public class NationStatsPanel : MonoBehaviour
{
    // Singleton implementation
    private static NationStatsPanel _instance;
    public static NationStatsPanel Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<NationStatsPanel>();
                
                if (_instance == null)
                {
                    // Create it in a Canvas
                    Canvas canvas = FindFirstObjectByType<Canvas>();
                    if (canvas == null)
                    {
                        // Create canvas if none exists
                        GameObject canvasObj = new GameObject("UICanvas");
                        canvas = canvasObj.AddComponent<Canvas>();
                        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                    }
                    
                    GameObject panelObj = new GameObject("NationStatsPanel");
                    panelObj.transform.SetParent(canvas.transform, false);
                    
                    _instance = panelObj.AddComponent<NationStatsPanel>();
                }
            }
            
            return _instance;
        }
    }
    
    [SerializeField] private NationStatsUIModule statsModule;
    
    private void Awake()
    {
        // Ensure singleton behavior
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple NationStatsPanel instances found. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        
        SetupUIModule();
    }
    
    public void SetNation(NationEntity nation)
    {
        if (statsModule != null)
        {
            // Make sure the nation has valid economy data
            if (nation != null)
            {
                // We need to repopulate the test data since the previous changes broke it
                // This is a temporary fix until real economy simulation is implemented
                PopulateEconomyDataForTesting(nation);
            }
            
            statsModule.SetNation(nation);
            statsModule.Show(); // Ensure the panel is visible
        }
        else
        {
            Debug.LogWarning("Stats module not found in NationStatsPanel");
        }
    }
    
    // Keep track of nations we've already populated with data to avoid duplicate logs
    private HashSet<string> populatedNations = new HashSet<string>();
    
    // Helper method to ensure nation has valid economy data for UI display
    private void PopulateEconomyDataForTesting(NationEntity nation)
    {
        // If this is a testing/placeholder nation, add some demo values
        var regionCount = nation.GetRegionIds().Count;
        
        // Calculate base values from region count - gives some variety
        float baseValue = 1000 + (regionCount * 250);
        
        // Set base economy values - directly using property setters now
        if (nation.Economy != null)
        {
            // Only log once per nation to avoid console spam
            bool firstTime = !populatedNations.Contains(nation.Id);
            if (firstTime)
            {
                Debug.Log($"Setting economic values for nation: {nation.Name}");
                populatedNations.Add(nation.Id);
            }
            
            // Set values directly on the component using our helper method
            // which now properly assigns values instead of trying to be too smart
            SetEconomyValue(nation.Economy, "TreasuryBalance", baseValue * 0.5f);
            SetEconomyValue(nation.Economy, "GDP", baseValue * 2.5f);
            SetEconomyValue(nation.Economy, "TotalProduction", baseValue * 0.65f);
            SetEconomyValue(nation.Economy, "TotalWealth", baseValue * 5f);
            SetEconomyValue(nation.Economy, "GDPGrowthRate", 0.05f);
        }
        
        // Ensure stability values are available too
        if (nation.Stability != null)
        {
            SetStabilityValue(nation.Stability, "Stability", 0.75f);
            SetStabilityValue(nation.Stability, "UnrestLevel", 0.15f);
        }
    }
    
    // Simplified helper method to directly set economy values
    private void SetEconomyValue(object component, string propertyName, object value)
    {
        try {
            var property = component.GetType().GetProperty(propertyName);
            if (property != null && property.CanWrite) {
                property.SetValue(component, value);
            }
        }
        catch (System.Exception e) {
            Debug.LogError($"Failed to set {propertyName}: {e.Message}");
        }
    }
    
    // Simplified helper method to directly set stability values
    private void SetStabilityValue(object component, string propertyName, object value)
    {
        try {
            var property = component.GetType().GetProperty(propertyName);
            if (property != null && property.CanWrite) {
                property.SetValue(component, value);
            }
        }
        catch (System.Exception e) {
            Debug.LogError($"Failed to set {propertyName}: {e.Message}");
        }
    }
    
    public void SetupUIModule()
    {
        // If the module reference is not set, try to find it on children
        if (statsModule == null)
        {
            statsModule = GetComponentInChildren<NationStatsUIModule>();
        }
        
        // If still not found, create it
        if (statsModule == null)
        {
            GameObject moduleObj = new GameObject("NationStatsUIModule");
            moduleObj.transform.SetParent(transform, false);
            
            statsModule = moduleObj.AddComponent<NationStatsUIModule>();
        }
        
        // Initialize the module
        statsModule.Initialize();
    }
}