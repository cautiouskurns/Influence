using UnityEngine;
using UnityEditor;
using System.IO;
using Core;

/// <summary>
/// Editor utility to create the DefaultGameSettings asset in Resources/GameConfig folder
/// </summary>
public class GameSettingsCreator
{
    private const string ResourcesPath = "Assets/Resources";
    private const string ConfigFolderName = "GameConfig";
    private const string DefaultSettingsName = "DefaultGameSettings";
    
    [MenuItem("Tools/Create Default Game Settings")]
    public static void CreateDefaultGameSettings()
    {
        // Create Resources folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder(ResourcesPath))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        
        // Create GameConfig subfolder if it doesn't exist
        string configPath = Path.Combine(ResourcesPath, ConfigFolderName);
        if (!AssetDatabase.IsValidFolder(configPath))
        {
            AssetDatabase.CreateFolder(ResourcesPath, ConfigFolderName);
        }
        
        // Create the asset
        string assetPath = Path.Combine(configPath, DefaultSettingsName + ".asset");
        
        // Check if asset already exists
        if (AssetDatabase.LoadAssetAtPath<Core.GameSettings>(assetPath) != null)
        {
            Debug.Log("DefaultGameSettings asset already exists at: " + assetPath);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Core.GameSettings>(assetPath);
            return;
        }
        
        // Create new settings asset
        Core.GameSettings settings = ScriptableObject.CreateInstance<Core.GameSettings>();
        
        // Initialize with default values (these will already be set by the class defaults)
        settings.ValidateSettings();
        
        // Create the asset file
        AssetDatabase.CreateAsset(settings, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("Created DefaultGameSettings asset at: " + assetPath);
        
        // Select the created asset in the Project window
        Selection.activeObject = settings;
    }
}