using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public static class InputUtils {
    public static KeyCode? GetKeyPressed(List<KeyCode> keys) {
        var pressedKeys = keys.Where(keyPressed => Input.GetKeyDown(keyPressed));
        if (pressedKeys.Count() == 0) { return null; }
        else { return pressedKeys.First(); }
    }
}

public class PollableButton {
    public Button uiButton;
    public bool isClicked;

    public PollableButton(Button uiButton) {
        this.uiButton = uiButton;
        uiButton.onClick.AddListener(ClickButton);
    }

    private void ClickButton() {
        isClicked = true;
    }
}

