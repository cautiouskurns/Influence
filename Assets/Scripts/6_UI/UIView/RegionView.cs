using UnityEngine;
using TMPro;
using Entities;
using Systems;
using Managers;

namespace UI
{
    /// <summary>
    /// RegionView represents a single region on the map, displaying its economic status.
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
        
        // Region data
        public string RegionName { get; private set; }
        public RegionEntity RegionEntity { get; private set; }
        
        // Cache for economic system
        private EconomicSystem economicSystem;
        
        private void Awake()
        {
            economicSystem = FindFirstObjectByType<EconomicSystem>();
        }
        
        private void Update()
        {
            if (RegionEntity != null)
            {
                UpdateUIFromEntity();
            }
        }
        
        public void Initialize(string id, string name, Color color)
        {
            RegionName = id;
            nameText.text = name;
            mainRenderer.color = color;
            highlightRenderer.enabled = false;
            
            // Initialize with zeros
            UpdateUI(0, 0);
            
            // Subscribe to events
            EventBus.Subscribe("RegionUpdated", OnRegionUpdated);
            EventBus.Subscribe("EconomicTick", OnEconomicTick);
            
            // Immediately try to find our entity
            TryGetRegionEntityFromSystem();
        }
        
        private void OnDestroy()
        {
            EventBus.Unsubscribe("RegionUpdated", OnRegionUpdated);
            EventBus.Unsubscribe("EconomicTick", OnEconomicTick);
        }
        
        private void TryGetRegionEntityFromSystem()
        {
            if (economicSystem != null && !string.IsNullOrEmpty(RegionName))
            {
                RegionEntity existingEntity = economicSystem.GetRegion(RegionName);
                if (existingEntity != null)
                {
//                    Debug.Log($"RegionView {RegionName}: Found economic entity");
                    RegionEntity = existingEntity;
                    UpdateUIFromEntity();
                }
            }
        }
        
        private void OnRegionUpdated(object data)
        {
            if (data is RegionEntity region && region.Name == RegionName)
            {
                RegionEntity = region;
                UpdateUIFromEntity();
            }
        }
        
        private void OnEconomicTick(object data)
        {
            // Refresh our entity reference and force update
            TryGetRegionEntityFromSystem();
        }
        
        public void SetHighlighted(bool highlighted)
        {
            if (highlightRenderer != null)
            {
                highlightRenderer.enabled = highlighted;
            }
        }
        
        public void SetRegionEntity(RegionEntity regionEntity)
        {
            if (regionEntity == null) return;
            
            RegionEntity = regionEntity;
            UpdateUIFromEntity();
        }
        
        private void UpdateUIFromEntity()
        {
            if (RegionEntity == null) return;
            
            UpdateUI(
                RegionEntity.Wealth,
                RegionEntity.Production
            );
        }
        
        private void UpdateUI(int wealth, int production)
        {
            if (wealthText != null)
            {
                // Use more compact format for better fit
                wealthText.text = wealth < 1000 ? $"W:{wealth}" : $"W:{wealth/1000}k";
                
                // Color-code wealth text based on value
                wealthText.color = wealth > 200 ? Color.green : (wealth < 100 ? Color.red : Color.white);
            }
                
            if (productionText != null)
            {
                // Use more compact format for better fit
                productionText.text = production < 1000 ? $"P:{production}" : $"P:{production/1000}k";
                
                // Color-code production text based on value
                productionText.color = production > 80 ? Color.green : (production < 40 ? Color.red : Color.white);
            }
        }
        
        private void OnMouseDown()
        {
            SetHighlighted(true);
            EventBus.Trigger("RegionSelected", RegionName);
            
            // Display additional info when selected
            if (RegionEntity != null)
            {
                // Make the name text slightly larger when selected for emphasis
                if (nameText != null)
                {
                    nameText.fontStyle = FontStyles.Bold;
                }
                
                Debug.Log($"Region {RegionName} - Wealth: {RegionEntity.Wealth}, " +
                         $"Production: {RegionEntity.Production}, " +
                         $"Labor: {RegionEntity.LaborAvailable}, " +
                         $"Infrastructure: {RegionEntity.InfrastructureLevel}");
            }
        }
        
        // Add a method to handle deselection
        public void Deselect()
        {
            SetHighlighted(false);
            
            // Reset text styling
            if (nameText != null)
            {
                nameText.fontStyle = FontStyles.Normal;
            }
        }

        // Add method to update the region's color (called when color mode changes)
        public void UpdateColor(Color newColor)
        {
            if (mainRenderer != null)
            {
                // Set the new color on the renderer
                mainRenderer.color = newColor;
//                Debug.Log($"RegionView {RegionName}: Updated color to {newColor}");
            }
            else
            {
                Debug.LogError($"RegionView {RegionName}: Cannot update color - mainRenderer is null");
            }
        }
    }
}