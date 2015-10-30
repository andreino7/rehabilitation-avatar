using UnityEngine;
using System.Collections;

public class LookAtJoints : MonoBehaviour {

	public Transform objectToRotate, target;
	public float xOffset = 90f, yOffset = 180f, zOffset = 0f;
	

	// Update is called once per frame
	void LateUpdate () {
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
