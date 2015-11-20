using UnityEngine;
using System.Collections;

public class ProgressiveDistanceGenerator : ObjectsManager {
	public enum Direction{LEFT, RIGHT, UPLEFT, UPRIGHT};
	protected float yOffset = 1f, verticalBounds = 1f, horizontalBounds = 1f;
	protected Direction direction;
	private float currentX, currentY;
	
	public ProgressiveDistanceGenerator(Direction direction) {
		numberOfObjects = 10;
		this.direction = direction;
		if (direction == Direction.LEFT || direction == Direction.UPLEFT) {
			currentX = -xAvatarSize;
		} else {
			currentX = xAvatarSize;
		}
		currentY = 0;
	}
	
	Vector3 PositionNewObject() {
		Vector3 newPosition = new Vector3();
		float z = SessionManager.GetInstance ().GetPatientPosition ().z + 0.1f;
		switch (direction) {
		case Direction.LEFT:
			newPosition = new Vector3 (--currentX,currentY,z);
			break;
		case Direction.RIGHT:
			newPosition = new Vector3 (++currentX,currentY,z);
			break;
		case Direction.UPLEFT:
			newPosition = new Vector3 (--currentX,++currentY,z);
			break;
		case Direction.UPRIGHT:
			newPosition = new Vector3 (++currentX,++currentY,z);
			break;
		}
		return newPosition;
	}
}
