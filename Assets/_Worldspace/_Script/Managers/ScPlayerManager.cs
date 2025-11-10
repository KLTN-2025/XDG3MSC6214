using System;
using _Workspace._Scripts.Player;
using _Workspace._Scripts.Spawner;
using UnityEngine;

namespace _Workspace._Scripts.Managers
{
    public class ScPlayerManager : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private ScPlayerSpawner spawner;

        private ScPlayerController _player;

        [Header("Spawn Settings")] 
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private float surfaceY;
        [SerializeField] private Transform mouthTransform;
        
        [SerializeField] private int maxLives;
        [SerializeField] private int currentLives;

        private void OnEnable()
        {
            currentLives = maxLives;
            SCEventbus.Instance.OnObstacleEatByPlayer += HandleEatByByObstacle;
            SCEventbus.Instance.OnSpecialSushiEaten += HandleSpecialSushiEaten;
            SCEventbus.Instance.RaisePlayerSpawned(mouthTransform, playerSpawnPoint);
        }

        private void OnDisable()
        {
            SCEventbus.Instance.OnObstacleEatByPlayer -= HandleEatByByObstacle;
            SCEventbus.Instance.OnSpecialSushiEaten -= HandleSpecialSushiEaten;
        }

        public void SpawnPlayerAtDefault()
        {
            Vector3 pos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
            pos.y = surfaceY;

            _player = spawner.Spawn(pos);
            if(_player == null) return;
            mouthTransform = _player.mouthTrigger.transform;
            _player.EnableInput(true);
            SCEventbus.Instance.RaiseLifeChanged(currentLives, maxLives);
            SCEventbus.Instance.RaisePlayerSpawned(mouthTransform, playerSpawnPoint);
        }

        public void SpawnPlayerAt(Vector3 pos)
        {
            pos.y = surfaceY;
            _player = spawner.Spawn(pos);
            if(_player == null) return;
            mouthTransform = _player.mouthTrigger.transform;
            _player.EnableInput(true);
            SCEventbus.Instance.RaisePlayerSpawned(mouthTransform, playerSpawnPoint);
        }

        private void HandleEatByByObstacle()
        {
            ChangeLive(-1);
        }

        private void HandleSpecialSushiEaten()
        {
            HealOneLife();
        }

        private void ChangeLive(int amount)
        {
            int preLives = currentLives;
            currentLives = Mathf.Clamp(currentLives + amount,0, maxLives);
            
            if(currentLives != preLives)
                SCEventbus.Instance.RaiseLifeChanged(currentLives, maxLives);
            if(currentLives <= 0)
                SCEventbus.Instance.RaiseGameOver();
        }

        private void HealOneLife()
        {
            if(currentLives >= maxLives) return;
            ChangeLive(1);
        }

        public void ResetPlayer()
        {
            currentLives  = maxLives;
            if (_player == null) return;
            _player.EnableInput(false);
            Destroy(_player.gameObject);
            _player = null;
        }
    }
}
