﻿using UnityEngine;
using System.Collections;

public class RandomGenerator : ObjectsManager {

	protected float yOffset = 1f, verticalBounds = 1f, horizontalBounds = 1f;

	public RandomGenerator() {
		numberOfObjects = 10;
	}

	Vector3 PositionNewObject() {
		Vector3 newPosition = new Vector3 (UnityEngine.Random.Range(-horizontalBounds, horizontalBounds), yOffset + UnityEngine.Random.Range(-verticalBounds, verticalBounds), SessionManager.GetInstance ().GetPatientPosition().z + 0.1f);
		if(Mathf.Abs(newPosition.x) < xAvatarSize) {
			if (newPosition.x > 0)
				newPosition.x = newPosition.x + xAvatarSize;
			else if(newPosition.x < 0)
				newPosition.x = newPosition.x - xAvatarSize;
		}
		return newPosition;
	}
}