using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UI;
using UI.MapComponents;
using Managers;
using Tests;
using Entities;
using System;

namespace PlayModeTests
{
    [TestFixture]
    public class RegionColorServicePlayModeTests
    {
        // Test objects
        private GameObject serviceObj;
        private RegionColorService colorService;
        private GameObject controllerObj;
        private MapColorController colorController;

        // Dictionary to track created test regions
        private Dictionary<string, GameObject> testRegions = new Dictionary<string, GameObject>();
        private Dictionary<string, RegionView> regionViews = new Dictionary<string, RegionView>();

        [UnitySetUp]
        public IEnumerator Setup()
        {
            // Create RegionColorService
            serviceObj = new GameObject("RegionColorService");
            colorService = serviceObj.AddComponent<RegionColorService>();

            // Configure service with test values
            TestHelper.SetPrivateField(colorService, "defaultRegionColor", Color.gray);
            TestHelper.SetPrivateField(colorService, "wealthMinColor", Color.red);
            TestHelper.SetPrivateField(colorService, "wealthMaxColor", Color.green);
            TestHelper.SetPrivateField(colorService, "productionMinColor", Color.blue);
            TestHelper.SetPrivateField(colorService, "productionMaxColor", Color.yellow);
            TestHelper.SetPrivateField(colorService, "nationDefaultColor", Color.white);

            // Create MapColorController
            controllerObj = new GameObject("MapColorController");
            colorController = controllerObj.AddComponent<MapColorController>();

            yield return null; // Wait a frame for initialization to complete

            // Create test region views
            CreateTestRegions();

            // Initialize service with region views
            TestHelper.SetPrivateField(colorService, "regionViews", regionViews);
            colorService.Initialize(regionViews, 10, 10);

            yield return null;
        }

        private void CreateTestRegions()
        {
            // Create a few test regions with RegionView components
            for (int i = 0; i < 5; i++)
            {
                string regionId = $"region_{i}_{i}";
                GameObject regionObj = new GameObject(regionId);
                RegionView view = regionObj.AddComponent<RegionView>();

                // Add renderer component to display the color
                MeshRenderer renderer = regionObj.AddComponent<MeshRenderer>();
                regionObj.AddComponent<MeshFilter>();
                renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                
                // Add region data component for wealth and production tests
                RegionDataComponent regionData = regionObj.AddComponent<RegionDataComponent>();
                TestHelper.SetPrivateField(regionData, "wealth", i * 100); // Different wealth values
                TestHelper.SetPrivateField(regionData, "production", i * 50); // Different production values

                // Store references to the test objects
                testRegions[regionId] = regionObj;
                regionViews[regionId] = view;
            }
        }

        [UnityTearDown]
        public IEnumerator Teardown()
        {
            // Clean up test objects
            foreach (var obj in testRegions.Values)
            {
                UnityEngine.Object.Destroy(obj);
            }
            testRegions.Clear();
            regionViews.Clear();

            UnityEngine.Object.Destroy(serviceObj);
            UnityEngine.Object.Destroy(controllerObj);

            yield return null;
        }

        [UnityTest]
        public IEnumerator ColorMode_ChangeUpdatesAllRegionColors()
        {
            // Store initial colors
            Dictionary<string, Color> initialColors = new Dictionary<string, Color>();
            foreach (var kvp in regionViews)
            {
                initialColors[kvp.Key] = kvp.Value.GetColor();
            }

            // Switch color mode
            colorService.SetColorMode(RegionColorMode.Position);
            
            yield return null; // Wait for color updates to apply

            // Verify colors changed
            int changedCount = 0;
            foreach (var kvp in regionViews)
            {
                Color currentColor = kvp.Value.GetColor();
                if (!ColorApproximatelyEqual(initialColors[kvp.Key], currentColor))
                {
                    changedCount++;
                }
            }

            // Assert at least one color changed
            Assert.Greater(changedCount, 0, "No region colors changed after switching color mode");
        }

        [UnityTest]
        public IEnumerator ColorController_ChangesRegionColorMode()
        {
            // Setup controller correctly and initialize
            yield return null;

            // Get initial color mode
            RegionColorMode initialMode = colorService.GetColorMode();
            
            // Switch to a different mode
            RegionColorMode newMode = initialMode == RegionColorMode.Default ? 
                RegionColorMode.Nation : RegionColorMode.Default;
            
            // Use controller to change color mode
            colorController.SetColorMode(newMode);
            
            yield return null; // Wait a frame for changes to apply
            
            // Verify service color mode was updated
            Assert.AreEqual(newMode, colorService.GetColorMode(), 
                "Color mode was not updated correctly through controller");
        }

        [UnityTest]
        public IEnumerator RegionColorService_SingletonInstance_IsAccessible()
        {
            // Verify we can get the instance through the singleton pattern
            RegionColorService instance = RegionColorService.Instance;
            
            // Verify instance matches our test object
            Assert.IsNotNull(instance);
            Assert.AreEqual(serviceObj.GetInstanceID(), instance.gameObject.GetInstanceID(), 
                "Singleton instance doesn't match our created test object");
            
            yield return null;
        }

        [UnityTest]
        public IEnumerator EventBus_TriggerUpdateMapColors_UpdatesAllRegions()
        {
            // Store initial colors
            Dictionary<string, Color> initialColors = new Dictionary<string, Color>();
            foreach (var kvp in regionViews)
            {
                initialColors[kvp.Key] = kvp.Value.GetColor();
            }

            // Switch to a different color mode to ensure colors will change
            colorService.SetColorMode(RegionColorMode.Position);
            
            // Change back to initial mode but through event
            colorService.SetColorMode(RegionColorMode.Default);
            
            // Trigger update through event bus (which the service subscribes to)
            EventBus.Trigger("UpdateMapColors", null);
            
            yield return null; // Wait for event handlers to process
            
            // Verify at least some regions were updated
            int updatedCount = 0;
            foreach (var kvp in regionViews)
            {
                RegionView view = kvp.Value;
                // Use approximate equality since colors can be slightly different due to floating point
                if (!ColorApproximatelyEqual(view.GetColor(), initialColors[kvp.Key]))
                {
                    updatedCount++;
                }
            }
            
            // At least one region should have updated
            Assert.Greater(updatedCount, 0, "No regions updated after EventBus trigger");
        }

        // Helper method for approximate color comparison
        private bool ColorApproximatelyEqual(Color a, Color b, float tolerance = 0.01f)
        {
            return Mathf.Abs(a.r - b.r) < tolerance &&
                   Mathf.Abs(a.g - b.g) < tolerance &&
                   Mathf.Abs(a.b - b.b) < tolerance &&
                   Mathf.Abs(a.a - b.a) < tolerance;
        }
    }
}