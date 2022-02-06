using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITextTracker : MonoBehaviour
{

    public CharacterControllerRBody charMovement;

    public GameObject velocityTracker;
    public GameObject stateTracker;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //velocityTracker.GetComponent<Text>().text = "X: " + charMovement.velocity.x + "\nY: " + charMovement.velocity.y + "\nZ: " + charMovement.velocity.z;
        stateTracker.GetComponent<Text>().text = "State: " + charMovement.currentState;
    }
}
