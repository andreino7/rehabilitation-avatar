using UnityEngine;
using System.Collections;
using omicron;
using omicronConnector;
using SimpleJSON;

public class FlatAvatarController : OmicronEventClient {

	public bool isThirdPerson = true;
	public float yOffset = 0.6f, zOffset = 2.5f;

	public GameObject kinect;
	public OmicronKinectManager kinectManager;
	
	public GameObject hips, leftHand, rightHand, leftElbow, rightElbow, leftShoulder, rightShoulder, head;
	public GameObject leftHip, rightHip, leftKnee, rightKnee, leftFoot, rightFoot;
	public GameObject leftFinger, rightFinger;
	public Transform leftHandIndicator, rightHandIndicator;

	public enum KinectHandState { Unknown, NotTracked, Open, Closed, Lasso };

	private Vector3 kinectPosition;

	private KinectHandState leftHandState, rightHandState;
	private Vector3 originalHipsPosition;

	public int bodyId = -1;

	private float verticalDistance, horizontalDistance, verticalMultiplier, horizontalMultiplier;

	static public JSONNode outputData;
	static public JSONArray positions = new JSONArray();

	public bool isPatient = false;
	private float lastUpdate, timeout = 0.1f;

	void Start() {
		/*
		horizontalDistance = Vector3.Distance(leftShoulder.transform.position, rightShoulder.transform.position);
		verticalDistance = head.transform.position.y;
		kinectPosition = kinect.transform.position;
		originalHipsPosition = hips.transform.position;
		*/
		OmicronManager omicronManager = GameObject.FindGameObjectWithTag("OmicronManager").GetComponent<OmicronManager>();
		omicronManager.AddClient(this);
		if(isPatient) {
			outputData = new JSONClass();
			outputData["patientId"] = PlayerPrefs.GetString("PatientId");
			outputData["trainingId"] = PlayerPrefs.GetString("TrainingModeId");
			Debug.Log(outputData.ToString());
			StartCoroutine(LogPositionsCoroutine());
		}
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

		if (e.serviceType != EventBase.ServiceType.ServiceTypeMocap) return;

		int sourceId = (int)e.sourceId;
		//Debug.Log(sourceId);
		//if(bodyId == -1 && sourceId > 1) bodyId=sourceId;

		if(bodyId != sourceId) {
			return;
		}

		float shoulderDistance = Vector3.Distance(GetJointPosition(e, 6), GetJointPosition(e, 16));
		horizontalMultiplier = horizontalDistance / shoulderDistance;

		verticalMultiplier = verticalDistance / GetJointPosition(e, 1).y;

		UpdateHipsPosition (e);

		UpdateJointPosition (leftElbow, e, 7);
		UpdateJointPosition (rightElbow, e, 17);

		UpdateJointPosition (leftHand, e, 9);
		UpdateJointPosition (rightHand, e, 19);

		UpdateJointPosition (leftShoulder, e, 6, new Vector3(-0.1f, -0.1f, 0f));
		UpdateJointPosition (rightShoulder, e, 16, new Vector3(0.1f, -0.1f, 0f));

		leftHandState = FetchHandState(e.orw);
		rightHandState = FetchHandState(e.orx);

		UpdateJointPosition (leftHip, e, 11);
		UpdateJointPosition (rightHip, e, 21);

		UpdateJointPosition (leftKnee, e, 12);
		UpdateJointPosition (rightKnee, e, 22);

		UpdateJointPosition (leftFoot, e, 13);
		UpdateJointPosition (rightFoot, e, 23);


		//UpdateJointPosition (leftFinger, e, 10);
		//UpdateJointPosition (rightFinger, e, 20);

		lastUpdate = Time.time;
	}


	private void UpdateJointPosition(GameObject joint, EventData e, int jointId, Vector3 optionalOffset = default(Vector3)) {
		Vector3 newPosition = GetJointPosition(e, jointId);
		if(!newPosition.Equals(Vector3.zero)) {
			joint.transform.localPosition = newPosition + new Vector3(0f, yOffset, (isThirdPerson ? zOffset : 0f)) + optionalOffset;
		}
	}

	private void UpdateHipsPosition(EventData e) {
		Vector3 newPosition = GetJointPosition(e, 0);
		if(!newPosition.Equals(Vector3.zero)) {
			hips.transform.localPosition = new Vector3(newPosition.x, newPosition.y, newPosition.z) + new Vector3(0f, yOffset, (isThirdPerson ? zOffset : 0f));
		}
	}

	IEnumerator LogPositionsCoroutine() {
		while(!ObjectGenerator.isTimerStopped) {
			yield return new WaitForSeconds(0.05f);
			JSONNode newPos = new JSONClass();
			newPos["time"].AsFloat = Time.time;

			JSONArray leftHandPos = new JSONArray();
			leftHandPos[0].AsFloat = leftHand.transform.position.x;
			leftHandPos[1].AsFloat = leftHand.transform.position.y;
			leftHandPos[2].AsFloat = leftHand.transform.position.z;

			newPos["leftHand"] = leftHandPos;

			positions.Add(newPos);
		}
	}

	private void KillAvatar() {
		SetFlaggedForRemoval();
		kinectManager.RemoveBody(bodyId);
		Destroy(gameObject);
	}

	void LateUpdate() {
		if(!isPatient && kinectManager.GetPatientId() == bodyId) {
			KillAvatar();
		}

		if (Time.time > lastUpdate + timeout) {
			if(!isPatient) {
				KillAvatar();
			} else {
				bodyId = -1;
				lastUpdate = int.MaxValue;
			}
		}
	}

}
