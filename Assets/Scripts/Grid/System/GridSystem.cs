using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GridSystem : MonoBehaviour {

    public bool activated = false;

    public new Camera camera;

    public int tileMapSize = 10;
    public GameObject[,] initTileMap;
    public GameObject tile;

    public GridEntity gridPlayer;
    public GridEntity lastSelectedAlly;

    // TODO SIDE: these feel like utility functions and should be moved
    public bool waiting = false;
    // time between turns
    public float turnGapTime = 0.5f;
    // time between AI steps
    public float aiStepTime = 0.25f;
    // just wait whenever you feel like it
    public float waitTime = 0.1f;

    public StateMachineComponent stateMachine = new StateMachineComponent();
    public TilemapComponent tilemap = new TilemapComponent();

    public Queue<Faction> factions = new Queue<Faction>();
    public Faction currentFaction;

    public List<KeyCode> skillKeys = new List<KeyCode>() { KeyCode.A, KeyCode.S, KeyCode.D };
    public Dictionary<KeyCode, int> keyToSkillIndex = new Dictionary<KeyCode, int>() {
        {KeyCode.A, 0},
        {KeyCode.S, 1},
        {KeyCode.D, 2}
    };

    public State currentState = new NoSelectionState();

    // TODO SIDE: Audio + UI should be in a different class
    public Dialog dialog;
    public AudioClip dialogNoise;

    public GameObject skillMenu;

    public GameObject redThreadPrefab;

    public void ActivateGrid (
        Vector2 playerLocation,
        List<GameObject> activeParty,
        List<KeyValuePair<GameObject, Vector2>> enemiesToSpawn
    ) {
        activated = true;
        waiting = false;
        GridUtils.FlattenGridTiles(tilemap.grid)
            .Where(tile => !tile.disabled)
            .Where(tile => {
                Vector3 screenPoint = camera.WorldToViewportPoint(tile.transform.position);
                var leftRightMargin = .2f;
                var topBottomMargin = .15f;
                return
                    screenPoint.z > 0 &&
                    screenPoint.x > 0 + leftRightMargin && screenPoint.x < 1 - leftRightMargin &&
                    screenPoint.y > 0 + topBottomMargin && screenPoint.y < 1 - topBottomMargin;
            }).ToList()
            .ForEach(tile => tile.gameObject.SetActive(true));

        // snap player into Grid
        var playerPrefab = activeParty.Find(obj => obj.name == "GridPlayer");
        var closestTile = tilemap.ClosestTile(playerLocation);
        gridPlayer = PutEntity(closestTile.x, closestTile.y, playerPrefab);

        // snap party into Grid
        var party = new List<GridEntity>() { gridPlayer };
        activeParty.Where(obj => obj.name != "GridPlayer").ToList().ForEach(entity => {
            var adjacentTile = GridUtils.GenerateTileSquare(tilemap.grid, 1, gridPlayer.tile)
                                    .Where(tile => tile.occupier == null)
                                    .First();
            party.Add(PutEntity(adjacentTile.x, adjacentTile.y, entity));
        });

        // put enemies into Grid
        var enemies = enemiesToSpawn.Select(enemyPair => {
            var enemyPrefab = enemyPair.Key;
            var location = enemyPair.Value;
            var tile = tilemap.ClosestTile(location);
            if (location == new Vector2(int.MaxValue, int.MaxValue)) {
                tile = GridUtils.GetRandomEnabledTile(tilemap.grid);
            }
            return PutEntity(tile.x, tile.y, enemyPrefab);
        }).ToList();

        var enemyFaction = new Faction("Enemy", false, enemies);
        var playerFaction = new Faction("Player", true, party);

        factions.Enqueue(playerFaction);
        factions.Enqueue(enemyFaction);
        currentFaction = this.factions.First();

        stateMachine.Start(this);
    }

    public void DeactivateGrid () {
        activated = false;
        waiting = true;
        GridUtils.FlattenGridTiles(tilemap.grid)
            .Where(tile => !tile.disabled).ToList()
            .ForEach(tile => tile.gameObject.SetActive(false));

        TilemapComponent.ClearAllHighlightsFromGrid(tilemap.grid);

        factions.ToList()
            .Where(faction => faction.isHostileFaction).ToList()
            .ForEach(faction => {
                faction.entities
                    .Where(entity => entity.outOfHP).ToList()
                    .ForEach(entity => {
                        GameObject onDeathPrefab = null;
                        if (entity.totalFearValue >= entity.fearThreshold) {
                            var rand = new System.Random();
                            if (rand.Next(2) == 0) {
                                onDeathPrefab = entity.corpse;
                            } else {
                                onDeathPrefab = redThreadPrefab;
                            }
                        } else {
                            onDeathPrefab = entity.corpse;
                        }
                        Instantiate(onDeathPrefab, entity.transform.position, Quaternion.identity);
                    });
            });

        factions.ToList().ForEach(faction => {
            faction.entities.ForEach(entity => entity.RemoveFromGrid());
        });
        factions = new Queue<Faction>();

        skillMenu.SetActive(false);
    }

    void Awake () {
        initTileMap = new GameObject[tileMapSize, tileMapSize];
        redThreadPrefab = Resources.Load<GameObject>("Prefabs/Overworld/Pickups/RedThreadPickup");
    }

    void Start () {
        CreateTilemapComponent();
        dialog = (Dialog) GameObject.Find("Dialog").GetComponent<Dialog>();
        skillMenu = GameObject.Find("SkillMenu");
        skillMenu.SetActive(false);
        DeactivateGrid();
    }

    void Update () {
        var mouseTile = GetTileUnderMouse();

        lastSelectedAlly = currentState.source ?? gridPlayer;
        UpdateSkillMenu();

        if (waiting) { return; }
        if (Input.GetKeyDown(KeyCode.Tab)) { ToggleSkillMenu(); }

        if (currentState is NoSelectionState) {
            if (mouseTile == null) { return; }
            else if (Input.GetMouseButtonDown(0) && mouseTile.occupier != null) {
                currentState = TransitionOnClick(currentState, mouseTile);
            }
            // this needs to be able to be triggered in any state
            else if (Input.GetKeyDown(KeyCode.G)) {
                currentState = TransitionOnEndTurn();
            }
        }

        else if (currentState is AllySelectedState) {
            if (mouseTile == null) { return; }
            else if (Input.GetMouseButtonDown(0)) {
                currentState = TransitionOnClick(currentState, mouseTile);
            }
            else if (InputUtils.GetKeyPressed(skillKeys) != null) {
                skillKeys.ForEach(keyPressed => {
                    if (Input.GetKeyDown(keyPressed)) {
                        currentState = TransitionOnSkillKeyPress(currentState, keyPressed);
                    }
                });
                StartCoroutine(WaitAMoment(waitTime, "Skill Activation"));
            }
            else if (Input.GetKeyDown(KeyCode.T)) {
                currentState = TransitionOnTeleportKeyPress(currentState);
            }
        }

        else if (currentState is EnemySelectedState) {
            if (mouseTile == null) { return; }
            else if (Input.GetMouseButtonDown(0)) {
                currentState = TransitionOnClick(currentState, mouseTile);
            }
        }

        else if (currentState is SelectSkillActivatedState) {
            if (Input.GetMouseButtonDown(0)) {
                currentState = TransitionOnClick(currentState, mouseTile);
            }
            else if (InputUtils.GetKeyPressed(skillKeys) != null) {
                skillKeys.ForEach(keyPressed => {
                    if (Input.GetKeyDown(keyPressed)) {
                        currentState = TransitionOnSkillKeyPress(currentState, keyPressed);
                    }
                });
                StartCoroutine(WaitAMoment(waitTime, "Skill Activation"));
            }
        }

        else if (currentState is TeleportActivatedState) {
            if (Input.GetMouseButtonDown(0)) {
                currentState = TransitionOnClick(currentState, mouseTile);
            }
            else if (Input.GetKeyDown(KeyCode.T)) {
                currentState = TransitionOnTeleportKeyPress(currentState);
            }
        }

        else if (currentState is EnemyTurnState) {
            var stateData = (EnemyTurnState) currentState;
            if (stateData.aiSteps == null) {
                // should be written something like
                // stateData.aiSteps = aiComponent.DetermineAITurns(faction, grid);
                stateData.aiSteps = stateMachine.DetermineAITurns();
            } else {
                var outputString = String.Format("Finishing actions on target {0}", stateData.aiSteps.First().Key);
                currentState = stateMachine.ExecuteAITurn(currentState);
                if (stateData.aiSteps.Count == 0) {
                    currentFaction.entities = stateData.enemies;
                    currentState = TransitionOnEndTurn();
                    StartCoroutine(WaitAMoment(aiStepTime, "Ending AI Turn"));
                } else {
                    StartCoroutine(WaitAMoment(aiStepTime, outputString));
                }
            }
        }
    }

    private State TransitionOnClick(State currentState, Tile clickedTile) {
        return stateMachine.SelectTile(currentState, clickedTile);
    }

    private State TransitionOnEndTurn() {
        var previousFaction = factions.Dequeue();
        currentFaction = factions.Peek();
        factions.Enqueue(previousFaction);
        var nextState = stateMachine.EndTurn(currentState, currentFaction);
        StartCoroutine(WaitAMoment(turnGapTime, "Ending Player Turn"));
        return nextState;
    }

    private State TransitionOnSkillKeyPress(State currentState, KeyCode keyPressed) {
        return stateMachine.ActivateSkill(currentState, keyToSkillIndex[keyPressed]);
    }

    private State TransitionOnTeleportKeyPress(State currentState) {
        return stateMachine.ActivateTeleport(currentState);
    }

    void CreateTilemapComponent() {
        Vector3 origin = new Vector3(0, 0, 0);
        // assuming tiles are squares
        float tileWidth = tile.GetComponent<SpriteRenderer>().size.x;
        Vector2 gridSize = new Vector2(tileWidth * initTileMap.GetLength(0), tileWidth * initTileMap.GetLength(1));
        Vector3 topLeft = new Vector3(origin.x - (gridSize.x / 4), origin.y + (gridSize.y / 4), origin.z);

        for (int i = 0; i < initTileMap.GetLength(0); i++) {
            for (int j = 0; j < initTileMap.GetLength(1); j++) {
                var gameObj = Instantiate(tile, topLeft, Quaternion.identity);
                gameObj.transform.parent = this.transform;
                gameObj.name = String.Format("{0},{1}", i, j);

                var boxCollider = gameObj.GetComponent<BoxCollider2D>();
                Collider2D[] colliders = new Collider2D[10];
                boxCollider.OverlapCollider(new ContactFilter2D().NoFilter(), colliders);

                var tileObj = gameObj.GetComponent<Tile>();

                tileObj.x = i;
                tileObj.y = j;
                initTileMap[i,j] = gameObj;
                topLeft.x += tileWidth / 2;

                // determining if placement is in game-bounds
                tileObj.disabled = true;
                gameObj.SetActive(false);

                if (
                    colliders.Where(collider => collider != null).Any(collider => collider.tag == "EnableGrid") &&
                    !colliders.Where(collider => collider != null).Any(collider => collider.tag == "BlockGridMovement")
                ) {
                    tileObj.disabled = false;
                    gameObj.SetActive(true);
                }
            }
            topLeft.x = origin.x - (gridSize.x / 4);
            topLeft.y -= tileWidth / 2;
        }
        tilemap.Start(this, initTileMap);
    }

    public static Tile GetTileUnderMouse() {
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var hits = Physics2D.RaycastAll(mouseWorldPosition, Vector2.zero).ToList();
        var objs = hits.Select(hit => hit.collider.gameObject);
        var tiles = objs.Select(obj => obj.GetComponent<Tile>()).Where(obj => obj != null).ToList();
        if (tiles.Count() != 0) { return tiles.First(); }
        else { return null; }
    }

    GridEntity PutEntity (int x, int y, GameObject prefab) {
        var target = tilemap.grid[x,y].GetComponent<Tile>();
        var entity = Instantiate(prefab, new Vector2(0,0), Quaternion.identity).GetComponent<GridEntity>();
        entity.GetComponent<SpriteRenderer>().sortingOrder = 1;
        target.TryOccupy(entity);
        return entity;
    }

    void ToggleSkillMenu() {
        skillMenu.SetActive(!skillMenu.activeInHierarchy);
        UpdateSkillMenu();
    }

    void UpdateSkillMenu() {
        if (lastSelectedAlly == null) { return; }

        var title = skillMenu.transform.Find("Title");
        var nameText = (Text) title.transform.Find("Name").GetComponent<Text>();
        var subnameText = (Text) title.transform.Find("Subname").GetComponent<Text>();
        var damage = (Image) title.transform.Find("Damage").GetComponent<Image>();
        var damageChips = GetChipsForSkillMenu(damage.transform);
        var moveRange = (Image) title.transform.Find("MoveRange").GetComponent<Image>();
        var moveRangeChips = GetChipsForSkillMenu(moveRange.transform);
        var atkRange = (Image) title.transform.Find("AtkRange").GetComponent<Image>();
        var atkRangeChips = GetChipsForSkillMenu(atkRange.transform);


        nameText.text = lastSelectedAlly.entityName;
        subnameText.text = lastSelectedAlly.entitySubname;
        SetChips(damageChips, lastSelectedAlly.damage);
        SetChips(moveRangeChips, lastSelectedAlly.maxMoves);
        SetChips(atkRangeChips, lastSelectedAlly.range);

        var skillElements = new List<Transform> {
            skillMenu.transform.Find("Skill1"),
            skillMenu.transform.Find("Skill2"),
            skillMenu.transform.Find("Skill3")
        };

        var skillNameTexts = skillElements.Select(element => {
            return element.Find("SkillName").GetComponent<Text>();
        }).ToList();

        var skillDescTexts = skillElements.Select(element => {
            return element.Find("SkillDesc").GetComponent<Text>();
        }).ToList();

        lastSelectedAlly.skills.ForEach(skill => {
            var skillNameText = skillNameTexts[0];
            skillNameTexts.Remove(skillNameText);

            skillNameText.text = skill.name ?? "";

            var skillDescText = skillDescTexts[0];
            skillDescTexts.Remove(skillDescText);

            skillDescText.text = skill.desc ?? "";
        });

        skillNameTexts.ForEach(text => text.text = "");
        skillDescTexts.ForEach(text => text.text = "");
    }

    private IEnumerable<Image> GetChipsForSkillMenu(Transform transform) {
        return Enumerable.Range(0,8).Select(index => {
            return (Image) transform.Find(
                String.Format("Chip{0}", index)
            ).GetComponent<Image>();
        });
    }

    private void SetChips(IEnumerable<Image> chips, int number) {
        chips.Take(number).ToList().ForEach(chip => chip.gameObject.SetActive(true));
        chips.Skip(number).ToList().ForEach(chip => chip.gameObject.SetActive(false));
    }

    public IEnumerator WaitAMoment(float waitTime, string name) {
        waiting = true;
        Debug.Log("Waiting for... " + name);
        yield return new WaitForSeconds(waitTime);
        Debug.Log("Waiting for... " + name);
        waiting = false;
    }
 }
