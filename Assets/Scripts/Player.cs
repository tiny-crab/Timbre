using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : ControllerInteractable {
	
	public float speed = 5f;
	private BoxCollider2D boxCollider;
	private ContactFilter2D contactFilter = new ContactFilter2D().NoFilter();

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
	private bool keyPressed(List<KeyCode> input) { return input.Any(key => Input.GetKey(key)); }

	void Awake () {
		boxCollider = GetComponent<BoxCollider2D>();
	}

	void Update () {
		Vector3 previousPos = transform.position;
		Vector3 movePos = transform.position;

		// TODO: Use UniRX if the project gets bigger
		if (keyPressed(UP)) { movePos.y += speed * Time.deltaTime; }
		if (keyPressed(DOWN)) { movePos.y -= speed * Time.deltaTime; }
		if (keyPressed(LEFT)) { movePos.x -= speed * Time.deltaTime; }
		if (keyPressed(RIGHT)) { movePos.x += speed * Time.deltaTime; }

		transform.position = movePos;

		int numColliders = 10;
		Collider2D[] colliders = new Collider2D[numColliders];
		int colliderCount = boxCollider.OverlapCollider(contactFilter, colliders);

		// do this in both x and y axes, only one is working right now (can't "slide" along walls)
		if (colliderCount > 0) {
			for (int i = 0; i < colliderCount; i++) {
				// this might be a poor and non-performant solution
				if (colliders[i].gameObject.GetComponent<Rigidbody2D>() != null) {
					transform.position = previousPos;
				}
			}
		}

		RaycastHit2D[] interactables = new RaycastHit2D[10];

		int interactColliderCount = Physics2D.CircleCast( 
			origin: transform.position,
			radius: 0.7f,
			direction: new Vector2(0, 0),
			contactFilter: contactFilter,
			results: interactables
		);

		if (interactColliderCount > 0) {
			var results = new List<RaycastHit2D>(interactables)
				.Where(entity => entity.collider != null)
				.Where(entity => entity.collider.gameObject.name != "Player");
			if (keyPressed(INTERACT)) { results.First().collider.gameObject.BroadcastMessage("PlayerInteract"); }
		}

	}
}
