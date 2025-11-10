using System.Collections;
using _Workspace._Scripts.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Workspace._Scripts.Player
{
    public class ScPlayerController : MonoBehaviour
    {
        [Header("Animation Configs")]
        private static readonly int MouthOpen = Animator.StringToHash("MouthOpen");
        private Animator _anim;
        
        private InputSystem_Actions _inputAction;

        [SerializeField] public Collider mouthTrigger;
        [SerializeField] private Collider headTrigger;

        [SerializeField] private float surfaceY;
        [SerializeField] private float moveLerp;
        
        [Header("Dive Config")] 
        [SerializeField] private float diveY;
        [SerializeField] private float diveDownTime;
        [SerializeField] private float riseUpTime;
        [SerializeField] private bool isDiving;
        private Coroutine _diveCo;


        private bool IsMouthOpen { get; set; }
        private bool InputEnable { get; set; } = true;

        #region Unity Methods

        private void OnEnable()
        {
            _inputAction = new InputSystem_Actions(); 
            _inputAction.Player.Enable();
            
            _inputAction.Player.Hold.performed += OnHoldPerformed;
            _inputAction.Player.Hold.canceled += OnHoldCanceled;
            _inputAction.Player.Dive.performed += OnDiveTogglePerformed;
            _anim = GetComponentInChildren<Animator>();
            
            SCEventbus.Instance.RaisePlayerDiving(isDiving);
        }

        private void OnDisable()
        {
            if (_inputAction == null) return;
            _inputAction.Player.Hold.performed -= OnHoldPerformed;
            _inputAction.Player.Hold.canceled -= OnHoldCanceled;
            _inputAction.Player.Dive.performed -= OnDiveTogglePerformed;
            _inputAction.Player.Disable();
        }

        #endregion

        #region Input

        private void OnHoldPerformed(InputAction.CallbackContext ctx)
        {
            if (!InputEnable) return;
            SetMouth(true);
        }
        
        private void OnHoldCanceled(InputAction.CallbackContext ctx)
        {
            SetMouth(false);
        }

        private void OnDiveTogglePerformed(InputAction.CallbackContext ctx)
        {
            if (!InputEnable && !isDiving) return;
            ToggleDive();
        }

        public void EnableInput(bool isEnable)
        {
            InputEnable = isEnable;
            if(_inputAction == null) return;

            if (isEnable)
            {
                _inputAction.Player.Enable();
            }
            else
            {
                _inputAction.Player.Disable();
                SetMouth(false);
            }
        }
        
        #endregion

        #region Set Functions

        private void SetMouth(bool open)
        {
            if (open == IsMouthOpen) return;
            IsMouthOpen = open;

            if (mouthTrigger) mouthTrigger.enabled = open;
            if (headTrigger) headTrigger.enabled = !open;
            if (_anim)
            {
                _anim.SetBool(MouthOpen, open);
            }
        }

        public void SetAnimator(Animator animator)
        {
            _anim = animator;
            _anim.Rebind();
            _anim.Update(0f);
            _anim.SetBool(MouthOpen, false);
        }

        #endregion

        #region Dive Functions

        private void ToggleDive()
        {
            ScAudioManager.instance.PlaySfx("Dive");
            if (isDiving)
            {
                if (_diveCo != null)
                {
                    StopCoroutine(_diveCo);
                    _diveCo = null;
                }
                isDiving = false;
                SCEventbus.Instance.RaisePlayerDiving(isDiving);
                InputEnable = true;
                SetMouth(false);
                StartCoroutine(MoveY(transform.position.y, surfaceY, riseUpTime));
            }
            else
            {
                if (_diveCo != null)
                {
                    StopCoroutine(_diveCo);
                    _diveCo = null;
                }
                isDiving = true;
                SCEventbus.Instance.RaisePlayerDiving(isDiving);
                InputEnable = false;
                SetMouth(false);
                _diveCo = StartCoroutine(DiveRoutine());
            }
        }

        private IEnumerator DiveRoutine()
        {
            yield return MoveY(transform.position.y, diveY, diveDownTime);
            while (isDiving)
                yield return null;
        }

        private IEnumerator MoveY(float fromY, float toY, float duration)
        {
            if (duration <= 0f)
            {
                var p0 = transform.position;
                p0.y = toY;
                transform.position = p0;
                yield break;
            }
            
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duration);
                float y = Mathf.Lerp(fromY, toY, k);
                Vector3 p = transform.position;
                p.y = y;
                transform.position = p;
                yield return null;
            }
        }

        #endregion

        #region Reset

        public void ResetPlayer()
        {
            if(_diveCo != null) {StopCoroutine(_diveCo);
                _diveCo = null;
            }

            isDiving = false;
            SCEventbus.Instance.RaisePlayerDiving(false);

            InputEnable = false;
            SetMouth(false);
            
            _anim.SetBool(MouthOpen, false);
        }

        #endregion
    }
}
