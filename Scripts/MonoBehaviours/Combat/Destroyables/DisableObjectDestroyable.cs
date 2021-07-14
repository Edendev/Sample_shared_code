using UnityEngine;
using System.Collections;
using Game.Interfaces;
using Game.Generic.Analytics;

namespace Game.MonoBehaviours.Combat
{
    public class DisableObjectDestroyable : MonoBehaviour, IDestroyable
    {
        private Coroutine DestroyingAfterEndOfFrame;

        public void OnDestroyed(GameObject destroyer)
        {
            // Check if already called to be destroyed
            if (DestroyingAfterEndOfFrame != null)
                return;

            // Destroy gameObject after end of frame
            DestroyingAfterEndOfFrame = StartCoroutine(DestroyAfterEndOfFrame());
        }

        private IEnumerator DestroyAfterEndOfFrame()
        {
            yield return new WaitForEndOfFrame();

            CustomAnalytics.SendDestroyedObject(gameObject);

            gameObject.SetActive(false);
        }
    }
}