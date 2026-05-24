using Connect.Core;
using Connect.Models;
using Connect.Systems.EventBus;
using Connect.Views.Credits;
using Connect.Views.Inventory;
using UnityEngine;

namespace Connect.Views {
    public class DashboardView : DrawView<Bundle> {

        [SerializeField] private DashboardTabsView dashboardTabsView;
        [SerializeField] private ButtonView playButtonView;
        [SerializeField] private CreditsPageView creditsPageView;
        [SerializeField] private InventoryPageView inventoryPageView;
        [SerializeField] private SettingsPopupView settingsPopupView;
        [SerializeField] private ButtonView settingsButton;
        
        private DashboardTabType _currentTab = DashboardTabType.Home;

        private void OnEnable() {
            EventBus<DashboardEvents.DashboardTabEvent>.Subscribe(OnDashboardTabEvent);
            Draw(null);
        }
        
        private void OnDisable() {
            EventBus<DashboardEvents.DashboardTabEvent>.Unsubscribe(OnDashboardTabEvent);
        }

        private void OnDashboardTabEvent(DashboardEvents.DashboardTabEvent eventData) {
            if (!isActiveAndEnabled) return;
            if (eventData.TabType != _currentTab) {
                OpenTab(eventData.TabType);
                _currentTab = eventData.TabType;
            }   
        }

        protected override void Render(Bundle context) {
            IsolatedDraw(dashboardTabsView, context);
            RenderButton();
        }

        private void RenderButton() {
            playButtonView.Draw(new ButtonViewData() {
                onClick = OnPlayButtonClick,
            });
            settingsButton.Draw(new ButtonViewData() {
                onClick = () => settingsPopupView.Draw(null),
            });
        }

        private void OnPlayButtonClick() {
            var gameSceneName = SceneType.SandboxScene.GetSceneName();
            GameManager.Instance.LoadSceneAsync(gameSceneName);
        }

        private void OpenTab(DashboardTabType tabType) {
            CloseDashboardViews();
            // Logic to open the specified tab
            dashboardTabsView.OpenTab(tabType);
            switch (tabType) {
                case DashboardTabType.Home:
                    break;
                case DashboardTabType.Credits:
                     creditsPageView.Draw(null);
                     break;
                case DashboardTabType.Inventory:
                    inventoryPageView.Draw(null);
                    break;
            }
        }

        private void CloseDashboardViews() {
            IsolatedReset(creditsPageView);
            IsolatedReset(inventoryPageView);
        }

        protected override bool CanDraw(Bundle context) {
            return true;
        }

        public override void Reset() {
            gameObject.SetActive(false);
        }
    }
}