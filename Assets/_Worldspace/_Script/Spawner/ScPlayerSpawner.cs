using _Workspace._Scripts.Player;
using UnityEngine;

namespace _Workspace._Scripts.Spawner
{
    public class ScPlayerSpawner : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private ScPlayerController player;
        [SerializeField] private Transform defaultParent;
        
        public ScPlayerController Spawn(Vector3 position, Quaternion rotation)
        {
            if (player is null)
            {
                Debug.LogError($"[PlayerSpawner] Missing prefab");
                return null;
            }

            Transform parent = defaultParent != null ? defaultParent : null;
            ScPlayerController go = Instantiate(player, position, Quaternion.Euler(0, 90, 0 ), parent);
            
            var switcher = go.GetComponentInChildren<ScSharkModelSwitcher>();
            if (switcher) switcher.ApplyFromPrefs();
            
            ScPlayerController controller = go.GetComponentInChildren<ScPlayerController>();
            if (controller != null)
            {
                Animator activeAnim = null;
                
                if (switcher != null)
                {
                    activeAnim = switcher.GetActiveAnimator();
                }
                if (activeAnim == null)
                {
                    foreach (var a in go.GetComponentsInChildren<Animator>(true))
                    {
                        if (a.gameObject.activeInHierarchy) { activeAnim = a; break; }
                    }
                }

                if (activeAnim != null) controller.SetAnimator(activeAnim);
            }
            if (controller != null) return controller;
            Debug.LogError("[PlayerSpawner] Missing ScPlayerController");
            return null;
        }

        public ScPlayerController Spawn(Vector3 position) => Spawn(position, Quaternion.identity);
    }
}
