using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.MonoBehaviours.Combat
{
    [RequireComponent(typeof(Rigidbody))]
    public class RagdollObject : MonoBehaviour
    {
        [Header("Set in Inspector")]
        public float DelayBeforeDestroy = 4f;
        public float Force;
        public float Lift;

        Rigidbody rigidBody;

        /// <summary>
        /// Direction in which the force is applied
        /// </summary>
        Vector3 direction = Vector3.zero;

        public void SetDirection(Vector3 dir) => direction = dir;

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            // Normalize direction and add lift
            direction.Normalize();
            direction.y += Lift;

            // Add force
            rigidBody.AddForce(direction.normalized * Force);

            StartCoroutine(DestroyAterDelay(DelayBeforeDestroy));
        }

        private IEnumerator DestroyAterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            Destroy(gameObject);
        }
    }
}
