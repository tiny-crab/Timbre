using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class GridEntity : MonoBehaviour {

    // character info
    public string entityName;
    public string entitySubname;

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

    // teleportation
    public int maxTeleports = 1;
    public int currentTeleports;
    public bool outOfTeleports = false;

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

    // AI
    public List<string> behaviorNames;
    public List<Behavior> behaviors = new List<Behavior>();
    public Behavior lastSelectedBehavior;

    // Fear
    public int baseFearValue;
    public int totalFearValue;
    public List<string> fearNames;
    public List<Fear> fears = new List<Fear>();
    public int fearThreshold;
    public List<string> afraidBehaviorNames;
    public List<Behavior> afraidBehaviors = new List<Behavior>();
    public GameObject fearIcon;

    //Threas
    public List<Ethread> equippedThreads = new List<Ethread>();

    // TODO UP: this coloring should be determined on a UI basis, not on an entity-level basis
    public Color moveRangeColor;
    public Color attackRangeColor;

    public bool isHostile;
    // if an entity is friendly, it will not attack the party but it is still controlled by AI
    public bool isFriendly;
    public bool isAllied;

    public Tile tile;

    public GameObject corpse;

    [SerializeField]
    private GameObject healthBarPrefab;
    private float healthBarXDelta = -0.25f;
    private float healthBarYDelta = 0.5f;
    private HealthBar healthBar;

    public Sequence turnAnimSequence;

    void Start () {
        currentMoves = maxMoves;
        currentAttacks = maxAttacks;
        currentHP = maxHP;
        damageReceiver = this;
        currentSkillUses = maxSkillUses;
        currentSP = maxSP;
        skills = skillNames.ToSkills();
        behaviors = behaviorNames.ToBehaviors(this);
        fears = fearNames.ToFears();
        afraidBehaviors = afraidBehaviorNames.ToBehaviors(this);
        healthBar = GenerateHealthBar();
        UpdateHealthBar();
        fearIcon = gameObject.transform.Find("FearIcon").gameObject;
        fearIcon.SetActive(false);
        turnAnimSequence = DOTween.Sequence();

        equippedThreads.ForEach(thread => thread.effect.ApplyEffect(this));
    }

    void Update() {
        UpdateHealthBar();
        if (currentHP <= 0 && !outOfHP) { Die(); }
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
        if (damageReceiver.currentHP <= 0) {
            damageReceiver.Die();
        }
    }

    private void Die() {
        outOfHP = true;
        currentHP = 0;
        currentMoves = 0;
        currentSP = 0;
        if (isHostile) {
            this.GetComponent<Animator>().enabled = false;
            this.GetComponent<SpriteRenderer>().sprite = corpse.GetComponent<SpriteRenderer>().sprite;
        }
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

    public void Move(List<Tile> path) {
        currentMoves -= path.Count;
        if (currentMoves <= 0) { outOfMoves = true; }
        var moveAnim = DOTween.Sequence();
        path.ForEach(tile => {
            moveAnim.Append(this.transform.DOMove(tile.transform.position, .1f));
        });
        turnAnimSequence.Prepend(moveAnim);
    }

    public void MakeAttack(GridEntity target) {
        currentAttacks--;
        var calculatedDamage = (damage + damageModify) * damageMult;
        var triggeredReactions = target.TriggerAttackReaction(this);

        void ResolveDefaultAttack() {
            target.TakeDamage(calculatedDamage);
            Debug.Log(String.Format("<color=blue>{0}</color> attacked <color=red>{1}</color> for <color=yellow>{2} damage</color>.",
                this.entityName,
                target.entityName,
                calculatedDamage
            ));
            Debug.Log(String.Format("<color=red>{0}</color> has <color=yellow>{1} health</color> remaining.",
                target.entityName,
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

        var attackAnim = DOTween.Sequence();
        attackAnim.Append(transform.DOMove(target.transform.position, .2f));
        // this uses the tile.transform, because the actual entity's position has not been updated at the time of this reference
        attackAnim.Append(transform.DOMove(this.tile.transform.position, .2f));
        turnAnimSequence.Append(attackAnim);

        DOTween.Sequence()
            .PrependInterval(attackAnim.Duration())
            .Append(target.transform.DOShakePosition(duration: .3f, strength: (calculatedDamage * .25f)));
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
        Debug.Log("<color=blue>" + entityName + "</color> used <color=green>" + skill.GetType().Name + "</color>");
        Debug.Log("<color=blue>" + entityName + "</color> has <color=green>" + currentSP + "</color> SP remaining.");
    }

    public void UseTeleport() {
        currentTeleports--;
        if (currentTeleports <= 0) { outOfTeleports = true; }
        Debug.Log("<color=blue>" + entityName + "</color> <color=green>teleported.</color>");
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

        totalFearValue = fears.Select(fear => fear.CalculateFear(this)).Sum() + baseFearValue;

        // once an enemy becomes afraid, they will stay afraid
        if (behaviors.Count > 0 && totalFearValue >= fearThreshold) {
            behaviors = afraidBehaviors;
            fearIcon.SetActive(true);
        }
    }

    public void RefreshEncounterResources() {
        RefreshTurnResources();
        currentHP = maxHP;
        outOfHP = false;
        currentTeleports = maxTeleports;
    }
}
