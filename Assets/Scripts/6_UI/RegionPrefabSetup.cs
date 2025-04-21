using UnityEngine;
using TMPro;
using UI;

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
        
        private void Awake()
        {
            // Ensure we have a sprite at runtime
            if (hexagonSprite != null)
            {
                sharedHexagonSprite = hexagonSprite;
            }
            else if (sharedHexagonSprite == null)
            {
                sharedHexagonSprite = CreateHexagonSprite();
            }
            
            // Apply the sprite to renderers if they exist
            if (mainRenderer != null && mainRenderer.sprite == null)
            {
                mainRenderer.sprite = sharedHexagonSprite;
            }
            
            if (highlightRenderer != null && highlightRenderer.sprite == null)
            {
                highlightRenderer.sprite = sharedHexagonSprite;
            }
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
                
                // Try to use the assigned sprite first, then look in Resources, then create one procedurally
                if (hexagonSprite != null)
                {
                    mainRenderer.sprite = hexagonSprite;
                }
                else
                {
                    Sprite loadedSprite = Resources.Load<Sprite>("Hexagon");
                    if (loadedSprite != null)
                    {
                        mainRenderer.sprite = loadedSprite;
                        hexagonSprite = loadedSprite; // Save for future use
                    }
                    else
                    {
                        hexagonSprite = CreateHexagonSprite();
                        mainRenderer.sprite = hexagonSprite;
                    }
                }
                
                mainRenderer.color = Color.white;
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
                highlightObj.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
                highlightObj.transform.SetSiblingIndex(0); // Make sure it's behind the main renderer
                Debug.Log("Created highlight renderer with hexagon sprite");
            }
            
            // Create text fields if needed
            if (nameText == null)
                nameText = CreateTextMeshPro("NameText", new Vector3(0, 0.5f, 0), 12);
                
            if (wealthText == null)
                wealthText = CreateTextMeshPro("WealthText", new Vector3(-0.2f, -0.1f, 0), 10);
                
            if (productionText == null)
                productionText = CreateTextMeshPro("ProductionText", new Vector3(0.2f, -0.1f, 0), 10);
                
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
            SetupHexagonCollider(hexCollider);
            Debug.Log("Added PolygonCollider2D for hexagon mouse interaction");
            
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
            Debug.Log($"Created {name} TextMeshPro");
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
        
        private void SetupHexagonCollider(PolygonCollider2D collider)
        {
            // Define hexagon points (flat-topped orientation)
            Vector2[] points = new Vector2[6];
            float radius = 0.5f;
            
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