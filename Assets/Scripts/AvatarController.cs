using UnityEngine;
using System.Collections;
using omicron;
using omicronConnector;

public class AvatarController : OmicronEventClient {

	public GameObject leftHand, rightHand;

	public enum KinectHandState { Unknown, NotTracked, Open, Closed, Lasso };

	KinectHandState leftHandState, rightHandState;

	//Fetch data gathered from Kinect
	void OnEvent(EventData e) {
		if (e.serviceType == EventBase.ServiceType.ServiceTypeMocap) {

			//Update hands position and state
			UpdateHandsPosition(e);

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
			leftHand.transform.localPosition = leftHandPosition;
		}
		leftHandState = FetchHandState(e.orw);

		//Right
		Vector3 rightHandPosition = GetJointPosition(e, 19);
		if(!rightHandPosition.Equals(Vector3.zero)) {
			rightHand.transform.localPosition = rightHandPosition;
		}
		rightHandState = FetchHandState(e.orx);
	}
}
