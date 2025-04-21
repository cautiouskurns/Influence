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
                    Debug.Log($"RegionView {RegionName}: Found economic entity");
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
                wealthText.text = $"W: {wealth}";
                
            if (productionText != null)
                productionText.text = $"P: {production}";
        }
        
        private void OnMouseDown()
        {
            SetHighlighted(true);
            EventBus.Trigger("RegionSelected", RegionName);
        }
    }
}