using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blink : MonoBehaviour {

    public float distance = 5.0f;
	

	void Update ()
    {
		if (Input.GetKeyDown(KeyCode.Q))
        {
            BlinkForward();
        }
	}
    public void BlinkForward()
    {
        RaycastHit hit;
        Vector3 destination = transform.position + transform.forward * distance;

        //intersecting obstacles
        if(Physics.Linecast(transform.position, destination, out hit))
        {
            destination = transform.position + transform.forward * (hit.distance - 1f);
        }

        //no obstacles found
        if(Physics.Raycast(destination, -Vector3.up, out hit))
        {
            destination = hit.point;
            destination.y = 0.5f;
            transform.position = destination;
        }
    }
}
