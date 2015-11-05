using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TrainingSelector : getReal3D.MonoBehaviourWithRpc {

	public GameObject firstPanel, secondPanel;
	public InputField patientId;

	private int trainingModeId;

	void Start(){
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
		getReal3D.RpcManager.call("StartTraining");
	}

	[getReal3D.RPC]
	private void StartTraining() {
		PlayerPrefs.SetString("PatientId", patientId.text);
		PlayerPrefs.SetInt("TrainingModeId", trainingModeId);
		Application.LoadLevel("Main");
	}
}
