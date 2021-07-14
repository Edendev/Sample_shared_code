using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Generic.Utiles
{
    public static class Utiles
    {
        /// <summary>
        /// Return all types subclass of generic type T
        /// </summary>
        /// <returns></returns>
        public static System.Type[] GetAllTypesSubclassOfT<T>()
        {
            System.Type type = typeof(T);

            System.Type[] allTypes = type.Assembly.GetTypes();

            List<System.Type> typesList = new List<System.Type>();

            for (int i = 0; i < allTypes.Length; i++)
            {
                if (allTypes[i].IsSubclassOf(type) && !allTypes[i].IsAbstract)
                {
                    typesList.Add(allTypes[i]);
                }
            }

            return typesList.ToArray();
        }

        /// <summary>
        /// Return all type names subclass of generic type T as a GUIContent
        /// </summary>
        /// <returns></returns>
        public static GUIContent[] GetAllTypeNamesSubclassOfT<T>()
        {
            System.Type type = typeof(T);

            System.Type[] allTypes = type.Assembly.GetTypes();

            List<GUIContent> GUIContentsList = new List<GUIContent>();

            for (int i = 0; i < allTypes.Length; i++)
            {
                if (allTypes[i].IsSubclassOf(type) && !allTypes[i].IsAbstract)
                {
                    GUIContentsList.Add(new GUIContent(allTypes[i].Name));
                }
            }

            return GUIContentsList.ToArray();
        }
    }
}

