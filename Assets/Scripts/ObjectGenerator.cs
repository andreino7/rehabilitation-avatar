using UnityEngine;
using System.Collections;

public class ObjectGenerator : MonoBehaviour {

	public GameObject objectPrefab;

	public int numberOfObjects;
	public float verticalBounds, horizontalBounds;
	private static ObjectGenerator instance;

	private ObjectGenerator () {
	}

	void Awake () {
		instance = this;
	}

	public static ObjectGenerator GetInstance () {
		return instance;
	}

	// Use this for initialization
	void Start () {
	
	}

	public void CreateNewObject () {
		Vector3 newPosition = new Vector3 (Random.Range(horizontalBounds, horizontalBounds*2), Random.Range(verticalBounds, verticalBounds*2), transform.position.z);
		Quaternion newQuaternion = Quaternion.Euler (Random.Range (0f, 360f), Random.Range (0.0f, 360f), Random.Range (0.0f, 360f));   
		Instantiate (objectPrefab, newPosition, newQuaternion);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
