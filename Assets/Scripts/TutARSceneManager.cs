using UnityEngine;
using UnityEngine.SceneManagement;

public class TutARSceneManager : MonoBehaviour
{
    public GameObject MixedRealityCameraParent;

    public void Load(string sceneName)
    { 
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void DeactivateMainCamera()
    {
        MixedRealityCameraParent.SetActive(false);
    }
}