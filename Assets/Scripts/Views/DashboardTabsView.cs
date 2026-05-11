using System;
using System.Collections.Generic;
using System.Linq;
using Connect.Core;
using Connect.Models;
using UnityEngine;

namespace Connect.Views {
    
    [Serializable]
    public class DashboardTabsViewData {
        public DashboardTabType TabType;
        public DashboardSingleTabView TabView;
    }
    
    public class DashboardTabsView : DrawView<Bundle> {

        [SerializeField] private List<DashboardTabsViewData> _tabsData;
        
        protected override void Render(Bundle context) {
            foreach (var tab in _tabsData) {
                IsolatedDraw(tab.TabView, context);
            }
        }

        protected override bool CanDraw(Bundle context) {
            return true;
        }

        public override void Reset() {
            gameObject.SetActive(false);
        }

        public void OpenTab(DashboardTabType tabType) {
            var tabData = GetTabData(tabType);
            tabData.TabView.Draw(null);
            tabData.TabView.SetState(true);
            HideOtherTabs(tabType);
        }

        private void HideOtherTabs(DashboardTabType tabType) {
            foreach (var tab in _tabsData) {
                if(tab.TabType == tabType) continue;
                IsolatedReset(tab.TabView);
            }
        }

        private DashboardTabsViewData GetTabData(DashboardTabType tabType) {
            return _tabsData.FirstOrDefault(tab => tab.TabType == tabType);
        }
    }
}