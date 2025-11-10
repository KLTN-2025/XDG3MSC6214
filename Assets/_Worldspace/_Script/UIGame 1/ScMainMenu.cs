using _Workspace._Scripts.Interfaces;
using _Workspace._Scripts.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Workspace._Scripts.UIGame
{
    public class ScMainMenu : ScBaseUI
    {
    
        [Header("Scene Setting")]
        [SerializeField] private string targetSceneName = "SC_Game";
    

        [SerializeField] private Button playBtn;
        [SerializeField] private Button settingBtn;
        [SerializeField] private Button quesBtn;
        [SerializeField] private Button customBtn;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (playBtn != null) playBtn.onClick.AddListener(OnPlayButtonClicked);
            if (settingBtn != null) settingBtn.onClick.AddListener(OnSettingButtonClicked);
            if (quesBtn != null) quesBtn.onClick.AddListener(OnQuestButtonClicked);
            if (customBtn != null) customBtn.onClick.AddListener(OnCustomizationClicked);

        }
        protected override void OnDisable()
        {
            base.OnDisable();
            if (playBtn != null) playBtn.onClick.RemoveListener(OnPlayButtonClicked);
            if (settingBtn != null) settingBtn.onClick.RemoveListener(OnSettingButtonClicked);
            if (quesBtn != null) quesBtn.onClick.RemoveListener(OnQuestButtonClicked);
            if (customBtn != null) customBtn.onClick.RemoveListener(OnCustomizationClicked);
        }

        private void OnPlayButtonClicked()
        {
            ScAudioManager.instance.PlaySfx("UI");
            ScGameManager.instance.ScSetState(GameState.InGame);
            SceneManager.LoadScene(targetSceneName);

        }
        private void OnSettingButtonClicked()
        {
            ScAudioManager.instance.PlaySfx("UI");
        
            ScUIManager.instance.PopupSettingMenu?.ShowPopup();
        }
        private void OnQuestButtonClicked()
        {
            ScAudioManager.instance.PlaySfx("UI");
            ScUIManager.instance.Tutorial.ShowTutorial();
        }
        private void OnCustomizationClicked()
        {
            ScAudioManager.instance.PlaySfx("UI");
            ScUIManager.instance.SharkSelectPanel.Open();
        }

    }
}
