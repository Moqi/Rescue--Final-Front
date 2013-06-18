// Author: Kenneth Rea
// Project: Rescue: Final Front
// All current code is not-final.

using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ship_control : MonoBehaviour {	
	
public 	static 	float 	currentTractorBeamMass = 0.0f;	

public 	Vector3 	previousPosition = new Vector3(0,0,0);
public 	Vector3 	differencePosition = new Vector3(0,0,0);
public 	float 		previousVelocity = 0;
public 	float 		acceleration = 0;
public 	float		disabledfor = 0;
public	float		disabledElapsed = 0;
public 	int 		player;
		Camera 		playerCam;
		GameObject 	shipModel;
		GameObject	selectedAsteroid;
		RaycastHit 	hit;
		Ray			ray;
			
		Transform 		lineTarget;
		LineRenderer 	lineRenderer;
		ParticleSystem	mineEmitter;
		ShipAttributes 	attributes;
		ControlStates 	cState;
		ControlStates	cRevert;
	
		AsteroidBehavior 	ab;
		MissileBehavior		missile;
		TargetingSystem		targetingSystem;
	
		public enum ControlStates {
			Regular = 0,
			Mining,
			Paused,
			Cameraing,
			Disabled,
			Debugging
		};	

	

	// Use this for initialization
	void Start () {
		rigidbody.useGravity = false;
		cState = ControlStates.Regular;	
		cRevert = ControlStates.Paused;
		// to load attributes, use Savegame
	}
	
	// Update is called once per frame
	void Update () {
		
		playerCam.transform.position += new Vector3(0,0,0.1f);
		SetPositionInit();
		
		if (Time.timeSinceLevelLoad > 0.2f && !Tutorials.movementTutorial)
			Tutorials.Tutorial_Movement();
		
		if (!Tutorials.collectTutorial)
		{
			Vector3 collectPosition = GameObject.FindGameObjectWithTag("collection").transform.position;
			if (Vector3.Distance(collectPosition, transform.position) < 1000)
				Tutorials.Tutorial_Collection();
		}
		
		acceleration = rigidbody.velocity.magnitude - previousVelocity;
		previousVelocity = rigidbody.velocity.magnitude;
		differencePosition = transform.position - previousPosition;
		//Debug.Log(differencePosition.ToString());
		previousPosition = transform.position;
		//Debug.Log("Acceleration: " + acceleration.ToString());
		
		rigidbody.drag = 0.2f;
		
		if (cState == ControlStates.Disabled)
		{
			disabledElapsed += Time.deltaTime;
			
			if (disabledElapsed > disabledfor)
			{
				disabledfor = 0;
				disabledElapsed = 0;
				cState = cRevert;
			}	
			
		}
		#region Regular Control State
		if (cState == ControlStates.Regular)
		{
			if (Input.GetButton("Forward"))
			{
				float tempSpeed = attributes.maxAcceleration;
				//Debug.Log("Max Accel: " + tempSpeed.ToString());
				//shipModel.transform.position += new Vector3(0,0,0.01f);
				
				if (Input.GetButton("Alternate"))
					tempSpeed *= 20;
				
				this.rigidbody.drag = 0.0f;
				if (rigidbody.velocity.magnitude < attributes.maxSpeed)
					this.gameObject.rigidbody.AddRelativeForce(Vector3.forward * tempSpeed);			
			}
			
			if (Input.GetButton("Backwards"))
			{
				this.gameObject.rigidbody.AddRelativeForce(Vector3.back * attributes.maxAcceleration);
			}
			
			if (Input.GetAxis("Vertical") != 0)
			{
				Vector3 currenthorizontal = Vector3.up*Input.GetAxis("Vertical")*attributes.yawSpeed;
				rigidbody.AddRelativeTorque(currenthorizontal);
				
			}
			
			if (Input.GetAxis("Horizontal") != 0)
			{
			Vector3 currentvertical = Vector3.left*Input.GetAxis("Horizontal")*attributes.pitchSpeed;
			rigidbody.AddRelativeTorque(currentvertical);
	
			rigidbody.AddRelativeForce(Vector3.Normalize(Vector3.Cross(currentvertical, rigidbody.velocity)));
			
			}
			
			if (Input.GetButton("RotateRight"))
			{
	   		Vector3 currentroll = Vector3.back*attributes.rollSpeed;
			rigidbody.AddRelativeTorque(currentroll);	
			}
			
			if (Input.GetButton("RotateLeft"))
			{
	   		Vector3 currentroll = Vector3.forward*attributes.rollSpeed;
			rigidbody.AddRelativeTorque(currentroll);	
			}		
			
			if (Input.GetKeyDown(KeyCode.Q))
			{
				//rigidbody.rotation = new Quaternion(0,0,0,0);	
			}
			
			if (Input.GetButton("AngularDrag"))
			{
				rigidbody.angularDrag = 5.0f;	
			}
			else
			{
				rigidbody.angularDrag = 1.0f;	
			}
			
			if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.LeftAlt))
			{
				cState = ControlStates.Mining;
				targetingSystem.cState = ControlStates.Mining;	
			}
			if (Input.GetKeyDown(KeyCode.C))
				cState = ControlStates.Cameraing;
			if (Input.GetKeyDown(KeyCode.P))
				cState = ControlStates.Paused;	
		}	
		// End of Regular Control State
		#endregion
		
		#region Camera Control
		if (cState == ControlStates.Cameraing)
		{
			if (!playerCam)
				playerCam = this.GetComponent("Camera") as Camera; 
			
			Camera planetCam = GameObject.Find("Main Camera").GetComponent("Camera") as Camera;
			
			if (Input.GetKeyDown(KeyCode.C))
			{				
				if (playerCam.camera.enabled == true)
				{
					playerCam.camera.enabled = false;
					planetCam.camera.enabled = true;
				}
				else
				{
					playerCam.camera.enabled = true; 
					planetCam.camera.enabled = false;
				}						
			}
			
			if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.LeftAlt))
			{
				cState = ControlStates.Regular;
				targetingSystem.cState = ControlStates.Regular;
				playerCam.camera.enabled = true; 
				planetCam.camera.enabled = false;
			}
		}
		// End of Camera Control State
		#endregion
		
		#region Mining Control
		if (cState == ControlStates.Mining)
		{
			if (!playerCam)
				playerCam = this.GetComponent("Camera") as Camera; 
			
			if (Input.GetMouseButtonDown(0))
			{
				//Debug.Log("Fired Ray");
				hit = new RaycastHit();
				ray = playerCam.ScreenPointToRay (Input.mousePosition);
				//Debug.Log("Mouse Position: " + Input.mousePosition);
				ray.origin += Vector3.forward * 20;
				
				if (Physics.Raycast(ray, out hit, 3000.0f))
				{
					//Debug.Log("Ray Hit Object: " + hit.collider.gameObject.tag);
					
					if (hit.collider.gameObject.CompareTag("asteroid"))
					{
						selectedAsteroid = hit.collider.gameObject;
						//Destroy(hit.collider.gameObject);
					}
				}
				else
				{
					selectedAsteroid = null;	
				}				
			}
			
			if (Input.GetMouseButton(1) && selectedAsteroid)
			{
				if (!mineEmitter)
					CreateEmitter();
				
				Quaternion previousRotation = transform.rotation;
				transform.LookAt(selectedAsteroid.transform);
				transform.rotation = Quaternion.Slerp(previousRotation, transform.rotation, 0.1f);	
				
				
				mineEmitter.transform.position = transform.position;				
				mineEmitter.transform.LookAt(selectedAsteroid.transform);				
				mineEmitter.enableEmission = true;		
				
				if(!ab)
					ab = selectedAsteroid.GetComponent("AsteroidBehavior") as AsteroidBehavior;	
				
				ab.IsBeingExtracted(5.0f);		
			}
			else
			{
				if(mineEmitter)
					mineEmitter.enableEmission = false;	
				ab = null;	
			}
			
			if (Input.GetKeyDown(KeyCode.R))
			{
				Debug.Log("Im in the change out code...");
				cState = ControlStates.Regular;
				targetingSystem.cState = ControlStates.Regular;
				selectedAsteroid = null;
			}
		}
		// End of Mining Control State
		#endregion
		
		// Brakes
		if (Input.GetButton("LeftCtrl"))
		{
			rigidbody.drag = 2.0f;	
		}
		else
		{
			rigidbody.drag = 0.2f;	
		}
		
		UpdateTractorBeamMass();

	}
	
	void CreateEmitter()
	{		
		if (mineEmitter)
			return;
		
		GameObject go = Instantiate(Resources.Load("mineEmitter")) as GameObject;
		mineEmitter = go.GetComponent("ParticleSystem") as ParticleSystem;
		go.transform.position = transform.position;	
		mineEmitter.startSize = 3;
		mineEmitter.startSpeed = 5;
		mineEmitter.startLifetime = 2.5f;
	}
	
	// Put this in its own class.
	public void SetShip(ShipAttributes inAtt)
	{	
		attributes = inAtt;
		
		// Fall through incase ship doesn't load.
		if (attributes == null)
		{
			Debug.Log("Attributes was NULL. Loading Default!");
			attributes = Ships.LoadShip_Default();
		}
		
		// RigidBody
		Debug.Log("File: "+ attributes.modelName);
		rigidbody.mass = attributes.mass;
		rigidbody.drag = 0.2f;
		rigidbody.angularDrag = 0.5f;
		
		//Collider
		var capsuleCollider = GetComponent ("CapsuleCollider") as CapsuleCollider;
		capsuleCollider.radius = (int)attributes.collisionCapsuleDimensions.x;
		capsuleCollider.height = (int)attributes.collisionCapsuleDimensions.y;
		capsuleCollider.direction = (int)attributes.collisionCapsuleDimensions.z;
		capsuleCollider.center = attributes.collisionCapsuleCenter;
		
		// Player Ship
		shipModel = Instantiate(Resources.Load(attributes.modelName)) as GameObject;
		shipModel.name = "playerShipModel";
		shipModel.tag = "player";
		shipModel.transform.localEulerAngles = new Vector3(0,0,0); //attributes.initialRotation;
		shipModel.transform.localScale = attributes.initialScale;
		shipModel.transform.parent = this.transform;
		
		playerCam = this.gameObject.GetComponent(typeof(Camera)) as Camera;
		playerCam.far = 250000.0f;
		playerCam.near = 0.35f;
		playerCam.gameObject.AddComponent<Skybox>();
		playerCam.gameObject.GetComponent<Skybox>().material = Resources.Load("Skyboxes/SuperStarMap") as Material;

		gameObject.AddComponent("TractorBeamBehavior");
		targetingSystem = gameObject.AddComponent("TargetingSystem") as TargetingSystem;

	}
	
	public void SetPositionInit()
	{

		shipModel.transform.localPosition = attributes.cameraDistance;//new Vector3(0,-0.9f,4.0f);
		shipModel.transform.localEulerAngles = attributes.initialRotation;
		
	}
	
	public ShipAttributes getShipAttributes()
	{
		return attributes;
	}
	
	public void UpdateTractorBeamMass()
	{
		
		currentTractorBeamMass = 0;
		
		
		if (this.transform.childCount < 2)
			return;
		
		int childCount = transform.childCount;
		
		// Camera and Projectiles are Children		
		for (int ndx = 0; ndx < childCount; ++ndx)
		{
			if (transform.GetChild(ndx).tag == "cargo")
			{
				currentTractorBeamMass += this.transform.GetChild(ndx).rigidbody.mass;
			}
		}
	}
	
	public void LoadShipStats(ShipAttributes inAtt)
	{
		
	}
	
	public void LoadNewShip(string inShipname)
	{
		
	}
	
	public void LoadShip()
	{
			
	}
	
	public void SaveGame()
	{
		
	}
	
	public void SetDisabled(float inTime)
	{
		cState = ControlStates.Disabled;
		disabledfor = inTime;
		cRevert = cState;
	}
	
	public ControlStates GetControlState()
	{
		return cState;	
	}
	
	#region GUI
	void OnGUI()
	{
		if (selectedAsteroid)
			ReportAsteroidSelected();		
	}
	
	void ReportAsteroidSelected()
	{
		GUI.Label(new Rect((Screen.width / 2 - 50), (Screen.height / 2),(Screen.width / 2 + 20),(Screen.height / 2 + 10)), "Asteroid Selected");			
	}
	#endregion

}

		//if(!lineTarget)
		//{
		//	lineTarget = GameObject.FindGameObjectWithTag("cargo").transform;
		//	lineRenderer = (LineRenderer)GetComponent("Line Renderer");
		//	lineRenderer.SetWidth(0.2f,0.2f);
     	//	lineRenderer.SetVertexCount(2);
		//	lineRenderer.SetPosition(0, transform.position);
		//	lineRenderer.SetPosition(1, lineTarget.position);
		//}


		// This is code for non-unity keyboard / mouse input
		/*
		<<TEMPORARY DISABLE>>
		if (Input.GetKeyDown(KeyCode.A))
		{
			this.gameObject.rigidbody.AddRelativeTorque(new Vector3(0.0f, -15.0f, 0.0f));				
		}
		
		if (Input.GetKeyDown(KeyCode.D))
		{
			this.gameObject.rigidbody.AddRelativeTorque(new Vector3(0.0f, 15.0f, 0.0f));				
		}
		
		
		if (Input.GetAxis("Mouse Y") < 0)
		{
		this.gameObject.rigidbody.AddRelativeTorque(new Vector3(-1.0f, 0.0f, 0.0f));
		//this.gameObject.rigidbody.
		}
		
		if (Input.GetAxis("Mouse Y") > 0)
		{
		this.gameObject.rigidbody.AddRelativeTorque(new Vector3(1.0f, 0.0f, 0.0f));
		}
		*/