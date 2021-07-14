using UnityEngine;
using System.Collections;
using Game.Interfaces;

namespace Game.MonoBehaviours.Combat
{
    public class FracturedDestroyable : MonoBehaviour, IDestroyable
    {
        public FracturatedObject FracturedObject;

        public void OnDestroyed(GameObject destroyer)
        {
            // Instantiate fractured object
            Instantiate(FracturedObject, gameObject.transform.position, gameObject.transform.rotation);
        }
    }
}

