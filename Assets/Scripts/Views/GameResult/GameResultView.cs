using Connect.Core;
using Connect.Models;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Connect.Views.GameResult {
    public class GameResultView : DrawView<Bundle> {

        [SerializeField] private ButtonView homeButton;
        [SerializeField] private TMP_Text resultText;
        
        protected override void Render(Bundle context) {
            gameObject.SetActive(true);
            AudioManager.Instance.PlayWinAudio();
            SetButtons();
            resultText.DOFade(1.0f, 0.6f);
        }

        private void SetButtons() {
            homeButton.Draw(new ButtonViewData() {
                onClick = OnHomeButtonClicked
            });
        }

        private void OnHomeButtonClicked() {
            GameManager.Instance.LoadSceneAsync(SceneType.DashboardScene.GetSceneName());
        }

        protected override bool CanDraw(Bundle context) {
            return true;
        }

        public override void Reset() {
            gameObject.SetActive(false);
        }
    }
}