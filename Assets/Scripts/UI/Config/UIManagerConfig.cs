using UnityEngine;

namespace UI.Config
{
    /// <summary>
    /// ScriptableObject that stores configuration settings for the UIManager.
    /// This allows panel sizes and layout parameters to be configured outside 
    /// of the UIManager implementation.
    /// </summary>
    [CreateAssetMenu(fileName = "UIManagerConfig", menuName = "UI/UIManager Configuration", order = 1)]
    public class UIManagerConfig : ScriptableObject
    {
        [Header("Panel Dimensions")]
        [Tooltip("Default height for the top panel")]
        [Min(0f)]
        public float topPanelHeight = 80f;
        
        [Tooltip("Default height for the bottom panel")]
        [Min(0f)]
        public float bottomPanelHeight = 150f;
        
        [Tooltip("Default width for the side panels")]
        [Min(0f)]
        public float sidePanelWidth = 200f;
        
        [Tooltip("Default size for the center panel")]
        public Vector2 centerPanelSize = new Vector2(400f, 300f);
        
        [Header("Layout Settings")]
        [Tooltip("Default spacing between UI elements in layouts")]
        [Min(0f)]
        public float defaultElementSpacing = 10f;
        
        [Tooltip("Padding around panel edges")]
        [Min(0)]
        public int panelPadding = 10;
        
        [Header("Canvas Settings")]
        [Tooltip("Reference resolution for the canvas scaler")]
        public Vector2 referenceResolution = new Vector2(1920, 1080);
        
        [Tooltip("Whether panels should show debug visual borders")]
        public bool showPanelDebugBorders = false;
        
        [Tooltip("Debug border color for panels")]
        public Color panelDebugBorderColor = new Color(1, 1, 1, 0.1f);
        
        [Header("Module Management")]
        [Tooltip("Whether to automatically create essential modules if missing")]
        public bool autoCreateEssentialModules = true;
        
        /// <summary>
        /// Creates a default configuration instance
        /// </summary>
        public static UIManagerConfig CreateDefaultConfig()
        {
            var config = CreateInstance<UIManagerConfig>();
            return config;
        }
    }
}