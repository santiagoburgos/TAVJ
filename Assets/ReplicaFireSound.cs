using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplicaFireSound : MonoBehaviour
{
    public float fireRate = 0.2f;	
    public bool shoot = false;
    private float nextFire; 
    private AudioSource gunAudio;	
    
    void Start()
    {
        gunAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if ( (Time.time > nextFire) && shoot)
        {
            gunAudio.Play();
            nextFire = Time.time + fireRate;
        }
    }
}
