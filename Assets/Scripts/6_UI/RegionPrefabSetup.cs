using UnityEngine;
using TMPro;

namespace UI
{
    /// <summary>
    /// Helper class to set up a region prefab with necessary components
    /// </summary>
    public class RegionPrefabSetup : MonoBehaviour
    {
        [Header("Required Components")]
        public SpriteRenderer mainRenderer;
        public SpriteRenderer highlightRenderer;
        public TextMeshPro nameText;
        public TextMeshPro wealthText;
        public TextMeshPro productionText;
        
        [Header("Sprite References")]
        [SerializeField] private Sprite hexagonSprite;
        private static Sprite sharedHexagonSprite;
        
        [Header("Size Settings")]
        [SerializeField] private float hexScale = 0.7f; // Scale factor for hex
        
        [Header("Text Settings")]
        [SerializeField] private int nameFontSize = 10; // Reduced from 14
        [SerializeField] private int statsFontSize = 8; // Reduced from 12
        [SerializeField] private float nameTextY = 0.25f; // Reduced from 0.4
        [SerializeField] private float statsTextY = -0.15f; // Moved down further
        
        private void Awake()
        {
            // Ensure we have a sprite at runtime
            if (hexagonSprite != null)
            {
                sharedHexagonSprite = hexagonSprite;
            }
            else if (sharedHexagonSprite == null)
            {
                // Try to load the project's Hexagon sprite first
                Sprite projectHexagon = Resources.Load<Sprite>("Hexagon");
                if (projectHexagon == null)
                {
                    projectHexagon = LoadHexagonFromAssets();
                }
                
                if (projectHexagon != null)
                {
                    sharedHexagonSprite = projectHexagon;
                }
                else
                {
                    // Fallback to creating a procedural sprite
                    sharedHexagonSprite = CreateHexagonSprite();
                }
            }
            
            // Apply the sprite to renderers if they exist
            if (mainRenderer != null && mainRenderer.sprite == null)
            {
                mainRenderer.sprite = sharedHexagonSprite;
                // Apply scale to the main renderer
                mainRenderer.transform.localScale = new Vector3(hexScale, hexScale, 1f);
            }
            
            if (highlightRenderer != null && highlightRenderer.sprite == null)
            {
                highlightRenderer.sprite = sharedHexagonSprite;
                // Apply scale to the highlight renderer
                highlightRenderer.transform.localScale = new Vector3(hexScale * 1.1f, hexScale * 1.1f, 1f);
            }
            
            // Adjust collider to match the new scale
            PolygonCollider2D hexCollider = GetComponent<PolygonCollider2D>();
            if (hexCollider != null)
            {
                SetupHexagonCollider(hexCollider, hexScale);
            }
        }
        
        private Sprite LoadHexagonFromAssets()
        {
            // Try to load the Hexagon.png file directly from the assets folder
            // This requires the sprite to be in Assets folder
            return Resources.Load<Sprite>("Hexagon") ?? Resources.LoadAll<Sprite>("")[0];
        }
        
        [ContextMenu("Setup Region Prefab")]
        public void SetupPrefab()
        {
            // Make sure we have a RegionView component
            RegionView regionView = gameObject.GetComponent<RegionView>();
            if (regionView == null)
            {
                regionView = gameObject.AddComponent<RegionView>();
                Debug.Log("Added RegionView component to prefab");
            }
            
            // Create main renderer if needed
            if (mainRenderer == null)
            {
                GameObject mainObj = new GameObject("MainRenderer");
                mainObj.transform.SetParent(transform);
                mainObj.transform.localPosition = Vector3.zero;
                mainRenderer = mainObj.AddComponent<SpriteRenderer>();
                
                // Try to use the existing project Hexagon sprite
                Sprite projectHexagon = LoadHexagonFromAssets();
                
                if (projectHexagon != null)
                {
                    mainRenderer.sprite = projectHexagon;
                    hexagonSprite = projectHexagon;
                    Debug.Log("Using project's Hexagon sprite");
                }
                else if (hexagonSprite != null)
                {
                    mainRenderer.sprite = hexagonSprite;
                }
                else
                {
                    hexagonSprite = CreateHexagonSprite();
                    mainRenderer.sprite = hexagonSprite;
                    Debug.Log("Created procedural hexagon sprite as fallback");
                }
                
                mainRenderer.color = Color.white;
                // Apply scale to the main renderer
                mainRenderer.transform.localScale = new Vector3(hexScale, hexScale, 1f);
                Debug.Log("Created main renderer with hexagon sprite");
            }
            
            // Create highlight renderer if needed
            if (highlightRenderer == null)
            {
                GameObject highlightObj = new GameObject("HighlightRenderer");
                highlightObj.transform.SetParent(transform);
                highlightObj.transform.localPosition = Vector3.zero;
                highlightRenderer = highlightObj.AddComponent<SpriteRenderer>();
                highlightRenderer.sprite = mainRenderer.sprite; // Use the same sprite
                highlightRenderer.color = Color.yellow;
                highlightRenderer.enabled = false;
                // Make highlight slightly larger than the main hexagon
                highlightRenderer.transform.localScale = new Vector3(hexScale * 1.1f, hexScale * 1.1f, 1f);
                highlightObj.transform.SetSiblingIndex(0); // Make sure it's behind the main renderer
                Debug.Log("Created highlight renderer with hexagon sprite");
            }
            
            // Create text fields if needed with adjusted positions and smaller font sizes for better fit
            if (nameText == null)
                nameText = CreateTextMeshPro("NameText", new Vector3(0, hexScale * nameTextY, 0), nameFontSize);
                
            if (wealthText == null)
                wealthText = CreateTextMeshPro("WealthText", new Vector3(-hexScale * 0.25f, hexScale * statsTextY, 0), statsFontSize);
                
            if (productionText == null)
                productionText = CreateTextMeshPro("ProductionText", new Vector3(hexScale * 0.25f, hexScale * statsTextY, 0), statsFontSize);
            
            // Reference all components in RegionView
            regionView.mainRenderer = mainRenderer;
            regionView.highlightRenderer = highlightRenderer;
            regionView.nameText = nameText;
            regionView.wealthText = wealthText;
            regionView.productionText = productionText;
            
            // Add PolygonCollider2D for mouse interaction with hexagon if needed
            Collider2D existingCollider = gameObject.GetComponent<Collider2D>();
            if (existingCollider != null)
            {
                DestroyImmediate(existingCollider);
            }
            
            PolygonCollider2D hexCollider = gameObject.AddComponent<PolygonCollider2D>();
            SetupHexagonCollider(hexCollider, hexScale);
            Debug.Log("Added PolygonCollider2D for hexagon mouse interaction");
            
            // Apply additional text formatting to improve readability
            if (nameText != null)
            {
                nameText.textWrappingMode = TextWrappingModes.NoWrap;
                nameText.overflowMode = TextOverflowModes.Ellipsis;
                nameText.margin = new Vector4(0, 0, 0, 0);
            }
            
            if (wealthText != null)
            {
                wealthText.textWrappingMode = TextWrappingModes.NoWrap;
                wealthText.overflowMode = TextOverflowModes.Truncate;
                wealthText.margin = new Vector4(0, 0, 0, 0);
            }
            
            if (productionText != null)
            {
                productionText.textWrappingMode = TextWrappingModes.NoWrap;
                productionText.overflowMode = TextOverflowModes.Truncate;
                productionText.margin = new Vector4(0, 0, 0, 0);
            }
            
            Debug.Log("Hexagon region prefab setup complete!");
        }
        
        private TextMeshPro CreateTextMeshPro(string name, Vector3 position, int fontSize)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(transform);
            textObj.transform.localPosition = position;
            TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
            tmp.text = name;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            // Set a smaller text width to fit within the hex
            RectTransform rect = tmp.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(hexScale * 0.8f, fontSize * 1.2f);
            }
            Debug.Log($"Created {name} TextMeshPro with fontSize {fontSize}");
            return tmp;
        }
        
        private Sprite CreateHexagonSprite()
        {
            // Create a hexagon texture
            int size = 128;
            Texture2D texture = new Texture2D(size, size);
            
            // Clear texture with transparent pixels
            Color[] colors = new Color[size * size];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Color.clear;
            texture.SetPixels(colors);
            
            // Draw hexagon
            float radius = size * 0.4f;
            Vector2 center = new Vector2(size / 2, size / 2);
            
            // Define hexagon points
            Vector2[] hexPoints = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                float angle = i * Mathf.PI / 3f;
                hexPoints[i] = center + new Vector2(
                    radius * Mathf.Cos(angle),
                    radius * Mathf.Sin(angle)
                );
            }
            
            // Draw the hexagon by filling triangles
            for (int i = 0; i < 6; i++)
            {
                int nextI = (i + 1) % 6;
                FillTriangle(texture, center, hexPoints[i], hexPoints[nextI], Color.white);
            }
            
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }
        
        private void FillTriangle(Texture2D tex, Vector2 p1, Vector2 p2, Vector2 p3, Color color)
        {
            // Get bounding rectangle of the triangle
            int minX = Mathf.FloorToInt(Mathf.Min(p1.x, Mathf.Min(p2.x, p3.x)));
            int maxX = Mathf.CeilToInt(Mathf.Max(p1.x, Mathf.Max(p2.x, p3.x)));
            int minY = Mathf.FloorToInt(Mathf.Min(p1.y, Mathf.Min(p2.y, p3.y)));
            int maxY = Mathf.CeilToInt(Mathf.Max(p1.y, Mathf.Max(p2.y, p3.y)));
            
            // Clip bounds to texture dimensions
            minX = Mathf.Max(0, minX);
            maxX = Mathf.Min(tex.width - 1, maxX);
            minY = Mathf.Max(0, minY);
            maxY = Mathf.Min(tex.height - 1, maxY);
            
            // Check each pixel in the bounding rectangle
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Vector2 p = new Vector2(x, y);
                    
                    // Check if pixel is inside the triangle
                    if (IsPointInTriangle(p, p1, p2, p3))
                    {
                        tex.SetPixel(x, y, color);
                    }
                }
            }
        }
        
        private bool IsPointInTriangle(Vector2 p, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            // Compute barycentric coordinates
            float d1 = Sign(p, p1, p2);
            float d2 = Sign(p, p2, p3);
            float d3 = Sign(p, p3, p1);
            
            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
            
            // Point is inside if all coordinates have the same sign
            return !(hasNeg && hasPos);
        }
        
        private float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }
        
        private void SetupHexagonCollider(PolygonCollider2D collider, float scale = 1.0f)
        {
            // Define hexagon points (flat-topped orientation)
            Vector2[] points = new Vector2[6];
            float radius = 0.5f * scale;
            
            for (int i = 0; i < 6; i++)
            {
                float angle = i * Mathf.PI / 3f;
                points[i] = new Vector2(
                    radius * Mathf.Cos(angle), 
                    radius * Mathf.Sin(angle)
                );
            }
            
            collider.points = points;
        }
    }
}