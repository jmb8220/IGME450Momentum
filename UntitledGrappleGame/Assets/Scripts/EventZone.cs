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

    private GameManager gameManager;
    private GameObject player;
    [SerializeField]
    private eventHandles eventHandled;
    [SerializeField]
    public bool pauseEvent;

    [SerializeField]
    private Vector3 resetPos;

    private Canvas timer;
    private Canvas airTimer;
    private Camera cam;

    private void Start()
    {
        gameManager = GameManager.Instance;
        player = gameManager.player;
        timer = gameManager.timer;
        airTimer = gameManager.airTimer;
        player.GetComponent<CharacterControllerRBody>().startable = true;
        player.GetComponent<GrapplePhysics>().gUpdate = true;
    }
    void Update()
    {
        if (player.GetComponent<CharacterControllerRBody>().startable)
        {
            if (Input.anyKey)
            {
                timer.GetComponent<Timer>().stopped = false;
                airTimer.GetComponent<Timer>().stopped = false;
                player.GetComponent<GrapplePhysics>().gUpdate = true;
            }
        }
        else
        { 
            timer.GetComponent<Timer>().stopped = true;
            airTimer.GetComponent<Timer>().stopped = true;
            player.GetComponent<GrapplePhysics>().gUpdate = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Player Triggered");
        if (!pauseEvent)
        {
            //Reset Player
            if (eventHandled == eventHandles.Reset)
            {
                player.transform.position = resetPos;

                player.GetComponent<GrapplePhysics>().gCount = 0;
                player.GetComponent<GrapplePhysics>().UpdateGrappleCount();
                player.GetComponent<Rigidbody>().velocity = Vector3.zero;

                timer.GetComponent<Timer>().stopped = true;
                airTimer.GetComponent<Timer>().stopped = true;

                timer.GetComponent<Timer>().ResetTimer();
                airTimer.GetComponent<Timer>().ResetTimer();
                player.GetComponent<CharacterControllerRBody>().startable = true;
                player.GetComponent<GrapplePhysics>().gUpdate = true;
            }
            //Stop Timers
            else if (eventHandled == eventHandles.TimerStop)
            {
                timer.GetComponent<Timer>().stopped = true;
                airTimer.GetComponent<Timer>().stopped = true;
                player.GetComponent<CharacterControllerRBody>().startable = false;
                player.GetComponent<GrapplePhysics>().gUpdate = false;
            }
            //Start Timers
            else if (eventHandled == eventHandles.TimerStart)
            {
                timer.GetComponent<Timer>().stopped = false;
                airTimer.GetComponent<Timer>().stopped = false;
                player.GetComponent<CharacterControllerRBody>().startable = true;
                player.GetComponent<GrapplePhysics>().gUpdate = true;
            }
            //End Level (To be implemented later)
            else if (eventHandled == eventHandles.EndLevel)
            {
                //Implement Level End Screen First
                timer.GetComponent<Timer>().stopped = true;
                airTimer.GetComponent<Timer>().stopped = true;
                player.GetComponent<CharacterControllerRBody>().startable = false;
                player.GetComponent<GrapplePhysics>().gUpdate = false;
            }
        }
    }
}
