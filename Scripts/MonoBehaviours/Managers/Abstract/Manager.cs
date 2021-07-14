using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.MonoBehaviours.Managers
{
    public abstract class Manager<T> : MonoBehaviour
    where T : Manager<T>
    {
        /// <summary>
        /// Private static reference to the manager instance
        /// Unaccesible
        /// </summary>
        static T _S;


        protected virtual void Awake()
        {
            // Makes the manager a singleton
            if (_S != null)
            {
                Destroy(gameObject);
            }
            else
            {
                _S = (T)this;
            }

            // Do not destroy on load
            DontDestroyOnLoad(this);
        }
    }
}
