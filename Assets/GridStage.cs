using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridStage : MonoBehaviour {

	public int[,] grid = new int[6,10];
	public SpriteRenderer tile;

	// Use this for initialization
	void Start () {
		Vector3 origin = new Vector3(0, 0, 0);
		// assuming tiles are squares
		float tileWidth = tile.size.x;
		Vector2 gridSize = new Vector2(tileWidth * grid.GetLength(0), tileWidth * grid.GetLength(1));

		print(gridSize.x);
		print(gridSize.y);

		Vector3 topLeft = new Vector3(origin.x - (gridSize.x / 4), origin.y + (gridSize.y / 4), origin.z);

		for (int i = 0; i < grid.GetLength(0); i++) {
			for (int j = 0; j < grid.GetLength(1); j++) {
				Instantiate(tile, topLeft, Quaternion.identity);
				topLeft.x += tileWidth / 2;
			}
			topLeft.x = origin.x - (gridSize.x / 4);
			topLeft.y -= tileWidth / 2;
		}

//		Instantiate(tile, topLeft, Quaternion.identity);
//		Instantiate(tile, new Vector3(origin.x + (tileWidth / 2), origin.y, origin.z), Quaternion.identity);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
