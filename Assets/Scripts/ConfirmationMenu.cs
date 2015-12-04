using UnityEngine;
using System.Collections;

public class ConfirmationMenu : ScrollableMenu {

	// Use this for initialization
	void Start () {
		numberOfButtons = 2;
	}
	
	// Update is called once per frame
	void Update () {
		if(CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.ButtonRight)){
			if (lastButtonUpdateTime + antiBouncing < Time.time) {
				lastButtonUpdateTime = Time.time;
				if (index < 2) {
					index = index+1;
				} else {
					index = 1;
				}
				UpdateGraphics();
			}
		} else if(CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.ButtonLeft)){
			if (lastButtonUpdateTime + antiBouncing < Time.time) {
				lastButtonUpdateTime = Time.time;
				if (index > 1) {
					index = index-1;
				} else {
					index = numberOfButtons;
				}
				UpdateGraphics();
			}
		} else if(CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.Button3)){
			if (lastButtonUpdateTime + antiBouncing < Time.time) {
				lastButtonUpdateTime = Time.time;
				Debug.Log(index);
				if (index == 1) {
					SessionManager.GetInstance().ExecuteDelegate();
				} else if (index == 2) {
					SessionManager.GetInstance().CancelDelegate();
				}
			}
		} else if(CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.Button2)) {
			if (lastButtonUpdateTime + antiBouncing < Time.time) {
				lastButtonUpdateTime = Time.time;
				SessionManager.GetInstance().CancelDelegate();
			}
		}
	}

}
