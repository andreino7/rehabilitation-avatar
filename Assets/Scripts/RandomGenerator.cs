using UnityEngine;
using System.Collections;

public class RandomGenerator : ObjectsManager {

	protected float yOffset = 1.5f, verticalBounds = 0.7f, horizontalBounds = 0.8f;

	public RandomGenerator() {
		numberOfObjects = 15;
	}

	protected override Vector3 PositionNewObject() {
		Vector3 newPosition = new Vector3 (UnityEngine.Random.Range(-horizontalBounds, horizontalBounds), yOffset + UnityEngine.Random.Range(-verticalBounds, verticalBounds), SessionManager.GetInstance ().GetPatientPosition().z + 0.1f);
		if(Mathf.Abs(newPosition.x) < xAvatarSize) {
			if (newPosition.x > 0)
				newPosition.x = newPosition.x + xAvatarSize;
			else if(newPosition.x < 0)
				newPosition.x = newPosition.x - xAvatarSize;
		}
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
