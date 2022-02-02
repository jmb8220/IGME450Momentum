using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Player states
[SerializeField] public enum PlayerState
{
    Grounded,
    Walking,
    Sprinting,
    Midair,
    Sliding,
    Clambering
}

public class CharacterMovement: MonoBehaviour
{
    public PlayerState currentState;

    public bool grappling = false;

    //physics parameters
    [SerializeField] public float maxWalkSpeed = 7f;
    [SerializeField] public float maxSprintSpeed = 12f;
    [SerializeField] public float gravity = -30f;
    [SerializeField] public float jumpImpulse = 15f;

    public float speed = 0;

    //for FOV shifting
    [SerializeField] public Camera playerCam;
    public float fovTime = 0.01f;

    float xMoveInput, zMoveInput;

    float terminalVelocity = -55.55f;

    public CharacterController controller;

    //master movement vector
    public Vector3 velocity = Vector3.zero;
    public Vector3 xzMovement = Vector3.zero;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        Debug.Log("Current Speed: " + maxSprintSpeed);
        //update state
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            Debug.Log("Player is Grounded!");

            //check for walk and sprint
            if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) && Input.GetKey(KeyCode.LeftShift))
            {
                currentState = PlayerState.Sprinting;
                playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, 100f, fovTime);
                Debug.Log("Player is Sprinting!");
            }
            //check for arbitrary number as a minimum forward velocity to start sliding
            else if (xzMovement.z > .8f && Input.GetKey(KeyCode.LeftControl))
            {
                //need to also move the camera down but I want it to be smooth so it's not here quite yet
                currentState = PlayerState.Sliding;
                playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, 102f, fovTime);
                Debug.Log("Player is Sliding!");
            }
            else
            {
                currentState = PlayerState.Walking;
                playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, 95f, fovTime);
                Debug.Log("Player is Walking!");
            }
            

        }

        //get inputs
        xMoveInput = Input.GetAxis("Horizontal");
        zMoveInput = Input.GetAxis("Vertical");

        //movement according to state
        //TODO: Update this when we add grappling because the params will need to be different
        switch (currentState)
        {
            case PlayerState.Walking:
                speed = Mathf.Lerp(speed, maxWalkSpeed, 0.1f);
                xzMovement = transform.right * xMoveInput + transform.forward * zMoveInput;
                controller.Move(xzMovement * speed * Time.deltaTime);

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    velocity.y += jumpImpulse;
                    Debug.Log("Jumped!");
                }

                break;

            case PlayerState.Sprinting:
                speed = Mathf.Lerp(speed, maxSprintSpeed, 0.05f);
                xzMovement = transform.right * xMoveInput + transform.forward * zMoveInput;
                controller.Move(xzMovement * speed * Time.deltaTime);

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    velocity.y += jumpImpulse;
                    Debug.Log("Jumped!");
                }

                break;

            case PlayerState.Midair:
                //we allow air control, just to a lesser extent
                xzMovement = transform.right * xMoveInput + transform.forward * zMoveInput;
                break;

            case PlayerState.Sliding:
                break;

            case PlayerState.Clambering:
                break;

        }

        //This will need to be modified conditional to the player's state of grappling, flying, etc
        

        

        //gravity is always a thing that happens
        velocity.y -= gravity * Time.deltaTime;
        velocity.y = Mathf.Clamp(velocity.y, terminalVelocity, 1000f);

        controller.Move(velocity * Time.deltaTime);

        //Debug.Log("Current Gravity Vector: " + velocity.y);
    }
}
