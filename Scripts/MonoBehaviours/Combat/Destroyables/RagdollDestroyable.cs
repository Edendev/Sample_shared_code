using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Interfaces;

namespace Game.MonoBehaviours.Combat
{
    public class RagdollDestroyable : MonoBehaviour, IDestroyable
    {
        public RagdollObject Ragdoll;        

        public void OnDestroyed(GameObject destroyer)
        {
            // Instantiate fractured object
            RagdollObject newRagdoll = Instantiate(Ragdoll, gameObject.transform.position, gameObject.transform.rotation);
            newRagdoll.SetDirection(transform.position - destroyer.transform.position);
        }
    }
}
