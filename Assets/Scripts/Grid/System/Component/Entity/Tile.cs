using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Tile : MonoBehaviour {

	public int x = 0;
	public int y = 0;
	public GridEntity occupier = null;
	private bool selected = false;
	private string highlightType;
	private List<string> currentHighlights = new List<string>();
	public Color currentColor = Color.grey;
	private Color unselectedColor = Color.grey;
	private Color defaultSelectionColor = Color.blue;
	
	// Update is called once per frame
	void Update () {
		if (occupier != null) { UpdateOccupier(); }
		currentColor = DetermineColor();
		this.GetComponent<SpriteRenderer>().color = currentColor;
	}

	// use this return value to determine whether or not to "roll-back" a move
	public bool TryOccupy (GridEntity entity) {
		if (occupier == null) {
			occupier = entity;
			entity.tile = this;
			return true;
		}
		else {
			return false;
		}
	}

	void UpdateOccupier () {
		occupier.transform.position = new Vector2(this.transform.position.x, this.transform.position.y);
		occupier.tile = this;
	}

	public void HighlightAs(string highlightType) {
		// this switch case also denotes "tiers" of highlights.
		// cases on top will be "colored over" by cases on the bottom.
		switch (highlightType)
		{
			case "attack":
				currentColor = Color.red;
				currentHighlights.Add(highlightType);
				break;
			case "move":
				currentColor = Color.blue;
				currentHighlights.Add(highlightType);
				break;
			case "skill":
				currentColor = Color.green;
				currentHighlights.Add(highlightType);
				break;
			default:
				currentColor = unselectedColor;
				break;
		}
		selected = true;
	}

	public void RemoveHighlight(string highlightType) {
		currentHighlights.Remove(highlightType);
		if (currentHighlights.Count == 0) {
			selected = false;
		}
	}

	Color DetermineColor () {
		Color tileColor = new Color();
		
		if (selected) {
			tileColor = currentColor;
		}
		else { tileColor = unselectedColor; }
		
		return tileColor;
	}
}
