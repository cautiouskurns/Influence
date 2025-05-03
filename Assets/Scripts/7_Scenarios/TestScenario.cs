using UnityEngine;
using System.Collections.Generic;

namespace Scenarios
{
    [CreateAssetMenu(fileName = "TestScenario", menuName = "Influence/Test Scenario")]
    public class TestScenario : ScriptableObject
    {
        [Header("Scenario Settings")]
        public string scenarioName = "Test Scenario";
        
        [TextArea(3, 5)]
        public string description = "Description of this test scenario";
        
        public int turnLimit = 10;
        
        [Header("Starting Conditions")]
        public List<RegionStartCondition> regionStartConditions = new List<RegionStartCondition>();
        public List<NationStartCondition> nationStartConditions = new List<NationStartCondition>();
        
        [Header("Victory Conditions")]
        public VictoryCondition victoryCondition;
        
        /// <summary>
        /// Creates a procedurally generated scenario using default settings
        /// </summary>
        /// <returns>A new TestScenario with procedurally generated content</returns>
        public static TestScenario CreateProcedural()
        {
            ProceduralScenarioGenerator generator = ScriptableObject.CreateInstance<ProceduralScenarioGenerator>();
            return generator.GenerateScenario();
        }
        
        /// <summary>
        /// Creates a procedurally generated scenario using the provided generator asset
        /// </summary>
        /// <param name="generator">The generator to use with its configured settings</param>
        /// <returns>A new TestScenario with procedurally generated content</returns>
        public static TestScenario CreateProcedural(ProceduralScenarioGenerator generator)
        {
            if (generator == null)
                return CreateProcedural();
                
            return generator.GenerateScenario();
        }
        
        /// <summary>
        /// Creates a procedurally generated scenario with the specified parameters
        /// </summary>
        /// <param name="regionCount">Number of regions to generate</param>
        /// <param name="nationCount">Number of nations to generate</param>
        /// <param name="seed">Random seed (0 for random)</param>
        /// <returns>A new TestScenario with procedurally generated content</returns>
        public static TestScenario CreateProcedural(int regionCount, int nationCount, int seed = 0)
        {
            ProceduralScenarioGenerator generator = ScriptableObject.CreateInstance<ProceduralScenarioGenerator>();
            generator.regionCount = regionCount;
            generator.nationCount = nationCount;
            generator.seed = seed;
            return generator.GenerateScenario();
        }
        
        /// <summary>
        /// Adds procedurally generated regions and nations to this scenario
        /// </summary>
        /// <param name="regionCount">Number of regions to generate</param>
        /// <param name="nationCount">Number of nations to generate</param>
        /// <param name="clearExisting">Whether to clear existing regions and nations</param>
        public void AddProceduralContent(int regionCount, int nationCount, bool clearExisting = false)
        {
            ProceduralScenarioGenerator generator = ScriptableObject.CreateInstance<ProceduralScenarioGenerator>();
            generator.regionCount = regionCount;
            generator.nationCount = nationCount;
            
            // Apply an offset to the grid position to avoid overlapping with existing regions
            if (!clearExisting && regionStartConditions.Count > 0)
            {
                // Find the maximum grid offset needed to avoid overlapping
                int maxGridWidth = Mathf.CeilToInt(Mathf.Sqrt(regionCount)); // Width of the new grid
                int existingGridWidth = Mathf.CeilToInt(Mathf.Sqrt(regionStartConditions.Count));
                
                // Extract coordinates from existing regions to find maximum extent
                int maxQ = 0;
                int maxR = 0;
                
                foreach (var region in regionStartConditions)
                {
                    // Extract q and r from region IDs if they follow the format "Region_q_r"
                    string[] parts = region.regionId.Split('_');
                    if (parts.Length >= 3 && int.TryParse(parts[1], out int q) && int.TryParse(parts[2], out int r))
                    {
                        maxQ = Mathf.Max(maxQ, q);
                        maxR = Mathf.Max(maxR, r);
                    }
                }
                
                // Set offset to be beyond the maximum extent of existing regions
                // Adding +1 as buffer between existing and new regions
                generator.gridOffsetQ = maxQ + 1;
                generator.gridOffsetR = maxR + 1;
                
                // Keep original width as we're now offsetting by position
                generator.gridWidth = maxGridWidth;
            }
            
            // Generate a temporary scenario
            TestScenario temp = generator.GenerateScenario();
            
            // Clear existing content if requested
            if (clearExisting)
            {
                regionStartConditions.Clear();
                nationStartConditions.Clear();
            }
            
            // Add the regions and update nation references accordingly
            Dictionary<string, string> regionIdMapping = new Dictionary<string, string>();
            
            foreach (var region in temp.regionStartConditions)
            {
                // Keep track of original ID for nation reference updates
                string oldId = region.regionId;
                
                // Add the region directly (no need to modify the ID as it now uses grid coordinates)
                regionStartConditions.Add(region);
                
                // Store the mapping for nation references
                regionIdMapping[oldId] = region.regionId;
            }
            
            // Add and update nations
            int nationOffset = nationStartConditions.Count;
            
            foreach (var nation in temp.nationStartConditions)
            {
                nation.nationId = $"proc_nation_{nationOffset + int.Parse(nation.nationId.Split('_')[1])}";
                
                // Update controlled regions using the mapping
                for (int i = 0; i < nation.controlledRegionIds.Count; i++)
                {
                    string oldRegionId = nation.controlledRegionIds[i];
                    if (regionIdMapping.ContainsKey(oldRegionId))
                    {
                        nation.controlledRegionIds[i] = regionIdMapping[oldRegionId];
                    }
                }
                
                nationStartConditions.Add(nation);
            }
            
            // If we don't have a victory condition, use the generated one
            if (victoryCondition == null)
            {
                victoryCondition = temp.victoryCondition;
            }
            
            // Clean up the temporary generator
            ScriptableObject.Destroy(generator);
        }
    }
}