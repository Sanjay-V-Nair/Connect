# Connect - Game Overview

## 1. What is "Connect"?
**Connect** is a 2D puzzle game built in Unity. The primary objective is to connect matching pairs of colored nodes on a grid by drawing paths between them. The game is similar to classic "Flow Free" mechanics where paths cannot overlap, and the puzzle is solved when all node pairs are successfully connected.

## 2. Core Game Mechanics
The game's interaction is primarily drag-and-drop based, governed by the following rules:
- **Starting a Path:** The player taps or clicks (pointer down) on a node to begin drawing a path.
- **Extending a Path:** Dragging the pointer over a valid, adjacent grid tile (checked via Manhattan distance) extends the path.
- **Backtracking:** If the player drags back over the previous tile in their current path, the last segment is erased.
- **Hole Pairs (Portals):** Some levels feature paired holes that act as portals. If a path enters one hole, it instantly teleports and continues out of its paired corresponding hole on the grid.
- **Path Conflicts:** Paths are strictly non-overlapping. If the player draws a path that crosses an already existing path belonging to a different node pair, the older conflicting path is cleared from the board.
- **Completion:** Releasing the pointer (pointer up) finalizes the current path. The level is considered complete when every node pair has been successfully connected with a valid path.

## 3. High-Level Technical Architecture
The project follows a modular, decoupled architecture leveraging Unity's ScriptableObjects and custom patterns.

### Key Architectural Layers
1. **Core Singletons (App-Level):** 
   - `GameManager`: Coordinates app state, data loading, and scene transitions.
   - `AudioManager`: Manages Background Music (BGM) and Sound Effects (SFX) globally.
2. **Scene Orchestrators:** 
   - `GameplayManager`: Manages the flow of the Game Scene (loading levels, handling the win state popup).
3. **Systems Layer:**
   - **Level System:** `LevelController` handles all puzzle logic, input processing, and path validation. `GridSpawner` takes level metadata and instantiates the board.
   - **Eventing:** A static generic `EventBus<T>` acts as a central observer, allowing systems to communicate (e.g., `LevelCompletedEvent`) without tight coupling.
   - **Persistence:** `LocalDatastore` wraps `PlayerPrefs` to save player level progression.
4. **UI / Views Layer:**
   - All UI components inherit from a reusable `DrawView<T>` pattern, standardizing UI lifecycle operations like `Draw`, `Render`, and `Reset`.

## 4. Data-Driven Level Design
Levels in **Connect** are heavily data-driven to improve the designer workflow:
- **ScriptableObjects:** Levels are authored as `LevelData` (dimensions and node pair metadata) and aggregated in a `LevelsData` list.
- **Runtime Generation:** Instead of hardcoding scenes, `GridSpawner` dynamically builds the board at runtime based on the selected `LevelData`.

## 5. Summary for AI Models
If you are an AI model analyzing this project, here is a quick mapping of responsibilities:
- **Puzzle Rules / Input:** Look at `LevelController.cs` for logic regarding path drawing, overlap resolution, and win condition checks.
- **Event Handling:** `EventBus.cs` is used globally for decoupled communication.
- **UI Logic:** Look for classes inheriting from `DrawView<T>` or `PopupDraw<T>`.
- **Level Loading:** Levels are injected dynamically from `ScriptableObjects` (`LevelData.cs`) into the `LevelManager.cs` and `GridSpawner.cs`.
