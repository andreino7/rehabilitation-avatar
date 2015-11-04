using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShowResults : getReal3D.MonoBehaviourWithRpc {

	public Text timeLabel;

	// Use this for initialization
	void Start () {
		timeLabel.text = "Total time: " + Mathf.Round(PlayerPrefs.GetFloat("TotalTime"));
	}
	
	public void RestartSession() {
		getReal3D.RpcManager.call("RestartSessionRPC");
	}

	[getReal3D.RPC]
	private void RestartSessionRPC() {
		Application.LoadLevel ("Main");
	}
	
}
