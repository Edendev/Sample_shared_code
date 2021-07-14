using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.MonoBehaviours.Managers;
using Game.Interfaces;
using Game.Generic.Utiles;
using UnityEngine.UI;
using Game.MonoBehaviours.Settings;

namespace Game.MonoBehaviours
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        /// <summary>
        /// Private static reference to the camera controller instance
        /// Unaccesible
        /// </summary>
        static CameraController _S;

        const int MAX_DISTANCE_TO_TERRAIN = 50;

        [Header("PlayerControl motion parameters")]
        [Range(0.5f, 1f)]
        [SerializeField] float horizontalBounds = 0.8f;
        [Range(0.5f, 1f)]
        [SerializeField] float verticalBounds = 0.8f;
        [Range(2f, 50f)]
        [SerializeField] float maxMovementSpeed = 20f;
        [Range(0.1f, 0.9f), Tooltip("This number is multiplied by the maximum screen distance to determine the mouse dragging distance when moving the camera after click")]
        [SerializeField] float mouseDragDistanceRatioToScreenSize = 0.4f;
        [Range(1f, 5f)]
        [SerializeField] float minOrthographicSize = 1f;
        [Range(6f, 20f)]
        [SerializeField] float maxOrthographicSize = 20f;
        [Range(1f, 10f)]
        [SerializeField] float zoomSpeed = 20f;
        [Range(0f, 2f)]
        [SerializeField] float zoomMaxBounceTime = 1f;
        [Range(0f, 1f)]
        [SerializeField] float zoomBounceIncreaseRate = 0.1f;
        [Range(10f, 360f), Tooltip("Maximum rotation speed in degrees when rotating the camera horizontally")]
        [SerializeField] float maxRotationSpeed = 100f;
        [Range(10f, 90f), Tooltip("Minimum angle in degrees when rotating the camera vertically")]
        [SerializeField] float min_Y_angle = 20f;
        [Range(10f, 90f), Tooltip("Maximum angle in degrees when rotating the camera vertically")]
        [SerializeField] float max_Y_angle = 60f;

        [SerializeField] float mouseDragPath;
        /// <summary>
        /// This is used to rotate the camera easilly when using the mouse right click
        /// </summary>
        [SerializeField] Transform cameraRotator;
        [SerializeField] Transform cameraMoveable;

        [Header("RemoteControl motion parameters")]
        [Tooltip("The camera will follow this speed curve while moving to a target position.")]
        [SerializeField] AnimationCurve movingToTargetSpeedCurve;

        Vector3 clickPosition;
        float actualMovementSpeed;
        float zoomingTime = 0f;
        int zoomingDirection = 1;
        float initialOrthographicSize;

        /// <summary>
        /// State machine
        /// </summary>
        public enum State
        {
            Uninitialized, None, PlayerControl, RemoteControl
        }

        [SerializeField] State currentState = State.Uninitialized;

        /// <summary>
        /// Sends a message when state changes containing the new state
        /// </summary>
        [SerializeField] private Events.CameraStateEvent onStateChanges = new Events.CameraStateEvent();
        [SerializeField] private Events.VoidEvent onTargetReached = new Events.VoidEvent();
        [SerializeField] private Events.VoidEvent onZoomingDone = new Events.VoidEvent();
        [SerializeField] private Events.VoidEvent onRotationDone = new Events.VoidEvent();

        private void Awake()
        {
            // Makes the camera controller a singleton
            if (_S != null)
            {
                Destroy(gameObject);
            }
            else
            {
                _S = this;
            }

            // Determine mouse drag path according to screen size
            mouseDragPath = Camera.main.pixelRect.max.magnitude * mouseDragDistanceRatioToScreenSize;

            // Set initial orthographicSize
            initialOrthographicSize = Camera.main.orthographicSize;

            // Initialize state to player control
            StartCoroutine(ChangeState(State.PlayerControl));
        }

        private void Start()
        {
            // Subscribe to world events manager
            WorldEventsManager.OnEventExecuted().AddListener(OnWorldEventExecuted);
            WorldEventsManager.OnEventFinished().AddListener(OnWorldEventFinished);
        }

        private void OnDestroy()
        {
            // Unsubscrbie to world events manager
            WorldEventsManager.OnEventExecuted().RemoveListener(OnWorldEventExecuted);
            WorldEventsManager.OnEventFinished().RemoveListener(OnWorldEventFinished);
        }

        private void Update()
        {
            switch(currentState)
            {
                case State.PlayerControl:
                    OnPlayerControl();
                    break;
                case State.RemoteControl:
                    OnRemoteControl();
                    break;
            }
        }
        private void OnPlayerControl()
        {
            // Camera rotation initialize
            if (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E))
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.forward, out hit, 50f, LayerMask.GetMask(LayerMasks.TERRAIN_LAYER_MASK)))
                {
                    transform.SetParent(null);
                    cameraRotator.position = hit.point;
                    transform.SetParent(cameraMoveable);
                    cameraMoveable.SetParent(cameraRotator);
                }
            }
            // Camera rotation with mouse
            if (Input.GetKey(KeyCode.Mouse1))
            {
                if (Input.GetAxis("Mouse X") != 0f)
                    cameraRotator.rotation = Quaternion.Euler(new Vector3(0f, cameraRotator.rotation.eulerAngles.y + Input.GetAxis("Mouse X") * GameSettings.S.CameraRotationSensitivity() * maxRotationSpeed * Time.deltaTime, 0f));
                if (Input.GetAxis("Mouse Y") != 0f)
                {
                    float newAngle = Mathf.Clamp(transform.localRotation.eulerAngles.x -Input.GetAxis("Mouse Y") * GameSettings.S.CameraRotationSensitivity() * maxRotationSpeed /2f * Time.deltaTime, min_Y_angle, max_Y_angle);
                    transform.localRotation = Quaternion.Euler(new Vector3(newAngle, 0f, 0f));
                }
            }
            // Camera rotation with keyboard input
            if (Input.GetKey(KeyCode.Q))
                cameraRotator.rotation = Quaternion.Euler(new Vector3(0f, cameraRotator.rotation.eulerAngles.y + GameSettings.S.CameraRotationSensitivity() * maxRotationSpeed * Time.deltaTime, 0f));
            if (Input.GetKey(KeyCode.E))
                cameraRotator.rotation = Quaternion.Euler(new Vector3(0f, cameraRotator.rotation.eulerAngles.y - GameSettings.S.CameraRotationSensitivity() * maxRotationSpeed * Time.deltaTime, 0f));

            actualMovementSpeed = maxMovementSpeed * (Camera.main.orthographicSize / initialOrthographicSize);

            // Camera motion
            if (Input.GetKey(KeyCode.A))
                cameraMoveable.Translate(-Vector3.right * actualMovementSpeed * GameSettings.S.CameraMotionSensitivity() * Time.deltaTime);
            if (Input.GetKey(KeyCode.W))
                cameraMoveable.Translate(Vector3.forward * actualMovementSpeed * GameSettings.S.CameraMotionSensitivity() * Time.deltaTime);
            if (Input.GetKey(KeyCode.S))
                cameraMoveable.Translate(-Vector3.forward * actualMovementSpeed * GameSettings.S.CameraMotionSensitivity() * Time.deltaTime);
            if (Input.GetKey(KeyCode.D))
                cameraMoveable.Translate(Vector3.right * actualMovementSpeed * GameSettings.S.CameraMotionSensitivity() * Time.deltaTime);

            // Camera scroll in and out
            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                // Bouncing time calculation and direction determination
                zoomingTime = Mathf.Clamp(zoomingTime + zoomBounceIncreaseRate, 0f, zoomMaxBounceTime);
                if (Input.GetAxis("Mouse ScrollWheel") > 0) zoomingDirection = -1;
                if (Input.GetAxis("Mouse ScrollWheel") < 0) zoomingDirection = 1;
            }

            if (zoomingTime > 0f)
            {
                GameManager.MainCamera().orthographicSize = Mathf.Clamp(GameManager.MainCamera().orthographicSize + zoomSpeed * zoomingDirection * Time.deltaTime, minOrthographicSize, maxOrthographicSize);
                zoomingTime -= Time.deltaTime;
            }

            // Teleport camera to player
            if (Input.GetKeyDown(KeyCode.C))
            {
                StartCoroutine(MoveToTarget(Player.S.transform, 5f, true));
            }
        }
        private void OnRemoteControl() { }

        #region StateMachine

        IEnumerator ChangeState(State newState)
        {
            if (newState == currentState)
                yield break;

            // Save old state and set current state as changing 
            State oldState = currentState;
            currentState = State.None;

            // End old state
            yield return EndStateTasks(oldState);

            // Assign new state as current state
            currentState = newState;

            // Send message of state change
            onStateChanges?.Invoke(newState);

            // Start new state
            yield return StartStateTasks(newState);
        }
        IEnumerator EndStateTasks(State oldState)
        {
            switch (oldState)
            {
                case State.PlayerControl:
                    break;
                case State.RemoteControl:
                    break;
            }

            yield return null;
        }
        IEnumerator StartStateTasks(State newState)
        {
            switch (newState)
            {
                case State.PlayerControl:
                    Camera.main.orthographicSize = initialOrthographicSize;
                    break;
                case State.RemoteControl:
                    break;
            }

            yield return null;
        }

        #endregion

        #region RemoteControl

        public bool MoveCameraToTarget(Transform target, float speed, bool instantly = false)
        {
            if (currentState != State.RemoteControl)
                return false;

            StopAllCoroutines();
            StartCoroutine(MoveToTarget(target, speed, instantly));
            return true;
        }
        IEnumerator MoveToTarget(Transform target, float speed, bool instantly = false)
        {
            RaycastHit hit;
            if (!Physics.Raycast(transform.position, transform.forward, out hit, 50f, LayerMask.GetMask(LayerMasks.TERRAIN_LAYER_MASK)))
                yield break;
            
            // Determine direction from camera "Y" position
            Vector3 hitAtCamerY = hit.point;
            hitAtCamerY.y = cameraMoveable.position.y;
            Vector3 cameraOffset = hitAtCamerY - cameraMoveable.position;
            Vector3 targetAtCameraY = target.position;
            targetAtCameraY.y = cameraMoveable.position.y;
            Vector3 direction = targetAtCameraY - (cameraMoveable.position + cameraOffset);

            if (!instantly)
            {
                float initialDirMagnitude = direction.magnitude;
                while (direction.magnitude > 0.01f)
                {

                    targetAtCameraY = target.position;
                    targetAtCameraY.y = cameraMoveable.position.y;
                    direction = targetAtCameraY - (cameraMoveable.position + cameraOffset);
                    cameraMoveable.position = cameraMoveable.position + direction.normalized * speed * movingToTargetSpeedCurve.Evaluate(1f - direction.magnitude / initialDirMagnitude) * Time.deltaTime;

                    yield return null;
                }
            }
            else
            {
                cameraMoveable.position = targetAtCameraY - cameraOffset;
            }

            onTargetReached?.Invoke();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="depth">Depth is the orthographicSize</param>
        /// <returns></returns>
        public bool ZoomToTarget(Transform target, float speed, float zoomSpeed, float depth)
        {
            if (currentState != State.RemoteControl)
                return false;

            StopAllCoroutines();

            // Move to and zoom in target
            StartCoroutine(MoveToTarget(target, speed));
            StartCoroutine(ZoomingToTarget(target, speed, zoomSpeed, depth));
            return true;
        }
        IEnumerator ZoomingToTarget(Transform target, float speed, float zoomSpeed, float depth)
        {
            zoomSpeed = (Camera.main.orthographicSize - depth) > 0 ? -zoomSpeed : zoomSpeed;
            while (Camera.main.orthographicSize != depth)
            {
                Camera.main.orthographicSize += zoomSpeed * Time.deltaTime;
                float currentValue = (zoomSpeed > 0) ? depth - Camera.main.orthographicSize : Camera.main.orthographicSize - depth;
                if (currentValue <= 0f)
                    Camera.main.orthographicSize = depth;

                yield return null;
            }

            onZoomingDone?.Invoke();
        }
        /// <summary>
        /// Rotate camera to the specified angle
        /// </summary>
        /// <param name="angle">Angle is given in degrees and represents the Y euler angle</param>
        /// <param name="speed"></param>
        /// <returns></returns>
        public bool RotateToAngle(float angle, float rotationSpeed, bool instantly = true)
        {
            if (currentState != State.RemoteControl)
                return false;

            StopAllCoroutines();

            // Rotate to angle
            StartCoroutine(RotatingToAngle(angle, rotationSpeed, instantly));
            return true;
        }
        IEnumerator RotatingToAngle(float angle, float rotationSpeed, bool instantly = true)
        {
            if (!instantly)
            {
                rotationSpeed = Mathf.Abs(cameraRotator.eulerAngles.y) + Mathf.Abs(angle) > 180 ? rotationSpeed * Mathf.Sign(angle) : -rotationSpeed * Mathf.Sign(angle);
                while (cameraRotator.eulerAngles.y - angle > 0.01f)
                {
                    cameraRotator.rotation = Quaternion.Euler(new Vector3(0, cameraRotator.rotation.eulerAngles.y + rotationSpeed * Time.deltaTime, 0));
                    float currentValue = (rotationSpeed > 0) ? angle - cameraRotator.eulerAngles.y : cameraRotator.eulerAngles.y - angle;
                    if (currentValue <= 0f)
                        cameraRotator.rotation = Quaternion.Euler(new Vector3(0, angle, 0));

                    yield return null;
                }
            }
            else
            {
                cameraRotator.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
            }


            onRotationDone?.Invoke();
        }

        #endregion

        #region Handle_WorldEvents

        void OnWorldEventExecuted(IEvent e)
        {
            EventInfo eventInfo = e.GetEventInfo();
            if (eventInfo.CameraRemoteControl)
            {
                StartCoroutine(ChangeState(State.RemoteControl));
            }
        }
        void OnWorldEventFinished(IEvent e)
        {
            EventInfo eventInfo = e.GetEventInfo();
            if (eventInfo.CameraRemoteControl)
            {
                StartCoroutine(ChangeState(State.PlayerControl));
            }
        }

        #endregion

        #region Accessors

        public Events.VoidEvent OnZoomingDone() => onZoomingDone;
        public Events.VoidEvent OnTargetReached() => onTargetReached;
        public Events.VoidEvent OnRotationDone() => onRotationDone;
        public Events.CameraStateEvent OnStateChanges() => onStateChanges;
        public static int Get_MAX_DISTANCE_TO_TERRAIN() => MAX_DISTANCE_TO_TERRAIN;

        #endregion
    }
}
