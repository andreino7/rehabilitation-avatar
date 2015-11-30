using UnityEngine;
using System.Collections;

public class TrainingModeMenu : ScrollableMenu {

	// Use this for initialization
	void Start () {
		numberOfButtons = 4;
	}
	
	// Update is called once per frame
	void Update () {
		if (gameObject.activeSelf) {
			base.Update();
			if(CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.Button3)){
				if (lastButtonUpdateTime + antiBouncing < Time.time) {
					lastButtonUpdateTime = Time.time;
					Debug.Log(index);
					if (index > 0 && index <= 4) {
						SessionManager.GetInstance().ToggleTrainingMode();
						SessionManager.GetInstance().StartNewTraining(index);
					} 
				}
			} else if(CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.Button2)){
				if (lastButtonUpdateTime + antiBouncing < Time.time) {
					lastButtonUpdateTime = Time.time;
					SessionManager.GetInstance().ToggleMenu();
					SessionManager.GetInstance().ToggleTrainingMode();
				}
			}
		}
	}
}