using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UI.MapComponents;
using UI;
using Tests;
using Entities;
using Managers;
using Systems;

public class RegionColorServiceTests
{
    private RegionColorService colorService;
    private GameObject serviceObject;

    [SetUp]
    public void Setup()
    {
        // Create GameObject and add RegionColorService component
        serviceObject = new GameObject("RegionColorService");
        colorService = serviceObject.AddComponent<RegionColorService>();
        
        // Use reflection to set needed private fields
        TestHelper.SetPrivateField(colorService, "defaultRegionColor", Color.gray);
        TestHelper.SetPrivateField(colorService, "wealthMinColor", Color.red);
        TestHelper.SetPrivateField(colorService, "wealthMaxColor", Color.green);
        TestHelper.SetPrivateField(colorService, "productionMinColor", Color.blue);
        TestHelper.SetPrivateField(colorService, "productionMaxColor", Color.yellow);
        TestHelper.SetPrivateField(colorService, "nationDefaultColor", Color.white);
        
        // Wait for Awake to complete
        UnityEngine.TestTools.LogAssert.Expect(LogType.Log, "RegionColorService: Initialized!");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(serviceObject);
    }

    [Test]
    public void DefaultColorMode_ReturnsCorrectColor()
    {
        // Arrange
        Color expectedColor = Color.gray; // default color
        
        // Act
        Color result = colorService.GetRegionColor("region_0_0", 0, 0, 10, 10, RegionColorMode.Default);
        
        // Assert
        ColorAssert.AreEqual(expectedColor, result);
    }
    
    [Test]
    public void PositionColorMode_ReturnsColorBasedOnCoordinates()
    {
        // Arrange
        // Expected calculation: 0.4f + (float)q/gridWidth * 0.6f, 0.4f + (float)r/gridHeight * 0.6f, 0.5f
        Color expectedCenter = new Color(0.7f, 0.7f, 0.5f); // Mid-point (5,5) in a 10x10 grid
        Color expectedOrigin = new Color(0.4f, 0.4f, 0.5f); // (0,0) in a 10x10 grid
        Color expectedEdge = new Color(1.0f, 1.0f, 0.5f);   // (10,10) in a 10x10 grid
        
        // Act
        Color resultCenter = colorService.GetRegionColor("region_5_5", 5, 5, 10, 10, RegionColorMode.Position);
        Color resultOrigin = colorService.GetRegionColor("region_0_0", 0, 0, 10, 10, RegionColorMode.Position);
        Color resultEdge = colorService.GetRegionColor("region_10_10", 10, 10, 10, 10, RegionColorMode.Position);
        
        // Assert
        ColorAssert.AreEqual(expectedCenter, resultCenter, 0.01f);
        ColorAssert.AreEqual(expectedOrigin, resultOrigin, 0.01f);
        ColorAssert.AreEqual(expectedEdge, resultEdge, 0.01f);
    }
    
    [Test]
    public void TerrainMode_ReturnsDifferentColorsForDifferentCoordinates()
    {
        // Note: We can't easily predict Perlin noise results, so just verify different positions
        // have different colors, and that they're within expected ranges
        
        Color terrain1 = colorService.GetRegionColor("region_0_0", 0, 0, 10, 10, RegionColorMode.Terrain);
        Color terrain2 = colorService.GetRegionColor("region_10_10", 10, 10, 10, 10, RegionColorMode.Terrain);
        Color terrain3 = colorService.GetRegionColor("region_5_5", 5, 5, 10, 10, RegionColorMode.Terrain);
        
        // We expect colors to be somewhat different 
        // (at least one will likely be different due to Perlin noise distribution)
        bool allSame = ColorsSimilar(terrain1, terrain2, 0.001f) && 
                      ColorsSimilar(terrain2, terrain3, 0.001f) && 
                      ColorsSimilar(terrain1, terrain3, 0.001f);
                      
        Assert.IsFalse(allSame, "All terrain colors were identical when they should be different");
        
        // Every terrain color should have valid RGB components between 0-1
        ValidateColorRange(terrain1);
        ValidateColorRange(terrain2);
        ValidateColorRange(terrain3);
    }
    
    private bool ColorsSimilar(Color color1, Color color2, float tolerance)
    {
        return Mathf.Abs(color1.r - color2.r) < tolerance &&
               Mathf.Abs(color1.g - color2.g) < tolerance &&
               Mathf.Abs(color1.b - color2.b) < tolerance;
    }
    
    private void ValidateColorRange(Color color)
    {
        Assert.IsTrue(color.r >= 0 && color.r <= 1, "Red component out of range: " + color.r);
        Assert.IsTrue(color.g >= 0 && color.g <= 1, "Green component out of range: " + color.g);
        Assert.IsTrue(color.b >= 0 && color.b <= 1, "Blue component out of range: " + color.b);
    }
}

/// <summary>
/// Helper class for color assertions with better error messages
/// </summary>
public static class ColorAssert
{
    public static void AreEqual(Color expected, Color actual, float tolerance = 0.001f)
    {
        bool equal = Mathf.Abs(expected.r - actual.r) < tolerance &&
                    Mathf.Abs(expected.g - actual.g) < tolerance &&
                    Mathf.Abs(expected.b - actual.b) < tolerance &&
                    Mathf.Abs(expected.a - actual.a) < tolerance;
        
        if (!equal)
        {
            Assert.Fail($"Colors are not equal. Expected: ({expected.r:F3}, {expected.g:F3}, {expected.b:F3}, {expected.a:F3}), " +
                        $"Actual: ({actual.r:F3}, {actual.g:F3}, {actual.b:F3}, {actual.a:F3})");
        }
    }
}