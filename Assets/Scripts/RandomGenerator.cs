using UnityEngine;
using System.Collections;

public class RandomGenerator : ObjectsManager {

	protected float yOffset = 1.65f, verticalBounds = 0.7f, horizontalBounds = 0.8f;

	public RandomGenerator() {
		numberOfObjects = 10;
	}

	protected override Vector3 PositionNewObject() {
		FlatAvatarController patient = GameObject.FindGameObjectWithTag("Patient").GetComponent<FlatAvatarController>();
			Vector3 newPosition = new Vector3 (UnityEngine.Random.Range(-horizontalBounds, horizontalBounds), yOffset + UnityEngine.Random.Range(-verticalBounds/3, verticalBounds/3), SessionManager.GetInstance ().GetPatientPosition().z + 0.3f);
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
		CreateOptimalTrajectory(newPosition);
		appearTime = Time.time;
	}

	public override void ClearTrajectories() {
		getReal3D.RpcManager.call("ClearTrajectoriesRPC");
	}

	[getReal3D.RPC]
	protected void ClearTrajectoriesRPC() {
		Destroy(directionArrow);
	}
	
}
