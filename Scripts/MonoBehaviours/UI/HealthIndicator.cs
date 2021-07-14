using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.MonoBehaviours.UI
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Image))]
    public class HealthIndicator : MonoBehaviour
    {
        // Required components
        Animator animator;
        Image image;

        /// <summary>
        /// Define the color gradient according to the health percentage
        /// </summary>
        [SerializeField] Gradient colorGradient;
        [SerializeField] AnimationCurve beatingRateCurve;
        /// <summary>
        /// Beating rate in seconds. Value will be increased according to Health percentage through the beatingRateCurve.
        /// </summary>
        [SerializeField] float maxBeatingRate;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            image = GetComponent<Image>();
        }

        private void Start()
        {
            Player.S.OnResistanceChanged().AddListener(UpdateIndicator);
            UpdateIndicator(Player.S.CurrentResistance(), Player.S.MaxResistance());
        }
        private void OnDestroy()
        {
            if (Player.S != null)
                Player.S.OnResistanceChanged().RemoveListener(UpdateIndicator);
        }

        void UpdateIndicator(float currentHealth, float maxHealth)
        {
            image.color = colorGradient.Evaluate(1 - currentHealth / maxHealth);
            animator.speed = maxBeatingRate * beatingRateCurve.Evaluate(1 - currentHealth / maxHealth);

            if (currentHealth == maxHealth)
                gameObject.SetActive(false);
            else
                gameObject.SetActive(true);
        }
    }

}
