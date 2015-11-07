using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.IO;

public class ObjectGenerator : getReal3D.MonoBehaviourWithRpc {

	public GameObject objectPrefab;

	public int numberOfObjects = 10;
	public float yOffset = 1f, verticalBounds = 1f, horizontalBounds = 1f;

	public Text labelLeft, labelRight; 
	public GameObject sessionCompleteAnimation;

	private static ObjectGenerator instance;
	private int currentObject = 0;
	private float xAvatarSize = 0.3f;

	private float elapsedTime = 0f;

	static public bool isTimerStopped = false;

	private ObjectGenerator () {}

	void Awake () {
		instance = this;
	}

	public static ObjectGenerator GetInstance () {
		return instance;
	}

	// Use this for initialization
	void Start () {
		CreateNewObject ();
	}

	public void CreateNewObject() {

		if(getReal3D.Cluster.isMaster) {
			
			if (currentObject == numberOfObjects) {
				Invoke("EndSession", 1f);
				return;
			}
			isTimerStopped = false;
			Vector3 newPosition = new Vector3 (UnityEngine.Random.Range(-horizontalBounds, horizontalBounds), yOffset + UnityEngine.Random.Range(-verticalBounds, verticalBounds), transform.position.z + 0.1f);
			if(Mathf.Abs(newPosition.x) < xAvatarSize) {
				if (newPosition.x > 0)
					newPosition.x = newPosition.x + xAvatarSize;
				else if(newPosition.x < 0)
					newPosition.x = newPosition.x - xAvatarSize;
			}
			Quaternion newQuaternion = Quaternion.Euler (UnityEngine.Random.Range (0f, 360f), UnityEngine.Random.Range (0.0f, 360f), UnityEngine.Random.Range (0.0f, 360f));
			getReal3D.RpcManager.call("CreateNewObjectRPC", newPosition, newQuaternion);
		}
	}

	[getReal3D.RPC]
	private void CreateNewObjectRPC (Vector3 newPosition, Quaternion newQuaternion) {
		currentObject++;
		elapsedTime = Time.time;
		labelLeft.text = "Object #" + currentObject;
		Instantiate (objectPrefab, newPosition, newQuaternion);
	}

	void Update() {
		if(!isTimerStopped) UpdateTime();
	}

	private void UpdateTime() {
		labelRight.text = "Time: " + (Time.time-elapsedTime);
	}

	private void EndSession() {
		PlayerPrefs.SetFloat ("TotalTime", elapsedTime);
		FlatAvatarController.outputData["elapsedTime"].AsFloat = elapsedTime;
		FlatAvatarController.outputData["numberOfObjects"].AsInt = numberOfObjects;
		FlatAvatarController.outputData["positions"] = FlatAvatarController.positions;
		if(getReal3D.Cluster.isMaster) WriteLogFile();
		if(getReal3D.Cluster.isMaster) {
			getReal3D.RpcManager.call("EndSessionRPC");
			Invoke ("ChangeScene",4f);
		}
	}

	[getReal3D.RPC]
	private void EndSessionRPC() {
		GetComponent<AudioSource>().Play();
		GameObject vfx = (GameObject) GameObject.Instantiate (sessionCompleteAnimation, transform.position, Quaternion.identity);

	}

	private void ChangeScene(){
		getReal3D.RpcManager.call("ChangeSceneRPC");
	}

	[getReal3D.RPC]
	private void ChangeSceneRPC() {
		Application.LoadLevel ("Results");
	}

	private void WriteLogFile() {
		using (StreamWriter sw = new StreamWriter("C:\\Users\\evldemo\\Desktop\\Rehabilitation Log\\log.txt")) {
			sw.Write(FlatAvatarController.outputData.ToString());
			sw.Close();
		}
		Debug.Log(FlatAvatarController.outputData.ToString());
	}
}
