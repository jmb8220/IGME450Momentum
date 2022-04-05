using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventZone : MonoBehaviour
{
    private enum eventHandles{
        Reset,
        Checkpoint,
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

    private Canvas timer;
    private Canvas airTimer;
    private Camera cam;

    [SerializeField]
    private Vector3 startResetPos;

    private void Start()
    {
        gameManager = GameManager.Instance;
        player = gameManager.player;
        timer = gameManager.timer;
        airTimer = gameManager.airTimer;
        player.GetComponent<CharacterControllerRBody>().startable = true;
        player.GetComponent<GrapplePhysics>().gUpdate = true;
        gameManager.resetPos = startResetPos;
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
            switch (eventHandled)
            {
                //Reset Player
                case eventHandles.Reset:
                    player.transform.position = gameManager.resetPos;

                    player.GetComponent<GrapplePhysics>().gCount = 0;
                    player.GetComponent<GrapplePhysics>().UpdateGrappleCount();
                    player.GetComponent<Rigidbody>().velocity = Vector3.zero;

                    timer.GetComponent<Timer>().stopped = true;
                    airTimer.GetComponent<Timer>().stopped = true;

                    timer.GetComponent<Timer>().ResetTimer();
                    airTimer.GetComponent<Timer>().ResetTimer();
                    player.GetComponent<CharacterControllerRBody>().startable = true;
                    player.GetComponent<GrapplePhysics>().gUpdate = true;
                    break;
                case eventHandles.Checkpoint:
                    gameManager.resetPos = this.transform.position;
                    break;
                case eventHandles.TimerStop:
                    timer.GetComponent<Timer>().stopped = true;
                    airTimer.GetComponent<Timer>().stopped = true;
                    player.GetComponent<CharacterControllerRBody>().startable = false;
                    player.GetComponent<GrapplePhysics>().gUpdate = false;
                    break;
                case eventHandles.TimerStart:
                    timer.GetComponent<Timer>().stopped = false;
                    airTimer.GetComponent<Timer>().stopped = false;
                    player.GetComponent<CharacterControllerRBody>().startable = true;
                    player.GetComponent<GrapplePhysics>().gUpdate = true;
                    break;
                case eventHandles.EndLevel:
                    //Implement Level End Screen First
                    timer.GetComponent<Timer>().stopped = true;
                    airTimer.GetComponent<Timer>().stopped = true;
                    player.GetComponent<CharacterControllerRBody>().startable = false;
                    player.GetComponent<GrapplePhysics>().gUpdate = false;
                    break;
            }
        }
    }
}
