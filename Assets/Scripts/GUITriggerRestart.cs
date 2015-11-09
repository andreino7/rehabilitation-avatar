using UnityEngine;
using System.Collections;

public class GUITriggerRestart : MonoBehaviour {
	

	void OnTriggerEnter(Collider other) {
		Invoke("ActivateFunction",1f);
	}

	void OnTriggerExit(Collider other) {
		CancelInvoke();
	}

	void ActivateFunction() {
		ObjectGenerator.GetInstance().RestartSession();
	}
}
