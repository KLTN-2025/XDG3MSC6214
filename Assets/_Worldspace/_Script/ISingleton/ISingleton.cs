using UnityEngine;

namespace _Workspace._Scripts.ISingleton
{
    public class ISingleton<T> : MonoBehaviour where T : ISingleton<T>
    {
        private static T _instance;
        public static T instance { get { return _instance; } }

        protected virtual void Awake()
        {
            if (_instance != null && this.gameObject != null)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = (T)this;
            }

            if (!gameObject.transform.parent)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        public static void DestroyInstance()
        {
            if (_instance != null)
            {
                Destroy(_instance.gameObject);
                _instance = null;
            }
        }

    }
}