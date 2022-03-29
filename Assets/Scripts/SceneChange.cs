using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour {
    public GameObject MixedRealityCameraParent;
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void LoadTutAR()
    {
        Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

        // Retrieve the name of this scene.
        string sceneName = currentScene.name;
        if (sceneName.Equals("TutAR"))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("VideoView");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("TutAR");

        }
    }

    public void DeactivateMainCamera()
    {
        MixedRealityCameraParent.SetActive(false);
    }
}
