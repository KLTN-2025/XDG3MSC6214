using System;
using _Workspace._Scripts.Interfaces;
using _Workspace._Scripts.Pooling;
using UnityEngine;

namespace _Workspace._Scripts.Object
{
    public class ScPoolableObject : MonoBehaviour, IPoolable
    {
        private ScPooler<ScPoolableObject> _ownPooler;
        private ScMoveableObject _item;
        private void OnEnable()
        {
            _item = GetComponent<ScMoveableObject>();
        }

        public void SetOwnPool(ScPooler<ScPoolableObject> pool) => _ownPooler = pool;
        
        public void OnGetFromPool()
        {
            if (_item) { _item.enabled = true; }
        }

        public void OnReturnToPool()
        {
            if (_item) { _item.enabled = false; }
        }
        
        public void Despawn()
        {
            if (_ownPooler) _ownPooler.ReturnToPool(this);
            else gameObject.SetActive(false);
        }

        public ScMoveableObject Item => _item;
    }
}
