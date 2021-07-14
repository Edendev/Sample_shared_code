using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.MonoBehaviours.Managers;

namespace Game.MonoBehaviours.Utiles
{
    public class ActiveOnlyOnSomeGameStates : MonoBehaviour
    {
        [Header("Set in Inspector")]
        public GameManager.State activeStates;

        private void Awake()
        {
            GameManager.OnStateChanges().AddListener(OnGameStateChanges);
        }

        public void OnGameStateChanges(GameManager.State newState)
        {
            gameObject.SetActive(activeStates.HasFlag(newState));
        }

        private void OnDestroy()
        {
            GameManager.OnStateChanges().RemoveListener(OnGameStateChanges);
        }
    }
}

