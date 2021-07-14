using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.MonoBehaviours.UI
{
    [RequireComponent(typeof(Image))]
    public class ColorChangeOverTime : MonoBehaviour
    {
        Image image;
        [SerializeField] Color initialColor;
        [SerializeField] Color finalColor;
        [SerializeField] AnimationCurve colorChangeCurve;
        /// <summary>
        /// Time needed to change from initial to final and back to initial color (whole cycly)
        /// </summary>
        [SerializeField] float time;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            StopAllCoroutines();
            StartCoroutine(ChangingColor());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        IEnumerator ChangingColor()
        {
            float totalTime = time / 2f;
            float timer = 0f;
            bool increasing = true;
            while (true)
            {
                image.color = Color.LerpUnclamped(initialColor, finalColor, colorChangeCurve.Evaluate(timer / time));
                timer += increasing ? Time.deltaTime : -Time.deltaTime;

                if (timer >= time)
                    increasing = false;
                if (timer <= 0f)
                    increasing = true;

                yield return null;
            }
        }
    }
}

