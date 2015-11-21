using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.IO;
using SimpleJSON;

public class SessionManager : getReal3D.MonoBehaviourWithRpc {

	public GameObject objectPrefab, menuPanel, trainingPanel;
	public AudioClip victorySound, activationSound;
	public Text textHint;

	public Text labelLeft, labelRight; 
	public GameObject sessionCompleteAnimation;

	private static SessionManager instance;
	private float xAvatarSize = 0.3f;

	private float elapsedTime = 0f;
	private GameObject patient;
	private float lastButtonUpdateTime;
	private float antiBouncing = 0.05f;

	private AudioSource audio;

	private ObjectsManager manager;

	protected bool isTimerStopped = true;

	protected JSONNode outputData;


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
		//PlayerPrefs.SetInt("TrainingModeId", 1);
		CreateObjectManager();

	}

	public IEnumerator StartCountdown() {
		Debug.Log ("Session starting...");
		DisplayText (".. 5 ..");
		yield return new WaitForSeconds(1f);
		DisplayText (".. 4 ..");
		yield return new WaitForSeconds(1f);
		DisplayText (".. 3 ..");
		yield return new WaitForSeconds(1f);
		DisplayText (".. 2 ..");
		yield return new WaitForSeconds(1f);
		DisplayText (".. 1 ..");
		yield return new WaitForSeconds(1f);
		CreateFirstObject();
		DisplayText ("Session started!!");
		yield return new WaitForSeconds(2f);
		DisplayText ("");
	}

	public void CreateObjectManager() {
		if(manager != null) {
			manager.CancelSession();
		}
		if(PlayerPrefs.HasKey("TrainingModeId")) {
			switch(PlayerPrefs.GetInt("TrainingModeId")) {
			case 1: StartCoroutine(Tutorial()); break;
			case 2: manager = gameObject.AddComponent<RandomGenerator> (); break;
			case 3: manager = gameObject.AddComponent<ProgressiveDistanceGenerator> (); break;
			}
			if(manager != null && PlayerPrefs.GetInt("TrainingModeId")!=1) {
				StartCoroutine(StartCountdown());
			}
		}
	}

	public void CreateFirstObject() {
		manager.NextObject ();
		elapsedTime = Time.time;
		InitializeOutput ();
	}


	public void StopTimer() {
		isTimerStopped = true;
	}

	public void StartTimer() {
		isTimerStopped = false;
	}

	private void UpdateTime() {
		labelRight.text = "Time: " + (Time.time-elapsedTime);
	}

	public void UpdateCurrentObject(int objectNumber) {
		labelLeft.text = "Object #" + (objectNumber);
	}

	public IEnumerator Tutorial() {
		gameObject.AddComponent<TutorialGenerator> ();
		DisplayText ("Welcome to the tutorial");
		yield return new WaitForSeconds (3f);
		DisplayText ("Now walk forward to the red circle");
		yield return new WaitForSeconds (2f);
		while (patient.transform.position.z > 1f) {
			yield return null;
		}
		DisplayText ("This is the position you will have to maintain during your training");
		yield return new WaitForSeconds (4f);
		DisplayText ("Now let's start!");
		yield return new WaitForSeconds (2f);
	}

	public void EndSession() {
		labelRight.text = "";
		labelLeft.text = "";
		DisplayText ("!! Training Complete !!");
		PlayerPrefs.SetFloat ("TotalTime", elapsedTime);
		if (getReal3D.Cluster.isMaster) {
			FinalizeLogFile ();
		}
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

	public void ChangeTrainingMode () {
		getReal3D.RpcManager.call("ChangeTrainingModeRPC");
	}

	public void RestartSession(){
		getReal3D.RpcManager.call("RestartSessionRPC");
	}

	[getReal3D.RPC]
	private void ChangeTrainingModeRPC(){
		Application.LoadLevel("");
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
		ToggleMenus(menuPanel);
	}

	public void ToggleTrainingMode() {
		ToggleMenus(trainingPanel);

	}

	private void ToggleMenus (GameObject menu) {
		menu.GetComponent<ScrollableMenu>().SetActivationTime(Time.time);
		if(menu.activeSelf) {
			menu.SetActive(false);
		}
		else {
			menu.SetActive(true);
		}
	}

	public void PlayNotificationSound() {
		audio.clip = activationSound;
		audio.Play();
	}

	public Vector3 GetPatientPosition() {
		return patient.transform.position;
	}

	public void RestartTimer() {
		elapsedTime = Time.time;
	}
	

	public bool IsTimerStopped() {
		return isTimerStopped;
	}

	protected void InitializeOutput() {
		outputData = new JSONClass();
		outputData["patientId"] = PlayerPrefs.GetString("PatientId");
		outputData["trainingId"] = PlayerPrefs.GetString("TrainingModeId");
	}

	private void FinalizeLogFile() {

		outputData["elapsedTime"].AsFloat = elapsedTime;
		outputData ["numberOfObjects"].AsInt = manager.GetNumberOfObjects ();
		outputData ["objects"] = manager.GetObjectsData ();
		outputData["positions"] = patient.GetComponent<FlatAvatarController>().GetPositionsLog();

		using (StreamWriter sw = new StreamWriter("C:\\Users\\evldemo\\Desktop\\Rehabilitation Log\\log.txt")) {
			sw.Write(outputData.ToString());
			sw.Close();
		}
		Debug.Log(outputData.ToString());
	}

	void Update() {
		if(CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.Button3)){
			if (lastButtonUpdateTime + antiBouncing < Time.time) {
				lastButtonUpdateTime = Time.time;
				if (!menuPanel.activeSelf && !trainingPanel.activeSelf) {
					ToggleMenus(menuPanel);
				}
			}
		}
		if(!isTimerStopped) UpdateTime();
	}

	public void DisplayText(string text) {
		textHint.text = text;
	}
}
