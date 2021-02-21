namespace Core.Editor {
    using UnityEngine;

    public class ClearPlayerPrefs : MonoBehaviour
    {
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Clear Player Prefs")]
        public static void ClearPrefs()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("Player prefs deleted!");
        }
        #endif
    }
}
