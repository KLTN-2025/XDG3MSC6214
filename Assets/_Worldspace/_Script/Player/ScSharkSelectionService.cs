using UnityEngine;

namespace _Workspace._Scripts.Player
{
    public class ScSharkSelectionService
    {
        private const string key = "SC_SELECTED_SKIN";

        public static int Get(int fallbackId) => PlayerPrefs.GetInt(key, fallbackId);

        public static void Set(int id)
        {
            PlayerPrefs.SetInt(key, id);
            PlayerPrefs.Save();
        }
    }
}
