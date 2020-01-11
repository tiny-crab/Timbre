using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;


public class Tile : MonoBehaviour {

    // should refactor this into enum
    public static class HighlightTypes {
        public const string SelectedEntity = "selected_entity";

        // range
        public const string Attack = "attack";
        public const string Move = "move";
        public const string Skill = "skill";

        // select
        public const string SkillSelect = "skill_select";
        public const string Teleport = "teleport";

        // test
        public const string Test = "test";
    }

    public int x = 0;
    public int y = 0;
    public bool disabled = false;
    public GridEntity occupier = null;
    public List<Hazard> hazards = new List<Hazard>();
    private bool selected = false;
    public List<string> currentHighlights = new List<string>();
    private float intensity = 0f;
    public Color currentColor = Color.grey;
    private Color unselectedColor = Color.grey;

    // Update is called once per frame
    void Update () {
        if (occupier != null) { UpdateOccupier(); }
        currentColor = DetermineColor(intensity);
        this.GetComponent<SpriteRenderer>().color = currentColor;
    }

    // use this return value to determine whether or not to "roll-back" a move
    public bool TryOccupy (GridEntity entity) {
        if (occupier == null) {
            // on grid-startup, all entities will have no tile
            if (entity.tile == null) {
                entity.transform.position = new Vector2(this.transform.position.x, this.transform.position.y);
            }
            occupier = entity;
            entity.tile = this;
            hazards.ForEach(hazard => hazard.OnEntityContact(entity));
            return true;
        }
        else {
            return false;
        }
    }

    void UpdateOccupier () {
        occupier.tile = this;
    }

    public void HighlightAs(string highlightType, float intensity = 0) {
        currentHighlights.Add(highlightType);
        this.intensity = intensity;
        selected = true;
    }

    public void RemoveHighlight(string highlightType) {
        currentHighlights.Remove(highlightType);
        if (currentHighlights.Count == 0) {
            selected = false;
        }
    }

    public void RemoveHighlights() {
        selected = false;
        currentHighlights.Clear();
    }

    Color DetermineColor(float intensity = 0) {
        Color tileColor = new Color();

        if (selected) {
            // these cases also denote "tiers" of highlights.
            // cases on top will be "colored over" by cases on the bottom.
            if (currentHighlights.Contains(HighlightTypes.Attack)) {
                tileColor = Color.red;
            }
            if (currentHighlights.Contains(HighlightTypes.Move)) {
                tileColor = Color.blue;
            }
            if (currentHighlights.Contains(HighlightTypes.Skill)) {
                tileColor = Color.green;
            }
            if (currentHighlights.Contains(HighlightTypes.SkillSelect)) {
                tileColor = Color.magenta;
            }
            if (currentHighlights.Contains(HighlightTypes.Teleport)) {
                tileColor = Color.cyan;
            }
            if (currentHighlights.Contains(HighlightTypes.Test)) {
                tileColor = Color.Lerp(Color.green, Color.red, intensity);
            }
            if (currentHighlights.Contains(HighlightTypes.SelectedEntity)) {
                tileColor = Color.white;
            }
            if (currentHighlights.Count == 0) {
                tileColor = unselectedColor;
            }
        }
        else { tileColor = unselectedColor; }

        return tileColor;
    }
}
