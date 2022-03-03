using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathZone : MonoBehaviour
{
    [SerializeField]
    private Vector3 resetPos;

    [SerializeField]
    private Canvas timer;
    [SerializeField]
    private Canvas airTimer;

    private void OnCollisionEnter(Collision player)
    {
        Debug.Log("Player Colliding");
        player.transform.position = resetPos;

        player.gameObject.GetComponent<GrapplePhysics>().gCount = 0;
        player.gameObject.GetComponent<GrapplePhysics>().UpdateGrappleCount();

        timer.GetComponent<Timer>().ResetTimer();
        airTimer.GetComponent<Timer>().ResetTimer();
    }
}
