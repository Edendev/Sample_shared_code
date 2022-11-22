using System.Collections.Generic;
using System;
using UnityEngine;
using Game.MonoBehaviours.Player;
using Game.MonoBehaviours.AI;
using Game.ScriptableObjects.Levels;
using Game.MonoBehaviours.UI;
using Game.ScriptableObjects.Core;
using Game.Interfaces;
using Game.Generic.StateMachine;
using Game.Generic.StateMachine.GameManagerStates;
using Game.MonoBehaviours.ShopSystem;

namespace Game.MonoBehaviours.Managers
{
    [RequireComponent(typeof(WavesManager))]
    [RequireComponent(typeof(PoolingManager))]
    [RequireComponent(typeof(PlayerInputManager))]
    [RequireComponent(typeof(UIManager))]
    [RequireComponent(typeof(GameSceneManager))]
    [RequireComponent(typeof(DebugManager))]
    public class GameManager : MonoBehaviour
    {
        public static GameManager S;

        public const int MAX_NUMBER_OF_PLAYERS = 2;

        Dictionary<GameSceneManager.SceneType, GameManagerState> scenesEnterState = new Dictionary<GameSceneManager.SceneType, GameManagerState>();

        [SerializeField] GameConfigurationSO gameConfigSO;

        public class CurrentGameParameters
        {
            public string CurrentSceneName = "";
            public string SceneToLoadName = "";
            public int NumberOfPlayers = 0;
            public LevelSO Level;
            public int LevelPhaseIndex;
            public Shop[] LevelShops = new Shop[0];
            public UIPlayerInfo[] UIPlayerInfos = new UIPlayerInfo[0];
            public UILevelMenuPanel LevelMenuPanel;

            public LevelSO.LevelPhase GetLevelPhase()
            {
                return Level.GetLevelPhaseAtIndex(LevelPhaseIndex);
            }
            public bool HasNextLevelPhase()
            {
                return Level.HasLevelPhaseAtIndex(LevelPhaseIndex + 1);
            }
            public int GetNumOfLevelPhases() => Level.GetNumOfLevelPhases();
        }

        CurrentGameParameters currentGameParameters;

        #region StateMachine

        StateMachine mainStateMachine;

        GMLevelInitState gmLevelInitState;
        GMLevelPreparationState gmLevelPreparationState;
        GMLevelWaveState gmLevelWaveState;
        GMLevelRewardState gmLevelRewardState;
        GMLevelWinState gmLevelWinState;
        GMLevelLostState gmLevelLostState;
        GMLevelTutorialState gmLevelTutorialState;

        GMGameConfigPlayerRegisterState gmGameConfigPlayerRegisterState;
        GMGameConfigCharSelState gmGameConfigCharSelState;
        GMGameConfigLevelSelState gmGameConfigLevelSelState;

        GMSceneTransitionState gmSceneTransitionState;

        #endregion

        public event Action<PlayerController> OnCurrentActivePlayerRemoved;
        public event Action OnCurrentActiveNavAgentsDropToZero;
        public event Action OnCurrentActivePlayersDropToZero;

        // Game entities
        Camera mainCamera;

        [SerializeField] List<PlayerController> current_active_players = new List<PlayerController>(); // Serializefield for debugging purposes
        [SerializeField] HashSet<IPhysicsUpdatable> current_active_physUpdatables = new HashSet<IPhysicsUpdatable>(); // Serializefield for debugging purposes
        [SerializeField] HashSet<NavAgentController> current_active_navAgents = new HashSet<NavAgentController>(); // Serializefield for debugging purposes
        [SerializeField] HashSet<IThrowable> current_active_throwables = new HashSet<IThrowable>();
        [SerializeField] HashSet<IUpdatable> current_active_updatables = new HashSet<IUpdatable>(); // Serializefield for debugging purposes

        // Add and remove control by queues processed first
        Queue<PlayerController> playersToRemove = new Queue<PlayerController>();
        Queue<PlayerController> playersToAdd = new Queue<PlayerController>();
        Queue<NavAgentController> navAgentsToRemove = new Queue<NavAgentController>();
        Queue<NavAgentController> navAgentsToAdd = new Queue<NavAgentController>();
        Queue<IPhysicsUpdatable> physUpdatablesToRemove = new Queue<IPhysicsUpdatable>();
        Queue<IPhysicsUpdatable> physUpdatablesToAdd = new Queue<IPhysicsUpdatable>();
        Queue<IUpdatable> updatablesToAdd = new Queue<IUpdatable>();
        Queue<IUpdatable> updatablesToRemove = new Queue<IUpdatable>();
        Queue<IThrowable> throwablesToRemove = new Queue<IThrowable>();
        Queue<IThrowable> throwablesToAdd = new Queue<IThrowable>();

        #region MonoBehaviour

        private void Awake()
        {
            if (S == null)
            {
                S = this;
            }
            else
            {
                DestroyImmediate(this);
                return;
            }

            DontDestroyOnLoad(this);

            Pooling = GetComponent<PoolingManager>();
            PlayerInput = GetComponent<PlayerInputManager>();
            Waves = GetComponent<WavesManager>();
            UI = GetComponent<UIManager>();
            GameScene = GetComponent<GameSceneManager>();
            GameDebug = GetComponent<DebugManager>();

            mainStateMachine = new StateMachine();
            gmLevelInitState = new GMLevelInitState(this, mainStateMachine, gameConfigSO);
            gmLevelPreparationState = new GMLevelPreparationState(this, mainStateMachine, gameConfigSO);
            gmLevelWaveState = new GMLevelWaveState(this, mainStateMachine, gameConfigSO);
            gmLevelRewardState = new GMLevelRewardState(this, mainStateMachine, gameConfigSO);
            gmLevelWinState = new GMLevelWinState(this, mainStateMachine, gameConfigSO);
            gmLevelLostState = new GMLevelLostState(this, mainStateMachine, gameConfigSO);
            gmLevelTutorialState = new GMLevelTutorialState(this, mainStateMachine, gameConfigSO);

            gmGameConfigPlayerRegisterState = new GMGameConfigPlayerRegisterState(this, mainStateMachine, gameConfigSO);
            gmGameConfigCharSelState = new GMGameConfigCharSelState(this, mainStateMachine, gameConfigSO);
            gmGameConfigLevelSelState = new GMGameConfigLevelSelState(this, mainStateMachine, gameConfigSO);

            gmSceneTransitionState = new GMSceneTransitionState(this, mainStateMachine, gameConfigSO);

            scenesEnterState.Add(GameSceneManager.SceneType.GameConfig, gmGameConfigPlayerRegisterState);
            scenesEnterState.Add(GameSceneManager.SceneType.Level, gmLevelTutorialState);

            currentGameParameters = new CurrentGameParameters();
        }

        private void Start()
        {
            GameSceneManager.SceneType currentActiveSceneType = GameScene.GetCurrentActiveSceneType();
            if (currentActiveSceneType == GameSceneManager.SceneType.None)
            {
                Debug.LogError("Scene type for current active scene does not exists");
            }

            // Debugging purposes when starting the game from an scene different than Boot
            if (currentActiveSceneType == GameSceneManager.SceneType.Level)
            {
                int levelIndex = GameScene.GetLevelSceneIndexFromSceneName(GameScene.GetCurrentActiveSceneName());
                if (levelIndex == -1)
                {
                    Debug.LogError("Scene type is a level scene but the name given to the scene does not follow Level_X, where X is the index of the level");
                }
                else
                {
                    currentGameParameters.Level = gameConfigSO.GetLevelSOAtIndex(levelIndex);
                }
            }

            GameManagerState startingState = GetGMSceneEnterState(GameScene.GetCurrentActiveSceneType());          
            if (startingState == null)
            {
                Debug.LogError("GameManagerState for current active scene type does not exists");
            }

            mainStateMachine.Initialize(startingState); 
        }

        private void Update()
        {
            // Remove and add new entities in queue
            RemoveEntitiesInQueue();
            AddEntitiesInQueue();

            // Update state machine
            mainStateMachine.CurrentState().HandleLogic();
            mainStateMachine.CurrentState().HandleAxisInput();
        }

        private void FixedUpdate()
        {
            RemoveEntitiesInQueue();
            mainStateMachine.CurrentState().HandlePhysics();
        }

        private void LateUpdate()
        {
            RemoveEntitiesInQueue();
            mainStateMachine.CurrentState().HandleLateUpdate();
        }

        void RemoveEntitiesInQueue()
        {
            while (physUpdatablesToRemove.Count > 0)
            {
                IPhysicsUpdatable toRemove = physUpdatablesToRemove.Dequeue();
                current_active_physUpdatables.Remove(toRemove);
            }
            while (updatablesToRemove.Count > 0)
            {
                IUpdatable toRemove = updatablesToRemove.Dequeue();
                current_active_updatables.Remove(toRemove);
            }
            while (throwablesToRemove.Count > 0)
            {
                IThrowable toRemove = throwablesToRemove.Dequeue();
                current_active_throwables.Remove(toRemove);
            }
            while (playersToRemove.Count > 0)
            {
                PlayerController toRemove = playersToRemove.Dequeue();
                current_active_players.Remove(toRemove);

                if (OnCurrentActivePlayerRemoved != null) OnCurrentActivePlayerRemoved.Invoke(toRemove);

                if (current_active_players.Count == 0)
                {
                    if (OnCurrentActivePlayersDropToZero != null) OnCurrentActivePlayersDropToZero.Invoke();
                }
            }
            while (navAgentsToRemove.Count > 0)
            {
                NavAgentController toRemove = navAgentsToRemove.Dequeue();
                current_active_navAgents.Remove(toRemove);

                if (current_active_navAgents.Count == 0)
                {
                    if (OnCurrentActiveNavAgentsDropToZero != null) OnCurrentActiveNavAgentsDropToZero.Invoke();
                }
            }
        }
        void AddEntitiesInQueue()
        {
            while (physUpdatablesToAdd.Count > 0)
            {
                IPhysicsUpdatable toAdd = physUpdatablesToAdd.Dequeue();
                current_active_physUpdatables.Add(toAdd);
            }
            while (updatablesToAdd.Count > 0)
            {
                IUpdatable toAdd = updatablesToAdd.Dequeue();
                current_active_updatables.Add(toAdd);
            }
            while (throwablesToAdd.Count > 0)
            {
                IThrowable toAdd = throwablesToAdd.Dequeue();
                current_active_throwables.Add(toAdd);
            }
            while (playersToAdd.Count > 0)
            {
                PlayerController toAdd = playersToAdd.Dequeue();
                current_active_players.Add(toAdd);
            }
            while (navAgentsToAdd.Count > 0)
            {
                NavAgentController toAdd = navAgentsToAdd.Dequeue();
                current_active_navAgents.Add(toAdd);
            }
        }

        #endregion

        #region Game_Entities

        public void AddNavAgentController(NavAgentController navAgent) => navAgentsToAdd.Enqueue(navAgent);
        public void RemoveNavAgentController(NavAgentController navAgent) => navAgentsToRemove.Enqueue(navAgent);
        public void AddPlayerController(PlayerController player) => playersToAdd.Enqueue(player);
        public void RemovePlayerController(PlayerController player) => playersToRemove.Enqueue(player);
        public void AddPhysicsUpdatable(IPhysicsUpdatable _physUpdatable) => physUpdatablesToAdd.Enqueue(_physUpdatable);
        public void RemovePhysicsUpdatable(IPhysicsUpdatable _physUpdatable) => physUpdatablesToRemove.Enqueue(_physUpdatable);
        public void AddUpdatable(IUpdatable _updatable) => updatablesToAdd.Enqueue(_updatable);
        public void RemoveUpdatable(IUpdatable _updatable) => updatablesToRemove.Enqueue(_updatable);
        public void AddThrowable(IThrowable _throwable) => throwablesToAdd.Enqueue(_throwable);
        public void RemoveThrowable(IThrowable _throwable) => throwablesToRemove.Enqueue(_throwable);

        #endregion

        #region Accessors

        public Transform MainCameraTransform() => mainCamera.transform;

        public List<PlayerController> GetCurrentActivePlayers() => current_active_players;
        public HashSet<IPhysicsUpdatable> GetCurrentActivePhysUpdatable() => current_active_physUpdatables;
        public HashSet<IUpdatable> GetCurrentActiveUpdatable() => current_active_updatables;
        public HashSet<NavAgentController> GetCurrentActiveNavAgents() => current_active_navAgents;
        public HashSet<IThrowable> GetCurrentActiveThrowables() => current_active_throwables;

        public PlayerController GetCurrentPlayerWithID(int ID)
        {
            return current_active_players.Find(x => x.GetPlayerID() == ID);
        }

        public static PoolingManager Pooling { get; private set; }
        public static PlayerInputManager PlayerInput { get; private set; }
        public static WavesManager Waves { get; private set; }
        public static UIManager UI { get; private set; }
        public static GameSceneManager GameScene { get; private set; }
        public static DebugManager GameDebug { get; private set; }

        public CurrentGameParameters GetCurrentGameParameters() => currentGameParameters;

        public GameManagerState GetGMSceneEnterState(GameSceneManager.SceneType sceneType)
        {
            GameManagerState gmState;

            if (scenesEnterState.TryGetValue(sceneType, out gmState))
            {
                return gmState;
            }

            return null;
        }

        public GMLevelInitState GetGMLevelInitState() => gmLevelInitState;
        public GMLevelPreparationState GetGMLevelPrepState() => gmLevelPreparationState;
        public GMLevelWaveState GetGLevelWaveState() => gmLevelWaveState;
        public GMLevelRewardState GetGMLevelRewardState() => gmLevelRewardState;
        public GMLevelWinState GetGMLevelWinState() => gmLevelWinState;
        public GMLevelLostState GetGMLevelLostState() => gmLevelLostState;
        public GMLevelTutorialState GetGMLevelTutorialState() => gmLevelTutorialState;
        public GMGameConfigPlayerRegisterState GetGMGameConfigPlayerRegisterState() => gmGameConfigPlayerRegisterState;
        public GMGameConfigCharSelState GetGMGameConfigCharSelState() => gmGameConfigCharSelState;
        public GMGameConfigLevelSelState GetGMGameConfigLevelSelState() => gmGameConfigLevelSelState;
        public GMSceneTransitionState GetGMSceneTransitionState() => gmSceneTransitionState;

        #endregion
    }

}
