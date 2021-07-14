using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Game.Generic.PathFinding;
using Game.Interfaces;
using Game.Generic.Data;
using Game.Generic.Utiles;
using Game.MonoBehaviours.UI;
using Game.MonoBehaviours.Settings;

namespace Game.MonoBehaviours.Managers
{
    [RequireComponent(typeof(SceneGameManager))]
    public class GameManager : Manager<GameManager>
    {
        /// <summary>
        /// Store all destroyable entities
        /// </summary>
        static List<INode> _NODES = new List<INode>();

        [System.Flags]
        public enum State
        {
            None = 0,
            PreSceneLoad = 1,
            LoadingScene = 2,
            PostSceneLoad = 4,
            Running = 8,
            InGameMenu = 16,
            Paused = 32,
            LevelWin = 64,
            GameOver = 128,
        }

        static State _CURRENTSTATE = State.None;

        [SerializeField] static Events.GameManagerStatEvent _ONSTATECHANGES = new Events.GameManagerStatEvent();
        [SerializeField] static Events.VoidEvent _ONSCREENCHANGES = new Events.VoidEvent();

        // Save reference to the scene game manager (only gama manager can access it)
        SceneGameManager sceneGameManager;

        // Required components
        static Camera mainCamera; // Reference to the main camera
        float cachedOrthographicSize;
        float cachedAspect;

        /// <summary>
        /// Index of current level
        /// </summary>
        [SerializeField] int currentLevelIndex = 1;

        bool canOpenInGameMenu = true;

        private void Start()
        {
            // Get main camera
            mainCamera = Camera.main;

            // Get scene manager and start post load scene tasks for current scene
            sceneGameManager = GetComponent<SceneGameManager>();
            PostLoadSceneTasks(SceneGameManager.CurrentSceneType());

            // Load game settings
            GameSettings.S.LoadDataFromFile();

            // Register to world events manager
            WorldEventsManager.OnEventExecuted().AddListener(OnWorldEventExecuted);
            WorldEventsManager.OnEventFinished().AddListener(OnWorldEventFinished);

            // Change state to Running
            StartCoroutine(ChangeState(State.Running));
        }

        private void Update()
        {
            // Check whether the screen has changed
            CheckIfScreenHasChanged();

            switch(_CURRENTSTATE)
            {
                case State.Running:
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        if (canOpenInGameMenu)
                            StartCoroutine(ChangeState(State.InGameMenu));
                    }
                    break;
                case State.InGameMenu:
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        StartCoroutine(ChangeState(State.Running));
                    }
                    break;
            }
        }

        void CheckIfScreenHasChanged()
        {
            if (mainCamera == null)
                return;

            if (mainCamera.aspect != cachedAspect || mainCamera.orthographicSize != cachedOrthographicSize)
            {
                // Update cached values and send message
                cachedAspect = mainCamera.aspect;
                cachedOrthographicSize = mainCamera.orthographicSize;
                _ONSCREENCHANGES?.Invoke();
            }
        }

        #region StateMachine

        IEnumerator ChangeState(State newState)
        {
            if (newState == _CURRENTSTATE)
                yield break;

            // Save old state and set current state as none 
            State oldState = _CURRENTSTATE;
            _CURRENTSTATE = State.None;

            // End old state
            yield return EndStateTasks(oldState);

            // Assign new state as current state
            _CURRENTSTATE = newState;

            // Send message of state change
            _ONSTATECHANGES?.Invoke(newState);

            // Start new state
            yield return StartStateTasks(newState);
        }
        IEnumerator EndStateTasks(State oldState)
        {
            switch (oldState)
            {
                case State.Running:
                    break;
                case State.Paused:
                    break;
            }

            yield return null;
        }
        IEnumerator StartStateTasks(State newState)
        {
            switch (newState)
            {
                case State.Running:
                    Time.timeScale = 1f;
                    break;
                case State.Paused:
                    Time.timeScale = 0f;
                    break;
                case State.PreSceneLoad:
                    PreloadSceneTasks(SceneGameManager.CurrentSceneType());
                    break;
                case State.LoadingScene:
                    break;
                case State.PostSceneLoad:
                    PostLoadSceneTasks(SceneGameManager.CurrentSceneType());
                    break;
                case State.InGameMenu:
                    break;
                case State.LevelWin:
                    break;
                case State.GameOver:
                    break;
            }

            yield return null;
        }

        #endregion

        #region LoadSceneTasks

        void PreloadSceneTasks(SceneGameManager.SceneType sceneType)
        {
            if (SceneGameManager.CurrentSceneType() == SceneGameManager.SceneType.Boot)
            {
                UIBootMenu.S.GetNewGameButton().UnregisterCallback<ClickEvent>(ev => StartNewGame());
                UIBootMenu.S.GetContinueButton().UnregisterCallback<ClickEvent>(ev => ContinueGame());
                UIBootMenu.S.GetExitButton().UnregisterCallback<ClickEvent>(ev => ExitGame());
                UIBootMenu.S.GetSettingsButton().SetEnabled(false);
            }
            else if (SceneGameManager.CurrentSceneType() == SceneGameManager.SceneType.Level)
            {
                // Unsubscribe to in game menu restart and quit buttons 
                UIInGameMenu.S.ExitButtonClickEvent().RemoveListener(ExitGame);
                UIInGameMenu.S.BackToBootButtonClickEvent().RemoveListener(BackToBoot);
                UIGameOverMenu.S.ExitButtonClickEvent().RemoveListener(ExitGame);
                UIGameOverMenu.S.RestartButtonClickEvent().RemoveListener(RestartGame);
                UIWinLevelMenu.S.ExitButtonClickEvent().RemoveListener(ExitGame);
                UIWinLevelMenu.S.RestartButtonClickEvent().RemoveListener(RestartGame);
                // Unsubscribe to player is dead
                Player.S.OnPlayerDead().RemoveListener(PlayerIsDead);
                // Unsubscribe to win level zone 
                WinLevelZone.S.OnPlayerEnterCollider().RemoveListener(LevelWin);
            }
        }
        void PostLoadSceneTasks(SceneGameManager.SceneType sceneType)
        {
            // Get main camera
            mainCamera = Camera.main;

            if (SceneGameManager.CurrentSceneType() == SceneGameManager.SceneType.Boot)
            {
                UIBootMenu.S.GetNewGameButton().RegisterCallback<ClickEvent>(ev => StartNewGame());
                UIBootMenu.S.GetContinueButton().RegisterCallback<ClickEvent>(ev => ContinueGame());
                UIBootMenu.S.GetExitButton().RegisterCallback<ClickEvent>(ev => ExitGame());
                UIBootMenu.S.GetSettingsButton().SetEnabled(false);
            }
            else if (SceneGameManager.CurrentSceneType() == SceneGameManager.SceneType.Level)
            {
                // Subscribe to in game menu restart and quit buttons (this goes in after loading scene actions!)
                UIInGameMenu.S.ExitButtonClickEvent().AddListener(ExitGame);
                UIInGameMenu.S.BackToBootButtonClickEvent().AddListener(BackToBoot);
                UIGameOverMenu.S.ExitButtonClickEvent().AddListener(ExitGame);
                UIGameOverMenu.S.RestartButtonClickEvent().AddListener(RestartGame);
                UIWinLevelMenu.S.ExitButtonClickEvent().AddListener(ExitGame);
                UIWinLevelMenu.S.RestartButtonClickEvent().AddListener(RestartGame);
                // Subscribe to player is dead
                Player.S.OnPlayerDead().AddListener(PlayerIsDead);
                // Subscribe to win level zone 
                WinLevelZone.S.OnPlayerEnterCollider().AddListener(LevelWin);
            }
        }

        #endregion

        void GameOver() => StartCoroutine(GameOverCoroutine());
        void PlayerIsDead() => StartCoroutine(PlayerDeadCoroutine());
        void LevelWin() => StartCoroutine(LevelWinCoroutine());
        void StartNewGame()
        {
            // Delete persistent data and reset all saved data (necessary when starting from an already running game)
            DataContainersManager.S.DeleteAllSavedFiles();
            DataContainersManager.S.ResetAllConditions();
            StartCoroutine(LoadingLevelCoroutine(2));
        }
        void ContinueGame()
        {
            // Reset all data and load from persistent data
            DataContainersManager.S.ResetAllConditions();
            DataContainersManager.S.LoadConditionsDataFromFile();
            StartCoroutine(LoadingLevelCoroutine(currentLevelIndex));
        }
        void RestartGame() => StartCoroutine(LoadingLevelCoroutine(currentLevelIndex));
        void BackToBoot() => StartCoroutine(LoadingBootCoroutine());
        void ExitGame() => StartCoroutine(ExitingCoroutine());

        IEnumerator PlayerDeadCoroutine()
        {
            yield return new WaitForSeconds(2f);
            GameOver();
        }
        IEnumerator LevelWinCoroutine()
        {
            yield return StartCoroutine(ChangeState(State.LevelWin));
        }
        IEnumerator LoadingLevelCoroutine(int levelIndex)
        {
            yield return StartCoroutine(ChangeState(State.PreSceneLoad));

            yield return StartCoroutine(ChangeState(State.LoadingScene));

            yield return StartCoroutine(sceneGameManager.LoadLevelScene(levelIndex));

            yield return StartCoroutine(ChangeState(State.PostSceneLoad));

            yield return StartCoroutine(ChangeState(State.Running));
        }
        IEnumerator LoadingBootCoroutine()
        {
            yield return StartCoroutine(ChangeState(State.PreSceneLoad));

            yield return StartCoroutine(ChangeState(State.LoadingScene));

            yield return StartCoroutine(sceneGameManager.LoadBootScene());

            yield return StartCoroutine(ChangeState(State.PostSceneLoad));

            yield return StartCoroutine(ChangeState(State.Running));
        }
        IEnumerator PauseGameCoroutine()
        {
            yield return StartCoroutine(ChangeState(State.Paused));
        }
        IEnumerator GameOverCoroutine()
        {
            yield return StartCoroutine(ChangeState(State.GameOver));
        }
        IEnumerator ExitingCoroutine()
        {
            Application.Quit();

            yield return null;
        }
        
        public static void AddNode(INode node) => _NODES.Add(node);
        public static void RemoveNode(INode node) => _NODES.Remove(node);

        #region Handle_WorldEvents

        void OnWorldEventExecuted(IEvent e)
        {
            EventInfo eventInfo = e.GetEventInfo();
            canOpenInGameMenu = !eventInfo.Focused;
        }
        void OnWorldEventFinished(IEvent e)
        {
            EventInfo eventInfo = e.GetEventInfo();
            canOpenInGameMenu = true;
        }

        #endregion

        #region Accessors

        public static List<INode> NODES() => _NODES;
        public static Events.GameManagerStatEvent OnStateChanges() => _ONSTATECHANGES;
        public static State CURRENTSTATE() => _CURRENTSTATE;
        public static float CurrentOrthographicSize() => mainCamera.orthographicSize;
        public static float CurrentPixelWidth() => mainCamera.pixelWidth;
        public static float CurrentPixelHeight() => mainCamera.pixelHeight;
        public static Transform MainCameraTransform() => mainCamera.transform;
        public static Camera MainCamera() => mainCamera;
        public static Events.VoidEvent OnScreenChanges() => _ONSCREENCHANGES;

        #endregion

        #region Data

#if UNITY_EDITOR

        public static void ResetSavedData()
        {
            DataContainersManager.S.ResetAllConditions();
        }
        public static void DeletePersistentSavedData()
        {
            DataContainersManager.S.DeleteAllSavedFiles();
        }
#endif

        #endregion
    }
}

