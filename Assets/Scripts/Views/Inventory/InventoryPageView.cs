using Connect.Core;
using Connect.Models;

namespace Connect.Views.Inventory {
    public class InventoryPageView : DrawView<Bundle> {
        protected override void Render(Bundle context) {
            gameObject.SetActive(true);
        }

        protected override bool CanDraw(Bundle context) {
            return true;
        }

        public override void Reset() {
            gameObject.SetActive(false);
        }
    }
}