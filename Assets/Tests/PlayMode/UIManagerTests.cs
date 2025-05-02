using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UI;  // This imports the UIManager class

namespace Tests.PlayMode
{
    public class UIManagerTests
    {
        private GameObject testGameObject;
        private UIManager uiManager;
        
        [SetUp]
        public void SetUp()
        {
            // Create a GameObject with UIManager for testing
            testGameObject = new GameObject("UIManagerTestObject");
            uiManager = testGameObject.AddComponent<UIManager>();
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up test objects
            Object.Destroy(testGameObject);
            uiManager = null;
        }
        
        [UnityTest]
        public IEnumerator UIManager_Initialize_CreatesUILayout()
        {
            // Act
            uiManager.Initialize();
            
            // Wait one frame for any layout operations to complete
            yield return null;
            
            // Assert - verify panels exist
            // We're not testing specific panel references since they're private,
            // but we can test that UI elements exist in the GameObject's hierarchy
            Assert.IsTrue(testGameObject.transform.Find("TopPanel") != null, "Top panel should be created");
            Assert.IsTrue(testGameObject.transform.Find("BottomPanel") != null, "Bottom panel should be created");
            Assert.IsTrue(testGameObject.transform.Find("LeftPanel") != null, "Left panel should be created");
            Assert.IsTrue(testGameObject.transform.Find("RightPanel") != null, "Right panel should be created");
            Assert.IsTrue(testGameObject.transform.Find("CenterPanel") != null, "Center panel should be created");
        }
        
        // Add more tests for UIManager functionality here
    }
}