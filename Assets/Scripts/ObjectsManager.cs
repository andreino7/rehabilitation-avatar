using UnityEngine;
using System.Collections;

public class ObjectsManager : getReal3D.MonoBehaviourWithRpc {

	protected int currentObject = 0;
	protected int numberOfObjects;

	protected float xAvatarSize = 0.3f;

	protected Object objectPrefab;

	void Start() {
		objectPrefab = Resources.Load ("BasicObject");
	}

	protected void NextObject() {
		if (getReal3D.Cluster.isMaster) {
			
			if (currentObject == numberOfObjects) {
				Invoke("EndSession", 1f);
				return;
			}

			SessionManager.GetInstance ().StopTimer();
			Vector3 newPosition = this.PositionNewObject();
			Quaternion newQuaternion = Quaternion.Euler (UnityEngine.Random.Range (0f, 360f), UnityEngine.Random.Range (0.0f, 360f), UnityEngine.Random.Range (0.0f, 360f));
			getReal3D.RpcManager.call("CreateNewObjectRPC", newPosition, newQuaternion);
		}
	}

	virtual protected Vector3 PositionNewObject ();

	protected void EndSession() {
		SessionManager.GetInstance().EndSession();
	}

	[getReal3D.RPC]
	private void CreateNewObjectRPC (Vector3 newPosition, Quaternion newQuaternion) {
		currentObject++;
		//elapsedTime = Time.time;
		//labelLeft.text = "Object #" + currentObject;
		Instantiate (objectPrefab, newPosition, newQuaternion);
	}

}
