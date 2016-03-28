using UnityEngine;
using System.Collections;

public class Transparency : MonoBehaviour {

	public float transparency = 1f;

	Renderer renderer;

	// Use this for initialization
	void Start () {
		renderer = GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void LateUpdate () {
		Color color = renderer.material.color;
		renderer.material.color = new Color(color.r, color.g, color.b, transparency);
	}
}
