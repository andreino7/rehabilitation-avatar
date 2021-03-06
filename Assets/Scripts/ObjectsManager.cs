﻿using UnityEngine;
using System.Collections;
using SimpleJSON;

public class ObjectsManager : getReal3D.MonoBehaviourWithRpc {

	protected int currentObject = 0;
	protected int numberOfObjects;
	protected int objectsCaught = 0;

	protected JSONArray objects = new JSONArray();
	protected float appearTime;

	protected GameObject virtualObject;

	protected float xAvatarSize = 0.3f;

	protected float allowedTime = 10f;

	protected Object objectPrefab;
	protected GameObject directionArrow;

	protected void Start() {
		objectPrefab = Resources.Load ("BasicObject");
		Debug.Log (objectPrefab);

	}

	public void NextObject() {
		currentObject++;
		SessionManager.GetInstance ().StartTimer();
		SessionManager.GetInstance().UpdateCurrentObject(currentObject);

		if(directionArrow) {
			ClearTrajectories();
		}

		if (currentObject == numberOfObjects+1) {
			SessionManager.GetInstance ().StopTimer();
			Invoke("EndSession", 1f);
			return;
		}
		if (getReal3D.Cluster.isMaster) {
			Vector3 newPosition = PositionNewObject();
			Quaternion newQuaternion = Quaternion.Euler (UnityEngine.Random.Range (0f, 360f), UnityEngine.Random.Range (0.0f, 360f), UnityEngine.Random.Range (0.0f, 360f));

			MakeRPCCall(newPosition, newQuaternion);
		}
	}

	virtual protected Vector3 PositionNewObject () { return Vector3.zero; }
	virtual protected void MakeRPCCall (Vector3 newPosition, Quaternion newQuaternion) {}
	virtual public void ClearTrajectories(){}

	protected void CreateOptimalTrajectory(Vector3 newPosition) {
		if(!(SessionManager.GetInstance ().IsTrajectoryEnabled())) return;
		Vector3 hand = SessionManager.GetInstance().GetNearestHand(newPosition);
		directionArrow = (GameObject) GameObject.Instantiate(Resources.Load("Cube"), newPosition, Quaternion.identity);
		directionArrow.transform.LookAt(hand);
		directionArrow.transform.localScale = new Vector3(0.005f, 0.005f, Vector3.Distance(newPosition, hand));
		directionArrow.transform.position = ((hand-newPosition)/2f) + newPosition;
	}

	protected void EndSession() {
		if(directionArrow) {
			ClearTrajectories();
		}
		SessionManager.GetInstance().EndSession();
	//	objectsCaught = 0;
	}

	public int GetNumberOfObjectsCaught () {
		Debug.Log ("Sono get number of objects caught e ritorno: " + objectsCaught);
		return objectsCaught;
	}

	public int GetNumberOfObjects () {
		return numberOfObjects;
	}


	public void ObjectCaught(float caughtTime) {
		if (getReal3D.Cluster.isMaster) {
			JSONNode obj = new JSONClass ();
			obj ["id"].AsInt = currentObject;
			obj ["time"].AsFloat = caughtTime - appearTime;
			obj["reached"] = "Yes";
			objects.Add (obj);
		}
		SessionManager.GetInstance().RestartTimer();
		objectsCaught++;
		Debug.Log ("object caught: " + objectsCaught);
		NextObject ();
	}

	public void ObjectNotCaught(float expirationTime) {
		SessionManager.GetInstance ().StopTimer();
		if (getReal3D.Cluster.isMaster) {
			JSONNode obj = new JSONClass ();
			obj ["id"].AsInt = currentObject;
			obj ["time"].AsFloat = expirationTime - appearTime;
			obj["reached"] = "No";
			objects.Add (obj);
		}
		SessionManager.GetInstance().RestartTimer();
		NextObject ();
	}

	public JSONArray GetObjectsData() {
		return objects;
	}

	public float GetTotalElapsedTime() {
		float time = 0f;
		foreach(JSONNode obj in objects.Childs) {
			time += obj["time"].AsFloat;
		}
		return time;
	}

	void Update() {
		if (currentObject > 0 && virtualObject != null && Time.time > allowedTime + appearTime) {
			Destroy(virtualObject);
			ObjectNotCaught(Time.time);
		}
	}

	public bool isEnded() {
		return currentObject >= numberOfObjects;
	}

	public void CancelSession() {
		if(directionArrow) {
			ClearTrajectories();
		}
		Destroy (virtualObject);
		Destroy(this);
	}

}
