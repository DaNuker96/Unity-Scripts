using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour

{
    public GameObject bulletPrefab;
    public GameObject bulletSpawnEmpty;
    public float fireRate_;

    private Transform tBullet;

    void Start()
    {
        fireRate_ = gameObject.GetComponent<machineGun>().fireRate;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            BulletSpawn();
    }

    public void BulletSpawn()
    {
        tBullet = Instantiate(bulletPrefab.transform, bulletSpawnEmpty.transform.position, Quaternion.identity);
        tBullet.rotation = bulletSpawnEmpty.transform.rotation;
    }

}
