using UnityEngine;
using System.Collections;
using omicron;
using omicronConnector;

public class AvatarController : OmicronEventClient {

	public GameObject leftHand, rightHand, leftElbow, rightElbow;

	public enum KinectHandState { Unknown, NotTracked, Open, Closed, Lasso };

	KinectHandState leftHandState, rightHandState;

	//Fetch data gathered from Kinect
	void OnEvent(EventData e) {
		if (e.serviceType == EventBase.ServiceType.ServiceTypeMocap) {

			//Update hands position and state
			UpdateHandsPosition(e);
			//UpdateJointsPosition(e);

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
	
	private void UpdateHandsPosition(EventData e) {
		//Left
		Vector3 leftHandPosition = GetJointPosition(e, 9);
		if(!leftHandPosition.Equals(Vector3.zero)) {
			leftHand.transform.position = new Vector3(leftHandPosition.x, leftHandPosition.y, leftHand.transform.position.z);
		}
		leftHandState = FetchHandState(e.orw);

		//Right
		Vector3 rightHandPosition = GetJointPosition(e, 19);
		if(!rightHandPosition.Equals(Vector3.zero)) {
			rightHand.transform.position = new Vector3(rightHandPosition.x, rightHandPosition.y, rightHand.transform.position.z);
		}
		rightHandState = FetchHandState(e.orx);
	}

	private void UpdateJointsPosition(EventData e) {
		Vector3 leftElbowPosition = GetJointPosition(e, 7);
		leftElbow.transform.position = new Vector3 (leftElbowPosition.x, leftElbowPosition.y, leftElbow.transform.position.z);
		Vector3 rightElbowPosition = GetJointPosition(e, 17);
		rightElbow.transform.position = new Vector3 (rightElbowPosition.x, rightElbowPosition.y, rightElbow.transform.position.z);
	}
}
