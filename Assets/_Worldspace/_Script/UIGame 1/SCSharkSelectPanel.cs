using _Workspace._Scripts.Interfaces;
using _Workspace._Scripts.Managers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Workspace._Scripts.UIGame
{
    public class ScSharkSelectPanel : ScBaseUI
    {
        
        [Header("Refs")]
        [SerializeField] private CanvasGroup panel;
        [SerializeField] private Transform previewRoot;
        [SerializeField] private RawImage previewImage;
        [SerializeField] private int defaultIndex;
        [SerializeField] private float rotationOnSwitch;
        [SerializeField] private float rotationDuration;

        [Header("Tween")]
        [SerializeField] private float fadeIn, fadeOut;
        [SerializeField] private float popInFrom;
        [SerializeField] private Ease popEaseIn = Ease.OutBack;
        [SerializeField] private Ease popEaseOut = Ease.InCubic;
        
        [Header("Buttons")]
        [SerializeField] private Button nextButton;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button chooseButton;

        [Header("Prefs")] 
        [SerializeField] private string prefsKey = "SelectedShark";

        private int index;
        private Transform[] sharks;
        private Tweener fadeTw, scaleTw, spinTw;

        protected override void Awake()
        {
            base.Awake();
            if (previewImage is null)
            {
                Debug.LogError("Preview image is null");
            }

            if (panel == null) panel = graphicHolder.AddComponent<CanvasGroup>();

            if (previewRoot == null) return;
            sharks = new Transform[previewRoot.childCount];
            for (int i = 0; i < sharks.Length; i++)
            {
                sharks[i] = previewRoot.GetChild(i);
            }
        }

        protected override void Start()
        {
            index = Mathf.Clamp(PlayerPrefs.GetInt(prefsKey, defaultIndex), 0, (sharks?.Length ?? 1) - 1);
            ApplyIndex();
            graphicHolder.SetActive(false);
            panel.alpha = 0f;
            graphicHolder.transform.localScale = Vector3.one;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            nextButton.onClick.AddListener(Next);
            previousButton.onClick.AddListener(Previous);
            chooseButton.onClick.AddListener(Choose);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            nextButton.onClick.RemoveListener(Next);
            previousButton.onClick.RemoveListener(Previous);
            chooseButton.onClick.RemoveListener(Choose);
            KillTween();
        }
        
        public void Open()
        {
            KillTween();
            graphicHolder.SetActive(true);
            panel.alpha = 0f;
            panel.interactable = true;
            panel.blocksRaycasts = true;
            graphicHolder.transform.localScale = Vector3.one * popInFrom;

            fadeTw = panel.DOFade(1f, fadeIn);
            scaleTw = graphicHolder.transform.DOScale(1, fadeIn).SetEase(popEaseIn);
        }

        private void Close()
        {
            KillTween();
            fadeTw = panel.DOFade(0f, fadeOut);
            scaleTw = graphicHolder.transform.DOScale(0.97f, fadeOut).SetEase(popEaseOut).OnComplete(() =>
            {
                graphicHolder.transform.localScale = Vector3.one;
                graphicHolder.SetActive(false);
            });;
        }

        private void Next()
        {
            ScAudioManager.instance.PlaySfx("UI");
            if(sharks is null || sharks.Length == 0) return;
            index = (index + 1) % sharks.Length;
            ApplyIndex();
        }

        private void Previous()
        {
            ScAudioManager.instance.PlaySfx("UI");
            if(sharks is null || sharks.Length == 0) return;
            index = (index - 1 + sharks.Length) % sharks.Length;
            ApplyIndex();
        }

        private void Choose()
        {
            ScAudioManager.instance.PlaySfx("UI");
            PlayerPrefs.SetInt(prefsKey, index);
            PlayerPrefs.Save();
            Close();
        }
        
        private void ApplyIndex()
        {
            if (sharks is null) return;
            for (int i = 0; i < sharks.Length; i++)
            {
                if(sharks[i]) sharks[i].gameObject.SetActive(i == index);

                if (sharks[index] is null || rotationOnSwitch == 0f || !(rotationDuration > 0f)) continue;
                if(spinTw != null && spinTw.IsActive()) spinTw.Kill();
                var t = sharks[index];
                t.localRotation = Quaternion.identity;
                spinTw = t.DORotate(new Vector3(0f, rotationOnSwitch, 0f), rotationDuration,
                    RotateMode.FastBeyond360);
            }
        }

        private void KillTween()
        {
            if(fadeTw != null && fadeTw.IsActive()) fadeTw.Kill();
            if(scaleTw != null && scaleTw.IsActive()) scaleTw.Kill();
            if(spinTw != null && spinTw.IsActive()) spinTw.Kill();
            fadeTw = null;
            scaleTw = null;
            spinTw = null;
        }
        
    }
}
