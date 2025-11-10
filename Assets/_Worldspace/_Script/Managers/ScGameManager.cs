using System;
using _Workspace._Scripts.ISingleton;
using _Workspace._Scripts.Spawner;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.SocialPlatforms.Impl;

namespace _Workspace._Scripts.Managers
{
    public enum GameState
    {
        Home,
        InGame,
        Playing,
        Resume,
        Pause,
        EndGame
    }
    public class ScGameManager : ISingleton<ScGameManager>
    {
        [Header("GAME STATE")]
        public GameState State { get; private set; } = GameState.Home;

        public GameState CurrentState => State;
        [Header("REFERENCES")]
        [SerializeField] private ScPlayerManager  playerManager;
        [SerializeField] private ScObjectSpawner objectSpawner;
        
        [Header("SCORE")]
        [SerializeField] private int scores;
        [SerializeField] private int highScore;
        [SerializeField] private float comboWindow;
        [SerializeField] private int tierStep;
        [SerializeField] private float mulPerTier;
        [SerializeField] private float maxMultiplier;
        [SerializeField] private int basePointPerSushi = 1;
        
        [Header("CONFIGS")] 
        [SerializeField] private bool autoPauseOnBackground;

        private int _comboCount;
        private float _comboTimer;
        private float _multiplier = 1f;

        #region Unity Methods

        private void Start()
        {
            Application.targetFrameRate = 45;
            highScore = PlayerPrefs.GetInt("HIGH_SCORE", 0);
            ScSetState(GameState.Home);
        }

        private void OnEnable()
        {
            // TODO: Subscribe event
            SCEventbus.Instance.OnGameOver += HandleGameOverEvent;
            SCEventbus.Instance.OnSushiEaten += HandleSushiEaten;
            SCEventbus.Instance.OnObstacleEatByPlayer += HandleObstacle;
        }

        private void OnDisable()
        {
            // TODO: Unsubscribe events
            SCEventbus.Instance.OnGameOver -= HandleGameOverEvent;
            SCEventbus.Instance.OnSushiEaten -= HandleSushiEaten;
            SCEventbus.Instance.OnObstacleEatByPlayer -= HandleObstacle;
        }

        private void Update()
        {
            if (_comboCount <= 0) return;
            _comboTimer -= Time.deltaTime;
            if (_comboTimer <= 0)
                BreakCombo();
        }

        #endregion

        #region Set Functions
        
        public void ScSetState(GameState newState)
        {
            if(State == newState) return;
            State = newState;

            switch (State)
            {
                case GameState.Home:
                    ScHandleHomeState();
                    break;
                case GameState.InGame:
                    ScHandleInGameState();
                    break;
                case GameState.Playing:
                    ScHandlePlayingState();
                    break;
                case GameState.Resume:
                    ScHandleResumeState();
                    break;
                case GameState.Pause:
                    ScHandlePauseState();
                    break;
                case GameState.EndGame:
                    ScHandleEndGameState();
                    break;
            }
        }
        #endregion

        #region Handle Functions

        private void ScHandleHomeState()
        {
            objectSpawner.StopSpawning(true);
            playerManager.ResetPlayer();
            ScUIManager.instance.SetState(UIState.Menu);
            Time.timeScale = 1f;
            if(ScAudioManager.instance.IsBgmPlaying) return;
            ScAudioManager.instance.PlayBGM("BGM 1");
        }

        private void ScHandleInGameState()
        {
            Time.timeScale = 1f;
            ScUIManager.instance.ProgressScPanel();
            ScUIManager.instance.SetState(UIState.Game);
            objectSpawner.StopSpawning(true);
            playerManager.ResetPlayer();
            scores = 0;
            SCEventbus.Instance.RaiseScoreChanged(scores);
            if(ScAudioManager.instance.IsBgmPlaying) return;
            ScAudioManager.instance.PlayBGM("BGM 1");
        }
        
        private void ScHandlePlayingState()
        {
            Time.timeScale = 1f;
            playerManager.SpawnPlayerAtDefault();
        }
        
        private void ScHandleResumeState()
        {
            Time.timeScale = 1f;
            ScUIManager.instance.SetState(UIState.Resume);
        }
        
        private void ScHandlePauseState()
        {
            Time.timeScale = 0f;
            ScUIManager.instance.SetState(UIState.Pause);
        }
        
        private void ScHandleEndGameState()
        {
            Time.timeScale = 0f;
            ScUIManager.instance.SetState(UIState.End);
            ScAudioManager.instance.PlaySfx("Lose");
            ScAudioManager.instance.PauseBGM();
        }

        private void HandleGameOverEvent()
        {
            ScSetState(GameState.EndGame);
        }

        private void HandleSushiEaten()
        {
            _comboCount++;
            _comboTimer = comboWindow;
            
            int tier = tierStep > 0 ? (_comboCount/tierStep) : 0;
            _multiplier = Mathf.Min(1f + tier * mulPerTier, maxMultiplier);
            
            int add  = Mathf.RoundToInt(basePointPerSushi * _multiplier);
            scores += add;
            SCEventbus.Instance.RaiseScoreChanged(scores);
            
            if (scores <= highScore) return;
            highScore = scores;
            PlayerPrefs.SetInt("HIGH_SCORE", highScore);
            PlayerPrefs.Save();
        }

        private void HandleObstacle()
        {
            BreakCombo();
        }

        #endregion

        #region Scores System

        private void BreakCombo()
        {
            _comboCount = 0;
            _multiplier = 1;
            _comboTimer = 0;
        }

        #endregion
        
        #region App Lifecycle

        private void OnApplicationPause(bool paused)
        {
            if (!autoPauseOnBackground) return;
            if(!paused) return;
            
            if (State == GameState.Playing) ScHandlePauseState();
        }

        #endregion
    }
}
