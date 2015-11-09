using UnityEngine;
using System.Collections;

public class JointRotation : MonoBehaviour {

	public Transform hips, leftHand, rightHand, leftElbow, rightElbow, leftShoulder, rightShoulder;
	public float xOffset = 90f, yOffset = 180f, zOffset = 0f;
	public float xOffset2 = 0f, yOffset2 = 180f, zOffset2 = 0f;

	public Transform leftHip, rightHip, leftKnee, rightKnee, leftFoot, rightFoot;
	public Transform leftFinger, rightFinger;
	
	void LateUpdate() {
		UpdateOrientation (leftElbow, leftHand);
		UpdateOrientation (rightElbow, rightHand);
		UpdateOrientation (leftShoulder, leftElbow);
		UpdateOrientation (rightShoulder, rightElbow);

		UpdateOrientationCustom (leftKnee, leftFoot, 0f, 90f, 0f);
		UpdateOrientationCustom (rightKnee, rightFoot, 0f, 90f, -90f);
		UpdateOrientationCustom (leftHip, leftKnee, 0f, 90f, 0f);
		UpdateOrientationCustom (rightHip, rightKnee, 0f, 90f, 0f);

		/*
		UpdateOrientationCustom (leftHand, leftFinger, xOffset, 0f, zOffset);
		UpdateOrientationCustom (rightHand, rightFinger, xOffset, 0f, zOffset);
		*/
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

	void UpdateOrientationCustom(Transform objectToRotate, Transform target, float xOffset, float yOffset, float zOffset) {
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
