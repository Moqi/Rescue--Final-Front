// Author: Kenneth Rea
// Project: Rescue: Final Front
// All current code is not-final.

using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public class AsteroidFields : MonoBehaviour {
	
	Transform target;
	
	public enum GameState {
		Playing = 0,
		Saving,
		Finished
	};
	
	public enum Grouping { 
		Sparse = 0, 
		Loose, 
		Moderate,
		Tight, 
		Single 
	};
	
	static public float[] GroupingDistance  = new float[5] { 
		1450, 
		1100, 
		850, 
		400,
		50
	};
	
	public int[,] GroupingNumbers = new int[5,5] {
		{2,3,4,5,6},
		{6,8,10,12,14},
		{5,7,9,10,13},
		{4,5,6,7,8},
		{1,1,1,1,1}
	};
	
	public struct AsteroidGroupInfo {
		
		public int amount;
		public Vector3 location;
		public Grouping groupType;
		
	}
	
	public GameState gState;
	int asteroidAmount = 0;
	
	public List<AsteroidGroupInfo> groupsInMemory = new List<AsteroidGroupInfo>();
	List<AsteroidBeacon> asteroidBeacons = new List<AsteroidBeacon>();
	
	// Use this for initialization
	void Start () 
	{
		gState = GameState.Playing;
		//target = GameObject.FindGameObjectWithTag("player").transform;
	
		// Check for load game
		
		//else Create Level
		//CreateLevel(300);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (!target)
			target = GameObject.FindGameObjectWithTag("player").transform;	
		if (gState == GameState.Playing)
			CheckDistances();
		if (gState == GameState.Saving)
			StoreAsteroids();
	}
	

	
	public Grouping RandomEnum<Grouping>()
	{ 
  		Grouping[] values = (Grouping[]) System.Enum.GetValues(typeof(Grouping));
  		return values[new System.Random().Next(0,values.Length)];
	}
	
	public Grouping SpecificEnum<Grouping>(int inEnum)
	{ 
  		Grouping[] values = (Grouping[]) System.Enum.GetValues(typeof(Grouping));
  		return values[inEnum];
	}
	
	public void CreateLevel(int inAmount) 
	{	 
		asteroidAmount = inAmount;
		// Need to better randomly generate positions
		//System.Security.Cryptography.RNGCryptoServiceProvider rnd = new System.Security.Cryptography.RNGCryptoServiceProvider();
		for (int i = 0; i < inAmount; ++i)
		{
			AsteroidGroupInfo storingInfo = new AsteroidGroupInfo();			
			storingInfo.groupType = RandomEnum<Grouping>();
			storingInfo.amount = GroupingNumbers[(int)storingInfo.groupType, Random.Range(0,4)];
			storingInfo.location = new Vector3(Random.Range(-250000, 250000), Random.Range(-27000, 27000), Random.Range(-250000, 250000));
			groupsInMemory.Add(storingInfo);
			//Debug.Log(storingInfo.location);
		}
	}
	
	// Used for saving game.
	void StoreAsteroids () 
	{		
		foreach (AsteroidBeacon beac in asteroidBeacons)
		{
				AsteroidGroupInfo storingInfo = new AsteroidGroupInfo();
				storingInfo.amount = beac.asteroidsAtBeacon;
				storingInfo.groupType = beac.groupType;
				storingInfo.location = beac.transform.position;
				groupsInMemory.Add(storingInfo);
				beac.DespawnAsteroids();
		}
		
		gState = GameState.Finished;
		Debug.Log(groupsInMemory.Count);
	}
	
	void CheckBeacons()
	{
		foreach (AsteroidBeacon beac in asteroidBeacons)
		{
			if (beac.asteroidsAtBeacon == 0)
				asteroidBeacons.Remove(beac);
		}		
	}
	
	// This is for creating beacons
	void CheckDistances() 
	{
		float distance = 999999999999999;
		List<AsteroidGroupInfo> deleteList = new List<AsteroidGroupInfo>();
		
		foreach (AsteroidGroupInfo agi in groupsInMemory)
		{
			distance = Vector3.Distance(target.transform.position, agi.location);
			if (distance < 65000)
			{
				CreateBeacon(agi);
				deleteList.Add(agi);
			}
		}
		
		foreach (AsteroidGroupInfo agi in deleteList)
		{
			groupsInMemory.Remove(agi);	
		}
	}
	
	void CreateBeacon(AsteroidGroupInfo agi)
	{
		GameObject beacon = Instantiate(Resources.Load("Asteroids/asteroidBeacon"), agi.location, Quaternion.identity) as GameObject;
		AsteroidBeacon temp = beacon.GetComponent("AsteroidBeacon") as AsteroidBeacon;
		temp.SetSelf(beacon.gameObject.transform);
		temp.SetInfo(agi.groupType, GroupingDistance[(int)agi.groupType], agi.amount, this);
		temp.RespawnAsteroids();
		asteroidBeacons.Add(temp);
	}
	
	public void StoreBeacon(AsteroidBeacon bcn)
	{
		AsteroidGroupInfo storingInfo = new AsteroidGroupInfo();
		storingInfo.amount = bcn.asteroidsAtBeacon;
		storingInfo.groupType = bcn.groupType;
		storingInfo.location = bcn.transform.position;
		groupsInMemory.Add(storingInfo);
		asteroidBeacons.Remove(bcn);
		Destroy(bcn.gameObject);
	}
	
	public void SetSaving()
	{
		gState = GameState.Saving;	
	}
	
	public void SetPlaying()
	{
		gState = GameState.Playing;	
	}
}
