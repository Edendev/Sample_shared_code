using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game.MonoBehaviours.Utiles;

namespace Game.MonoBehaviours.UI
{
    [RequireComponent(typeof(ActiveOnlyOnSomeGameStates))]
    public class UIWinLevelMenu : MonoBehaviour
    {
        public static UIWinLevelMenu S;

        [SerializeField] Button restartButton;
        [SerializeField] Button exitButton;

        private void Awake()
        {
            S = this;
        }

        #region Accessors

        public Button.ButtonClickedEvent RestartButtonClickEvent() => restartButton.onClick;
        public Button.ButtonClickedEvent ExitButtonClickEvent() => exitButton.onClick;

        #endregion
    }
}