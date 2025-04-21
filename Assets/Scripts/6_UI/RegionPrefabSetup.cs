using UnityEngine;


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
        public TMPro.TextMeshPro nameText;
        public TMPro.TextMeshPro wealthText;
        public TMPro.TextMeshPro productionText;
        
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
                mainRenderer.sprite = Resources.Load<Sprite>("Square") ?? CreateDefaultSprite();
                mainRenderer.color = Color.white;
                Debug.Log("Created main renderer");
            }
            
            // Create highlight renderer if needed
            if (highlightRenderer == null)
            {
                GameObject highlightObj = new GameObject("HighlightRenderer");
                highlightObj.transform.SetParent(transform);
                highlightObj.transform.localPosition = Vector3.zero;
                highlightRenderer = highlightObj.AddComponent<SpriteRenderer>();
                highlightRenderer.sprite = Resources.Load<Sprite>("Square") ?? CreateDefaultSprite();
                highlightRenderer.color = Color.yellow;
                highlightRenderer.enabled = false;
                highlightObj.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
                highlightObj.transform.SetSiblingIndex(0); // Make sure it's behind the main renderer
                Debug.Log("Created highlight renderer");
            }
            
            // Create text fields if needed
            if (nameText == null)
                nameText = CreateTextMeshPro("NameText", new Vector3(0, 0.7f, 0), 12);
                
            if (wealthText == null)
                wealthText = CreateTextMeshPro("WealthText", new Vector3(-0.3f, 0, 0), 10);
                
            if (productionText == null)
                productionText = CreateTextMeshPro("ProductionText", new Vector3(0.3f, 0, 0), 10);
                
            // Reference all components in RegionView
            regionView.mainRenderer = mainRenderer;
            regionView.highlightRenderer = highlightRenderer;
            regionView.nameText = nameText;
            regionView.wealthText = wealthText;
            regionView.productionText = productionText;
            
            // Add BoxCollider2D for mouse interaction if needed
            BoxCollider2D boxCollider = gameObject.GetComponent<BoxCollider2D>();
            if (boxCollider == null)
            {
                boxCollider = gameObject.AddComponent<BoxCollider2D>();
                boxCollider.size = new Vector2(1, 1);
                Debug.Log("Added BoxCollider2D for mouse interaction");
            }
            
            Debug.Log("Region prefab setup complete!");
        }
        
        private TMPro.TextMeshPro CreateTextMeshPro(string name, Vector3 position, int fontSize)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(transform);
            textObj.transform.localPosition = position;
            TMPro.TextMeshPro tmp = textObj.AddComponent<TMPro.TextMeshPro>();
            tmp.text = name;
            tmp.fontSize = fontSize;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = Color.white;
            Debug.Log($"Created {name} TextMeshPro");
            return tmp;
        }
        
        private Sprite CreateDefaultSprite()
        {
            // Create a default white square texture
            Texture2D texture = new Texture2D(32, 32);
            Color[] colors = new Color[32 * 32];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Color.white;
            texture.SetPixels(colors);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        }
    }
}