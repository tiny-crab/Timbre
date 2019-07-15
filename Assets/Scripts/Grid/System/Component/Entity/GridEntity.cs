using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEntity : MonoBehaviour {

	public int health;
	public int remainingHealth;
	public bool dead = false;

	public int moveRange;
	public int remainingMoves;

	public int attackRange;
	public int attackDamage;

	public Color moveRangeColor;
	public Color attackRangeColor;

	public bool isHostile;
	public bool isFriendly;
	public bool isAllied;

	public int tileX;
	public int tileY;
	
	[SerializeField]
	private GameObject healthBarPrefab;
	private float healthBarXDelta = -0.25f;
	private float healthBarYDelta = 0.5f;
	private HealthBar healthBar;

	void Start () {
		healthBar = GenerateHealthBar();
		healthBar.Update();
	}

	void Update() {
		healthBar.fullBar.transform.position = new Vector2(transform.position.x + healthBarXDelta, transform.position.y + healthBarYDelta);
		if (dead) { Destroy(this); }
	}

	public HealthBar GenerateHealthBar() {
		var fullBar = Instantiate(
			healthBarPrefab, 
			new Vector2(transform.position.x + healthBarXDelta, transform.position.y + healthBarYDelta), 
			Quaternion.identity
		);

		return new HealthBar(fullBar, health);
	}

	public void ChangeHealth(int damage) {
		healthBar.ChangeHealth(damage);
		health += damage;
		if (health <= 0) {
			dead = true;
			Die();
		}
	}

	private void Die() {
		transform.position = new Vector2(int.MaxValue, int.MaxValue);
	}

	public void Move(int spaces) {
		remainingMoves -= spaces;
		if(remainingMoves < 0) {
			
		}
	}
}
