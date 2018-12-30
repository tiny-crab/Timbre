using System.Collections;
using System.Collections.Generic;
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
				if (colliders[i].gameObject.tag == "Wall") {
					transform.position = previousPos;
				}
			}
		}
	}
}
