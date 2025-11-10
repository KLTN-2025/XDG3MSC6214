using _Workspace._Scripts.Interfaces;
using _Workspace._Scripts.Managers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Workspace._Scripts.UIGame
{
    public class ScTapToPlay : ScBaseUI, IPointerClickHandler
    {
        [SerializeField] private GameObject tapToPlayPanel;  
        [SerializeField] private ScInGame inGameUI;

        private bool _hasStarted;
        public void SetState(UIState newState)
        {
            switch (newState)
            {
                case UIState.Menu:
                    _hasStarted = false;
                    tapToPlayPanel?.SetActive(false);
                    inGameUI?.SetUIActive(false);
                    break;
                case UIState.Game:
                    tapToPlayPanel?.SetActive(true);
                    inGameUI?.SetUIActive(true);
                    break;
                case UIState.Pause:
                case UIState.Resume:
                case UIState.End:
                    break;
            }
        }
    
        public void ShowTapToPlay()
        {
            _hasStarted = false;
            tapToPlayPanel?.SetActive(true);
            inGameUI?.SetUIActive(false);
        }

        private void HideTapToPlay()
        {
            if (_hasStarted) return;
            _hasStarted = true;

            tapToPlayPanel?.SetActive(false);
            inGameUI?.SetUIActive(true);
        }
    
        public void OnPointerClick(PointerEventData eventData)
        {
            HideTapToPlay();
            ScGameManager.instance.ScSetState(GameState.Playing);
        }
    }
}
