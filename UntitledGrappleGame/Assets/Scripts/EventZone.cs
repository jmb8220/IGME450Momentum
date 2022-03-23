using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventZone : MonoBehaviour
{
    [SerializeField]
    private int eventHandled; // 0 = Reset | 1 = Timer Stop | 2 = Timer Start | 3 = End Level
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

    private void OnCollisionEnter(Collision player)
    {
        Debug.Log("Player Colliding");

        if (!pauseEvent)
        {
            //Reset Player
            if (eventHandled == 0)
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
            else if (eventHandled == 1)
            {
                timer.GetComponent<Timer>().stopped = true;
                airTimer.GetComponent<Timer>().stopped = true;
            }
            //Start Timers
            else if (eventHandled == 2)
            {
                timer.GetComponent<Timer>().stopped = false;
                airTimer.GetComponent<Timer>().stopped = false;
            }
            //End Level (To be implemented later)
            else if (eventHandled == 3)
            {
                //Implement Level End Screen First
            }
        }
    }
}
