using Connect.Core;
using Connect.Models;
using Connect.Systems.EventBus;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Connect.Views {
    public class SettingsPopupView : PopupDraw<Bundle> {
        
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button connectButton;
        
        protected override void Render(Bundle context) {
            gameObject.SetActive(true);
            SetButtons();
        }

        private void SetButtons() {
            _closeButton.onClick.RemoveAllListeners();
            _closeButton.onClick.AddListener(() => Reset());
            connectButton.onClick.RemoveAllListeners();
            connectButton.onClick.AddListener(OnConnectButtonClicked);
        }

        private void OnConnectButtonClicked() {
            Reset();
            EventBus<DashboardEvents.DashboardTabEvent>.Raise(new DashboardEvents.DashboardTabEvent()
                { TabType = DashboardTabType.Credits });
        }

        protected override bool CanDraw(Bundle context) {
            return true;
        }

        public override void Reset() {
            transform.DOScale(Vector3.zero, 0.5f)
                .OnComplete(() => gameObject.SetActive(false));
        }
    }
}