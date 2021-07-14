using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game.Generic.Utiles;
using Game.MonoBehaviours.Managers;

namespace Game.MonoBehaviours.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class UIWindow : MonoBehaviour
    {
        [Header("Set from Inspector")]
        [SerializeField] AnimationCurve motionCurve;
        [SerializeField] float popUpTime = 1f;
        [SerializeField] float popDownTime = 0.5f;
        /// <summary>
        /// Pop up and down direction
        /// </summary>
        [SerializeField] Vector2Int direction;

        /// <summary>
        /// Window visible position in screen pixel coordinates
        /// </summary>
        Vector2 visiblePosition;
        /// <summary>
        /// Window hidden position in screen pixel coordinates
        /// </summary>
        Vector2 hiddenPosition;

        // Required components
        RectTransform rectTransform;
        float pathMaxDistance;

        Events.VoidEvent onWindowIsVisible = new Events.VoidEvent();
        Events.VoidEvent onWindowIsHidden = new Events.VoidEvent();

        bool windowVisibleState;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            visiblePosition = rectTransform.localPosition;
            hiddenPosition = new Vector2(-UICanvas.RefWidth() * direction.x, -UICanvas.RefHeight() * direction.y);
            pathMaxDistance = (visiblePosition - hiddenPosition).magnitude;

            // Hide window
            PopDown(true);
        }

        public void PopUp(bool instantly = false)
        {
            // Stop all coroutines
            StopAllCoroutines();

            if (instantly)
            {
                rectTransform.localPosition = visiblePosition;
                onWindowIsVisible?.Invoke();
                windowVisibleState = true;
                return;
            }

            // Start poping up coroutine
            StartCoroutine(PopingUp());
        }
        public void PopDown(bool instantly = false)
        {
            // Stop all coroutines
            StopAllCoroutines();

            if (instantly)
            {
                rectTransform.localPosition = hiddenPosition;
                onWindowIsHidden?.Invoke();
                windowVisibleState = false;
                return;
            }

            // Start poping up coroutine
            StartCoroutine(PopingDown());
        }

        IEnumerator PopingUp()
        {
            Vector3 currentPosition = rectTransform.localPosition;
            float timer = Time.time;
            // Adjust pop up time according to the current position of the window
            float adjustedPopUpTime = popUpTime * ((Vector2)currentPosition - visiblePosition).magnitude / pathMaxDistance;
            while (Time.time - timer < adjustedPopUpTime)
            {
                rectTransform.localPosition = Vector3.LerpUnclamped(currentPosition, visiblePosition, motionCurve.Evaluate((Time.time - timer) / adjustedPopUpTime));
                yield return null;
            }

            onWindowIsVisible?.Invoke();
            windowVisibleState = true;

            yield return null;
        }
        IEnumerator PopingDown()
        {
            Vector3 currentPosition = rectTransform.localPosition;
            float timer = Time.time;
            // Adjust pop up time according to the current position of the window
            float adjustedPopDownTime = popDownTime * ((Vector2)currentPosition - hiddenPosition).magnitude / pathMaxDistance;
            while (Time.time - timer < adjustedPopDownTime)
            {
                rectTransform.localPosition = Vector3.LerpUnclamped(currentPosition, hiddenPosition, motionCurve.Evaluate((Time.time - timer) / adjustedPopDownTime));
                yield return null;
            }

            onWindowIsHidden?.Invoke();
            windowVisibleState = false;

            yield return null;
        }

        #region Accessors

        public Events.VoidEvent OnWindowIsVisible() => onWindowIsVisible;
        public Events.VoidEvent OnWindowIsHidden() => onWindowIsHidden;
        public bool WindowVisibleState() => windowVisibleState;

        #endregion
    }
}
