using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Interfaces.UI;
using Game.ScriptableObjects.UI;

namespace Game.MonoBehaviours.UI.Clickables
{
    public class Clickable : MonoBehaviour, IClickable
    {
        [Header("External inputs")]
        [SerializeField] float mouseOver_VFX_radius = 0.05f;
        [SerializeField] Vector3 mouseOver_VFX_offset = Vector3.zero;
        [SerializeField] CursorTextures.CursorType cursorType;

        public virtual CursorTextures.CursorType GetCursorType()
        {
            if (cursorType == CursorTextures.CursorType.Attack)
                return (Player.S.HasEquippedWeapon() == true) ? CursorTextures.CursorType.Attack : CursorTextures.CursorType.Attack_N;

            if (cursorType == CursorTextures.CursorType.JumpWall)
                return (Player.S.HasEquippedRope() == true) ? CursorTextures.CursorType.JumpWall : CursorTextures.CursorType.JumpWall_N;

            return cursorType;
        }

        public virtual Vector3 MouseOver_VFX_offset() => mouseOver_VFX_offset;
        public virtual float MouseOver_VFX_radius() => mouseOver_VFX_radius;

        public virtual void OnClick() { }
    }
}

