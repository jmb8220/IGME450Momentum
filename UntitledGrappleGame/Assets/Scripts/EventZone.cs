using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventZone : MonoBehaviour
{
    private enum eventHandles{
        Reset,
        TimerStop,
        TimerStart,
        EndLevel
    };

    [SerializeField]
    private eventHandles eventHandled;
    [SerializeField]
    public bool pauseEvent;

    [SerializeField]
    private Vector3 resetPos;

    [SerializeField]
    private Canvas timer;
    [SerializeField]
    private Canvas airTimer;
    [SerializeField]
    private Camera cam;

    void Update()
    {
        if (Input.anyKey)
        {
            timer.GetComponent<Timer>().stopped = false;
            airTimer.GetComponent<Timer>().stopped = false;
        }
    }

    private void OnCollisionEnter(Collision player)
    {
        Debug.Log("Player Colliding");

        if (!pauseEvent)
        {
            //Reset Player
            if (eventHandled == eventHandles.Reset)
            {
                player.transform.position = resetPos;

                player.gameObject.GetComponent<GrapplePhysics>().gCount = 0;
                player.gameObject.GetComponent<GrapplePhysics>().UpdateGrappleCount();
                player.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;

                timer.GetComponent<Timer>().stopped = true;
                airTimer.GetComponent<Timer>().stopped = true;

                timer.GetComponent<Timer>().ResetTimer();
                airTimer.GetComponent<Timer>().ResetTimer();
            }
            //Stop Timers
            else if (eventHandled == eventHandles.TimerStop)
            {
                timer.GetComponent<Timer>().stopped = true;
                airTimer.GetComponent<Timer>().stopped = true;
            }
            //Start Timers
            else if (eventHandled == eventHandles.TimerStart)
            {
                timer.GetComponent<Timer>().stopped = false;
                airTimer.GetComponent<Timer>().stopped = false;
            }
            //End Level (To be implemented later)
            else if (eventHandled == eventHandles.EndLevel)
            {
                //Implement Level End Screen First
            }
        }
    }
}
