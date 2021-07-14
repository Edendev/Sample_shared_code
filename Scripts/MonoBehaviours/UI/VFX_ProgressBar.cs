using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.MonoBehaviours.UI
{
    public class VFX_ProgressBar : MonoBehaviour
    {
        [Header("Set in inspector")]
        [Tooltip("Position in world units relative to object world position where the bar will be shown")]
        public Vector3 OffsetShowPosition = Vector3.zero;
        [Tooltip("Size in pixel units of the UI slide bar")]
        public Vector2 Size = new Vector2(50, 10);
        public UIProgressBar ProgressBar;
        public float ShowingTime = 3f;
        public bool ShowPermanently = false;

        [Header("_Bounce effect when poping up")]
        public AnimationCurve BounceEffectCurve;
        public float BounceMaxHeight = 0.1f;
        public float BounceTime = 0.5f;

        protected float actualShowingTime;

        // Necessary progressbar components
        RectTransform pB_rectTransform;

        protected virtual void Start()
        {
            // Create stat bar in VFXCanvas
            ProgressBar = Instantiate(ProgressBar);
            ProgressBar.transform.SetParent(GameObject.FindGameObjectWithTag("VFXCanvas").transform);
            ProgressBar.transform.localScale = GameObject.FindGameObjectWithTag("VFXCanvas").transform.localScale;
            ProgressBar.gameObject.SetActive(false);

            // Get stat bar rectTransform and set size through scale
            pB_rectTransform = ProgressBar.GetComponent<RectTransform>();
            float xScale = Size.x / pB_rectTransform.sizeDelta.x;
            float yScale = Size.y / pB_rectTransform.sizeDelta.y;
            Vector3 currentScale = ProgressBar.transform.localScale;
            ProgressBar.transform.localScale = new Vector3(currentScale.x * xScale, currentScale.y * yScale, currentScale.z);

            // Initialize actual showing time to 0f to trigger coroutine the first time
            actualShowingTime = 0f;
        }

        protected IEnumerator ShowProgressBar()
        {
            // Update initial position before showing
            pB_rectTransform.position = transform.position;
            pB_rectTransform.transform.LookAt(Vector3.MoveTowards(pB_rectTransform.transform.position, pB_rectTransform.transform.position - Camera.main.transform.forward.normalized, 50f));

            ProgressBar.gameObject.SetActive(true);

            int steps = (int)(BounceTime / Time.deltaTime);
            int count = 0;
            while (actualShowingTime > 0f || ShowPermanently)
            {
                Vector3 newPosition = transform.position + OffsetShowPosition;
                Vector3 maxHeightPosition = newPosition;
                maxHeightPosition.y += BounceMaxHeight;
                newPosition = Vector3.LerpUnclamped(newPosition, maxHeightPosition, BounceEffectCurve.Evaluate(1f / steps * count));
                if (count < steps) count++;

                pB_rectTransform.position = newPosition;
                pB_rectTransform.transform.LookAt(Vector3.MoveTowards(pB_rectTransform.transform.position, pB_rectTransform.transform.position - Camera.main.transform.forward.normalized, 50f));

                // Decrease showing time
                actualShowingTime -= Time.deltaTime;

                yield return null;
            }

            ProgressBar.gameObject.SetActive(false);
        }
    }
}
