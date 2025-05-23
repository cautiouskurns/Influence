using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UI.MapComponents;

namespace UI
{
    public class MapColorController : MonoBehaviour
    {
        [Header("References")]
        private RegionColorService colorService;
        
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
            // Get the RegionColorService instance
            colorService = RegionColorService.Instance;
            if (colorService == null)
            {
                Debug.LogError("MapColorController: Could not find RegionColorService in scene!");
            }
            else if (logDebugInfo)
            {
//                Debug.Log("MapColorController: Found RegionColorService");
            }
        }
        
        private void Start()
        {
            // Set up button listeners directly in Start
            SetupButtonListeners();
            
            // Initialize with the current mode from the service
            if (colorService != null)
            {
                UpdateLegend(colorService.GetColorMode());
            }
            else
            {
                // Fallback to default mode
                UpdateLegend(RegionColorMode.Default);
            }
            
            if (logDebugInfo)
            {
                // Debug.Log("MapColorController initialized. UI Controls: " + 
                //          (defaultColorButton != null ? "Default✓ " : "Default✗ ") +
                //          (positionColorButton != null ? "Position✓ " : "Position✗ ") +
                //          (wealthColorButton != null ? "Wealth✓ " : "Wealth✗ ") +
                //          (productionColorButton != null ? "Production✓ " : "Production✗ ") +
                //          (nationColorButton != null ? "Nation✓ " : "Nation✗ "));
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
            if (colorService == null)
            {
                Debug.LogError("Cannot set color mode: RegionColorService is null!");
                
                // Try to find it again as a last resort
                colorService = RegionColorService.Instance;
                if (colorService == null) return;
            }
            
            Debug.Log($"Setting color mode to: {mode}");
            
            // Call the RegionColorService's method to change color mode
            colorService.SetColorMode(mode);
            
            // Update the legend based on the selected mode
            UpdateLegend(mode);
        }
        
        private void UpdateLegend(RegionColorMode mode)
        {
            if (legendPanel == null) return;
            
            // Get legend configuration from the service
            if (colorService != null)
            {
                LegendConfiguration config = colorService.GetLegendConfiguration(mode);
                
                // Apply configuration to UI elements
                legendPanel.SetActive(config.ShowLegend);
                
                if (config.ShowLegend)
                {
                    legendTitle.text = config.Title;
                    minColorImage.color = config.MinColor;
                    maxColorImage.color = config.MaxColor;
                    minValueText.text = config.MinLabel;
                    maxValueText.text = config.MaxLabel;
                }
            }
            else
            {
                // Fallback to previous hardcoded behavior if service is unavailable
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
            if (colorService == null)
            {
                colorService = RegionColorService.Instance;
            }
            
            SetupButtonListeners();
            Debug.Log("MapColorController: Connections refreshed");
        }
    }
}