using UnityEngine;
using UnityEditor;
using System.IO;
using Core;

public class CreateGameSettingsAsset
{
    [MenuItem("Tools/Create DefaultGameSettings in GameConfig")]
    public static void CreateDefaultGameSettings()
    {
        // First check if we already have the asset in Resources but just in the wrong location
        var existingSettings = Resources.Load<GameSettings>("DefaultGameSettings");
        
        // Config paths
        string configFolderPath = "Assets/Resources/GameConfig";
        string assetPath = configFolderPath + "/DefaultGameSettings.asset";
        
        // Create the GameSettings asset
        GameSettings settings;
        
        if (existingSettings != null)
        {
            Debug.Log("Found existing DefaultGameSettings, moving it to GameConfig folder");
            settings = Object.Instantiate(existingSettings);
        }
        else
        {
            Debug.Log("Creating new DefaultGameSettings asset");
            settings = ScriptableObject.CreateInstance<GameSettings>();
            // Initialize with default values
            settings.ValidateSettings();
        }
        
        // Ensure the directory exists
        if (!AssetDatabase.IsValidFolder(configFolderPath))
        {
            string[] folderPaths = configFolderPath.Split('/');
            string currentPath = folderPaths[0];
            
            for (int i = 1; i < folderPaths.Length; i++)
            {
                string folderName = folderPaths[i];
                string parentFolder = currentPath;
                currentPath = currentPath + "/" + folderName;
                
                if (!AssetDatabase.IsValidFolder(currentPath))
                {
                    AssetDatabase.CreateFolder(parentFolder, folderName);
                }
            }
        }
        
        // Create the asset file
        AssetDatabase.CreateAsset(settings, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("DefaultGameSettings asset created at: " + assetPath);
        
        // Select the created asset
        Selection.activeObject = settings;
    }
}