using System.Collections.Generic;
using Connect.Core;
using Connect.Systems.EventBus;
using Connect.Views;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Connect.Systems.LevelSystem {
    public class LevelController : MonoBehaviour {
        public static LevelController Instance { get; private set; }

        [SerializeField] private GridLayoutGroup gridLayoutGroup;
        [SerializeField] private GridSpawner gridSpawner;
        [SerializeField] private Transform gridParent;
        
        private LevelData currentLevelData;
        private TileView[,] grid;
        
        // Path tracking
        private bool isDragging = false;
        private int currentPathPairIndex = -1;
        private Color currentPathColor;
        private List<TileView> currentPath = new List<TileView>();
        
        // Store all completed/active paths by pair index
        private Dictionary<int, List<TileView>> paths = new Dictionary<int, List<TileView>>();

        private void Awake() {
            if (Instance == null) Instance = this;
        }

        public void StartLevel(LevelData levelData) {
            currentLevelData = levelData;
            gridSpawner.ClearGrid(gridParent);
            paths.Clear();
            currentPath.Clear();
            isDragging = false;

            gridLayoutGroup.constraintCount = levelData.gridYSize; // Number of columns is actually the size on X in order
            
            grid = gridSpawner.SpawnGrid(levelData, gridParent);
        }

        public void ResetLevel() {
            ClearAllPaths();
        }
        
        private void ClearAllPaths() {
            if (grid == null) return;
            foreach (var tile in grid) {
                if (tile != null) tile.ClearPath();
            }
            paths.Clear();
            currentPath.Clear();
            isDragging = false;
        }

        private void Update() {
            if (grid == null) return;

            if (Pointer.current == null) return;

            bool pointerDownThisFrame = Pointer.current.press.wasPressedThisFrame;
            bool pointerReleasedThisFrame = Pointer.current.press.wasReleasedThisFrame;
            bool pointerIsPressed = Pointer.current.press.isPressed;
            Vector2 pointerPos = Pointer.current.position.ReadValue();

            if (pointerDownThisFrame) {
                TileView hitTile = GetTileAtPointer(pointerPos);
                if (hitTile != null) {
                    HandlePointerDown(hitTile);
                }
            } 
            else if (pointerReleasedThisFrame) {
                HandlePointerUp();
            } 
            else if (pointerIsPressed && isDragging) {
                TileView hitTile = GetTileAtPointer(pointerPos);
                if (hitTile != null) {
                    HandlePointerEnter(hitTile);
                }
            }
        }

        private TileView GetTileAtPointer(Vector2 screenPos) {
            if (Camera.main == null) return null;
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            if (hit.collider != null) {
                return hit.collider.GetComponent<TileView>();
            }
            return null;
        }

        private void HandlePointerDown(TileView tile) {
            if (tile.IsNode) {
                // If a path of this pair already exists, clear it
                if (paths.ContainsKey(tile.PairIndex)) {
                    ClearPathByPair(tile.PairIndex);
                }

                isDragging = true;
                currentPathPairIndex = tile.PairIndex;
                currentPathColor = tile.NodeColor;
                currentPath.Clear();
                currentPath.Add(tile);
            }
        }

        private void HandlePointerEnter(TileView tile) {
            if (!isDragging) return;
            if (currentPath.Count == 0) return;

            TileView lastTile = currentPath[currentPath.Count - 1];
            if (tile == lastTile) return;

            // Check if we are backtracking
            if (currentPath.Count >= 2 && currentPath[currentPath.Count - 2] == tile) {
                RemoveLastTileFromPath();
                return;
            }

            // Must be adjacent
            if (!IsAdjacent(lastTile.GridPosition, tile.GridPosition)) return;

            // Check if tile is valid to move into
            if (tile.IsNode) {
                // Must match our current pair
                if (tile.PairIndex != currentPathPairIndex) return;
                if (currentPath.Contains(tile)) return;
                
                AddTileToPath(lastTile, tile);
                
                // End dragging automatically when target node is reached
                HandlePointerUp();
                return;
            }

            // Normal tile, must be free
            if (tile.PathPairIndex != null) {
                if (tile.PathPairIndex.Value != currentPathPairIndex) {
                    // Overlapping another path. Clear the other path.
                    ClearPathByPair(tile.PathPairIndex.Value);
                } else if (currentPath.Contains(tile)) {
                    // Crossing our own path (creating a loop)
                    return; 
                }
            }

            AddTileToPath(lastTile, tile);
        }

        private void HandlePointerUp() {
            if (!isDragging) return;
            isDragging = false;
            
            if (currentPath.Count > 1) {
                TileView startTile = currentPath[0];
                TileView endTile = currentPath[currentPath.Count - 1];

                if (startTile.IsNode && endTile.IsNode && startTile.PairIndex == endTile.PairIndex) {
                    // Path is complete
                    paths[currentPathPairIndex] = new List<TileView>(currentPath);
                    if (AudioManager.Instance != null) {
                        AudioManager.Instance.PlayConnectAudio();
                    }
                    currentPath.Clear();
                    CheckWinCondition();
                    return;
                }
            }

            // Leave the path hanging on the board if it didn't connect,
            // but add it to tracking so it can be overwritten
            paths[currentPathPairIndex] = new List<TileView>(currentPath);
            currentPath.Clear();
        }

        private void AddTileToPath(TileView fromTile, TileView toTile) {
            Vector2Int dir = toTile.GridPosition - fromTile.GridPosition;
            
            TileEdge fromEdge = GetEdgeDirection(dir);
            TileEdge toEdge = GetEdgeDirection(-dir);

            fromTile.EnablePathEdge(fromEdge, currentPathColor, currentPathPairIndex);
            toTile.EnablePathEdge(toEdge, currentPathColor, currentPathPairIndex);
            
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlayEdgeProgressAudio();
            }

            currentPath.Add(toTile);
        }

        private void RemoveLastTileFromPath() {
            TileView lastTile = currentPath[currentPath.Count - 1];
            TileView prevTile = currentPath[currentPath.Count - 2];

            Vector2Int dir = lastTile.GridPosition - prevTile.GridPosition;
            
            TileEdge fromEdge = GetEdgeDirection(dir);

            prevTile.DisablePathEdge(fromEdge);
            lastTile.ClearPath(); 

            currentPath.RemoveAt(currentPath.Count - 1);
        }

        private void ClearPathByPair(int pairIndex) {
            if (paths.TryGetValue(pairIndex, out List<TileView> oldPath)) {
                foreach (var t in oldPath) {
                    t.ClearPath();
                }
                paths.Remove(pairIndex);
            }
        }

        private bool IsAdjacent(Vector2Int pos1, Vector2Int pos2) {
            return Mathf.Abs(pos1.x - pos2.x) + Mathf.Abs(pos1.y - pos2.y) == 1;
        }

        private TileEdge GetEdgeDirection(Vector2Int dir) {
            if (dir == Vector2Int.right) return TileEdge.Right;
            if (dir == Vector2Int.left) return TileEdge.Left;
            if (dir == Vector2Int.up) return TileEdge.Top;
            if (dir == Vector2Int.down) return TileEdge.Bottom;
            return TileEdge.Right;
        }

        private void CheckWinCondition() {
            if (currentLevelData == null) return;
            
            int pairsConnected = 0;
            for (int i = 0; i < currentLevelData.nodes.Count; i++) {
                var nodePair = currentLevelData.nodes[i];
                if (paths.ContainsKey(i)) {
                    var path = paths[i];
                    if (path.Count > 1) {
                        var start = path[0];
                        var end = path[path.Count - 1];
                        if ((start.GridPosition == nodePair.startNode.nodePosition && end.GridPosition == nodePair.endNode.nodePosition) ||
                            (start.GridPosition == nodePair.endNode.nodePosition && end.GridPosition == nodePair.startNode.nodePosition)) {
                            pairsConnected++;
                        }
                    }
                }
            }

            if (pairsConnected == currentLevelData.nodes.Count) {
                Debug.Log("LEVEL COMPLETED! ALL NODES CONNECTED!");
                EventBus<GameSceneEvents.LevelCompletedEvent>.Raise(new GameSceneEvents.LevelCompletedEvent(){IsWin = true});
            }
        }
    }
}
