using UnityEngine;
using System.Collections;

public class GUITriggerRestart : MonoBehaviour {
	

	void OnTriggerEnter(Collider other) {
		//SessionManager.GetInstance ().PlayNotificationSound();
		Invoke("ActivateFunction",1f);
	}

	void OnTriggerExit(Collider other) {
		CancelInvoke();
	}

	void ActivateFunction() {
		SessionManager.GetInstance().RestartSession();
	}
}
