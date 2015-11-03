using UnityEngine;
using System.Collections;

public class ObjectGenerator : MonoBehaviour {

	public GameObject objectPrefab;

	public int numberOfObjects = 10;
	public float yOffset = 1f, verticalBounds = 1f, horizontalBounds = 1f;
	private static ObjectGenerator instance;
	private int currentObject = 0;
	private float xAvatarSize = 0.5f;

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

	public void CreateNewObject() {
		if(getReal3D.Cluster.isMaster) {
			currentObject++;
			if (currentObject == numberOfObjects) {
				EndSession();
				return;
			}
			Vector3 newPosition = new Vector3 (Random.Range(-horizontalBounds, horizontalBounds), yOffset + Random.Range(-verticalBounds, verticalBounds), transform.position.z + 0.5f);
			if(Mathf.Abs(newPosition.x) < xAvatarSize) {
				if (newPosition.x > 0)
					newPosition.x = newPosition.x + xAvatarSize;
				else if(newPosition.x < 0)
					newPosition.x = newPosition.x - xAvatarSize;
			}
			Quaternion newQuaternion = Quaternion.Euler (Random.Range (0f, 360f), Random.Range (0.0f, 360f), Random.Range (0.0f, 360f));
			getReal3D.RpcManager.call("CreateNewObjectRPC", newPosition, newQuaternion);
		}
	}

	[getReal3D.RPC]
	private void CreateNewObjectRPC (Vector3 newPosition, Quaternion newQuaternion) {
		Instantiate (objectPrefab, newPosition, newQuaternion);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void EndSession() {
		Invoke ("ChangeScene",3f);
	}

	private void ChangeScene() {
		Application.LoadLevel ("Results");
	}
}
