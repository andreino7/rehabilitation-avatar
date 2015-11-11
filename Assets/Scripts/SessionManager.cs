using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.IO;

public class SessionManager : getReal3D.MonoBehaviourWithRpc {

	public GameObject objectPrefab, menuPanel;
	public AudioClip victorySound, activationSound;

	public int numberOfObjects = 10;
	public float yOffset = 1f, verticalBounds = 1f, horizontalBounds = 1f;

	public Text labelLeft, labelRight; 
	public GameObject sessionCompleteAnimation;

	private static SessionManager instance;
	private int currentObject = 0;
	private float xAvatarSize = 0.3f;

	private float elapsedTime = 0f;
	private GameObject patient;

	private AudioSource audio;

	static public bool isTimerStopped = false;

	private SessionManager () {}

	void Awake () {
		instance = this;
	}

	public static SessionManager GetInstance () {
		return instance;
	}

	// Use this for initialization
	void Start () {
		patient = GameObject.FindGameObjectWithTag("Patient");
		audio = GetComponent<AudioSource>();
		CreateNewObject ();
	}

	public void CreateNewObject() {

		if(getReal3D.Cluster.isMaster) {
			
			if (currentObject == numberOfObjects) {
				Invoke("EndSession", 1f);
				return;
			}
			isTimerStopped = false;
			Vector3 newPosition = new Vector3 (UnityEngine.Random.Range(-horizontalBounds, horizontalBounds), yOffset + UnityEngine.Random.Range(-verticalBounds, verticalBounds), patient.transform.position.z + 0.1f);
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
			//Invoke ("ChangeScene",4f);
		}
	}

	[getReal3D.RPC]
	private void EndSessionRPC() {
		audio.clip = victorySound;
		audio.Play();
		GameObject vfx = (GameObject) GameObject.Instantiate (sessionCompleteAnimation, patient.transform.position, Quaternion.identity);

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

	public void RestartSession(){
		getReal3D.RpcManager.call("RestartSessionRPC");
	}

	[getReal3D.RPC]
	private void RestartSessionRPC(){
		Application.LoadLevel("Main");
	}
	public void VoiceCommand(string command) {
		audio.clip = activationSound;
		audio.Play();
		switch(command) {
			case "RESTART": Invoke("RestartSession", 1f); break;
			case "MENU": ToggleMenu(); break;
		}
	}

	public void ToggleMenu() {
		if(menuPanel.activeSelf) {
			menuPanel.SetActive(false);
		}
		else {
			menuPanel.SetActive(true);
		}
	}

	public void PlayNotificationSound() {
		audio.clip = activationSound;
		audio.Play();
	}

}
