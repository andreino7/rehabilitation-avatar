using UnityEngine;
using System.Collections;

public class JointRotation : MonoBehaviour {

	public Transform hips, leftHand, rightHand, leftElbow, rightElbow, leftShoulder, rightShoulder;
	public float xOffset = 90f, yOffset = 180f, zOffset = 0f;
	
	void LateUpdate() {
		UpdateOrientation (leftElbow, leftHand);
		UpdateOrientation (rightElbow, rightHand);
		UpdateOrientation (rightShoulder, rightElbow);
		UpdateOrientation (rightShoulder, rightElbow);
	}

	void UpdateOrientation(Transform objectToRotate, Transform target) {
		Vector3 oldPosition = target.position;
		Quaternion oldRotation = target.rotation;
		objectToRotate.LookAt (target);
		objectToRotate.Rotate (xOffset, 0f, 0f);
		objectToRotate.Rotate (0f, yOffset, 0f);
		objectToRotate.Rotate (0f, 0f, zOffset);
		target.position = oldPosition;
		target.rotation = oldRotation;
	}
}
