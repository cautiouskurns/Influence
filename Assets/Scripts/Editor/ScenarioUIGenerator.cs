using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;
using System.IO;
using Scenarios; // Add this to access ScenarioUI

namespace Editor
{
    public class ScenarioUIGenerator : EditorWindow
    {
        private GameObject scenarioItemPrefab;
        private bool createPrefab = true;
        private string prefabPath = "Assets/Prefabs/UI/ScenarioItemPrefab.prefab";
        private bool autoWireReferences = true;

        [MenuItem("Tools/Scenario UI Generator")]
        public static void ShowWindow()
        {
            GetWindow<ScenarioUIGenerator>("Scenario UI Generator");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Scenario UI Generator", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("This tool creates the full ScenarioUI hierarchy with all required components.", MessageType.Info);
            EditorGUILayout.Space();

            createPrefab = EditorGUILayout.Toggle("Create ScenarioItemPrefab", createPrefab);
            
            if (createPrefab)
            {
                prefabPath = EditorGUILayout.TextField("Prefab Path", prefabPath);
            }

            autoWireReferences = EditorGUILayout.Toggle("Auto-wire References", autoWireReferences);

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Scenario UI"))
            {
                GenerateScenarioUI();
            }
        }

        private void GenerateScenarioUI()
        {
            // Check if we need to create a prefab first
            if (createPrefab)
            {
                CreateScenarioItemPrefab();
            }

            // 1. Find Canvas or create one
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasObj.AddComponent<GraphicRaycaster>();
                Debug.Log("Created new Canvas.");
            }

            // 2. Create ScenarioUI parent GameObject
            GameObject scenarioUIObj = new GameObject("ScenarioUI");
            scenarioUIObj.transform.SetParent(canvas.transform, false);
            RectTransform scenarioUIRect = scenarioUIObj.AddComponent<RectTransform>();
            scenarioUIRect.anchorMin = Vector2.zero;
            scenarioUIRect.anchorMax = Vector2.one;
            scenarioUIRect.offsetMin = Vector2.zero;
            scenarioUIRect.offsetMax = Vector2.zero;
            
            // Add ScenarioUI component
            ScenarioUI scenarioUI = scenarioUIObj.AddComponent<ScenarioUI>();
            
            // 3. Create ScenarioInfoPanel
            GameObject infoPanel = CreateUIElement("ScenarioInfoPanel", scenarioUIObj.transform);
            RectTransform infoPanelRect = infoPanel.GetComponent<RectTransform>();
            infoPanelRect.anchorMin = new Vector2(0, 1);
            infoPanelRect.anchorMax = new Vector2(1, 1);
            infoPanelRect.pivot = new Vector2(0.5f, 1);
            infoPanelRect.sizeDelta = new Vector2(0, 150);
            infoPanelRect.anchoredPosition = Vector2.zero;
            
            // Add background image
            Image infoPanelImage = infoPanel.AddComponent<Image>();
            infoPanelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            // Create Title text
            TextMeshProUGUI titleText = CreateText("Title", infoPanel.transform, TextAlignmentOptions.Top, 30, new Vector2(0, -15));
            
            // Create Description text
            TextMeshProUGUI descText = CreateText("Description", infoPanel.transform, TextAlignmentOptions.TopLeft, 18, new Vector2(0, -60), new Vector2(0, -85));
            
            // Create TurnCounter text
            TextMeshProUGUI turnCounterText = CreateText("TurnCounter", infoPanel.transform, TextAlignmentOptions.TopRight, 20, new Vector2(-20, -20), new Vector2(-150, -30));
            
            // Create ObjectiveText text
            TextMeshProUGUI objectiveText = CreateText("ObjectiveText", infoPanel.transform, TextAlignmentOptions.TopLeft, 18, new Vector2(20, -110), new Vector2(0, -30));

            // 4. Create EventNotificationPanel
            GameObject eventPanel = CreateUIElement("EventNotificationPanel", scenarioUIObj.transform);
            RectTransform eventPanelRect = eventPanel.GetComponent<RectTransform>();
            eventPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            eventPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
            eventPanelRect.pivot = new Vector2(0.5f, 0.5f);
            eventPanelRect.sizeDelta = new Vector2(500, 300);
            eventPanelRect.anchoredPosition = Vector2.zero;
            
            // Add background image and make panel initially inactive
            Image eventPanelImage = eventPanel.AddComponent<Image>();
            eventPanelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);
            eventPanel.SetActive(false);
            
            // Create EventTitle text
            TextMeshProUGUI eventTitleText = CreateText("EventTitle", eventPanel.transform, TextAlignmentOptions.Top, 24, new Vector2(0, -25));
            
            // Create EventDescription text
            TextMeshProUGUI eventDescText = CreateText("EventDescription", eventPanel.transform, TextAlignmentOptions.TopLeft, 18, new Vector2(0, -100), new Vector2(0, -150));
            
            // Create CloseButton
            Button eventCloseButton = CreateButton("CloseButton", eventPanel.transform, "Close", new Vector2(0, -250), new Vector2(150, 40));

            // 5. Create VictoryPanel
            GameObject victoryPanel = CreateUIElement("VictoryPanel", scenarioUIObj.transform);
            RectTransform victoryPanelRect = victoryPanel.GetComponent<RectTransform>();
            victoryPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            victoryPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
            victoryPanelRect.pivot = new Vector2(0.5f, 0.5f);
            victoryPanelRect.sizeDelta = new Vector2(500, 300);
            victoryPanelRect.anchoredPosition = Vector2.zero;
            
            // Add background image and make panel initially inactive
            Image victoryPanelImage = victoryPanel.AddComponent<Image>();
            victoryPanelImage.color = new Color(0.1f, 0.4f, 0.1f, 0.95f);
            victoryPanel.SetActive(false);
            
            // Create VictoryText text
            TextMeshProUGUI victoryText = CreateText("VictoryText", victoryPanel.transform, TextAlignmentOptions.Center, 30, new Vector2(0, -100), new Vector2(0, -150));
            
            // Create ContinueButton
            Button victoryContinueButton = CreateButton("ContinueButton", victoryPanel.transform, "Continue", new Vector2(0, -230), new Vector2(150, 40));

            // 6. Create DefeatPanel
            GameObject defeatPanel = CreateUIElement("DefeatPanel", scenarioUIObj.transform);
            RectTransform defeatPanelRect = defeatPanel.GetComponent<RectTransform>();
            defeatPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            defeatPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
            defeatPanelRect.pivot = new Vector2(0.5f, 0.5f);
            defeatPanelRect.sizeDelta = new Vector2(500, 300);
            defeatPanelRect.anchoredPosition = Vector2.zero;
            
            // Add background image and make panel initially inactive
            Image defeatPanelImage = defeatPanel.AddComponent<Image>();
            defeatPanelImage.color = new Color(0.4f, 0.1f, 0.1f, 0.95f);
            defeatPanel.SetActive(false);
            
            // Create DefeatText text
            TextMeshProUGUI defeatText = CreateText("DefeatText", defeatPanel.transform, TextAlignmentOptions.Center, 30, new Vector2(0, -100), new Vector2(0, -150));
            
            // Create ContinueButton (same name is fine here)
            Button defeatContinueButton = CreateButton("ContinueButton", defeatPanel.transform, "Continue", new Vector2(0, -230), new Vector2(150, 40));

            // 7. Create ScenarioSelectionPanel
            GameObject selectionPanel = CreateUIElement("ScenarioSelectionPanel", scenarioUIObj.transform);
            RectTransform selectionPanelRect = selectionPanel.GetComponent<RectTransform>();
            selectionPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            selectionPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
            selectionPanelRect.pivot = new Vector2(0.5f, 0.5f);
            selectionPanelRect.sizeDelta = new Vector2(600, 400);
            selectionPanelRect.anchoredPosition = Vector2.zero;
            
            // Add background image and make panel initially inactive
            Image selectionPanelImage = selectionPanel.AddComponent<Image>();
            selectionPanelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);
            selectionPanel.SetActive(false);
            
            // Create SelectionTitle text
            TextMeshProUGUI selectionTitleText = CreateText("SelectionTitle", selectionPanel.transform, TextAlignmentOptions.Top, 24, new Vector2(0, -25));
            
            // Create ScrollView
            GameObject scrollView = CreateScrollView(selectionPanel.transform);
            GameObject content = scrollView.transform.Find("Viewport/ScenarioListContent").gameObject;
            
            // Add CloseButton at the bottom
            Button selectionCloseButton = CreateButton("CloseButton", selectionPanel.transform, "Close", new Vector2(0, -350), new Vector2(150, 40));
            
            // 8. Create ScenarioSelectionButton
            Button scenarioSelectionButton = CreateButton("ScenarioSelectionButton", scenarioUIObj.transform, "Scenarios", new Vector2(-80, -30), new Vector2(120, 40));
            RectTransform buttonRect = scenarioSelectionButton.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(1, 1);
            buttonRect.anchorMax = new Vector2(1, 1);
            
            // Wire up references if requested
            if (autoWireReferences)
            {
                WireReferences(
                    scenarioUI,
                    infoPanel,
                    titleText, 
                    descText,
                    turnCounterText,
                    objectiveText,
                    eventPanel,
                    eventTitleText,
                    eventDescText,
                    eventCloseButton,
                    victoryPanel,
                    victoryText,
                    victoryContinueButton,
                    defeatPanel,
                    defeatText, 
                    defeatContinueButton,
                    selectionPanel,
                    selectionTitleText,
                    content,
                    selectionCloseButton,
                    scenarioSelectionButton);
                
                // Set the prefab reference if we created one
                if (createPrefab && scenarioItemPrefab != null)
                {
                    SetScenarioItemPrefab(scenarioUI, scenarioItemPrefab);
                }
            }

            Debug.Log("ScenarioUI hierarchy created successfully!");
        }

        private void WireReferences(
            ScenarioUI scenarioUI,
            GameObject infoPanel,
            TextMeshProUGUI titleText,
            TextMeshProUGUI descriptionText,
            TextMeshProUGUI turnCounterText,
            TextMeshProUGUI objectiveText,
            GameObject eventPanel,
            TextMeshProUGUI eventTitleText,
            TextMeshProUGUI eventDescriptionText,
            Button eventCloseButton,
            GameObject victoryPanel,
            TextMeshProUGUI victoryText,
            Button victoryContinueButton,
            GameObject defeatPanel,
            TextMeshProUGUI defeatText,
            Button defeatContinueButton,
            GameObject selectionPanel,
            TextMeshProUGUI selectionTitleText,
            GameObject scenarioListContent,
            Button selectionCloseButton,
            Button scenarioSelectionButton)
        {
            // Use serialized object to modify serialized properties of ScenarioUI
            SerializedObject so = new SerializedObject(scenarioUI);
            
            // Set UI panels
            so.FindProperty("scenarioInfoPanel").objectReferenceValue = infoPanel;
            so.FindProperty("eventNotificationPanel").objectReferenceValue = eventPanel;
            so.FindProperty("victoryPanel").objectReferenceValue = victoryPanel;
            so.FindProperty("defeatPanel").objectReferenceValue = defeatPanel;
            so.FindProperty("scenarioSelectionPanel").objectReferenceValue = selectionPanel;
            
            // Set UI Text elements
            so.FindProperty("scenarioNameText").objectReferenceValue = titleText;
            so.FindProperty("descriptionText").objectReferenceValue = descriptionText;
            so.FindProperty("turnCounterText").objectReferenceValue = turnCounterText;
            so.FindProperty("objectiveText").objectReferenceValue = objectiveText;
            so.FindProperty("eventTitleText").objectReferenceValue = eventTitleText;
            so.FindProperty("eventDescriptionText").objectReferenceValue = eventDescriptionText;
            so.FindProperty("victoryText").objectReferenceValue = victoryText;
            so.FindProperty("defeatText").objectReferenceValue = defeatText;
            
            // Set buttons
            so.FindProperty("eventCloseButton").objectReferenceValue = eventCloseButton;
            so.FindProperty("victoryContinueButton").objectReferenceValue = victoryContinueButton;
            so.FindProperty("defeatContinueButton").objectReferenceValue = defeatContinueButton;
            so.FindProperty("scenarioCloseButton").objectReferenceValue = selectionCloseButton;
            so.FindProperty("scenarioSelectionButton").objectReferenceValue = scenarioSelectionButton;
            
            // Set content container
            so.FindProperty("scenarioListContent").objectReferenceValue = scenarioListContent;
            
            so.ApplyModifiedProperties();
            
            Debug.Log("Successfully wired ScenarioUI references!");
        }
        
        private void SetScenarioItemPrefab(ScenarioUI scenarioUI, GameObject prefab)
        {
            SerializedObject so = new SerializedObject(scenarioUI);
            so.FindProperty("scenarioItemPrefab").objectReferenceValue = prefab;
            so.ApplyModifiedProperties();
            
            Debug.Log("Set ScenarioItemPrefab reference in ScenarioUI.");
        }

        private GameObject CreateUIElement(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            return obj;
        }

        private TextMeshProUGUI CreateText(string name, Transform parent, TextAlignmentOptions alignment, 
            float fontSize, Vector2 position, Vector2? sizeDelta = null)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1);
            rect.anchorMax = new Vector2(0.5f, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.sizeDelta = sizeDelta ?? new Vector2(400, 50);
            rect.anchoredPosition = position;
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = name;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.alignment = alignment;
            
            return tmp;
        }

        private Button CreateButton(string name, Transform parent, string text, Vector2 position, Vector2 size)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);
            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            
            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            
            Button button = buttonObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            colors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            button.colors = colors;
            
            // Add text to button
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 20;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            
            return button;
        }

        private GameObject CreateScrollView(Transform parent)
        {
            // Create ScrollView
            GameObject scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(parent, false);
            RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.5f, 0.5f);
            scrollRect.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRect.pivot = new Vector2(0.5f, 0.5f);
            scrollRect.sizeDelta = new Vector2(500, 250);
            scrollRect.anchoredPosition = new Vector2(0, -150);
            
            Image scrollImage = scrollView.AddComponent<Image>();
            scrollImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            
            ScrollRect scrollRectComponent = scrollView.AddComponent<ScrollRect>();
            
            // Create Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(5, 5);
            viewportRect.offsetMax = new Vector2(-5, -5);
            viewportRect.pivot = new Vector2(0, 1);
            
            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            
            // Create Content
            GameObject content = new GameObject("ScenarioListContent");
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 300);  // Height will be determined by content
            
            // Add layout group
            VerticalLayoutGroup layoutGroup = content.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.spacing = 10;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            
            // Add content size fitter
            ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Set references
            scrollRectComponent.content = contentRect;
            scrollRectComponent.viewport = viewportRect;
            scrollRectComponent.horizontal = false;
            scrollRectComponent.vertical = true;
            scrollRectComponent.scrollSensitivity = 10;
            
            return scrollView;
        }

        private void CreateScenarioItemPrefab()
        {
            // Create the base GameObject
            GameObject prefabRoot = new GameObject("ScenarioItemPrefab");
            RectTransform rectTransform = prefabRoot.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(480, 80);  // Set the size to fit in the scroll view
            
            // Add background image
            Image bgImage = prefabRoot.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            // Add text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(prefabRoot.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "Scenario Title";
            tmp.fontSize = 20;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.color = Color.white;
            
            // Add button component
            Button button = prefabRoot.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            colors.pressedColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            button.colors = colors;
            
            // Create directory if it doesn't exist
            string directory = Path.GetDirectoryName(prefabPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Create the prefab
            #if UNITY_EDITOR
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            DestroyImmediate(prefabRoot);
            scenarioItemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Debug.Log("Created ScenarioItemPrefab at: " + prefabPath);
            #endif
        }
    }
}