﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Tile : MonoBehaviour {

	public int x = 0;
	public int y = 0;
	public GridEntity occupier = null;
	public bool selected = false;
	private string highlightType;
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
			return true;
		}
		else {
			return false;
		}
	}

	void UpdateOccupier () {
		occupier.transform.position = new Vector2(this.transform.position.x, this.transform.position.y);
		occupier.tileX = x;
		occupier.tileY = y;
	}

	public void HighlightAs(string highlightType) {
		switch (highlightType)
		{
			case "attack":
				currentColor = Color.red;
				break;
			case "move":
				currentColor = Color.blue;
				break;
			default:
				currentColor = unselectedColor;
				break;
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
