using System.Threading.Tasks;
using Connect.Core;
using Connect.Models;
using UnityEngine;

namespace Connect.Views.OnBoarding {
    public class StartScreen : DrawView<Bundle> {

        [SerializeField] private GameObject bird;
        
        private void Start() {
            Render(null);
        }

        protected override void Render(Bundle context) {
            _ = DoBirdFlyAnimation();
        }

        private async Task DoBirdFlyAnimation() {
            await Task.Delay(3000);
            bird.SetActive(true);
            await Task.Delay(2000);
            GameManager.Instance.LoadScene(SceneType.DashboardScene);
        }

        protected override bool CanDraw(Bundle context) {
            return true;
        }

        public override void Reset() {
            
        }
    }
}