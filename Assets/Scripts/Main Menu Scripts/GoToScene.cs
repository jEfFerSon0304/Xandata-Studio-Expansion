using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToScene : MonoBehaviour
{
    [Header("Scene to Load")]
    public string sceneName; // set this in the Inspector

    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
    }
}
