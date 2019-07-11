using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEntityTemplate {

    public int health;
    public Color selectedColor;
    public List<Component> components;

    private GridEntityTemplate(int health, Color selectedColor, List<Component> components) {
        this.health = health;
        this.selectedColor = selectedColor;
        this.components = components;
    }
    
    public static GridEntityTemplate AsPlayer() {
        return new GridEntityTemplate (
            3, 
            Color.white,
            new List<Component> () {
                GetSprite("Sprites/Placeholder")
            }            
        );
    }

    public static GridEntityTemplate AsNPC() {
        return new GridEntityTemplate (
            2,
            Color.red,
            new List<Component> () {
                GetSprite("Sprites/Placeholder")
            }
        );
    }

    private static SpriteRenderer GetSprite(string path) {
        var renderer = new SpriteRenderer();
        var sprite = Resources.Load<Sprite>(path);
        renderer.sprite = sprite;
        return renderer;
    }

}