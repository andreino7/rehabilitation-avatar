using UnityEngine;
using System.Collections;

public class VirtualObject : MonoBehaviour {

	public Material defaultMaterial, triggerMaterial;
	public GameObject animationVfx;

	private float animationTime = 1.4f;
	private float scaleMultiplier = 3f;
	private Vector3 originalScale;
	private bool isCaught;

	void Start () {
		originalScale = transform.localScale;
		GetComponent<Renderer> ().material = defaultMaterial;
	}

	void OnTriggerEnter(Collider other) {
		Debug.Log ("Object reached!!");
		Invoke ("ObjectCaught", 2f);
		GetComponent<Renderer> ().material = triggerMaterial;
	}

	void OnTriggerExit(Collider other) {
		if(!isCaught) GetComponent<Renderer> ().material = defaultMaterial;
		CancelInvoke ();	
	}

	void ObjectCaught() {
		Debug.Log ("Object caught!!");
		GetComponent<AudioSource> ().Play ();
		StartCoroutine (ObjectCaughtCoroutine());
		isCaught = true;
		GameObject.Instantiate (animationVfx, transform.position, Quaternion.identity);
	}

	IEnumerator ObjectCaughtCoroutine() {
		float startTime = Time.time;
		while(Time.time < startTime + animationTime) {
			transform.localScale = new Vector3(
				Mathf.Lerp(originalScale.x, scaleMultiplier * originalScale.x, (Time.time - startTime) / animationTime),
				Mathf.Lerp(originalScale.y, scaleMultiplier * originalScale.y, (Time.time - startTime) / animationTime),
				Mathf.Lerp(originalScale.z, scaleMultiplier * originalScale.z, (Time.time - startTime) / animationTime));
				yield return null;
		}
		startTime = Time.time; 
		while(Time.time < startTime + animationTime / 2f) {
			transform.localScale = new Vector3(
				Mathf.Lerp(scaleMultiplier * originalScale.x, 0f, (Time.time - startTime) / (animationTime / 2f)),
				Mathf.Lerp(scaleMultiplier * originalScale.y, 0f, (Time.time - startTime) / (animationTime / 2f)),
				Mathf.Lerp(scaleMultiplier * originalScale.z, 0f, (Time.time - startTime) / (animationTime / 2f)));
			yield return null;
		}
		Destroy (gameObject);
	}
}
