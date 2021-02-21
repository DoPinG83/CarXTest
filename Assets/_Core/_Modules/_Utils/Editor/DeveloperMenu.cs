using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Utils
{
    [InitializeOnLoadAttribute]
    [ExecuteInEditMode]
    public class DeveloperMenu
    {
        static bool playGameStarted;
        static DeveloperMenu()
        {
            EditorApplication.playModeStateChanged += ModeChanged;
        }

        static void ModeChanged(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == PlayModeStateChange.EnteredEditMode)
            {
                var lastScene = PlayerPrefs.GetString("lastScene");
                if (!string.IsNullOrEmpty(lastScene))
                {
                    EditorSceneManager.OpenScene(lastScene);
                    playGameStarted = false;
                }
            }
            else if(!playGameStarted && playModeStateChange == PlayModeStateChange.ExitingEditMode)
            {
                PlayerPrefs.SetString("lastScene", EditorSceneManager.GetActiveScene().path);
                PlayerPrefs.Save();
            }
        }

        [MenuItem("Dev/Start Play Game", false, 1)]
        public static void StartPlayGame()
        {
            
            if (!EditorApplication.isPlaying && !EditorApplication.isPaused)
            {
                PlayerPrefs.SetString("lastScene", EditorSceneManager.GetActiveScene().path);
                PlayerPrefs.Save();

                playGameStarted = true;
                
                var answer = EditorSceneManager.SaveModifiedScenesIfUserWantsTo( new Scene[] { EditorSceneManager.GetActiveScene() });
                if (answer)
                {
                    EditorSceneManager.OpenScene("Assets/_Scenes/Bootstrap.unity");
                    EditorApplication.ExecuteMenuItem("Edit/Play");
                }
            }
        }
    }
}