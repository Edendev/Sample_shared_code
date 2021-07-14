using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.MonoBehaviours.Managers;
using Game.Interfaces;
using UnityEngine.UI;

namespace Game.MonoBehaviours.UI
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(RectTransform))]
    public class MapPointer : MonoBehaviour
    {
        // Required components
        RectTransform rectTransform;

        Vector3 currentScreenPosition;

        bool onWorldEvent;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

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
            onWorldEvent = true;
            gameObject.SetActive(false);
        }
        void OnWorldEventFinished(IEvent e) => onWorldEvent = false;

        public void OnPlayerBecomesVisible(Vector3 screenPosition)
        {
            if (onWorldEvent)
                return;

            StopAllCoroutines();
            gameObject.SetActive(false);
        }
        public void OnPlayerBecomesInvisible(Vector3 screenPosition)
        {
            if (onWorldEvent)
                return;

            StopAllCoroutines();
            gameObject.SetActive(true);
            StartCoroutine(TrackPlayerPosition());
        }

        IEnumerator TrackPlayerPosition()
        {
            while(true)
            {
                // Get player position in the viewport and clamp it between 0 and 1
                Vector3 screenPosition = GameManager.MainCamera().WorldToViewportPoint(Player.S.transform.position);

                if (screenPosition != currentScreenPosition)
                {
                    currentScreenPosition = screenPosition;

                    screenPosition.x = Mathf.Clamp(screenPosition.x, 0, 1);
                    screenPosition.y = Mathf.Clamp(screenPosition.y, 0, 1);
                    // Position relative to center of screen
                    screenPosition.x -= 0.5f;
                    screenPosition.y -= 0.5f;
                    // Angle from Y axis
                    float angle = Mathf.Atan2(screenPosition.y, screenPosition.x) * Mathf.Rad2Deg - 90f;
                    // Get pixel positions  
                    if (Mathf.Abs(screenPosition.x) > 0.45f)
                        screenPosition.x = (screenPosition.x < 0f) ? screenPosition.x* UICanvas.RefWidth() + rectTransform.sizeDelta.x : screenPosition.x * UICanvas.RefWidth() - rectTransform.sizeDelta.x;
                    else
                        screenPosition.x *= UICanvas.RefWidth();

                    if (Mathf.Abs(screenPosition.y) > 0.45f)
                        screenPosition.y = (screenPosition.y < 0f) ? screenPosition.y * UICanvas.RefHeight() + rectTransform.sizeDelta.y : screenPosition.y * UICanvas.RefHeight() - rectTransform.sizeDelta.y;
                    else
                        screenPosition.y *= UICanvas.RefHeight();

                    screenPosition.z = 0f;
                    rectTransform.localPosition = screenPosition;
                    // Tilt pointer towards position
                    rectTransform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
                }

                yield return null;
            }
        }
    }
}
