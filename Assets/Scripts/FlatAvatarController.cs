using UnityEngine;
using System.Collections;
using omicron;
using omicronConnector;
using SimpleJSON;

public class FlatAvatarController : OmicronEventClient {

	protected float samplingRate = 5f;
	protected JSONArray positions = new JSONArray();

	private bool isThirdPerson = true;
	public bool isPatient = false;
	public bool isDistortedReality;

	public GameObject protoGuyBody, protoGuyHead;
	public GameObject[] bodyParts;

	public float yOffset = 0.6f, zOffset = 2.5f;
	private float lastUpdate, timeout = 0.1f;
	public int bodyId = -1;

	public OmicronKinectManager kinectManager;
	
	public GameObject hips, leftHand, rightHand, leftElbow, rightElbow, leftShoulder, rightShoulder;
	public GameObject leftHip, rightHip, leftKnee, rightKnee, leftFoot, rightFoot;
	public GameObject kinect;


	public enum KinectHandState { Unknown, NotTracked, Open, Closed, Lasso };
	private KinectHandState leftHandState, rightHandState;



	//public Transform leftHandIndicator, rightHandIndicator;
	//private Vector3 kinectPosition;
	//public GameObject leftFinger, rightFinger;
	//private float verticalDistance, horizontalDistance, verticalMultiplier, horizontalMultiplier;





	void Start() {
		OmicronManager omicronManager = GameObject.FindGameObjectWithTag("OmicronManager").GetComponent<OmicronManager>();
		omicronManager.AddClient(this);
		if(isPatient) {
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

		if (e.serviceType != EventBase.ServiceType.ServiceTypeMocap) {
			return;
		}

		int sourceId = (int)e.sourceId;
		if(bodyId != sourceId) {
			return;
		}

		/*float shoulderDistance = Vector3.Distance(GetJointPosition(e, 6), GetJointPosition(e, 16));
		horizontalMultiplier = horizontalDistance / shoulderDistance;

		verticalMultiplier = verticalDistance / GetJointPosition(e, 1).y;*/

		if (!isDistortedReality) {
			UpdateHipsPosition (e);
			UpdateJointPosition (leftElbow, e, 7);
			UpdateJointPosition (rightElbow, e, 17);
				
			UpdateJointPosition (leftHand, e, 9);
			UpdateJointPosition (rightHand, e, 19);

			UpdateJointPosition (leftShoulder, e, 6); //, new Vector3(-0.1f, -0.1f, 0f));
			UpdateJointPosition (rightShoulder, e, 16); //, new Vector3(0.1f, -0.1f, 0f));
			leftHandState = FetchHandState(e.orw);
			rightHandState = FetchHandState(e.orx);
			
			UpdateJointPosition (leftHip, e, 11);
			UpdateJointPosition (rightHip, e, 21);
			
			UpdateJointPosition (leftKnee, e, 12);
			UpdateJointPosition (rightKnee, e, 22);
			
			UpdateJointPosition (leftFoot, e, 13);
			UpdateJointPosition (rightFoot, e, 23);
		} else {
			UpdateHipsPositionDistorted(e);
			UpdateJointPositionDistorted (leftElbow, e, 17);
			UpdateJointPositionDistorted (rightElbow, e, 7);
			
			UpdateJointPositionDistorted (leftHand, e, 19);
			UpdateJointPositionDistorted (rightHand, e, 9);
			
			UpdateJointPositionDistorted (leftShoulder, e, 16); //, new Vector3(-0.1f, -0.1f, 0f));
			UpdateJointPositionDistorted (rightShoulder, e, 6);

			UpdateJointPositionDistorted (leftHip, e, 21);
			UpdateJointPositionDistorted (rightHip, e, 11);
			
			UpdateJointPositionDistorted (leftKnee, e, 22);
			UpdateJointPositionDistorted (rightKnee, e, 12);
			
			UpdateJointPositionDistorted (leftFoot, e, 23);
			UpdateJointPositionDistorted (rightFoot, e, 13);
		}




		//UpdateJointPosition (leftFinger, e, 10);
		//UpdateJointPosition (rightFinger, e, 20);

		lastUpdate = Time.time;
	}


	private void UpdateJointPosition(GameObject joint, EventData e, int jointId, Vector3 optionalOffset = default(Vector3)) {
		Vector3 newPosition = GetJointPosition(e, jointId);
		if(!newPosition.Equals(Vector3.zero)) {
			joint.transform.localPosition = newPosition + new Vector3(0f, isThirdPerson ? yOffset : 0f, isThirdPerson ? zOffset : 0f) + optionalOffset;
		}
	}

	private void UpdateJointPositionDistorted(GameObject joint, EventData e, int jointId, Vector3 optionalOffset = default(Vector3)) {
		Vector3 newPosition = GetJointPosition(e, jointId);
		newPosition = new Vector3(-newPosition.x, newPosition.y, newPosition.z);
		if(!newPosition.Equals(Vector3.zero)) {
			joint.transform.localPosition = newPosition +  new Vector3(0f, isThirdPerson ? yOffset : 0f, isThirdPerson ? zOffset : 0f) + optionalOffset;
		}
	}

	private void UpdateHipsPosition(EventData e) {
		Vector3 newPosition = GetJointPosition(e, 0);
		if(!newPosition.Equals(Vector3.zero)) {
			hips.transform.localPosition = new Vector3(newPosition.x, newPosition.y, newPosition.z) + new Vector3(0f, yOffset, zOffset);
		}
	}

	private void UpdateHipsPositionDistorted(EventData e) {
		Vector3 newPosition = GetJointPosition(e, 0);
		if(!newPosition.Equals(Vector3.zero)) {
			hips.transform.localPosition = new Vector3(-newPosition.x, newPosition.y, newPosition.z) + new Vector3(0f, yOffset, zOffset);
		}
	}

	IEnumerator LogPositionsCoroutine() {
		while(!SessionManager.GetInstance().IsTimerStopped()) {
			yield return new WaitForSeconds(1f / samplingRate);
			JSONNode newPos = new JSONClass();
			newPos["time"].AsFloat = Time.time;

			JSONArray leftHandPos = new JSONArray();
			leftHandPos[0].AsFloat = leftHand.transform.position.x;
			leftHandPos[1].AsFloat = leftHand.transform.position.y;
			leftHandPos[2].AsFloat = leftHand.transform.position.z;
			newPos["leftHand"] = leftHandPos;

			JSONArray rightHandPos = new JSONArray();
			rightHandPos[0].AsFloat = rightHand.transform.position.x;
			rightHandPos[1].AsFloat = rightHand.transform.position.y;
			rightHandPos[2].AsFloat = rightHand.transform.position.z;
			newPos["rightHand"] = rightHandPos;

			JSONArray leftElbowPos = new JSONArray();
			leftElbowPos[0].AsFloat = leftElbow.transform.position.x;
			leftElbowPos[1].AsFloat = leftElbow.transform.position.y;
			leftElbowPos[2].AsFloat = leftElbow.transform.position.z;
			newPos["leftElbow"] = leftElbowPos;
			
			JSONArray rightElbowPos = new JSONArray();
			rightElbowPos[0].AsFloat = rightElbow.transform.position.x;
			rightElbowPos[1].AsFloat = rightElbow.transform.position.y;
			rightElbowPos[2].AsFloat = rightElbow.transform.position.z;
			newPos["rightElbow"] = rightElbowPos;

			positions.Add(newPos);
		}
	}

	public JSONArray GetPositionsLog() {
		return positions;
	}

	private void KillAvatar() {
		SetFlaggedForRemoval();
		kinectManager.RemoveBody(bodyId);
		Destroy(gameObject);
	}

	private void KillPatient() {
		SetFlaggedForRemoval();
		kinectManager.RemoveBody(bodyId);
		//Destroy(gameObject);
	}



	void LateUpdate() {
		if(!isPatient && kinectManager.GetPatientId() == bodyId) {
			KillAvatar();
		}

		if (Time.time > lastUpdate + timeout) {
			if(!isPatient) {
				KillAvatar();
			} else if (bodyId != -1) {
				KillPatient();
				bodyId = -1;
				gameObject.SetActive(false);
			}
		}
	}

	public void SetBodyId(int newBodyId) {
		bodyId = newBodyId;
		lastUpdate = Time.time;
	}

	public void SetFirstPerson() {
		isThirdPerson = false;
		protoGuyHead.SetActive(false);
		protoGuyBody.SetActive(false);
		foreach(GameObject g in bodyParts) {
			g.SetActive(true);
		}
	}

	public void SetThirdPerson() {
		isThirdPerson = true;
		protoGuyHead.SetActive(true);
		protoGuyBody.SetActive(true);
		foreach(GameObject g in bodyParts) {
			g.SetActive(false);
		}
	}

	public bool IsThirdPerson() {
		return isThirdPerson;
	}

}
