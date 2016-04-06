using UnityEngine;
using UnityEngine.UI;
using System.Net.Mail;
using UnityStandardAssets.ImageEffects;
using System.Collections;
using System;
using System.IO;
using SimpleJSON;

public class SessionManager : getReal3D.MonoBehaviourWithRpc {

	public GameObject objectPrefab, menuPanel, trainingPanel, camDisplay, helpPanel, confirmPanel, mapPanel;
	public Text textHint;
	public Material litMaterial, normalMaterial;
	public Text labelLeft, labelRight, labelMode, labelHelp; 
	public GameObject sessionCompleteAnimation;
	public AudioSource voice;
	private bool tutorialMode = false, lastPhaseOfTutorial = false; 

	private static SessionManager instance;
	private float xAvatarSize = 0.3f;
	private bool patientInsideCircle;
	private float elapsedTime = 0f;
	private GameObject patient, patientHips;
	private float lastButtonUpdateTime;
	private float antiBouncing = 0.4f;
	private float minimumZ = 4.6f;

	public float bodyOffset = 4.6f;

	private GameObject redCircle;

	private AudioSource audio;

	private ObjectsManager manager;

	protected bool isTimerStopped = true;

	private bool trajectoryFeedback = false;
	private bool isDistortedMode = false;

	protected JSONNode outputData;

	delegate void ConfirmDelegate();
	private ConfirmDelegate currentDelegate;

	private void ConfirmMethod(string message, ConfirmDelegate del) {
		ToggleMenus(confirmPanel);

		//confirmPanel.SetActive(true);
		currentDelegate = del;
	}

	public void ExecuteDelegate() {
		currentDelegate ();
		CloseMenus ();
		confirmPanel.SetActive(false);
	}

	public void CancelDelegate() {
		ToggleMenus(confirmPanel);
	}

	private SessionManager () {}

	void Awake () {
		instance = this;
	}

	public static SessionManager GetInstance () {
		return instance;
	}

	public bool IsTrajectoryEnabled() {
		return trajectoryFeedback;
	}

	// Use this for initialization
	void Start () {
		patient = GameObject.FindGameObjectWithTag("Patient");
		patientHips = GameObject.FindGameObjectWithTag("Hips");
		audio = GetComponent<AudioSource>();

		//CreateObjectManager();
		if(PlayerPrefs.HasKey("TrainingModeId")) {
			StartNewTraining(PlayerPrefs.GetInt("TrainingModeId"));
		}

	}

	public bool isTutorialMode () {
		return tutorialMode;
	}

	public IEnumerator StartCountdown() {
		ShowRedCircle();
		while (!patientInsideCircle) {
			yield return null;
		}
		//HideRedCircle();
		patientInsideCircle = false;
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

	private void ShowRedCircle() {
		if(redCircle == null) {
			FlatAvatarController script = patient.GetComponent<FlatAvatarController>();
			redCircle = (GameObject) GameObject.Instantiate(Resources.Load("RedCircle"));
			redCircle.transform.position = new Vector3(redCircle.transform.position.x, redCircle.transform.position.y, redCircle.transform.position.z - (script.IsThirdPerson() ? 0f : bodyOffset));
		}
	}

	private void HideRedCircle() {
		if(redCircle != null) {
			Destroy(redCircle);
		}
	}

	public void CreateObjectManager() {
		Debug.Log ("object manager " + PlayerPrefs.GetInt("TrainingModeId"));
		if(manager != null) {
			manager.CancelSession();
			StopTimer();
			labelLeft.text = "";
			labelRight.text = "";
			StopAllCoroutines();
		}
		if (tutorialMode) {
			StopAllCoroutines();
		}
		DisplayText (" ");
		patient.SetActive(true);
		GameObject[] objects = GameObject.FindGameObjectsWithTag("BasicObject");
		foreach (GameObject o in objects) {
			Destroy(o);
		}
		if(PlayerPrefs.HasKey("TrainingModeId")) {
			switch(PlayerPrefs.GetInt("TrainingModeId")) {
			case 1: StartCoroutine(Tutorial()); break;
			case 2: manager = gameObject.AddComponent<RandomGenerator> (); break;
			case 3: manager = gameObject.AddComponent<ProgressiveDistanceGenerator> (); break;
			case 4: manager = gameObject.AddComponent<CustomGenerator> (); break;
			}
			if(manager != null && PlayerPrefs.GetInt("TrainingModeId")!=1) {
				StartCoroutine(StartCountdown());
			}
		}
	}

	public void StartNewTraining(int trainingId) {
		PlayerPrefs.SetInt("TrainingModeId", trainingId);
		if(!manager || manager.isEnded()) {
			StartNewTrainingConfirmed();
		}
		else {
			ConfirmMethod("", StartNewTrainingConfirmed);
		}
	}

	public void StartNewTrainingConfirmed() {
		int trainingId = PlayerPrefs.GetInt("TrainingModeId");
		string modeName = "";
		switch(trainingId) {
		case 1: modeName = "Tutorial"; break;
		case 2: modeName = "Random Objects"; break;
		case 3: modeName = "Progressive distance"; break;
		case 4: modeName = "Custom training"; break;
		}
		CheckIfLabelNeeded();
		PlayerPrefs.SetString("TrainingMode", modeName);
		labelMode.text = modeName;
		CreateObjectManager();
		DisplayText("Please walk into the red circle");
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
		tutorialMode = true;
		CheckIfLabelNeeded();
		ToggleCamDisplay();
		DisplayText ("Welcome to the tutorial");
		PlayVoice ("Voce00002");

		patient.SetActive (false);
		yield return new WaitForSeconds (4f);

		GameObject pat = (GameObject)GameObject.Instantiate (Resources.Load("TutorialPatient"));
		DisplayText ("The boy on the left will represent your avatar");
		PlayVoice ("Voce00003");
		yield return new WaitForSeconds (4f);

		GameObject the = (GameObject)GameObject.Instantiate (Resources.Load("TutorialTherapist"));
		DisplayText ("The skeleton on the right will represent your therapist's avatar");
		PlayVoice ("Voce00004");
		yield return new WaitForSeconds (6f);

		Destroy(pat);
		Destroy (the);
		patient.SetActive (true);

		DisplayText ("Now walk forward to the red circle");
		ShowRedCircle();
		PlayVoice ("Voce00005");
		yield return new WaitForSeconds (6f);
		while (patientHips.transform.position.z < minimumZ) {
			yield return null;
		}

		DisplayText ("This is the position you will have to maintain during your training");
		PlayVoice ("Voce00006");
		yield return new WaitForSeconds (5f);
		DisplayText ("Now let's start!");
		//Missing audio
		yield return new WaitForSeconds (2f);
		HideRedCircle();

		DisplayText ("Reach the ball with your rigth hand and stay in position");
		PlayVoice ("Voce00007");
		UnityEngine.Object objPrefab = Resources.Load ("BasicObject");
		GameObject obj1 = (GameObject)GameObject.Instantiate (objPrefab, new Vector3 (0.8f, 1.5f, patientHips.transform.position.z + 0.1f), Quaternion.identity);

		while(obj1 != null) {
			yield return null;
		}

		DisplayText ("Great! Now do the same with your left hand");
		PlayVoice ("Voce00008");
		GameObject obj2 = (GameObject)GameObject.Instantiate (objPrefab, new Vector3 (-0.8f, 1.5f, patientHips.transform.position.z + 0.1f), Quaternion.identity);

		while(obj2 != null) {
			yield return null;
		}

		PlayAudio ("Victory");
		DisplayText ("");
		GameObject vfx = (GameObject) GameObject.Instantiate (sessionCompleteAnimation, patientHips.transform.position, Quaternion.identity);
		yield return new WaitForSeconds (4f);
		patient.SetActive (false);
		PlayVoice ("Voce00010");
		DisplayText ("Good job! Now you'll learn how to open the menu");
		yield return new WaitForSeconds (5f);
		UnityEngine.Object wandPrefab = Resources.Load ("wand");
		GameObject wand = (GameObject)GameObject.Instantiate (wandPrefab);

		wand.GetComponentsInChildren<Renderer> () [0].material = litMaterial;

		DisplayText ("Press the 'X' button on the wand controller");
		PlayVoice ("Voce00014");
		while(!menuPanel.activeSelf) {
			yield return null;
		}
		wand.GetComponentsInChildren<Renderer> () [0].material = normalMaterial;

		wand.GetComponentsInChildren<Renderer> () [3].material = litMaterial;
		wand.GetComponentsInChildren<Renderer> () [6].material = litMaterial;

		yield return new WaitForSeconds (2f);
		lastPhaseOfTutorial = true;
		DisplayText ("Now use the arrows to select 'Training Mode' and then press 'X' again");
		PlayVoice ("Voce00015");
		while(!trainingPanel.activeSelf) {
			yield return null;
		}
		wand.GetComponentsInChildren<Renderer> () [3].material = normalMaterial;
		wand.GetComponentsInChildren<Renderer> () [6].material = normalMaterial;
		yield return new WaitForSeconds (1f);
		DisplayText ("From here you can select which training mode to start.");
		PlayVoice ("Voce00016");
		yield return new WaitForSeconds (6f);

		wand.GetComponentsInChildren<Renderer> () [1].material = litMaterial;
		DisplayText ("Now press two times the 'O' button on the wand controller to exit the menu.");
		PlayVoice ("Voce00017");
		while(trainingPanel.activeSelf || menuPanel.activeSelf) {
			yield return null;
		}
		wand.GetComponentsInChildren<Renderer> () [1].material = normalMaterial;

		Destroy (wand);
		patient.SetActive (true);
		lastPhaseOfTutorial = false;
		yield return new WaitForSeconds (1f);
		DisplayText ("Nice! Now we'll try to open the menu using your voice!");
		PlayVoice ("Voce00019");
		yield return new WaitForSeconds (5f);

		DisplayText ("Say aloud the word 'menu' and the menu will appear.");
		PlayVoice ("Voce00020");
		while(!menuPanel.activeSelf) {
			yield return null;
		}
		yield return new WaitForSeconds (1f);
		DisplayText ("Good job! Your training is complete");
		ToggleCamDisplay();
		PlayVoice ("Voce00021");
		yield return new WaitForSeconds (3f);
		DisplayText ("");
		tutorialMode = false;
	}

	public void EndSession() {
		labelRight.text = "";
		labelLeft.text = "";
//		DisplayText ("!! Training Complete !!");
//		DisplayText("Number of objects caught: " + manager.GetNumberOfObjectsCaught () );
//		DisplayText("Number of objects caught: " + manager.GetNumberOfObjectsCaught () );
		StartCoroutine(EndSessionCoroutine());
		PlayerPrefs.SetFloat ("TotalTime", elapsedTime);
		if (getReal3D.Cluster.isMaster) {
			string filePath = FinalizeLogFile ();
			//SendMail(filePath);
		}
	}

	public IEnumerator EndSessionCoroutine() {
		DisplayText ("!! Training Complete !!");
		PlayAudio ("Victory");
		GameObject vfx = (GameObject) GameObject.Instantiate (sessionCompleteAnimation, patientHips.transform.position, Quaternion.identity);
		yield return new WaitForSeconds (2f);
		if (getReal3D.Cluster.isMaster) {
			Debug.Log ("sono la courutine e chiamo manager.GetNumberOfObjectsCaught(): " +  manager.GetNumberOfObjectsCaught());
			getReal3D.RpcManager.call("DisplayTrainingSummary", manager.GetNumberOfObjectsCaught(), manager.GetTotalElapsedTime());
		}
	}

	public bool isLastPhaseOfTutorial() {
		return lastPhaseOfTutorial;
	}

	[getReal3D.RPC]
	private void DisplayTrainingSummary(int numberOfObjectsCaught, float time) {
		DisplayText(/*"Mode: " + PlayerPrefs.GetString("TrainingMode") + */"Objects caught: " + numberOfObjectsCaught + " out of " + manager.GetNumberOfObjects ()+ "\nElapsed time: " + Mathf.Round(time) + "s");
	}

	public void PlayAudio(string name) {
		audio.clip = (AudioClip) Resources.Load ("Audio/" + name);
		audio.Play ();
	}

	public void PlayVoice(string name) {
		voice.clip = (AudioClip) Resources.Load ("Audio/" + name);
		voice.Play ();
	}

	private void ChangeScene(){
		getReal3D.RpcManager.call("ChangeSceneRPC");
	}

	[getReal3D.RPC]
	private void ChangeSceneRPC() {
		Application.LoadLevel ("Results");
	}
	

	public void RestartSession(){
		if(!manager || manager.isEnded()) {
			RestartConfirmed();
		}
		else {
			ConfirmMethod ("", new ConfirmDelegate(RestartConfirmed));
		}
	}

	public void RestartConfirmed() {
		CreateObjectManager();
	}

	public void ConfirmExit() {
		ConfirmMethod ("", new ConfirmDelegate(ExitSession));
	}

	public void ExitSession() {
		if(getReal3D.Cluster.isMaster){
			getReal3D.RpcManager.call("ExitSessionRPC");
		}
	}

	[getReal3D.RPC]
	private void ExitSessionRPC(){
		Application.LoadLevel("Menu");
	}
	
	public void VoiceCommand(string command) {
		PlayAudio ("Activation");
		switch(command) {
			case "RESTART": case "START": Invoke("RestartSession", 1f); break;
			case "MENU": case "OPEN MENU": ToggleMenu(); break;
			case "STOP": AbortSession(); break;
			case "EXIT": ExitSession(); break;
			case "MAP": ToggleMap(); break;
			case "TRAJECTORY": ToggleMotionTrail(); break;
			case "FIRST PERSON": FirstPersonMode(); break;
			case "DISTORTED REALITY": DistortedRealityMode(); break;
			case "THIRD PERSON": ThirdPersonMode(); break;
			case "MODE": case "TRAINING MODE": ShowTrainingModes(); break;
			case "CLOSE": case "CLOSE MENU": CloseMenus(); break;
			case "RANDOM OBJECTS": StartNewTraining(2); break;
			case "PROGRESSIVE DISTANCE": StartNewTraining(3); break;
			case "TUTORIAL": StartNewTraining(1); break;
			case "CUSTOM TRAINING": StartNewTraining(4); break;
			case "HELP": ToggleHelpPanel(); break;
		}
	}

	public void ToggleMap() {
		ToggleMenus (mapPanel);
	}

	public void CloseMenus() {
		if(menuPanel.activeSelf || trainingPanel.activeSelf) {
			PlayAudio ("Cancel");
			menuPanel.SetActive(false);
			trainingPanel.SetActive(false);
		}
	}

	public void ToggleMenu() {
		closeHelpPanel();
		ToggleMenus(menuPanel);
	}

	public void ToggleTrainingMode() {
		closeHelpPanel();
		ToggleMenus(trainingPanel);

	}

	public void AbortSession() {
		StopTimer();
		if(getReal3D.Cluster.isMaster) {
			getReal3D.RpcManager.call("DisplayTrainingSummary", manager.GetTotalElapsedTime());
		}
	}

	public void ShowTrainingModes() {
		if(menuPanel.activeSelf) {
			menuPanel.SetActive(false);
		}
		closeHelpPanel();
		trainingPanel.SetActive(true);
	}

	public void ChangePerspective() {
		FlatAvatarController script = patient.GetComponent<FlatAvatarController>();
		if(script.IsThirdPerson()) {
			FirstPersonMode();
		} else {
			ThirdPersonMode();
		}

	}

	public void FirstPersonMode() {
		FlatAvatarController script = patient.GetComponent<FlatAvatarController>();
		script.SetFirstPerson();
		//patientHips.transform.localScale = new Vector3(0f, 0f, 0f);
		GameObject[] basicObjects = GameObject.FindGameObjectsWithTag("BasicObject");
		foreach(GameObject g in basicObjects) {
			g.transform.position = new Vector3(g.transform.position.x, g.transform.position.y, g.transform.position.z - bodyOffset);
		}
	}
	public void DistortedRealityMode() {
		FlatAvatarController script = patient.GetComponent<FlatAvatarController>();
		if(script.isDistortedReality) {
			script.isDistortedReality = false;
			isDistortedMode = false;
		//	ToggleCameraEffect();
		}
		else {
			script.isDistortedReality = true;
			isDistortedMode = true;
		//	ToggleCameraEffect();
		}
		PlayAudio("Start");
		RegenerateLabelMode();
	}


	public void ThirdPersonMode() {
		FlatAvatarController script = patient.GetComponent<FlatAvatarController>();
		script.SetThirdPerson();
		GameObject[] basicObjects = GameObject.FindGameObjectsWithTag("BasicObject");
		foreach(GameObject g in basicObjects) {
			g.transform.position = new Vector3(g.transform.position.x, g.transform.position.y, g.transform.position.z + bodyOffset);
		}
		//patientHips.transform.localScale = new Vector3(1.3f, 1f, 1f);
	}

	public void ToggleCamDisplay() {
		if(camDisplay.activeSelf) {
			camDisplay.SetActive(false);
		}
		else {
			camDisplay.SetActive(true);
		}
	}

	private void ToggleMenus (GameObject menu) {
		Debug.Log (menu);
		if(menu.GetComponent<ScrollableMenu>()) {
			menu.GetComponent<ScrollableMenu>().SetActivationTime(Time.time);
		}
		if(menu.activeSelf) {
			PlayAudio ("Cancel");
			menu.SetActive(false);
		}
		else {
			PlayAudio ("Activation");
			menu.SetActive(true);
		}
		CheckIfLabelNeeded();
	}

	private void CheckIfLabelNeeded() {
		if (menuPanel.activeSelf || trainingPanel.activeSelf || confirmPanel.activeSelf || helpPanel.activeSelf || tutorialMode) {
			labelHelp.enabled = false;
		} else {
			labelHelp.enabled = true;
		}
	}

	public Vector3 GetPatientPosition() {
		FlatAvatarController script = patient.GetComponent<FlatAvatarController>();
		return patientHips.transform.position - new Vector3(0f, 0f, script.IsThirdPerson() ? 0f : bodyOffset);
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

	private void SendMail(string filePath) {
		Debug.Log ("sending email");
		MailMessage mail = new MailMessage("rehabilitation-avatar@mail.com", "mastercava@hotmail.it");
		SmtpClient client = new SmtpClient();
		Attachment attachment = new Attachment(filePath);
		mail.Attachments.Add(attachment);
		client.Port = 587;
		client.DeliveryMethod = SmtpDeliveryMethod.Network;
		client.UseDefaultCredentials = false;
		client.Host = "smtp.mail.com";
		client.Credentials = (System.Net.ICredentialsByHost) new System.Net.NetworkCredential("rehabilitation-avatar@mail.com", "password" );
		mail.Subject = "New training log for patient " + PlayerPrefs.GetString("PatientId");
		mail.Body = "The training log file is attached to this email.";
		client.Send(mail);
	}

	private string FinalizeLogFile() {

		outputData["elapsedTime"].AsFloat = manager.GetTotalElapsedTime();
		outputData ["numberOfObjects"].AsInt = manager.GetNumberOfObjects ();
		outputData ["objects"] = manager.GetObjectsData ();
		outputData["positions"] = patient.GetComponent<FlatAvatarController>().GetPositionsLog();
		string filePath = "C:\\Users\\evldemo\\Desktop\\Rehabilitation Log\\" + (PlayerPrefs.GetString("PatientId").Replace(" ", "")) + "_" + (PlayerPrefs.GetString("TrainingMode").Replace(" ", "")) + "_" + GetTimestamp(DateTime.Now) + ".txt";
		using (StreamWriter sw = new StreamWriter(filePath)) {
			sw.Write(outputData.ToString());
			sw.Close();
		}
		Debug.Log(outputData.ToString());
		return filePath;
	}

	private String GetTimestamp(DateTime value) {
		return value.ToString("yyyyMMddHHmmssffff");
	}
	
	void Update() {
		if(CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.Button3)){
			if (lastButtonUpdateTime + antiBouncing < Time.time) {
				lastButtonUpdateTime = Time.time;
				if (!menuPanel.activeSelf && !trainingPanel.activeSelf) {
					ToggleMenu();
				}
			}
		}
		if(!isTimerStopped) UpdateTime();
	}

	public void DisplayText(string text) {
		textHint.text = text;
	}

	public void ToggleHelpPanel() {
		closeAllMenu();
		ToggleMenus(helpPanel);
	}

	public void closeHelpPanel() {
		Debug.Log("close help panel");
		helpPanel.SetActive(false);
	}

	private void closeAllMenu() {
		menuPanel.SetActive(false);
		trainingPanel.SetActive(false);
	}

/*	private void ToggleCameraEffect() {
		Grayscale cameraEffect = Camera.main.gameObject.GetComponent<Grayscale>();
		if(cameraEffect.enabled) {
			cameraEffect.enabled = false;
		}
		else {
			cameraEffect.enabled = true;
		}
	}
*/
	public Vector3 GetNearestHand(Vector3 pos) {
		FlatAvatarController controller = patient.GetComponent<FlatAvatarController>();
		Vector3 leftHand = controller.leftHand.transform.position;
		Vector3 rightHand = controller.rightHand.transform.position;
		if(Vector3.Distance(leftHand, pos) > Vector3.Distance(rightHand, pos)) {
			return rightHand;
		}
		return leftHand;
	}

	public void ToggleMotionTrail() {
		trajectoryFeedback = !trajectoryFeedback;
		FlatAvatarController controller = patient.GetComponent<FlatAvatarController>();
		TrailRenderer left = controller.leftHand.GetComponent<TrailRenderer>();
		TrailRenderer right = controller.rightHand.GetComponent<TrailRenderer>();
		left.enabled = trajectoryFeedback;
		right.enabled = trajectoryFeedback;
		PlayAudio("Start");
		RegenerateLabelMode();
	}

	private void RegenerateLabelMode() {
		string text = PlayerPrefs.GetString("TrainingMode") + "\n";
		if(trajectoryFeedback) {
			text += "Trajectory";
		}
		if(isDistortedMode) {
			text += (trajectoryFeedback ? " + " : "") + "Distorsion";
		}
		if(trajectoryFeedback ^ isDistortedMode) {
			text += " enabled";
		}
		labelMode.text = text;
	}

	public void PatientInPosition() {
		patientInsideCircle = true;
	}

}
