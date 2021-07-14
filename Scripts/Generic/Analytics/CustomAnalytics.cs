using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_CLOUD_SERVICES_ANALYTICS
using UnityEngine.Analytics;
#endif

namespace Game.Generic.Analytics
{
    public static class CustomAnalytics
    {
        /// <summary>
        /// Test function for sending data to analytics
        /// </summary>
        public static void SendDestroyedObject(GameObject destroyed)
        {
#if ENABLE_CLOUD_SERVICES_ANALYTICS
            UnityEngine.Analytics.Analytics.CustomEvent("destroyedObject", new Dictionary<string, object>
           {
                {"name" , destroyed.name},
                {"position" , destroyed.transform.position}
           });
#endif
        }
    }
}

