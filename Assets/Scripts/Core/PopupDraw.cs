using DG.Tweening;
using UnityEngine;

namespace Connect.Core {
    public abstract class PopupDraw<T> : DrawView<T> where T : class {
        private void OnEnable() {
            transform.localScale = Vector3.zero;
            DoPopupAnimation();
        }

        private void DoPopupAnimation() {
            transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        }
        
    }
}