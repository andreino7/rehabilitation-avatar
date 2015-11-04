using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ObjectGenerator : getReal3D.MonoBehaviourWithRpc {

	public GameObject objectPrefab;

	public int numberOfObjects = 10;
	public float yOffset = 1f, verticalBounds = 1f, horizontalBounds = 1f;

	public Text labelLeft, labelRight; 
	public GameObject sessionCompleteAnimation;

	private static ObjectGenerator instance;
	private int currentObject = 0;
	private float xAvatarSize = 0.3f;

	private float elapsedTime = 0f;

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
			
			if (currentObject+1 == numberOfObjects) {
				Invoke("EndSession", 1f);
				return;
			}
			Vector3 newPosition = new Vector3 (Random.Range(-horizontalBounds, horizontalBounds), yOffset + Random.Range(-verticalBounds, verticalBounds), transform.position.z + 0.1f);
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
		currentObject++;
		elapsedTime = Time.time;
		labelLeft.text = "Object #" + currentObject;
		Instantiate (objectPrefab, newPosition, newQuaternion);
	}

	void Update() {
		getReal3D.RpcManager.call("UpdateTime");
		UpdateTime();
	}

	private void UpdateTime() {
		labelRight.text = "Time: " + (Time.time-elapsedTime);
	}

	private void EndSession() {
		PlayerPrefs.SetFloat ("TotalTime", elapsedTime);
		GetComponent<AudioSource>().Play();
		GameObject vfx = (GameObject) GameObject.Instantiate (sessionCompleteAnimation, transform.position, Quaternion.identity);
		Invoke ("ChangeScene",3f);
	}

	private void ChangeScene(){
		getReal3D.RpcManager.call("ChangeSceneRPC");
	}

	[getReal3D.RPC]
	private void ChangeSceneRPC() {
		Application.LoadLevel ("Results");
	}
}
