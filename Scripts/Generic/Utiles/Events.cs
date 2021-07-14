using UnityEngine;
using UnityEngine.Events;
using Game.MonoBehaviours;
using Game.Interfaces;
using Game.MonoBehaviours.Inventory;
using Game.MonoBehaviours.Managers;

namespace Game.Generic.Utiles
{
    public class Events
    {
        [System.Serializable] public class VoidEvent : UnityEvent { }
        [System.Serializable] public class BoolEvent : UnityEvent<bool> { }
        [System.Serializable] public class DoubleFloatEvent : UnityEvent<float, float> { }
        [System.Serializable] public class Vector3Event : UnityEvent<Vector3> { }
        [System.Serializable] public class GameObjectEvent : UnityEvent<GameObject> { }
        [System.Serializable] public class CameraStateEvent : UnityEvent<CameraController.State> { }
        [System.Serializable] public class EnemyStateEvent : UnityEvent<Enemy.State> { }
        [System.Serializable] public class EnemyBehaviourEvent : UnityEvent<Enemy.Behaviour, Enemy.Behaviour> { }
        [System.Serializable] public class PlayerStateEvent : UnityEvent<Player.State> { }
        [System.Serializable] public class GameManagerStatEvent : UnityEvent<GameManager.State> { }
        [System.Serializable] public class IEventEvent : UnityEvent<IEvent> { }
        /// <summary>
        /// Where the first float is the CurrentValue and the second is the MaxValue
        /// </summary>
        [System.Serializable] public class StatChangeEvent : UnityEvent<float, float> { }
        [System.Serializable] public class InventoryEntryEvent : UnityEvent<InventoryEntry> { }
    }
}
