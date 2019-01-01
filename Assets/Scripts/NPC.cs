using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour {

	public string message;
	private BoxCollider2D boxCollider;
	private ContactFilter2D contactFilter = new ContactFilter2D().NoFilter();

	void Awake () {
		boxCollider = GetComponent<BoxCollider2D>();
	}
	void Update () {
		int numColliders = 10;
		Collider2D[] colliders = new Collider2D[numColliders];
		int colliderCount = boxCollider.OverlapCollider(contactFilter, colliders);

		//print(colliderCount);

		if (colliderCount > 0) {
			for (int i = 0; i < colliderCount; i++) {
				if (colliders[i].gameObject.tag == "Player") {
					//print(message);
				}
			}
		}
	}

	void PlayerInteract() {
		print(message);
	}
}
