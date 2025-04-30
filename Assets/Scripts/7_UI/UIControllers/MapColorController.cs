using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace UI
{
    public class MapColorController : MonoBehaviour
    {
        [Header("References")]
        public MapManager mapManager;
        
        [Header("UI References")]
        public Button defaultColorButton;
        public Button positionColorButton;
        public Button wealthColorButton;
        public Button productionColorButton;
        public Button nationColorButton; // New button for nation coloring
        public Button terrainColorButton; // New button for terrain coloring
        
        [Header("Legend")]
        public GameObject legendPanel;
        public Image minColorImage;
        public Image maxColorImage;
        public TextMeshProUGUI legendTitle;
        public TextMeshProUGUI minValueText;
        public TextMeshProUGUI maxValueText;
        
        [Header("Debug")]
        [SerializeField] private bool logDebugInfo = true;
        
        private void Awake()
        {
            // If not set in inspector, try to find MapManager
            if (mapManager == null)
            {
                mapManager = FindFirstObjectByType<MapManager>();
                if (mapManager == null)
                {
                    Debug.LogError("MapColorController: Could not find MapManager in scene!");
                }
                else if (logDebugInfo)
                {
                    Debug.Log("MapColorController: Found MapManager: " + mapManager.name);
                }
            }
        }
        
        private void Start()
        {
            // Set up button listeners directly in Start
            SetupButtonListeners();
            
            // Initialize with default mode
            SetColorMode(RegionColorMode.Default);
            
            if (logDebugInfo)
            {
                Debug.Log("MapColorController initialized. UI Controls: " + 
                         (defaultColorButton != null ? "Default✓ " : "Default✗ ") +
                         (positionColorButton != null ? "Position✓ " : "Position✗ ") +
                         (wealthColorButton != null ? "Wealth✓ " : "Wealth✗ ") +
                         (productionColorButton != null ? "Production✓ " : "Production✗ ") +
                         (nationColorButton != null ? "Nation✓ " : "Nation✗ "));
            }
        }
        
        private void SetupButtonListeners()
        {
            // IMPORTANT: Completely clear and re-add all button listeners
            SetupButton(defaultColorButton, () => OnColorButtonClick(RegionColorMode.Default), "Default");
            SetupButton(positionColorButton, () => OnColorButtonClick(RegionColorMode.Position), "Position");
            SetupButton(wealthColorButton, () => OnColorButtonClick(RegionColorMode.Wealth), "Wealth");
            SetupButton(productionColorButton, () => OnColorButtonClick(RegionColorMode.Production), "Production");
            SetupButton(nationColorButton, () => OnColorButtonClick(RegionColorMode.Nation), "Nation");
            SetupButton(terrainColorButton, () => OnColorButtonClick(RegionColorMode.Terrain), "Terrain");
        }
        
        private void SetupButton(Button button, UnityAction action, string name)
        {
            if (button != null)
            {
                // Clear existing listeners to avoid duplicates
                button.onClick.RemoveAllListeners();
                
                // Add fresh listener
                button.onClick.AddListener(action);
                
                if (logDebugInfo)
                {
                    button.onClick.AddListener(() => Debug.Log($"{name} button clicked"));
                }
            }
            else if (logDebugInfo)
            {
                Debug.LogWarning($"{name} button reference is missing!");
            }
        }
        
        private void OnColorButtonClick(RegionColorMode mode)
        {
            SetColorMode(mode);
        }
        
        public void SetColorMode(RegionColorMode mode)
        {
            if (mapManager == null)
            {
                Debug.LogError("Cannot set color mode: MapManager is null!");
                
                // Try to find it again as a last resort
                mapManager = FindFirstObjectByType<MapManager>();
                if (mapManager == null) return;
            }
            
            Debug.Log($"Setting color mode to: {mode}");
            
            // Call the MapManager's method to change color mode
            mapManager.SetColorMode((int)mode);
            
            // Update the legend based on the selected mode
            UpdateLegend(mode);
            
            // Directly force an update of colors in case the event system fails
            if (mapManager != null)
            {
                mapManager.UpdateRegionColors();
            }
        }
        
        private void UpdateLegend(RegionColorMode mode)
        {
            if (legendPanel == null) return;
            
            switch (mode)
            {
                case RegionColorMode.Default:
                    // Hide legend for default uniform coloring
                    legendPanel.SetActive(false);
                    break;
                    
                case RegionColorMode.Position:
                    // Simple position-based legend
                    legendPanel.SetActive(true);
                    legendTitle.text = "Position";
                    minColorImage.color = new Color(0.4f, 0.4f, 0.5f);
                    maxColorImage.color = new Color(1.0f, 1.0f, 0.5f);
                    minValueText.text = "Top-Left";
                    maxValueText.text = "Bottom-Right";
                    break;
                    
                case RegionColorMode.Wealth:
                    // Wealth-based legend
                    legendPanel.SetActive(true);
                    legendTitle.text = "Wealth";
                    minColorImage.color = new Color(0.8f, 0.2f, 0.2f); // Red for poor
                    maxColorImage.color = new Color(0.2f, 0.8f, 0.2f); // Green for wealthy
                    minValueText.text = "Poor";
                    maxValueText.text = "Wealthy";
                    break;
                    
                case RegionColorMode.Production:
                    // Production-based legend
                    legendPanel.SetActive(true);
                    legendTitle.text = "Production";
                    minColorImage.color = new Color(0.2f, 0.2f, 0.8f); // Blue for low
                    maxColorImage.color = new Color(0.8f, 0.8f, 0.2f); // Yellow for high
                    minValueText.text = "Low";
                    maxValueText.text = "High";
                    break;

                case RegionColorMode.Nation:
                    // Nation-based legend
                    legendPanel.SetActive(true);
                    legendTitle.text = "Nation";
                    minColorImage.color = new Color(0.5f, 0.5f, 0.5f); // Example color for nation
                    maxColorImage.color = new Color(1.0f, 0.5f, 0.5f); // Example color for nation
                    minValueText.text = "Nation A";
                    maxValueText.text = "Nation B";
                    break;
                    
                case RegionColorMode.Terrain:
                    // Terrain-based legend
                    legendPanel.SetActive(true);
                    legendTitle.text = "Terrain Type";
                    // Sample terrain colors
                    minColorImage.color = new Color(0.7f, 0.85f, 0.5f); // Plains green
                    maxColorImage.color = new Color(0.95f, 0.85f, 0.6f); // Desert yellow
                    minValueText.text = "Plains";
                    maxValueText.text = "Desert";
                    break;
            }
        }
        
        // Public method that can be called from UI buttons directly
        public void SetDefaultColorMode() => OnColorButtonClick(RegionColorMode.Default);
        public void SetPositionColorMode() => OnColorButtonClick(RegionColorMode.Position);
        public void SetWealthColorMode() => OnColorButtonClick(RegionColorMode.Wealth);
        public void SetProductionColorMode() => OnColorButtonClick(RegionColorMode.Production);
        public void SetNationColorMode() => OnColorButtonClick(RegionColorMode.Nation);
        public void SetTerrainColorMode() => OnColorButtonClick(RegionColorMode.Terrain);
        
        // Call this method to manually refresh connections
        public void RefreshConnections()
        {
            if (mapManager == null)
            {
                mapManager = FindFirstObjectByType<MapManager>();
            }
            
            SetupButtonListeners();
            Debug.Log("MapColorController: Connections refreshed");
        }
    }
}