using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Workspace._Scripts.UIGame
{
    [DisallowMultipleComponent]
    public class ScButtonAnim : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler,
        ISelectHandler, IDeselectHandler
    {
        public enum AnimMode { Scale, Punch, Shake, None }

        [Header("Refs")]
        [SerializeField] private Transform target;
        [SerializeField] private Graphic colorTarget;

        [Header("General")]
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private bool respectInteractable = true;

        [Header("Scale Mode")]
        [SerializeField] private AnimMode hoverAnim = AnimMode.Scale;
        [SerializeField] private AnimMode pressAnim = AnimMode.Punch;

        [Header("Scale Values")]
        [SerializeField] private float hoverScale = 1.06f;
        [SerializeField] private float pressScale = 0.95f;
        [SerializeField] private float scaleDuration = 0.15f;
        [SerializeField] private Ease scaleEaseIn = Ease.OutQuad;
        [SerializeField] private Ease scaleEaseOut = Ease.OutQuad;

        [Header("Punch (Press)")]
        [SerializeField] private Vector3 punchAmount = new Vector3(0.1f, 0.1f, 0);
        [SerializeField] private float punchDuration = 0.2f;
        [SerializeField] private int punchVibrato = 8;
        [SerializeField] private float punchElasticity = 1f;

        [Header("Shake (Press)")]
        [SerializeField] private float shakeDuration = 0.18f;
        [SerializeField] private Vector3 shakeStrength = new Vector3(10f, 10f, 0);
        [SerializeField] private int shakeVibrato = 20;

        [Header("Color (Optional)")]
        [SerializeField] private bool useColorTween = false;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor  = Color.white;
        [SerializeField] private Color pressColor  = Color.white;
        [SerializeField] private float colorDuration = 0.12f;
    
    
        private Button _btn;
        private Vector3 _baseScale;
        private Tweener _scaleTw, _colorTw, _fxTw;
        private bool _pressed;

        void Reset()
        {
            target = transform;
            colorTarget = GetComponent<Graphic>();
        }

        void Awake()
        {
            if (target == null) target = transform;
            if (colorTarget == null) colorTarget = GetComponent<Graphic>();
            _btn = GetComponent<Button>();
            _baseScale = target.localScale;
        
            if (useColorTween && colorTarget != null)
                colorTarget.color = normalColor;
        }

        void OnDisable()
        {
            KillAllTweens();
            if (target) target.localScale = _baseScale;
            if (useColorTween && colorTarget) colorTarget.color = normalColor;
            _pressed = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!AllowAnim()) return;
            PlayHover(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!AllowAnim()) return;
            if (!_pressed)
                PlayHover(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!AllowAnim()) return;
            _pressed = true;
            PlayPress();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!AllowAnim()) return;
            _pressed = false;
            if (IsPointerOverSelf(eventData))
                PlayHover(true);
            else
                PlayHover(false);
        }
    
        public void OnSelect(BaseEventData eventData)
        {
            if (!AllowAnim()) return;
            PlayHover(true);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (!AllowAnim()) return;
            if (!_pressed) PlayHover(false);
        }
    
        private void PlayHover(bool enter)
        {
            if (target == null) return;
        
            KillScale();
            float to = enter ? hoverScale : 1f;
            if (hoverAnim == AnimMode.Scale)
            {
                _scaleTw = target.DOScale(_baseScale * to, scaleDuration)
                    .SetEase(enter ? scaleEaseIn : scaleEaseOut)
                    .SetUpdate(useUnscaledTime);
            }
        
            if (useColorTween && colorTarget != null)
            {
                KillColor();
                _colorTw = colorTarget.DOColor(enter ? hoverColor : normalColor, colorDuration)
                    .SetUpdate(useUnscaledTime);
            }
        }

        private void PlayPress()
        {
            if (target == null) return;
        
            if (pressAnim == AnimMode.Scale)
            {
                KillScale();
                _scaleTw = target.DOScale(_baseScale * pressScale, scaleDuration)
                    .SetEase(scaleEaseIn)
                    .SetUpdate(useUnscaledTime)
                    .OnComplete(() =>
                    {
                        _scaleTw = target.DOScale(_baseScale * hoverScale, scaleDuration)
                            .SetEase(scaleEaseOut)
                            .SetUpdate(useUnscaledTime);
                    });
            }
            else if (pressAnim == AnimMode.Punch)
            {
                KillFx();
                _fxTw = target.DOPunchScale(punchAmount, punchDuration, punchVibrato, punchElasticity)
                    .SetUpdate(useUnscaledTime);
            }
            else if (pressAnim == AnimMode.Shake)
            {
                KillFx();
                _fxTw = target.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato)
                    .SetUpdate(useUnscaledTime);
            }
        
            if (useColorTween && colorTarget != null)
            {
                KillColor();
                _colorTw = colorTarget.DOColor(pressColor, colorDuration)
                    .SetUpdate(useUnscaledTime)
                    .OnComplete(() =>
                    {
                        var to = hoverColor;
                        _colorTw = colorTarget.DOColor(to, colorDuration).SetUpdate(useUnscaledTime);
                    });
            }
        }
    
        private bool AllowAnim()
        {
            if (!isActiveAndEnabled) return false;
            if (!target) return false;
            if (respectInteractable && _btn != null && !_btn.interactable) return false;
            return true;
        }

        private bool IsPointerOverSelf(PointerEventData data)
        {
            if (data == null) return false;
            var go = data.pointerEnter;
            return go != null && (go == gameObject || go.transform.IsChildOf(transform));
        }

        private void KillScale()
        {
            if (_scaleTw != null && _scaleTw.IsActive()) _scaleTw.Kill();
            _scaleTw = null;
        }
        private void KillColor()
        {
            if (_colorTw != null && _colorTw.IsActive()) _colorTw.Kill();
            _colorTw = null;
        }
        private void KillFx()
        {
            if (_fxTw != null && _fxTw.IsActive()) _fxTw.Kill();
            _fxTw = null;
        }
        private void KillAllTweens()
        {
            KillScale(); KillColor(); KillFx();
        }
    }
}