using UnityEngine;
using System.Collections;
using omicron;
using omicronConnector;

public class IKAvatarController : OmicronEventClient {
	
	public GameObject kinect;

	public Transform leftHand, rightHand,leftHandIndicator, rightHandIndicator;
	
	public enum KinectHandState { Unknown, NotTracked, Open, Closed, Lasso };
	
	private Vector3 kinectPosition, rightHandPosition, leftHandPosition;
	private Quaternion leftHandRotation, rightHandRotation;
	
	private KinectHandState leftHandState, rightHandState;
	
	private Animator animator;
	
	void Start() {
		leftHandPosition = leftHand.position;
		leftHandRotation = leftHand.rotation;
		rightHandPosition = rightHand.position;
		rightHandRotation = rightHand.rotation;
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
		//animator.SetLookAtWeight (1);
		//animator.SetLookAtPosition (rightHand.transform.position);

		//Right hand IK
		animator.SetIKPositionWeight (AvatarIKGoal.RightHand, 1);
		animator.SetIKRotationWeight (AvatarIKGoal.RightHand, 1);  
		animator.SetIKPosition (AvatarIKGoal.RightHand, rightHandPosition);
		//animator.SetIKRotation (AvatarIKGoal.RightHand, rightHandRotation);

		//Left hand IK
		animator.SetIKPositionWeight (AvatarIKGoal.LeftHand, 1);
		animator.SetIKRotationWeight (AvatarIKGoal.LeftHand, 1);  
		animator.SetIKPosition (AvatarIKGoal.LeftHand, leftHandPosition);
		//animator.SetIKRotation (AvatarIKGoal.RightHand, rightHandRotation);
	}
	
	private void UpdateJointsPosition(EventData e) {

		leftHandPosition = GetJointPosition(e, 9);
		leftHandIndicator.position = leftHandPosition + kinectPosition;
		rightHandPosition = GetJointPosition(e, 19);
		rightHandIndicator.position = rightHandPosition + kinectPosition;
		
	}

}
