using System;
using Connect.Core;
using Connect.Models;
using Connect.Systems.EventBus;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Connect.Views {
    
    [Serializable]
    public enum DashboardTabType {
        Inventory,
        Skills,
        Quests,
        Map,
        Home,
        Credits,
    }
    
    public class DashboardSingleTabView :DrawView<Bundle> {
        
        [SerializeField] private DashboardTabType _tabType;
        [SerializeField] private Image _tabIcon;
        [SerializeField] private TMP_Text _tabName;
        [SerializeField] private ButtonView tabButton;

        private bool isSelected = false;
        
        protected override void Render(Bundle context) {
            RenderTab();
            SetButtons();
        }

        private void SetButtons() {
            tabButton.Draw(new ButtonViewData() {
                onClick = OnTabClick
            });
        }

        private void OnTabClick() {
            if (isSelected) return;
            EventBus<DashboardEvents.DashboardTabEvent>.Raise(new DashboardEvents.DashboardTabEvent()
                { TabType = _tabType });
            _tabIcon.transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.2f).SetEase(Ease.OutBack);
            isSelected = true;
        }

        private void RenderTab() {
            switch (_tabType) {
                case DashboardTabType.Home:
                    break;
            }
        }

        protected override bool CanDraw(Bundle context) {
            return true;
        }

        public override void Reset() {
            _tabIcon.transform.DOScale(Vector3.one, 0.5f)
                .OnComplete(() => _tabIcon.transform.localScale = Vector3.one);
            isSelected = false;
        }

        public void SetState(bool b) {
            if (isSelected) return;
            _tabIcon.transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.5f).SetEase(Ease.OutBack);
            isSelected = b;
        }
    }
}