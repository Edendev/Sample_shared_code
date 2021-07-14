using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.ScriptableObjects.Core;

namespace Game.ScriptableObjects.Enemies
{
    public class PlayAnimatorStatePA : PatrolAction
    {
        /// <summary>
        /// Animator state name.
        /// </summary>
        public string StateName;
        public int StateLayerNumber = 0;
        public float StateNormalizedTime = 0f;
        /// <summary>
        /// If true, the animation state will be played after a randomly generated delay within 0 and the MaxDelay given.
        /// If false, the animation state will be player after the MaxDelay given.
        /// </summary>
        public bool GenerateRandomDelay = false;
        public float MaxDelay;

        // Required components
        Animator animator;

        public override IEnumerator Execute(GameObject target)
        {
            animator = target.GetComponent<Animator>();
            if (!animator)
                yield break;

            float delay = MaxDelay;

            if (GenerateRandomDelay)
                delay = Random.Range(0f, MaxDelay);

            MonoBehaviour targetMonoBehaviour = target.GetComponent<MonoBehaviour>();
            targetMonoBehaviour.StartCoroutine(PlayAnimationAfterDelay(delay));

            yield return null;
        }

        IEnumerator PlayAnimationAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (!animator)
                yield break;

            animator.Play(StateName, StateLayerNumber, StateNormalizedTime);
            yield return new WaitForEndOfFrame(); // To enable the animator entering the attack state
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length / animator.GetCurrentAnimatorStateInfo(0).speed);
        }
    }
}
