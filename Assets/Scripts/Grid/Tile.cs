using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Tile : MonoBehaviour {

	public int gridX = 0;
	public int gridY = 0;
	public GridEntity occupier = null;
	public bool selected = false;
	public Color currentColor = Color.grey;
	
	// Update is called once per frame
	void Update () {
		if (occupier != null) { UpdateOccupier(); }
		if (selected) { DetermineSelectionColor(); }
		else { currentColor = Color.grey; }
		this.GetComponent<SpriteRenderer>().color = currentColor;
	}

	// use this return value to determine whether or not to "roll-back" a move
	public bool TryOccupy (GridEntity entity) {
		if (occupier == null) {
			occupier = entity;
			return true;
		}
		else {
			return false;
		}
	}

	void UpdateOccupier () {
		occupier.transform.position = new Vector2(this.transform.position.x, this.transform.position.y);
	}

	// bleh, OOP methods, I should change this to return something
	void DetermineSelectionColor () {
		if (occupier != null) { currentColor = occupier.selectedColor; } 
		else { currentColor = Color.blue; }
	}
}
