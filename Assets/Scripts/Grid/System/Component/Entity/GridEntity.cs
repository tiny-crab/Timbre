using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEntity : MonoBehaviour {

	// HP
	public int maxHP;
	public int currentHP;
	public bool outOfHP = false;

	// move
	public int maxMoves;
	public int currentMoves;
	public bool outOfMoves = false;

	// attack
	public int maxAttacks;
	public int currentAttacks;
	public bool outOfAttacks = false;

	public int damage;
	public int range;
	public int damageMult;
	public int damageModify;

	// SP
	public int maxSP;
	public int currentSP;
	public bool outOfSP = false;

	// skills
	// public List<Skill> skills = {};

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
		currentMoves = maxMoves;
		currentHP = maxHP;
		healthBar = GenerateHealthBar();
		healthBar.Update();
	}

	void Update() {
		healthBar.fullBar.transform.position = new Vector2(transform.position.x + healthBarXDelta, transform.position.y + healthBarYDelta);
		if (outOfHP) { Destroy(this); }
	}

	public HealthBar GenerateHealthBar() {
		var fullBar = Instantiate(
			healthBarPrefab, 
			new Vector2(transform.position.x + healthBarXDelta, transform.position.y + healthBarYDelta), 
			Quaternion.identity
		);

		return new HealthBar(fullBar, maxHP);
	}

	public void ChangeHealth(int damage) {
		healthBar.ChangeHealth(damage);
		maxHP += damage;
		if (maxHP <= 0) {
			outOfHP = true;
			Die();
		}
	}

	private void Die() {
		transform.position = new Vector2(int.MaxValue, int.MaxValue);
	}

	public void Move(int spaces) {
		currentMoves -= spaces;
	}

	public void RefreshTurnResources() {
		currentMoves = maxMoves;
	}
}
