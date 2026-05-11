using Connect.Views;
using UnityEngine;

namespace Connect.Systems.LevelSystem {
    public class GridSpawner : MonoBehaviour {
        [SerializeField] private TileView tilePrefab;
        [SerializeField] private float tileSize = 1f;

        /// <summary>
        /// Spawns the grid based on the provided LevelData into the specified parent transform.
        /// Starts from lower bottom (0,0) to upper right (gridSize-1, gridSize-1).
        /// </summary>
        public TileView[,] SpawnGrid(LevelData levelData, Transform parentTransform) {
            if (levelData == null || levelData.gridXSize <= 0 || levelData.gridYSize <= 0) {
                Debug.LogWarning("Invalid LevelData or grid size.");
                return null;
            }
            
            TileView[,] grid = new TileView[levelData.gridXSize, levelData.gridYSize];
            
            for (var y = 0; y < levelData.gridYSize; y++) {
                for (var x = 0; x < levelData.gridXSize; x++) {
                    // Position calculated from lower bottom to upper right
                    var spawnPosition = new Vector3(x * tileSize, y * tileSize, 0);
                    
                    var spawnedTile = Instantiate(tilePrefab, parentTransform);
                    spawnedTile.transform.localPosition = spawnPosition;
                    spawnedTile.name = $"Tile_{x}_{y}";

                    var pos = new Vector2Int(x, y);
                    var isNode = false;
                    var currentNode = new Node { nodePosition = pos };
                    int pairIndex = -1;
                    
                    if (levelData.nodes != null) {
                        for (int i = 0; i < levelData.nodes.Count; i++) {
                            var nodePair = levelData.nodes[i];
                            if (nodePair.startNode.nodePosition == pos) {
                                isNode = true;
                                currentNode = nodePair.startNode;
                                pairIndex = i;
                                break;
                            }
                            if (nodePair.endNode.nodePosition == pos) {
                                isNode = true;
                                currentNode = nodePair.endNode;
                                pairIndex = i;
                                break;
                            }
                        }
                    }

                    // Initialize the TileData for drawing
                    var tileData = new TileData {
                        nodeData = currentNode,
                        isNode = isNode,
                        pairIndex = pairIndex,
                        gridXSize = levelData.gridXSize,
                        gridYSize = levelData.gridYSize
                    };

                    spawnedTile.Draw(tileData);
                    grid[x, y] = spawnedTile;
                }
            }
            return grid;
        }
        
        /// <summary>
        /// Clears all spawned tiles from the parent transform.
        /// </summary>
        public void ClearGrid(Transform parentTransform) {
            foreach (Transform child in parentTransform) {
                Destroy(child.gameObject);
            }
        }
    }
}
