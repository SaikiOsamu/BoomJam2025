using UnityEngine;
using UnityEngine.SceneManagement;
public class sceneManagement : MonoBehaviour
{
    public string scenename;
    public void goToScene()
    {
        SceneManager.LoadScene(scenename);
    }
}
