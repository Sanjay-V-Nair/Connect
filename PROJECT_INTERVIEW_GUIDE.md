# Connect Project - Interview Master Guide

This document is a complete technical reference for interview discussions about this project.

---

## 1) Project Overview

`Connect` is a Unity puzzle game where players connect matching node pairs on a grid by drawing non-overlapping paths.

- Engine: Unity (C# scripts under `Assets/Scripts`)
- Primary scenes: `DashboardScene` and `GameScene`
- Data-driven levels: ScriptableObjects in `Assets/SO`
- UI style: reusable `DrawView<T>`-based rendering pattern
- Communication style: static generic `EventBus<T>` for decoupled events

---

## 2) High-Level Architecture

The architecture is a hybrid of:

- **Persistent global managers** for app-level concerns
- **Scene-level orchestrators** for gameplay/dashboard
- **Feature systems** for levels, eventing, datastore
- **View components** for UI and tile rendering
- **Model/static state** for runtime and persistence-backed data

### Core architectural layers

1. **Core layer** (`Assets/Scripts/Core`)
   - Bootstrapping, scene transitions, global audio, gameplay flow entry points.
2. **Systems layer** (`Assets/Scripts/Systems`)
   - Level generation and path logic, event bus, local persistence.
3. **Views layer** (`Assets/Scripts/Views`)
   - Dashboard/game UI and tile visuals with reusable draw/reset mechanics.
4. **Models layer** (`Assets/Scripts/Models`)
   - Game-level runtime state (`GameStateData`).

---

## 3) Runtime Flow (End-to-End)

### App startup

1. Unity loads first enabled scene (`DashboardScene`).
2. `GameManager.Awake()` singleton initializes + `DontDestroyOnLoad`.
3. `AudioManager.Awake()` singleton initializes + `DontDestroyOnLoad`.
4. `GameManager.OnEnable()` loads saved `"Level"` from `PlayerPrefs` through `LocalDatastore` and stores it in `GameStateData`.

### Dashboard flow

1. `DashboardView.OnEnable()` subscribes to `DashboardTabEvent` and draws initial UI.
2. Tab clicks (`DashboardSingleTabView`) raise `DashboardTabEvent`.
3. `DashboardView` receives event, resets pages, opens selected tab.
4. Play button calls `GameManager.LoadSceneAsync("GameScene")`.

### Game flow

1. `GameplayManager.Start()` renders gameplay UI and loads current level.
2. `GameplayManager.Render()` fetches level from `LevelManager` using `GameStateData.GetGameLevel()`.
3. `LevelController.StartLevel()` clears previous grid state and asks `GridSpawner` to create board tiles.
4. `LevelController.Update()` handles pointer-driven path drawing:
   - Pointer down on node -> begin path
   - Drag over valid adjacent tile -> extend path
   - Pointer backtrack -> remove last segment
   - Pointer up -> finalize/keep partial path and test completion
5. On complete puzzle, `LevelController` raises `LevelCompletedEvent`.
6. `GameplayManager` listens and shows `GameResultView` (with win audio).

---

## 4) Systems and Responsibilities

## 4.1 Core System

### `GameManager` (`Assets/Scripts/Core/GameManager.cs`)
- Global singleton app coordinator.
- Reads/saves initial level state through datastore.
- Central scene navigation entry point.
- Scene-based BGM switching through `AudioManager`.

Key methods:
- `Awake()`
- `SetGameData()`
- `LoadSceneAsync(string sceneName)`
- `SetAudioForScene(string sceneName)`

### `AudioManager` (`Assets/Scripts/Core/AudioManager.cs`)
- Global singleton audio orchestrator.
- Uses separate audio sources for BGM and one-shot SFX.

Key methods:
- `PlayGameAudio(bool)`
- `PlayLevelAudio(bool)`
- `PlayButtonClickAudio()`
- `PlayEdgeProgressAudio()`
- `PlayConnectAudio()`
- `PlayWinAudio()`

### `GameplayManager` (`Assets/Scripts/Core/GameplayManager.cs`)
- Scene-level coordinator for GameScene.
- Connects level loading, result popup, and gameplay UI buttons.
- Subscribes to `LevelCompletedEvent`.

---

## 4.2 Level System

### `LevelController` (`Assets/Scripts/Systems/LevelSystem/LevelController.cs`)
- Main puzzle logic brain.
- Handles:
  - Input state (`isDragging`, current path)
  - Path rules (adjacency, backtracking, overlap clearing)
  - Board updates via `TileView` edges
  - Win condition checks and event dispatch

### `GridSpawner` (`Assets/Scripts/Systems/LevelSystem/GridSpawner.cs`)
- Instantiates tile prefabs into grid.
- Maps level node endpoints into tile data.

### `LevelManager` (`Assets/Scripts/Systems/LevelSystem/LevelManager.cs`)
- Provides `LevelData` by level number from `LevelsData`.

### Data classes
- `LevelData.cs` -> one level definition (dimensions + node pairs)
- `LevelsData.cs` -> list of level definitions
- `Node`, `NodePair` -> endpoint metadata

### Utility
- `EdgeResolver.cs` -> edge rules based on board position.

---

## 4.3 Eventing System

### `EventBus<T>` (`Assets/Scripts/Systems/EventBus/EventBus.cs`)
- Generic static observer implementation:
  - `Subscribe(Action<T>)`
  - `Unsubscribe(Action<T>)`
  - `Raise(T eventData)`

Events used:
- `DashboardEvents.DashboardTabEvent`
- `GameSceneEvents.LevelCompletedEvent`

---

## 4.4 Persistence System

### `LocalDatastore` (`Assets/Scripts/Systems/Datastore/LocalDatastore.cs`)
- Thin wrapper over `PlayerPrefs` for string keys/values.

Current usage:
- Key `"Level"` for user progression bootstrap.

---

## 4.5 UI/View System

### Base abstractions

- `DrawView<T>` (`Assets/Scripts/Core/DrawView.cs`)
  - Template lifecycle: `Draw -> CanDraw -> Render / Reset`
  - Utilities: `IsolatedDraw`, `IsolatedReset`

- `PopupDraw<T>` (`Assets/Scripts/Core/PopupDraw.cs`)
  - Popup extension with DOTween scale animation on enable.

### Dashboard views

- `DashboardView.cs`
- `DashboardTabsView.cs`
- `DashboardSingleTabView.cs`
- `CreditsPageView.cs`
- `InventoryPageView.cs`
- `SettingsPopupView.cs`

### Gameplay views

- `GameResultView.cs`
- `TileView.cs`
- Reusable button wrapper `ButtonView.cs`

---

## 5) Design Patterns Used (Where and Why)

## 5.1 Singleton Pattern

Used for globally shared managers:
- `GameManager.Instance`
- `AudioManager.Instance`
- `LevelManager.Instance`
- `LevelController.Instance`

Why:
- Easy access across scenes and components.
- Simplifies scene transition and global service calls.

Trade-off:
- Tighter coupling and harder unit testing.

## 5.2 Observer Pattern (Event Bus)

Implemented via `EventBus<T>`.

Examples:
- Dashboard tab changes: button views publish -> dashboard listens.
- Win event: level logic publishes -> gameplay manager listens.

Why:
- Decouples sender and receiver classes.

## 5.3 Template Method Pattern

`DrawView<T>` defines fixed draw lifecycle while subclasses implement specific rendering logic.

Why:
- Consistent UI rendering contract.
- Less duplicated draw/reset logic.

## 5.4 Data-Driven Design (ScriptableObject)

Level content is authored in assets (`LevelData` / `LevelsData`) not hardcoded.

Why:
- Faster level iteration without code edits.
- Better designer-developer workflow.

## 5.5 Spawner/Factory-like Pattern

`GridSpawner` constructs runtime grid objects from level metadata.

Why:
- Isolates board instantiation from gameplay logic.

## 5.6 Composite-style View Composition

Parent views render child views through helper methods (`IsolatedDraw/IsolatedReset`).

Why:
- Structured UI composition with explicit parent-child draw control.

---

## 6) Data and State Architecture

### Persistent data
- `PlayerPrefs` key `"Level"` via `LocalDatastore`.

### Runtime state
- `GameStateData.currentLevel` (static in-memory state).
- `LevelController` runtime interaction state:
  - active path, map of completed paths, drag flags.

### Level content
- `Assets/SO/AllLevels.asset` stores references to `LevelData` assets.

---

## 7) Audio Architecture

Audio is centralized in `AudioManager`.

- **BGM source** (`gameAudioSource`)
  - Dashboard music
  - Game level music
- **SFX source** (`genericAudioSource`)
  - Button click
  - Edge progress while path drawing
  - Pair connect completion
  - Win audio

Important trigger points:
- `GameManager.SetAudioForScene()`
- `ButtonView.OnButtonViewClick()`
- `LevelController.AddTileToPath()` -> edge progress SFX
- `LevelController.HandlePointerUp()` -> connect SFX
- `GameResultView.Render()` -> win SFX

---

## 8) Scene Architecture

### `DashboardScene`
- Home/dashboard UX entry.
- Tabs + pages + settings popup.
- `DashboardView` root view orchestrates tab/page transitions.

### `GameScene`
- Puzzle board and gameplay controls.
- `GameplayManager` and `LevelController` coordinate game loop.

Build order and active scenes are configured in:
- `ProjectSettings/EditorBuildSettings.asset`

---

## 9) Important Engineering Decisions

1. **Static event bus over direct references**
   - Reduced coupling among UI and gameplay modules.

2. **Singleton managers for cross-scene services**
   - Simplifies scene transitions/audio continuity.

3. **ScriptableObject-driven levels**
   - Decouples content creation from gameplay code.

4. **View lifecycle abstraction**
   - Uniform and reusable UI behavior through `DrawView<T>`.

---

## 10) Known Limitations and Technical Debt

1. **Progression update gap**
   - Level increment persistence after win is incomplete in current code path (`GameStateData.UpdateGameLevel()` exists but is not integrated end-to-end).

2. **Large `LevelController`**
   - Input handling, board mutation, and win logic live in one class (high responsibility concentration).

3. **Singleton-heavy design**
   - Harder to unit test and mock dependencies.

4. **EventBus safety**
   - `Raise` iterates active set directly; can be risky if listeners mutate subscriptions during invocation.

5. **Partially implemented tabs**
   - Enum values for some tabs do not have full rendering behavior yet.

6. **Controller layer not fully used**
   - `Controllers/DashboardController.cs` is currently empty.

---

## 11) Interview-Ready Talking Points (Short)

Use this 60-90 second summary:

"This project is a Unity puzzle game built around scene-level coordinators and reusable systems. `GameManager` and `AudioManager` are persistent singletons for app state and audio continuity across scenes. Gameplay is data-driven through ScriptableObject level assets. Core puzzle logic lives in `LevelController`, which processes pointer input, builds valid paths with overlap/backtracking rules, and emits completion events through a generic static `EventBus<T>`. UI is structured with a `DrawView<T>` template lifecycle so dashboard/game views follow a consistent render-reset contract. The architecture optimized developer speed and clarity for a small game, with known next-step refactors around splitting `LevelController`, reducing singleton coupling, and hardening progression persistence." 

---

## 12) Top 25 Interview Questions and Strong Answers

1. **How is scene navigation handled?**  
   Through `GameManager.LoadSceneAsync(sceneName)` as a centralized entry point.

2. **How do you maintain global state across scenes?**  
   With persistent singleton managers (`DontDestroyOnLoad`) and static `GameStateData`.

3. **How do you avoid duplicate managers when scenes reload?**  
   In `Awake()`, duplicate instances self-destroy if `Instance != this`.

4. **How are levels configured?**  
   `LevelData` ScriptableObjects define grid size and node pairs; `LevelsData` aggregates levels.

5. **How does the game know which level to load?**  
   `GameplayManager` reads `GameStateData.GetGameLevel()` and requests `LevelManager.GetLevel(level)`.

6. **How are tile objects created?**  
   `GridSpawner.SpawnGrid()` instantiates a prefab per coordinate and binds node/tile metadata.

7. **How is player input handled during gameplay?**  
   `LevelController.Update()` uses `Pointer.current` and routes to pointer-down/drag/up handlers.

8. **How do you enforce valid path movement?**  
   Adjacency check (Manhattan distance), node matching by pair index, and loop prevention.

9. **How is backtracking supported?**  
   Dragging to previous tile triggers `RemoveLastTileFromPath()`.

10. **What happens if player crosses another path?**  
    Existing conflicting pair path is cleared using `ClearPathByPair()`.

11. **How is puzzle completion validated?**  
    For each pair index, path endpoints must match the node pair endpoints.

12. **How is completion communicated to UI?**  
    `LevelController` raises `LevelCompletedEvent` via `EventBus`, handled by `GameplayManager`.

13. **Where is Observer pattern used?**  
    `EventBus<T>` for dashboard tab events and gameplay completion events.

14. **Where is Template Method pattern used?**  
    `DrawView<T>` defines lifecycle and delegates render/reset to subclasses.

15. **How are button actions standardized?**  
    `ButtonView` wraps Unity `Button` setup and injects audio + callback logic.

16. **How is audio managed?**  
    `AudioManager` with separate sources for music and one-shot SFX.

17. **How do you switch music per scene?**  
    `GameManager.SetAudioForScene(sceneName)` calls appropriate audio methods.

18. **Why use ScriptableObjects here?**  
    Content scalability and faster iteration without changing gameplay code.

19. **What are current architecture trade-offs?**  
    Fast implementation and clear flow, but higher singleton coupling and reduced testability.

20. **What would you refactor first?**  
    Split `LevelController` into smaller services (input interpreter, path rules engine, board renderer adapter).

21. **How would you improve persistence?**  
    Add post-win progression updates and write-through synchronization to `PlayerPrefs`.

22. **How would you make this testable?**  
    Extract pure path-validation logic from MonoBehaviour dependencies and inject services.

23. **What failure cases did you recently fix?**  
    Duplicate singleton instances and stale event subscriptions causing null references after scene return.

24. **How do tabs update dashboard pages?**  
    `DashboardSingleTabView` raises `DashboardTabEvent`; `DashboardView` handles and opens matching page.

25. **What demonstrates modular thinking in this project?**  
    Separated systems (`LevelSystem`, `EventBus`, `Datastore`) and reusable views (`DrawView`, `ButtonView`, `PopupDraw`).

---

## 13) File Index (Fast Navigation)

### Core
- `Assets/Scripts/Core/GameManager.cs`
- `Assets/Scripts/Core/AudioManager.cs`
- `Assets/Scripts/Core/GameplayManager.cs`
- `Assets/Scripts/Core/DrawView.cs`
- `Assets/Scripts/Core/PopupDraw.cs`
- `Assets/Scripts/Core/SceneType.cs`

### Models
- `Assets/Scripts/Models/GameStateData.cs`

### Systems
- `Assets/Scripts/Systems/EventBus/EventBus.cs`
- `Assets/Scripts/Systems/EventBus/DashboardEvents.cs`
- `Assets/Scripts/Systems/Datastore/LocalDatastore.cs`
- `Assets/Scripts/Systems/LevelSystem/LevelController.cs`
- `Assets/Scripts/Systems/LevelSystem/GridSpawner.cs`
- `Assets/Scripts/Systems/LevelSystem/LevelManager.cs`
- `Assets/Scripts/Systems/LevelSystem/LevelData.cs`
- `Assets/Scripts/Systems/LevelSystem/LevelsData.cs`
- `Assets/Scripts/Systems/LevelSystem/EdgeResolver.cs`

### Views
- `Assets/Scripts/Views/DashboardView.cs`
- `Assets/Scripts/Views/DashboardTabsView.cs`
- `Assets/Scripts/Views/DashboardSingleTabView.cs`
- `Assets/Scripts/Views/ButtonView.cs`
- `Assets/Scripts/Views/GameResultView.cs`
- `Assets/Scripts/Views/TileView.cs`
- `Assets/Scripts/Views/SettingsPopupView.cs`
- `Assets/Scripts/Views/Credits/CreditsPageView.cs`
- `Assets/Scripts/Views/Inventory/InventoryPageView.cs`

### Scenes and content
- `Assets/Scenes/DashboardScene.unity`
- `Assets/Scenes/GameScene.unity`
- `ProjectSettings/EditorBuildSettings.asset`
- `Assets/SO/AllLevels.asset`
- `Assets/SO/Levels/Level1.asset`

---

## 14) Quick Revision Sheet (Before Interview)

If you have only 10 minutes, review these:

1. `GameManager` (scene flow + persistence bootstrap)
2. `AudioManager` (global audio strategy)
3. `LevelController` (core puzzle algorithm)
4. `EventBus<T>` (observer architecture)
5. `DrawView<T>` (template lifecycle pattern)
6. `GridSpawner + LevelData` (data-driven level pipeline)

Then rehearse:
- Architecture summary (Section 11)
- Top 25 questions (Section 12)
- Technical debt + refactor roadmap (Section 10)

