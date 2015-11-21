using UnityEngine;
using System.Collections;

public class TutorialGenerator : ObjectsManager {

	public TutorialGenerator() {
		numberOfObjects = 3;
	}
	
	protected override Vector3 PositionNewObject() {
		return Vector3.zero;
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
