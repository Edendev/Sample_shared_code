using UnityEngine;
using Game.ScriptableObjects.UI;

namespace Game.Interfaces.UI
{
    public interface IClickable
    {
        /// <summary>
        ///  Returns the cursor type to be displayed when the mouse is over this clickable
        /// </summary>
        /// <returns></returns>
        CursorTextures.CursorType GetCursorType();
        /// <summary>
        ///  Determines the radius of the VFX effect displayed when the mouse is over this clickable
        /// </summary>
        /// <returns></returns>
        float MouseOver_VFX_radius();
        /// <summary>
        /// Set the offset position relative to the transform position where the effect will be displayed on the clickable (x,z coordinates)
        /// </summary>
        /// <returns></returns>
        Vector3 MouseOver_VFX_offset();
        /// <summary>
        ///  Any action triggered when click on clickable
        /// </summary>
        /// <returns></returns>
        void OnClick();
    }
}
