using System;
using System.Collections;
using _Workspace._Scripts.Managers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Workspace._Scripts.Object
{
    public class ScBoatMovement : MonoBehaviour
    {
        private enum FaceMode
        {
            None,
            FlipScaleX,
            RotateY
        }

        [Header("Path")] 
        [SerializeField] private Transform leftPoint;
        [SerializeField] private Transform rightPoint;
        [SerializeField] private AudioSource boatAudio;

        [Header("Motion")] 
        [SerializeField] private float speed = 5f;
        [Header("Speed Ramp")]
        [SerializeField] private float speedStart = 3f;
        [SerializeField] private float speedEnd = 8f;
        [SerializeField] private float rampDuration = 60f;
        private float _runTime;

        [Header("Interval Wait")]
        [SerializeField] private float minWait = 1f;
        [SerializeField] private float maxWait = 3f;
        
        [Header("Facing")] [SerializeField] private FaceMode faceMode = FaceMode.RotateY;
        [SerializeField] private float baseYawFacingRight = 0f;
        [SerializeField] private Vector2 startDelayRange = new Vector2(5f, 10f);
        
        private Vector3 _initialScale;

        private void Awake()
        {
            _initialScale = transform.localScale;
        }

        private Coroutine _delayCR, _moveCR;
        private bool _delayElapsed;

        private void OnEnable()
        {
            SCEventbus.Instance.OnPlayerSpawn += StarRun;
            _delayCR = StartCoroutine(DelayThenMaybeStart());
        }
        
        private void OnDisable()
        {
            if (boatAudio && boatAudio.isPlaying) boatAudio.Stop();
            if (_delayCR != null) { StopCoroutine(_delayCR); _delayCR = null; }
            if (_moveCR  != null) { StopCoroutine(_moveCR);  _moveCR  = null; }
            SCEventbus.Instance.OnPlayerSpawn -= StarRun;
            _delayElapsed = false;
        }

        private IEnumerator DelayThenMaybeStart()
        {
            float delay = Mathf.Clamp(Random.Range(startDelayRange.x, startDelayRange.y), 0f, 999f);
            yield return new WaitForSeconds(delay);

            _delayElapsed = true;
            _delayCR = null;
            
            var bus = SCEventbus.Instance;
            if (bus != null && bus.LastPlayerSpawn != null)
            {
                StarRun(bus.LastPlayerSpawn);
            }
        }

        private void StarRun(SCEventbus.PlayerSpawnData data)
        {
            if (!_delayElapsed) return;
            if (_moveCR != null) { StopCoroutine(_moveCR); _moveCR = null; }

            if (leftPoint == null || rightPoint == null)
            {
                Debug.LogError("[ScBoatMovement] leftPoint/rightPoint is null.");
                return;
            }
            _moveCR = StartCoroutine(MoveLoop());
        }

        private IEnumerator MoveLoop()
        {
            Transform start = leftPoint;
            Transform end = rightPoint;

            while (true)
            {
                _runTime += Time.deltaTime;
                
                float t = Mathf.Clamp01(_runTime / rampDuration);
                speed = Mathf.Lerp(speedStart, speedEnd, t);
                SCEventbus.Instance.RaiseBoatStartRun();
                ScAudioManager.instance.PlaySfx("Boat");
                
                float dirX = Mathf.Sign(end.position.x - start.position.x);
                ApplyFacing(dirX);
                
                while ((transform.position - end.position).sqrMagnitude > 0.01f)
                {
                    _runTime += Time.deltaTime;
                    t = Mathf.Clamp01(_runTime / rampDuration);
                    speed = Mathf.Lerp(speedStart, speedEnd, t);
                    
                    transform.position = Vector3.MoveTowards(transform.position, end.position, speed * Time.deltaTime);
                    yield return null;
                }
                
                (start, end) = (end, start);

                float wait = Random.Range(minWait, maxWait);
                yield return new WaitForSeconds(wait);
            }
        }

        private void ApplyFacing(float dirX)
        {
            switch (faceMode)
            {
                case FaceMode.None:
                    return;

                case FaceMode.FlipScaleX:
                    var s = _initialScale;
                    s.x = Mathf.Abs(s.x) * (dirX > 0 ? 1f : -1f);
                    transform.localScale = s;
                    break;

                case FaceMode.RotateY:
                    float targetYaw = (dirX > 0 ? baseYawFacingRight : baseYawFacingRight + 180f);
                    transform.rotation = Quaternion.Euler(0f, targetYaw, 0f);
                    break;
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            SCEventbus.Instance.RaiseGameOver();
        }
    }
}
