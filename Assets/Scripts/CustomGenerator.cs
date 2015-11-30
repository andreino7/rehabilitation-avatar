using UnityEngine;
using System.Collections;
using SimpleJSON;

public class CustomGenerator : ObjectsManager {

	protected float yOffset = 1.5f, verticalBounds = 0.7f, horizontalBounds = 0.8f;

	JSONArray customObjects;

	public CustomGenerator() {
		numberOfObjects = 15;
		TextAsset json = (TextAsset) Resources.Load ("CustomObjects");
		customObjects = (JSON.Parse (json.text)).AsArray;
		numberOfObjects = customObjects.Count;
	}

	protected override Vector3 PositionNewObject() {
		JSONNode currentObj = customObjects[currentObject];
		Vector3 newPosition = new Vector3 (currentObj["x"].AsFloat, currentObj["y"].AsFloat, SessionManager.GetInstance ().GetPatientPosition().z + currentObj["z"].AsFloat);
		return newPosition;
	}

	protected override void MakeRPCCall(Vector3 newPosition, Quaternion newQuaternion) {
		getReal3D.RpcManager.call("CreateNewObjectRPC", newPosition, newQuaternion);
	}


	[getReal3D.RPC]
	private void CreateNewObjectRPC (Vector3 newPosition, Quaternion newQuaternion) {
		virtualObject = (GameObject) GameObject.Instantiate (objectPrefab, newPosition, newQuaternion);
		virtualObject.GetComponent<VirtualObject> ().manager = this;
		appearTime = Time.time;
	}
	
}
