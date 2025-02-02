using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseGame : MonoBehaviour
{

    public GameObject canvasObj;
    bool gamePaused = false;

    void Start() {
        canvasObj.SetActive(false);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
         {
            if (canvasObj.activeSelf) // are we currently paused and showing the menu?
            {
                canvasObj.SetActive(false); // hide the menu
                Cursor.lockState = CursorLockMode.Locked; // hide cursor
                Cursor.visible = false; // Hide cursor
                Time.timeScale=1;    // unfreeze time
            }
            else
            {
                canvasObj.SetActive(true); // show the menu
                Cursor.lockState = CursorLockMode.None; // show cursor
                Cursor.visible = true; // Show cursor
                Time.timeScale=0;    // freeze time
            }
        }
    }

    
}