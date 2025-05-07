using UnityEngine;
using UnityEngine.UI;

namespace UI.UIManager.HelperClasses
{
    /// <summary>
    /// Helper class that handles the creation, configuration, and management of UI panels
    /// </summary>
    public class PanelManager
    {
        // References to the main panels
        private RectTransform _topPanel;
        private RectTransform _bottomPanel;
        private RectTransform _leftPanel;
        private RectTransform _rightPanel;
        private RectTransform _centerPanel;
        
        // Canvas reference
        private Canvas _mainCanvas;
        
        // Configuration
        private float _topPanelHeight = 80f;
        private float _bottomPanelHeight = 150f;
        private float _sidePanelWidth = 200f;
        private Vector2 _centerPanelSize = new Vector2(400f, 300f);
        private float _defaultElementSpacing = 10f;
        private int _panelPadding = 10;
        private bool _showPanelDebugBorders = false;
        private Color _panelDebugBorderColor = new Color(1, 1, 1, 0.1f);
        
        /// <summary>
        /// Constructor that initializes the PanelManager with default values
        /// </summary>
        public PanelManager()
        {
            // Default constructor
        }
        
        /// <summary>
        /// Constructor that initializes the PanelManager with specific panel settings
        /// </summary>
        /// <param name="topPanelHeight">Height of the top panel</param>
        /// <param name="bottomPanelHeight">Height of the bottom panel</param>
        /// <param name="sidePanelWidth">Width of side panels</param>
        /// <param name="centerPanelSize">Size of the center panel</param>
        /// <param name="defaultElementSpacing">Default spacing between UI elements</param>
        /// <param name="panelPadding">Panel padding amount</param>
        public PanelManager(float topPanelHeight, float bottomPanelHeight, float sidePanelWidth, 
                           Vector2 centerPanelSize, float defaultElementSpacing, int panelPadding)
        {
            _topPanelHeight = topPanelHeight;
            _bottomPanelHeight = bottomPanelHeight;
            _sidePanelWidth = sidePanelWidth;
            _centerPanelSize = centerPanelSize;
            _defaultElementSpacing = defaultElementSpacing;
            _panelPadding = panelPadding;
        }
        
        /// <summary>
        /// Set debug visualization options
        /// </summary>
        /// <param name="showBorders">Whether to show debug borders</param>
        /// <param name="borderColor">Color for debug borders</param>
        public void SetDebugVisualization(bool showBorders, Color borderColor)
        {
            _showPanelDebugBorders = showBorders;
            _panelDebugBorderColor = borderColor;
        }
        
        /// <summary>
        /// Initialize panels with existing references
        /// </summary>
        public void Initialize(Canvas mainCanvas, RectTransform topPanel, RectTransform bottomPanel, 
                              RectTransform leftPanel, RectTransform rightPanel, RectTransform centerPanel)
        {
            _mainCanvas = mainCanvas;
            _topPanel = topPanel;
            _bottomPanel = bottomPanel;
            _leftPanel = leftPanel;
            _rightPanel = rightPanel;
            _centerPanel = centerPanel;
        }
        
        /// <summary>
        /// Create the necessary UI panels if they don't already exist
        /// </summary>
        /// <returns>True if panels were created or already existed, false on failure</returns>
        public bool CreateUILayoutIfNeeded()
        {
            // Check for canvas
            if (_mainCanvas == null)
            {
                GameObject canvasObj = new GameObject("MainCanvas");
                _mainCanvas = canvasObj.AddComponent<Canvas>();
                _mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            // Create panels if they don't exist
            if (_topPanel == null)
            {
                _topPanel = CreatePanel("TopPanel", _mainCanvas.transform);
                _topPanel.anchorMin = new Vector2(0, 1);
                _topPanel.anchorMax = new Vector2(1, 1);
                _topPanel.pivot = new Vector2(0.5f, 1);
                _topPanel.sizeDelta = new Vector2(0, _topPanelHeight);
                _topPanel.anchoredPosition = Vector2.zero;
            }
            
            if (_bottomPanel == null)
            {
                _bottomPanel = CreatePanel("BottomPanel", _mainCanvas.transform);
                _bottomPanel.anchorMin = new Vector2(0, 0);
                _bottomPanel.anchorMax = new Vector2(1, 0);
                _bottomPanel.pivot = new Vector2(0.5f, 0);
                _bottomPanel.sizeDelta = new Vector2(0, _bottomPanelHeight);
                _bottomPanel.anchoredPosition = Vector2.zero;
            }
            
            if (_leftPanel == null)
            {
                _leftPanel = CreatePanel("LeftPanel", _mainCanvas.transform);
                _leftPanel.anchorMin = new Vector2(0, 0);
                _leftPanel.anchorMax = new Vector2(0, 1);
                _leftPanel.pivot = new Vector2(0, 0.5f);
                _leftPanel.sizeDelta = new Vector2(_sidePanelWidth, 0);
                _leftPanel.anchoredPosition = Vector2.zero;
            }
            
            if (_rightPanel == null)
            {
                _rightPanel = CreatePanel("RightPanel", _mainCanvas.transform);
                _rightPanel.anchorMin = new Vector2(1, 0);
                _rightPanel.anchorMax = new Vector2(1, 1);
                _rightPanel.pivot = new Vector2(1, 0.5f);
                _rightPanel.sizeDelta = new Vector2(_sidePanelWidth, 0);
                _rightPanel.anchoredPosition = Vector2.zero;
            }
            
            if (_centerPanel == null)
            {
                _centerPanel = CreatePanel("CenterPanel", _mainCanvas.transform);
                _centerPanel.anchorMin = new Vector2(0.5f, 0.5f);
                _centerPanel.anchorMax = new Vector2(0.5f, 0.5f);
                _centerPanel.pivot = new Vector2(0.5f, 0.5f);
                _centerPanel.sizeDelta = _centerPanelSize;
                _centerPanel.anchoredPosition = Vector2.zero;
            }
            
            return true;
        }
        
        /// <summary>
        /// Create a panel with the given name
        /// </summary>
        /// <param name="name">Name of the panel GameObject</param>
        /// <param name="parent">Parent transform</param>
        /// <returns>RectTransform of the created panel</returns>
        public RectTransform CreatePanel(string name, Transform parent)
        {
            GameObject panelObj = new GameObject(name);
            panelObj.transform.SetParent(parent, false);
            
            // Add components
            RectTransform rectTransform = panelObj.AddComponent<RectTransform>();
            
            // Add image with configurable color for debugging
            Image image = panelObj.AddComponent<Image>();
            image.color = _showPanelDebugBorders ? _panelDebugBorderColor : new Color(0, 0, 0, 0);
            
            return rectTransform;
        }
        
        /// <summary>
        /// Get the panel for a specific position
        /// </summary>
        /// <param name="position">The desired UI position</param>
        /// <returns>RectTransform for the corresponding panel</returns>
        public RectTransform GetPanelForPosition(UIPosition position)
        {
            switch (position)
            {
                case UIPosition.Top:
                    return _topPanel;
                case UIPosition.Bottom:
                    return _bottomPanel;
                case UIPosition.Left:
                    return _leftPanel;
                case UIPosition.Right:
                    return _rightPanel;
                case UIPosition.Center:
                default:
                    return _centerPanel;
            }
        }
        
        /// <summary>
        /// Resize a panel to the specified size
        /// </summary>
        /// <param name="position">The panel to resize</param>
        /// <param name="size">The new size (for side panels: width, for top/bottom: height)</param>
        public void ResizePanel(UIPosition position, float size)
        {
            RectTransform panel = GetPanelForPosition(position);
            if (panel == null)
            {
                Debug.LogError($"Cannot resize panel: No panel found for position {position}");
                return;
            }
            
            switch (position)
            {
                case UIPosition.Top:
                case UIPosition.Bottom:
                    // For top/bottom panels, change the height
                    panel.sizeDelta = new Vector2(panel.sizeDelta.x, size);
                    break;
                    
                case UIPosition.Left:
                case UIPosition.Right:
                    // For left/right panels, change the width
                    panel.sizeDelta = new Vector2(size, panel.sizeDelta.y);
                    break;
                    
                case UIPosition.Center:
                    // For center panel, both dimensions
                    Debug.LogWarning("Use ResizeCenterPanel for center panel to specify both dimensions");
                    break;
            }
        }
        
        /// <summary>
        /// Resize the center panel with specific width and height
        /// </summary>
        /// <param name="width">The new width</param>
        /// <param name="height">The new height</param>
        public void ResizeCenterPanel(float width, float height)
        {
            if (_centerPanel != null)
            {
                _centerPanel.sizeDelta = new Vector2(width, height);
            }
            else
            {
                Debug.LogError("Cannot resize center panel: Panel not found");
            }
        }
        
        /// <summary>
        /// Configure horizontal layout on a panel
        /// </summary>
        public void ConfigureHorizontalLayout(RectTransform parent)
        {
            HorizontalLayoutGroup layout = parent.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
            {
                layout = parent.gameObject.AddComponent<HorizontalLayoutGroup>();
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.spacing = _defaultElementSpacing;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = true;
                layout.padding = new RectOffset(_panelPadding, _panelPadding, 5, 5);
            }
        }
        
        /// <summary>
        /// Configure vertical layout on a panel
        /// </summary>
        public void ConfigureVerticalLayout(RectTransform parent)
        {
            VerticalLayoutGroup vertLayout = parent.GetComponent<VerticalLayoutGroup>();
            if (vertLayout == null)
            {
                vertLayout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
                vertLayout.childAlignment = TextAnchor.MiddleCenter;
                vertLayout.spacing = _defaultElementSpacing;
                vertLayout.childForceExpandWidth = true;
                vertLayout.childForceExpandHeight = false;
                vertLayout.padding = new RectOffset(5, 5, _panelPadding, _panelPadding);
            }
        }
        
        /// <summary>
        /// Configure grid layout on a panel
        /// </summary>
        public void ConfigureGridLayout(RectTransform parent)
        {
            GridLayoutGroup gridLayout = parent.GetComponent<GridLayoutGroup>();
            if (gridLayout == null)
            {
                gridLayout = parent.gameObject.AddComponent<GridLayoutGroup>();
                gridLayout.cellSize = new Vector2(100, 100);
                gridLayout.spacing = new Vector2(_defaultElementSpacing, _defaultElementSpacing);
                gridLayout.childAlignment = TextAnchor.MiddleCenter;
            }
        }
        
        /// <summary>
        /// Get all panels as property references
        /// </summary>
        public (RectTransform top, RectTransform bottom, RectTransform left, 
                RectTransform right, RectTransform center) GetAllPanels()
        {
            return (_topPanel, _bottomPanel, _leftPanel, _rightPanel, _centerPanel);
        }
    }
}