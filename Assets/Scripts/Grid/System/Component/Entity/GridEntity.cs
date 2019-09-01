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

    public int maxSkillUses;
    public int currentSkillUses;

    // skills
    // public List<Skill> skills = {};
    // to test multi-skill selection feature, will currently just be strings
    public List<string> skills;

    // TODO UP: this coloring should be determined on a UI basis, not on an entity-level basis
    public Color moveRangeColor;
    public Color attackRangeColor;

    public bool isHostile;
    // if an entity is friendly, it will not attack the party but it is still controlled by AI
    public bool isFriendly;
    public bool isAllied;

    public Tile tile;

    [SerializeField]
    private GameObject healthBarPrefab;
    private float healthBarXDelta = -0.25f;
    private float healthBarYDelta = 0.5f;
    private HealthBar healthBar;

    void Start () {
        currentMoves = maxMoves;
        currentAttacks = maxAttacks;
        currentHP = maxHP;
        currentSkillUses = maxSkillUses;
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

    public void TakeDamage(int damage) {
        // damage should be a positive value
        healthBar.TakeDamage(damage);
        currentHP -= damage;
        if (currentHP <= 0) { Die(); }
    }

    private void Die() {
        outOfHP = true;
        transform.position = new Vector2(int.MaxValue, int.MaxValue);
    }

    public void Move(int spaces) {
        currentMoves -= spaces;
        if (currentMoves <= 0) { outOfMoves = true; }
    }

    public void MakeAttack(GridEntity target) {
        currentAttacks -= 1;
        target.TakeDamage((damage + damageModify) * damageMult);
        if (currentAttacks <= 0) { outOfAttacks = true; }
    }

    public void RefreshTurnResources() {
        currentMoves = maxMoves;
        outOfMoves = false;
        currentAttacks = maxAttacks;
        outOfAttacks = false;
    }
}
