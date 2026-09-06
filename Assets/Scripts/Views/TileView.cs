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
        public bool isMutant;
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
        [SerializeField] private GameObject mutantObject;
        
        [SerializeField] private List<TileEdgeData> edgeDataArray;
        
        public Vector2Int GridPosition { get; private set; }
        public Color? PathColor { get; private set; }
        public int? PathPairIndex { get; private set; }
        public bool IsNode { get; private set; }
        public bool IsHole { get; private set; }
        public bool IsMutant { get; private set; }
        public Color NodeColor { get; private set; }
        public int PairIndex { get; private set; }

        private Color defaultColor;
        private float defaultYValue = -0.3f;

        // State Tracking
        private List<TileEdge> activeEdges = new List<TileEdge>();
        private TileState currentState = TileState.Default;
        private float currentTargetY = 0f;

        private void OnEnable() {
            defaultColor = meshRenderer.material.color;
        }

        protected override void Render(TileData context) {
            var nodeData = context.nodeData;
            
            GridPosition = nodeData.nodePosition;
            IsNode = context.isNode;
            IsHole = context.isHole;
            IsMutant = context.isMutant;
            NodeColor = nodeData.nodeColor;
            PairIndex = context.pairIndex;
            
            // Set initial Y position immediately without animating
            float initialY = IsNode ? 0f : defaultYValue;
            var tilePos = tileTransform.localPosition;
            tileTransform.localPosition = new Vector3(tilePos.x, initialY, tilePos.z);
            currentTargetY = initialY;

            UpdateVisuals(immediate: true);
        }

        public void SetState(TileState state, Color? color = null) {
            currentState = state;
            if (color.HasValue) PathColor = color;
            UpdateVisuals();
        }

        public void AddEdge(TileEdge edge, Color color, int pairIndex) {
            if (!activeEdges.Contains(edge)) {
                activeEdges.Add(edge);
            }
            PathColor = color;
            PathPairIndex = pairIndex;
            currentState = TileState.Path;
            UpdateVisuals();
        }

        public void RemoveEdge(TileEdge edge) {
            activeEdges.Remove(edge);
            if (activeEdges.Count == 0) {
                currentState = TileState.Default;
            } else {
                currentState = TileState.Path;
            }
            UpdateVisuals();
        }

        public void SetPathData(Color color, int pairIndex) {
            PathColor = color;
            PathPairIndex = pairIndex;
            currentState = TileState.Path;
            UpdateVisuals();
        }

        public void ClearPath() {
            activeEdges.Clear();
            PathColor = null;
            PathPairIndex = null;
            currentState = TileState.Default;
            UpdateVisuals();
        }

        private void UpdateVisuals(bool immediate = false) {
            // 1. Y-Position
            bool shouldBeLifted = IsNode || currentState == TileState.Path || currentState == TileState.Complete;
            float targetY = shouldBeLifted ? 0f : defaultYValue;
            
            if (Mathf.Abs(currentTargetY - targetY) > 0.001f) {
                currentTargetY = targetY;
                if (immediate) {
                    var tilePos = tileTransform.localPosition;
                    tileTransform.localPosition = new Vector3(tilePos.x, targetY, tilePos.z);
                } else {
                    tileTransform.DOLocalMoveY(targetY, 0.3f).SetEase(Ease.OutBack);
                }
            }

            // 2. Node Dot
            if (IsNode) {
                if (nodeTile != null) nodeTile.SetActive(true);
                SetNodeColor(NodeColor);
            } else if (currentState == TileState.Path || currentState == TileState.Complete) {
                if (nodeTile != null) nodeTile.SetActive(true);
                if (PathColor.HasValue) SetNodeColor(PathColor.Value);
            } else {
                if (nodeTile != null) nodeTile.SetActive(false);
            }

            // 3. Edges
            if (edgeDataArray != null) {
                foreach (var edgeData in edgeDataArray) {
                    if (edgeData.edgeObject == null) continue;
                    
                    bool isEdgeActive = activeEdges.Contains(edgeData.edge);
                    edgeData.edgeObject.SetActive(isEdgeActive);
                    
                    if (isEdgeActive && PathColor.HasValue) {
                        var sr = edgeData.edgeObject.GetComponent<SpriteRenderer>();
                        if (sr != null) {
                            var c = PathColor.Value;
                            c.a = 1f;
                            sr.color = c;
                        }
                    }
                }
            }

            // 4. Complete State Firework
            if (currentState == TileState.Complete && PathColor.HasValue) {
                SetPathComplete(PathColor.Value);
            } else {
                if (fireWork != null) fireWork.SetActive(false);
            }

            // 5. Hole & Mutant visual overrides
            if (IsHole) {
                if (blackHole != null) blackHole.SetActive(true);
            } else {
                if (blackHole != null) blackHole.SetActive(false);
            }

            if (IsMutant) {
                if (mutantObject != null) mutantObject.SetActive(true);
                else if (meshRenderer != null) meshRenderer.material.color = Color.gray;
            } else {
                if (mutantObject != null) mutantObject.SetActive(false);
                else if (meshRenderer != null) meshRenderer.material.color = defaultColor;
            }
        }

        private void SetNodeColor(Color nodeColor) {
            nodeColor.a = 1f; // Force alpha to 1 in case it was left at 0 in the Inspector
            nodeImage.color = nodeColor;
        }

        private void SetPathComplete(Color color) {
            if (fireWorkParticle == null || fireWork == null) return;
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

        protected override bool CanDraw(TileData context) {
            return true;
        }

        public override void Reset() {
            activeEdges.Clear();
            PathColor = null;
            PathPairIndex = null;
            currentState = TileState.Default;
            UpdateVisuals(immediate: true);
        }
    }
}