using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpDelete : MonoBehaviour
{
    [SerializeField]
    private Canvas timer;
    [SerializeField]
    private Canvas airTimer;
    [SerializeField]
    private GameObject startTimer;

    void Update()
    {
        if (Input.anyKey)
        {
            timer.GetComponent<Timer>().stopped = false;
            airTimer.GetComponent<Timer>().stopped = false;
            startTimer.GetComponent<EventZone>().pauseEvent = false;
            DeleteCurrentCanvas();
        }
    }

    public void DeleteCurrentCanvas()
    {
        Destroy(transform.gameObject.GetComponentInParent<Canvas>().gameObject);
    }
}
