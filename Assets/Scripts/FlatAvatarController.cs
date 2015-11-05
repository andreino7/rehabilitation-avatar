using UnityEngine;
using System.Collections;
using omicron;
using omicronConnector;

public class FlatAvatarController : OmicronEventClient {

	public float yOffset = 0;

	public GameObject kinect;
	
	public GameObject hips, leftHand, rightHand, leftElbow, rightElbow, leftShoulder, rightShoulder, head;
	public Transform leftHandIndicator, rightHandIndicator;

	public enum KinectHandState { Unknown, NotTracked, Open, Closed, Lasso };

	private Vector3 kinectPosition;

	private KinectHandState leftHandState, rightHandState;
	private Vector3 originalHipsPosition;

	private float verticalDistance, horizontalDistance, verticalMultiplier, horizontalMultiplier;
	

	void Start() {
		/*
		horizontalDistance = Vector3.Distance(leftShoulder.transform.position, rightShoulder.transform.position);
		verticalDistance = head.transform.position.y;
		kinectPosition = kinect.transform.position;
		originalHipsPosition = hips.transform.position;
		*/
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



	private void UpdateJointsPosition(EventData e) {

		float shoulderDistance = Vector3.Distance(GetJointPosition(e, 6), GetJointPosition(e, 16));
		horizontalMultiplier = horizontalDistance / shoulderDistance;

		verticalMultiplier = verticalDistance / GetJointPosition(e, 1).y;

		UpdateHipsPosition (e);

		UpdateJointPosition (leftElbow, e, 7);
		UpdateJointPosition (rightElbow, e, 17);

		UpdateJointPosition (leftHand, e, 9);
		UpdateJointPosition (rightHand, e, 19);

		UpdateJointPosition (leftShoulder, e, 6);
		UpdateJointPosition (rightShoulder, e, 16);

		leftHandState = FetchHandState(e.orw);
		rightHandState = FetchHandState(e.orx);

		//leftHandIndicator.localPosition = GetJointPosition(e, 9);
		//rightHandIndicator.localPosition = GetJointPosition(e, 19);

	}


	private void UpdateJointPosition(GameObject joint, EventData e, int jointId) {
		Vector3 newPosition = GetJointPosition(e, jointId);
		if(!newPosition.Equals(Vector3.zero)) {
			joint.transform.localPosition = newPosition + new Vector3(0f,0.6f,2.5f);
		}
	}

	private void UpdateHipsPosition(EventData e) {
		Vector3 newPosition = GetJointPosition(e, 0);
		if(!newPosition.Equals(Vector3.zero)) {
			hips.transform.localPosition = new Vector3(newPosition.x, newPosition.y, newPosition.z) + new Vector3(0f,0.6f,2.5f);
		}
	}

}
