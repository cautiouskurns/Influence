using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Entities;
using Managers;

namespace Views
{
    public class NationView : MonoBehaviour
    {
        [Header("Nation Info")]
        [SerializeField] private string nationId;
        
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI nationNameText;
        [SerializeField] private TextMeshProUGUI wealthText;
        [SerializeField] private TextMeshProUGUI productionText;
        [SerializeField] private TextMeshProUGUI stabilityText;
        [SerializeField] private TextMeshProUGUI regionsText;
        [SerializeField] private Image nationColorImage;
        
        private NationEntity nation;
        private NationManager nationManager;
        
        private void Start()
        {
            nationManager = NationManager.Instance;
            
            // Get the nation if ID is set
            if (!string.IsNullOrEmpty(nationId))
            {
                SetNation(nationId);
            }
            
            // Subscribe to events
            EventBus.Subscribe("NationStatisticsUpdated", OnNationStatisticsUpdated);
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            EventBus.Unsubscribe("NationStatisticsUpdated", OnNationStatisticsUpdated);
        }
        
        // Set the nation to display
        public void SetNation(string id)
        {
            nationId = id;
            nation = nationManager.GetNation(nationId);
            
            if (nation != null)
            {
                Debug.Log($"[NationView] Now displaying nation: {nation.Name}");
                UpdateUI();
            }
            else
            {
                Debug.LogWarning($"[NationView] Nation with ID {nationId} not found");
            }
        }
        
        // Update the UI with nation data
        private void UpdateUI()
        {
            if (nation == null) return;
            
            // Update text fields
            if (nationNameText != null)
                nationNameText.text = nation.Name;
                
            if (wealthText != null)
                wealthText.text = $"Wealth: {nation.TotalWealth:F0}";
                
            if (productionText != null)
                productionText.text = $"Production: {nation.TotalProduction:F0}";
                
            if (stabilityText != null)
                stabilityText.text = $"Stability: {nation.Stability:P0}";
                
            if (regionsText != null)
                regionsText.text = $"Regions: {nation.GetRegionIds().Count}";
                
            // Update color
            if (nationColorImage != null)
                nationColorImage.color = nation.Color;
        }
        
        // Event handler for nation statistic updates
        private void OnNationStatisticsUpdated(object data)
        {
            // Update the UI when nation statistics change
            UpdateUI();
        }
    }
}