using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEntity : MonoBehaviour {

    // character info
    public string name;

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
    public AttackSkill currentAttackSkill;

    public int maxSkillUses;
    public int currentSkillUses;
    public bool outOfSkillUses = false;

    // skills
    // public List<Skill> skills = {};
    public List<string> skillNames;
    public List<Skill> skills;

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
        currentSP = maxSP;
        skills = skillNames.ToSkills();
        healthBar = GenerateHealthBar();
        healthBar.Update(currentHP);
    }

    void Update() {
        healthBar.fullBar.transform.position = new Vector2(transform.position.x + healthBarXDelta, transform.position.y + healthBarYDelta);
        healthBar.Update(currentHP);
        if (currentHP <= 0) { Die(); }
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
    }

    private void Die() {
        outOfHP = true;
        currentHP = 0;
        currentMoves = 0;
        currentSP = 0;
    }

    public void Move(int spaces) {
        currentMoves -= spaces;
        if (currentMoves <= 0) { outOfMoves = true; }
    }

    public void MakeAttack(GridEntity target) {
        currentAttacks--;
        target.TakeDamage((damage + damageModify) * damageMult);
        if (currentAttacks <= 0) { outOfAttacks = true; }
        if (currentAttackSkill != null) { UseSkill(currentAttackSkill); }
    }

    public void UseSkill(Skill skill) {
        currentSP -= skill.cost;
        currentSkillUses--;
        if (currentSkillUses <= 0) { outOfSkillUses = true; }
        if (currentSP <= 0) { outOfSP = true; }
        Debug.Log("<color=blue>" + name + "</color> used <color=green>" + skill.GetType().Name + "</color>");
        Debug.Log("<color=blue>" + name + "</color> has <color=green>" + currentSP + "</color> SP remaining.");
    }

    public void RefreshTurnResources() {
        currentMoves = maxMoves;
        outOfMoves = false;
        currentAttacks = maxAttacks;
        outOfAttacks = false;
        currentSkillUses = maxSkillUses;
        outOfSkillUses = false;
        currentAttackSkill = null;
    }

    public void RefreshEncounterResources() {
        RefreshTurnResources();
        currentHP = maxHP;
        outOfHP = false;
    }
}
