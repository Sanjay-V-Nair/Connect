namespace Connect.Core {
    public enum SceneType {
        OnboardingScene,
        DashboardScene,
        GameScene,
        SandboxScene,
    }
    
    public static class SceneTypeExtensions {
        public static string GetSceneName(this SceneType sceneType) {
            return sceneType switch {
                SceneType.OnboardingScene => "OnboardingScene",
                SceneType.DashboardScene => "DashboardScene",
                SceneType.GameScene => "GameScene",
                SceneType.SandboxScene => "SandboxScene",
                _ => throw new System.ArgumentOutOfRangeException(nameof(sceneType), sceneType, null)
            };
        }
    }
}