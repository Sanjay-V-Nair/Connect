using DG.Tweening;
using UnityEngine;

namespace Connect.Core {
    public abstract class PopupDraw<T> : DrawView<T> where T : class {
        
        protected bool _isAnimating = false;
        
        private void OnEnable() {
            transform.localScale = Vector3.zero;
            DoPopupAnimation();
        }

        private void DoPopupAnimation() {
            _isAnimating = true;
            transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack)
                .OnComplete(() => _isAnimating = true);
        }
        
        public override void Reset() {
            _isAnimating = true;
            transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InOutBack)
                .OnComplete(() => {
                    gameObject.SetActive(false);
                    _isAnimating = false;
                });
        }
        
    }
}