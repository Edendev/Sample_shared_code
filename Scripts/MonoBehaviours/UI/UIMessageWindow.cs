using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game.Generic.Utiles;

namespace Game.MonoBehaviours.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class UIMessageWindow : UIWindow
    {
        [Header("Set from Inspector")]
        [SerializeField] Text messageText;
        [SerializeField] Image portraitImage;

        public void SetPortrait(Sprite sprite)
        {
            portraitImage.sprite = sprite;
        }
        public void SetMessageText(string text)
        {
            CleanMessageText();
            messageText.text = text;
        }
        public void CleanMessageText() => messageText.text = "";
    }

}
