using System;
using _Workspace._Scripts.Object;
using UnityEngine;

public class SCEventbus : MonoBehaviour
{
    private static SCEventbus _instance;
    public static SCEventbus Instance
    {
        get
        {
            if (_instance is not null) return _instance;
            _instance = FindAnyObjectByType<SCEventbus>();
            if (_instance is not null) return _instance;
            GameObject obj = new GameObject("SCEventbus");
            _instance = obj.AddComponent<SCEventbus>();
            DontDestroyOnLoad(obj);
            return _instance;
        }
    }
    
    public event Action OnFinishedLoading;
    public void RaiseFinishedLoading()
    {
        OnFinishedLoading?.Invoke();
    }
    
    public class PlayerSpawnData
    {
        public Transform PlayerTransform;
        public Transform MouthTransform;
    }
    public event Action<PlayerSpawnData> OnPlayerSpawn; 
     public PlayerSpawnData LastPlayerSpawn { get; private set; }
    public event Action<bool> OnPlayerDiving;
    public bool IsDiving {get; private set; }

    public void RaisePlayerSpawned(Transform mouth, Transform player)
    {
        LastPlayerSpawn = new PlayerSpawnData()
        {
            PlayerTransform = player,
            MouthTransform = mouth
        };
        OnPlayerSpawn?.Invoke(LastPlayerSpawn);
    }
    public void RaisePlayerDiving(bool isDiving)
    {
        IsDiving = isDiving;
        OnPlayerDiving?.Invoke(isDiving);
    }
    
    public event Action<int> OnScoreChanged;
    public event Action<int> OnComboChanged;
    
    public void RaiseScoreChanged(int delta) => OnScoreChanged?.Invoke(delta);
    public void RaiseComboChanged(int combo) => OnComboChanged?.Invoke(combo);
    
    public event Action OnSushiSpawned;
    public event Action OnSushiEaten;
    public event Action OnSushiMissed;
    public event Action OnSushiExpired;
    
    public void RaiseSushiSpawned() => OnSushiSpawned?.Invoke();
    public void RaiseSushiEaten()   => OnSushiEaten?.Invoke();
    public void RaiseSushiMissed()  => OnSushiMissed?.Invoke();
    public void RaiseSushiExpired() => OnSushiExpired?.Invoke();
    
    public event Action OnSpecialSushiSpawned;
    public event Action OnSpecialSushiEaten;
    
    public void RaiseSpecialSushiSpawned() => OnSpecialSushiSpawned?.Invoke();
    public void RaiseSpecialSushiEaten()   => OnSpecialSushiEaten?.Invoke();

    public event Action OnObstacleSpawned;
    public event Action OnObstacleEatByPlayer;
    public event Action OnObstacleMissed;
    public event Action OnObstacleExpired;

    public void RaiseObstacleSpawned()   => OnObstacleSpawned?.Invoke();
    public void RaiseObstacleEatByPlayer() => OnObstacleEatByPlayer?.Invoke();
    public void RaiseObstacleMissed()    => OnObstacleMissed?.Invoke();
    public void RaiseObstacleExpired()   => OnObstacleExpired?.Invoke();

    public event Action OnItemReturnedToPool; 
    public void RaiseItemReturnedToPool() => OnItemReturnedToPool?.Invoke();

    public event Action<int, int> OnLivesChanged;
    public event Action OnGameOver;
    public void RaiseLifeChanged(int currentLives, int maxLives) => OnLivesChanged?.Invoke(currentLives, maxLives);
    public void RaiseGameOver()                    => OnGameOver?.Invoke();

    public event Action<ScMoveableObject> OnOutOfBound;
    public void RaiseOutOfBound(ScMoveableObject moveObject) => OnOutOfBound?.Invoke(moveObject);

    public event Action OnBoatStartRun;
    public void RaiseBoatStartRun() => OnBoatStartRun?.Invoke();
    
    public event Action OnGameRestart;
    public event Action OnGameHome;
    public void RaiseGameRestart() => OnGameRestart?.Invoke();
    public void RaiseGameHome() => OnGameHome?.Invoke();
    
}
