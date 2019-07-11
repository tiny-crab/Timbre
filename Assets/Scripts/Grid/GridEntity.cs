using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEntity : MonoBehaviour {

	public Color selectedColor;
	public int health;
	public int moveRange;
	public int attackRange;

	public int tileX;
	public int tileY;
	
	[SerializeField]
	private GameObject healthBarPrefab;
	private float healthBarXDelta = -0.25f;
	private float healthBarYDelta = 0.5f;
	private HealthBar healthBar;

	void Start () {
		healthBar = GenerateHealthBar();
	}

	void Update() {
		healthBar.Update();
		healthBar.fullBar.transform.position = new Vector2(transform.position.x + healthBarXDelta, transform.position.y + healthBarYDelta);
	}

	public HealthBar GenerateHealthBar() {
		var fullBar = Instantiate(
			healthBarPrefab, 
			new Vector2(transform.position.x + healthBarXDelta, transform.position.y + healthBarYDelta), 
			Quaternion.identity
		);

		return new HealthBar(fullBar, health);
	}
}
