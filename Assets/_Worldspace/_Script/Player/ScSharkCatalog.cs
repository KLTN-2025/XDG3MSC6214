using UnityEngine;

namespace _Workspace._Scripts.Player
{
    [CreateAssetMenu(menuName = "SharkCatch/Shark Catalog", fileName = "SharkCatalog")]
    public class ScSharkCatalog : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            public int id;
            public string displayName;
            public Sprite icon;
            public GameObject previewPrefab;
        }

        public Entry[] skins;
        public int Count => skins?.Length ?? 0;
        public Entry Get(int i) => (skins != null && i >= 0 && i < skins.Length) ? skins[i] : null;
        public int IndexOfId(int id) {
            if (skins == null) return -1;
            for (int i = 0; i < skins.Length; i++) if (skins[i].id == id) return i;
            return -1;
        }
    }
}
