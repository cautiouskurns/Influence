using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Editor utility for importing and organizing sprite assets for testing
/// </summary>
public class SpriteAssetImporter : EditorWindow
{
    // Source folder paths
    private string hexTilesPath = "Hex World Tiles - Free";
    private string resourceFolderPath = "Assets/Resources";
    
    // Target folders for organization
    private string targetResourceFolder = "HexSprites";
    
    // List of found sprites
    private List<Sprite> foundSprites = new List<Sprite>();
    private Vector2 scrollPosition;
    private bool showAllSprites = false;
    
    [MenuItem("Tools/Sprite Asset Importer")]
    public static void ShowWindow()
    {
        GetWindow<SpriteAssetImporter>("Sprite Importer");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Hex Sprite Asset Importer", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Hex Tiles folder path
        EditorGUILayout.LabelField("Hex Tiles Asset Path:", EditorStyles.boldLabel);
        hexTilesPath = EditorGUILayout.TextField(hexTilesPath);
        
        // Resources folder path
        EditorGUILayout.LabelField("Resources Folder:", EditorStyles.boldLabel);
        resourceFolderPath = EditorGUILayout.TextField(resourceFolderPath);
        
        // Target resource subfolder
        EditorGUILayout.LabelField("Target Resource Subfolder:", EditorStyles.boldLabel);
        targetResourceFolder = EditorGUILayout.TextField(targetResourceFolder);
        
        EditorGUILayout.Space();
        
        // Buttons for main operations
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Find Hex Sprites", GUILayout.Height(30)))
        {
            FindAllHexSprites();
        }
        
        if (GUILayout.Button("Copy to Resources", GUILayout.Height(30)))
        {
            CopySpriteAssetsToResources();
        }
        
        if (GUILayout.Button("Create Sprite Test Asset", GUILayout.Height(30)))
        {
            CreateSpriteTestAsset();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Toggle for showing all sprites
        showAllSprites = EditorGUILayout.Toggle("Show All Found Sprites", showAllSprites);
        
        // Display found sprites
        if (foundSprites.Count > 0 && showAllSprites)
        {
            EditorGUILayout.LabelField($"Found {foundSprites.Count} sprites:", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
            
            float thumbnailSize = 64;
            int columns = Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - 50) / thumbnailSize);
            int i = 0;
            
            EditorGUILayout.BeginHorizontal();
            foreach (Sprite sprite in foundSprites)
            {
                // Create a new row after every 'columns' sprites
                if (i > 0 && i % columns == 0)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
                
                // Display sprite thumbnail
                GUILayout.Box(sprite.texture, GUILayout.Width(thumbnailSize), GUILayout.Height(thumbnailSize));
                i++;
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndScrollView();
        }
    }
    
    /// <summary>
    /// Find all hex sprites in the assets folder
    /// </summary>
    private void FindAllHexSprites()
    {
        foundSprites.Clear();
        
        string[] guids = AssetDatabase.FindAssets("t:sprite", new[] { "Assets/" + hexTilesPath });
        int spriteCount = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            
            if (sprite != null)
            {
                foundSprites.Add(sprite);
                spriteCount++;
            }
        }
        
        if (spriteCount > 0)
        {
            Debug.Log($"Found {spriteCount} sprites in {hexTilesPath}");
        }
        else
        {
            // Try broader search if no sprites found
            guids = AssetDatabase.FindAssets("t:sprite");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.ToLower().Contains("hex") || path.ToLower().Contains("tile"))
                {
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sprite != null)
                    {
                        foundSprites.Add(sprite);
                        spriteCount++;
                    }
                }
            }
            
            Debug.Log($"Found {spriteCount} hex/tile sprites across all project assets");
        }
        
        // If still no sprites found, give warning
        if (spriteCount == 0)
        {
            EditorUtility.DisplayDialog("No Sprites Found", 
                "Could not find any sprite assets in the specified path or anywhere in the project.", 
                "OK");
        }
        
        Repaint();
    }
    
    /// <summary>
    /// Copy sprite assets to Resources folder for runtime access
    /// </summary>
    private void CopySpriteAssetsToResources()
    {
        if (foundSprites.Count == 0)
        {
            FindAllHexSprites();
            
            if (foundSprites.Count == 0)
            {
                EditorUtility.DisplayDialog("No Sprites Found", 
                    "Please find hex sprites first before copying to Resources.", 
                    "OK");
                return;
            }
        }
        
        // Create Resources directory if it doesn't exist
        if (!Directory.Exists(resourceFolderPath))
        {
            Directory.CreateDirectory(resourceFolderPath);
            AssetDatabase.Refresh();
        }
        
        // Create subfolder in Resources if specified
        string targetPath = resourceFolderPath;
        if (!string.IsNullOrEmpty(targetResourceFolder))
        {
            targetPath = Path.Combine(resourceFolderPath, targetResourceFolder);
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
                AssetDatabase.Refresh();
            }
        }
        
        // Copy each sprite to Resources
        int copiedCount = 0;
        foreach (Sprite sprite in foundSprites)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sprite);
            string fileName = Path.GetFileName(sourcePath);
            string destPath = Path.Combine(targetPath, fileName);
            
            // Only copy if the file doesn't already exist
            if (!File.Exists(destPath))
            {
                AssetDatabase.CopyAsset(sourcePath, destPath);
                copiedCount++;
            }
        }
        
        AssetDatabase.Refresh();
        
        if (copiedCount > 0)
        {
            EditorUtility.DisplayDialog("Copy Complete", 
                $"Successfully copied {copiedCount} sprite assets to the Resources folder.", 
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("No Copy Needed", 
                "All sprites already exist in the Resources folder.", 
                "OK");
        }
    }
    
    /// <summary>
    /// Create a test asset that contains references to hex sprites for runtime testing
    /// </summary>
    private void CreateSpriteTestAsset()
    {
        if (foundSprites.Count == 0)
        {
            FindAllHexSprites();
            
            if (foundSprites.Count == 0)
            {
                EditorUtility.DisplayDialog("No Sprites Found", 
                    "Please find hex sprites first before creating a test asset.", 
                    "OK");
                return;
            }
        }
        
        // Create a ScriptableObject to hold sprite references
        UI.SpriteTestAsset asset = ScriptableObject.CreateInstance<UI.SpriteTestAsset>();
        asset.TestSprites = foundSprites.ToArray();
        
        // Create Resources directory if it doesn't exist
        if (!Directory.Exists(resourceFolderPath))
        {
            Directory.CreateDirectory(resourceFolderPath);
            AssetDatabase.Refresh();
        }
        
        // Create the asset file
        string assetPath = Path.Combine(resourceFolderPath, "HexSpriteTestAsset.asset");
        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        
        // Select the created asset
        Selection.activeObject = asset;
        
        EditorUtility.DisplayDialog("Asset Created", 
            $"Created test asset with {foundSprites.Count} sprites at {assetPath}", 
            "OK");
    }
}