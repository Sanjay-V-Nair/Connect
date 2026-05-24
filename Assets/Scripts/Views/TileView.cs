using System;
using System.Collections.Generic;
using System.Linq;
using Connect.Core;
using Connect.Systems.LevelSystem;
using DG.Tweening;
using TMPro;
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
        public string tileIndex; // Optional: can be used for debugging or specific tile identification
    }
    
    [Serializable]
    public enum TileEdge {
        Left,
        Right,
        Top,
        Bottom,
    }
    
    [Serializable]
    public enum TileState {
        Default,
        Path,
        Hole,
        Complete,
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
        
        [Header("3D Tile")]
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private SpriteRenderer nodeImage;
        [SerializeField] private Transform tileTransform;
        [SerializeField] private GameObject fireWork;
        [SerializeField] private ParticleSystem fireWorkParticle;
        [SerializeField] private MeshRenderer upperTile;
        [SerializeField] private MeshRenderer lowerTile;
        [SerializeField] private Material whiteMaterial;
        [SerializeField] private Material blackMaterial;
        [SerializeField] private GameObject blackHole;
        
        [SerializeField] private List<TileEdgeData> edgeDataArray;
        
        public Vector2Int GridPosition { get; private set; }
        public Color? PathColor { get; private set; }
        public int? PathPairIndex { get; private set; }
        public bool IsNode { get; private set; }
        public Color NodeColor { get; private set; }
        public int PairIndex { get; private set; }

        private Color defaultColor;
        private float defaultYValue = -0.3f;

        private void OnEnable() {
            defaultColor = meshRenderer.material.color;
        }

        protected override void Render(TileData context) {
            var nodeData = context.nodeData;
            
            GridPosition = nodeData.nodePosition;
            IsNode = context.isNode;
            NodeColor = nodeData.nodeColor;
            PairIndex = context.pairIndex;
            
            if (context.isNode) {
                if (nodeTile != null) nodeTile.SetActive(true);
                //if (nodeImage != null) SetNodeColor(nodeData.nodeColor);
                SetNodeColor(context.nodeData.nodeColor);
            } else {
                if (nodeTile != null) nodeTile.SetActive(false);
                var tilePos = tileTransform.localPosition;
                tileTransform.localPosition = new Vector3(tilePos.x, tilePos.y + defaultYValue, tilePos.z);
            }

            if (context.isHole) {
                SetHole();
            }
        }
        
        public void RenderState(TileState state, Color color) {
            switch (state) {
                case TileState.Path:
                    SetPath(color);
                    break;
                case TileState.Complete:
                    SetPathComplete(color);
                    break;
            }
        }

        private void SetEdges(TileEdge edge, bool isActive = true) {
            switch (edge) {
                case TileEdge.Left: leftEdge.SetActive(isActive); break;
                case TileEdge.Right: rightEdge.SetActive(isActive); break;
                case TileEdge.Top: topEdge.SetActive(isActive); break;
                case TileEdge.Bottom: bottomEdge.SetActive(isActive); break;
            }
        }

        private void SetNodeColor(Color nodeColor) {
            nodeColor.a = 1f; // Force alpha to 1 in case it was left at 0 in the Inspector
            //meshRenderer.material.color = nodeColor;
            nodeImage.color = nodeColor;
        }
        
        private void SetHole(bool isActive = true) {
            blackHole.SetActive(isActive);
        }

        private void SetPathComplete(Color color) {
            var col = fireWorkParticle.colorOverLifetime;
            var grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { 
                    new (color, 0.0f), 
                    new (color, 1.0f) 
                },
                new GradientAlphaKey[] { 
                    new (1f, 0.0f), 
                    new (1f, 1.0f) 
                }
            );
            col.color = grad;
            fireWork.SetActive(true);
        }

        private void SetPath(Color color) {
            //meshRenderer.material.color = color;
            var tilePos = tileTransform.localPosition;
            tileTransform.DOLocalMove(new Vector3(tilePos.x, 0f , tilePos.z), 0.3f).SetEase(Ease.OutBack);
        }

        public void EnablePathEdge(TileEdge edge, Color color, int pairIndex) {
            PathColor = color;
            PathPairIndex = pairIndex;
            SetEdges(edge);
            var edgeData = GetEdgeData(edge);
            if (edgeData != null && edgeData.edgeObject != null) {
                edgeData.edgeObject.SetActive(true);
                var sr = edgeData.edgeObject.GetComponent<SpriteRenderer>();
                if (sr != null) {
                    color.a = 1f;
                    sr.color = color;
                }
            }
            nodeImage.color = color;
            nodeTile.SetActive(true);
        }

        public void DisablePathEdge(TileEdge edge) {
            if(!IsNode) nodeTile.SetActive(false);
            SetEdges(edge, false);
            meshRenderer.material.color = defaultColor;
            var tilePos = tileTransform.localPosition;
            tileTransform.DOLocalMove(new Vector3(tilePos.x, defaultYValue , tilePos.z), 0.3f).SetEase(Ease.OutBack);
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