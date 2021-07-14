using System.Collections;
using UnityEngine;
using Game.Generic.Utiles;
using Game.MonoBehaviours.Managers;
using Game.Interfaces;
using Game.ScriptableObjects.WorldEvents.EventTriggers;
using Game.ScriptableObjects.WorldEvents.Conditions;
using Game.ScriptableObjects.WorldEvents.Reactions;

namespace Game.MonoBehaviours.WorldEvents
{
    public class WorldEvent : MonoBehaviour, IEvent
    {
        /// <summary>
        /// Trigger to execute the world event
        /// </summary>
        public EventTrigger EventTrigger;

        /// <summary>
        /// Conditions that need to be satisfied for this event to be executed
        /// </summary>
        public Condition[] Conditions = new Condition[0];

        /// <summary>
        /// Reactions that will be executed when the event is executed
        /// </summary>
        public Reaction[] Reactions = new Reaction[0];

        /// <summary>
        /// Event info sent to different MonoBehaviour to control the state of the game 
        /// </summary>
        public EventInfo EventInfo;

        public enum State
        {
            Uninitialized, Initialized, OnEventsManager, Executing, Finished
        }

        State currentState = State.Uninitialized;

        Events.IEventEvent onEventFinished = new Events.IEventEvent();

        #region MonoBehaviour

        private void Start()
        {
           if (EventTrigger != null)
           {
                EventTrigger.OnTrigger().AddListener(OnEventTrigger);
                EventTrigger.Initialize(this);
            }

            currentState = State.Initialized;
        }
        private void Update()
        {
            if (GameManager.CURRENTSTATE() != GameManager.State.Running)
                return;

            if (currentState != State.Initialized)
                return;

            if (EventTrigger != null)
            {
                EventTrigger.Check();
            }
        }

        #endregion

        /// <summary>
        /// External trigger for the event
        /// </summary>
        public void TriggerEventExternally()
        {
            if (GameManager.CURRENTSTATE() != GameManager.State.Running)
                return;

            if (currentState != State.Initialized)
                return;

            OnEventTrigger();
        }

        void OnEventTrigger()
        {
            bool canExecute = true;
            if (Conditions != null && Conditions.Length > 0)
            {
                foreach(Condition condition in Conditions)
                {
                    if (!condition.Check())
                    {
                        canExecute = false;
                        break;
                    }
                }
            }

            if (canExecute)
            {
                WorldEventsManager.AddWorldEventToQueue(this);
                currentState = State.OnEventsManager;
            }
        }

        #region IEvent

        public IEnumerator Execute()
        {
            currentState = State.Executing;

            // This is just to allow listeners to end their tasks
            yield return new WaitForSeconds(0.1f);

            if (Reactions != null && Reactions.Length > 0)
            {
                foreach(Reaction reaction in Reactions)
                {
                    yield return StartCoroutine(reaction.React());
                }
            }

            onEventFinished?.Invoke(this);

            currentState = State.Finished;

            yield return null;
        }

        public EventInfo GetEventInfo() => EventInfo;
        public Events.IEventEvent OnEventFinished() => onEventFinished;

        #endregion
    }
}

