using System;
using Connect.Core;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Connect.Views {
    public class ButtonView : DrawView<ButtonViewData> {

        [SerializeField] private TMP_Text text;
        [SerializeField] private Button button;
        
        protected override void Render (ButtonViewData data) {
            SetButtonClick(data.onClick);
            SetButtonText(data.buttonText);
        }

        protected override bool CanDraw(ButtonViewData context) {
            return context != null && context.onClick != null;
        }

        public override void Reset() {
            gameObject.SetActive(false);
        }

        private void SetButtonClick(Action dataOnClick) {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnButtonViewClick(dataOnClick));
        }

        private void OnButtonViewClick(Action dataOnClick) {
            AudioManager.Instance.PlayButtonClickAudio();
            dataOnClick?.Invoke();
        }

        private void SetButtonText(string dataButtonText) {
            if (!string.IsNullOrEmpty(text.text) && string.IsNullOrEmpty(dataButtonText)) return;
            text.text = dataButtonText;
        }
    }
    
    [Serializable]
    public class ButtonViewData {
        public string buttonText;
        public Action onClick;
        public Color buttonColor;
        public float textSize;
    }
}