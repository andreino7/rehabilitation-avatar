using UnityEngine;
using System.Collections;
using omicron;
using omicronConnector;

public class AvatarController : OmicronEventClient {

	public float yOffset = 0;

	public GameObject kinect;

	public bool useIK = false;
	public Transform objectToReach;

	public GameObject hips, leftHand, rightHand, leftElbow, rightElbow;

	public enum KinectHandState { Unknown, NotTracked, Open, Closed, Lasso };

	private Vector3 kinectPosition;

	private KinectHandState leftHandState, rightHandState;

	private Animator animator;

	void Start() {
		kinectPosition = kinect.transform.position;
		animator = GetComponent<Animator> ();
		OmicronManager omicronManager = GameObject.FindGameObjectWithTag("OmicronManager").GetComponent<OmicronManager>();
		omicronManager.AddClient(this);
	}

	//Fetch data gathered from Kinect
	void OnEvent(EventData e) {
		if (e.serviceType == EventBase.ServiceType.ServiceTypeMocap) {

			//Update joints position and state
			UpdateJointsPosition(e);

		}
	}

	private Vector3 GetJointPosition(EventData e, int jointId) {
		float[] jointPosition = new float[3];
		e.getExtraDataVector3(jointId, jointPosition);
		return new Vector3(jointPosition[0], jointPosition[1], -jointPosition[2]);
	}

	private int GetJointNumber(EventData e) {
		return (int) e.extraDataItems;
	}

	private KinectHandState FetchHandState(float value) {
		switch((int) value) {
		case(0): return KinectHandState.Unknown;
		case(1): return KinectHandState.NotTracked;
		case(2): return KinectHandState.Open;
		case(3): return KinectHandState.Closed;
		case(4): return KinectHandState.Lasso;
		default: return KinectHandState.Unknown;
		}
	}


	void OnAnimatorIK() {
		if (useIK) {
			//animator.SetLookAtWeight (1);
			//animator.SetLookAtPosition (rightHand.transform.position);
			animator.SetIKPositionWeight (AvatarIKGoal.RightHand, 1);
			animator.SetIKRotationWeight (AvatarIKGoal.RightHand, 1);  
			animator.SetIKPosition (AvatarIKGoal.RightHand, objectToReach.position);
			animator.SetIKRotation (AvatarIKGoal.RightHand, objectToReach.rotation);
		}
	}

	private void UpdateJointsPosition(EventData e) {

		//UpdateJointPosition (hips, e, 0);
		UpdateJointPosition (leftElbow, e, 7);
		UpdateJointPosition (rightElbow, e, 17);

		UpdateJointPosition (leftHand, e, 9);
		UpdateJointPosition (rightHand, e, 19);
		leftHandState = FetchHandState(e.orw);
		rightHandState = FetchHandState(e.orx);

	}


	private void UpdateJointPosition(GameObject joint, EventData e, int jointId) {
		Vector3 newPosition = GetJointPosition(e, jointId);
		if(!newPosition.Equals(Vector3.zero)) {
			joint.transform.position = newPosition + kinectPosition + new Vector3(0f,yOffset,0f);
		}
	}
}
