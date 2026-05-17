using System;
using System.Collections.Generic;
using System.Linq;
using Connect.Core;
using Connect.Systems.LevelSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Connect.Views {

    [Serializable]
    public class TileData {
        public Node nodeData;
        public bool isNode;
        public bool isHole;
        public int pairIndex;
        public int gridXSize;
        public int gridYSize;
    }
    
    [Serializable]
    public enum TileEdge {
        Left,
        Right,
        Top,
        Bottom,
    }
    
    [Serializable]
    public class TileEdgeData {
        public TileEdge edge;
        public GameObject edgeObject;
    }
    
    public class TileView : DrawView<TileData> {

        [SerializeField] private GameObject nodeTile;
        [SerializeField] private GameObject rightEdge;
        [SerializeField] private GameObject leftEdge;
        [SerializeField] private GameObject topEdge;
        [SerializeField] private GameObject bottomEdge;
        [SerializeField] private GameObject holeObject;
        [SerializeField] private SpriteRenderer nodeImage;
        
        [SerializeField] private List<TileEdgeData> edgeDataArray;
        
        public Vector2Int GridPosition { get; private set; }
        public Color? PathColor { get; private set; }
        public int? PathPairIndex { get; private set; }
        public bool IsNode { get; private set; }
        public Color NodeColor { get; private set; }
        public int PairIndex { get; private set; }
        
        protected override void Render(TileData context) {
            var nodeData = context.nodeData;
            
            GridPosition = nodeData.nodePosition;
            IsNode = context.isNode;
            NodeColor = nodeData.nodeColor;
            PairIndex = context.pairIndex;
            
            if (context.isNode) {
                if (nodeTile != null) nodeTile.SetActive(true);
                if (nodeImage != null) SetNodeColor(nodeData.nodeColor);
            } else {
                if (nodeTile != null) nodeTile.SetActive(false);
            }
            
            if(EdgeResolver.HasLeftEdge(nodeData.nodePosition)) SetEdges(TileEdge.Left);
            if(EdgeResolver.HasRightEdge(nodeData.nodePosition, context.gridXSize)) SetEdges(TileEdge.Right);
            if(EdgeResolver.HasTopEdge(nodeData.nodePosition, context.gridYSize)) SetEdges(TileEdge.Top);
            if(EdgeResolver.HasBottomEdge(nodeData.nodePosition)) SetEdges(TileEdge.Bottom);
        }

        private void SetEdges(TileEdge edge) {
            switch (edge) {
                case TileEdge.Left: leftEdge.SetActive(true); break;
                case TileEdge.Right: rightEdge.SetActive(true); break;
                case TileEdge.Top: topEdge.SetActive(true); break;
                case TileEdge.Bottom: bottomEdge.SetActive(true); break;
            }
        }

        private void SetNodeColor(Color nodeColor) {
            nodeColor.a = 1f; // Force alpha to 1 in case it was left at 0 in the Inspector
            nodeImage.color = nodeColor;
        }

        public void EnablePathEdge(TileEdge edge, Color color, int pairIndex) {
            PathColor = color;
            PathPairIndex = pairIndex;
            var edgeData = GetEdgeData(edge);
            if (edgeData != null && edgeData.edgeObject != null) {
                edgeData.edgeObject.SetActive(true);
                var sr = edgeData.edgeObject.GetComponent<SpriteRenderer>();
                if (sr != null) {
                    color.a = 1f;
                    sr.color = color;
                }
            }
        }

        public void DisablePathEdge(TileEdge edge) {
            var edgeData = GetEdgeData(edge);
            if (edgeData != null && edgeData.edgeObject != null) {
                edgeData.edgeObject.SetActive(false);
            }
        }

        public void ClearPath() {
            PathColor = null;
            PathPairIndex = null;
            if (edgeDataArray != null) {
                foreach (var edgeData in edgeDataArray) {
                    if (edgeData.edgeObject != null) {
                        edgeData.edgeObject.SetActive(false);
                    }
                }
            }
        }

        private TileEdgeData GetEdgeData(TileEdge edge) {
            return edgeDataArray.FirstOrDefault(data => data.edge == edge);
        }

        protected override bool CanDraw(TileData context) {
            return true;
        }

        public override void Reset() {
            nodeTile.SetActive(false);
            leftEdge.SetActive(false);
            rightEdge.SetActive(false);
            topEdge.SetActive(false);
            bottomEdge.SetActive(false);
        }
    }
}