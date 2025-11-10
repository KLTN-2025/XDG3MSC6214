using _Workspace._Scripts.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace _Workspace._Scripts.UIGame
{
    public class ScCustomization : ScBaseUI
    {
        [SerializeField] private GameObject customizationPanel;
        [SerializeField] private Button closeBT;

        protected override void Awake()
        {
            base.Awake();
            if (customizationPanel == null)
                customizationPanel = this.gameObject;
            if (closeBT != null)
                closeBT.onClick.AddListener(OnCloseButtonClicked);
        }
        protected override void Start()
        {
            base.Start();
            SetUIActive(setActiveOnStart);
        }
        private void OnCloseButtonClicked()
        {
            SetUIActive(false);
        }
    }
}
