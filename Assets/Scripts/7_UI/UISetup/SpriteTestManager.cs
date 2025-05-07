using UnityEngine;
using System.Collections.Generic;

namespace UI
{
    /// <summary>
    /// Manager for testing different sprite variations on region prefabs at runtime
    /// </summary>
    public class SpriteTestManager : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool applyOnStart = false;
        [SerializeField] private bool applyRandomSpritesToAll = false;
        [SerializeField] private KeyCode randomizeKey = KeyCode.R;
        [SerializeField] private float randomizeInterval = 2.0f;
        
        [Header("References")]
        [SerializeField] private Sprite[] testSprites;
        [SerializeField] private Transform regionsContainer;
        
        private List<RegionPrefabSetup> regionPrefabs = new List<RegionPrefabSetup>();
        private float timeSinceLastRandomize = 0f;
        private bool autoRandomize = false;
        
        private void Start()
        {
            // Find all region prefabs in the scene
            FindAllRegionPrefabs();
            
            // Apply random sprites on start if enabled
            if (applyOnStart)
            {
                ApplyRandomSpritesToAllRegions();
            }
        }
        
        private void Update()
        {
            // Check for key press to randomize sprites
            if (Input.GetKeyDown(randomizeKey))
            {
                ApplyRandomSpritesToAllRegions();
            }
            
            // Toggle auto-randomize with Ctrl+R
            if (Input.GetKeyDown(randomizeKey) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                autoRandomize = !autoRandomize;
                Debug.Log($"Auto-randomize sprites: {(autoRandomize ? "Enabled" : "Disabled")}");
            }
            
            // Handle auto-randomizing
            if (autoRandomize)
            {
                timeSinceLastRandomize += Time.deltaTime;
                if (timeSinceLastRandomize >= randomizeInterval)
                {
                    ApplyRandomSpritesToAllRegions();
                    timeSinceLastRandomize = 0f;
                }
            }
        }
        
        /// <summary>
        /// Find all region prefabs in the scene
        /// </summary>
        public void FindAllRegionPrefabs()
        {
            regionPrefabs.Clear();
            
            // Try to find regions in the specified container
            if (regionsContainer != null)
            {
                foreach (Transform child in regionsContainer)
                {
                    RegionPrefabSetup setup = child.GetComponent<RegionPrefabSetup>();
                    if (setup != null)
                    {
                        regionPrefabs.Add(setup);
                    }
                }
            }
            
            // If no container specified or no regions found, search in entire scene
            if (regionPrefabs.Count == 0)
            {
                RegionPrefabSetup[] allRegions = FindObjectsOfType<RegionPrefabSetup>();
                regionPrefabs.AddRange(allRegions);
            }
            
            Debug.Log($"Found {regionPrefabs.Count} region prefabs in the scene");
        }
        
        /// <summary>
        /// Apply random sprites to all regions
        /// </summary>
        [ContextMenu("Apply Random Sprites")]
        public void ApplyRandomSpritesToAllRegions()
        {
            if (regionPrefabs.Count == 0)
            {
                FindAllRegionPrefabs();
            }
            
            // First ensure we have sprites to test with
            if (testSprites != null && testSprites.Length > 0)
            {
                // Share the test sprites with all region prefabs
                foreach (RegionPrefabSetup region in regionPrefabs)
                {
                    // Use reflection to set the random sprites array
                    System.Type type = region.GetType();
                    System.Reflection.FieldInfo field = type.GetField("randomHexagonSprites", 
                        System.Reflection.BindingFlags.Instance | 
                        System.Reflection.BindingFlags.NonPublic);
                    
                    if (field != null)
                    {
                        field.SetValue(region, testSprites);
                    }
                }
            }
            
            // Apply random sprites to each region
            foreach (RegionPrefabSetup region in regionPrefabs)
            {
                region.ApplyRandomSprite();
            }
            
            Debug.Log($"Applied random sprites to {regionPrefabs.Count} regions");
        }
    }
}