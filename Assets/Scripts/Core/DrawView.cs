using System;
using UnityEngine;

namespace Connect.Core {
    public abstract class DrawView<TGameSceneData> : MonoBehaviour 
    where TGameSceneData : class {

        public virtual void Draw(TGameSceneData context) {
            if (CanDraw(context)) {
                Render(context);
                gameObject.SetActive(true);
            } else {
                Reset();
                gameObject.SetActive(false);
            }
        }

        protected abstract void Render(TGameSceneData context);
        
        protected abstract bool CanDraw(TGameSceneData context);

        public abstract void Reset();
        
        protected void IsolatedDraw<TBundleData>(DrawView<TBundleData> drawView, TBundleData context) where TBundleData : class {
            try {
                if (drawView == null) {
                    Debug.LogWarning($"Draw View {drawView.name} is Null!");
                    return;
                }
                drawView.Draw(context);
            } catch (Exception e) {
                drawView.gameObject.SetActive(false);
                Debug.LogError($"Error drawing {drawView.name} because of an exception!");
                Debug.LogError(e);
            }
        }

        protected void IsolatedReset<TBundleData>(DrawView<TBundleData> drawView) where TBundleData : class{
            try {
                if (drawView == null) {
                    Debug.LogWarning($"Reset View {drawView.name} is Null!");
                }
                drawView.Reset();
            } catch (Exception e) {
                drawView.gameObject.SetActive(false);
                Debug.LogError($"Error resetting {drawView.name} because of an exception!");
                Debug.LogError(e);
            }
        }
        
    }
}