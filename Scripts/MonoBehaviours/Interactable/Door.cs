using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Interfaces.UI;
using Game.Generic.Utiles;

namespace Game.MonoBehaviours.Interactable
{
    public class Door : MonoBehaviour, IInteractable
    {
        [Header("Set in Inspector")]
        [Range(0.1f, 10f)]
        [Tooltip("Time to open or close")]
        [SerializeField] float switchTime = 1f;
        [SerializeField] bool closeAutomatic = false;
        [Range(10f, 100f)]
        [Tooltip("Steps to open or close")]
        [SerializeField] int smoothness = 100;
        [SerializeField] Transform anchorPoint;
        [SerializeField, Tooltip("Opening constraints")] Constraints constraint = Constraints.None;

        [System.Serializable]
        [System.Flags]
        public enum Constraints
        {
            None,
            Forward,
            Backward,
        }

        int currentSteps;

        public enum State
        {
            Open, Closed, Opening, Closing
        }

        private State state = State.Closed;

        // Saved locations
        Quaternion closedRotation, openRotation;

        #region Callbacks

        [SerializeField] Events.VoidEvent onInteract = new Events.VoidEvent();

        #endregion

        private void Awake()
        {
            closedRotation = anchorPoint.rotation;
            openRotation = Quaternion.LookRotation(anchorPoint.right);

            // Starts completely closed
            currentSteps = smoothness;
        }

        #region IInteractable

        public IEnumerator Interact(GameObject target)
        {
            // First stop any coroutine
            StopAllCoroutines();

            if (state == State.Closed)
            {
                // Determine opening direction
                if (Vector3.Dot(anchorPoint.forward.normalized, (target.transform.position - anchorPoint.position).normalized) > 0f)
                {
                    if (!constraint.HasFlag(Constraints.Backward))
                        openRotation = Quaternion.LookRotation(anchorPoint.right);
                    else
                        yield break;
                }
                else
                {
                    if (!constraint.HasFlag(Constraints.Forward))
                        openRotation = Quaternion.LookRotation(-anchorPoint.right);
                    else
                        yield break;
                }
            }

            if (state == State.Closing || state == State.Closed)
            {
                StartCoroutine(Open());
            }
            else
            {
                StartCoroutine(Close());
            }

            onInteract?.Invoke();

            yield return null;
        }

        public Events.VoidEvent OnInteract() => onInteract;

        #endregion

        IEnumerator Open()
        {
            state = State.Opening;

            // Starts opening from where it was left after closing
            currentSteps = smoothness - currentSteps;

             while (currentSteps < smoothness)
            {

                anchorPoint.rotation = Quaternion.Lerp(closedRotation, openRotation, 1f / smoothness * currentSteps);
                currentSteps++;
                yield return new WaitForSeconds(switchTime / smoothness);
            }

            state = State.Open;

            yield return null;
        }

        IEnumerator Close()
        {
            state = State.Closing;

            // Starts closing from where it was left after closing
            currentSteps = smoothness - currentSteps;

             while (currentSteps < smoothness)
            {
                anchorPoint.rotation = Quaternion.Lerp(openRotation, closedRotation, 1f / smoothness * currentSteps);
                currentSteps++;
                yield return new WaitForSeconds(switchTime / smoothness);
            }

            state = State.Closed;

            yield return null;
        }
    }

}
