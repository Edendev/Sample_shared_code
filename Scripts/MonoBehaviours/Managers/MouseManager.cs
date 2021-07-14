//#define DEBUG_MOUSEMANAGER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.MonoBehaviours.Particles;
using Game.ScriptableObjects.UI;
using Game.Generic.Utiles;
using Game.Interfaces;
using Game.Interfaces.UI;

namespace Game.MonoBehaviours.Managers
{
    public class MouseManager : Manager<MouseManager>
    {
        // Particle effects
        [Header("Set in Inspector")]
        [SerializeField] GameObject ClickEnv_VFX;
        [SerializeField] MouseOverTargetEffect MouseOverTarget_VFX;

        static Events.Vector3Event MouseClickEnvironmentEvent = new Events.Vector3Event();
        static Events.GameObjectEvent MouseClickAttackableEvent = new Events.GameObjectEvent();
        static Events.GameObjectEvent MouseClickInteractableEvent = new Events.GameObjectEvent();

        public enum State
        {
            Enabled, Disabled
        }

        State currentState;

        public enum ActionOnClick
        {
            Nothing,
            MoveToDestination,
            Interact,
            Attack
        }

        ActionOnClick currentActionOnClick = ActionOnClick.Nothing;

        // Cursors animation
        [SerializeField] CursorTextures cursorTextures;
        CursorTextures.CursorSettings currentCursorSettings;
        float cursorAnimationFrameRate = 0.2f;
        float cursorAnimationTimer = 0f;
        int cursorTextureIndex = 0;
        GameObject currentCursorTarget;

        // On mouse over target effect
        MouseOverTargetEffect currentOnMouseOver_VFX_object;

        private void Start()
        {
            // For testing purposes, when starting the game from another scene rather than level
            if (SceneGameManager.CurrentSceneType() == SceneGameManager.SceneType.Level)
                Player.S.OnPlayerDead().AddListener(OnPlayerIsDead);

            GameManager.OnStateChanges().AddListener(OnGameStateChanges);
            WorldEventsManager.OnEventExecuted().AddListener(OnWorldEventExecuted);
            WorldEventsManager.OnEventFinished().AddListener(OnWorldEventFinished);
        }

        void OnGameStateChanges(GameManager.State newState)
        {
            if (SceneGameManager.CurrentSceneType() != SceneGameManager.SceneType.Level)
                return;

            switch(newState)
            {
                case GameManager.State.Running:
                    currentState = State.Enabled;
                    break;
                case GameManager.State.PreSceneLoad:
                    Player.S.OnPlayerDead().RemoveListener(OnPlayerIsDead);
                    break;
                case GameManager.State.PostSceneLoad:
                    Player.S.OnPlayerDead().AddListener(OnPlayerIsDead);
                    break;
            }

            if (newState != GameManager.State.Running)
                currentState = State.Disabled;
        }
        void OnPlayerIsDead()
        {
            currentState = State.Disabled;
        }

        #region Handle_WorldEvents

        void OnWorldEventExecuted(IEvent e)
        {
            EventInfo eventInfo = e.GetEventInfo();
            if (eventInfo.StopPlayer)
            {
                currentState = State.Disabled;
            }
        }
        void OnWorldEventFinished(IEvent e)
        {
            EventInfo eventInfo = e.GetEventInfo();
            if (eventInfo.StopPlayer)
            {
                currentState = State.Enabled;
            }
        }

        #endregion

        private void Update()
        {
            if (currentState == State.Disabled && SceneGameManager.CurrentSceneType() == SceneGameManager.SceneType.Level)
                return;

            // Raycast into scene
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, CameraController.Get_MAX_DISTANCE_TO_TERRAIN(), LayerMasks.ClickablesMask()))
            {
                IClickable clickable = null;
                hit.transform.TryGetComponent<IClickable>(out clickable);

                if (clickable != null)
                {
                    // Check if object has interactables
                    IInteractable[] interactables = hit.transform.gameObject.GetComponentsInChildren<IInteractable>();
                    if (interactables != null && interactables.Length > 0)
                    {
                        currentActionOnClick = ActionOnClick.Interact;
                    }
                    else
                    {
                        // Check if object has attackables
                        IAttackable[] attackables = hit.transform.gameObject.GetComponentsInChildren<IAttackable>();
                        if (attackables != null && attackables.Length > 0)
                        {
                            currentActionOnClick = ActionOnClick.Attack;
                        }
                    }
                }
                else
                {
                    currentActionOnClick = ActionOnClick.MoveToDestination;
                }

                // Update cursor settings according to gameObject being inspected
                UpdateCursorSettings(hit.transform.gameObject, clickable);

                // Update mouse over VFX
                UpdateMouseOver_VFX(hit.transform.gameObject, clickable);

                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    // Trigger OnClick 
                    if (clickable != null)
                        clickable.OnClick();

                    TriggerActionOnClick(hit);
                }

                // Animate cursor
                if (Time.time - cursorAnimationTimer > cursorAnimationFrameRate)
                {
                    cursorAnimationTimer = Time.time;
                    Cursor.SetCursor(currentCursorSettings.textures[cursorTextureIndex], currentCursorSettings.hotSpot, CursorMode.ForceSoftware);
                    cursorTextureIndex++;
                    if (cursorTextureIndex >= currentCursorSettings.textures.Length)
                        cursorTextureIndex = 0;
                }

#if DEBUG_MOUSEMANAGER
                Debug.DrawLine(hit.point, hit.point + Vector3.up * 2f, Color.white, 1f);
#endif
            }
        }

        void UpdateCursorSettings(GameObject target, IClickable clickable)
        {
            if (target == currentCursorTarget)
                return;

            // Update mouse over VFX
            UpdateMouseOver_VFX(target, clickable);

            currentCursorTarget = target;

            // Reset animation parameters
            cursorAnimationTimer = Time.time - cursorAnimationFrameRate; // Change immediately
            cursorTextureIndex = 0;

            if (clickable != null)
                currentCursorSettings = cursorTextures.GetCursorSettings(clickable.GetCursorType());
            else
                currentCursorSettings = cursorTextures.GetCursorSettings(CursorTextures.CursorType.MoveTo);
        }
        void UpdateMouseOver_VFX(GameObject target, IClickable clickable)
        {
            if (target == currentCursorTarget)
            {
                if (currentOnMouseOver_VFX_object != null)
                {
                    currentOnMouseOver_VFX_object.transform.position = target.transform.position + clickable.MouseOver_VFX_offset();
                    return;
                }
            }

            if (currentOnMouseOver_VFX_object != null)
                    DestroyImmediate(currentOnMouseOver_VFX_object.gameObject);

            if (target.layer != LayerMask.NameToLayer(LayerMasks.TERRAIN_LAYER_MASK) && clickable != null)
            {
                currentOnMouseOver_VFX_object = Instantiate(MouseOverTarget_VFX, target.transform.position + clickable.MouseOver_VFX_offset(), Quaternion.LookRotation(Vector3.up));
                currentOnMouseOver_VFX_object.SetRadius(clickable.MouseOver_VFX_radius());
            }
        }
        void TriggerActionOnClick(RaycastHit hit)
        {
            switch(currentActionOnClick)
            {
                case ActionOnClick.MoveToDestination:
                    if (ClickEnv_VFX != null) Instantiate(ClickEnv_VFX, hit.point, Quaternion.LookRotation(Vector3.up)); // Animation effect on click in environment
                    MouseClickEnvironmentEvent?.Invoke(hit.point);
                    break;
                case ActionOnClick.Interact:
                    MouseClickInteractableEvent?.Invoke(hit.transform.gameObject);
                    break;
                case ActionOnClick.Attack:
                    MouseClickAttackableEvent?.Invoke(hit.transform.gameObject);
                    break;
            }
        }

        #region Accessors

        public static Events.Vector3Event GetMouseClickEnvironmentEvent() => MouseClickEnvironmentEvent;
        public static Events.GameObjectEvent GetMouseClickAttackableEvent() => MouseClickAttackableEvent;
        public static Events.GameObjectEvent GetMouseClickInteractableEvent() => MouseClickInteractableEvent;

        #endregion
    }    
}

