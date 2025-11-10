
using System.Collections;
using System.Collections.Generic;
using _Workspace._Scripts.Hub;
using _Workspace._Scripts.Object;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Workspace._Scripts.Spawner
{
    public class ScObjectSpawner : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private ScPoolHub poolHub;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform mouthPosition;

        [Header("Difficulty (Ramps)")]
        [SerializeField] private float speedStart = 3.0f;
        [SerializeField] private float speedEnd = 7.5f;
        [SerializeField] private float speedRampDuration = 60f;

        [Header("Spawn Timing")]
        [SerializeField] private float foodIntervalMin = 0.6f;
        [SerializeField] private float foodIntervalMax = 1.2f;
        [SerializeField] private float obstacleInterval = 1.5f;
        [SerializeField] private float specialChance;

        [Header("Category Weights")]
        [SerializeField] private float foodWeight;
        [SerializeField] private float obstacleWeight;
        [SerializeField] private float specialWeight;
        
        private Coroutine _waitSpawnCR;
        private Coroutine _spawnRoutine;
    

        private bool  _running;
        private float _runTime;
        private readonly List<ScPoolableObject> _active = new();
        

        private void OnEnable()
        {
        SCEventbus.Instance.OnOutOfBound += HandleOutOfBound;
        SCEventbus.Instance.OnPlayerSpawn += HandlePlayerSpawn;
        SCEventbus.Instance.OnPlayerDiving += HandelPlayerDiving;
        
        if (mouthPosition == null && _waitSpawnCR == null)
            _waitSpawnCR = StartCoroutine(WaitForPlayerSpawn());
        }

        private void OnDisable()
        {
            SCEventbus.Instance.OnOutOfBound -= HandleOutOfBound;
            SCEventbus.Instance.OnPlayerSpawn -= HandlePlayerSpawn;
            SCEventbus.Instance.OnPlayerDiving -= HandelPlayerDiving;
        
            if (_waitSpawnCR != null) { StopCoroutine(_waitSpawnCR); _waitSpawnCR = null; }
        }
        
        private IEnumerator WaitForPlayerSpawn()
        {
            if (mouthPosition is not null) { _waitSpawnCR = null; yield break; }

            while (mouthPosition is null)
            {
                var last = SCEventbus.Instance.LastPlayerSpawn;
                if (IsValidSpawnForThisScene(last))
                {
                    HandlePlayerSpawn(last);
                    break;
                }
                yield return null;
            }
            _waitSpawnCR = null;
        }
        
        private bool IsValidSpawnForThisScene(SCEventbus.PlayerSpawnData data)
        {
            if (data == null) return false;
            if (!data.MouthTransform || !data.PlayerTransform) return false; 
            return data.PlayerTransform.gameObject.scene == gameObject.scene; 
        }
        
        
        private void StartSpawning()
        {
            if (_running) return;
            _running = true;
            _spawnRoutine = StartCoroutine(StartWhenReady());
        }

        public void StopSpawning(bool clearActive)
        {
            _running = false;
            if (_spawnRoutine != null)
            {
                StopCoroutine(_spawnRoutine);
                _spawnRoutine = null;
            }

            if (!clearActive) return;
            for (int i = _active.Count - 1; i >= 0; i--)
                SafeReturn(_active[i]);
            _active.Clear();
        }

    private IEnumerator SpawnLoop()
    {
        _runTime = 0f;

        while (_running)
        {
            float spd = Mathf.Max(0.1f, EvalRamp(speedStart, speedEnd, speedRampDuration, _runTime));
            
            float totalW = Mathf.Max(0.0001f, foodWeight + obstacleWeight);
            float r = Random.value * totalW;
            SpawnCategory cat = (r < obstacleWeight) ? SpawnCategory.Obstacle : SpawnCategory.Food;
            
            Vector3 pos  = spawnPoint.position;
            Quaternion rot = spawnPoint.rotation;
            
            ScPoolableObject pooled;
            SpawnCategory actualCat = cat;
            
            if (cat == SpawnCategory.Food)
            {
                bool special = (Random.value < specialChance);
                if (special)
                {
                    actualCat = SpawnCategory.Special;
                    pooled = poolHub.Get(SpawnCategory.Special, pos, rot);
                }
                else
                {
                    pooled = poolHub.Get(SpawnCategory.Food, pos, rot);
                }
            }
            else
            {
                pooled = poolHub.Get(SpawnCategory.Obstacle, pos, rot);
            }
            
            if (pooled is not null)
            {

                _active.Add(pooled);

                ScObjectMovement obj = pooled.GetComponent<ScObjectMovement>();
                obj?.LaunchTo(mouthPosition.position, spd);

                if (actualCat == SpawnCategory.Food)    
                    SCEventbus.Instance.RaiseSushiSpawned();
                else if (actualCat == SpawnCategory.Special)
                    SCEventbus.Instance.RaiseSpecialSushiSpawned();
                else                                         
                    SCEventbus.Instance.RaiseObstacleSpawned();
            }

            float interval = (cat == SpawnCategory.Food)
                ? Random.Range(foodIntervalMin, foodIntervalMax)
                : Mathf.Max(0.05f, obstacleInterval);

            yield return new WaitForSeconds(interval);
            _runTime += interval;
        }
    }
    
    private IEnumerator StartWhenReady()
    {
        while (mouthPosition == null) yield return null;
        yield return StartCoroutine(SpawnLoop());
    }

    private void HandleOutOfBound(ScMoveableObject go)
    {
        var pooled = go.GetComponent<ScPoolableObject>();
        if (pooled == null) return;

        _active.Remove(pooled);
        SafeReturn(pooled);
    }

    private void HandlePlayerSpawn(SCEventbus.PlayerSpawnData data)
    {
        mouthPosition = data.MouthTransform;
        
        if (_waitSpawnCR != null) { StopCoroutine(_waitSpawnCR); _waitSpawnCR = null; }
        
        StartSpawning();
    }

        private void HandelPlayerDiving(bool isDiving)
        {
            if(isDiving)
                StopSpawning(false);
            else
            {
                StartSpawning();
            }
        }
    
        private void SafeReturn(ScPoolableObject pooled)
        {
            if (pooled == null) return;
            poolHub.Return(pooled);
        }
        
        private static float EvalRamp(float start, float end, float duration, float t)
        {
            if (duration <= 0f) return end;
             float u = Mathf.Clamp01(t / duration);
            u = u * u * (3f - 2f * u);
            return Mathf.LerpUnclamped(start, end, u);
        }

    }
}
