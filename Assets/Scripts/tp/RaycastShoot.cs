using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastShoot : MonoBehaviour
{
  

	public float fireRate = 0.12f;										
	public float weaponRange = 50f;										
	public Transform gunEnd;											

	private Camera fpsCam;											
	//private WaitForSeconds shotDuration = new WaitForSeconds(0.07f);	
	private AudioSource gunAudio;										
	private LineRenderer laserLine;										
	private float nextFire; 


	public bool hitted = false;
	public bool shooting = false;
	public int id = 0;
	public ParticleSystem muzzleFlash;
	public GameObject impactStoneEffect;
	public GameObject impactMetalEffect;


	public int maxBullets = 30;
	[NonSerialized]
	public int bullets;
	void Start ()
	{
		bullets = maxBullets;
		
		
		gunAudio = GetComponent<AudioSource>();

		fpsCam = GetComponentInParent<Camera>();
	}
	

	void Update () 
	{
		// Check if the player has pressed the fire button and if enough time has elapsed since they last fired
		if (Input.GetButton("Fire1") && Time.time > nextFire && bullets > 0)
		{
			gunAudio.Play();
			bullets -= 1;
			shooting = true;
			muzzleFlash.Play();
			nextFire = Time.time + fireRate;

		

            Vector3 rayOrigin = fpsCam.ViewportToWorldPoint (new Vector3(0.5f, 0.5f, 0.0f));
            RaycastHit hit;

		
			if (Physics.Raycast (rayOrigin, fpsCam.transform.forward, out hit, weaponRange))
			{
				
				if (hit.collider.tag.Equals("Shootable"))
				{
					
					id = hit.collider.GetComponent<ReplicaCollider>().id;
					hitted = true;
					
					GameObject impmet = Instantiate(impactMetalEffect, hit.point, Quaternion.LookRotation(hit.normal));
					Destroy(impmet, 2f);
				
				}
				else
				{
					GameObject impst = Instantiate(impactStoneEffect, hit.point, Quaternion.LookRotation(hit.normal));
					Destroy(impst, 2f);
				}
				

			
			}
		}
		
		if(!Input.GetButton("Fire1") || bullets <= 0)
		{
			shooting = false;
		}
	}
	
}