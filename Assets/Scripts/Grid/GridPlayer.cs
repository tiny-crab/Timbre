using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPlayer : GridEntity {

	private HealthBar healthBar;

	void Start () {
		selectedColor = Color.white;
		healthBar = GenerateHealthBar();
	}

	void Update() {
		healthBar.Update();
		healthBar.fullBar.transform.position = new Vector2(transform.position.x + healthBarXDelta, transform.position.y + healthBarYDelta);
	}
}
