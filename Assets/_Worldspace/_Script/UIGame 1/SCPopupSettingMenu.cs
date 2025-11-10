using _Workspace._Scripts.Interfaces;
using _Workspace._Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace _Workspace._Scripts.UIGame
{
    public class ScPopupSettingMenu : ScBaseUI
    {
        [Header("Setting UI")]
        [SerializeField] private GameObject settingPanel;
        [SerializeField] private Button home;

        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider bgmSlider;

        private void RefreshSlidersFromVolume()
        {
            if (sfxSlider is not null && ScAudioManager.instance is not null)
                sfxSlider.SetValueWithoutNotify(ScAudioManager.instance.GetSfxVolume());
            if (bgmSlider is not null && ScAudioManager.instance is not null)
                bgmSlider.SetValueWithoutNotify(ScAudioManager.instance.GetBGMVolume());

        }
        protected override void Awake()
        {
            base.Awake();
            if (settingPanel == null)
                settingPanel = this.gameObject;
            if (home != null)
                home.onClick.AddListener(OnHomeButton);
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            sfxSlider?.onValueChanged.AddListener(OnSfxVolumeChanged);
            bgmSlider?.onValueChanged.AddListener(OnBgmVolumeChanged);
            RefreshSlidersFromVolume();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            sfxSlider?.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            bgmSlider?.onValueChanged.RemoveListener(OnBgmVolumeChanged);
        }
        private void OnHomeButton()
        {
            ScAudioManager.instance.PlaySfx("UI");
            SetUIActive(false);
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
    }
}
