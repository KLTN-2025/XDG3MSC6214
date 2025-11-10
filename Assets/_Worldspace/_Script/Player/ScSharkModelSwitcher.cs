using System.Collections.Generic;
using UnityEngine;

namespace _Workspace._Scripts.Player
{
    public class ScSharkModelSwitcher : MonoBehaviour
    {
        [Header("Where the models live")]
        [SerializeField] private Transform modelsRoot;
        [SerializeField] private string namePrefix = "Dino_Shark"; 

        [Header("Prefs")]
        [SerializeField] private string prefsKey = "SelectedShark";
        [SerializeField] private int defaultIndex = 0;
        
        private readonly List<GameObject> _models = new();
        public int CurrentIndex { get; private set; } = -1;

        void Awake()
        {
            if (modelsRoot == null) modelsRoot = transform; 
            
            _models.Clear();
            for (int i = 0; i < modelsRoot.childCount; i++)
            {
                var c = modelsRoot.GetChild(i);
                if (c.name.StartsWith(namePrefix)) _models.Add(c.gameObject);
            }
        }

        public void ApplyFromPrefs()
        {
            int idx = PlayerPrefs.GetInt(prefsKey, defaultIndex);
            ApplyIndex(idx);
        }

        public void ApplyIndex(int idx)
        {
            if (_models.Count == 0) return;

            idx = Mathf.Clamp(idx, 0, _models.Count - 1);
            CurrentIndex = idx;

            for (int i = 0; i < _models.Count; i++)
                if (_models[i]) _models[i].SetActive(i == idx);
        }
        
        public Animator GetActiveAnimator()
        {
            for (int i = 0; i < modelsRoot.childCount; i++)
            {
                var child = modelsRoot.GetChild(i);
                if (child.gameObject.activeInHierarchy)
                    return child.GetComponentInChildren<Animator>(true);
            }
            return null;
        }
    }
}
