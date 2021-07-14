using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game.MonoBehaviours.Stats;
using Game.Generic.Utiles;

namespace Game.MonoBehaviours.UI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Slider))]
    public class UIProgressBar : MonoBehaviour
    {
        [Header("Set in Inspector")]
        [SerializeField] Image fillImage;
        [SerializeField] Gradient colorGradient;

        Slider slider;
        Events.DoubleFloatEvent progressChangeCallback;

        public bool Inverted = false;

        // Start is called before the first frame update
        void Awake()
        {
            slider = GetComponent<Slider>();
        }

        private void OnDestroy()
        {
           if (progressChangeCallback != null)
                progressChangeCallback.RemoveListener(UpdateValue);
        }

        /// <summary>
        /// Assign callback to the target via the progressChangeCallback 
        /// </summary>
        /// <param name="callback"></param>
        public void Initialize(Events.DoubleFloatEvent callback)
        {
            progressChangeCallback = callback;
            progressChangeCallback.AddListener(UpdateValue);
        }

        public void UpdateValue(float currentValue, float maxValue)
        {
            slider.value = Inverted ? 1 - currentValue / maxValue : currentValue / maxValue;
            UpdateColor(currentValue, maxValue);
        }

        public void UpdateColor(float currentValue, float maxValue)
        {
            fillImage.color = Inverted ? colorGradient.Evaluate(1- currentValue / maxValue) : colorGradient.Evaluate(1 - currentValue / maxValue);
        }
    }
}
