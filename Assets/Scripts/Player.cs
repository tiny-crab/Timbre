using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : ControllerInteractable {

    public float speed = 5f;
    private GridSystem grid;

    private Dictionary<string, int> inventory = new Dictionary<string, int>();

    private List<GameObject> partyPrefabs;

    public GameObject ethreadMenu;

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
    private bool keyDown(List<KeyCode> input) { return input.Any(key => Input.GetKeyDown(key)); }

    void Awake () {
        grid = (GridSystem) GameObject.Find("GridSystem").GetComponent<GridSystem>();

        partyPrefabs = new List<GameObject>() {
            Resources.Load<GameObject>("Prefabs/Grid/AllyClasses/GridPlayer"),
            Resources.Load<GameObject>("Prefabs/Grid/AllyClasses/Knight"),
            Resources.Load<GameObject>("Prefabs/Grid/AllyClasses/Dog"),
            Resources.Load<GameObject>("Prefabs/Grid/AllyClasses/Woodsman"),
        };

        ethreadMenu = GameObject.Find("EthreadMenu");
    }

    void Start () {
        ethreadMenu.SetActive(false);
    }

    void Update () {
        // -----
        // Grid CAN BE ENABLED while the following code is executed
        // -----

        // commented block will be used for ally-follow in overworld

        // var mask = LayerMask.GetMask("HiddenTile");
        // var tileColliderNames = Physics2D.RaycastAll(
        //     origin: this.transform.position,
        //     direction: Vector2.zero,
        //     distance: 0.0f,
        //     layerMask: mask
        // ).ToList().Select(tileObj => tileObj.collider.gameObject.name);

        // here i can add a "continue" dialog input that will be enabled in both overworld and grid combat

        var allEnemiesDefeated = grid.factions
                                    .Where(faction => faction.isHostileFaction)
                                    .All(faction => {
                                        return faction.entities.All(entity => entity.outOfHP || entity.currentHP <= 0);
                                    });
        if ((allEnemiesDefeated || keyPressed(DEACTIVATE_GRID)) && grid.activated) { DeactivateGrid(); }
        if (waiting) { return; }


        // -----
        // Grid is DISABLED beyond this point.
        // -----


        // take note - this is the first place where the term "Ethread" is coming up
        // a truncation of the codename "Ethereal Thread" to represent the lifeforce items of monsters
        // and to reduce confusion with "thread" terms in the code
        // I will most likely regret this term and hate having it everywhere in my code because it looks ugly
        if (Input.GetKeyDown(KeyCode.Tab)) { ToggleEthreadMenu(); }

        Vector3 movePos = transform.position;

        // TODO: Use UniRX if the project gets bigger
        if (keyPressed(UP)) { movePos.y += speed * Time.deltaTime; }
        if (keyPressed(DOWN)) { movePos.y -= speed * Time.deltaTime; }
        if (keyPressed(LEFT)) { movePos.x -= speed * Time.deltaTime; }
        if (keyPressed(RIGHT)) { movePos.x += speed * Time.deltaTime; }

        if (keyPressed(ACTIVATE_GRID)) { ActivateGrid(); }

        var colliders = Physics2D.BoxCastAll(
            origin: movePos,
            size: new Vector2(0.5f, 0.5f),
            angle: 0,
            direction: Vector2.zero
        ).Select(hit => hit.collider).ToList();

        var physicsColliders = colliders.Where(collider => collider.gameObject.GetComponent<Rigidbody2D>() != null);
        if (physicsColliders.Count() > 0) {

                void revertCollisionFor(Vector2 offset, bool vertical, System.Action revertPosition) {
                        var collisions = Physics2D.BoxCastAll(
                        origin: offset,
                        size: vertical ? new Vector2(0.1f, 0.5f) : new Vector2(0.5f, 0.1f),
                        angle: 0,
                        direction: Vector2.zero
                    )
                    .Select(hit => hit.collider)
                    .Where(collider => collider.gameObject.GetComponent<Rigidbody2D>() != null)
                    .ToList();
                    if (collisions.Count() > 0) { revertPosition(); }
                }

                // colliding to north
                revertCollisionFor(
                    offset: new Vector2(movePos.x, movePos.y + .5f),
                    vertical: true,
                    revertPosition: () => { movePos.y -= speed * Time.deltaTime; }
                );
                // colliding to south
                revertCollisionFor(
                    offset: new Vector2(movePos.x, movePos.y - .5f),
                    vertical: true,
                    revertPosition: () => { movePos.y += speed * Time.deltaTime; }
                );

                // colliding to east
                revertCollisionFor(
                    offset: new Vector2(movePos.x - .5f, movePos.y),
                    vertical: false,
                    revertPosition: () => { movePos.x += speed * Time.deltaTime; }
                );
                // colliding to west
                revertCollisionFor(
                    offset: new Vector2(movePos.x + .5f, movePos.y),
                    vertical: false,
                    revertPosition: () => { movePos.x -= speed * Time.deltaTime; }
                );
        }

        transform.position = movePos;

        var encounterColliders = colliders.Where(collider => collider.gameObject.tag == "Encounter").ToList();
        encounterColliders.ForEach(collider => {
            var encounterObj = collider.GetComponent<Encounter>();
                if (encounterObj != null) {
                    encounterObj.Trigger();
                    ActivateGrid(encounterObj.enemyPrefabs);
                }
                else { ActivateGrid(); }
                collider.gameObject.tag = "TriggeredEncounter";
        });

        var pickupColliders = colliders.Where(collider => collider.gameObject.tag == "Pickup").ToList();
        pickupColliders.ForEach(collider => {
            var pickup = collider.gameObject.GetComponent<Pickup>();
            if (pickup.pickupType == "Ethread") {
                var matchingInventory = ethreadMenu.GetComponent<EthreadMenu>()
                    .threadButtonGroups
                    .Find(group => group.prefabName == pickup.pickupName);
                if (matchingInventory != null) {
                    matchingInventory.quantity++;
                }
                Destroy(collider.gameObject);
            }
        });

        var interactColliders = Physics2D.CircleCastAll(
            origin: transform.position,
            radius: 1f,
            direction: Vector2.zero
        )
        .Select(hit => hit.collider)
        .Where(collider => collider.gameObject.tag == "Interaction")
        .OrderBy(collider => Vector2.Distance(collider.transform.position, transform.position))
        .ToList();

        if (keyDown(INTERACT) && interactColliders.Count() > 0) {
            interactColliders.First().gameObject.BroadcastMessage("PlayerInteract");
        }
    }

    private void ActivateGrid(List<KeyValuePair<GameObject, Vector2>> encounteredEnemies = null) {
        this.GetComponent<SpriteRenderer>().enabled = false;
        grid.ActivateGrid(
            this.transform.position,
            partyPrefabs,
            encounteredEnemies ?? GenerateRandomEnemies()
        );
        ethreadMenu.SetActive(false);
        waiting = true;
        return;
    }

    private void ToggleEthreadMenu() {
        ethreadMenu.SetActive(!ethreadMenu.activeInHierarchy);
        ethreadMenu.GetComponent<EthreadMenu>().PopulateParty(partyPrefabs);
    }

    private List<KeyValuePair<GameObject, Vector2>> GenerateRandomEnemies() {
        var pair = new KeyValuePair<GameObject, Vector2>(
            // Resources.Load<GameObject>("Prefabs/Grid/EnemyClasses/ElkCult/ElkCultist"),
            Resources.Load<GameObject>("Prefabs/Grid/EnemyClasses/Goblin"),
            new Vector2(int.MaxValue, int.MaxValue)
        );
        return new List<KeyValuePair<GameObject, Vector2>>() {
            pair,
            pair,
            pair,
            // pair,
            // pair,
            // pair,
        };
    }

    private void DeactivateGrid() {
        this.transform.position = grid.gridPlayer.transform.position;
        this.GetComponent<SpriteRenderer>().enabled = true;
        grid.DeactivateGrid();
        waiting = false;
    }
}
