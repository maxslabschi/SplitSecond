using UnityEngine;
using UnityEngine.SceneManagement;

public class EndLevel : MonoBehaviour
{
    public string nextScene;

    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("Is colliding");
        if (collision.CompareTag("Player"))
        {
            Debug.Log("LoadNextScene");
            SceneManager.LoadScene(nextScene);
        }
    }
}
