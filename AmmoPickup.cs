using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickup : MonoBehaviour {

    public float speed;
    bool enter = true;
    public machineGun script;


	void Update ()
    {
        this.gameObject.transform.Rotate(Vector3.up * Time.deltaTime * speed); //rotate
	}

    private void OnTriggerEnter(Collider other)
    {
        if (enter)
        {
            machineGun.ammoClips++; //add one to ammo clips
            Debug.Log("Ammo picked up");
            Destroy(this.gameObject);
        }
    }

}
