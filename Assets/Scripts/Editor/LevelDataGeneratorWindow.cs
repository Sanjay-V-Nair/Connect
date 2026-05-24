using System.Collections.Generic;
using System.Linq;
using Connect.Systems.LevelSystem;
using UnityEditor;
using UnityEngine;

public class LevelDataGeneratorWindow : EditorWindow {
    private const string DefaultOutputFolder = "Assets/SO/Levels";

    private int startLevelNumber = 1;
    private int levelsToGenerate = 1;
    private int gridXSize = 5;
    private int gridYSize = 5;
    private int nodePairsPerLevel = 3;
    private int minEndpointDistance = 2;
    private int minPathLength = 4;
    private int attemptsPerLevel = 40;
    private bool overwriteExistingAssets = false;

    private string outputFolder = DefaultOutputFolder;
    private LevelsData levelsDataAsset;
    private int seed = 12345;
    private bool randomizeSeed = true;

    private System.Random rng;

    [MenuItem("Tools/Level Data Generator")]
    private static void OpenWindow() {
        var window = GetWindow<LevelDataGeneratorWindow>("Level Data Generator");
        window.minSize = new Vector2(420f, 460f);
    }

    private void OnGUI() {
        EditorGUILayout.LabelField("Generate LevelData Assets", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        startLevelNumber = EditorGUILayout.IntField("Start Level Number", startLevelNumber);
        levelsToGenerate = EditorGUILayout.IntField("Levels To Generate", levelsToGenerate);
        gridXSize = EditorGUILayout.IntField("Grid X Size", gridXSize);
        gridYSize = EditorGUILayout.IntField("Grid Y Size", gridYSize);
        nodePairsPerLevel = EditorGUILayout.IntField("Node Pairs Per Level", nodePairsPerLevel);
        minEndpointDistance = EditorGUILayout.IntField("Min Endpoint Distance", minEndpointDistance);
        minPathLength = EditorGUILayout.IntField("Min Path Length (cells)", minPathLength);
        attemptsPerLevel = EditorGUILayout.IntField("Attempts Per Level", attemptsPerLevel);
        overwriteExistingAssets = EditorGUILayout.Toggle("Overwrite Existing", overwriteExistingAssets);

        EditorGUILayout.Space();
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
        levelsDataAsset = (LevelsData)EditorGUILayout.ObjectField("LevelsData Asset (Optional)", levelsDataAsset, typeof(LevelsData), false);

        EditorGUILayout.Space();
        randomizeSeed = EditorGUILayout.Toggle("Randomize Seed", randomizeSeed);
        using (new EditorGUI.DisabledScope(randomizeSeed)) {
            seed = EditorGUILayout.IntField("Seed", seed);
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate Levels", GUILayout.Height(34f))) {
            GenerateLevels();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "This tool creates LevelData assets with node pairs that are generated from non-overlapping paths. " +
            "It also keeps future fields (node groups / hole pairs) initialized for upcoming mechanics.",
            MessageType.Info
        );
    }

    private void GenerateLevels() {
        if (!ValidateInput()) {
            return;
        }

        EnsureFolderExists(outputFolder);

        rng = randomizeSeed ? new System.Random() : new System.Random(seed);
        int successCount = 0;
        var createdOrUpdatedLevels = new List<LevelData>();

        for (int i = 0; i < levelsToGenerate; i++) {
            int levelNumber = startLevelNumber + i;
            string assetPath = $"{outputFolder}/Level{levelNumber}.asset";
            LevelData existingAsset = AssetDatabase.LoadAssetAtPath<LevelData>(assetPath);

            if (existingAsset != null && !overwriteExistingAssets) {
                Debug.LogWarning($"Skipped {assetPath} because it already exists and overwrite is disabled.");
                continue;
            }

            if (!TryGenerateNodePairs(out var nodePairs)) {
                Debug.LogWarning($"Failed to generate a valid layout for Level {levelNumber} after {attemptsPerLevel} attempts.");
                continue;
            }

            LevelData levelAsset = existingAsset;
            if (levelAsset == null) {
                levelAsset = CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(levelAsset, assetPath);
            }

            levelAsset.levelNumber = levelNumber;
            levelAsset.gridXSize = gridXSize;
            levelAsset.gridYSize = gridYSize;
            levelAsset.
                nodes = nodePairs;
            levelAsset.nodeGroups = new List<NodeGroup>();
            levelAsset.holePairs = new List<HolePair>();

            EditorUtility.SetDirty(levelAsset);
            createdOrUpdatedLevels.Add(levelAsset);
            successCount++;
        }

        if (levelsDataAsset != null && createdOrUpdatedLevels.Count > 0) {
            UpdateLevelsDataAsset(createdOrUpdatedLevels);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Level generation complete. Successfully generated/updated {successCount} level(s).");
    }

    private bool ValidateInput() {
        if (startLevelNumber < 1) {
            Debug.LogError("Start level number must be >= 1.");
            return false;
        }

        if (levelsToGenerate < 1) {
            Debug.LogError("Levels to generate must be >= 1.");
            return false;
        }

        if (gridXSize < 2 || gridYSize < 2) {
            Debug.LogError("Grid size must be at least 2x2.");
            return false;
        }

        if (nodePairsPerLevel < 1) {
            Debug.LogError("Node pairs per level must be >= 1.");
            return false;
        }

        int maxEndpointCells = gridXSize * gridYSize;
        if (nodePairsPerLevel * 2 > maxEndpointCells) {
            Debug.LogError("Too many node pairs for current grid size.");
            return false;
        }

        if (minPathLength < 2) {
            Debug.LogError("Minimum path length must be at least 2 cells.");
            return false;
        }

        if (attemptsPerLevel < 1) {
            Debug.LogError("Attempts per level must be >= 1.");
            return false;
        }

        if (!outputFolder.StartsWith("Assets")) {
            Debug.LogError("Output folder must start with 'Assets'.");
            return false;
        }

        return true;
    }

    private bool TryGenerateNodePairs(out List<NodePair> resultPairs) {
        resultPairs = new List<NodePair>();

        for (int levelAttempt = 0; levelAttempt < attemptsPerLevel; levelAttempt++) {
            var occupied = new bool[gridXSize, gridYSize];
            var candidatePairs = new List<NodePair>();
            bool levelValid = true;

            for (int pairIndex = 0; pairIndex < nodePairsPerLevel; pairIndex++) {
                if (!TryGenerateSinglePair(occupied, out var pair)) {
                    levelValid = false;
                    break;
                }

                candidatePairs.Add(pair);
            }

            if (levelValid && candidatePairs.Count == nodePairsPerLevel) {
                resultPairs = candidatePairs;
                return true;
            }
        }

        return false;
    }

    private bool TryGenerateSinglePair(bool[,] occupied, out NodePair pair) {
        pair = default;
        int pairAttempts = Mathf.Max(20, gridXSize * gridYSize * 2);

        for (int attempt = 0; attempt < pairAttempts; attempt++) {
            var freeCells = GetFreeCells(occupied);
            if (freeCells.Count < 2) {
                return false;
            }

            Vector2Int start = freeCells[rng.Next(freeCells.Count)];
            var validEnds = freeCells.Where(cell => cell != start && ManhattanDistance(cell, start) >= minEndpointDistance).ToList();
            if (validEnds.Count == 0) {
                continue;
            }

            Vector2Int end = validEnds[rng.Next(validEnds.Count)];
            if (!TryFindPath(start, end, occupied, out var path)) {
                continue;
            }

            if (path.Count < minPathLength) {
                continue;
            }

            foreach (var pos in path) {
                occupied[pos.x, pos.y] = true;
            }

            Color pairColor = RandomBrightColor();
            pair = new NodePair {
                startNode = new Node { nodePosition = start, nodeColor = pairColor },
                endNode = new Node { nodePosition = end, nodeColor = pairColor }
            };
            return true;
        }

        return false;
    }

    private bool TryFindPath(Vector2Int start, Vector2Int end, bool[,] occupied, out List<Vector2Int> path) {
        path = new List<Vector2Int>();
        var visited = new HashSet<Vector2Int>();
        return DfsFindPath(start, end, occupied, path, visited);
    }

    private bool DfsFindPath(
        Vector2Int current,
        Vector2Int end,
        bool[,] occupied,
        List<Vector2Int> path,
        HashSet<Vector2Int> visited
    ) {
        visited.Add(current);
        path.Add(current);

        if (current == end) {
            return true;
        }

        var neighbors = GetNeighbors(current)
            .Where(next => !visited.Contains(next) && (!occupied[next.x, next.y] || next == end))
            .OrderBy(_ => rng.Next())
            .ThenBy(next => ManhattanDistance(next, end))
            .ToList();

        foreach (var next in neighbors) {
            if (DfsFindPath(next, end, occupied, path, visited)) {
                return true;
            }
        }

        path.RemoveAt(path.Count - 1);
        visited.Remove(current);
        return false;
    }

    private List<Vector2Int> GetFreeCells(bool[,] occupied) {
        var result = new List<Vector2Int>();
        for (int x = 0; x < gridXSize; x++) {
            for (int y = 0; y < gridYSize; y++) {
                if (!occupied[x, y]) {
                    result.Add(new Vector2Int(x, y));
                }
            }
        }
        return result;
    }

    private IEnumerable<Vector2Int> GetNeighbors(Vector2Int pos) {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var dir in dirs) {
            var next = pos + dir;
            if (next.x >= 0 && next.x < gridXSize && next.y >= 0 && next.y < gridYSize) {
                yield return next;
            }
        }
    }

    private int ManhattanDistance(Vector2Int a, Vector2Int b) {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private Color RandomBrightColor() {
        float hue = (float)rng.NextDouble();
        float saturation = 0.7f + ((float)rng.NextDouble() * 0.3f);
        float value = 0.8f + ((float)rng.NextDouble() * 0.2f);
        return Color.HSVToRGB(hue, saturation, value);
    }

    private void UpdateLevelsDataAsset(List<LevelData> generatedLevels) {
        if (levelsDataAsset.levels == null) {
            levelsDataAsset.levels = new List<LevelData>();
        }

        foreach (var level in generatedLevels) {
            int existingIndex = levelsDataAsset.levels.FindIndex(l => l != null && l.levelNumber == level.levelNumber);
            if (existingIndex >= 0) {
                levelsDataAsset.levels[existingIndex] = level;
            } else {
                levelsDataAsset.levels.Add(level);
            }
        }

        levelsDataAsset.levels = levelsDataAsset.levels
            .Where(l => l != null)
            .OrderBy(l => l.levelNumber)
            .ToList();

        EditorUtility.SetDirty(levelsDataAsset);
    }

    private static void EnsureFolderExists(string folderPath) {
        if (AssetDatabase.IsValidFolder(folderPath)) {
            return;
        }

        string[] parts = folderPath.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++) {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next)) {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }
    }
}
