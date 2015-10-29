using UnityEngine;
using System.Collections;

public class SelfDestroy : MonoBehaviour {

	public float time = 3f;

	// Use this for initialization
	void Start () {
		Invoke ("DestroyObject", time);
	}
	
	// Update is called once per frame
	void DestroyObject() {
		Destroy (gameObject);
	}
}
