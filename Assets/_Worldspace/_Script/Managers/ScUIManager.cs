using _Workspace._Scripts.ISingleton;
using _Workspace._Scripts.UIGame;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Workspace._Scripts.Managers
{
    public enum UIState
    {
        Menu,
        Game,
        Pause,
        Resume,
        End
    }
    public class ScUIManager : ISingleton<ScUIManager>
    {
        [SerializeField] private string menuSceneName = "SC_Menu";
        [SerializeField] private string gameSceneName = "SC_Game";

        [SerializeField] private ScMainMenu scMainMenu;
        [SerializeField] private ScInGame scInGame;
        [SerializeField] private ScClimbLoading loading;
        [SerializeField] private ScTutorialPanel tutorial;
        [SerializeField] private ScSharkSelectPanel scSharkSelectPanel;
        [SerializeField] private ScPopupSettingGame popupSettingGame;
        [SerializeField] private ScPopupSettingMenu popupSettingMenu;
        [SerializeField] private ScTapToPlay tapToPlay;
    
        public bool enableSceneUIHandling = true;

        [Header("properties")]
        public ScTutorialPanel Tutorial => tutorial;
        public ScSharkSelectPanel SharkSelectPanel => scSharkSelectPanel;
        public ScPopupSettingGame PopupSettingGame => popupSettingGame;
        public ScPopupSettingMenu PopupSettingMenu => popupSettingMenu;
        public ScClimbLoading Loading => loading;
        private ScTapToPlay TapToPlay => tapToPlay;


        [SerializeField] private UIState currentState;
    

        public void SetState(UIState newState)
        {
            currentState = newState;
            UpdateAllUI(); 
        }
    
        private void UpdateAllUI()
        {
        
            switch (currentState)
            {
                case UIState.Menu:
                    ScHomePanel(); 
                    break;
                case UIState.Game:
                    ScHandlePlaying(); 
                    break;
                case UIState.Pause:
                    ScHandlePauseUIState(); 
                    break;
                case UIState.Resume:
                    ScHandleResumeUIState(); 
                    break;
                case UIState.End:
                    ScHandleLoseUiState(); 
                    break;
                default:
                    Debug.LogWarning("Unhandled UIState: " + currentState);
                    break;
            }
        }
    
        private void ScHomePanel()
        {
            scMainMenu.SetUIActive(true, instant: true);
            scInGame.SetUIActive(false);
        }
    
        private void ScHandlePlaying()
        {
            scMainMenu.SetUIActive(false);
            scInGame.SetUIActive(true, instant: true);
            SceneManager.sceneLoaded += OnGameSceneLoaded;
        }
        private void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == gameSceneName)
            {
                TapToPlay.ShowTapToPlay();
                scMainMenu.SetUIActive(false);
                scInGame.SetUIActive(true);
            }
            SceneManager.sceneLoaded -= OnGameSceneLoaded;
        }
    
        private void ScHandlePauseUIState()
        {
            popupSettingGame.ShowPopup();
            scInGame.SetUIActive(false, instant: true);
        }

        private void ScHandleResumeUIState()
        {
            popupSettingGame.SetUIActive(false);
            scInGame.SetUIActive(true,  instant: true);
        }

        private void ScHandleLoseUiState()
        {
            scInGame.ShowEndPanel(true);
        }
    
        public void ProgressScPanel()
        {
            loading.SetUIActive(true, instant: true);
            loading.TriggerLoading();
        }
        public void ReturnToMenu()
        {
            SceneManager.LoadScene(menuSceneName);
            SetState(UIState.Menu);
        }    

    }
}