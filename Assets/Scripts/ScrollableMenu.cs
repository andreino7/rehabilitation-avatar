using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScrollableMenu : MonoBehaviour {

	protected float lastButtonUpdateTime = 0f;
	protected float antiBouncing = 0.05f;
	protected int index = 0;
	protected int numberOfButtons = 3;
	public Image[] images = new Image[3];
	public Sprite[] sprites = new Sprite[2];


	
	// Update is called once per frame
	virtual protected void Update () {
		if(CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.ButtonDown)){
			if (lastButtonUpdateTime + antiBouncing < Time.time) {
				lastButtonUpdateTime = Time.time;
				index = (index%numberOfButtons)+1;
				UpdateGraphics();
			}
		} else if(CAVE2Manager.GetButtonDown(1,CAVE2Manager.Button.ButtonUp)){
			if (lastButtonUpdateTime + antiBouncing < Time.time) {
				lastButtonUpdateTime = Time.time;
				if (index > 1) {
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

	public void SetActivationTime(float time) {
		lastButtonUpdateTime = time;
	}
}
