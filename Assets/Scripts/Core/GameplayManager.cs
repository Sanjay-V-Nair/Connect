using Connect.Models;
using Connect.Systems.Datastore;
using Connect.Systems.EventBus;
using Connect.Systems.LevelSystem;
using Connect.Views;
using Connect.Views.GameResult;
using UnityEngine;

namespace Connect.Core {
    public class GameplayManager : DrawView<Bundle> {
        
        [SerializeField] private Transform _gridParent;
        [SerializeField] private GameResultView gameResultView;
        [SerializeField] private ButtonView resetButton;
        [SerializeField] private ButtonView goToHomeButton;
        [SerializeField] private ButtonView nextLevelButton;

        private void OnEnable() {
            EventBus<GameSceneEvents.LevelCompletedEvent>.Subscribe(OnLevelCompleted);
        }
        
        private void OnDisable() {
            EventBus<GameSceneEvents.LevelCompletedEvent>.Unsubscribe(OnLevelCompleted);
        }

        private void OnLevelCompleted(GameSceneEvents.LevelCompletedEvent obj) {
            gameResultView.Draw(null);
            GameManager.Instance.UnlockNextLevel();
        }

        private void Start() {
            Render(null);
            SetButtons();
        }

        private void SetButtons() {
            resetButton.Draw(new ButtonViewData() {
                onClick = OnResetClicked,
            });
            goToHomeButton.Draw(new ButtonViewData() {
                onClick = OnHomeButtonClicked,
            });
            nextLevelButton.Draw(new ButtonViewData() {
                onClick = NextLevelButtonClicked,
            });
        }

        protected override void Render(Bundle context) {
            var levelData = LevelManager.Instance.GetLevel(GameStateData.GetGameLevel());
            if (levelData != null) {
                LevelController.Instance.StartLevel(levelData);
            }
        }

        private void OnResetClicked() {
            LevelController.Instance.ResetLevel();
        }
        
        private void OnHomeButtonClicked() {
            LevelController.Instance.ResetLevel();
            GameManager.Instance.LoadSceneAsync(SceneType.DashboardScene.GetSceneName());
        }
        
        private void NextLevelButtonClicked() {
            GameManager.Instance.LoadSceneAsync(SceneType.DashboardScene.GetSceneName());
        }
        
        protected override bool CanDraw(Bundle context) {
            return true;
        }

        public override void Reset() {
            
        }
    }
}