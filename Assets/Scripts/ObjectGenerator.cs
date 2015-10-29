using UnityEngine;
using System.Collections;

public class ObjectGenerator : MonoBehaviour {

	public GameObject objectPrefab;

	public int numberOfObjects = 10;
	public float yOffset = 1f, verticalBounds = 1f, horizontalBounds = 1f;
	private static ObjectGenerator instance;
	private int currentObject = 0;

	private ObjectGenerator () {}

	void Awake () {
		instance = this;
	}

	public static ObjectGenerator GetInstance () {
		return instance;
	}

	// Use this for initialization
	void Start () {
		CreateNewObject ();
	}

	public void CreateNewObject () {
		currentObject++;
		Vector3 newPosition = new Vector3 (Random.Range(-horizontalBounds, horizontalBounds), yOffset + Random.Range(-verticalBounds, verticalBounds), transform.position.z);
		Quaternion newQuaternion = Quaternion.Euler (Random.Range (0f, 360f), Random.Range (0.0f, 360f), Random.Range (0.0f, 360f));   
		newQuaternion = Quaternion.identity;
		Instantiate (objectPrefab, newPosition, newQuaternion);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
