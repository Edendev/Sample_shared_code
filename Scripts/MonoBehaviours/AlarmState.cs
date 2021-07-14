using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Generic.Utiles;

namespace Game.MonoBehaviours
{
    public class AlarmState : MonoBehaviour
    {
        public static AlarmState S { get; private set; }

        // State of the alarm 
        bool isOn = false;
        // Determines wether the alarm mode is on or not
        [SerializeField] int enemiesHuntingCount = 0;

        [SerializeField] private Events.BoolEvent onStateChanges = new Events.BoolEvent();

        #region Accessors

        public bool IsOn() => isOn;
        public Events.BoolEvent OnStateChanges() => onStateChanges;

        #endregion

        private void Awake()
        {
            // Make instance a singleton
            if (S != null)
            {
                Destroy(gameObject);
            }
            else
            {
                S = this;
            }
        }

        private void Start()
        {
            // Register to all enemies change state
            Enemy[] enemies = FindObjectsOfType<Enemy>();
            if (enemies != null && enemies.Length > 0)
            {
                foreach (Enemy enemy in enemies)
                {
                    enemy.OnBehaviourChanges().AddListener(EnemyChangedBehaviour);
                }
            }
        }

        void EnemyChangedBehaviour(Enemy.Behaviour oldBehaviour, Enemy.Behaviour newBehaviour)
        {
            if (oldBehaviour == Enemy.Behaviour.HuntingPlayer && newBehaviour != Enemy.Behaviour.HuntingPlayer)
                enemiesHuntingCount--;
            else if (oldBehaviour != Enemy.Behaviour.HuntingPlayer && newBehaviour == Enemy.Behaviour.HuntingPlayer)
                enemiesHuntingCount++;

            UpdateAlarmState();
        }

        void UpdateAlarmState()
        {
            bool oldValue = isOn;
            isOn = enemiesHuntingCount > 0 ? true : false;

            if (isOn != oldValue)
                onStateChanges?.Invoke(isOn);
        }
    }
}

