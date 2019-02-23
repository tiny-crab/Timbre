using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridStage : MonoBehaviour {

	public GameObject[,] grid = new GameObject[6,6];
	public GameObject tile;

	// Use this for initialization
	void Start () {
		Vector3 origin = new Vector3(0, 0, 0);
		// assuming tiles are squares
		float tileWidth = tile.GetComponent<SpriteRenderer>().size.x;
		Vector2 gridSize = new Vector2(tileWidth * grid.GetLength(0), tileWidth * grid.GetLength(1));

		print(gridSize.x);
		print(gridSize.y);

		Vector3 topLeft = new Vector3(origin.x - (gridSize.x / 4), origin.y + (gridSize.y / 4), origin.z);

		var index = 0;
		for (int i = 0; i < grid.GetLength(0); i++) {
			for (int j = 0; j < grid.GetLength(1); j++) {
				grid[i,j] = Instantiate(tile, topLeft, Quaternion.identity);
				index++;
				grid[i,j].name = string.Format("I am Tile #{0}", index);
				topLeft.x += tileWidth / 2;
			}
			topLeft.x = origin.x - (gridSize.x / 4);
			topLeft.y -= tileWidth / 2;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			RaycastHit2D hitInfo = Physics2D.Raycast(mouseWorldPosition, Vector2.zero);
			GameObject mouseTile = hitInfo.collider.gameObject;
			print(string.Format(
				"{0} at {1}, {2}",
				mouseTile.name,
				mouseTile.transform.position.x,
				mouseTile.transform.position.y
			));
		}
	}
}
