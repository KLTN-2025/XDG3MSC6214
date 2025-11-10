using _Workspace._Scripts.Managers;
using UnityEngine;

namespace _Workspace._Scripts.Object
{
    public class ScMoveableObject : MonoBehaviour
    {
        public enum FlyKind
        {
            Food,
            Obstacle,
            Special
        }

        public FlyKind kind;
        private bool Edible => kind == FlyKind.Food;
        private bool Obstacle => kind == FlyKind.Obstacle;
        private bool Special => kind == FlyKind.Special;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Mouth"))
            {
                if (Edible) 
                {
                    SCEventbus.Instance.RaiseSushiEaten();
                    ScAudioManager.instance.PlaySfx("Eat");
                }
                else if (Obstacle)
                {
                    SCEventbus.Instance.RaiseObstacleEatByPlayer();
                    ScAudioManager.instance.PlaySfx("Obstacle");
                }
                else if (Special)
                {
                    SCEventbus.Instance.RaiseSpecialSushiEaten();
                    SCEventbus.Instance.RaiseSushiEaten();
                    ScAudioManager.instance.PlaySfx("Eat");
                }

                SCEventbus.Instance.RaiseOutOfBound(this);
                enabled = false;
                return;
            }

            if (!other.CompareTag("Water")) return;
            SCEventbus.Instance.RaiseOutOfBound(this);
            enabled = false;
        }
    }
}
