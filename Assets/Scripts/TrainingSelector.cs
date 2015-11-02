using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TrainingSelector : MonoBehaviour {

	public Text patientId;

	public void InsertPatientId() {
		if(patientId.text != "") {
			PlayerPrefs.SetString("PatientId", patientId.text);
			Application.LoadLevel("Main");
		}
	}

}
