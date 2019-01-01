using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour {
	
	public float speed = 5f;
	private BoxCollider2D boxCollider;
	private ContactFilter2D contactFilter = new ContactFilter2D().NoFilter();

	void Awake () {
		boxCollider = GetComponent<BoxCollider2D>();
	}

	void Update () {
		Vector3 previousPos = transform.position;
		Vector3 movePos = transform.position;

		// TODO: Use UniRX if the project gets bigger
		if (Input.GetKey ("w")) {
			movePos.y += speed * Time.deltaTime;
		}
		if (Input.GetKey ("s")) {
			movePos.y -= speed * Time.deltaTime;
		}
		if (Input.GetKey ("d")) {
			movePos.x += speed * Time.deltaTime;
		}
		if (Input.GetKey ("a")) {
			movePos.x -= speed * Time.deltaTime;
		}

		transform.position = movePos;

		int numColliders = 10;
		Collider2D[] colliders = new Collider2D[numColliders];
		int colliderCount = boxCollider.OverlapCollider(contactFilter, colliders);

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
			if (Input.GetKey ("f")) {
				results.First().collider.gameObject.BroadcastMessage("PlayerInteract");
			}
		}

	}
}
