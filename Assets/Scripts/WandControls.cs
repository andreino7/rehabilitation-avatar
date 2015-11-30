using UnityEngine;
using System.Collections;

public class WandControls : MonoBehaviour {

	protected float lastButtonUpdateTime = 0f;
	protected float antiBouncing = 0.4f;
	
	// Update is called once per frame
	void Update () {
		//Distorted reality
		if(CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.Button5)){
			if (lastButtonUpdateTime + antiBouncing < Time.time) {
				lastButtonUpdateTime = Time.time;
				SessionManager.GetInstance().DistortedRealityMode();
			}
		}

		if(CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.Button7)){
			if (lastButtonUpdateTime + antiBouncing < Time.time) {
				lastButtonUpdateTime = Time.time;
				SessionManager.GetInstance().ToggleHelpPanel();
			}
		}
	}
}
