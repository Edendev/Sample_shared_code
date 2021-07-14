using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.MonoBehaviours.Managers;
using Game.Interfaces;

namespace Game.MonoBehaviours.Utiles
{
    public class DisabledOnWorldEvents : MonoBehaviour
    {
        bool oldState;

        private void Start()
        {
            WorldEventsManager.OnEventExecuted().AddListener(OnWorldEventExecuted);
            WorldEventsManager.OnEventFinished().AddListener(OnWorldEventFinished);
        }

        private void OnDestroy()
        {
            WorldEventsManager.OnEventExecuted().RemoveListener(OnWorldEventExecuted);
            WorldEventsManager.OnEventFinished().RemoveListener(OnWorldEventFinished);
        }

        void OnWorldEventExecuted(IEvent e)
        {
            oldState = gameObject.activeSelf;
            gameObject.SetActive(false);
        }
        void OnWorldEventFinished(IEvent e) => gameObject.SetActive(oldState);
    }
}
