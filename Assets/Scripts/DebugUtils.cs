using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DebugUtils {

    public static void DebugMouseOverColliders() {
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var hits = Physics2D.RaycastAll(mouseWorldPosition, Vector2.zero).ToList();
        var objs = hits.Select(hit => hit.collider.gameObject);
        Debug.Log("Hit: " + String.Join(", ", objs.Select(obj => obj.name)));
    }

    public static void DebugMouseClickColliders() {
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var hits = Physics2D.RaycastAll(mouseWorldPosition, Vector2.zero).ToList();
        var objs = hits.Select(hit => hit.collider.gameObject);
        if (Input.GetMouseButtonDown(0)) {
            Debug.Log("Hit: " + String.Join(", ", objs.Select(obj => obj.name)));
        }
    }
}