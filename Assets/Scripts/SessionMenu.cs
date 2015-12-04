using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SessionMenu : ScrollableMenu {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (gameObject.activeSelf) {
			base.Update();
			if(CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.Button3)){
				if (lastButtonUpdateTime + antiBouncing < Time.time) {
					Debug.Log ("session menu " + index);

					lastButtonUpdateTime = Time.time;
					if (index > 0 && index <= 3) {
						SessionManager.GetInstance().ToggleMenu();
						switch (index) {
						case 1:
							SessionManager.GetInstance ().RestartSession();
							break;
						case 2:
							SessionManager.GetInstance ().ToggleTrainingMode();
							break;
						case 3:
							SessionManager.GetInstance ().ConfirmExit();
							break;
						default:
							break;
						}
					} 
				}
			} else if(CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.Button2)){
				if (lastButtonUpdateTime + antiBouncing < Time.time) {
					lastButtonUpdateTime = Time.time;
					SessionManager.GetInstance().ToggleMenu();
				}
			}
		}
	}


	private void UpdateGraphics() {
		for(int i=0; i<images.Length; i++) {
			if(i == index-1) {
				images[i].sprite = sprites[1];
			}
			else {
				images[i].sprite = sprites[0];
			}
		}
	}
}
