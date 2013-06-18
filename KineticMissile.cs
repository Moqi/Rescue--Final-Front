// Author: Kenneth Rea
// Project: Rescue: Final Front
// All current code is not-final.

using UnityEngine;
using System.Collections;

public class KineticMissile : MissileBehavior {
	
	Transform prevTransform;
	
	public override void Update()
	{		
		
		//Debug.Log("Missile Target: " + target.gameObject.ToString());
		if (!armed)
			Arming();		
				
		if (!target)
			Destroy(gameObject);
		
		if (mStage == CurrentStage.Detonating)
		{
			// Do detonaty things here.	
		}
	}
	
	void LateUpdate()
	{
		if (mStage == CurrentStage.Chasing)
		{
			Follow();			
		}		
	}
	
	void Follow()
	{		
		
		if (!target)
			return;
		
		if (!prevTransform)
			prevTransform = target.transform;
		
		Vector3 diff = target.position - prevTransform.position;
		Vector3 predictedPosition = target.position + (diff * 2000);
		// Need to do slight predictive algorithm. SLEEPTIME
		turnspeed = 2.0f;
		// Doesn't Quite do what I expected.
		transform.rotation = Quaternion.LookRotation(predictedPosition - transform.position);
			
			//Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.position - transform.position), turnspeed); 
		// Add * time.delta to slow down.
		rigidbody.AddForce(transform.forward * rigidbody.mass * 10000.0f);
		

	}
	
	void MissileUpdate()
	{		
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
	
	void OnCollisionEnter(Collision collision)
	{
		// object needs to have rigidbody using force to make this work.
		damage = collision.impactForceSum.magnitude;
		
		if (collision.collider.gameObject.transform == target)
			DoSomething(collision.collider.gameObject);
		else
			DoSomething(collision.collider.gameObject);
		
		Destroy(gameObject);
		// Get the sum of the impact. For kinetic this is important as it will detail the damage done.
		
	}
	
	void DoSomething(GameObject go)
	{
		AllObjectsScript targetScript = go.GetComponent("AllObjectsScript") as AllObjectsScript;
		
		if (!targetScript)
		{
			go.AddComponent("AllObjectsScript");
			targetScript = go.GetComponent("AllObjectsScript") as AllObjectsScript;
			if (targetScript.gameObject.rigidbody)
				targetScript.Health = targetScript.gameObject.rigidbody.mass;
		}
		
		targetScript.DamageObject(damage);
		Debug.Log("Damage: " + damage);
	}
}

