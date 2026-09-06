using System;
using System.Collections.Generic;
using UnityEngine;

namespace Connect.Systems.LevelSystem {
    
    [CreateAssetMenu(fileName = "Level",menuName = "SO/Level")]
    public class LevelData : ScriptableObject {
        public int levelNumber;
        public int gridXSize; // Width of the grid
        public int gridYSize; // Height of the grid
        public List<NodePair> nodes;
        public List<NodeGroup> nodeGroups;
        public List<HolePair> holePairs;
        public List<Mutant> mutants;
        public List<Vector2Int> emptySpaces;
        public List<SolutionPath> solutions;
    }

    [Serializable]
    public struct NodePair {
        public Node startNode;
        public Node endNode;
    }

    [Serializable]
    public struct Node {
        public Vector2Int nodePosition;
        public Color nodeColor;
    }

    [Serializable]
    public struct Hole {
        public Vector2Int nodePosition;
    }

    [Serializable]
    public struct NodeGroup {
        public Color groupColor;
        public List<Node> nodes;
    }

    [Serializable]
    public struct HolePair {
        public Vector2Int entryPosition;
        public Vector2Int exitPosition;
    }

    [Serializable]
    public struct Mutant {
        public Vector2Int position;
    }
    
    [Serializable]
    public struct SolutionPath {
        public List<Vector2Int> pathCells;
    }

}