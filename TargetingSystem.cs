// Author: Kenneth Rea
// Project: Rescue: Final Front
// All current code is not-final.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Temporary for lockon
using UnityEditor;

public class TargetingSystem : MonoBehaviour {
	
public 	static 	TargetInfo	tInfo;
public 	ship_control.ControlStates cState;
	
	int		projectilesFired;
	int 	currentTargetID;
	int		currentMissileID;
	int		targetsAquired;
	int 	targetsMax;
	int 	loops;	
	bool 	targetingMode;
	float 	lockOnTime;
	float 	lockOnRange;
	float 	targetSwitchTime;
	
	BoxCollider targetFinder;
	MissileBehavior.MissileType missileTypeSelected;	

	
	List<Transform> targetList = new List<Transform>();
	List<Transform> targetListPotential = new List<Transform>();
	List<string> 	projectileList= new List<string>();
	
	// Use this for initialization
	TargetingMode 	tMode;
	
	
	public struct TargetInfo {
		public 	Transform	 target;
		
		public	float 	LockingSuccessTime;
		public 	bool 	offScreen;
		public 	bool	locked;
		public 	int 	targetID; 			// Will probably have to check targetID every once and a while to make sure it's right? maybe not because out of list stuff.
											// Will be sorted out at end of list anyways.
	};
	
	public enum TargetingMode {
		OFF = 0,
		STANDARD,
		STANDARDSWITCH,
		DONTLOSE,
		LOSING,
		LOCKED,
		SWITCH
	};
	
	void Start () {
		
		lockOnRange = 4000.0f;
		targetFinder = gameObject.AddComponent("BoxCollider") as BoxCollider;
		targetFinder.size = new Vector3(1200,1200,lockOnRange);
		targetFinder.center = new Vector3(0,0,lockOnRange/2);
		targetFinder.isTrigger = true;
		targetingMode = true;
		tInfo = new TargetInfo();
		lockOnTime = 3;
		targetSwitchTime = 1;
		currentTargetID = 0;
		
	}
	
	// Update is called once per frame
	void Update () {
		
		if (cState != ship_control.ControlStates.Regular)
		{
			if (tMode != TargetingMode.OFF)
				SwitchOffMode();				
		}
		
		if (cState == ship_control.ControlStates.Regular && tMode == TargetingMode.OFF)
			tMode = TargetingMode.STANDARD;
		
		++loops;
		targetSwitchTime += Time.deltaTime;
		
		if (tMode == TargetingMode.OFF)
			return;
		
		if (!targetingMode)
			return;
		
		if (Input.GetAxis("Mouse ScrollWheel") > 0)
		{
			++currentMissileID;
			missileTypeSelected = SelectMissile();
		}
		else if (Input.GetAxis("Mouse ScrollWheel") < 0)
		{
			--currentMissileID;
			missileTypeSelected = SelectMissile();
		}
		// Optimize checks
		if (loops > 20)
		{
			CheckTallTargets();
			loops = 0;
		}
		//LockOnParameters(); // Remove this once missiles are in. DEBUG PURPOSES
		
		if (missileTypeSelected == MissileBehavior.MissileType.NULL)
			tMode = TargetingMode.OFF;
		else if (tMode == TargetingMode.OFF)
			tMode = TargetingMode.STANDARD;
				// Change this to false when finished debugging. DEBUG PURPOSES
	
		if (tMode == TargetingMode.OFF && Input.GetMouseButtonDown(0))
		{
			FireProjectile();
		}
		
		if (tMode != TargetingMode.SWITCH && Input.GetKey(KeyCode.Tab) && targetSwitchTime > 1)
			tMode = TargetingMode.SWITCH;
		
		if (tMode == TargetingMode.STANDARD)
		{
			currentTargetID = 0;
			LockOnParameters();
		}
		
		if (tMode == TargetingMode.DONTLOSE)
		{
			AttemptLockOn();
			
		}
		if (tMode == TargetingMode.LOCKED)
		{
			UpdateLockTone();
			if (Input.GetMouseButtonDown(0))
				FireMissile();
			// if left click, fire missile yada yada.	
		}
		
		// Mode for switching targets.
		if (tMode == TargetingMode.SWITCH)
		{
			targetSwitchTime = 0;
			ResetTargetInfo();
			++currentTargetID;		
			if(currentTargetID > targetList.Count - 1)
			{
				//Debug.Log("ID OUT OF BOUNDS");
				currentTargetID = 0;
			}			
			LockOnParameters();
		}
		
		//if (Input.GetKey(KeyCode.K))
			//targetingMode = !targetingMode;
		// Tab is switch targeting mode.
	}
	
	bool CheckDontTarget(Transform tf)
	{
		string ct = tf.gameObject.tag;
		return false;
		// Check for all objects that don't hit. Or just do layers.
	}
	
	void OnTriggerEnter(Collider collider)
	{
		
		if (missileTypeSelected == null || collider.gameObject.layer == 13)
			return;
		
		if (!collider.gameObject.renderer)
			return; 
		
		if (!collider.gameObject.renderer.isVisible)
			targetListPotential.Add(collider.transform);;
		
		//if (collider.gameObject.renderer.isVisible) // This could keep the target out of this list. Bad.
		targetList.Add(collider.transform);
		
		Debug.Log("Adding Target: " + collider.gameObject.name);
	}
	
	void OnTriggerExit(Collider collider)
	{
		if (!collider.gameObject.renderer)
			return;
		
		targetList.Remove(collider.transform);
		
		if (collider.transform == tInfo.target)
			tInfo.offScreen = true;
		
		Debug.Log("Removing Target: " + collider.gameObject.name);
	}	
	
	void LockOnParameters()
	{		
		if (targetList.Count == 0)
			return;
		
		// Set tInfo target if possible
		//if (tInfo.target == null || tInfo.target != targetList[currentTargetID])
		tInfo.target = targetList[currentTargetID];		
		tInfo.targetID = currentTargetID;

		tMode = TargetingMode.DONTLOSE;

		// Set Range
	}
	
	void AttemptLockOn()
	{	
		tInfo.LockingSuccessTime += Time.deltaTime;
		if (tInfo.LockingSuccessTime > lockOnTime)
			LockedOnTarget();
	}
	
	void LosingTarget()
	{
		tInfo.LockingSuccessTime -= Time.deltaTime;
		if (tInfo.LockingSuccessTime < 0)
			LostTarget();
	}
	
	void LostTarget()
	{
		ResetTargetInfo();
		
		if (currentTargetID > 0)
			tMode = TargetingMode.SWITCH;
		else
			tMode = TargetingMode.STANDARD;
	}
	
	void ResetTargetInfo()
	{
		//Debug.Log("Reset Target Info");
		tInfo.locked = false;
		tInfo.LockingSuccessTime = 0;
		tInfo.offScreen = false;
		tInfo.target = null;
		tInfo.targetID = 0;		
	}
	
	void LockedOnTarget()
	{
		//Debug.Log("Target Locked");
		tMode = TargetingMode.LOCKED;
		tInfo.LockingSuccessTime = lockOnTime;
	}
	
	// Updates the locking information to make sure there is no false targets, etc.
	void UpdateLockTone()
	{
		if (!tInfo.target)
		{
			LostTarget();
			return;
		}
		
		// Check All Targets should dictate if offscreen so might not need the other.
		if ( tInfo.offScreen || !tInfo.target.gameObject.renderer.isVisible)
			tInfo.LockingSuccessTime -= Time.deltaTime;
		else
			tInfo.LockingSuccessTime = lockOnTime;
		
		if (tInfo.LockingSuccessTime < 0)
			LostTarget();
		
		
	}
	
	void SwitchOffMode()
	{
		ResetTargetInfo();
		tMode = TargetingMode.OFF;
	}
	
	// Checks if all targets are on screen. Puts them in potential targets otherwise. 
	void CheckTallTargets()
	{
		List<Transform> temp = new List<Transform>(targetList);
		
		foreach (Transform tf in targetList)
		{	
			if (!tf)
			{
				temp.Remove(tf);
				continue;
			}
			
			if (!tf.gameObject.renderer.isVisible)
			{
				if (tf == tInfo.target)
					tInfo.offScreen = true;
				temp.Remove(tf);
			}
		}
		targetList = new List<Transform>(temp);
		temp = new List<Transform>(targetListPotential);
		// Can safely do this by making temporary list but it doesn't seem to be causing any serious issues at this point.
		foreach (Transform tfp in targetListPotential)
		{
			if (!tfp)
				continue;
			if (tfp.gameObject.renderer.isVisible)
			{
				targetList.Add(tfp);
				temp.Remove(tfp);
			}				
		}
		targetListPotential = new List<Transform>(temp);
	}
	
	
	// Redo bullet creation. Need to do this without using strings.
	public void FireProjectile()
	{		
		string newname = projectilesFired.ToString();
		GameObject projectile = Instantiate(Resources.Load("Projectiles/projectile"), transform.position, transform.rotation) as GameObject;
		projectile.name = projectilesFired.ToString();
		bulletBehavior temp = (bulletBehavior) projectile.GetComponent("bulletBehavior");
		temp.SetAndFire(this.transform, projectile.transform);
		projectileList.Add("projectile" + newname.ToString());
		++projectilesFired;
	}
	
	public void FireMissile()
	{
		
		GameObject go = Instantiate(Resources.Load("Projectiles/TempMissile")) as GameObject;
		go.name = missileTypeSelected.ToString();
		go.AddComponent("Rigidbody");
		go.rigidbody.velocity = gameObject.rigidbody.velocity;
		go.transform.position = gameObject.transform.position;
		go.transform.rotation = gameObject.transform.rotation;
		go.transform.position += go.transform.forward * 10;		
		go.AddComponent(missileTypeSelected.ToString() + "Missile");
		go.layer = 14;
		
	}
	
	public MissileBehavior.MissileType SelectMissile()
	{ 		
		Debug.Log("Changing Missile");
  		MissileBehavior.MissileType[] values = (MissileBehavior.MissileType[]) System.Enum.GetValues(typeof(MissileBehavior.MissileType));
				
		if (currentMissileID > 4)
			currentMissileID = 0;		
		if (currentMissileID < 0)
			currentMissileID = 4;
		
		
		Debug.Log("CurMisID: " + currentMissileID);
		return values[currentMissileID];
	}
	
	void OnGUI()
	{
		GUI.Label(new Rect(0,(Screen.height - 20),520,Screen.height), "MISSILE SELECTED: " + missileTypeSelected.ToString());
		
		GUI.Label(new Rect(0,(Screen.height - 40),520,Screen.height - 20), targetList.Count.ToString());
		if(tInfo.target)
		{
			if (tMode == TargetingMode.LOCKED)
				GUI.Label(new Rect((Screen.width / 2 - 50), (Screen.height / 2),(Screen.width / 2 + 20),(Screen.height / 2 + 10)), "TARGET LOCKED: " + tInfo.target.gameObject.name);
			if (tMode == TargetingMode.DONTLOSE)
				GUI.Label(new Rect((Screen.width / 2 - 50), (Screen.height / 2),(Screen.width / 2 + 20),(Screen.height / 2 + 10)), "TARGETING: " + tInfo.target.gameObject.name + " ID: " + currentTargetID);
		}
	}
}
