using System.Collections;
using _Workspace._Scripts.Interfaces;
using _Workspace._Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Workspace._Scripts.UIGame
{
    public class ScTutorialPanel : ScBaseUI
    {
        [System.Serializable]
        public class ScTutorialStep
        {
            public string title;
            public Sprite image;
            [TextArea]
            public string description;
        }
        
        [Header("Button")]
        [SerializeField] private Button closePanel;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button previousButton;
        
        [Header("Text")]
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("Panels")]
        [SerializeField] private Image tutorialImage;
        [SerializeField] private ScTutorialStep[] steps;
        private int _currentStep;

        [Header("Canvas")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeInDuration;
        
        private int _currentPage;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            previousButton.onClick.AddListener(Previous);
            nextButton.onClick.AddListener(Next);
            closePanel.onClick.AddListener(Return);
            UpdateTutorialUI();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            previousButton.onClick.RemoveListener(Previous);
            nextButton.onClick.RemoveListener(Next);
            closePanel.onClick.RemoveListener(Return);
        }
        
        private void Previous()
        {
            ScAudioManager.instance.PlaySfx("UI");
            if (_currentStep <= 0) return;
            _currentStep--;
            UpdateTutorialUI();
        }

        private void Next()
        {
            ScAudioManager.instance.PlaySfx("UI");
            if (_currentStep >= steps.Length - 1) return;
            _currentStep++;
            UpdateTutorialUI();
        }

        private void Return()
        {
            ScAudioManager.instance.PlaySfx("UI");
            HideTutorial();
        }

        private void UpdateTutorialUI()
        {
            var step = steps[_currentStep];
            tutorialImage.sprite = step.image;
            descriptionText.text = step.description;
            
            previousButton.interactable = _currentStep > 0;
            nextButton.interactable = _currentStep < steps.Length - 1;
        }
        
        public void ShowTutorial()
        {
            if (graphicHolder != null)
                graphicHolder.SetActive(true);
            if (canvasGroup != null)
            {
                StopAllCoroutines();
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                StartCoroutine(FadeInTutorial());
            }
        }

        private void HideTutorial()
        {
            if (canvasGroup != null)
            {
                StopAllCoroutines();
                StartCoroutine(FadeOutTutorial());
            }
            else
            {
                SetUIActive(false);
            }
        }

        private IEnumerator FadeInTutorial()
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / fadeInDuration;
                if (canvasGroup != null)
                    canvasGroup.alpha = Mathf.Lerp(0, 1, t);
                yield return null;
            }
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        private IEnumerator FadeOutTutorial()
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / fadeInDuration;
                if (canvasGroup != null)
                    canvasGroup.alpha = Mathf.Lerp(1, 0, t);
                yield return null;
            }
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            SetUIActive(false);
        }
    }
}
