using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq.Expressions;
using System.Linq;

public class FactionTracker{

	private static FactionTracker instance;
	
	private FactionTracker() 
	{
		Start();		
	}
	
	public static FactionTracker Instance 
	{
		get
		{
			if (instance == null)
			{
				Debug.Log("Creating new Faction Tracker");
				instance = new FactionTracker();
				instance.Start();
			}
			return instance;
		}
	}	
	
	public struct Factions {
		public string FactionName;
		public string[] otherFactionsName;
		public int[] otherFactionsRelation;
		public int maxShipsInFleet;
		
	}
	
	List<Factions> relationsList = new List<Factions>();
	
	void Start () {
		Debug.Log("Initializing Factions");
		InitFactions();
	}
	
	void InitFactions()
	{
		int count = 0;
		string filepath = Application.dataPath + "/Resources/Data Files/factiondata.xml";
		XDocument factionXML = XDocument.Load(filepath);
		
		var factionNames = from factionName in factionXML.Root.Elements("FactionAttributes")
			select new {
				factionName_XML = (string)factionName.Element("name"),
				factionID_XML = (int)factionName.Element("id"),
				factionRelations_XML = factionName.Element("relations")// Need to turn this into array.
		};
		
		foreach ( var factionName in factionNames)
			++count;

		foreach ( var factionName in factionNames)
		{
			Factions f = new Factions();			
			f.otherFactionsName = new string[count];
			f.otherFactionsRelation = new int[count];
			int others = 0;
			
			f.FactionName = factionName.factionName_XML;

			Debug.Log(factionName.factionRelations_XML);
			
			// Adds Rivals, not self to other list.
			foreach (var factionName2 in factionNames)
			{
				if (factionName.factionID_XML == factionName2.factionID_XML)
					continue;
				f.otherFactionsName[(int)factionName2.factionID_XML] = factionName2.factionName_XML;
				//f.otherFactionsRelation[(int)factionName2.factionID_XML] = factionName.factionRelations_XML[(int)factionName2.factionID_XML];
				Debug.Log(f.FactionName + " adds: " + factionName2.factionName_XML);
				++others;
			}
		}
	}
	
	void Update () {
	
	}
	
	// Gets faction by name.
	public Factions GetFaction(string inFaction)
	{
		foreach (Factions fac in relationsList)
			if (fac.FactionName.Equals(inFaction)) return fac;
		
		// return a blank faction if none can be found.
		return new Factions();
	}
	
	public int GetFactionCount()
	{
		Debug.Log("FactionCount" + relationsList.Count);
		return relationsList.Count;
	}
	
	public string[] GetFactionNames()
	{
		string[] factionNamesList = new string[relationsList.Count];
		int index = 0;
		foreach (Factions fac in relationsList)
		{
			factionNamesList[index] = fac.FactionName;
			++index;
		}
		return factionNamesList;
	}
}
