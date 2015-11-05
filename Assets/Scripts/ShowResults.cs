using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShowResults : getReal3D.MonoBehaviourWithRpc {

	public Text timeLabel;

	// Use this for initialization
	void Start () {
		timeLabel.text = "Patient ID: " + PlayerPrefs.GetString("PatientId") +
			"\nTotal time: " + Mathf.Round(PlayerPrefs.GetFloat("TotalTime")) + "s";
	}
	
	public void RestartSession() {
		getReal3D.RpcManager.call("RestartSessionRPC");
	}

	public void BackToMenu () {
		getReal3D.RpcManager.call("BackToMenuRPC");
	}

	public void ExitApplication() {
		getReal3D.RpcManager.call("ExitApplicationRPC");
	}

	[getReal3D.RPC]
	private void RestartSessionRPC() {
		Application.LoadLevel ("Main");
	}

	[getReal3D.RPC]
	private void BackToMenuRPC() {
		Application.LoadLevel ("Menu");
	}

	[getReal3D.RPC]
	private void ExitApplicationRPC() {
		Application.Quit();
	}
	
}
