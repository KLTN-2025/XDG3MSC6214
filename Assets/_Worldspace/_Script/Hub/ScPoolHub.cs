using System;
using System.Collections.Generic;
using _Workspace._Scripts.Object;
using _Workspace._Scripts.Pooling;
using UnityEngine;

namespace _Workspace._Scripts.Hub
{
    public enum SpawnCategory
    {
        Food,
        Obstacle,
        Special
    }

    [Serializable]
    public struct PoolEntry
    {
        public SpawnCategory category;
        public ScPooler<ScPoolableObject> pool;
    }
    public class ScPoolHub : MonoBehaviour
    {
        [Header("Registered Pools")]
        [SerializeField]
        private List<PoolEntry> pools = new();
        
        private Dictionary<SpawnCategory, ScPooler<ScPoolableObject>> map =
            new Dictionary<SpawnCategory, ScPooler<ScPoolableObject>>();

        private void OnEnable()
        {
            map.Clear();
            foreach (PoolEntry e in pools)
            {
                if(e.pool == null) continue;
                if(map.ContainsKey(e.category)) continue;
                map.Add(e.category, e.pool);
            }
        }

        public ScPoolableObject Get(SpawnCategory category, Vector3 pos, Quaternion rot)
        {
            if(!map.TryGetValue(category, out var pool) || pool == null)
            {
                Debug.LogWarning($"[ScPoolHub] Missing pool for {category}");
                return null;
            }

            var obj = pool.GetRandomFromPool(pos, rot);
            obj.SetOwnPool(pool);
            return obj;
        }

        public void Return(ScPoolableObject obj)
        {
            if(obj == null) return;
            obj.Despawn();
        }
    }
}
