using System.Collections;
using _Workspace._Scripts.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _Workspace._Scripts.UIGame
{
    public class ScClimbLoading : ScBaseUI
    {
        [Header("Loading")]
        [SerializeField] private Image progressFillImage; 
        [SerializeField] private CanvasGroup canvasGroup; 

        [Header("Setting")]
        [SerializeField] private float initialLoadTime = 5f; 
        [SerializeField] private float fadeOutTime = 1f; 
        [SerializeField] private bool randomizeSpeed = true; 
        [SerializeField] private float minTransitionTime = 1f; 
        
        private Coroutine _loadingCR;
        private bool _suppressOnEnableOnce;

        private bool _firstLoad = true; 

        protected override void Start()
        {
            if (progressFillImage != null)
            {
                progressFillImage.type = Image.Type.Filled; 
                progressFillImage.fillMethod = Image.FillMethod.Horizontal; 
                progressFillImage.fillOrigin = (int)Image.OriginHorizontal.Left; 
                progressFillImage.fillAmount = 0f; 
            } 
            StartCoroutine(ShowLoadingRoutine());
        }

        protected override void OnEnable()
        {
            if (_suppressOnEnableOnce) { _suppressOnEnableOnce = false; return; }
            
            if (!_firstLoad) 
            {
                if (progressFillImage != null)
                {
                    progressFillImage.fillAmount = 0f;
                }
                StartCoroutine(ShowLoadingRoutine());
            }
        }   
        
        public void TriggerLoading(bool useInitialDuration = false, float? durationOverride = null)
        {
            _suppressOnEnableOnce = true;
            StopLoadingIfRunning();

            if (progressFillImage != null) progressFillImage.fillAmount = 0f;
            SetCanvasVisibility(1f, true, true);

            if (useInitialDuration) _firstLoad = true;

            _loadingCR = StartCoroutine(ShowLoadingRoutineInternal(durationOverride));
        }
        
        private IEnumerator ShowLoadingRoutineInternal(float? durationOverride)
        {
            SetProgress(0f);
            SetCanvasVisibility(1f, true, true);

            float loadTime = durationOverride ?? (_firstLoad ? initialLoadTime : minTransitionTime);

            yield return StartCoroutine(FakeLoadingRoutine(loadTime));
            
            SCEventbus.Instance.RaiseFinishedLoading();

            yield return null;
            yield return StartCoroutine(FadeOutRoutine());

            SetUIActive(false);

            if (_firstLoad) _firstLoad = false;

            _loadingCR = null;
        }
        
        private void StopLoadingIfRunning()
        {
            if (_loadingCR != null)
            {
                StopCoroutine(_loadingCR);
                _loadingCR = null;
            }
        }
        private IEnumerator ShowLoadingRoutine()
        {
            SetProgress(0f); 
            SetCanvasVisibility(1f, true, true); 

            float loadTime = _firstLoad ? initialLoadTime : minTransitionTime;

            yield return StartCoroutine(FakeLoadingRoutine(loadTime));

            SCEventbus.Instance.RaiseFinishedLoading();
            
            yield return null;
            
            yield return StartCoroutine(FadeOutRoutine());

            SetUIActive(false);
            
            if (_firstLoad)
                _firstLoad = false;
            
        }

      
        private IEnumerator FakeLoadingRoutine(float duration)
        {
            float progress = 0f;
            float elapsed = 0f;

            while (progress < 1f)
            {
                elapsed += Time.deltaTime;
                float speed = randomizeSpeed ? Random.Range(0.5f, 2f) : 1f;

                progress = randomizeSpeed
                    ? Mathf.Clamp01(progress + (Time.deltaTime / duration) * speed)
                    : elapsed / duration;

                SetProgress(progress);
                yield return null;
            }
        }

      
        private IEnumerator FadeOutRoutine()
        {
            if(canvasGroup == null) yield break;

            canvasGroup.interactable = false; 

            float elapsed = 0f;
            while (elapsed < fadeOutTime)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutTime);
                canvasGroup.alpha = alpha;
                yield return null;
            }

            SetCanvasVisibility(0f, false, false);
        }
        private void SetProgress(float value)
        {
            if (progressFillImage != null)
            {
                progressFillImage.fillAmount = value;
            }
        }

        private void SetCanvasVisibility(float alpha, bool interactable, bool blocksRaycasts)
        {
            if (canvasGroup is null) return;
            canvasGroup.alpha = alpha;
            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = blocksRaycasts;
        }

        [ContextMenu("Restart Loading")]
        public void RestartLoading()
        {
            StopAllCoroutines();
            gameObject.SetActive(true);
            if (progressFillImage != null)
            {
                progressFillImage.fillAmount = 0f;
            }
            StartCoroutine(ShowLoadingRoutine());
        }
    }
    
}