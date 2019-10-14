﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridEntity : MonoBehaviour {

    // character info
    public string name;

    // HP
    public int maxHP;
    public int currentHP;
    public GridEntity damageReceiver;
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

    // override turnDuration is as follows:
    //      turnDuration is decremented at the beginning of an entity's turn...
    //      When an effect needs to last through the enemy's turn, duration is 0
    //      When an effect needs to last through the next player's turn, duration is 1
    //      When an effect needs to last throughout the rest of the battle, duration is int.MaxValue
    public class Override {
        public int turnDuration = 0;
        public Func<bool> overrideFunction;

        public Override(int turnDuration, Func<bool> overrideFunction) {
            this.turnDuration = turnDuration;
            this.overrideFunction = overrideFunction;
        }
    }

    public List<Override> overrides = new List<Override>();

    // skills
    public List<string> skillNames;
    public List<Skill> skills;

    // reactions
    public List<Reaction> currentReactions = new List<Reaction>();

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
        damageReceiver = this;
        currentSkillUses = maxSkillUses;
        currentSP = maxSP;
        skills = skillNames.ToSkills();
        healthBar = GenerateHealthBar();
        UpdateHealthBar();
    }

    void Update() {
        UpdateHealthBar();
        if (currentHP <= 0) { Die(); }
    }

    public HealthBar GenerateHealthBar() {
        if (!isHostile) {
            var fullBar = Instantiate(
                healthBarPrefab,
                new Vector2(transform.position.x + healthBarXDelta, transform.position.y + healthBarYDelta),
                Quaternion.identity
            );
            return new HealthBar(fullBar, maxHP);
        }
        else { return null; }
    }

    private void UpdateHealthBar() {
        if (!isHostile) {
            healthBar.fullBar.transform.position = new Vector2(transform.position.x + healthBarXDelta, transform.position.y + healthBarYDelta);
            healthBar.Update(currentHP);
        }
    }

    private void DestroyHealthBar() {
        if (!isHostile) { Destroy(healthBar.fullBar); }
    }

    public void TakeDamage(int damage) {
        // damage should be a positive value
        damageReceiver.currentHP -= damage;
    }

    private void Die() {
        outOfHP = true;
        currentHP = 0;
        currentMoves = 0;
        currentSP = 0;
    }

    public void RemoveFromGrid() {
        this.transform.position = new Vector2(int.MaxValue, int.MaxValue);
        if (this.tile != null) {
            this.tile.occupier = null;
            this.tile = null;
        }
        DestroyHealthBar();
        Destroy(this.gameObject);
    }

    public void Move(int spaces) {
        currentMoves -= spaces;
        if (currentMoves <= 0) { outOfMoves = true; }
    }

    public void MakeAttack(GridEntity target) {
        currentAttacks--;
        var calculatedDamage = (damage + damageModify) * damageMult;
        var triggeredReactions = target.TriggerAttackReaction(this);

        void ResolveDefaultAttack() {
            target.TakeDamage(calculatedDamage);
            Debug.Log(String.Format("<color=blue>{0}</color> attacked <color=red>{1}</color> for <color=yellow>{2} damage</color>.",
                this.name,
                target.name,
                calculatedDamage
            ));
            Debug.Log(String.Format("<color=red>{0}</color> has <color=yellow>{1} health</color> remaining.",
                target.name,
                target.currentHP
            ));
        }

        if(triggeredReactions.Count() > 0) {
            triggeredReactions.ForEach(reaction => {
                var useDefaultAttackResolution = reaction.ResolveAttack(this, target);
                if (useDefaultAttackResolution) { ResolveDefaultAttack(); }
            });
        } else {
            ResolveDefaultAttack();
        }
        if (currentAttacks <= 0) { outOfAttacks = true; }
        if (currentAttackSkill != null) { UseSkill(currentAttackSkill); }
    }

    private List<AttackReaction> TriggerAttackReaction(GridEntity attacker) {
        return currentReactions
                .Where(reaction => reaction is AttackReaction && reaction.ReactsTo(attacker))
                .Select(reaction => (AttackReaction) reaction)
                .ToList();
    }

    public void UseSkill(Skill skill) {
        currentSP -= skill.cost;
        currentSkillUses--;
        if (currentSkillUses <= 0) { outOfSkillUses = true; }
        if (currentSP <= 0) { outOfSP = true; }
        Debug.Log("<color=blue>" + name + "</color> used <color=green>" + skill.GetType().Name + "</color>");
        Debug.Log("<color=blue>" + name + "</color> has <color=green>" + currentSP + "</color> SP remaining.");
    }

    public void ConsumeTurnResources() {
        currentMoves = 0;
        outOfMoves = true;
        currentAttacks = 0;
        outOfAttacks = true;
        currentSkillUses = 0;
        outOfSkillUses = true;
    }

    public void RefreshTurnResources() {
        currentMoves = maxMoves;
        outOfMoves = false;
        currentAttacks = maxAttacks;
        outOfAttacks = false;
        currentSkillUses = maxSkillUses;
        outOfSkillUses = false;
        currentAttackSkill = null;

        currentReactions.ForEach(reaction => reaction.turnDuration--);
        currentReactions = currentReactions.Where(reaction => reaction.turnDuration >= 0).ToList();

        overrides.ForEach(x => {
            x.turnDuration--;
            if (x.turnDuration < 0) { x.overrideFunction(); }
        });
        overrides = overrides.Where(x => x.turnDuration >= 0).ToList();
    }

    public void RefreshEncounterResources() {
        RefreshTurnResources();
        currentHP = maxHP;
        outOfHP = false;
    }
}
