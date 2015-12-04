using UnityEngine;
using System.Collections;

public class ProgressiveDistanceGenerator : ObjectsManager {
	public enum Direction{LEFT, RIGHT, UPLEFT, UPRIGHT};
	protected float yOffset = 1f, verticalBounds = 1f, horizontalBounds = 1f;
	protected float increment;
	protected Direction direction = Direction.LEFT;
	private float currentX, currentY;
	
	public ProgressiveDistanceGenerator() {
		numberOfObjects = 10;
		increment = 0.2f * 3f / numberOfObjects;
		this.direction = Direction.LEFT;
		if (direction == Direction.LEFT || direction == Direction.UPLEFT) {
			currentX = -xAvatarSize;
		} else {
			currentX = xAvatarSize;
		}
		currentY = 2;
	}
	
	protected override Vector3 PositionNewObject() {
		Debug.Log("here");
		Vector3 newPosition = new Vector3();
		float z = SessionManager.GetInstance ().GetPatientPosition ().z + 0.1f;
		switch (direction) {
		case Direction.LEFT:
			currentX = currentX - increment;
			newPosition = new Vector3 (currentX,currentY,z);
			break;
		case Direction.RIGHT:
			currentX = currentX + increment;
			newPosition = new Vector3 (currentX,currentY,z);
			break;
		case Direction.UPLEFT:
			currentX = currentX - increment;
			currentY = currentY + increment;
			newPosition = new Vector3 (currentX,currentY,z);
			break;
		case Direction.UPRIGHT:
			currentX = currentX + increment;
			currentY = currentY + increment;
			newPosition = new Vector3 (currentX,currentY,z);
			break;
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
