using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Player states
[SerializeField] enum PlayerState
{
    Grounded,
    Grappling,
    Midair,
    Sliding,
    Clambering
}

public class CharacterMovement: MonoBehaviour
{

    //physics parameters
    [SerializeField] public float walkSpeed = 6f;
    [SerializeField] public float sprintSpeed = 7.5f;
    [SerializeField] public float gravity = -9.81f;
    [SerializeField] public float jumpSpeed = 15f;

    float xMoveInput, zMoveInput;

    float terminalVelocity = -55.55f;

    public CharacterController controller;

    //master movement vector
    public Vector3 velocity = Vector3.zero;
    public Vector3 xzMovement = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        xMoveInput = Input.GetAxis("Horizontal");
        zMoveInput = Input.GetAxis("Vertical");

        //This will need to be modified conditional to the player's state of grappling, flying, etc
        xzMovement = transform.right * xMoveInput + transform.forward * zMoveInput;

        //gravity is always a thing that happens
        velocity.y += gravity * Time.deltaTime;
        velocity.y = Mathf.Clamp(velocity.y, terminalVelocity, 1000f);

        controller.Move(velocity * Time.deltaTime);
    }
}
