using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonNavigation : MonoBehaviour
{
    [SerializeField] private string sceneName;

    public void LoadScene()
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("Cannot load scene: configure a scene name on ButtonNavigation.", this);
            return;
        }

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            int fileNameStart = scenePath.LastIndexOf('/') + 1;
            int extensionStart = scenePath.LastIndexOf('.');
            string buildSceneName = scenePath.Substring(fileNameStart, extensionStart - fileNameStart);

            if (buildSceneName != sceneName)
                continue;

            Time.timeScale = 1f;
            SceneManager.LoadScene(i);
            return;
        }

        Debug.LogError("Cannot load scene '" + sceneName + "': it is not included in Build Settings.", this);
    }
}