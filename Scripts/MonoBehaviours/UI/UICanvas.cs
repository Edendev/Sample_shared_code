using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.MonoBehaviours
{
    public class UICanvas : MonoBehaviour
    {
        // Static parameters
        static CanvasScaler canvasScaler;
        static float refAspect;
        static float refWidth, refHeight;

        private void Awake()
        {
            canvasScaler = GetComponent<CanvasScaler>();

            // Determine aspect ratio and reference width, height ratios
            refWidth = canvasScaler.referenceResolution.x;
            refHeight = canvasScaler.referenceResolution.y;
            refAspect = canvasScaler.referenceResolution.x / canvasScaler.referenceResolution.y;
        }

        #region Accessors

        public static float RefAspect() => refAspect;
        public static float RefWidth() => refWidth;
        public static float RefHeight() => refHeight;

        #endregion
    }
}