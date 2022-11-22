using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Interfaces;

namespace Game.Generic.ObjectPooling
{
    public class ObjectPool<T> 
        where T : Component, IPooleable<T>
    {
        public T PoolObject;

        public int PoolSize { get { return activePoolObjects.Count + inactivePoolObjects.Count; } }
        public Transform PoolParent;
        public bool CanIncrease = true;

        List<T> activePoolObjects = new List<T>();
        Queue<T> inactivePoolObjects = new Queue<T>();

        public ObjectPool(T poolObject, Transform poolParent, int poolSize)
        {
            PoolObject = poolObject;
            PoolParent = poolParent;
            IncreasePoolSize(poolSize);
        }

        public ObjectPool(T poolObject, Transform poolParent, int poolSize, bool canIncrease)
        {
            PoolObject = poolObject;
            PoolParent = poolParent;
            CanIncrease = canIncrease;
            IncreasePoolSize(poolSize);
        }

        public void IncreasePoolSize(int num = 1)
        {
            if (!CanIncrease) return;

            for (int i = 0; i < num; i++)
            {
                T lookAtObj = Object.Instantiate(PoolObject, PoolParent);
                lookAtObj.PoolOrigin = this;
                lookAtObj.gameObject.SetActive(false);
                inactivePoolObjects.Enqueue(lookAtObj);
            }
        }

        public void DecreasePoolSize(int num)
        {
            if (inactivePoolObjects.Count < num) num = inactivePoolObjects.Count;
            for (int i = 0; i < num; i++)
            {
                GameObject objectToDestroy = inactivePoolObjects.Dequeue().gameObject;
                if (objectToDestroy == null) return;
                Object.Destroy(objectToDestroy);
            }
        }

        public T GetObject(Transform parent, Vector3 location, Vector3 rotation, Space relativeTo = Space.Self)
        {
            T lookAtObj = null;
            if (!HasInactveObjects()) IncreasePoolSize();
            if (!HasInactveObjects()) return null;
            lookAtObj = inactivePoolObjects.Dequeue();
            lookAtObj.transform.parent = parent;

            if (relativeTo == Space.Self)
            {
                lookAtObj.transform.localPosition = location;
                lookAtObj.transform.localRotation = Quaternion.Euler(rotation);
            }
            else
            {
                lookAtObj.transform.position = location;
                lookAtObj.transform.rotation = Quaternion.Euler(rotation);
            }
 
            lookAtObj.gameObject.SetActive(true);
            lookAtObj.OnPulledOut();
            activePoolObjects.Add(lookAtObj);
            return lookAtObj;
        }

        public T GetObject(Transform parent, Vector3 location, Space relativeTo = Space.Self)
        {
            T lookAtObj = null;
            if (!HasInactveObjects()) IncreasePoolSize();
            if (!HasInactveObjects()) return null;
            lookAtObj = inactivePoolObjects.Dequeue();
            lookAtObj.transform.parent = parent;

            if (relativeTo == Space.Self)
            {
                lookAtObj.transform.localPosition = location;
            }
            else
            {
                lookAtObj.transform.position = location;
            }

            lookAtObj.gameObject.SetActive(true);
            lookAtObj.OnPulledOut();
            activePoolObjects.Add(lookAtObj);
            return lookAtObj;
        }

        public void ReturnObject(T objectToReturn)
        {
            activePoolObjects.Remove(objectToReturn);
            objectToReturn.transform.SetParent(PoolParent);
            objectToReturn.gameObject.SetActive(false);
            inactivePoolObjects.Enqueue(objectToReturn);
        }

        bool HasInactveObjects() => inactivePoolObjects.Count > 0;
    }
}