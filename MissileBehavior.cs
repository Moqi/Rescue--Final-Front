// Author: Kenneth Rea
// Project: Rescue: Final Front
// All current code is not-final.

using UnityEngine;
using System.Collections;

public abstract class MissileBehavior : MonoBehaviour {

public	float 	damage;
public	float	timing;
public	float 	armingTime;
public	float	speed;
public	float 	turnspeed;
public	bool	armed;

	
public	Transform 		target;
public	MissileType		mType;
public	CurrentStage 	mStage;
	
	public enum MissileType {
		NULL = 0,
		Tracer,			// Trace your warhead.
		Kinetic,			// Great for small targets, light fast and high capacity.
		ShapedExplosive,	// Very good at piercing armor.
		EMP ,				// Disables enemy ships / systems.
		Nuclear,			// Both EMP and Nuclear in capabilities.
		Antimatter,			// Extremely destructive warhead
		Graviton, 			// Will suck targets towards them
		Blackhole			// Ultimate Warhead, massive in size, only the largest ships can use. Will suck up suns. Will sphagetifi things at event horizon.
	};
	
	public enum CurrentStage {
		Chasing = 0,
		Detonating
	};
	
	// Use this for initialization
	void Start () {
		Debug.Log("Starting");
		gameObject.AddComponent("TrailRenderer");
		TrailRenderer tr = gameObject.GetComponent("TrailRenderer") as TrailRenderer;
		tr.time = 3600;
		tr.startWidth = 50;
		target = TargetingSystem.tInfo.target;
		speed = 3000.0f;
		turnspeed = 1.0f;
		gameObject.AddComponent("SphereCollider");
		rigidbody.drag = 0.8f;
		rigidbody.angularDrag = 800000.0f;
	}
	
	// Maybe add a no collide for 2 seconds to emulate 
	// Update is called once per frame
	public virtual void Update () {
		Debug.Log("Missile Target: " + target.gameObject.ToString());
		if (!armed)
			Arming();
		
		if (mStage == CurrentStage.Chasing)
		{
			Follow();
			
		}
		
		if (mStage == CurrentStage.Detonating)
		{
			// Do detonaty things here.	
		}
	}
	
	public virtual void Follow() 
	{
		//transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.position - transform.position), turnspeed); 
		// Add * time.delta to slow down.
		//transform.position += transform.forward * speed * Time.deltaTime;	
	}
	
	public void SetInfo(Transform inTarget, MissileType inType)
	{
		target = inTarget;
		mType = inType;
		
		// Based on type get information on speed turning and damage.
	}
	
	public void SetTarget(Transform inTarget)
	{
		target = inTarget;	
	}
	
	public virtual void MissileUpdate()
	{
		
	}
	
	public void Arming()
	{
		timing += Time.deltaTime;		
		if (timing > armingTime)
			armed = true;
	}
	// Use command pattern for explosion settings
}
