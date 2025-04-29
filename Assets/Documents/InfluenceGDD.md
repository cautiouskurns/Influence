# 1. Game Design Document

## 1. Game Overview

- **Title**: Economic Cycles
- **Genre**: Roguelike Grand Strategy + Economic Simulator
- **Pitch**: A satirical economic strategy game where players guide a nation through booms, busts, and generational change in a world of absurd policies, trade rifts, and bureaucratic comedy.

## 2. Core Gameplay Loop

- **Turn Structure**: Strategic Assessment → Development Decisions → External Relations → End Turn Processing
- **Run Structure**: 30–40 year runs; generational shifts simulate longer-term change, affecting resources, infrastructure, and narrative state.
- **Meta-progression**: Unlocks new nations, policies, and conditions over multiple runs.

## 3. Inspirations & Influences

### Gameplay Influences
| Game                  | Influence                                               |
|-----------------------|---------------------------------------------------------|
| Crusader Kings        | Emergent storytelling, generational structure          |
| Europa Universalis IV | Regional simulation, production chains, trade nodes    |
| Slay the Spire        | Meta-progression and run-based structure               |
| Frostpunk             | Systems + story interplay, difficult trade-offs        |
| Dwarf Fortress        | Simulation depth, emergent chaos                       |
| FTL: Faster Than Light | Procedural run structure, branching node-based progression |

### Narrative & Thematic Influences
- Terry Pratchett (absurdism, wit, worldbuilding)
- Black Mirror (societal satire, system logic)
- Bureaucratic comedy (e.g., The Thick of It, Yes Minister)
- Historical satire and soft anachronisms

## 4. Core Features & Mechanics

### Summary Table
| Feature               | Description                                                                 |
|-----------------------|-----------------------------------------------------------------------------|
| Economic Cycles       | Boom → Peak → Recession → Recovery affecting gameplay conditions           |
| Generational Shifts   | Change in demographics, technology, and production after each run          |
| Infrastructure System | Region-based levels that boost output and enable projects                  |
| Policy Decisions      | Game-wide or regional decisions with trade-offs and cooldowns              |
| Event System          | Narrative and systemic events triggered by economic/political thresholds   |
| Production Chains     | Basic → Advanced resource refinement affecting outputs                     |
| Population Satisfaction | Driven by fulfillment of needs; affects productivity and unrest          |
| Meta-Progression      | Unlocks over multiple runs (nations, traits, techs)                        |

## 5. Major Game Systems

### 5.1 Economic System
- Sectors: Agriculture, Industry, Commerce, etc.
- Inputs/outputs, infrastructure multipliers, labor dependency
- Implements Cobb-Douglas production (Output = A × L^α × K^β)
- Economic cycles apply phase-based multipliers to wealth and production
- Price volatility introduces strategic unpredictability in supply-demand balancing

### 5.2 Resource System
- Primary (timber, food) and secondary (textiles, consumer goods)
- Regional surplus/shortage mechanics and interdependence
- Dynamic pricing based on supply/demand imbalance and elasticity
- Substitution logic via cross-price elasticity

### 5.3 Population System
- Labor availability, satisfaction, unrest
- Class dynamics and demographic shifts
- Wealth-based consumption modeling
- Unmet needs lower satisfaction and trigger unrest events

### 5.4 Infrastructure & Projects
- Level-based bonuses
- Enables regional specialization and unlocks
- Infrastructure boosts production efficiency
- Maintenance cost reduces wealth each turn

### 5.5 Event & Narrative System
- Triggered by cycles, policies, population status
- Ties back to tone and world state
- Supports conditional dialogue with player stats (e.g., Charisma, Intelligence)
- Implements modular narrative logic (intro → choice → consequence)
- Tracks event flags like “MediaScandalActive” for persistent outcomes

### 5.6 Nation System
- **National Structure**: Groups multiple regions into single political/economic units
- **Nation Properties**: Unique ID, name, color, diplomatic status with other nations
- **Nation Management**: Nations can add/remove regions and track their member regions
- **Diplomatic Relations**: Basic diplomatic states (Allied, Neutral, Hostile) between nations
- **Economic Aggregation**: Nation-level economic data (total wealth, production, average infrastructure)
- **National Comparison**: Tools to identify strongest nations by economic metrics
- **Region Coloring**: Region colors based on national affiliation for map visualization

### 5.7 Diplomacy & Trade (Planned)
- Future expansion with routes and inter-nation strategy

## 6. Meta Systems & Replayability

- Generational transitions simulate legacy and systemic decay/progress
- Unlockable nations, ideologies, technologies
- Randomized events and world states ensure variation
- Thought Cabinet system:
  - Ideologies are discovered or chosen through events or conditions
  - Thoughts require incubation (delayed activation) and provide permanent buffs/debuffs
  - Ministers and factions react to ideological shifts
  - Conflicting ideologies block each other, shaping national doctrine

## 7. Narrative, Tone & Worldbuilding

- Fictional historical era: 16th–17th century inspiration
- Nations loosely inspired by real cultures
- Humor via satire, cosmic weirdness, and bureaucratic absurdity
- Anachronisms: TikTok guilds, Ministry of Optimistic Projections
- Ministers maintain relationships and memory of player decisions
- Dynamic events evolve from simulation context and active ideologies
- Anachronistic and surreal events integrated into narrative layers

## 8. Art & Visual Style

- Satirical and stylized historical map presentation (planned)
- Dashboards and region overlays over pure visual fidelity
- Event cards with illustrations and flavor text

## 9. UI/UX Design

- Region dashboards and nation-wide summaries
- Minimal micromanagement, high-level directives
- Emphasis on readable data and player comprehension

## 10. Technical Design

- **Architecture**: ECS + MVC hybrid
- **ScriptableObjects** for static data: nation, project, terrain, events
- **EventBus** system for decoupling
- Modular, debug-driven prototyping
- Unity + editor tools for turn simulation
- Dialogue engine supports stat-based skill checks and dice rolls
- Event system supports delayed callbacks and chained consequences
- National stats stored in runtime component and updated from game state
- Debug-first tooling used to prototype and test economic logic and narrative flow

## 11. Milestones & Implementation Strategy

- Core loop in place via editor/debug windows
- V1: full economic simulation of a single region
- V2: add multiple regions, UI overlays, and full project system
- V3+: events, generation shifts, meta-progression, diplomacy

## 12. Design Pillars & Constraints

- Systemic over scripted
- Strategic, not micromanaged
- Replayability via variation and legacy
- Humor as a spice, not the dish

## 13. Appendices

- Glossary
- Debug tools and simulation functions
- System flow diagrams (pending)
- Pseudocode for turn logic and economic processing

## Appendix: Design Notes and Future Concepts

This appendix contains design ideas that are not yet part of the core GDD, but may be integrated or expanded upon in future iterations.

### 🧮 Economic Model Extensions

- **Supply & Demand Mechanics**
  - Dynamic price = BasePrice × (1 + (Demand - Supply) / Elasticity)
  - Elasticity varies per resource
  - Demand influenced by population, wealth, infrastructure
  - Supply driven by infrastructure, terrain, and production efficiency

- **Elasticity & Substitution**
  - Income elasticity: richer populations consume more
  - Cross-price elasticity: substitution logic (e.g., rice vs bread)

- **Cobb-Douglas Production Function**
  - Output = A × L^α × K^β
  - A = productivity factor; L = labor; K = capital; α/β = elasticity coefficients

- **Infrastructure Impact**
  - Efficiency boost = 1 + level × modifier
  - Potential decay and maintenance costs

- **Consumption Modeling**
  - Wealth-based consumption formula
  - Unmet demand leads to unrest and stagnation

- **Economic Cycle Integration**
  - CycleEffect = PhaseModifier × Coefficient
  - Influences production, unrest, prices

- **Price Volatility**
  - Price(t+1) = Price(t) + α × (SupplyShock - ConsumptionTrend)
  - Random market shocks and feedback loops

### 🎭 RPG-Style Event Mechanics

- **Player Stats & Traits**
  - Charisma, Intelligence, Political Capital, Credibility, etc.
  - Tracked in a PlayerStats component

- **Skill Checks & Dice Rolls**
  - Dialogue outcomes gated behind skill thresholds or random rolls
  - Simple d20 implementation for choice branches

- **Event Memory System**
  - Choices leave flags like “MediaScandalActive = true”
  - Affects future event branches and tone

- **Minister & Faction Relationships**
  - Track trust/distrust levels with key characters
  - Choices influence future support or conflict

- **Conditional Dialogue Templates**
  - Modular text built from:
    - Intro → Conflict → Choice → Consequence
  - Context-aware fragments use game state variables

### 🧠 National Thought Cabinet System (Inspired by Disco Elysium)

- **Core Concept**
  - Ideologies or doctrines are researched like "thoughts"
  - Incubation period → permanent nation-level modifier

- **Examples**
  - “Agrarian Exceptionalism”: Boosts food output, slows industry
  - “Invisible Hand Manifesto”: Improves market efficiency, raises inequality

- **Incubation Effects**
  - Temporary instability, bureaucracy conflict
  - Ministers react, events adapt

- **Systemic Hooks**
  - Modify equations (production, pricing)
  - Alter faction satisfaction and unlock content
  - Conflicting doctrines block each other

### 🌐 Roguelike Event Tree

- **Branching Nodes**
  - Events, crises, ideologies form a branching meta-structure
  - Players navigate ideological progression like a policy map

- **Reactive Progression**
  - Nodes appear based on state triggers or economic phases
  - Player decisions or world conditions unlock future branches

- **Dynamic Outcomes**
  - Narrative arcs change based on prior decisions
  - Meta-paths shape tone and direction of the run

### 🧰 Real-Time & Tick-Based Hybrid Time Model

- Time flows in months/weeks; events trigger asynchronously
- Nodes or crises trigger automatically or are queued for player decision
- Events create a living, reactive policy layer over a stable economic sim

### 🎨 Visual & Thematic Concepts

- **Visual Style**
  - Bureaucratic retro aesthetic (folders, stamps, maps, red tape)
  - Stylized dashboards and absurd paperwork UI

- **Narrative Flavor**
  - Ministers with ludicrous titles
  - Events range from plausible crises to surreal absurdities
  - Tooltip humor and achievement flavor text

- **Audio Direction**
  - Ambient orchestral or lo-fi industrial
  - Thematic cues for satire, crisis, or prosperity

### 📌 Future Development Concepts

- Policy tech tree vs. event chaos layer
- Emergent ideology blending (e.g., technocratic populism)
- Replay map showing past generations' ideologies and events
- Experimental debug-first scripting for narrative flow control
- Procedurally generated historical timeline
- Constrained number of actions/interactions/influence in a given area/time 
- Monthly newspaper published chronicaling your game

# 2. Project Plan
 
## Phase 1: Core Simulation & Dialogue Foundations
 
This phase focuses on implementing the foundational systems required for economic simulation and narrative delivery. The goal is to produce a fully functional vertical slice that allows turn progression, economic changes in regions, and event-driven dialogue interactions.
 
### Objectives
 
- ✅ Implement turn-based structure with EventBus communication
- ✅ Create GameManager and TurnManager for simulation control
- ✅ Initialize MapModel with one or more test regions
- ⏳ Finalize RegionEntity architecture and all core components
- ⏳ Develop production and consumption flows using ResourceComponent and ProductionComponent
- ⏳ Introduce economic cycles with basic phase effects via EconomicCycleSystem
- ⏳ Visualize economic state through debug logs or minimal UI
 
### Narrative & Event System
 
- ✅ EventDialogueManager with basic dialogue display
- ⏳ Implement dialogue event loading and triggering from events
- ⏳ Link narrative flags to game state (e.g., satisfaction, resources)
- ⏳ Create sample dialogue events with multiple branches
- ⏳ Integrate DialogueOutcome with GameStateManager for persistent effects
 
### Vertical Slice Goals
 
- End-turn progression updates economy
- Population satisfaction reflects economic outcomes
- Triggered event dialogue based on region conditions
- Debug tools to simulate, test, and validate logic paths





# 3. Game Architecture

## Architecture Components

### 1. Data Layer (ScriptableObjects)
While the overall architecture mentions ScriptableObjects, none are directly visible in the provided git files. References to them exist in code comments.

### 2. Entity Component System (Game Logic)

#### Entities (Domain Objects)

| Entity | Purpose | Implementation Status |
|--------|---------|------------------------|
| **RegionEntity** | Represents an economic region with components | Implemented in `3_Entities/RegionEntity.cs` |
| **NationEntity** | Not visible in current files | Not found in provided files |
| **ProjectEntity** | Not visible in current files | Not found in provided files |

#### Components (Data Containers)

| Component | Purpose | Implementation Status |
|-----------|---------|------------------------|
| **ResourceComponent** | Manages basic resources (Food, Wood) with generation and access methods | Implemented in `4_Components/RegionComponents/ResourceComponent.cs` |
| **ProductionComponent** | Processes production from resources with simplified output calculation | Implemented in `4_Components/RegionComponents/ProductionComponent.cs` |
| **RegionEconomyComponent** | Tracks wealth and production values for a region | Implemented in `4_Components/RegionComponents/RegionEconomyComponent.cs` |
| **PopulationComponent** | Manages labor availability and satisfaction | Implemented in `4_Components/RegionComponents/PopulationComponent.cs` |
| **InfrastructureComponent** | Basic implementation with level tracking and maintenance costs | Implemented in `4_Components/RegionComponents/InfrastructureComponent.cs` |

#### Systems (Logic Processors)

| System | Purpose | Implementation Status |
|--------|---------|------------------------|
| **EconomicSystem** | Processes economic simulation including supply/demand, production, infrastructure, cycles | Implemented in `5_Systems/EconomicSystem.cs` |
| **Other systems** | Not visible in current files | Not found in provided files |

### 3. Core Management

| Component | Purpose | Implementation Status |
|-----------|---------|------------------------|
| **GameManager** | Singleton that manages game state and test region | Implemented in `1_Core/GameManager.cs` |
| **TurnManager** | Handles turn progression, time controls, and turn events | Implemented in `1_Core/TurnManager.cs` |
| **EventBus** | Central event communication system with subscription management | Implemented in `8_Managers/EventBus.cs` |

### 4. Event System (Communication Layer)

| Key Event | Triggered By | Subscribed By | Implementation Status |
|-----------|--------------|---------------|------------------------|
| **TurnEnded** | TurnManager | GameManager, EconomicSystem | Implemented |
| **RegionUpdated** | RegionEntity | EconomicSystem | Implemented |
| **RegionsUpdated** | GameManager | Not visible in files | Implemented |
| **GameReset** | GameManager | Not visible in files | Implemented |

## Folder Structure Implementation

The current implementation follows a numbered folder structure:

```
Assets/Scripts/V2/
  ├── 1_Core/
  │    ├── GameManager.cs
  │    └── TurnManager.cs
  ├── 2_Data/
  │    └── [Empty or not visible in provided files]
  ├── 3_Entities/
  │    └── RegionEntity.cs
  ├── 4_Components/
  │    └── RegionComponents/
  │         ├── ResourceComponent.cs
  │         ├── ProductionComponent.cs
  │         ├── RegionEconomyComponent.cs
  │         ├── PopulationComponent.cs
  │         └── InfrastructureComponent.cs
  ├── 5_Systems/
  │    └── EconomicSystem.cs
  ├── 6_UI/
  │    └── [Empty or not visible in provided files]
  ├── 7_Controllers/
  │    └── [Empty or not visible in provided files]
  ├── 8_Managers/
  │    └── EventBus.cs
  └── 9_Utils/
       └── [Empty or not visible in provided files]
```

## Implementation Details

### RegionEntity Implementation
```csharp
public class RegionEntity
{
    // Identity
    public string Name { get; set; }
    
    // Components
    public ResourceComponent Resources { get; private set; }
    public ProductionComponent Production { get; private set; }
    public RegionEconomyComponent Economy { get; private set; }
    public InfrastructureComponent Infrastructure { get; private set; }
    public PopulationComponent Population { get; private set; }

    // Processing methods
    public void ProcessTurn() { ... }
    public string GetSummary() { ... }
}
```

### EventBus Implementation
```csharp
public static class EventBus
{
    private static Dictionary<string, Action<object>> eventDictionary = new();
    
    // Core methods
    public static void Subscribe(string eventName, Action<object> listener) { ... }
    public static void Unsubscribe(string eventName, Action<object> listener) { ... }
    public static void Trigger(string eventName, object eventData = null) { ... }
}
```

### EconomicSystem Implementation
```csharp
public class EconomicSystem : MonoBehaviour
{
    // Singleton pattern
    public static EconomicSystem Instance { get; private set; }
    
    // References
    public RegionEntity testRegion;
    
    // Economic processing methods
    public void ProcessEconomicTick() { ... }
    private void ProcessSupplyAndDemand(RegionEntity region) { ... }
    private void ProcessProduction(RegionEntity region) { ... }
    private void ProcessInfrastructure(RegionEntity region) { ... }
    private void ProcessPopulationConsumption(RegionEntity region) { ... }
    private void ProcessEconomicCycle(RegionEntity region) { ... }
    private void ProcessPriceVolatility() { ... }
}
```

## Current Architecture Analysis

The current implementation shows:

1. **Strong Component Architecture**: RegionEntity uses composition with specialized components for different aspects of gameplay.

2. **Event-Driven Communication**: EventBus implementation with subscription/unsubscription and event triggering.

3. **Singleton Pattern**: Used for GameManager and EconomicSystem for global access.

4. **Turn-Based Simulation**: TurnManager handles progression with pause/resume functionality.

5. **Early-Stage Economic Model**: 
   - Basic resource generation
   - Simple production conversion
   - Economic calculations using Cobb-Douglas model
   - Population satisfaction tracking
   - Infrastructure levels with maintenance costs

6. **Testing Framework**:
   - Single test region managed by GameManager
   - Console logging for debugging
   - Manual tick functionality in EconomicSystem

## Gap Analysis

Comparing the current implementation to the target architecture:

### Present Components
- Core event system
- Basic region entity with components
- Economic simulation foundation
- Turn-based progression
- Component-based architecture

### Missing Components
- ScriptableObjects data layer
- Nation entities
- Project entities
- MapSystem
- TradeSystem
- UI components
- Controllers
- State management system
- Dialogue system
- Economic cycle specialized components

## Next Implementation Steps

Based on the current state:

1. **Complete Resource System**
   - Expand beyond basic Food/Wood resources
   - Implement production chains
   - Add resource constraints

2. **Add Economic Cycle Component**
   - Implement cycle phases
   - Connect to EconomicSystem
   - Add phase-specific modifiers

3. **Develop Multi-Region Support**
   - Move from single test region to multi-region
   - Implement region relationships
   - Add nation grouping

4. **Implement UI Layer**
   - Create visualization for regions
   - Build economic dashboards
   - Develop interaction controls

5. **Add Data-Driven Configuration**
   - Implement ScriptableObjects
   - Move hardcoded values to data

The current implementation provides a solid foundation for the entity-component architecture and economic simulation core, requiring expansion to meet the full architectural vision.



# 4. Game Architecture Diagram
## Game Architecture

```mermaid
graph LR
    %% Core Managers
    subgraph "Core Managers"
        GM[GameManager]
        TM[TurnManager]
        EM[EventBus]
    end

    %% Data Layer
    subgraph "Data Layer"
        SO[ScriptableObjects]
    end

    %% Entities
    subgraph "Entities"
        RE[RegionEntity]
    end

    %% Components
    subgraph "Components"
        RC[ResourceComponent]
        PC[ProductionComponent]
        REC[RegionEconomyComponent]
        POPC[PopulationComponent]
        IC[InfrastructureComponent]
    end

    %% Systems
    subgraph "Systems"
        ECS[EconomicSystem]
        subgraph "Economic Subsystems"
            PROD[ProductionCalculator]
            CONS[ConsumptionManager]
            CYCM[CycleManager]
            INFM[InfrastructureManager]
            MARK[MarketBalancer]
            PRIC[PriceVolatilityManager]
            WEAL[WealthManager]
        end
    end

    %% UI
    subgraph "User Interface"
        subgraph "Debug Tools"
            ESCO[EconomicDebugWindow]
            ECHG[EconomicDebugGraph]
            EHIS[EconomicDataHistory]
            EPRM[EconomicParameters]
            ERCO[EconomicRegionController]
        end
    end

    %% Major Relationships with Text
    GM -->|"Creates & manages"| RE
    GM -->|"Subscribes to events"| EM
    ECS -->|"References & processes"| RE
    ECS -->|"Creates & owns"| PROD
    ECS -->|"Creates & owns"| CONS
    ECS -->|"Creates & owns"| CYCM
    ECS -->|"Creates & owns"| INFM
    ECS -->|"Creates & owns"| MARK
    ECS -->|"Creates & owns"| PRIC
    ECS -->|"Creates & owns"| WEAL
    
    TM -.->|"Broadcasts turn events"| EM
    GM -.->|"Broadcasts region events"| EM
    ECS -.->|"Subscribes to turn events"| EM
    
    RE -->|"Owns & initializes"| RC
    RE -->|"Owns & initializes"| PC
    RE -->|"Owns & initializes"| REC
    RE -->|"Owns & initializes"| POPC
    RE -->|"Owns & initializes"| IC
    
    PC -->|"Uses resources from"| RC
    
    ESCO -->|"Monitors & controls"| ECS
    ESCO -->|"Manages parameters via"| EPRM
    ESCO -->|"Visualizes data using"| ECHG
    ESCO -->|"Records history in"| EHIS
    ESCO -->|"Controls region via"| ERCO
    
    %% Legend
    classDef core fill:#ff9900,color:#000,stroke:#333,stroke-width:2px
    classDef data fill:#3498db,color:#000,stroke:#333,stroke-width:2px
    classDef entities fill:#2ecc71,color:#000,stroke:#333,stroke-width:2px
    classDef components fill:#1abc9c,color:#000,stroke:#333,stroke-width:2px
    classDef systems fill:#9b59b6,color:#000,stroke:#333,stroke-width:2px
    classDef ui fill:#e74c3c,color:#000,stroke:#333,stroke-width:2px
    classDef utils fill:#95a5a6,color:#000,stroke:#333,stroke-width:2px
    
    class GM,TM,EM core
    class SO data
    class RE entities
    class RC,PC,REC,POPC,IC components
    class ECS,PROD,CONS,CYCM,INFM,MARK,PRIC,WEAL systems
    class ESCO,ECHG,EHIS,EPRM,ERCO ui
```

## Game Event Sequence

```mermaid
sequenceDiagram
    participant User
    participant TM as TurnManager
    participant EM as EventBus
    participant GM as GameManager
    participant RE as RegionEntity
    participant RC as ResourceComponent
    participant PC as ProductionComponent
    participant REC as RegionEconomyComponent
    participant POPC as PopulationComponent
    participant IC as InfrastructureComponent
    participant ECS as EconomicSystem
    participant PROD as ProductionCalculator
    participant CONS as ConsumptionManager
    participant CYCM as CycleManager
    participant INFM as InfrastructureManager
    participant MARK as MarketBalancer
    participant PRIC as PriceVolatilityManager
    participant WEAL as WealthManager
    
    Note over User,TM: Player initiates turn end
    User->>TM: Click "End Turn"
    TM->>EM: Trigger "TurnEnded"
    Note over TM,EM: Global turn event broadcast
    
    Note over EM,GM: GameManager handles basic region processing
    EM->>GM: Notify of "TurnEnded"
    GM->>RE: ProcessTurn()
    Note over RE,RC: Resource generation phase
    RE->>RC: GenerateResources()
    RC-->>RE: Update resources
    Note over RE,PC: Production processing phase
    RE->>PC: ProcessProduction()
    PC-->>RE: Set output
    Note over RE,REC: Economy update phase
    RE->>REC: UpdateEconomy()
    REC-->>RE: Update wealth and production
    RE->>EM: Trigger "RegionUpdated"
    Note over RE,EM: Region broadcasts state change
    
    Note over EM,ECS: EconomicSystem performs detailed simulation
    EM->>ECS: Notify of "TurnEnded"
    ECS->>ECS: ProcessEconomicTick()
    Note over ECS: Economic simulation begins
    
    Note over ECS,MARK: Market dynamics calculation
    ECS->>MARK: Process(testRegion)
    MARK-->>ECS: Supply/Demand calculation
    
    Note over ECS,PROD: Production calculation using Cobb-Douglas
    ECS->>PROD: Process(testRegion)
    PROD->>PROD: Calculate Cobb-Douglas
    PROD-->>ECS: Production result
    
    Note over ECS,INFM: Infrastructure maintenance processing
    ECS->>INFM: Process(testRegion)
    INFM->>IC: GetMaintenanceCost()
    INFM->>REC: Deduct maintenance
    
    Note over ECS,CONS: Population consumption and satisfaction
    ECS->>CONS: Process(testRegion)
    CONS->>POPC: UpdateSatisfaction()
    
    Note over ECS,WEAL: Wealth accumulation and management
    ECS->>WEAL: Process(testRegion)
    WEAL->>REC: Update wealth
    
    Note over ECS,CYCM: Economic cycle effects application
    ECS->>CYCM: Process(testRegion)
    CYCM->>REC: Apply cycle effects
    
    Note over ECS,PRIC: Price volatility simulation
    ECS->>PRIC: Process(testRegion)
    PRIC-->>ECS: Price volatility
    
    Note over ECS,EM: Simulation complete, notify listeners
    ECS->>EM: Trigger "RegionUpdated"
    
    Note over EM,TM: Turn cycle completion
    EM->>TM: Continue turn processing
    TM-->>User: Update visual state
    Note over TM,User: Player sees updated game state
```

# 4. System Architecture Diagrams
## Economy Architecture
```mermaid
graph LR
    %% Main System
    EconomicSystem["EconomicSystem\n(MonoBehaviour)"]
    
    %% Core Components
    RegionEntity["RegionEntity"]
    
    %% Subsystems
    ProductionCalc["ProductionCalculator"]
    InfraManager["InfrastructureManager"]
    ConsumptionMgr["ConsumptionManager"]
    MarketBalancer["MarketBalancer"]
    CycleMgr["CycleManager"]
    PriceVolatility["PriceVolatilityManager"]
    WealthMgr["WealthManager"]
    
    %% Region Components
    EconomyComp["RegionEconomyComponent"]
    ProdComp["ProductionComponent"]
    ResourceComp["ResourceComponent"]
    InfraComp["InfrastructureComponent"]
    PopulationComp["PopulationComponent"]
    
    %% Debug & Editor
    EconomicDebugWindow["EconomicDebugWindow\n(EditorWindow)"]
    EconomicDataHistory["EconomicDataHistory"]
    EconomicParameters["EconomicParameters"]
    EconomicRegionController["EconomicRegionController"]
    
    %% Event System
    EventBus[("EventBus")]
    
    %% Action Node
    ManualTick["ManualTick()"]
    
    %% Connections - Main System
    EconomicSystem --"owns"--> ProductionCalc
    EconomicSystem --"owns"--> InfraManager
    EconomicSystem --"owns"--> ConsumptionMgr
    EconomicSystem --"owns"--> MarketBalancer
    EconomicSystem --"owns"--> CycleMgr
    EconomicSystem --"owns"--> PriceVolatility
    EconomicSystem --"owns"--> WealthMgr
    EconomicSystem --"manages"--> RegionEntity
    
    %% Region Entity owns Components
    RegionEntity --"owns"--> EconomyComp
    RegionEntity --"owns"--> ProdComp
    RegionEntity --"owns"--> ResourceComp
    RegionEntity --"owns"--> InfraComp
    RegionEntity --"owns"--> PopulationComp
    
    %% Component interactions
    ProdComp --"reads"--> ResourceComp
    ProdComp --"provides output to"--> EconomyComp
    
    %% Subsystem interactions
    ProductionCalc --"processes"--> RegionEntity
    ProductionCalc --"updates"--> ProdComp
    ProductionCalc --"updates"--> EconomyComp
    
    InfraManager --"processes"--> RegionEntity
    InfraManager --"updates"--> InfraComp
    InfraManager --"deducts maintenance from"--> EconomyComp
    
    ConsumptionMgr --"processes"--> RegionEntity
    ConsumptionMgr --"updates"--> PopulationComp
    
    WealthMgr --"processes"--> RegionEntity
    WealthMgr --"updates"--> EconomyComp
    
    CycleMgr --"processes"--> RegionEntity
    CycleMgr --"modifies"--> EconomyComp
    
    PriceVolatility --"processes"--> RegionEntity
    
    %% Debug window connections
    EconomicDebugWindow --"displays"--> EconomicSystem
    EconomicDebugWindow --"manages"--> EconomicParameters
    EconomicDebugWindow --"records data in"--> EconomicDataHistory
    EconomicDebugWindow --"controls region with"--> EconomicRegionController
    
    EconomicParameters --"syncs with"--> EconomicSystem
    EconomicRegionController --"modifies"--> RegionEntity
    
    %% Event flow
    RegionEntity --"triggers events via"--> EventBus
    EconomicSystem --"subscribes to"--> EventBus

    %% Processing flow
    EconomicSystem --"calls"--> ManualTick
    ManualTick --"triggers"--> ProductionCalc
    ManualTick --"triggers"--> InfraManager
    ManualTick --"triggers"--> ConsumptionMgr
    ManualTick --"triggers"--> MarketBalancer
    ManualTick --"triggers"--> WealthMgr
    ManualTick --"triggers"--> CycleMgr
    ManualTick --"triggers"--> PriceVolatility
    
    %% Class type notes
    classDef component fill:#c2e0ff,stroke:#6495ed,stroke-width:2px,color:#000000,font-weight:bold
    classDef system fill:#ffe6cc,stroke:#d79b00,stroke-width:2px,color:#000000,font-weight:bold
    classDef entity fill:#d5e8d4,stroke:#82b366,stroke-width:2px,color:#000000,font-weight:bold
    classDef editor fill:#e1d5e7,stroke:#9673a6,stroke-width:2px,color:#000000,font-weight:bold
    classDef event fill:#f5f5f5,stroke:#666666,stroke-width:2px,color:#000000,font-weight:bold
    classDef action fill:#fff2cc,stroke:#d6b656,stroke-width:2px,color:#000000,font-weight:bold

    class EconomyComp,ProdComp,ResourceComp,InfraComp,PopulationComp component
    class EconomicSystem,ProductionCalc,InfraManager,ConsumptionMgr,MarketBalancer,CycleMgr,PriceVolatility,WealthMgr system
    class RegionEntity entity
    class EconomicDebugWindow,EconomicDataHistory,EconomicParameters,EconomicRegionController editor
    class EventBus event
    class ManualTick action

```

## Economy Flow
```mermaid
flowchart TD
    %% Main Economic Flow Concepts
    
    %% Resources & Inputs
    Labor["Labor\n(PopulationComponent)"] 
    Capital["Capital/Infrastructure\n(InfrastructureComponent)"]
    Resources["Resources\n(ResourceComponent)"]
    
    %% Core Economic Processes
    Production["Production Process\n(Cobb-Douglas Function)"]
    MarketDynamics["Market Dynamics\n(Supply & Demand)"]
    Consumption["Consumption\n(Population Needs)"]
    EconomicCycle["Economic Cycle\n(Growth & Contraction)"]
    
    %% Outputs & Results
    Wealth["Wealth Accumulation"]
    ProductionOutput["Production Output"]
    Satisfaction["Population Satisfaction"]
    MarketPrices["Market Prices & Volatility"]
    
    %% Main Flow Connections
    Labor -->|"Labor Input (L^α)"|Production
    Capital -->|"Capital Input (K^β)"|Production
    Resources -->|"Resource Consumption"|Production
    
    Production -->|"Generates"|ProductionOutput
    ProductionOutput -->|"Supply"|MarketDynamics
    ProductionOutput -->|"Contributes to"|Wealth
    
    Labor -->|"Creates Demand"|MarketDynamics
    Labor -->|"Population Size"|Consumption
    
    MarketDynamics -->|"Price Signals"|MarketPrices
    MarketDynamics -->|"Imbalance"|EconomicCycle
    
    ProductionOutput -->|"Available Goods"|Consumption
    Consumption -->|"Satisfaction Level"|Satisfaction
    
    EconomicCycle -->|"Multiplier Effect"|Production
    EconomicCycle -->|"Growth Rate"|Wealth
    EconomicCycle -->|"Volatility"|MarketPrices
    
    %% Maintenance & Decay
    Capital -->|"Decay Rate"|Capital
    Wealth -->|"Maintenance Costs"|Capital
    
    %% Parameter Influences
    Params["Economic Parameters"]
    Params -->|"Productivity Factor"|Production
    Params -->|"Labor Elasticity"|Production
    Params -->|"Capital Elasticity"|Production
    Params -->|"Cycle Multiplier"|EconomicCycle
    Params -->|"Wealth Growth Rate"|Wealth
    Params -->|"Price Volatility"|MarketPrices
    Params -->|"Decay Rate"|Capital
    Params -->|"Maintenance Cost"|Capital
    Params -->|"Consumption Rate"|Consumption
    
    %% Visual Styling with lighter backgrounds
    classDef input fill:#e8f5e9,stroke:#81c784,stroke-width:2px,color:#2e7d32,font-weight:bold
    classDef process fill:#fff8e1,stroke:#ffca28,stroke-width:2px,color:#f57f17,font-weight:bold
    classDef output fill:#e3f2fd,stroke:#64b5f6,stroke-width:2px,color:#1565c0,font-weight:bold
    classDef parameter fill:#f3e5f5,stroke:#ba68c8,stroke-width:2px,color:#7b1fa2,font-weight:bold
    
    class Labor,Capital,Resources input
    class Production,MarketDynamics,Consumption,EconomicCycle process
    class Wealth,ProductionOutput,Satisfaction,MarketPrices output
    class Params parameter
```


```mermaid
classDiagram
    %% Main Dialogue System Classes
    class DialogueEventManager {
        -List~DialogueEvent~ availableEvents
        -Queue~DialogueEvent~ pendingEvents
        -DialogueEvent currentEvent
        -EconomicSystem economicSystem
        +DialogueEvent CurrentEvent
        +void CheckForEvents()
        +void QueueEvent(DialogueEvent)
        +void MakeChoice(int choiceIndex)
        +void TriggerEvent(string eventId)
        +void ResetEvent(string eventId)
        +void ResetAllEvents()
        +List~DialogueEvent~ GetAllEvents()
        -void DisplayNextEvent()
        -void OnEventCompleted(object data)
        -void GenerateFallbackEvent(string eventId)
    }
    
    class DialogueEvent {
        +string id
        +string title
        +string description
        +EventCategory category
        +bool hasTriggered
        +List~EventCondition~ conditions
        +List~EventChoice~ choices
        +bool CheckConditions(EconomicSystem, RegionEntity)
        +void ApplyChoice(int choiceIndex, EconomicSystem, RegionEntity)
        +string GetNextEventId(int choiceIndex)
        +DialogueEvent Clone()
        -void RecalculateProduction(EconomicSystem, RegionEntity)
    }
    
    class EventChoice {
        +string text
        +string response
        +List~string~ narrativeEffects
        +List~ParameterEffect~ economicEffects
        +string nextEventId
        +EventChoice(EventChoice) // Copy constructor
    }
    
    class EventCondition {
        +ParameterType parameter
        +ComparisonType comparison
        +float thresholdValue
        +bool CheckCondition(EconomicSystem, RegionEntity)
        +string ToString()
    }
    
    class ParameterEffect {
        +EffectTarget target
        +EffectType effectType
        +float value
        +string description
        +void Apply(EconomicSystem, RegionEntity)
    }
    
    %% Enums
    class EventCategory {
        <<enumeration>>
        Economic
        Political
        Military
        Social
        Diplomatic
        Environment
        Technology
    }
    
    class ParameterType {
        <<enumeration>>
        Wealth
        Production
        PopulationSatisfaction
        InfrastructureLevel
        Resources
    }
    
    class ComparisonType {
        <<enumeration>>
        LessThan
        GreaterThan
        Equals
    }
    
    class EffectTarget {
        <<enumeration>>
        Wealth
        Production
        PopulationSatisfaction
        ProductivityFactor
        InfrastructureLevel
    }
    
    class EffectType {
        <<enumeration>>
        Add
        Multiply
        Set
    }
    
    %% Editor Interface
    class EventDisplayWindow {
        -List~DialogueEvent~ allEvents
        -List~DialogueEvent~ filteredEvents
        -DialogueEventManager eventManager
        -EconomicSystem economicSystem
        -int selectedEventIndex
        -int selectedChoiceIndex
        +void ShowWindow()
        -void RefreshEventList()
        -void DrawEventDetails(DialogueEvent)
        -void FilterEvents()
        -void DisplayChoice(EventChoice, int)
        -void OnEditorUpdate()
    }
    
    %% Economic System Classes
    class EconomicSystem {
        +RegionEntity testRegion
        +float productivityFactor
        +float laborElasticity
        +float capitalElasticity
        +void ManualTick()
    }
    
    class RegionEntity {
        +string Name
        +EconomyComponent Economy
        +PopulationComponent Population
        +InfrastructureComponent Infrastructure
        +ResourceComponent Resources
        +ProductionComponent Production
    }
    
    class EconomyComponent {
        +int Wealth
        +int Production
    }
    
    class PopulationComponent {
        +float Satisfaction
        +int LaborAvailable
        +void UpdateSatisfaction(float)
    }
    
    class InfrastructureComponent {
        +int Level
    }
    
    class ResourceComponent {
        +Dictionary~string, float~ GetAllResources()
    }
    
    class ProductionComponent {
        +void SetOutput(int)
    }
    
    %% Event System
    class EventBus {
        +static void Trigger(string eventName, object data)
        +static void Subscribe(string eventName, Action~object~ callback)
        +static void Unsubscribe(string eventName, Action~object~ callback)
    }
    
    %% Economic Debug Window
    class EconomicDebugWindow {
        -EconomicSystem economicSystem
        -void DrawDialogueTestingSection()
    }
    
    %% Relationships
    DialogueEventManager o-- DialogueEvent : manages
    DialogueEvent o-- EventChoice : contains
    DialogueEvent o-- EventCondition : evaluated by
    EventChoice o-- ParameterEffect : applies
    
    DialogueEventManager --> EconomicSystem : affects
    DialogueEventManager --> EventBus : publishes events
    
    DialogueEvent --> RegionEntity : modifies
    ParameterEffect --> RegionEntity : affects
    
    EventDisplayWindow --> DialogueEventManager : displays and controls
    EventDisplayWindow --> DialogueEvent : edits and displays
    
    EconomicDebugWindow --> DialogueEventManager : tests
    
    RegionEntity o-- EconomyComponent : has
    RegionEntity o-- PopulationComponent : has
    RegionEntity o-- InfrastructureComponent : has
    RegionEntity o-- ResourceComponent : has
    RegionEntity o-- ProductionComponent : has
    
    %% Event Flow Connections
    EventBus <.. DialogueEventManager : Subscribes to "EventCompleted"
    EventBus <.. DialogueEventManager : Subscribes to "EconomicTick"
    DialogueEventManager ..> EventBus : Triggers "DisplayEvent"
    DialogueEventManager ..> EventBus : Triggers "EventCompleted"
    
    %% Parameter enums relationships
    EventCondition --> ParameterType : uses
    EventCondition --> ComparisonType : uses
    ParameterEffect --> EffectTarget : uses
    ParameterEffect --> EffectType : uses
    DialogueEvent --> EventCategory : categorized by
    
    %% Add comments/notes
    note for DialogueEventManager "Central coordinator for dialogue events\nManages event queue and progression"
    note for DialogueEvent "Core data structure for narrative events\nContains choices and conditions"
    note for EventChoice "Player decision option\nCan trigger economic effects and chain to other events"
    note for EventCondition "Requirement for event triggering\nLinked to economic parameters"
    note for ParameterEffect "Game state modification\nChanges economic variables when choices are made"
    note for EventBus "Message broker for dialogue system\nEnables loose coupling between components"
    note for EventDisplayWindow "Editor UI for dialogue management\nProvides testing interface for event chain"
```


# Economic Cycles - UML Class Diagram

```mermaid
classDiagram
    %% Core Systems
    class GameManager {
        +static Instance: GameManager
        +testRegion: RegionEntity
        -Awake()
        -Start()
        -OnEnable()
        -OnDisable()
        -OnTurnEnded(object)
        +GetTestRegion() RegionEntity
        +ResetGame()
    }
    
    class TurnManager {
        -tickIntervalSeconds: float
        -timeScale: float
        -isPaused: bool
        -tickRoutine: Coroutine
        -currentDay: int
        -currentYear: int
        -DaysPerYear: int
        -Start()
        -Update()
        -TickLoop() IEnumerator
        +Pause()
        +Resume()
        +SetTimeScale(float)
    }
    
    class EventBus {
        -eventDictionary: Dictionary~string, Action~object~~
        -detailedLogging: bool
        +Subscribe(string, Action~object~)
        +Unsubscribe(string, Action~object~)
        +Trigger(string, object)
        +SetDetailedLogging(bool)
        +ClearAllSubscriptions()
    }
    
    class EconomicSystem {
        +static Instance: EconomicSystem
        +testRegion: RegionEntity
        +productivityFactor: float
        +laborElasticity: float
        +capitalElasticity: float
        +cycleMultiplier: float
        +wealthGrowthRate: float
        +priceVolatility: float
        +decayRate: float
        +maintenanceCostMultiplier: float
        +laborConsumptionRate: float
        -regions: Dictionary~string, RegionEntity~
        -productionCalculator: ProductionCalculator
        -infrastructureManager: InfrastructureManager
        -consumptionManager: ConsumptionManager
        -marketBalancer: MarketBalancer
        -cycleManager: CycleManager
        -priceVolatilityManager: PriceVolatilityManager
        -wealthManager: WealthManager
        -Awake()
        -InitializeSubsystems()
        -OnEnable()
        -OnDisable()
        -OnTurnEnded(object)
        -OnRegionUpdated(object)
        +ManualTick()
        +RegisterRegion(RegionEntity)
        +ProcessEconomicTick()
        -ProcessRegion(RegionEntity)
        -CreateDefaultRegion()
        -CalculateAndApplyProduction(RegionEntity)
        +GetRegion(string) RegionEntity
        +GetAllRegions() List~RegionEntity~
    }

    %% Entities
    class RegionEntity {
        +Name: string
        +Resources: ResourceComponent
        +Production: ProductionComponent
        +Economy: RegionEconomyComponent
        +Infrastructure: InfrastructureComponent
        +Population: PopulationComponent
        +RegionEntity(string, int, int)
        +RegionEntity(string)
        +ProcessTurn()
        +GetSummary() string
    }

    %% Components
    class ResourceComponent {
        -resources: Dictionary~string, float~
        +GenerateResources()
        +GetAllResources() Dictionary~string, float~
        +GetResource(string) float
        +UseResource(string, float) bool
        +GetResourceOverview() string
    }
    
    class ProductionComponent {
        -resources: ResourceComponent
        -output: int
        +ProductionComponent(ResourceComponent)
        +ProcessProduction()
        +GetOutput() int
        +SetOutput(int)
    }
    
    class RegionEconomyComponent {
        +Wealth: int
        +Production: int
        +RegionEconomyComponent(int, int)
        +UpdateEconomy(int)
    }
    
    class InfrastructureComponent {
        +Level: int
        -maintenanceCost: float
        +InfrastructureComponent()
        +Upgrade()
        +GetMaintenanceCost() float
    }
    
    class PopulationComponent {
        +LaborAvailable: int
        +Satisfaction: float
        +UpdateSatisfaction(float)
        +UpdateLabor(int)
    }

    %% Economic Subsystems
    class EconomicSubsystem {
        #economicSystem: EconomicSystem
        +EconomicSubsystem(EconomicSystem)
        +Process(RegionEntity)*
    }
    
    class ProductionCalculator {
        +ProductionCalculator(EconomicSystem)
        +Process(RegionEntity)
        +Calculate(float, float, float, float, float) float
    }
    
    class MarketBalancer {
        -supplyFactor: float
        -demandFactor: float
        +MarketBalancer(EconomicSystem) 
        +Process(RegionEntity)
    }
    
    class ConsumptionManager {
        +ConsumptionManager(EconomicSystem)
        +Process(RegionEntity)
    }
    
    class CycleManager {
        +CycleManager(EconomicSystem)
        +Process(RegionEntity)
    }
    
    class WealthManager {
        +WealthManager(EconomicSystem)
        +Process(RegionEntity)
    }
    
    class InfrastructureManager {
        +InfrastructureManager(EconomicSystem)
        +Process(RegionEntity)
    }
    
    class PriceVolatilityManager {
        +PriceVolatilityManager(EconomicSystem)
        +Process(RegionEntity)
    }

    %% Managers
    class MapManager {
        -regionPrefab: GameObject
        -regionsContainer: Transform
        -gridWidth: int
        -gridHeight: int
        -regionSpacing: float
        -regionViews: Dictionary~string, RegionView~
        -selectedRegionId: string
        -Start()
        -OnEnable()
        -OnDisable()
        -CreateRegionGrid()
        -CreateRegion(string, string, Vector3, Color)
        -OnRegionSelected(object)
        -OnRegionUpdated(object)
    }
    
    class RegionManager {
        -regionViews: Dictionary~string, RegionView~
        -economicSystem: EconomicSystem
        -dialogueManager: DialogueEventManager
        -Awake()
        -Start()
        -OnDestroy()
        -OnRegionsCreated(object)
        -OnEconomicTick(object)
        -InitializeRegionReferences()
        -OnRegionSelected(object)
        +SelectRegion(string)
        -TriggerRegionEvent(RegionEntity)
        -HasEvent(string) bool
    }
    
    class MapCameraController {
        -panSpeed: float
        -zoomSpeed: float
        -minZoom: float
        -maxZoom: float
        -boundCamera: bool
        -boundarySize: Vector2
        -cam: Camera
        -dragOrigin: Vector3
        -isDragging: bool
        -Awake()
        -Update()
        -HandlePanning()
        -HandleZooming()
        -EnforceBoundaries()
    }

    %% UI Components
    class RegionView {
        -mainRenderer: SpriteRenderer
        -highlightRenderer: SpriteRenderer
        -nameText: TextMeshPro
        -wealthText: TextMeshPro
        -productionText: TextMeshPro
        -satisfactionText: TextMeshPro
        +RegionName: string
        +RegionEntity: RegionEntity
        -economicSystem: EconomicSystem
        -lastWealthValue: int
        -lastProductionValue: int
        -lastSatisfactionValue: float
        -Awake()
        -Update()
        +Initialize(string, Color)
        -TryGetRegionEntityFromSystem()
        +Initialize(string, string, Color)
        -OnDestroy()
        -OnTurnEnded(object)
        -OnRegionUpdated(object)
        -OnEconomicTick(object)
        +SetSelected(bool)
        +SetHighlighted(bool)
        +SetRegionEntity(RegionEntity)
        -UpdateUIFromEntity()
        -UpdateUI(int, int, float)
        +UpdateStatus(int, int, float)
        -OnMouseDown()
    }
    
    class DialogueUIManager {
        -dialoguePanel: GameObject
        -titleText: TextMeshProUGUI
        -descriptionText: TextMeshProUGUI
        -choicesContainer: Transform
        -choiceButtonPrefab: GameObject
        -activeChoiceButtons: List~GameObject~
        -currentEvent: SimpleDialogueEvent
        -Start()
        -OnEnable()
        -OnDisable()
        -OnShowDialogueEvent(object)
        +ShowDialogueEvent(SimpleDialogueEvent)
        -OnChoiceSelected(int)
        +CloseDialogue()
        -ClearChoiceButtons()
        -ApplyOutcomes(List~DialogueOutcome~)
    }

    %% Dialogue System
    class DialogueEventManager {
        +static Instance: DialogueEventManager
        -availableEvents: List~DialogueEvent~
        -pendingEvents: Queue~DialogueEvent~
        -currentEvent: DialogueEvent
        +CurrentEvent: DialogueEvent
        -economicSystem: EconomicSystem
        -checkTimer: float
        -checkInterval: float
        -generateFallbackEvents: bool
        -Awake()
        -Update()
        -OnEnable()
        -OnDisable()
        -OnEconomicTick(object)
        -OnEventCompleted(object)
        +CheckForEvents()
        +QueueEvent(DialogueEvent)
        -DisplayNextEvent()
        +MakeChoice(int)
        -GenerateFallbackEvent(string)
        -LoadEvents()
        -CreateSampleEvents()
        +ResetEvent(string)
        +ResetAllEvents()
        +AddEvent(DialogueEvent)
        +GetAllEvents() List~DialogueEvent~
        +TriggerEvent(string)
    }
    
    class DialogueEvent {
        +id: string
        +title: string
        +description: string
        +category: EventCategory
        +hasTriggered: bool
        +conditions: List~EventCondition~
        +choices: List~EventChoice~
        +showConditions: bool
        +showChoices: bool
        +CheckConditions(EconomicSystem, RegionEntity) bool
        +ApplyChoice(int, EconomicSystem, RegionEntity)
        -RecalculateProduction(EconomicSystem, RegionEntity)
        +GetNextEventId(int) string
        +Clone() DialogueEvent
    }
    
    class EventChoice {
        +text: string
        +response: string
        +narrativeEffects: List~string~
        +economicEffects: List~ParameterEffect~
        +nextEventId: string
        +showEffects: bool
        +EventChoice()
        +EventChoice(EventChoice)
    }
    
    class EventCondition {
        +parameter: ParameterType
        +comparison: ComparisonType
        +thresholdValue: float
        +CheckCondition(EconomicSystem, RegionEntity) bool
        -GetParameterValue(ParameterType, EconomicSystem, RegionEntity) float
        +ToString() string
    }
    
    class ParameterEffect {
        +target: EffectTarget
        +effectType: EffectType
        +value: float
        +description: string
        +Apply(EconomicSystem, RegionEntity)
        -GetCurrentValue(EffectTarget, EconomicSystem, RegionEntity) float
        -SetParameterValue(EffectTarget, float, EconomicSystem, RegionEntity)
    }
    
    %% Enums
    class EventCategory {
        <<enumeration>>
        Economic
        Diplomatic
        Military
        Social
        Technology
        Disaster
    }
    
    %% Relationships
    GameManager --> RegionEntity : contains
    GameManager --> EconomicSystem : references
    EconomicSystem --> RegionEntity : references
    RegionEntity --> ResourceComponent : owns
    RegionEntity --> ProductionComponent : owns
    RegionEntity --> RegionEconomyComponent : owns
    RegionEntity --> InfrastructureComponent : owns
    RegionEntity --> PopulationComponent : owns
    ProductionComponent --> ResourceComponent : uses
    
    EconomicSubsystem <|-- ProductionCalculator
    EconomicSubsystem <|-- MarketBalancer
    EconomicSubsystem <|-- ConsumptionManager
    EconomicSubsystem <|-- CycleManager
    EconomicSubsystem <|-- WealthManager
    EconomicSubsystem <|-- InfrastructureManager
    EconomicSubsystem <|-- PriceVolatilityManager
    
    EconomicSystem --> ProductionCalculator : owns
    EconomicSystem --> MarketBalancer : owns
    EconomicSystem --> ConsumptionManager : owns
    EconomicSystem --> CycleManager : owns
    EconomicSystem --> WealthManager : owns
    EconomicSystem --> InfrastructureManager : owns
    EconomicSystem --> PriceVolatilityManager : owns
    
    MapManager --> RegionView : manages
    RegionManager --> RegionView : manages
    RegionManager --> EconomicSystem : references
    RegionManager --> DialogueEventManager : references
    
    RegionView --> RegionEntity : references
    RegionView --> EconomicSystem : references
    
    DialogueEventManager --> DialogueEvent : manages
    DialogueEvent --> EventChoice : contains
    DialogueEvent --> EventCondition : contains
    EventChoice --> ParameterEffect : contains
    
    DialogueUIManager --> DialogueEventManager : references
```

## Event Flow Diagram

```mermaid
sequenceDiagram
    participant Player
    participant TM as TurnManager
    participant EB as EventBus
    participant ES as EconomicSystem
    participant RE as RegionEntity
    participant RV as RegionView
    participant DM as DialogueEventManager
    participant UI as DialogueUIManager
    
    TM->>EB: Trigger("TurnEnded")
    EB->>ES: OnTurnEnded()
    ES->>ES: ProcessEconomicTick()
    ES->>RE: ProcessRegion(region)
    ES->>EB: Trigger("RegionUpdated", region)
    EB->>RV: OnRegionUpdated(region)
    RV->>RV: UpdateUI()
    
    Player->>RV: Click
    RV->>EB: Trigger("RegionSelected", regionId)
    EB->>ES: Set testRegion
    EB->>DM: CheckForEvents()
    DM->>DM: QueueEvent(event)
    DM->>DM: DisplayNextEvent()
    DM->>EB: Trigger("DisplayEvent", event)
    EB->>UI: OnShowDialogueEvent(event)
    UI->>UI: ShowDialogueEvent(event)
    
    Player->>UI: Select Choice
    UI->>DM: MakeChoice(choiceIndex)
    DM->>RE: ApplyChoice(choice)
    DM->>EB: Trigger("EventCompleted")
```

## Core Systems Architecture

```mermaid
graph TD
    %% Core Systems
    subgraph "Core Systems"
        GM[GameManager]
        TM[TurnManager]
        EB[EventBus]
    end
    
    %% Economic Systems
    subgraph "Economic Systems"
        ES[EconomicSystem]
        PC[ProductionCalculator]
        CM[ConsumptionManager]
        WM[WealthManager]
        MB[MarketBalancer]
        CY[CycleManager]
        IM[InfrastructureManager]
        PV[PriceVolatilityManager]
    end
    
    %% Entities and Components
    subgraph "Entities & Components"
        RE[RegionEntity]
        RC[ResourceComponent]
        PR[ProductionComponent]
        EC[RegionEconomyComponent]
        IC[InfrastructureComponent]
        PC2[PopulationComponent]
    end
    
    %% UI/Visual Systems
    subgraph "UI & Visual Systems"
        RV[RegionView]
        MM[MapManager]
        RM[RegionManager]
        MC[MapCameraController]
    end
    
    %% Dialogue Systems
    subgraph "Dialogue Systems"
        DM[DialogueEventManager]
        DE[DialogueEvent]
        DU[DialogueUIManager]
        EC2[EventChoice]
        ECD[EventCondition]
        PE[ParameterEffect]
    end
    
    %% Relationships
    GM --> TM
    GM --> RE
    
    ES --> RE
    ES --> PC
    ES --> CM
    ES --> WM
    ES --> MB
    ES --> CY
    ES --> IM
    ES --> PV
    
    RE --> RC
    RE --> PR
    RE --> EC
    RE --> IC
    RE --> PC2
    
    MM --> RV
    RM --> RV
    RV --> RE
    
    DM --> DE
    DE --> EC2
    DE --> ECD
    EC2 --> PE
    DU --> DM
    
    %% Event Bus connections
    GM -.-> EB
    TM -.-> EB
    ES -.-> EB
    RE -.-> EB
    RV -.-> EB
    RM -.-> EB
    MM -.-> EB
    DM -.-> EB
    DU -.-> EB
    
    classDef core fill:#f96,stroke:#333,stroke-width:2px
    classDef economic fill:#9cf,stroke:#333,stroke-width:2px
    classDef entity fill:#9f9,stroke:#333,stroke-width:2px
    classDef ui fill:#f9f,stroke:#333,stroke-width:2px
    classDef dialogue fill:#ff9,stroke:#333,stroke-width:2px
    
    class GM,TM,EB core
    class ES,PC,CM,WM,MB,CY,IM,PV economic
    class RE,RC,PR,EC,IC,PC2 entity
    class RV,MM,RM,MC ui
    class DM,DE,DU,EC2,ECD,PE dialogue
```


```mermaid

classDiagram
    %% Core Management Layer
    class GameManager {
        +static Instance: GameManager
        -turnManager: TurnManager
        -economicSystem: EconomicSystem
        -uiManager: UIManager
        -mapManager: MapManager
        -totalPopulation: int
        -currentTurn: int
        +GetTotalPopulation() int
        +GetPopulationGrowthRate() float
        +SetSimulationSpeed(float)
        +PauseSimulation()
        +ResumeSimulation()
    }

    class TurnManager {
        +IsPaused: bool
        -tickIntervalSeconds: float
        -timeScale: float
        -currentTurn: int
        +Pause()
        +Resume()
        +SetTimeScale(float)
        -TickLoop() IEnumerator
    }

    class EventBus {
        -eventDictionary: Dictionary~string, Action~object~~
        +Subscribe(string, Action~object~)
        +Unsubscribe(string, Action~object~)
        +Trigger(string, object)
    }

    %% Core Systems
    class EconomicSystem {
        +productivityFactor: float
        +laborElasticity: float
        +capitalElasticity: float
        -regions: Dictionary~string, RegionEntity~
        +RegisterRegion(RegionEntity)
        +GetRegion(string) RegionEntity
        +GetAllRegionIds() List~string~
        +ProcessEconomicTick()
        -ProcessRegion(RegionEntity)
        -CalculateProduction(RegionEntity)
        +UpdateRegion(RegionEntity)
        +GetTotalWealth() int
    }

    %% UI Management Layer
    class UIManager {
        -mainCanvas: Canvas
        -topPanel: RectTransform
        -bottomPanel: RectTransform
        -leftPanel: RectTransform
        -rightPanel: RectTransform
        -centerPanel: RectTransform
        -uiModules: List~UIModuleBase~
        +Initialize()
        +RegisterModule(UIModuleBase)
        +CreateModule<T>() T
        +GetModule<T>() T
        +ShowAllModules()
        +HideAllModules()
    }

    class UIModuleBase {
        #isVisible: bool
        +Initialize()
        +Show()
        +Hide()
        +ToggleVisibility()
    }

    class SimulationUIModule {
        -turnManager: TurnManager
        -economicSystem: EconomicSystem
        -playButton: Button
        -pauseButton: Button
        -stepButton: Button
        -speedSlider: Slider
        -statusText: TextMeshProUGUI
        -ResumeSimulation()
        -PauseSimulation()
        -StepSimulation()
        -ChangeSpeed(float)
        -UpdateStatusText()
    }

    class VisualizationUIModule {
        -mapManager: MapManager
        -defaultColorButton: Button
        -positionColorButton: Button
        -wealthColorButton: Button
        -productionColorButton: Button
        -legendPanel: GameObject
        -legendTitle: TextMeshProUGUI
        -minColorImage: Image
        -maxColorImage: Image
    }

    class StatsDisplayUIModule {
        -gameManager: GameManager
        -economicSystem: EconomicSystem
        -statTexts: Dictionary~string, TextMeshProUGUI~
        -updateTimer: float
        -UpdateStats()
        -FormatNumber(float) string
        -FormatPercent(float) string
    }

    class MapColorController {
        +mapManager: MapManager
        +defaultColorButton: Button
        +positionColorButton: Button
        +wealthColorButton: Button
        +productionColorButton: Button
        +legendPanel: GameObject
        +SetColorMode(RegionColorMode)
        -UpdateLegend(RegionColorMode)
    }

    %% Map & Visualization
    class MapManager {
        -regionPrefab: GameObject
        -regionsContainer: Transform
        -gridWidth: int
        -gridHeight: int
        -hexSize: float
        -colorMode: RegionColorMode
        -regionViews: Dictionary~string, RegionView~
        -selectedRegionId: string
        -economicSystem: EconomicSystem
        +SetColorMode(int)
        +UpdateRegionColors()
        -CreateHexGrid()
        -GetRegionColor(int, int, string) Color
        -CreateRegionEntity(string)
    }

    class RegionView {
        +mainRenderer: SpriteRenderer
        +highlightRenderer: SpriteRenderer
        +nameText: TextMeshPro
        +wealthText: TextMeshPro
        +productionText: TextMeshPro
        +RegionName: string
        +RegionEntity: RegionEntity
        +Initialize(string, string, Color)
        +SetHighlighted(bool)
        +SetRegionEntity(RegionEntity)
        -UpdateUIFromEntity()
        -UpdateUI(int, int)
        +UpdateColor(Color)
        +Deselect()
    }

    %% Entities
    class RegionEntity {
        +Name: string
        +Wealth: int
        +Production: int
        +LaborAvailable: float
        +InfrastructureLevel: float
        +GetSummary() string
    }

    %% Event & Dialogue System
    class EventManager {
        +static Instance: EventManager
        -availableEvents: List~GameEvent~
        -pendingEvents: Queue~GameEvent~
        -currentEvent: GameEvent
        -dialogueView: DialogueView
        -economicSystem: EconomicSystem
        -gameManager: GameManager
        +CheckForEvents()
        -QueueEvent(GameEvent)
        -DisplayNextEvent()
        -ProcessChoice(int)
        +TriggerRandomEvent()
        +ResetAllEvents()
    }

    class GameEvent {
        +id: string
        +title: string
        +description: string
        +hasTriggered: bool
        +conditionType: ConditionType
        +conditionValue: float
        +choices: List~EventChoice~
        +IsConditionMet(EconomicSystem, RegionEntity) bool
    }

    class EventChoice {
        +text: string
        +result: string
        +wealthEffect: int
        +productionEffect: int
        +laborEffect: int
        +nextEventId: string
    }

    class DialogueView {
        +dialoguePanel: RectTransform
        +eventTitleText: TextMeshProUGUI
        +eventDescriptionText: TextMeshProUGUI
        +responseContainer: RectTransform
        +responseButtonTemplate: Button
        +ShowDialogue(string, string, string, List~string~)
        +ShowDialogueWithEffects(string, string, string, List~string~, List~ResponseEffects~)
        +HideDialogue()
        -HandleResponseSelected(int)
        +OnResponseSelected: event Action~string, int~
    }

    %% Debug & Tools
    class EconomicDebugWindow {
        -economicSystem: EconomicSystem
        -autoRunEnabled: bool
        -autoRunInterval: float
        -productivityFactor: float
        -laborElasticity: float
        -capitalElasticity: float
        -wealthHistory: List~int~
        -productionHistory: List~int~
        -RunSimulationTick()
        -ResetSimulation()
        -SyncFromSystem()
        -ApplyToSystem()
        -RecordHistory()
    }

    %% Relationships - Core Management
    GameManager *-- TurnManager : manages
    GameManager *-- EconomicSystem : manages
    GameManager *-- UIManager : manages
    GameManager *-- MapManager : manages
    GameManager ..> EventBus : uses for events

    TurnManager ..> EventBus : triggers TurnEnded

    %% Relationships - Economic System
    EconomicSystem "1" o-- "*" RegionEntity : contains
    EconomicSystem ..> EventBus : triggers events

    %% Relationships - UI System
    UIManager "1" *-- "*" UIModuleBase : manages
    UIModuleBase <|-- SimulationUIModule : extends
    UIModuleBase <|-- VisualizationUIModule : extends
    UIModuleBase <|-- StatsDisplayUIModule : extends

    SimulationUIModule ..> TurnManager : controls
    SimulationUIModule ..> EconomicSystem : references

    VisualizationUIModule *-- MapColorController : has
    VisualizationUIModule ..> MapManager : references

    StatsDisplayUIModule ..> GameManager : references
    StatsDisplayUIModule ..> EconomicSystem : references

    %% Relationships - Map & Visualization
    MapManager "1" *-- "*" RegionView : manages
    MapManager ..> EconomicSystem : gets region data
    MapManager ..> EventBus : handles RegionSelected

    RegionView ..> RegionEntity : displays 
    RegionView ..> EventBus : triggers RegionSelected

    %% Relationships - Event System
    EventManager "1" *-- "*" GameEvent : manages
    EventManager *-- DialogueView : controls
    EventManager ..> EconomicSystem : modifies regions
    EventManager ..> GameManager : controls simulation

    GameEvent "1" *-- "*" EventChoice : contains
    
    DialogueView ..> EventBus : publishes responses

    %% Debug & Tool Relationships
    EconomicDebugWindow ..> EconomicSystem : debugs & modifies

    %% Notes for clarity
    note for EventBus "Central messaging system that enables loose coupling between components"
    note for GameManager "Singleton that manages the entire game simulation"
    note for EconomicSystem "Processes economic calculations for all regions"
    note for RegionEntity "Represents a single economic unit with resources and production"
    note for MapManager "Manages the visual grid of regions and their colors"
    note for UIManager "Manages all UI modules and their layout"
    note for EventManager "Handles dialogue events based on game state"

    %% Class style definitions
    classDef core fill:#f9d5e5,stroke:#333,stroke-width:1px
    classDef system fill:#eeeeee,stroke:#333,stroke-width:1px
    classDef ui fill:#e3f2fd,stroke:#333,stroke-width:1px
    classDef entity fill:#e8f5e9,stroke:#333,stroke-width:1px
    classDef event fill:#fff8e1,stroke:#333,stroke-width:1px
    classDef debug fill:#f5f5f5,stroke:#333,stroke-width:1px

    %% Assign classes
    class GameManager,TurnManager,EventBus core
    class EconomicSystem system
    class UIManager,UIModuleBase,SimulationUIModule,VisualizationUIModule,StatsDisplayUIModule,MapColorController ui
    class MapManager,RegionView ui
    class RegionEntity entity
    class EventManager,GameEvent,EventChoice,DialogueView event
    class EconomicDebugWindow debug
```


```mermaid
classDiagram
    %% Entities
    class RegionEntity {
        +string Name
        +int Wealth
        +int Production
        +float LaborAvailable
        +float InfrastructureLevel
        +RegionEntity(string name, int initialWealth, int initialProduction)
        +RegionEntity(string name)
        +NationEntity GetNation()
        +void ProcessTurn()
        +string GetSummary()
    }
    
    class NationEntity {
        +string Id
        +string Name
        +Color Color
        -List~string~ regionIds
        +float TotalWealth
        +float TotalProduction
        +float Stability
        +enum PolicyType
        +enum DiplomaticStatus
        -Dictionary~PolicyType, float~ policies
        -Dictionary~string, DiplomaticStatus~ diplomaticRelations
        +NationEntity(string id, string name, Color color)
        +void AddRegion(string regionId)
        +void RemoveRegion(string regionId)
        +List~string~ GetRegionIds()
        +void SetPolicy(PolicyType type, float value)
        +float GetPolicy(PolicyType type)
        +void SetDiplomaticStatus(string otherNationId, DiplomaticStatus status)
        +DiplomaticStatus GetDiplomaticStatus(string otherNationId)
        +string GetSummary()
    }
    
    %% Systems
    class NationSystem {
        -static NationSystem _instance
        +static NationSystem Instance
        -NationManager nationManager
        -EconomicSystem economicSystem
        -NationEconomicSubsystem economicSubsystem
        -NationDiplomaticSubsystem diplomaticSubsystem
        -NationStabilitySubsystem stabilitySubsystem
        -NationEventSubsystem eventSubsystem
        -NationPolicySubsystem policySubsystem
        -Dictionary~string, RegionEntity~ regionCache
        -void Awake()
        -void Start()
        -void OnEnable()
        -void OnDisable()
        -void OnTurnProcessed(object data)
        -void OnRegionsAssignedToNations(object data)
        -void OnRegionNationChanged(object data)
        -void InitializeSubsystems()
        -void UpdateRegionCache()
        -void UpdateAllNationData()
        -void UpdateNationData(string nationId)
        +Dictionary~string, RegionEntity~ GetRegionCache()
        +void SetNationPolicy(string nationId, NationEntity.PolicyType policyType, float value)
        +float GetNationPolicy(string nationId, NationEntity.PolicyType policyType)
    }
    
    %% Views
    class RegionView {
        +SpriteRenderer mainRenderer
        +SpriteRenderer highlightRenderer
        +TextMeshPro nameText
        +TextMeshPro wealthText
        +TextMeshPro productionText
        +string RegionName
        +RegionEntity RegionEntity
        -EconomicSystem economicSystem
        -void Awake()
        -void Update()
        +void Initialize(string id, string name, Color color)
        -void OnDestroy()
        -void TryGetRegionEntityFromSystem()
        -void OnRegionUpdated(object data)
        -void OnEconomicTick(object data)
        +void SetHighlighted(bool highlighted)
        +void SetRegionEntity(RegionEntity regionEntity)
        -void UpdateUIFromEntity()
        -void UpdateUI(int wealth, int production)
        -void OnMouseDown()
        +void Deselect()
        +void UpdateColor(Color newColor)
    }
    
    class NationView {
        -string nationId
        -TextMeshProUGUI nationNameText
        -TextMeshProUGUI wealthText
        -TextMeshProUGUI productionText
        -TextMeshProUGUI stabilityText
        -TextMeshProUGUI regionsText
        -Image nationColorImage
        -NationEntity nation
        -NationManager nationManager
        -void Start()
        -void OnDestroy()
        +void SetNation(string id)
        -void UpdateUI()
        -void OnNationStatisticsUpdated(object data)
    }
    
    %% Relationships
    RegionEntity --> NationEntity : references through GetNation()
    NationEntity "1" *-- "*" RegionEntity : owns/contains
    
    NationSystem --> NationEntity : manages
    NationSystem --> RegionEntity : caches
    
    RegionView --> RegionEntity : displays
    NationView --> NationEntity : displays
    
    NationSystem ..> NationView : updates via events
    NationSystem ..> RegionView : updates via events
    
    NationEntity "1" -- "*" RegionEntity : contains references to
    
    %% External dependencies represented as interfaces
    class NationManager {
        <<interface>>
    }
    class EconomicSystem {
        <<interface>>
    }
    class EventBus {
        <<interface>>
    }
    
    NationSystem --> NationManager : uses
    NationSystem --> EconomicSystem : uses
    NationSystem --> EventBus : publishes/subscribes
    RegionView --> EventBus : publishes/subscribes
    NationView --> EventBus : publishes/subscribes
    RegionEntity --> NationManager : uses to find nation
```