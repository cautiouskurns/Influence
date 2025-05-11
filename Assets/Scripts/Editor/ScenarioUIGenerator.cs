using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;
using System.IO;
using Scenarios;

namespace Editor
{
    public class ScenarioUIGenerator : EditorWindow
    {

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
            EditorGUILayout.HelpBox("This tool creates a simplified ScenarioUI with text display and advance button.", MessageType.Info);
            EditorGUILayout.Space();

            autoWireReferences = EditorGUILayout.Toggle("Auto-wire References", autoWireReferences);

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Simple Scenario UI"))
            {
                GenerateSimpleScenarioUI();
            }
        }

        private void GenerateSimpleScenarioUI()
        {
            // 1. Find Canvas or create one
            Canvas canvas = FindFirstObjectByType<Canvas>();
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
            
            // 3. Create Info Text in top-left corner
            GameObject textObj = new GameObject("ScreenInfoText");
            textObj.transform.SetParent(scenarioUIObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 1);
            textRect.anchorMax = new Vector2(0, 1);
            textRect.pivot = new Vector2(0, 1);
            textRect.sizeDelta = new Vector2(500, 200);
            textRect.anchoredPosition = new Vector2(10, -10);
            
            TextMeshProUGUI screenInfoText = textObj.AddComponent<TextMeshProUGUI>();
            screenInfoText.fontSize = 24;
            screenInfoText.color = Color.white;
            screenInfoText.alignment = TextAlignmentOptions.TopLeft;
            screenInfoText.text = "No scenario loaded";
            
            // 4. Create Advance Turn Button in top-right corner
            GameObject buttonObj = new GameObject("AdvanceTurnButton");
            buttonObj.transform.SetParent(scenarioUIObj.transform, false);
            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(1, 1);
            buttonRect.anchorMax = new Vector2(1, 1);
            buttonRect.pivot = new Vector2(1, 1);
            buttonRect.sizeDelta = new Vector2(150, 50);
            buttonRect.anchoredPosition = new Vector2(-20, -20);
            
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.6f, 0.2f, 1f);
            
            Button advanceTurnButton = buttonObj.AddComponent<Button>();
            ColorBlock colors = advanceTurnButton.colors;
            colors.highlightedColor = new Color(0.3f, 0.7f, 0.3f, 1f);
            colors.pressedColor = new Color(0.1f, 0.5f, 0.1f, 1f);
            advanceTurnButton.colors = colors;
            
            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(buttonObj.transform, false);
            RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "Advance Turn";
            btnText.fontSize = 18;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = Color.white;
            
            // Wire up the references
            if (autoWireReferences)
            {
                SerializedObject so = new SerializedObject(scenarioUI);
                so.FindProperty("screenInfoText").objectReferenceValue = screenInfoText;
                so.FindProperty("advanceTurnButton").objectReferenceValue = advanceTurnButton;
                so.ApplyModifiedProperties();
            }
            
            // Ensure the button starts disabled until a scenario is loaded
            advanceTurnButton.gameObject.SetActive(false);

            // Create a ScenarioTester component to quickly test scenarios
            GameObject testerObj = new GameObject("ScenarioTester");
            ScenarioTester tester = testerObj.AddComponent<ScenarioTester>();
            
            // Wire up the ScenarioTester
            SerializedObject testerSO = new SerializedObject(tester);
            testerSO.FindProperty("autoGenerateUI").boolValue = false; // We already created the UI
            
            // Try to find any test scenarios in Resources
            TestScenario[] scenarios = Resources.LoadAll<TestScenario>("Scenarios");
            if (scenarios != null && scenarios.Length > 0) 
            {
                testerSO.FindProperty("testScenario").objectReferenceValue = scenarios[0];
                Debug.Log($"Found and linked existing test scenario: {scenarios[0].name}");
            }
            else 
            {
                testerSO.FindProperty("autoCreateTestScenario").boolValue = true;
            }
            
            testerSO.ApplyModifiedProperties();

            Debug.Log("Simple ScenarioUI created successfully! A ScenarioTester has also been added to help test scenarios.");
        }
    }
}