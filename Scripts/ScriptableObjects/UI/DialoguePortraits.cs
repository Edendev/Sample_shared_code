using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.ScriptableObjects.Core;

namespace Game.ScriptableObjects.UI
{
    public class DialoguePortraits : GameSO
    {
        public enum Emotion
        {
            Normal,
            Frustrated,
        }

        public readonly Dictionary<Emotion, Sprite> LookAtSprites = new Dictionary<Emotion, Sprite>();

        [Header("Character face portraits")]
        public Sprite Normal;
        public Sprite Frustrated;

        private void OnEnable()
        {
            LookAtSprites.Add(Emotion.Normal, Normal);
            LookAtSprites.Add(Emotion.Frustrated, Frustrated);
        }

        /// <summary>
        /// Returns null if sprite does not exists
        /// </summary>
        /// <param name="emotion"></param>
        /// <returns></returns>
        public Sprite GetPortrait(Emotion emotion)
        {
            return LookAtSprites[emotion];
        }
    }

}
