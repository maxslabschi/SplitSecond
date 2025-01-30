using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

public class EndLevel : MonoBehaviour
{
    public string nextScene;
    public string level;

    private Timer timer;

    void Start()
    {
        timer = FindObjectOfType<Timer>(); 
    }

    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("Is colliding");
        if (collision.CompareTag("Player"))
        {
            postTime();
            Debug.Log("LoadNextScene");
            SceneManager.LoadScene(nextScene);
        }
    }

    private void postTime()
    {
        if (timer != null)
        {
            string name = PlayerPrefs.GetString("user_name", "Anonym");
            float time = timer.getTime();

            Debug.Log("Posting: " + name + ", Time: " + time);

            StartCoroutine(PostRequest(name, time)); // Send request
        }
        else
        {
            Debug.LogError("Timer script not found in the scene!");
        }
    }

    private IEnumerator PostRequest(string name, float time)
    {
        //string url = "https://2c6a-193-170-158-244.ngrok-free.app/level/" + level + "/scores";
        string url = "https://it200250.cloud.htl-leonding.ac.at/splitsecond-api/level/" + level + "/scores";
        // Create JSON data
        string jsonData = JsonUtility.ToJson(new TimeData(name, time));

        // Convert to bytes
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

        // Create request
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Send request
        yield return request.SendWebRequest();

        // Handle response
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("POST Successful: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("POST Failed: " + request.error);
        }
    }
}

[System.Serializable]
public class TimeData
{
    public string username;
    public float time;

    public TimeData(string username, float time)
    {
        this.username = username;
        this.time = time;
    }
}