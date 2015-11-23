using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TrainingSelector : getReal3D.MonoBehaviourWithRpc {

	public GameObject firstPanel, secondPanel;
	public InputField patientId;

	private int trainingModeId;

	void Start(){
		PlayerPrefs.DeleteKey("TrainingModeId");
		if(PlayerPrefs.HasKey("PatientId")) {
			patientId.text = PlayerPrefs.GetString("PatientId");
		}
	}

	public void InsertPatientId() {
		if(patientId.text != "" && getReal3D.Cluster.isMaster) {
			firstPanel.SetActive(false);
			secondPanel.SetActive(true);
		}
	}

	public void SelectTrainingMode(int mode) {
		trainingModeId = mode;
		getReal3D.RpcManager.call("StartTraining", patientId.text, trainingModeId);
	}

	[getReal3D.RPC]
	private void StartTraining(string pId, int mode) {
		string modeName = "";
		switch(mode) {
			case 1: modeName = "Tutorial"; break;
			case 2: modeName = "Random Objects"; break;
			case 3: modeName = "Progressive distance"; break;
		}
		PlayerPrefs.SetString("PatientId", pId);
		PlayerPrefs.SetInt("TrainingModeId", mode);
		PlayerPrefs.SetString("TrainingMode", modeName);
		Application.LoadLevel("Main");
	}

	[getReal3D.RPC]
	private void SkipTrainingModeSelection() {
		PlayerPrefs.SetString("PatientId", patientId.text);
		Application.LoadLevel("Main");
	}
}
