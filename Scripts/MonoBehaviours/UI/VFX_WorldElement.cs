using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.MonoBehaviours.Managers;

namespace Game.MonoBehaviours.Enemies
{
    public abstract class VFX_WorldElement<T> : MonoBehaviour
        where T : MonoBehaviour
    {
        [Header("Set in inspector")]
        [Tooltip("World element to show in the world")]
        [SerializeField] protected T WorldElement;
        [Tooltip("Position in world units relative to object world position where the bar will be shown")]
        [SerializeField] protected Vector3 OffsetShowPosition = Vector3.zero;
        [Tooltip("Size in pixel units of the world element")]
        [SerializeField] protected Vector2 Size = new Vector2(50, 50);

        // World element required components
        protected RectTransform worldElementRectTransform;

        // Start is called before the first frame update
        protected void Start()
        {
            // Create world element in VFXCanvas
            WorldElement = Instantiate(WorldElement);
            WorldElement.transform.SetParent(GameObject.FindGameObjectWithTag("VFXCanvas").transform);
            WorldElement.transform.localScale = GameObject.FindGameObjectWithTag("VFXCanvas").transform.localScale;
            WorldElement.gameObject.SetActive(false);

            // Get stat bar rectTransform and set size through scale
            worldElementRectTransform = WorldElement.GetComponent<RectTransform>();
            float xScale = Size.x / worldElementRectTransform.sizeDelta.x;
            float yScale = Size.y / worldElementRectTransform.sizeDelta.y;
            Vector3 currentScale = WorldElement.transform.localScale;
            WorldElement.transform.localScale = new Vector3(currentScale.x * xScale, currentScale.y * yScale, currentScale.z);
        }

        void OnDisable()
        {
            if (WorldElement != null)
                WorldElement.gameObject.SetActive(false);
        }

        protected IEnumerator ShowWorldElement()
        {
            // Update initial position before showing
            worldElementRectTransform.position = transform.position;
            worldElementRectTransform.transform.LookAt(Vector3.MoveTowards(worldElementRectTransform.transform.position, worldElementRectTransform.transform.position - GameManager.MainCameraTransform().forward.normalized, 50f));

            worldElementRectTransform.gameObject.SetActive(true);

            while (true)
            {
                worldElementRectTransform.position = transform.position + OffsetShowPosition;
                worldElementRectTransform.transform.LookAt(Vector3.MoveTowards(worldElementRectTransform.transform.position, worldElementRectTransform.transform.position - GameManager.MainCameraTransform().forward.normalized, 50f));
                yield return null;
            }
        }
    }
}
