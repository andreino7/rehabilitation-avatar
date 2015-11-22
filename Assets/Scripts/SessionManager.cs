using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.IO;
using SimpleJSON;

public class SessionManager : getReal3D.MonoBehaviourWithRpc {

	public GameObject objectPrefab, menuPanel, trainingPanel;
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
		PlayerPrefs.SetInt("TrainingModeId", 1);
		CreateObjectManager();

	}

	public IEnumerator StartCountdown() {
		Debug.Log ("Session starting...");
		for(int i=5; i>0; i--) {
			DisplayText (".. " + i + " ..");
			PlayAudio("Countdown");
			yield return new WaitForSeconds(1f);
		}
		CreateFirstObject();
		PlayAudio ("Start");
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
		//gameObject.AddComponent<TutorialGenerator> ();
		DisplayText ("Welcome to the tutorial");
		PlayAudio ("Voce00002");

		patient.SetActive (false);
		yield return new WaitForSeconds (4f);

		GameObject pat = (GameObject)GameObject.Instantiate (Resources.Load("TutorialPatient"));
		DisplayText ("The boy on the left will represent your avatar");
		yield return new WaitForSeconds (4f);

		GameObject the = (GameObject)GameObject.Instantiate (Resources.Load("TutorialTherapist"));
		DisplayText ("The skeleton on the right will represent your therapist's avatar");
		yield return new WaitForSeconds (4f);

		Destroy(pat);
		Destroy (the);
		patient.SetActive (true);

		DisplayText ("Now walk forward to the red circle");
		yield return new WaitForSeconds (2f);
		while (patient.transform.position.z < 1f) {
			yield return null;
		}

		DisplayText ("This is the position you will have to maintain during your training");
		yield return new WaitForSeconds (4f);
		DisplayText ("Now let's start!");
		yield return new WaitForSeconds (2f);

		DisplayText ("Now reach the ball with your rigth hand and stay in position");
		UnityEngine.Object objPrefab = Resources.Load ("BasicObject");
		GameObject obj1 = (GameObject)GameObject.Instantiate (objPrefab, new Vector3 (0.8f, 1.5f, 2f), Quaternion.identity);

		while(obj1 != null) {
			yield return null;
		}

		DisplayText ("Great! Now do the same with your left hand");
		GameObject obj2 = (GameObject)GameObject.Instantiate (objPrefab, new Vector3 (-0.8f, 1.5f, 2f), Quaternion.identity);

		while(obj2 != null) {
			yield return null;
		}

		PlayAudio ("Victory");
		GameObject vfx = (GameObject) GameObject.Instantiate (sessionCompleteAnimation, patient.transform.position, Quaternion.identity);
		yield return new WaitForSeconds (3f);

		DisplayText ("Good job! Now you'll learn how to open the menu");
		yield return new WaitForSeconds (3f);

		DisplayText ("Press the 'X' button on the wand controller");
		while(!menuPanel.activeSelf) {
			yield return null;
		}

		DisplayText ("Now use the arrows to select 'Training Mode' and then press 'X' again");
		while(!trainingPanel.activeSelf) {
			yield return null;
		}

		DisplayText ("From here you can select which training mode to start.");
		yield return new WaitForSeconds (3f);

		DisplayText ("Now press two times the 'O' button on the wand controller to exit the menu.");
		while(trainingPanel.activeSelf || menuPanel.activeSelf) {
			yield return null;
		}

		DisplayText ("Nice! Now we'll try to open the menu using your voice!");
		yield return new WaitForSeconds (3f);

		DisplayText ("Say aloud the word 'menu' and the menu will appear.");
		while(!menuPanel.activeSelf) {
			yield return null;
		}

		DisplayText ("Good job! Your training is complete");
		yield return new WaitForSeconds (3f);

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
		PlayAudio ("Victory");
		GameObject vfx = (GameObject) GameObject.Instantiate (sessionCompleteAnimation, patient.transform.position, Quaternion.identity);

	}

	public void PlayAudio(string name) {
		audio.clip = (AudioClip) Resources.Load ("Audio/" + name);
		audio.Play ();
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
		PlayAudio ("Activation");
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
			PlayAudio ("Cancel");
			menu.SetActive(false);
		}
		else {
			PlayAudio ("Activation");
			menu.SetActive(true);
		}
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
