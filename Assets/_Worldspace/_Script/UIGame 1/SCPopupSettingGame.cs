using _Workspace._Scripts.Interfaces;
using _Workspace._Scripts.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Workspace._Scripts.UIGame
{
    public class ScPopupSettingGame : ScBaseUI
    {
        [Header("Scenes")]
        [SerializeField] private string mainMenuSceneName = "SC_Menu";
        
        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button resetButton;

        [Header("Sliders")]
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider bgmSlider;
        
        [Header("Canvas")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeInDuration;

        protected override void OnEnable()
        {
            base.OnEnable();
            closeButton?.onClick.AddListener(OnCloseButtonClicked);
            homeButton?.onClick.AddListener(OnHomeButtonClicked);
            resetButton?.onClick.AddListener(OnresetButtonClicked);
            sfxSlider?.onValueChanged.AddListener(OnSfxVolumeChanged);
            bgmSlider?.onValueChanged.AddListener(OnBgmVolumeChanged);

        }

        protected override void OnDisable()
        {
            base.OnDisable();
            closeButton?.onClick.RemoveListener(OnCloseButtonClicked);
            homeButton?.onClick.RemoveListener(OnHomeButtonClicked);
            resetButton?.onClick.RemoveListener(OnresetButtonClicked);
            sfxSlider?.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            bgmSlider?.onValueChanged.RemoveListener(OnBgmVolumeChanged);
        }

        private void OnCloseButtonClicked()
        {
            ScAudioManager.instance.PlaySfx("UI");
            if (ScGameManager.instance is null) return;
            ScGameManager.instance.ScSetState(GameState.Resume);
            SetUIActive(false);

        }

        private void OnHomeButtonClicked()
        {
            ScGameManager.instance.ScSetState(GameState.Home);
            ScAudioManager.instance.PlaySfx("UI");
            SetUIActive(false);
            SceneManager.LoadScene(mainMenuSceneName);
            SCEventbus.Instance.RaiseGameHome();

        }
        private void OnresetButtonClicked()
        {
            ScAudioManager.instance.PlaySfx("UI");
            ScGameManager.instance.ScSetState(GameState.InGame);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            SetUIActive(false);
            SCEventbus.Instance.RaiseGameRestart();
        }
        private void OnSfxVolumeChanged(float value)
        {
            ScAudioManager.instance.SetSfxVolume(value);
        }

        private void OnBgmVolumeChanged(float value)
        {
            ScAudioManager.instance.SetBGMVolume(value);
        }

        public void ShowPopup()
        {
            SetUIActive(true);
            RefreshSlidersFromVolume();
        }
        private void RefreshSlidersFromVolume()
        {
            if (sfxSlider is not null && ScAudioManager.instance is not null)
                sfxSlider.SetValueWithoutNotify(ScAudioManager.instance.GetSfxVolume());
            if (bgmSlider is not null && ScAudioManager.instance is not null)
                bgmSlider.SetValueWithoutNotify(ScAudioManager.instance.GetBGMVolume());
        }
    }
}
