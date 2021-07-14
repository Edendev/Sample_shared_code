using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game.MonoBehaviours.Managers;

namespace Game.MonoBehaviours.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class ScaleWithScreen : MonoBehaviour
    {
        /// <summary>
        /// Defines the scalling mode: only resizing, scalling or everything
        /// </summary>
        [SerializeField] ScaleMode scaleMode;

        [System.Serializable]
        [System.Flags]
        public enum ScaleMode
        {
            None,
            RectSize,
            Scale,
        }

        // Required components
        RectTransform rectTransform;

        // Saved reference parameters
        float referenceWidth;
        float referenceHeight;
        float refWidthRatio;
        float refHeightRatio;

        private void Awake()
        {
            rectTransform = transform.GetComponent<RectTransform>();
        }

        private void Start()
        {
            // Get reference parameters
            referenceWidth = rectTransform.sizeDelta.x;
            referenceHeight = rectTransform.sizeDelta.y;

            // Determine aspect ratio and reference width, height ratios
            refWidthRatio = referenceWidth / UICanvas.RefAspect();
            refHeightRatio = referenceHeight / UICanvas.RefAspect();

            GameManager.OnScreenChanges().AddListener(AdjustSize);

            AdjustSize();
        }

        private void OnDestroy()
        {
            GameManager.OnScreenChanges().RemoveListener(AdjustSize);
        }

        void AdjustSize()
        {
            if (scaleMode.HasFlag(ScaleMode.Scale))
                Scale();

            if (scaleMode.HasFlag(ScaleMode.RectSize))
                Resize();
        }

        void Scale()
        {
            float new_x_scale = 1f;
            float new_y_scale = 1f;

            if (GameManager.MainCamera().aspect < UICanvas.RefAspect())
            {
                new_y_scale = GameManager.MainCamera().aspect / UICanvas.RefAspect();
                new_x_scale = GameManager.MainCamera().aspect / UICanvas.RefAspect();
            }
            else
            {
                new_x_scale = GameManager.MainCamera().aspect / UICanvas.RefAspect();
            }

            rectTransform.localScale = new Vector2(new_x_scale, new_y_scale);
        }
        void Resize()
        {
            float new_width = referenceWidth;
            float new_height = referenceHeight;

            if (GameManager.MainCamera().aspect < UICanvas.RefAspect())
            {
                new_width = GameManager.MainCamera().aspect * refWidthRatio;
                new_height = GameManager.MainCamera().aspect * refHeightRatio;
            }
            else
                new_width = GameManager.MainCamera().aspect * refWidthRatio;

            rectTransform.sizeDelta = new Vector2(new_width, new_height);
        }
    }
}

