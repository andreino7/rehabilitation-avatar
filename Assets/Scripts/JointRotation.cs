using UnityEngine;
using System.Collections;

public class JointRotation : MonoBehaviour {

	public GameObject hips, leftHand, rightHand, leftElbow, rightElbow;
	
	void LateUpdate() {
		Vector3 leftHandPosition = leftHand.transform.position;
		leftElbow.transform.LookAt (leftHandPosition);
		leftHand.transform.position = leftHandPosition;
	}
}
