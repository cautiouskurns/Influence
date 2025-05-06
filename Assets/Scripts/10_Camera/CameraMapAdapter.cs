using UnityEngine;
using UI;

namespace Managers
{
    /// <summary>
    /// Helper class that connects the MapManager with the CameraController.
    /// Automatically configures camera boundaries based on the map size.
    /// </summary>
    public class CameraMapAdapter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CameraController cameraController;
        [SerializeField] private MapManager mapManager;
        
        [Header("Settings")]
        [SerializeField] private float boundaryPadding = 2f;
        [SerializeField] private bool autoFitOnStart = true;
        
        private void Awake()
        {
            // Auto-find references if not assigned
            if (cameraController == null)
                cameraController = FindFirstObjectByType<CameraController>();
                
            if (mapManager == null)
                mapManager = FindFirstObjectByType<MapManager>();
        }
        
        private void Start()
        {
            if (cameraController != null && mapManager != null)
            {
                SetupCameraBoundaries();
                
                if (autoFitOnStart)
                {
                    FitCameraToMap();
                }
            }
            else
            {
                Debug.LogError("CameraMapAdapter: Missing required references to CameraController or MapManager.");
            }
        }
        
        /// <summary>
        /// Sets up camera boundaries based on the map size
        /// </summary>
        public void SetupCameraBoundaries()
        {
            // Calculate the map bounds
            Vector2 mapSize = CalculateMapSize();
            Vector2 center = Vector2.zero; // Assuming map is centered at origin
            
            // Set the camera boundaries with padding
            Vector2 min = center - (mapSize / 2) - new Vector2(boundaryPadding, boundaryPadding);
            Vector2 max = center + (mapSize / 2) + new Vector2(boundaryPadding, boundaryPadding);
            
            cameraController.SetBoundaries(min, max);
            
        }
        
        /// <summary>
        /// Fits the camera to show the entire map
        /// </summary>
        public void FitCameraToMap()
        {
            Vector2 mapSize = CalculateMapSize();
            Vector2 center = Vector2.zero; // Assuming map is centered at origin
            
            Bounds mapBounds = new Bounds(center, new Vector3(mapSize.x, mapSize.y, 0));
            cameraController.FitToBounds(mapBounds);
            
            Debug.Log("CameraMapAdapter: Fitted camera to show entire map");
        }
        
        /// <summary>
        /// Calculates the overall size of the map
        /// </summary>
        private Vector2 CalculateMapSize()
        {
            // Get map dimensions from MapManager
            // This is an estimate based on your hex grid implementation
            // You might need to adjust this based on your exact implementation
            
            // Extract grid dimensions and spacing through reflection
            System.Reflection.FieldInfo widthField = typeof(MapManager).GetField("gridWidth", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
            System.Reflection.FieldInfo heightField = typeof(MapManager).GetField("gridHeight", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
            System.Reflection.FieldInfo hexSizeField = typeof(MapManager).GetField("hexSize", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
            System.Reflection.FieldInfo horizontalAdjustField = typeof(MapManager).GetField("horizontalSpacingAdjust", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
            System.Reflection.FieldInfo verticalAdjustField = typeof(MapManager).GetField("verticalSpacingAdjust", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
            int width = 8; // Default
            int height = 8; // Default
            float hexSize = 1.0f; // Default
            float horizontalAdjust = 1.0f; // Default
            float verticalAdjust = 1.0f; // Default
            
            // Try to get actual values
            if (widthField != null) width = (int)widthField.GetValue(mapManager);
            if (heightField != null) height = (int)heightField.GetValue(mapManager);
            if (hexSizeField != null) hexSize = (float)hexSizeField.GetValue(mapManager);
            if (horizontalAdjustField != null) horizontalAdjust = (float)horizontalAdjustField.GetValue(mapManager);
            if (verticalAdjustField != null) verticalAdjust = (float)verticalAdjustField.GetValue(mapManager);
            
            // Calculate the size
            // For simplicity, this implementation conservatively estimates based on your hex grid formulas
            float sqrt3 = Mathf.Sqrt(3);
            
            // Use the same calculations as in your MapManager for consistency
            float horizontalSpacing = hexSize * sqrt3 * horizontalAdjust;
            float verticalSpacing = hexSize * 2.0f * 0.75f * verticalAdjust;
            
            float mapWidth = width * horizontalSpacing;
            float mapHeight = height * verticalSpacing;
            
            return new Vector2(mapWidth, mapHeight);
        }
    }
}