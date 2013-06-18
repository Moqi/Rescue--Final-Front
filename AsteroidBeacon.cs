// Author: Kenneth Rea
// Project: Rescue: Final Front
// All current code is not-final.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AsteroidBeacon : MonoBehaviour {
	
	public	SphereCollider 	despawnCollider;
	public	bool			despawned;
	public 	int 			asteroidsAtBeacon;
	public	float			grouping;
			float 			distanceFromPlayer;
			float 			frames;
			Transform 		target;
			Transform		self;
			AsteroidFields	asteroidFieldClass;
	
	public 	AsteroidFields.Grouping	groupType;
	public	List<GameObject> asteroidsSpawnedList = new List<GameObject>();
	
			
	
	void Start()
	{
		if (!target)
			target = GameObject.FindGameObjectWithTag("player").transform;
		frames = 0;		
		despawned = false;
	}
	
	void Update()
	{
		++frames;
		if (frames > 1200)
			UpdateOffset();
		
	}
	
	void UpdateOffset()
	{
		CheckDistance();
		
		if (distanceFromPlayer > 65000)
			DespawnAsteroids();
		frames = 0;
	}
	
	void CheckDistance()
	{
		distanceFromPlayer = Vector3.Distance(transform.position, target.transform.position);
	}
	
	public void SetInfo(AsteroidFields.Grouping inGrouping, float groupDistance, int inAsteroidAmount, AsteroidFields afIn)
	{
		groupType = inGrouping;
		grouping = groupDistance;
		asteroidsAtBeacon = inAsteroidAmount;
		asteroidFieldClass = afIn;
	}
	
	public void SetSelf(Transform inT)
	{
		self = inT;	
	}
	public void RespawnAsteroids() 
	{
		//float maxDistance = (float)AsteroidFields.GroupingDistance[grouping];		
		Vector3 rndLocation = new Vector3(Random.Range(-grouping, grouping), Random.Range(-grouping, grouping), Random.Range(-grouping, grouping)) + transform.position;
		GameObject asteroid = Instantiate(Resources.Load("Asteroids/asteroid"), rndLocation, Quaternion.identity) as GameObject;
		asteroidsSpawnedList.Add(asteroid);
		
		for (int asteroidsSpawned = 1; asteroidsSpawned < asteroidsAtBeacon; ++asteroidsSpawned)
		{
			rndLocation = new Vector3(Random.Range(-grouping, grouping), Random.Range(-grouping, grouping), Random.Range(-grouping, grouping)) + transform.position;
			asteroid = Instantiate(asteroid, rndLocation, Quaternion.identity) as GameObject;
			asteroidsSpawnedList.Add(asteroid);
			// Put beacon in AsteroidFields list for Beacons.
		}
	}
	
	void AddDespawnCollider()
	{
		despawnCollider = self.gameObject.GetComponent("SphereCollider") as SphereCollider;
		despawnCollider.radius = grouping * 2;
		//despawnCollider.isTrigger = true;
	}
	
	void DestroyAsteroids()
	{
		foreach(GameObject ast in asteroidsSpawnedList)
		{
			if (ast)
				Destroy(ast);
		}
	}
	
	public void DespawnAsteroids()
	{
		//Vector3 prevPos = self.transform.position;
		//self.transform.position = new Vector3(9999999, 99999999, 999999999);
		//self.transform.position = prevPos;
		Debug.Log("Triggered Despawn");
		despawned = true;
		asteroidsAtBeacon = 0;
		DestroyAsteroids();
		asteroidFieldClass.StoreBeacon(this);
		//AddDespawnCollider();
	}
	
	/*
	void OnCollisionEnter(Collision other)
	{
		Debug.Log("In Enter Collider");
		if (other.gameObject.CompareTag("asteroid"))
			Destroy(other.gameObject);
	}
	
	void OnTriggerEnter(Collision other)
	{
		Debug.Log("In Enter Collider");
		++asteroidsAtBeacon;
		if (other.gameObject.CompareTag("asteroid"))
			Destroy(other.gameObject);
	}	
	
	void OnTriggerStay(Collision other)
	{
		Debug.Log("In Stay Collider");
		++asteroidsAtBeacon;
		if (other.gameObject.CompareTag("asteroid"))
			Destroy(other.gameObject);
	}	
	*/
}