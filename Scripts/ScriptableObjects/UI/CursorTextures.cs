using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.ScriptableObjects.Core;

namespace Game.ScriptableObjects.UI
{
    public class CursorTextures : GameSO
    {
        public enum CursorType
        {
            MoveTo,
            Attack,
            Attack_N,
            PickUp_item,
            OpenDoor,
            JumpWall,
            JumpWall_N,
        }

        [System.Serializable]
        public struct CursorSettings
        {
            public Texture2D[] textures;
            public float frameRate;
            public Vector2 hotSpot;
        }

        public readonly Dictionary<CursorType, CursorSettings> LookAtTextures = new Dictionary<CursorType, CursorSettings>();

        [Header("Cursor texture")]
        public CursorSettings MoveTo;
        /// <summary>
        /// Textures animation when attack is allowed on target
        /// </summary>
        public CursorSettings Attack;
        /// <summary>
        /// Textures animation when attack is not allowed on target
        /// </summary>
        public CursorSettings Attack_N;
        public CursorSettings PickUp_item;
        public CursorSettings OpenDoor;
        public CursorSettings JumpWall;
        public CursorSettings JumpWall_N;

        private void OnEnable()
        {
            if (!LookAtTextures.ContainsKey(CursorType.MoveTo))
                LookAtTextures.Add(CursorType.MoveTo, MoveTo);

            if (!LookAtTextures.ContainsKey(CursorType.Attack))
                LookAtTextures.Add(CursorType.Attack, Attack);

            if (!LookAtTextures.ContainsKey(CursorType.Attack_N))
                LookAtTextures.Add(CursorType.Attack_N, Attack_N);

            if (!LookAtTextures.ContainsKey(CursorType.PickUp_item))
                LookAtTextures.Add(CursorType.PickUp_item, PickUp_item);

            if (!LookAtTextures.ContainsKey(CursorType.OpenDoor))
                LookAtTextures.Add(CursorType.OpenDoor, OpenDoor);

            if (!LookAtTextures.ContainsKey(CursorType.JumpWall))
                LookAtTextures.Add(CursorType.JumpWall, JumpWall);

            if (!LookAtTextures.ContainsKey(CursorType.JumpWall_N))
                LookAtTextures.Add(CursorType.JumpWall_N, JumpWall_N);
        }

        /// <summary>
        /// Returns null if sprite does not exists
        /// </summary>
        /// <param name="emotion"></param>
        /// <returns></returns>
        public CursorSettings GetCursorSettings(CursorType cursorType) => LookAtTextures[cursorType];
        public Texture2D[] GetTextures(CursorType cursorType) => LookAtTextures[cursorType].textures;
        public float GetFrameRate(CursorType cursorType) => LookAtTextures[cursorType].frameRate;
    }

}
