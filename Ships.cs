// Author: Kenneth Rea
// Project: Rescue: Final Front
// All current code is not-final.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This script is going to be used to load in different attributes for each ship that can be used.
/// It will return a class that holds the attributes of the ship needed.
/// </summary>
public class Ships {
	
	public static List<ShipAttributes> shipList = new List<ShipAttributes>();
	public static List<AiShipAttributes> aiShipList = new List<AiShipAttributes>();
	
	// Use this for initialization
	void Start () {
		// On start, Check for saved game XML
		// If no, New Game
		// If yes, load saved ship from XML
		// Win?
		
		//LoadShip_Default();
	}
	
	// Update is called once per frame
	void Update () {
	
	}	
	
	// Loads ship based upon string name.
	public static ShipAttributes LoadShip_Custom(string inName)
	{
		Debug.Log("Entered LoadingShip_Custom");
		ShipAttributes loadingShip = new ShipAttributes();
		string name = inName;
		Debug.Log("Attempting to load ship " + name.ToString());
		loadingShip.LoadShip(name);	
		return loadingShip;
	}	
	
	// Loads the default ship if it cannot find the proper named one.
	public static ShipAttributes LoadShip_Default()
	{
		Debug.Log("Entered LoadingShip_Default");
		ShipAttributes loadingShip = new ShipAttributes();
		string name = "Default";
		Debug.Log("Attempting to load ship Default");
		loadingShip = loadingShip.LoadShip(name);	
		Debug.Log("File: " + loadingShip.modelName);
		return loadingShip;
	}
	
	public static AiShipAttributes LoadAiShip(string inName)
	{
		Debug.Log("Testing Ai Load!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
		AiShipAttributes loadingShip = new AiShipAttributes();
		loadingShip.SerializeTest();
		return loadingShip;
	}
}
