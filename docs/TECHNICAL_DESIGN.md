# Technical Design

## Baseline

Current project baseline observed in the repository:

- Unity Editor: 6000.5.4f1
- Universal Render Pipeline installed
- Unity Input System installed
- 2D Tilemap packages installed
- 2D Animation tooling installed

This is sufficient for the initial isometric 2D/2.5D prototype.

## Architecture principles

Keep the first implementation simple.

1. Core gameplay code must not directly depend on Steam or console APIs.
2. Input is expressed as logical actions, not hard-coded keys.
3. Rendering concerns should not be mixed into combat/domain logic.
4. Avoid premature frameworks and dependency injection containers.
5. Prefer small MonoBehaviours for scene-facing behavior and plain C# classes for reusable rules when complexity appears.

## Initial folder structure

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

Do not create empty folders purely for architecture. Add them as content appears.

## Scene strategy

Start with one scene:

```text
Assets/Scenes/Prototype/Prototype.unity
```

The prototype scene should contain:

- Grid
- Isometric Tilemap(s)
- Player placeholder
- Main Camera
- one NPC placeholder
- one enemy placeholder
- one interaction target

## Isometric world

Use Unity Tilemap with an isometric layout for the prototype.

Recommended separation:

```text
Grid
  Ground Tilemap
  Collision Tilemap
  Props Tilemap
  Foreground Tilemap
```

Collision data and visual data should be separable where practical.

## Sprite sorting

The game needs deterministic depth ordering so characters can walk behind and in front of scenery.

Initial rule:

```text
sortingOrder = -round(worldY * precision)
```

Use a small component such as `YSortSprite` on dynamic sprites. Static tilemap sorting should be configured consistently with the chosen isometric layout.

Do not rely on manual sorting orders for every object.

## Input

Use Unity Input System.

Initial action map:

```text
Player
  Move        Vector2
  Attack      Button
  Interact    Button
  Dodge       Button
  Pause       Button
```

Suggested bindings:

```text
Move
  WASD
  Arrow keys
  Gamepad left stick

Attack
  Keyboard: J or Space (prototype only)
  Gamepad: South button

Interact
  Keyboard: E
  Gamepad: West button

Dodge
  Keyboard: Left Shift
  Gamepad: East button

Pause
  Keyboard: Escape
  Gamepad: Start
```

Gameplay scripts should consume actions, not inspect specific keyboard keys.

## Player movement

For the first prototype:

- Rigidbody2D-based movement
- movement applied in FixedUpdate
- normalized input to prevent diagonal speed increase
- movement speed exposed in Inspector
- animation integration deferred until the placeholder movement feels correct

The visual is isometric, but movement input remains a 2D world-space vector initially. A later iteration can remap axes if the chosen art/grid orientation requires it.

## Camera

Start with a simple orthographic follow camera.

Requirements:

- smooth follow
- no rotation during prototype
- camera framing tuned before final character sprite production

Cinemachine may be introduced later if it adds value, but it is not required for milestone 1.

## Interaction

Use a small interaction contract rather than coding NPC logic into the player.

Conceptually:

```csharp
public interface IInteractable
{
    void Interact();
}
```

The player checks for an interactable in a short radius or trigger zone and invokes it.

## Combat

Prototype combat should be deliberately small:

```text
Attack input
  -> short attack window
  -> overlap check / hitbox
  -> IDamageable.TakeDamage
  -> enemy HP decreases
  -> death when HP <= 0
```

Initial abstractions may include:

```text
IDamageable
Health
PlayerAttack
EnemyHealth
```

Do not build a complete ability system before the basic attack feels useful.

## Platform portability

Keep these behind interfaces if/when they are added:

- save storage
- achievements
- cloud saves
- platform user identity
- DLC / entitlement checks

Example future boundary:

```text
Core Game
  -> ISaveService
  -> IAchievementService
  -> IPlatformUserService

Platform implementations
  -> Steam
  -> Console
```

No platform abstraction is needed until the first real platform-dependent feature is introduced.

## Performance direction

Because a future console port is a possibility:

- keep transparent overdraw under control
- avoid excessive full-screen 2D lights
- use sprite atlases when asset volume grows
- profile on modest hardware, not only a high-end development PC
- avoid loading the entire game world at once if maps become large

Optimization should be measured, not speculative.

## Milestone 1 definition of done

The first technical milestone is complete when:

1. The project opens without errors.
2. A prototype isometric map is visible.
3. The player can move using keyboard and gamepad.
4. The camera follows the player.
5. Y-based sorting works with at least one foreground object.
6. The player can interact with one object/NPC.
7. The player can damage and defeat one enemy.
