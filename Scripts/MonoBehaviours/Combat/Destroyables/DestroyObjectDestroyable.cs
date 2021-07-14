using UnityEngine;
using System.Collections;
using Game.Interfaces;

namespace Game.MonoBehaviours.Combat
{
    public class DestroyObjectDestroyable : MonoBehaviour, IDestroyable
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

            Destroy(gameObject);
        }
    }
}

