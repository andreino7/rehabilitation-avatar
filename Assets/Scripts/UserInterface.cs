using UnityEngine;
using System.Collections;

public class UserInterface : MonoBehaviour {
	private float lastButtonUpdateTime = 0f;
	private float antiBouncing = 0.4f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update() {
		//Pressing R2
		if(CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.Button7)){
			if (lastButtonUpdateTime + antiBouncing < Time.time) {
				lastButtonUpdateTime = Time.time;
				SessionManager.GetInstance().ToggleMenu();
			}
		} else if(CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.Button7)){
			if (lastButtonUpdateTime + antiBouncing < Time.time) {
				lastButtonUpdateTime = Time.time;
				SessionManager.GetInstance().ToggleMenu();
			}
		} 
	}
}
