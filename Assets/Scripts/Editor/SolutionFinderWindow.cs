using UnityEngine;
using UnityEditor;
using Connect.Systems.LevelSystem;

public class SolutionFinderWindow : EditorWindow {
    private LevelData levelData;
    private float cellSize = 40f;
    
    [MenuItem("Tools/Solution Finder")]
    private static void OpenWindow() {
        var window = GetWindow<SolutionFinderWindow>("Solution Finder");
        window.minSize = new Vector2(400f, 400f);
    }

    private void OnGUI() {
        EditorGUILayout.LabelField("Solution Finder", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        levelData = (LevelData)EditorGUILayout.ObjectField("Level Data", levelData, typeof(LevelData), false);

        if (levelData == null) {
            EditorGUILayout.HelpBox("Assign a LevelData to view its solution.", MessageType.Info);
            return;
        }

        if (levelData.solutions == null || levelData.solutions.Count == 0) {
            EditorGUILayout.HelpBox("This level does not have saved solutions. Regenerate it to view solutions.", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space();
        
        // Add a flexible space before drawing the grid to keep it centered if desired, or just draw it.
        Rect drawArea = GUILayoutUtility.GetRect(levelData.gridXSize * cellSize, levelData.gridYSize * cellSize);
        
        if (Event.current.type == EventType.Repaint) {
            // Draw the grid cells
            for (int y = 0; y < levelData.gridYSize; y++) {
                for (int x = 0; x < levelData.gridXSize; x++) {
                    int drawY = levelData.gridYSize - 1 - y;
                    Rect cellRect = new Rect(drawArea.x + x * cellSize, drawArea.y + drawY * cellSize, cellSize, cellSize);
                    
                    Vector2Int pos = new Vector2Int(x, y);
                    
                    // Default tile color
                    Color cellColor = new Color(0.3f, 0.3f, 0.3f); 
                    
                    if (levelData.emptySpaces != null && levelData.emptySpaces.Contains(pos)) {
                        cellColor = Color.black; 
                    } else if (levelData.holePairs != null && levelData.holePairs.Exists(h => h.entryPosition == pos || h.exitPosition == pos)) {
                        cellColor = new Color(0.1f, 0.1f, 0.1f); 
                    }
                    
                    EditorGUI.DrawRect(cellRect, cellColor);
                    
                    Handles.color = Color.black;
                    Handles.DrawWireCube(cellRect.center, cellRect.size);

                    // Draw Hole identifier
                    if (levelData.holePairs != null) {
                        for (int h = 0; h < levelData.holePairs.Count; h++) {
                            if (levelData.holePairs[h].entryPosition == pos || levelData.holePairs[h].exitPosition == pos) {
                                GUIStyle style = new GUIStyle();
                                style.normal.textColor = Color.white;
                                style.alignment = TextAnchor.MiddleCenter;
                                GUI.Label(cellRect, $"H{h+1}", style);
                            }
                        }
                    }
                }
            }
            
            // Draw lines for paths
            foreach (var solution in levelData.solutions) {
                if (solution.pathCells == null || solution.pathCells.Count < 2) continue;
                
                Color pathColor = Color.white;
                if (levelData.nodes != null) {
                    foreach (var nodePair in levelData.nodes) {
                        if (nodePair.startNode.nodePosition == solution.pathCells[0] || nodePair.endNode.nodePosition == solution.pathCells[0]) {
                            pathColor = nodePair.startNode.nodeColor; 
                            break;
                        }
                    }
                }
                
                Color currentColor = pathColor;
                for (int i = 0; i < solution.pathCells.Count - 1; i++) {
                    var p1 = solution.pathCells[i];
                    var p2 = solution.pathCells[i + 1];

                    if (levelData.mutants != null && levelData.mutants.Exists(m => m.position == p1)) {
                        currentColor = ColorConstants.GetMutatedColor(currentColor);
                    }

                    Handles.color = currentColor;

                    if (Mathf.Abs(p1.x - p2.x) + Mathf.Abs(p1.y - p2.y) <= 1) {
                        int drawY1 = levelData.gridYSize - 1 - p1.y;
                        int drawY2 = levelData.gridYSize - 1 - p2.y;
                        Vector2 center1 = new Vector2(drawArea.x + p1.x * cellSize + cellSize / 2, drawArea.y + drawY1 * cellSize + cellSize / 2);
                        Vector2 center2 = new Vector2(drawArea.x + p2.x * cellSize + cellSize / 2, drawArea.y + drawY2 * cellSize + cellSize / 2);
                        Handles.DrawAAPolyLine(8f, center1, center2);
                    }
                }
            }
            
            // Draw Nodes
            if (levelData.nodes != null) {
                foreach (var nodePair in levelData.nodes) {
                    DrawNode(drawArea, nodePair.startNode);
                    DrawNode(drawArea, nodePair.endNode);
                }
            }
            
            // Draw Mutants
            if (levelData.mutants != null) {
                foreach (var mutant in levelData.mutants) {
                    int drawY = levelData.gridYSize - 1 - mutant.position.y;
                    Rect mRect = new Rect(drawArea.x + mutant.position.x * cellSize + cellSize * 0.25f, drawArea.y + drawY * cellSize + cellSize * 0.25f, cellSize * 0.5f, cellSize * 0.5f);
                    EditorGUI.DrawRect(mRect, Color.white);
                    
                    GUIStyle mStyle = new GUIStyle();
                    mStyle.normal.textColor = Color.black;
                    mStyle.fontStyle = FontStyle.Bold;
                    mStyle.alignment = TextAnchor.MiddleCenter;
                    GUI.Label(mRect, "M", mStyle);
                }
            }
        }
    }
    
    private void DrawNode(Rect drawArea, Node node) {
        int drawY = levelData.gridYSize - 1 - node.nodePosition.y;
        Rect cellRect = new Rect(drawArea.x + node.nodePosition.x * cellSize + cellSize * 0.15f, drawArea.y + drawY * cellSize + cellSize * 0.15f, cellSize * 0.7f, cellSize * 0.7f);
        
        // Draw colored circle
        Handles.color = node.nodeColor;
        Handles.DrawSolidDisc(cellRect.center, Vector3.forward, cellRect.width / 2f);
    }
}
