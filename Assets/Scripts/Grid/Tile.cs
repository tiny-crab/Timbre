using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Tile : MonoBehaviour {

	public int gridX = 0;
	public int gridY = 0;
	public List<GameObject> occupiers = new List<GameObject>();
	public bool selected = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (selected) {
			this.GetComponent<SpriteRenderer>().color = Color.blue;
		} else {
			this.GetComponent<SpriteRenderer>().color = Color.grey;
		}
	}
}
