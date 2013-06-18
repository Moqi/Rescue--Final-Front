// Author: Kenneth Rea
// Project: Rescue: Final Front
// All current code is not-final.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.IO;

//[System.Xml.Serialization.XmlRootAttribute("ShipAttributes", Namespace = "List<ShipAttributes>", IsNullable = false)]
public class ShipAttributes
{
	#region Attributes
	// Model Information
	public string name;
	public int id;
	public string modelName;
	
	
	public string firingPosition;
	public string cameraPosition;
	public string cargoPosition;
	public Vector3 cameraDistance;
	public Vector3 initialScale;
	public Vector3 initialRotation;
	
	// (Radius, Height, Direction)
	public Vector3 collisionCapsuleDimensions;
	public Vector3 collisionCapsuleCenter;

	
	// Rotation
	public float yawSpeed;
	public float pitchSpeed;
	public float rollSpeed;
	
	// Speed
	public float maxSpeed;
	public float cruiseSpeed;
	public float drag;
	public float angularDrag;
	public float maxAcceleration;
	public float maxDeceleration;
	
	// Physics Properties
	public float mass;
	
	// Collection Capacity
	public float cargoSpace;
	
	// Combat [Defense]
	public float structureHealth;
	public float armorHealth;
	public float shieldHealth;
	public float shieldRadius;
	
	// Combat [Assault]
	public float missileRechargeTime;
	public float fireRate;	
	
	// Currency Related
	public float cost;
	public float repairMultiplier;
	public float fuel;
	#endregion
	
	// Deserialize the ShipList XML so ships can be loaded into memory and selected / loaded easily. 
	void PopulateShipList()
	{
		// Ship list is already populated at this point.
		if (Ships.shipList.Count >= 1)
			return;
		
		//SerializeTest();
		string filepath = Application.dataPath + "/Resources/Data Files/shipsdata.xml";		
		
		// Create new serializer with type of List <ShipAttributes>
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ShipAttributes>));
		// Open a filestream to stream in the text.
		Stream streamReader = new FileStream(filepath, FileMode.Open);
		// Let the deserializer do its work. If the XML was serialized properly, no errors should occur.
		Ships.shipList = (List<ShipAttributes>)xmlSerializer.Deserialize(streamReader);
		

	}
	
	// Load a ship based on name.
	public ShipAttributes LoadShip(string inName)
	{
		PopulateShipList();
		
		//Determing if the matching ship has been selected, if not return null. Handle that elsewhere.		
		foreach (ShipAttributes att in Ships.shipList)
		{
			if (att.name == inName)
			{
				Debug.Log("Matching Names Found");
				Debug.Log("File: " + att.modelName);
				return att;
			}
		}		
		Debug.Log("whoops returned null");
		return null;
	}
	
	#region Debugging and Serialization Code	
	// Set information in the ShipAttributes so we can tell if Serialization was successful.
	public void QuickAdd()
	{
		name = "Default";
		id = 0;
		modelName = "Feisar_Ship_FBX/Feisar_Ship_Fix";		
		mass = 5000.0f;
	}
	
	// Quickly add a ship to be serialized
	void TestStart()
	{
		QuickAdd();
		Ships.shipList.Add(new ShipAttributes());
		Ships.shipList[0].QuickAdd();	
	}
	
	// Seralizes template for ShipAttributes so it can be easily modified later without confusion.
	public void SerializeList()
	{
		// Creates new XML document and Creates its Navigator
    	XmlDocument xmlDoc = new XmlDocument();
    	XPathNavigator nav = xmlDoc.CreateNavigator();
		
		// Sets XML filepath
		string filepath = Application.dataPath + "/Resources/DebugData/shipsdataDEBUG.xml";
		
		// Creates XmlWriter and sets it to the correct place
   		using (XmlWriter writer = nav.AppendChild())
    	{
			// Initializes serializer to the type of List with Ship Attributes
      	  XmlSerializer ser = new XmlSerializer(typeof(List<ShipAttributes>));
			// Serializes all ShipAttiribute classes in Ship List
		  ser.Serialize(writer, Ships.shipList);	    	  
    	}
		
		// Saves the Document. This file can now be deserialized easily.
		xmlDoc.Save(filepath);
	}
	
	// Debug to see if Serialization works, this allows us to modify the xml later with a template in hand.
	void SerializeTest()
	{
		TestStart();
		SerializeList();			
	}	
	#endregion
	
}
