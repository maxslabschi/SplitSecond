using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;

public class Leaderboard : MonoBehaviour
{
    public TextMeshProUGUI leaderBoardText;
    public string level;

    public void FetchTopScores()
    {
        StartCoroutine(GetRequest("https://it200250.cloud.htl-leonding.ac.at/splitsecond-api/level/" + level + "/scores?limit=3"));
    }

    void Start() {
        FetchTopScores();
    }

    private IEnumerator GetRequest(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Accept", "application/json");

            // Send request
            yield return request.SendWebRequest();

            // Handle response
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("GET Successful: " + request.downloadHandler.text);

                // Parse JSON response
                ScoreData[] scores = JsonUtility.FromJson<ScoreList>("{\"scores\":" + request.downloadHandler.text + "}").scores;

                // Update the UI
                UpdateLeaderboardUI(scores);
            }
            else
            {
                Debug.LogError("GET Failed: " + request.error);
                leaderBoardText.text = "Failed to load leaderboard.";
            }
        }
    }

    private void UpdateLeaderboardUI(ScoreData[] scores)
    {
        // Sort scores by time (ascending order: fastest time first)
        Array.Sort(scores, (a, b) => a.time.CompareTo(b.time));

        leaderBoardText.text = "<b>Leaderboard:</b>\n";

        for (int i = 0; i < scores.Length; i++)
        {
            leaderBoardText.text += $"{i + 1}. {scores[i].username} - {scores[i].time:F2}s\n";
        }
    }
}

[System.Serializable]
public class ScoreData
{
    public string username;
    public float time;
    public string date; // Keep as string since DateTime parsing can vary
}

[System.Serializable]
public class ScoreList
{
    public ScoreData[] scores;
}
