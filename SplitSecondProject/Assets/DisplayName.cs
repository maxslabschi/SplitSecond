using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Import TextMeshPro namespace

public class DisplayName : MonoBehaviour
{
    public TextMeshProUGUI obj_text; // Change Text to TextMeshProUGUI
    public TMP_InputField display; // Change InputField to TMP_InputField

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        obj_text.text = PlayerPrefs.GetString("user_name");
    }

    public void Create() {
        obj_text.text = display.text;
        PlayerPrefs.SetString("user_name", obj_text.text);
        PlayerPrefs.Save();
    }
}
