using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    private float timer;

    [SerializeField]
    private Text minutes;
    [SerializeField]
    private Text seconds;
    
    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        timer += Time.deltaTime;
        UpdateTimerDisplay();
    }

    private void ResetTimer()
    {
        timer = 0;
    }
    
    private void UpdateTimerDisplay()
    {
        float min = Mathf.FloorToInt(timer / 60);
        float sec = Mathf.Round((timer % 60)*1000.0f)/1000.0f;

        string currentTime = string.Format("{00:00}:{1:00.00}", min, sec);
        minutes.text = currentTime;
    }
}
