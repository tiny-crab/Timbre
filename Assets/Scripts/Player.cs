using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : ControllerInteractable {

    public float speed = 5f;
    private BoxCollider2D boxCollider;
    private ContactFilter2D contactFilter = new ContactFilter2D().NoFilter();
    private GridSystem grid;

    private List<GameObject> partyPrefabs;

    private bool waiting = false;

    // KEY INTERACTIONS
    private List<KeyCode> UP = new List<KeyCode>() {
        KeyCode.W, KeyCode.UpArrow
    };
    private List<KeyCode> DOWN = new List<KeyCode>() {
        KeyCode.S, KeyCode.DownArrow
    };
    private List<KeyCode> LEFT = new List<KeyCode>() {
        KeyCode.A, KeyCode.LeftArrow
    };
    private List<KeyCode> RIGHT = new List<KeyCode>() {
        KeyCode.D, KeyCode.RightArrow
    };
    private List<KeyCode> INTERACT = new List<KeyCode>() {
        KeyCode.F
    };

    private List<KeyCode> ACTIVATE_GRID = new List<KeyCode>() {
        KeyCode.N
    };
    private List<KeyCode> DEACTIVATE_GRID = new List<KeyCode>() {
        KeyCode.M
    };
    private bool keyPressed(List<KeyCode> input) { return input.Any(key => Input.GetKey(key)); }

    void Awake () {
        boxCollider = GetComponent<BoxCollider2D>();
        grid = (GridSystem) GameObject.Find("GridSystem").GetComponent<GridSystem>();

        partyPrefabs = new List<GameObject>() {
            Resources.Load<GameObject>("Prefabs/Grid/AllyClasses/GridPlayer"),
            // Resources.Load<GameObject>("Prefabs/Grid/AllyClasses/Knight"),
            Resources.Load<GameObject>("Prefabs/Grid/AllyClasses/Dog"),
            // Resources.Load<GameObject>("Prefabs/Grid/AllyClasses/Woodsman")
        };
    }

    void Update () {
        var allEnemiesDefeated = grid.factions
                                    .Where(faction => faction.isHostileFaction)
                                    .All(faction => {
                                        return faction.entities.All(entity => entity.outOfHP || entity.currentHP <= 0);
                                    });
        if ((allEnemiesDefeated || keyPressed(DEACTIVATE_GRID)) && grid.activated) { DeactivateGrid(); }
        if (waiting) { return; }

        Vector3 previousPos = transform.position;
        Vector3 movePos = transform.position;

        // TODO: Use UniRX if the project gets bigger
        if (keyPressed(UP)) { movePos.y += speed * Time.deltaTime; }
        if (keyPressed(DOWN)) { movePos.y -= speed * Time.deltaTime; }
        if (keyPressed(LEFT)) { movePos.x -= speed * Time.deltaTime; }
        if (keyPressed(RIGHT)) { movePos.x += speed * Time.deltaTime; }

        if (keyPressed(ACTIVATE_GRID)) { ActivateGrid(); }

        transform.position = movePos;

        int numColliders = 10;
        Collider2D[] colliders = new Collider2D[numColliders];
        int colliderCount = boxCollider.OverlapCollider(contactFilter, colliders);

        // do this in both x and y axes, only one is working right now (can't "slide" along walls)
        if (colliderCount > 0) {
            for (int i = 0; i < colliderCount; i++) {
                // this might be a poor and non-performant solution
                if (colliders[i].gameObject.GetComponent<Rigidbody2D>() != null) {
                    transform.position = previousPos;
                }
            }
        }

        RaycastHit2D[] interactables = new RaycastHit2D[10];

        int interactColliderCount = Physics2D.CircleCast(
            origin: transform.position,
            radius: 0.7f,
            direction: new Vector2(0, 0),
            contactFilter: contactFilter,
            results: interactables
        );

        if (interactColliderCount > 0) {
            var results = new List<RaycastHit2D>(interactables)
                .Where(entity => entity.collider != null)
                .Where(entity => entity.collider.gameObject.name != "Player");
            if (keyPressed(INTERACT)) { results.First().collider.gameObject.BroadcastMessage("PlayerInteract"); }
        }

        var encounterColliders = Physics2D.BoxCastAll(
            origin: transform.position,
            size: new Vector2(0.5f, 0.5f),
            angle: 0,
            direction: Vector2.zero
        ).Select(hit => hit.collider).ToList();

        encounterColliders.ForEach(collider => {
            if (collider.gameObject.tag == "Encounter") {
                var encounterObj = collider.GetComponent<Encounter>();
                if (encounterObj != null) {
                    encounterObj.Trigger();
                    ActivateGrid(encounterObj.enemyPrefabs);
                }
                else { ActivateGrid(); }
                collider.gameObject.tag = "TriggeredEncounter";
            }
        });
    }

    private void ActivateGrid(List<KeyValuePair<GameObject, Vector2>> encounteredEnemies = null) {
        this.GetComponent<SpriteRenderer>().enabled = false;
        grid.ActivateGrid(
            this.transform.position,
            partyPrefabs,
            encounteredEnemies ?? GenerateRandomEnemies()
        );
        waiting = true;
        return;
    }

    private List<KeyValuePair<GameObject, Vector2>> GenerateRandomEnemies() {
        var pair = new KeyValuePair<GameObject, Vector2>(
            Resources.Load<GameObject>("Prefabs/Grid/EnemyClasses/Goblin"),
            new Vector2(int.MaxValue, int.MaxValue)
        );
        return new List<KeyValuePair<GameObject, Vector2>>() {
            pair,
            pair,
            pair,
            pair,
            pair,
            pair,
        };
    }

    private void DeactivateGrid() {
        this.transform.position = grid.gridPlayer.transform.position;
        this.GetComponent<SpriteRenderer>().enabled = true;
        grid.DeactivateGrid();
        waiting = false;
    }
}
