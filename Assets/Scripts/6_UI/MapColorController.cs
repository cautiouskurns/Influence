using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        
        [Header("Legend")]
        public GameObject legendPanel;
        public Image minColorImage;
        public Image maxColorImage;
        public TextMeshProUGUI legendTitle;
        public TextMeshProUGUI minValueText;
        public TextMeshProUGUI maxValueText;
        
        private void Awake()
        {
            // If not set in inspector, try to find MapManager
            if (mapManager == null)
            {
                mapManager = FindFirstObjectByType<MapManager>();
            }
            
            // Set up button listeners
            if (defaultColorButton != null)
                defaultColorButton.onClick.AddListener(() => SetColorMode(RegionColorMode.Default));
                
            if (positionColorButton != null)
                positionColorButton.onClick.AddListener(() => SetColorMode(RegionColorMode.Position));
                
            if (wealthColorButton != null)
                wealthColorButton.onClick.AddListener(() => SetColorMode(RegionColorMode.Wealth));
                
            if (productionColorButton != null)
                productionColorButton.onClick.AddListener(() => SetColorMode(RegionColorMode.Production));
        }
        
        private void Start()
        {
            // Initialize with default mode
            SetColorMode(RegionColorMode.Default);
        }
        
        public void SetColorMode(RegionColorMode mode)
        {
            if (mapManager == null) return;
            
            // Call the MapManager's method to change color mode
            mapManager.SetColorMode((int)mode);
            
            // Update the legend based on the selected mode
            UpdateLegend(mode);
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
            }
        }
    }
}