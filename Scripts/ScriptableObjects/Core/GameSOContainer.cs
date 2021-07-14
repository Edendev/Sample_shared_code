using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.ScriptableObjects.Core
{
    public abstract class GameSOContainer<T> : GameSO
        where T : GameSO
    {
        [SerializeField] protected T[] SO_objects;

        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            GetAll_SO_objects();
#endif
        }

#if UNITY_EDITOR
        protected void GetAll_SO_objects()
        {
            SO_objects = GetAllAssetsOfType<T>();
        }
#endif

        /// <summary>
        /// Returns null if index is out of bounds or there are no SO objects
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T Get_SO_object_AtIndex(int index)
        {
            if (SO_objects == null || SO_objects.Length == 0)
                return null;

            if (index >= 0 && index < SO_objects.Length)
                return SO_objects[index];

            return null;
        }
        /// <summary>
        /// Returns null if index is out of bounds or there are no SO objects
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T Get_SO_object_BySO_Name(string objName)
        {
            if (SO_objects == null || SO_objects.Length == 0)
                return null;

            foreach(T obj in SO_objects)
            {
                if (obj.SO_Name == objName)
                    return obj;
            }

            return null;
        }
    }
}



