using System;
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

    private Text endTimer;
    private Text endAirTimer;

    [SerializeField]
    private Vector3 resetPos;
    private Vector3 startResetPos;

    private void Start()
    {
        gameManager = GameManager.Instance;
        player = gameManager.player;
        timer = gameManager.timer;
        airTimer = gameManager.airTimer;
        endTimer = gameManager.endTimer;
        endAirTimer = gameManager.endAirTimer;
        player.GetComponent<CharacterControllerRBody>().startable = true;
        player.GetComponent<GrapplePhysics>().gUpdate = true;
        gameManager.resetPos = resetPos;
        startResetPos = resetPos;
        CrossScript.checkReached = false;
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

                    player.GetComponent<Rigidbody>().velocity = Vector3.zero;

                    if (!CrossScript.checkReached)
                    {
                        player.GetComponent<GrapplePhysics>().gCount = 0;
                        player.GetComponent<GrapplePhysics>().UpdateGrappleCount();

                        timer.GetComponent<Timer>().stopped = true;
                        airTimer.GetComponent<Timer>().stopped = true;

                        timer.GetComponent<Timer>().ResetTimer();
                        airTimer.GetComponent<Timer>().ResetTimer();
                        player.GetComponent<CharacterControllerRBody>().startable = true;
                        player.GetComponent<GrapplePhysics>().gUpdate = true;
                    }
                    break;
                case eventHandles.Checkpoint:
                    resetPos = this.transform.position;
                    gameManager.resetPos = resetPos;
                    pauseEvent = true;
                    CrossScript.checkReached = true;
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
                    endTimer.text = timer.GetComponent<Timer>().timerText.text;
                    endAirTimer.text = airTimer.GetComponent<Timer>().timerText.text;
                    timer.GetComponent<Canvas>().enabled = false;
                    airTimer.GetComponent<Canvas>().enabled = false;
                    Time.timeScale = 0;
                    CrossScript.pauseGrapple = true;
                    AudioListener.pause = true;
                    Cursor.lockState = CursorLockMode.None;
                    CrossScript.gameOver = true;
                    CrossScript.EndScene.SetActive(true);
                    break;
            }
        }
    }
}
