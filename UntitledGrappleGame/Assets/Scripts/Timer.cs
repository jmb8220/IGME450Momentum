using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    private float timer;

    public bool paused;

    public bool stopped;

    [SerializeField]
    private Text timerText;

    [SerializeField]
    private string marker;

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        if (!stopped)
        {
            if (!paused)
            {
                timer += Time.deltaTime;
            }
        }
        UpdateTimerDisplay();
    }

    //resets timer to 0
    public void ResetTimer()
    {
        timer = 0;
    }
    
    //updates text object with formatted information
    private void UpdateTimerDisplay()
    {
        //calculates and rounds minutes and seconds
        float min = Mathf.FloorToInt(timer / 60);
        float sec = Mathf.Round((timer % 60)*1000.0f)/1000.0f;

        string currentTime = string.Format("{00:00}:{1:00.00}", min, sec); ;

        //create string with 00:00.00 format and update text
        if (marker.Length > 0)
        {
            currentTime = marker + ": " + currentTime;
        }
        timerText.text = currentTime;
    }
}
