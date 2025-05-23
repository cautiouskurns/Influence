using UnityEngine;
using TMPro;
using Entities;
using Systems;
using Managers;
using Controllers;

namespace UI
{
    /// <summary>
    /// CLASS PURPOSE:
    /// RegionView focuses solely on displaying a region visually on the map.
    /// It follows the Single Responsibility Principle by handling only visualization concerns.
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Display visual elements (sprite colors, highlighting)
    /// - Update text components with provided data
    /// - Forward click events to the event system
    /// </summary>
    public class RegionView : MonoBehaviour
    {
        [Header("References")]
        public SpriteRenderer mainRenderer;
        public SpriteRenderer highlightRenderer;
        public TextMeshPro nameText;
        
        [Header("Status Texts")]
        public TextMeshPro wealthText;
        public TextMeshPro productionText;
        
        // Region identifier
        public string RegionName { get; private set; }
        
        // Entity reference 
        private RegionEntity _regionEntity;
        public RegionEntity RegionEntity => _regionEntity;
        
        // Controller reference
        private RegionController controller;
        
        // Cache the last production value to detect changes
        private int lastProductionValue = 0;
        
        private void Awake()
        {
            // Create the controller for this view
            controller = new RegionController(this);
        }
        
        private void OnDestroy()
        {
            // Clean up the controller
            controller?.Dispose();
        }
        
        /// <summary>
        /// Initialize the view with basic data
        /// </summary>
        public void Initialize(string id, string name, Color color)
        {
            RegionName = id;
            SetNameText(name);
            SetColor(color);
            SetHighlighted(false);
            
            // Initialize with zeros
            UpdateEconomicDisplay(0, 0);
        }
        
        /// <summary>
        /// Set the display name of the region
        /// </summary>
        public void SetNameText(string name)
        {
            if (nameText != null)
            {
                nameText.text = name;
            }
        }
        
        /// <summary>
        /// Set the base color of the region
        /// </summary>
        public void SetColor(Color color)
        {
            if (mainRenderer != null)
            {
                mainRenderer.color = color;
            }
        }
        
        /// <summary>
        /// Get the current color of the region
        /// </summary>
        public Color GetColor()
        {
            return mainRenderer != null ? mainRenderer.color : Color.white;
        }
        
        /// <summary>
        /// Toggle the highlight state
        /// </summary>
        public void SetHighlighted(bool highlighted)
        {
            if (highlightRenderer != null)
            {
                highlightRenderer.enabled = highlighted;
            }
            
            // Update text style based on highlight state
            if (nameText != null)
            {
                nameText.fontStyle = highlighted ? FontStyles.Bold : FontStyles.Normal;
            }
        }
        
        /// <summary>
        /// Update economic data display
        /// </summary>
        public void UpdateEconomicDisplay(int wealth, int production)
        {
            // Update wealth text
            if (wealthText != null)
            {
                // Use more compact format for better fit
                wealthText.text = wealth < 1000 ? $"W:{wealth}" : $"W:{wealth/1000}k";
                
                // Color-code wealth text based on value
                wealthText.color = wealth > 200 ? new Color(0.2f, 0.8f, 0.2f) : (wealth < 100 ? new Color(0.8f, 0.2f, 0.2f) : Color.white);
            }
            
            // Update production text
            if (productionText != null)
            {
                // Use more compact format for better fit
                productionText.text = production < 1000 ? $"P:{production}" : $"P:{production/1000}k";
                
                // Cache the production value
                lastProductionValue = production;
                
                // Color-code production text based on value with more distinct colors
                if (production > 80)
                    productionText.color = new Color(0.1f, 0.9f, 0.1f); // Brighter green
                else if (production < 40)
                    productionText.color = new Color(0.9f, 0.1f, 0.1f); // Brighter red
                else
                    productionText.color = new Color(0.9f, 0.9f, 0.2f); // Yellow for middle values
            }
        }
        
        // Update text colors when the object becomes visible or re-enabled
        private void OnEnable()
        {
            // Re-apply the color based on last known production value
            if (productionText != null && lastProductionValue > 0)
            {
                // Re-apply the color logic
                if (lastProductionValue > 80)
                    productionText.color = new Color(0.1f, 0.9f, 0.1f); // Brighter green
                else if (lastProductionValue < 40)
                    productionText.color = new Color(0.9f, 0.1f, 0.1f); // Brighter red
                else
                    productionText.color = new Color(0.9f, 0.9f, 0.2f); // Yellow for middle values
            }
            
            // Request color refresh from the RegionColorService when the view is enabled
            // This ensures proper color representation if color modes changed while inactive
            if (!string.IsNullOrEmpty(RegionName))
            {
                EventBus.Trigger("RegionViewEnabled", RegionName);
            }
        }
        
        /// <summary>
        /// Update the region name to include nation information
        /// </summary>
        public void UpdateNationInfo(string nationName)
        {
            if (nameText != null && !string.IsNullOrEmpty(nationName))
            {
                // Extract coordinates from region name
                string[] parts = RegionName.Split('_');
                if (parts.Length >= 3)
                {
                    // nameText.text = $"{parts[1]},{parts[2]}\n ({nationName})";
                    nameText.text = $"{parts[1]},{parts[2]}";
                }
            }
        }
        
        /// <summary>
        /// Set the associated region entity
        /// </summary>
        public void SetRegionEntity(RegionEntity entity)
        {
            _regionEntity = entity;
            controller?.SetRegionEntity(entity);
        }
        
        /// <summary>
        /// Handle mouse click on the region
        /// </summary>
        private void OnMouseDown()
        {
            Debug.Log($"Region clicked: {RegionName}");
            
            // Use the controller to handle the click
            controller?.HandleRegionClick();
            
            // Original event trigger remains for compatibility
            EventBus.Trigger("RegionSelected", RegionName);
        }
    }
}