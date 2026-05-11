using Connect.Core;
using Connect.Models;
using UnityEngine;

namespace Connect.Views.Credits {
    public class CreditsPageView : DrawView<Bundle> {
        
        [SerializeField] private ButtonView connectButton;

        protected override void Render(Bundle context) {
            gameObject.SetActive(true);
            SetButtons();
        }

        private void SetButtons() {
            connectButton.Draw(new ButtonViewData() {
                onClick = OnConnectButtonClicked,
            });
        }

        private void OnConnectButtonClicked() {
            MailHelper.SendEmail();
        }

        protected override bool CanDraw(Bundle context) {
            return true;
        }

        public override void Reset() {
            gameObject.SetActive(false);
        }
    }
}