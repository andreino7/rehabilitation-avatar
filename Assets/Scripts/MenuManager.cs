using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour {


	public GameObject menuTemplate;

	public void OnVoiceCommand(string message) {
		if (message == "menu") {
			GameObject menu = Instantiate(menuTemplate) as GameObject;
			menu.transform.parent = transform;
		}
	}
}
