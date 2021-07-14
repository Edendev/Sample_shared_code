using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Generic.Utiles;
using Game.Interfaces;
using Game.MonoBehaviours.WorldEvents;
using Game.Generic.Data;

namespace Game.MonoBehaviours.Managers
{
    public class WorldEventsManager : Manager<WorldEventsManager>
    {
        /// <summary>
        /// Queue of events that must be executed
        /// </summary>
        static Queue<IEvent> eventsToExecute = new Queue<IEvent>();
        static Events.IEventEvent onEventExecuted = new Events.IEventEvent();
        static Events.IEventEvent onEventFinished = new Events.IEventEvent();

        bool executingEvents = false;

        private void Start()
        {
            // Subscribe to game state change
            GameManager.OnStateChanges().AddListener(OnGameStateChanges);
        }
        private void OnDisable()
        {
            // Unsubscribe to game state change
            GameManager.OnStateChanges().RemoveListener(OnGameStateChanges);
        }

        void OnGameStateChanges(GameManager.State newState) {}

        private void Update()
        {
            if (GameManager.CURRENTSTATE() != GameManager.State.Running)
                return;

            if (!executingEvents)
            {
                if (eventsToExecute.Count > 0)
                    StartCoroutine(ExecuteCurrentEvents());
            }
        }

        IEnumerator ExecuteCurrentEvents()
        {
            executingEvents = true;

            while(eventsToExecute.Count > 0)
            {
                IEvent currentEvent = eventsToExecute.Dequeue();
                currentEvent.OnEventFinished().AddListener(EventFinishedCallback);

                onEventExecuted?.Invoke(currentEvent);

                if (currentEvent.GetEventInfo().Focused)
                {
                    yield return StartCoroutine(currentEvent.Execute());
                }
                else
                {
                    StartCoroutine(currentEvent.Execute());
                }
            }

            executingEvents = false;
        }

        void EventFinishedCallback(IEvent e)
        {
            onEventFinished.Invoke(e);
            e.OnEventFinished().RemoveListener(EventFinishedCallback);
        }

        public static void AddWorldEventToQueue(IEvent e) => eventsToExecute.Enqueue(e);

        #region Accessors

        public static Events.IEventEvent OnEventExecuted() => onEventExecuted;
        public static Events.IEventEvent OnEventFinished() => onEventFinished;

        #endregion
    }
}
