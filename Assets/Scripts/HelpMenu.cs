using UnityEngine;
using System.Collections;

public class HelpMenu : ScrollableMenu {
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.Button2)){
			if (lastButtonUpdateTime + antiBouncing < Time.time) {
				lastButtonUpdateTime = Time.time;
				SessionManager.GetInstance().ToggleHelpPanel();
			}
		}
	}
}
