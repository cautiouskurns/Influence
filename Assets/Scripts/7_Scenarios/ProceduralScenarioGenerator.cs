using UnityEngine;
using System.Collections.Generic;
using Entities;
using System;
using Random = UnityEngine.Random;

namespace Scenarios
{
    [CreateAssetMenu(fileName = "ProceduralScenarioGenerator", menuName = "Influence/Procedural Scenario Generator")]
    public class ProceduralScenarioGenerator : ScriptableObject
    {
        [Header("General Settings")]
        [Tooltip("Number of regions to generate")]
        public int regionCount = 15;
        
        [Tooltip("Number of nations to generate")]
        public int nationCount = 3;

        [Header("Region Generation")]
        [Tooltip("Minimum initial wealth for generated regions")]
        public int minRegionWealth = 80;
        
        [Tooltip("Maximum initial wealth for generated regions")]
        public int maxRegionWealth = 250;
        
        [Tooltip("Minimum initial production for generated regions")]
        public int minRegionProduction = 30;
        
        [Tooltip("Maximum initial production for generated regions")]
        public int maxRegionProduction = 80;
        
        [Tooltip("Minimum initial infrastructure level for generated regions")]
        public float minInfrastructureLevel = 5f;
        
        [Tooltip("Maximum initial infrastructure level for generated regions")]
        public float maxInfrastructureLevel = 15f;
        
        [Tooltip("Minimum initial population for generated regions")]
        public int minPopulation = 500;
        
        [Tooltip("Maximum initial population for generated regions")]
        public int maxPopulation = 2000;
        
        [Tooltip("Minimum initial satisfaction for generated regions")]
        [Range(0f, 1f)]
        public float minSatisfaction = 0.3f;
        
        [Tooltip("Maximum initial satisfaction for generated regions")]
        [Range(0f, 1f)]
        public float maxSatisfaction = 0.8f;

        [Header("Nation Generation")]
        [Tooltip("Nation name generation style")]
        public NationNameStyle nationNameStyle = NationNameStyle.Fantasy;
        
        [Tooltip("Should nations have balanced strength")]
        public bool balanceNations = true;
        
        [Tooltip("Should rich regions cluster together")]
        public bool clusterWealthyRegions = true;
        
        [Tooltip("Randomness seed (0 for random seed)")]
        public int seed = 0;
        
        public enum NationNameStyle
        {
            Fantasy,
            Historical,
            Modern,
            SciFi
        }
        
        [Header("Grid Layout")]
        [Tooltip("Width of the hex grid for procedurally placed regions")]
        public int gridWidth = 5;
        
        [Tooltip("Height of the hex grid for procedurally placed regions")]
        public int gridHeight = 5;
        
        [Tooltip("Should the layout use pointy-top hexes (otherwise flat-top)")]
        public bool pointyTopHexes = false;
        
        // Generate a full procedural scenario with regions and nations
        public TestScenario GenerateScenario()
        {
            // Initialize random seed
            InitializeRandomSeed();
            
            // Create the scenario
            TestScenario scenario = ScriptableObject.CreateInstance<TestScenario>();
            scenario.scenarioName = "Generated Scenario";
            scenario.description = $"Procedurally generated scenario with {regionCount} regions and {nationCount} nations.";
            scenario.turnLimit = 15;
            
            // Generate regions
            scenario.regionStartConditions = GenerateRegions();
            
            // Generate nations and assign regions
            scenario.nationStartConditions = GenerateNations(scenario.regionStartConditions);
            
            // Set victory conditions
            scenario.victoryCondition = GenerateVictoryCondition();
            
            return scenario;
        }
        
        // Initialize random seed
        private void InitializeRandomSeed()
        {
            int seedToUse = seed;
            if (seedToUse == 0)
            {
                seedToUse = UnityEngine.Random.Range(1, 99999);
            }
            UnityEngine.Random.InitState(seedToUse);
            Debug.Log($"Procedural generator using seed: {seedToUse}");
        }
        
        // Generate region definitions
        private List<RegionStartCondition> GenerateRegions()
        {
            List<RegionStartCondition> regions = new List<RegionStartCondition>();
            
            // Calculate grid dimensions based on region count
            CalculateGridDimensions();
            
            // Create a list of grid positions (q,r coordinates)
            List<Vector2Int> gridPositions = GenerateGridPositions();
            
            // Shuffle the positions for random placement
            ShuffleList(gridPositions);
            
            // Limit to the requested region count
            int positionsToUse = Mathf.Min(regionCount, gridPositions.Count);
            
            for (int i = 0; i < positionsToUse; i++)
            {
                // Get grid coordinates
                int q = gridPositions[i].x;
                int r = gridPositions[i].y;
                
                RegionStartCondition region = new RegionStartCondition
                {
                    regionId = $"Region_{q}_{r}", // Use grid coordinates in the ID
                    regionName = GenerateRegionName(i),
                    initialWealth = Random.Range(minRegionWealth, maxRegionWealth),
                    initialProduction = Random.Range(minRegionProduction, maxRegionProduction),
                    initialInfrastructureLevel = Random.Range(minInfrastructureLevel, maxInfrastructureLevel),
                    initialPopulation = Random.Range(minPopulation, maxPopulation),
                    initialSatisfaction = Random.Range(minSatisfaction, maxSatisfaction)
                };
                
                regions.Add(region);
            }
            
            // If clustering enabled, adjust wealth based on "regions" being close to each other
            if (clusterWealthyRegions)
            {
                ClusterRegionWealth(regions);
            }
            
            return regions;
        }
        
        // Calculate appropriate grid dimensions based on region count
        private void CalculateGridDimensions()
        {
            // Calculate grid size to accommodate the number of regions
            // with some extra space for randomness
            int squareRoot = Mathf.CeilToInt(Mathf.Sqrt(regionCount * 1.5f));
            gridWidth = Mathf.Max(3, squareRoot);
            gridHeight = Mathf.Max(3, squareRoot);
            
            // Ensure we don't have too many grid cells
            int maxCells = regionCount * 3; // Allow extra space for varied layouts
            if (gridWidth * gridHeight > maxCells)
            {
                // Reduce grid size proportionally
                float ratio = Mathf.Sqrt(maxCells / (float)(gridWidth * gridHeight));
                gridWidth = Mathf.Max(3, Mathf.FloorToInt(gridWidth * ratio));
                gridHeight = Mathf.Max(3, Mathf.FloorToInt(gridHeight * ratio));
            }
        }
        
        // Generate a list of valid grid positions based on grid size
        private List<Vector2Int> GenerateGridPositions()
        {
            List<Vector2Int> positions = new List<Vector2Int>();
            
            for (int r = 0; r < gridHeight; r++)
            {
                for (int q = 0; q < gridWidth; q++)
                {
                    // Skip some positions to create irregular borders
                    // More likely to skip positions near the edges
                    float distanceFromCenter = Vector2.Distance(
                        new Vector2(q, r),
                        new Vector2(gridWidth / 2f, gridHeight / 2f)
                    );
                    
                    float skipChance = Mathf.Clamp01(distanceFromCenter / Mathf.Max(gridWidth, gridHeight));
                    
                    if (Random.value > skipChance * 0.7f) // 0.7f factor reduces skipping
                    {
                        positions.Add(new Vector2Int(q, r));
                    }
                }
            }
            
            return positions;
        }
        
        // Shuffle a list using Fisher-Yates algorithm
        private void ShuffleList<T>(List<T> list)
        {
            int n = list.Count;
            for (int i = 0; i < n - 1; i++)
            {
                int j = Random.Range(i, n);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
        
        // Generate a name for a region
        private string GenerateRegionName(int index)
        {
            string[] prefixes = new string[] {
                "North", "South", "East", "West", "Central", "Upper", "Lower", 
                "Great", "New", "Old", "Lake", "River", "Mountain", "Forest", "Valley"
            };
            
            string[] suffixes = new string[] {
                "Plains", "Hills", "Valley", "Forest", "Mountains", "Ridge", "Cape",
                "Haven", "Point", "Basin", "Highlands", "Shores", "Fields", "Peaks"
            };
            
            string[] nouns = new string[] {
                "Wood", "Stone", "Iron", "Gold", "Silver", "Crystal", "Amber", "Jade",
                "Oak", "Pine", "Cedar", "Maple", "Birch", "Rose", "Lily", "Hawk", "Eagle",
                "Wolf", "Bear", "Lion", "Tiger", "Stag", "Boar", "Fox", "Raven"
            };
            
            // Different name generation patterns
            switch (Random.Range(0, 4))
            {
                case 0: // Prefix + Suffix (North Plains)
                    return $"{prefixes[Random.Range(0, prefixes.Length)]} {suffixes[Random.Range(0, suffixes.Length)]}";
                
                case 1: // Noun + Suffix (Oakwood Valley)
                    return $"{nouns[Random.Range(0, nouns.Length)]} {suffixes[Random.Range(0, suffixes.Length)]}";
                
                case 2: // Prefix + Noun (Western Wood)
                    return $"{prefixes[Random.Range(0, prefixes.Length)]} {nouns[Random.Range(0, nouns.Length)]}";
                
                case 3: // Compound name (Oakwood)
                    return $"{nouns[Random.Range(0, nouns.Length)]}{suffixes[Random.Range(0, suffixes.Length)]}";
                
                default:
                    return $"Region {index+1}";
            }
        }
        
        // Make neighboring regions have similar wealth values
        private void ClusterRegionWealth(List<RegionStartCondition> regions)
        {
            // Simple version: just smooth out the values somewhat
            for (int i = 1; i < regions.Count - 1; i++)
            {
                // Average with neighbors for a more natural distribution
                int avgWealth = (regions[i-1].initialWealth + regions[i].initialWealth + regions[i+1].initialWealth) / 3;
                regions[i].initialWealth = Mathf.Clamp((avgWealth + regions[i].initialWealth) / 2, minRegionWealth, maxRegionWealth);
            }
        }
        
        // Generate nation definitions and assign regions
        private List<NationStartCondition> GenerateNations(List<RegionStartCondition> regions)
        {
            List<NationStartCondition> nations = new List<NationStartCondition>();
            
            // Create nations
            for (int i = 0; i < nationCount; i++)
            {
                NationStartCondition nation = new NationStartCondition
                {
                    nationId = $"nation_{i+1}",
                    nationName = GenerateNationName(i),
                    controlledRegionIds = new List<string>()
                };
                
                nations.Add(nation);
            }
            
            // Assign regions to nations
            if (balanceNations)
            {
                // Balanced assignment: sort regions by wealth and distribute evenly
                regions.Sort((a, b) => b.initialWealth.CompareTo(a.initialWealth));
                
                for (int i = 0; i < regions.Count; i++)
                {
                    int nationIndex = i % nationCount;
                    nations[nationIndex].controlledRegionIds.Add(regions[i].regionId);
                }
            }
            else
            {
                // Random assignment
                foreach (var region in regions)
                {
                    int nationIndex = Random.Range(0, nationCount);
                    nations[nationIndex].controlledRegionIds.Add(region.regionId);
                }
            }
            
            return nations;
        }
        
        // Generate a name for a nation based on the selected style
        private string GenerateNationName(int index)
        {
            switch (nationNameStyle)
            {
                case NationNameStyle.Fantasy:
                    return GenerateFantasyNationName();
                case NationNameStyle.Historical:
                    return GenerateHistoricalNationName();
                case NationNameStyle.Modern:
                    return GenerateModernNationName();
                case NationNameStyle.SciFi:
                    return GenerateSciFiNationName();
                default:
                    return $"Nation {index+1}";
            }
        }
        
        private string GenerateFantasyNationName()
        {
            string[] prefixes = { "Ar", "El", "Fae", "Gal", "Il", "Lor", "Mith", "Nar", "Sil", "Thal" };
            string[] middles = { "and", "end", "dor", "ran", "lin", "ven", "ril", "ador", "thi", "ion" };
            string[] suffixes = { "ia", "or", "eth", "yr", "ion", "dal", "mar", "iel", "ond", "duin" };
            
            string name = prefixes[Random.Range(0, prefixes.Length)];
            
            // 50% chance to add a middle part
            if (Random.value > 0.5f)
            {
                name += middles[Random.Range(0, middles.Length)];
            }
            
            name += suffixes[Random.Range(0, suffixes.Length)];
            
            return name;
        }
        
        private string GenerateHistoricalNationName()
        {
            string[] types = { "Empire", "Kingdom", "Republic", "Duchy", "Principality", "Confederation", "Dominion" };
            
            string[] adjectives = {
                "Northern", "Southern", "Eastern", "Western", "Ancient", "Great", "Imperial",
                "Royal", "Holy", "United", "Free", "Grand", "Noble", "Golden"
            };
            
            string[] nouns = {
                "Sun", "Moon", "Star", "Lion", "Eagle", "Dragon", "Wolf",
                "Crown", "Sword", "Shield", "Mountain", "Sea", "River", "Lake"
            };
            
            // Different name generation patterns
            switch (Random.Range(0, 3))
            {
                case 0: // Adjective + Type (Northern Empire)
                    return $"{adjectives[Random.Range(0, adjectives.Length)]} {types[Random.Range(0, types.Length)]}";
                
                case 1: // Noun + Type (Eagle Republic)
                    return $"{nouns[Random.Range(0, nouns.Length)]} {types[Random.Range(0, types.Length)]}";
                
                case 2: // The + Adjective + Type (The Imperial Kingdom)
                    return $"The {adjectives[Random.Range(0, adjectives.Length)]} {types[Random.Range(0, types.Length)]}";
                
                default:
                    return "Kingdom";
            }
        }
        
        private string GenerateModernNationName()
        {
            string[] prefixes = {
                "United", "Federal", "Republic of", "Commonwealth of", "Democratic", "People's",
                "Socialist", "Independent", "Sovereign", "Allied"
            };
            
            string[] roots = {
                "Azur", "Bel", "Cas", "Dram", "Est", "Frand", "Gal", "Hel", "Ivo",
                "Kaz", "Lux", "Mont", "Nor", "Pol", "Quad", "Rav", "Sol", "Targ", "Vos"
            };
            
            string[] suffixes = { "ia", "land", "stan", "nia", "mark", "berg", "burg", "ville", "grad", "polis" };
            
            // Different name generation patterns
            switch (Random.Range(0, 3))
            {
                case 0: // Prefix + Root + Suffix (Republic of Belmark)
                    return $"{prefixes[Random.Range(0, prefixes.Length)]} {roots[Random.Range(0, roots.Length)]}{suffixes[Random.Range(0, suffixes.Length)]}";
                
                case 1: // Root + Suffix (Luxburg)
                    return $"{roots[Random.Range(0, roots.Length)]}{suffixes[Random.Range(0, suffixes.Length)]}";
                
                case 2: // Prefix + Root + Suffix (United Montia)
                    return $"{prefixes[Random.Range(0, prefixes.Length)]} {roots[Random.Range(0, roots.Length)]}{suffixes[Random.Range(0, suffixes.Length)]}";
                
                default:
                    return "Nation";
            }
        }
        
        private string GenerateSciFiNationName()
        {
            string[] prefixes = {
                "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Omega", "Neo",
                "Xen", "Nova", "Stellar", "Cosmo", "Astro", "Quantum", "Cyber"
            };
            
            string[] roots = {
                "Tec", "Corp", "Sys", "Net", "Gen", "Dyne", "Flex", "Plex",
                "Tron", "Com", "Mech", "Bot", "Ware", "Forge", "Core", "Sphere"
            };
            
            string[] types = {
                "Alliance", "Collective", "Federation", "Union", "Consortium",
                "Nexus", "Hegemony", "Enclave", "Directive", "Concordat"
            };
            
            // Different name generation patterns
            switch (Random.Range(0, 3))
            {
                case 0: // Prefix + Type (Alpha Federation)
                    return $"{prefixes[Random.Range(0, prefixes.Length)]} {types[Random.Range(0, types.Length)]}";
                
                case 1: // Root + Type (DyneCorp Consortium)
                    return $"{roots[Random.Range(0, roots.Length)]}{(Random.Range(0, 2) == 0 ? "Corp" : "")} {types[Random.Range(0, types.Length)]}";
                
                case 2: // Prefix + Root (Quantum Plex)
                    return $"{prefixes[Random.Range(0, prefixes.Length)]} {roots[Random.Range(0, roots.Length)]}";
                
                default:
                    return "Federation";
            }
        }
        
        // Generate a victory condition for the scenario
        private VictoryCondition GenerateVictoryCondition()
        {
            VictoryCondition condition = new VictoryCondition();
            
            // Randomly select victory type
            condition.type = (VictoryCondition.VictoryType)Random.Range(0, 3);
            
            // Configure victory condition based on type
            switch (condition.type)
            {
                case VictoryCondition.VictoryType.Economic:
                    condition.requiredWealth = Random.Range(maxRegionWealth * 2, maxRegionWealth * 3);
                    break;
                
                case VictoryCondition.VictoryType.Development:
                    condition.requiredInfrastructure = Random.Range(maxInfrastructureLevel * 1.5f, maxInfrastructureLevel * 2);
                    break;
                
                case VictoryCondition.VictoryType.Stability:
                    condition.requiredSatisfaction = Mathf.Clamp01(Random.Range(0.8f, 0.95f));
                    break;
            }
            
            condition.requiredConsecutiveTurns = Random.Range(1, 4);
            
            // 20% chance to target a specific region instead of all
            if (Random.value < 0.2f)
            {
                condition.targetRegionId = $"region_{Random.Range(1, regionCount+1)}";
            }
            
            return condition;
        }
    }
}