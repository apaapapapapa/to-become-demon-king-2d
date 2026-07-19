# To Become Demon King 2D

A Unity 2D/2.5D isometric RPG prototype inspired by the visual direction of games such as Witchbrook.

## Technical direction

- Unity 6
- C#
- Universal Render Pipeline (URP)
- 2D Renderer / 2D Lighting
- Isometric Tilemap
- Unity Input System
- Pixel-art oriented presentation
- Keyboard and gamepad support from the start
- Steam first, with future console portability in mind

## First playable milestone

The first vertical slice should allow the player to:

1. Move in an isometric map with keyboard or gamepad.
2. Walk behind/in front of scenery with correct sprite sorting.
3. Interact with one NPC.
4. Attack one enemy.
5. Defeat the enemy and receive a simple result/reward.

The first goal is not a polished game. It is a small playable loop that validates movement, camera, rendering order, interaction, and combat.

## Recommended project structure

```text
Assets/
  Art/
    Characters/
    Environment/
    Tiles/
    UI/
  Audio/
  Prefabs/
    Characters/
    Environment/
    UI/
  Scenes/
    Prototype/
  Scripts/
    Core/
    Player/
    Combat/
    Interaction/
    World/
    UI/
  Settings/
```

## Development order

1. Create an isometric prototype scene.
2. Configure the Input System.
3. Implement player movement.
4. Add camera follow.
5. Add sprite sorting based on Y position.
6. Add interaction with one NPC.
7. Add one simple enemy and attack action.
8. Add basic HP/damage/death handling.

See `docs/GAME_DIRECTION.md` and `docs/TECHNICAL_DESIGN.md` for details.
