using UnityEngine;
using UnityEngine.SceneManagement;

public class loadScene : MonoBehaviour
{
  


    public void LoadScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
        Debug.Log("Button Pressed");
    }

    public void LoadScnenAsync(string name) {
        SceneManager.LoadSceneAsync(name);
    }

    // called first
    void Awake()
    {
        Debug.Log("Awake");
    }

    // called second
    void OnEnable()
    {
        Debug.Log("OnEnable called");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // called third
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);
    }

    // called fourth
    void Start()
    {
        Debug.Log("Start");
    }

    // called when the game is terminated
    void OnDisable()
    {
        Debug.Log("OnDisable");
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
