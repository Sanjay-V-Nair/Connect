using Connect.Core;
using Connect.Models;
using DG.Tweening;
using UnityEngine;

namespace Connect.Views.Board {
    public class GameBoardView : DrawView<Bundle> {
        
        [Header("Levitation Settings")]
        [SerializeField] private float floatDistance = 0.5f;
        [SerializeField] private float floatDuration = 2f;
        
        private void Start() {
            Render(null);
        }

        protected override void Render(Bundle context) {
            transform.DOLocalMoveY(transform.position.y + floatDistance, floatDuration)
                .SetEase(Ease.InOutQuad)
                .SetLoops(-1, LoopType.Yoyo);
        }

        protected override bool CanDraw(Bundle context) {
            return true;
        }

        public override void Reset() {
            
        }
    }
}