using DG.Tweening;
using UnityEngine;

namespace _Workspace._Scripts.Interfaces
{
    public abstract class ScBaseUI : MonoBehaviour
    {
        [Header("Setting UI")]
        [SerializeField] protected GameObject graphicHolder;
        [SerializeField] protected bool setActiveOnStart;
    
        [Header("Tween Settings")]
        [SerializeField] protected float fadeDuration = 0.3f;
        [SerializeField] protected float scaleDuration = 0.3f;
        [SerializeField] protected float popScaleFrom = 0.8f;
        [SerializeField] protected Ease easeIn = Ease.OutBack;
        [SerializeField] protected Ease easeOut = Ease.InCubic;

        public enum EnterAnim { FadeScalePop, Slide, SlideOvershoot, Bounce, None }
        public enum ExitAnim  { FadeScalePop, Slide, SlideOvershoot, Bounce, None }
        public enum SlideDirection { Left, Right, Top, Bottom }

        [Header("Animation Mode")]
        [SerializeField] protected EnterAnim enterAnim = EnterAnim.FadeScalePop;
        [SerializeField] protected ExitAnim  exitAnim  = ExitAnim.FadeScalePop;

        [Header("Slide Settings")]
        [SerializeField] protected SlideDirection slideInFrom  = SlideDirection.Bottom;
        [SerializeField] protected SlideDirection slideOutTo   = SlideDirection.Bottom;
        [SerializeField] protected float slideDistance = 800f;

        [Header("Slide Overshoot")]
        [SerializeField] protected float overshootPx = 60f;
        [SerializeField] protected Ease slideEaseIn  = Ease.OutCubic;
        [SerializeField] protected Ease slideEaseOut = Ease.InCubic;

        [Header("Bounce / Shake")]
        [SerializeField] protected Vector3 punchScale = new Vector3(0.12f, 0.12f, 0f);
        [SerializeField] protected float punchDuration = 0.35f;
        [SerializeField] protected int punchVibrato = 10;
        [SerializeField] protected float punchElasticity = 1f;
        
        [SerializeField] protected float shakeDuration = 0.25f;
        [SerializeField] protected Vector3 shakeStrength = new Vector3(12f, 12f, 0f);
        [SerializeField] protected int shakeVibrato = 20;
        
        [Header("Timing Flags")] 
        [SerializeField] protected bool useUnscaledTime = true;

        protected CanvasGroup CanvasGroup;
        protected Tweener FadeTw, ScaleTw;

        protected RectTransform RectT;
        protected Vector3 InitialLocalScale;
        protected Vector2 InitialAnchoredPos;

        protected virtual void Awake()
        {
            if (graphicHolder == null)
                graphicHolder = this.gameObject;
        
            CanvasGroup = graphicHolder.GetComponent<CanvasGroup>();
            if(CanvasGroup == null) CanvasGroup = graphicHolder.AddComponent<CanvasGroup>();

            RectT = graphicHolder.GetComponent<RectTransform>();
            InitialLocalScale = graphicHolder.transform.localScale;
            if (RectT != null) InitialAnchoredPos = RectT.anchoredPosition;
        }    
        protected virtual void Start()
        {
            SetUIActive(setActiveOnStart, instant: true);
        }   
        public void SetUIActive(bool isActive, bool instant = false)
        {
            if (FadeTw != null && FadeTw.IsActive()) FadeTw.Kill();
            if (ScaleTw != null && ScaleTw.IsActive()) ScaleTw.Kill();

            if (isActive)
            {
                graphicHolder.SetActive(true);
                CanvasGroup.interactable = true;
                CanvasGroup.blocksRaycasts = true;

                if (instant)
                {
                    CanvasGroup.alpha = 1f;
                    if (RectT != null) RectT.anchoredPosition = InitialAnchoredPos; else graphicHolder.transform.localPosition = graphicHolder.transform.localPosition;
                    graphicHolder.transform.localScale = Vector3.one;
                    return;
                }

                switch (enterAnim)
                {
                    case EnterAnim.None:
                        CanvasGroup.alpha = 1f;
                        if (RectT != null) RectT.anchoredPosition = InitialAnchoredPos;
                        graphicHolder.transform.localScale = Vector3.one;
                        break;

                    case EnterAnim.FadeScalePop:
                        CanvasGroup.alpha = 0f;
                        graphicHolder.transform.localScale = Vector3.one * popScaleFrom;
                        FadeTw  = CanvasGroup.DOFade(1f, fadeDuration).SetUpdate(useUnscaledTime);
                        ScaleTw = graphicHolder.transform.DOScale(1f, scaleDuration).SetEase(easeIn).SetUpdate(useUnscaledTime);
                        break;

                    case EnterAnim.Slide:
                        CanvasGroup.alpha = 1f;
                        if (RectT != null)
                        {
                            var from = InitialAnchoredPos + SlideOffset(slideInFrom, slideDistance);
                            RectT.anchoredPosition = from;
                            FadeTw = RectT.DOAnchorPos(InitialAnchoredPos, fadeDuration).SetEase(easeIn).SetUpdate(useUnscaledTime);
                        }
                        else
                        {
                            var t = graphicHolder.transform;
                            var start = t.localPosition + (Vector3)SlideOffset(slideInFrom, slideDistance);
                            t.localPosition = start;
                            FadeTw = t.DOLocalMove(InitialAnchoredPos, fadeDuration).SetEase(easeIn).SetUpdate(useUnscaledTime);
                        }
                        break;
                    case EnterAnim.SlideOvershoot:
                    {
                        CanvasGroup.alpha = 1f;
                        if (RectT != null)
                        {
                            var from = InitialAnchoredPos + SlideOffset(slideInFrom, slideDistance);
                            var past = InitialAnchoredPos - SlideOffset(slideInFrom, overshootPx); 
                            RectT.anchoredPosition = from;

                            var seq = DOTween.Sequence().SetUpdate(useUnscaledTime);
                            seq.Append(RectT.DOAnchorPos(past, fadeDuration).SetEase(slideEaseIn));
                            seq.Append(RectT.DOAnchorPos(InitialAnchoredPos, 0.15f).SetEase(Ease.OutSine));
                        }
                        else
                        {
                            var t = graphicHolder.transform;
                            var start = t.localPosition + (Vector3)SlideOffset(slideInFrom, slideDistance);
                            var past = (Vector3)InitialAnchoredPos - (Vector3)SlideOffset(slideInFrom, overshootPx);
                            t.localPosition = start;

                            var seq = DOTween.Sequence().SetUpdate(useUnscaledTime);
                            seq.Append(t.DOLocalMove(past, fadeDuration).SetEase(slideEaseIn));
                            seq.Append(t.DOLocalMove(InitialAnchoredPos, 0.15f).SetEase(Ease.OutSine));
                        }
                        break;
                    }
                    case EnterAnim.Bounce:
                    {
                        CanvasGroup.alpha = 0f;
                        graphicHolder.transform.localScale = Vector3.one * popScaleFrom;

                        FadeTw  = CanvasGroup.DOFade(1f, fadeDuration).SetUpdate(useUnscaledTime);
                        ScaleTw = graphicHolder.transform
                            .DOScale(1f, scaleDuration)
                            .SetEase(easeIn)
                            .SetUpdate(useUnscaledTime)
                            .OnComplete(() =>
                            {
                                graphicHolder.transform
                                    .DOPunchScale(punchScale, punchDuration, punchVibrato, punchElasticity)
                                    .SetUpdate(useUnscaledTime);
                            });
                        break;
                    }
                }
            }
            else
            {
                CanvasGroup.interactable = false;
                CanvasGroup.blocksRaycasts = false;

                if (instant)
                {
                    CanvasGroup.alpha = 0f;
                    if (RectT != null) RectT.anchoredPosition = InitialAnchoredPos; 
                    graphicHolder.transform.localScale = InitialLocalScale;
                    graphicHolder.SetActive(false);
                    return;
                }

                switch (exitAnim)
                {
                    case ExitAnim.None:
                        CanvasGroup.alpha = 0f;
                        graphicHolder.SetActive(false);
                        break;

                    case ExitAnim.FadeScalePop:
                        FadeTw  = CanvasGroup.DOFade(0f, fadeDuration).SetUpdate(useUnscaledTime);
                        ScaleTw = graphicHolder.transform.DOScale(popScaleFrom, scaleDuration)
                                .SetEase(easeOut)
                                .SetUpdate(useUnscaledTime)
                                .OnComplete(() => graphicHolder.SetActive(false));
                        break;

                    case ExitAnim.Slide:
                        if (RectT != null)
                        {
                            var to = InitialAnchoredPos + SlideOffset(slideOutTo, slideDistance);
                            FadeTw = RectT.DOAnchorPos(to, fadeDuration).SetEase(easeOut).SetUpdate(useUnscaledTime)
                                .OnComplete(() =>
                                {
                                    RectT.anchoredPosition = InitialAnchoredPos;
                                    graphicHolder.SetActive(false);
                                });
                        }
                        else
                        {
                            var t = graphicHolder.transform;
                            var to = t.localPosition + (Vector3)SlideOffset(slideOutTo, slideDistance);
                            FadeTw = t.DOLocalMove(to, fadeDuration).SetEase(easeOut).SetUpdate(useUnscaledTime)
                                .OnComplete(() =>
                                {
                                    if (RectT != null) RectT.anchoredPosition = InitialAnchoredPos;
                                    graphicHolder.SetActive(false);
                                });
                        }
                        break;
                    case ExitAnim.SlideOvershoot:
                    {
                        if (RectT != null)
                        {
                            var toPast = InitialAnchoredPos + SlideOffset(slideOutTo, overshootPx);
                            var toOff  = InitialAnchoredPos + SlideOffset(slideOutTo, slideDistance);

                            var seq = DOTween.Sequence().SetUpdate(useUnscaledTime);
                            seq.Append(RectT.DOAnchorPos(toPast, 0.12f).SetEase(Ease.InSine));
                            seq.Append(RectT.DOAnchorPos(toOff,  fadeDuration).SetEase(slideEaseOut));
                            seq.OnComplete(() =>
                            {
                                RectT.anchoredPosition = InitialAnchoredPos;
                                graphicHolder.SetActive(false);
                            });
                        }
                        else
                        {
                            var t = graphicHolder.transform;
                            var toPast = t.localPosition + (Vector3)SlideOffset(slideOutTo, overshootPx);
                            var toOff  = t.localPosition + (Vector3)SlideOffset(slideOutTo, slideDistance);

                            var seq = DOTween.Sequence().SetUpdate(useUnscaledTime);
                            seq.Append(t.DOLocalMove(toPast, 0.12f).SetEase(Ease.InSine));
                            seq.Append(t.DOLocalMove(toOff,  fadeDuration).SetEase(slideEaseOut));
                            seq.OnComplete(() => graphicHolder.SetActive(false));
                        }
                        break;
                    }
                    case ExitAnim.Bounce:
                    {
                        var seq = DOTween.Sequence().SetUpdate(useUnscaledTime);
                        seq.Append(graphicHolder.transform
                            .DOPunchScale(-punchScale * 0.6f, 0.15f, punchVibrato / 2, punchElasticity));
                        seq.Join(CanvasGroup.DOFade(0f, fadeDuration));
                        seq.OnComplete(() =>
                        {
                            graphicHolder.SetActive(false);
                            graphicHolder.transform.localScale = Vector3.one;
                        });
                        break;
                    }
                }
            }
        }
        protected Vector2 SlideOffset(SlideDirection dir, float dist)
        {
            switch (dir)
            {
                case SlideDirection.Left:  return new Vector2(-Mathf.Abs(dist), 0f);
                case SlideDirection.Right: return new Vector2( Mathf.Abs(dist), 0f);
                case SlideDirection.Top:   return new Vector2(0f,  Mathf.Abs(dist));
                case SlideDirection.Bottom:return new Vector2(0f, -Mathf.Abs(dist));
                default: return Vector2.zero;
            }
        }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }

    }
}
