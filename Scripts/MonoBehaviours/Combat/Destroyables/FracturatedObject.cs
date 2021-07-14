using UnityEngine;
using System.Collections;

namespace Game.MonoBehaviours.Combat
{
    public class FracturatedObject : MonoBehaviour
    {
        [Header("Set in Inspector")]
        public float DelayBeforeDestroy = 4f;
        public float ExplosionForce = 250;
        public float ExplosionRadius = 1;

        Rigidbody[] rigidBodies;

        private void Awake()
        {
            rigidBodies = gameObject.GetComponentsInChildren<Rigidbody>();
        }

        private void Start()
        {

            if (rigidBodies != null)
            {
                foreach (Rigidbody rigidbody in rigidBodies)
                {
                    rigidbody.AddExplosionForce(ExplosionForce, rigidbody.gameObject.transform.position, ExplosionRadius);
                }
            }

            StartCoroutine(DestroyAterDelay(DelayBeforeDestroy));
        }

        private IEnumerator DestroyAterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            Destroy(gameObject);
        }
    }
}
