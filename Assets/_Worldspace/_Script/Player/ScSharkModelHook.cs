using UnityEngine;

namespace _Workspace._Scripts.Player
{
    public class ScSharkModelHook : MonoBehaviour
    {
        [Header("Identity")]
        public int id;
        
        [Header("Animator")]
        public Animator animator;

        private void OnValidate()
        {
            if (animator == null) animator = GetComponent<Animator>();
        }
    }
}
