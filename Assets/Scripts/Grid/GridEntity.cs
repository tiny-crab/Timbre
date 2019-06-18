using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEntity : MonoBehaviour {
	public Color selectedColor = Color.white;
	public int health = 3;

	public GameObject healthBarPrefab;
	protected float healthBarXDelta = -0.25f;
	protected float healthBarYDelta = 0.5f;

	protected HealthBar GenerateHealthBar() {
		var fullBar = Instantiate(
			healthBarPrefab, 
			new Vector2(transform.position.x + healthBarXDelta, transform.position.y + healthBarYDelta), 
			Quaternion.identity
		);

		return new HealthBar(fullBar, health);
	}
}
