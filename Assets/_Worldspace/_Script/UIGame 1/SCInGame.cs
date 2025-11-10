using System.Collections.Generic;
using _Workspace._Scripts.Interfaces;
using _Workspace._Scripts.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Workspace._Scripts.UIGame
{
    public class ScInGame : ScBaseUI
    {
        [Header("Buttons")]
        [SerializeField] private Button settingButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button homeButton;
        
        [Header("Panels")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject warningPanel;
        
        [Header("Score")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private float scorePunchAmount = 0.25f;
        [SerializeField] private float scorePunchDuration = 0.2f;
        [SerializeField] private bool punchScoreOnChange = true;
        
        [Header("Lives")]
        [SerializeField] private Transform heartContainer;
        [SerializeField] private Image heartPrefab;
        [SerializeField] private Sprite heartFull, heartEmpty;
        [SerializeField] private bool shakeHeartOnLose = true;
        [SerializeField] private bool useHearts = true;
        [SerializeField] private int shakeVibration;
        private readonly List<Image> _hearts = new List<Image>();
        private int _lastLives;
        private int _maxLives;
        private int _currentScore;
        private Tweener _scorePunchTw;
        
        private bool _gamePause;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (settingButton == null) return;
            settingButton.onClick.AddListener(OnSettingGame);
            resetButton.onClick.AddListener(HandleResetButton);
            homeButton.onClick.AddListener(HandleHomeButtonClicked);
            
            HandleLivesChanged(_lastLives, _maxLives);

            SCEventbus.Instance.OnScoreChanged += HandleScoresChanged;
            SCEventbus.Instance.OnLivesChanged += HandleLivesChanged;
            SCEventbus.Instance.OnBoatStartRun += ShowBoatWarning;
            SCEventbus.Instance.OnGameRestart += HideWarning;
            SCEventbus.Instance.OnGameHome += HideWarning;
        }
    
        protected override void OnDisable()
        {
            base.OnDisable();
            if (settingButton == null) return;
            settingButton.onClick.RemoveListener(OnSettingGame);
            resetButton.onClick.RemoveListener(HandleResetButton);
            homeButton.onClick.RemoveListener(HandleHomeButtonClicked);
            SCEventbus.Instance.OnScoreChanged -= HandleScoresChanged;
            SCEventbus.Instance.OnLivesChanged -= HandleLivesChanged;
            SCEventbus.Instance.OnBoatStartRun -= ShowBoatWarning;
            SCEventbus.Instance.OnGameRestart -= HideWarning;
            SCEventbus.Instance.OnGameHome -= HideWarning;
        }
        
        public void GamePause()
        {
            if (_gamePause) return;
            Time.timeScale = 0f;   
            _gamePause = true;
        }
        public void ResumeGame()
        {
            if (!_gamePause) return;
            Time.timeScale = 1f;   
            _gamePause = false;
        }
    
        #region On Button

        private void OnSettingGame()
        {
            ScAudioManager.instance.PlaySfx("UI");
            ScGameManager.instance.ScSetState(GameState.Pause);

        }
        #endregion

        public void ShowEndPanel(bool active)
        {
            SetUIActive(false);
            gameOverPanel?.SetActive(active);
            if (active && highScoreText != null)
            {
                var cg = gameOverPanel?.GetComponent<CanvasGroup>();
                if(cg == null) cg = gameOverPanel?.AddComponent<CanvasGroup>();

                if (cg != null)
                {
                    cg.alpha = 0f;
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                    if (gameOverPanel != null)
                    {
                        gameOverPanel.transform.localScale = Vector3.one * 0.8f;

                        cg.DOFade(1f, 0.5f).SetEase(Ease.OutCubic).SetUpdate(true);
                        gameOverPanel.transform.DOScale(1, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
                    }
                }

                if (highScoreText != null)
                {
                    highScoreText.text = _currentScore.ToString() + "x";
                    highScoreText.transform.DOPunchScale(Vector3.one * 0.3f, 0.6f, 6, 0.7f).SetUpdate(true);
                }
            }

            if (active || gameOverPanel == null) return;
            {
                var cg = gameOverPanel.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.DOFade(0f, 0.3f).OnComplete(() =>
                    {
                        gameOverPanel.SetActive(false);
                    });
                }
                else gameOverPanel.SetActive(false);
            }
        }
        
        private void ShowBoatWarning()
        {
            warningPanel.SetActive(true);
            var cg = warningPanel.GetComponent<CanvasGroup>();
            if (!cg) cg = warningPanel.AddComponent<CanvasGroup>();

            cg.alpha = 0f;
            cg.DOFade(1f, 0.3f)
                .SetLoops(11, LoopType.Yoyo)
                .OnComplete(() =>
                {
                    cg.alpha = 0f;
                    warningPanel.SetActive(false);
                });
        }
        
        private void HideWarning()
        {
            warningPanel.SetActive(false);
        }
        
    
        private void OnDestroy()
        {
            if(settingButton != null)
            {
                Destroy(settingButton.transform.parent.gameObject);
            }
        }

        private void HandleResetButton()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            ScGameManager.instance.ScSetState(GameState.InGame);
            gameOverPanel.SetActive(false);
            if(warningPanel.activeInHierarchy) 
                warningPanel.SetActive(false);
        }

        private void HandleHomeButtonClicked()
        {
            ShowEndPanel(false);
            SceneManager.LoadScene("SC_Menu");
            ScGameManager.instance.ScSetState(GameState.Home);
            HideWarning();
        }
        
        private void HandleScoresChanged(int newScore)
        {
            _currentScore = newScore;
            if (scoreText == null) return;
            scoreText.text = newScore.ToString() + "x";

            if (!punchScoreOnChange) return;
            _scorePunchTw?.Kill();
            Transform t = scoreText.transform;
            t.localScale = Vector3.one;
            _scorePunchTw = t.DOPunchScale(Vector3.one * scorePunchAmount, scorePunchDuration, 6, 0.9f);
        }
        private void HandleLivesChanged(int current, int max)
        {
            int preLives = _lastLives;
            _lastLives = current;
            _maxLives = Mathf.Max(1, max);
            
            if(useHearts) DrawHearts(_lastLives, _maxLives, preLives);
            
        }

        private void DrawHearts(int current, int max, int preLives)
        {
            if (_hearts.Count != max)
            {
                RebuildHeart(max);
            }
            
            for (int i = 0; i < _hearts.Count; i++)
            {
               Image img = _hearts[i];
               bool isFull = i < current;
               if(img != null)
                   img.sprite = isFull ? heartFull : heartEmpty;
            }

            if (preLives < 0 || current == preLives) return;
            {
                if (current > preLives)
                {
                    int from = Mathf.Clamp(preLives, 0, _hearts.Count);
                    int to   = Mathf.Clamp(current,   0, _hearts.Count);
                    for (int i = from; i < to; i++)
                    {
                        var img = _hearts[i];
                        if (!img) continue;
                        var rt = img.rectTransform;
                        rt.DOKill();
                        rt.localScale = Vector3.one;
                        rt.DOPunchScale(Vector3.one * 0.25f, 0.25f, 8, 0.7f);
                    }
                }
                else
                {
                    if (!shakeHeartOnLose) return;
                    int from = Mathf.Clamp(current,   0, _hearts.Count);
                    int to   = Mathf.Clamp(preLives,  0, _hearts.Count);
                    for (int i = from; i < to; i++)
                    {
                        var img = _hearts[i];
                        if (!img) continue;
                        var rt = img.rectTransform;
                        rt.DOKill();
                        rt.DOShakeScale(0.25f, 0.2f, shakeVibration, 90f).OnComplete(() => rt.localScale = Vector3.one);
                    }
                }
            }
        }

        private void RebuildHeart(int max)
        {
            foreach (Transform c in heartContainer) Destroy(c.gameObject);
            _hearts.Clear();
            for (int i = 0; i < max; i++)
            {
                Image img = Instantiate(heartPrefab, heartContainer);
                img.transform.localScale = Vector3.one;
                img.sprite = heartFull;
                _hearts.Add(img);
            }
        }
    }
}
