using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Generic.Utiles;

namespace Game.Interfaces
{
    [System.Serializable]
    public class EventInfo
    {
        /// <summary>
        ///  A focused event is executed and has to be ended before executing any other event.
        /// </summary>
        public bool Focused;
        public bool StopEnemies;
        public bool StopPlayer;
        public bool CameraRemoteControl;
    }

    public interface IEvent
    {
        EventInfo GetEventInfo();
        IEnumerator Execute();
        Events.IEventEvent OnEventFinished();
    }
}
