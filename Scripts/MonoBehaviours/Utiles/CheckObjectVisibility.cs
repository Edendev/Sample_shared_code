using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Game.MonoBehaviours.Managers;
using Game.Interfaces.UI;
using Game.Generic.PathFinding;
using Game.Generic.Data;
using Game.MonoBehaviours.Inventory;
using Game.Generic.Utiles;
using Game.MonoBehaviours.Stats;
using Game.MonoBehaviours.Interactable;
using Game.Interfaces;
using Game.ScriptableObjects.Combat;

namespace Game.MonoBehaviours.Utiles
{
    public class CheckObjectVisibility : MonoBehaviour
    {
        [SerializeField] Events.Vector3Event onBecomeVisible = new Events.Vector3Event();
        [SerializeField] Events.Vector3Event onBecomeInvisible = new Events.Vector3Event();

        private void OnBecameInvisible()
        {
            if (GameManager.MainCamera() != null)
                onBecomeInvisible?.Invoke(GameManager.MainCamera().WorldToScreenPoint(transform.root.position));
        }
        private void OnBecameVisible()
        {
            if (GameManager.MainCamera() != null)
                onBecomeVisible?.Invoke(GameManager.MainCamera().WorldToScreenPoint(transform.root.position));
        }

        public Events.Vector3Event OnBecomeVisible() => onBecomeVisible;
        public Events.Vector3Event OnBecomeInvisible() => onBecomeInvisible;
    }
}

