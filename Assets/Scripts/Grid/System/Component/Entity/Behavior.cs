using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// GridSystem (DM) is going to "think" -
//      "what would these individual enemies do"
// but would also think -
//      "what makes this encounter fun / challenging"
// GridSystem is the only component that is capable of "animating" the enemies in the encounter
// the enemies themselves have rules of how they "would" move if they were capable of doing it
//
// Enemies themselves won't have skills defined, but behaviors that determine how those skills are used
// if an enemy runs out of SP, then their skill behaviors are scored lower
//
//

public abstract class Behavior {
    public GridEntity entity;
    public List<Tile> tiles;
    public KeyValuePair<Tile, double> bestAction;
    public GridEntity bestTarget;
    public abstract Dictionary<Tile, double> ScoreGrid(GameObject[,] grid);
    public abstract double FindBestAction(GameObject[,] grid);
    public abstract bool DoBestAction(TilemapComponent tilemap, State currentState);
}

public static class BehaviorUtils {
    // using Func<Behavior> allows a new behavior to be constructed for every entity
    // without this, a single Behavior object would be used between all entities that share the same behavior
    public static Dictionary<string, Func<Behavior>> behaviorNameToBehavior =
        new Dictionary<string, Func<Behavior>>() {
            { "MeleeAttackV1", () => { return new MeleeAttackV1(); } },
            { "RangedAttackV1", () => { return new RangedAttackV1(); } },
            { "Flee", () => { return new Flee(); } },
            { "EvasiveTeleport", () => { return new EvasiveTeleport(); } },
        };

    public static List<Behavior> ToBehaviors(this List<string> behaviorNames, GridEntity entity)
        { return behaviorNames.Select(name => {
            var behavior = behaviorNameToBehavior[name]();
            behavior.entity = entity;
            return behavior;
        }).ToList(); }

    // this is relative to the player
    public enum Hostility {
        HOSTILE,
        FRIENDLY
    }

    public static List<GridEntity> GetAllEntitiesFromGrid(List<Tile> activeTiles, Hostility hostility) {
        return activeTiles
            .Where(tile => tile.occupier != null)
            .Where(tile => {
                if (hostility == Hostility.HOSTILE) { return tile.occupier.isHostile; }
                else if (hostility == Hostility.FRIENDLY) { return tile.occupier.isFriendly || tile.occupier.isAllied; }
                else { return false; }
            })
            .Select(tile => tile.occupier)
            .ToList();
    }
}

// BASIC SKILLS

public class MeleeAttackV1 : Behavior {
    public override Dictionary<Tile, double> ScoreGrid(GameObject[,] grid) {
        tiles = GridUtils.FlattenGridTiles(grid);

        // find all valid targets for a melee attack
        var targets = BehaviorUtils.GetAllEntitiesFromGrid(tiles, BehaviorUtils.Hostility.FRIENDLY).Where(target => target.currentHP > 0);

        // get all the tiles that the aiEntity can use to attack each target
        var targetRanges = targets.SelectMany(target => GridUtils.GenerateTileCircle(grid, 1, target.tile)).ToList();

        // get tiles that are valid to move to in this turn
        var nextTurnRange = GridUtils.GenerateTileCircle(grid, entity.maxMoves, entity.tile);
        nextTurnRange.Add(entity.tile);

        // score each tile in valid move range with distance to be in range of target
        var nextMoveMap = new Dictionary<Tile, double>();
        nextTurnRange.ForEach(tile => {
            var score = targetRanges.Select(attackTile =>
                Mathf.Abs(tile.x-attackTile.x) + Mathf.Abs(tile.y-attackTile.y)
            ).Min();
            nextMoveMap[tile] = score;
        });
        return nextMoveMap;
    }

    public override double FindBestAction(GameObject[,] grid) {
        bestAction = ScoreGrid(grid).OrderBy(element => element.Value).First();
        bestTarget = bestAction.Key.occupier;
        return bestAction.Value;
    }
    public override bool DoBestAction(TilemapComponent tilemap, State currentState) {
        Debug.Log(String.Format("{0} chose to do {1} with score of {2}", entity, "MeleeAttackV1", bestAction.Value));

        var nextTile = bestAction.Key;
        tilemap.MoveEntity(entity.tile, nextTile);

        var tileWithTarget = GridUtils.GenerateTileCircle(tilemap.grid, 1, entity.tile)
                            .ToList()
                            .FirstOrDefault(tile =>
                                tile.occupier != null &&
                                (tile.occupier.isAllied || tile.occupier.isFriendly) &&
                                !tile.occupier.outOfHP
                            );
        if (tileWithTarget != null) { entity.MakeAttack(tileWithTarget.occupier); }
        return true;
    }
}

public class RangedAttackV1 : Behavior {
    public override Dictionary<Tile, double> ScoreGrid(GameObject[,] grid) {
        tiles = GridUtils.FlattenGridTiles(grid);
        // find all valid targets for an attack
        var targets = BehaviorUtils.GetAllEntitiesFromGrid(tiles, BehaviorUtils.Hostility.FRIENDLY).Where(target => target.currentHP > 0);

        // get all the tiles that the aiEntity can use to attack each target
        var targetRanges = targets.SelectMany(target => GridUtils.GenerateTileRing(grid, entity.range, target.tile)).ToList();

        // get tiles that are valid to move to in this turn
        var nextTurnRange = GridUtils.GenerateTileCircle(grid, entity.maxMoves, entity.tile);
        nextTurnRange.Add(entity.tile);

        // score each tile in valid move range with distance to be in range of target
        var nextMoveMap = new Dictionary<Tile, double>();
        nextTurnRange.ForEach(tile => {
            // TODO: score tiles inversely proportional to distance once in range
            // i.e. if a unit can attack from 3 tiles away,
            //      a tile that is 1 tile away will be ranked lowest, 2 tiles away will be ranked in the middle,
            //      and 3 tiles away will be highest ranked
            var score = targetRanges.Select(attackTile =>
                Mathf.Abs(tile.x-attackTile.x) + Mathf.Abs(tile.y-attackTile.y)
            ).Sum();
            nextMoveMap[tile] = score;
        });
        return nextMoveMap;
    }

    public override double FindBestAction(GameObject[,] grid) {
        bestAction = ScoreGrid(grid).OrderBy(element => element.Value).First();
        bestTarget = bestAction.Key.occupier;
        return bestAction.Value;
    }

    public override bool DoBestAction(TilemapComponent tilemap, State currentState) {
        Debug.Log(String.Format("{0} chose to do {1} with score of {2}", entity, "RangedAttackV1", bestAction.Value));

        var nextTile = bestAction.Key;
        tilemap.MoveEntity(entity.tile, nextTile);

        var tileWithTarget = GridUtils.GenerateTileRing(tilemap.grid, entity.range, entity.tile)
                            .ToList()
                            .FirstOrDefault(tile =>
                                tile.occupier != null &&
                                (tile.occupier.isAllied || tile.occupier.isFriendly) &&
                                !tile.occupier.outOfHP
                            );
        if (tileWithTarget != null) { entity.MakeAttack(tileWithTarget.occupier); }
        return true;
    }
}

public class Flee : Behavior {
    public override Dictionary<Tile, double> ScoreGrid(GameObject[,] grid) {
        tiles = GridUtils.FlattenGridTiles(grid);

        var edges = GridUtils.GetEdgesOfEnabledGrid(grid);

        // find all valid targets that could do damage to this entity
        var targets = BehaviorUtils.GetAllEntitiesFromGrid(tiles, BehaviorUtils.Hostility.FRIENDLY).Where(target => target.currentHP > 0);

        // get all the tiles that the targets can move to attack the aiEntity
        var targetRanges = targets.SelectMany(target => GridUtils.GenerateTileRing(grid, target.range + target.maxMoves, target.tile)).ToList();

        // get tiles that are valid to move to in this turn
        var nextTurnRange = GridUtils.GenerateTileCircle(grid, entity.maxMoves, entity.tile);
        nextTurnRange.Add(entity.tile);

        // score each tile in valid move range with distance to be in range of target
        var nextMoveMap = new Dictionary<Tile, double>();
        nextTurnRange.ForEach(tile => {
            var score = targetRanges.Select(attackTile =>
                (Mathf.Abs(tile.x-attackTile.x) + Mathf.Abs(tile.y-attackTile.y)) * -1
            ).Sum();

            if (edges.Contains(tile)) {
                score *= 2;
            }
            nextMoveMap[tile] = score;
        });
        return nextMoveMap;
    }

    public override double FindBestAction(GameObject[,] grid) {
        bestAction = ScoreGrid(grid).OrderBy(element => element.Value).First();
        bestTarget = entity;
        return bestAction.Value;
    }

    public override bool DoBestAction(TilemapComponent tilemap, State currentState) {
        var stateData = (EnemyTurnState) currentState;
        Debug.Log(String.Format("{0} chose to do {1} with score of {2}", entity, "Flee", bestAction.Value));

        var nextTile = bestAction.Key;
        tilemap.MoveEntity(entity.tile, nextTile);

        if (GridUtils.GetEdgesOfEnabledGrid(tilemap.grid).Contains(nextTile)) {
            this.entity.RemoveFromGrid();
            stateData.enemies.Remove(this.entity);
        }

        return true;
    }
}

public class EvasiveTeleport : Behavior {
    // when this behavior is used, enemies will queue a teleport on Turn 1, then teleport to a tile as far away from allies as possible.
    // this will not trigger in fear, and thus the edges of the grid will not enable the enemy to escape

    public override Dictionary<Tile, double> ScoreGrid(GameObject[,] grid) {
        tiles = GridUtils.FlattenGridTiles(grid, true).Where(tile => tile.gameObject.activeInHierarchy).ToList();

        // find all valid targets that could do damage to this entity
        var targets = BehaviorUtils.GetAllEntitiesFromGrid(tiles, BehaviorUtils.Hostility.FRIENDLY).Where(target => target.currentHP > 0);

        // get all the tiles that the targets can move to attack the aiEntity
        var targetRanges = targets.SelectMany(target => GridUtils.GenerateTileRing(grid, target.range + target.maxMoves, target.tile)).ToList();

        // get tiles that are valid to move to in this turn (all tiles that are not currently occupied)
        var nextTurnRange = tiles.Where(tile => tile.occupier == null).ToList();

        var edgeTiles = GridUtils.GetEdgesOfEnabledGrid(grid);

        // score each tile in valid teleport range to avoid range of targets
        var nextMoveMap = new Dictionary<Tile, double>();
        nextTurnRange.ForEach(tile => {
            double score = targetRanges.Select(attackTile => {
                // score is first determined as inversely related to the distance to the attack range of allies
                double dist = Mathf.Abs(tile.x-attackTile.x) + Mathf.Abs(tile.y-attackTile.y) + 1;
                return 1 / dist;
            }).Sum();

            if (entity.currentHP == entity.maxHP) {
                // clamp to avoid being picked at high health
                score = double.MaxValue;
            } else {
                // boost when health is lower
                // this value will always be between 0 and 1, and will "boost" properly
                score *= (double) entity.currentHP / (double) entity.maxHP;
            }


            if (entity.lastSelectedBehavior is EvasiveTeleport) {
                // improve chance this behavior is picked. uses - instead of * in order to keep order of scoring per tile
                score -= 1;
            }
            nextMoveMap[tile] = score;
        });
        return nextMoveMap;
    }
    public override double FindBestAction(GameObject[,] grid) {
        bestAction = ScoreGrid(grid).OrderBy(element => element.Value).First();
        bestTarget = entity;
        return bestAction.Value;
    }
    public override bool DoBestAction(TilemapComponent tilemap, State currentState) {
        var stateData = (EnemyTurnState) currentState;
        if (!(entity.lastSelectedBehavior is EvasiveTeleport)) {
            Debug.Log(String.Format("{0} chose to do {1} turn 1 with score of {2}", entity, "EvasiveTeleport", bestAction.Value));
            entity.lastSelectedBehavior = this;
            // turn 1 of evasive teleport
        } else {
            // turn 2 of evasive teleport
            Debug.Log(String.Format("{0} chose to do {1} turn 2 with score of {2}", entity, "EvasiveTeleport", bestAction.Value));
            tilemap.TeleportEntity(entity.tile, bestAction.Key);
            entity.lastSelectedBehavior = null;
        }
        return true;
    }
}