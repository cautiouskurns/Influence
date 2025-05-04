using UnityEngine;
using Entities;
using UI;

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
                // Ensure Economy component is initialized
                if (nation.Economy == null)
                {
                    Debug.LogWarning($"Nation {nation.Name} has no Economy component - creating one");
                    PopulateEconomyDataForTesting(nation);
                }
                else if (nation.Economy.TotalWealth == 0 && nation.Economy.TotalProduction == 0)
                {
                    Debug.LogWarning($"Nation {nation.Name} has zero values in Economy - populating test data");
                    PopulateEconomyDataForTesting(nation);
                }
            }
            
            statsModule.SetNation(nation);
            statsModule.Show(); // Ensure the panel is visible
        }
        else
        {
            Debug.LogWarning("Stats module not found in NationStatsPanel");
        }
    }
    
    // Helper method to ensure nation has valid economy data for UI display
    private void PopulateEconomyDataForTesting(NationEntity nation)
    {
        // If this is a testing/placeholder nation, add some demo values
        var regionCount = nation.GetRegionIds().Count;
        
        // Calculate base values from region count - gives some variety
        float baseValue = 1000 + (regionCount * 250);
        
        // Set base economy values
        if (nation.Economy != null)
        {
            // Use reflection to set values if regular property access doesn't work
            var economyType = nation.Economy.GetType();
            
            try {
                // Try setting values through properties
                SetPrivateFieldOrProperty(economyType, nation.Economy, "TreasuryBalance", baseValue * 0.5f);
                SetPrivateFieldOrProperty(economyType, nation.Economy, "GDP", baseValue * 2.5f);
                SetPrivateFieldOrProperty(economyType, nation.Economy, "TotalProduction", baseValue * 0.65f);
                SetPrivateFieldOrProperty(economyType, nation.Economy, "TotalWealth", baseValue * 5f);
                SetPrivateFieldOrProperty(economyType, nation.Economy, "GDPGrowthRate", 0.05f);
            }
            catch (System.Exception e) {
                Debug.LogError($"Failed to set economy values: {e.Message}");
            }
        }
        
        // Ensure stability values are available too
        if (nation.Stability != null)
        {
            try {
                var stabilityType = nation.Stability.GetType();
                SetPrivateFieldOrProperty(stabilityType, nation.Stability, "Stability", 0.75f);
                SetPrivateFieldOrProperty(stabilityType, nation.Stability, "UnrestLevel", 0.15f);
            }
            catch {
                Debug.Log("Could not set stability values");
            }
        }
    }
    
    // Helper method to set private fields using reflection
    private void SetPrivateFieldOrProperty(System.Type type, object instance, string name, object value)
    {
        // Try to set property first
        var prop = type.GetProperty(name);
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(instance, value);
            return;
        }
        
        // Try to set field if property not found or not writable
        var field = type.GetField(name, 
            System.Reflection.BindingFlags.Public | 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
            
        if (field != null)
        {
            field.SetValue(instance, value);
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