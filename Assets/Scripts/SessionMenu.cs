using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SessionMenu : MonoBehaviour {

	private float lastButtonUpdateTime = 0f;
	private float antiBouncing = 0.05f;
	private int index = 0;
	private int numberOfButtons = 4;
	public Image[] images = new Image[3];
	public Sprite[] sprites = new Sprite[2];

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.ButtonDown)){
			if (lastButtonUpdateTime + antiBouncing < Time.time) {
				lastButtonUpdateTime = Time.time;
				index = (index+1)%4;
				UpdateGraphics();
			}
		} else if(CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.ButtonUp)){
					if (lastButtonUpdateTime + antiBouncing < Time.time) {
						lastButtonUpdateTime = Time.time;
						if (index > 0) {
							index = index-1;
						} else {
					index = numberOfButtons;
						}
				UpdateGraphics();
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
