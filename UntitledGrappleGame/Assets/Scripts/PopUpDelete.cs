using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpDelete : MonoBehaviour
{
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
