using UnityEngine;

namespace _Workspace._Scripts.Interfaces
{
    public interface IPoolable
    {
        void OnGetFromPool();
        void OnReturnToPool();
    }
}
