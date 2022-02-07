using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHUD : MonoBehaviour
{
    void Start()
    {

    }


    void Update()
    {

        if (Input.anyKey)
        {
            DeleteCurrentCanvas();
        }
    }

    public void DeleteCurrentCanvas()
    {
        Destroy(transform.gameObject.GetComponentInParent<Canvas>().gameObject);
    }
}
