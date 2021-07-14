using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.MonoBehaviours.Utiles
{
    public class ActiveIfAlarmStateIsOn : MonoBehaviour
    {
        private void Start()
        {
            AlarmState.S.OnStateChanges().AddListener(OnAlarmStateChanges);

            // Initialize
            OnAlarmStateChanges(AlarmState.S.IsOn());
        }

        void OnAlarmStateChanges(bool isOn)
        {
            gameObject.SetActive(isOn);
        }

        private void OnDestroy()
        {
            if (AlarmState.S != null)
                AlarmState.S.OnStateChanges().RemoveListener(OnAlarmStateChanges);
        }
    }

}
