using Connect.Views;

namespace Connect.Systems.EventBus {
    public class DashboardEvents {
        public struct DashboardTabEvent : IEvent { public DashboardTabType TabType; }

    }
    
    public class GameSceneEvents {
        public struct LevelCompletedEvent : IEvent { public bool IsWin; }
    }
}