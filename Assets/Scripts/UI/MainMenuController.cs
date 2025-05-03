using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [System.Serializable]
    public class SceneButton
    {
        public string buttonName;
        public string sceneName;
    }

    [Header("Scene Settings")]
    [SerializeField] private SceneButton startButton = new SceneButton { buttonName = "START", sceneName = "GameScene" };
    [SerializeField] private SceneButton tutorialButton = new SceneButton { buttonName = "TUTORIAL", sceneName = "TutorialScene" };
    [SerializeField] private SceneButton creditsButton = new SceneButton { buttonName = "CREDITS", sceneName = "CreditsScene" };

    public void LoadStartScene()
    {
        LoadScene(startButton.sceneName);
    }

    public void LoadTutorialScene()
    {
        LoadScene(tutorialButton.sceneName);
    }

    public void LoadCreditsScene()
    {
        LoadScene(creditsButton.sceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private void LoadScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            Debug.Log("Loading scene: " + sceneName);
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Scene name is empty. Please assign a scene name in the inspector.");
        }
    }
}