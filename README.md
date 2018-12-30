# Timbre
This is a prototype of a game.

## Gist
You play as an adventurer. You recruit sidekicks to complement your abilities. Online multiplayer where other players can join in.

## Gimmick
What if there were no numbers in a turn based RPG?

#### Follow up questions:
- What happens when you level up, if there are no stats to increase?
- How does the player feel stronger as they play through the game
- How do characters transform in an RPG other than the level increase and corresponding stat growth?
- How do we give the player a feeling of limitless possibility (DnD) without giving them an overwhelming number of tools (assassin’s creed (bad) vs mario odyssey (good))

## Demo

The player is in a square area constrained on all four sides. They can walk around freely and talk to other entities in the area.
Once a certain event is met, then the grid will overlay and the player will enter into a combat situation. They can then take actions in a turn-based fashion to defeat other entities in the area. Once the other enemies are defeated, the game can return to the free roam method.

## Milestones

### Implementation challenges:

- I want to learn how to make dialog boxes like this: https://www.youtube.com/watch?v=EhLMNWD-H-w
  - V2: give different characters different rhythms / timbre to their voices
- I want to learn how to transition between free overhead 2d movement (Hyper Light Drifter / Link to the Past) to grid-based overhead combat (fire emblem, civilization, advance wars)
  - V2: how to map a grid to an irregularly shaped area
- I want to learn how to create a character class-based game efficiently and modularly
  - I’m curious about how to define player classes (JSON? Unique file format? etc) and how to load predefined NPCs / allies into the game. What about FSMs for combat? What’s the best way to allow flexible / diverse character classes?
- I want to get more familiar with Canvas elements
  - I imagine with a prototype, a character’s actions will appear in a menu above their head, and then you click the action that you want to take.

### Design challenges:
- I want to learn how to make turn-based RPGs flow better (fire emblem echoes dungeon example: https://youtu.be/EETVx8GjT8o?t=33 see how jarring the transition from overworld to grid-based combat?)
- How do we prevent players from “telling” the characters what to do instead of “doing” what you want? I.e. “Pokemon doesn’t even seem fun, you’re not fighting, you’re just ‘telling’ the pokemon what to do”

This README will definitely be out of date in short order. Look at the [GDoc](https://docs.google.com/document/d/1zEJ7_WDPccM0XzXekxrwy5Qn5Qpw6JL8mXUtrWVweAk/edit?usp=sharing) for more up-to-date stuff.
