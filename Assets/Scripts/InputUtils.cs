using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class InputUtils {
    public static KeyCode? GetKeyPressed(List<KeyCode> keys) {
        var pressedKeys = keys.Where(keyPressed => Input.GetKeyDown(keyPressed));
        if (pressedKeys.Count() == 0) { return null; }
        else { return pressedKeys.First(); }
    }
}

