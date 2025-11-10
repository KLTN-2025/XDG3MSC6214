using System;
using System.Collections.Generic;
using UnityEngine;
using IPoolable = _Workspace._Scripts.Interfaces.IPoolable;
using Random = UnityEngine.Random;

namespace _Workspace._Scripts.Pooling
{
    public class ScPooler<TPoolableObject> : MonoBehaviour where TPoolableObject : MonoBehaviour, IPoolable
    {
        [SerializeField] private List<TPoolableObject> prefabList;
        [SerializeField] private int poolSize;

        private readonly Queue<TPoolableObject> _pool = new();

        private void Start()
        {
            for (int i = 0; i < poolSize; i++)
            {
                TPoolableObject obj = Instantiate(GetRandomPrefab(),transform);
                obj.gameObject.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        private TPoolableObject GetRandomPrefab()
        {
            if (prefabList is null || prefabList.Count == 0)
            {
                throw new SystemException("No prefabs assigned in Pooler");
            }

            int index = Random.Range(0, prefabList.Count);
            return prefabList[index];
        }

        public TPoolableObject GetRandomFromPool(Vector3 position, Quaternion rotation)
        {
            var obj = _pool.Count > 0 ? _pool.Dequeue() : Instantiate(GetRandomPrefab(), transform);
            
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.gameObject.SetActive(true);
            obj.OnGetFromPool();
            return obj;
        }

        public virtual void ReturnToPool(TPoolableObject obj)
        {
            obj.OnReturnToPool();
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }
}
