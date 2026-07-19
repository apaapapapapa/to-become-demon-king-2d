# Prototype Scene Setup

The project now includes an Editor utility for creating the initial isometric scene hierarchy safely from inside Unity.

## Create the scene

1. Open the project in Unity.
2. Wait for script compilation to finish.
3. From the Unity menu, choose:

```text
Demon King
  -> Prototype
    -> Create Isometric Scene
```

The tool creates and saves:

```text
Assets/Scenes/Prototype/Prototype.unity
```

with the following hierarchy:

```text
Main Camera
Global Light Placeholder
Grid
  Ground
  Collision
  Props
  Foreground
Runtime Prototype Bootstrap
```

## Grid configuration

The generated Grid uses:

```text
Cell Layout: Isometric
Cell Size: (1, 0.5, 1)
```

The Tilemaps are separated by responsibility:

- `Ground`: walkable visual ground tiles
- `Collision`: invisible blocking tiles with `TilemapCollider2D`
- `Props`: trees, rocks, furniture, and other world props
- `Foreground`: elements that may visually overlap actors

This is the intended long-term structure for the game's isometric 2D/2.5D maps.

## Current transition state

The existing `FieldBootstrap` still generates the temporary prototype meadow at runtime. This is intentional so the project remains immediately playable while real prototype tiles are introduced.

The migration sequence is:

1. Validate the isometric composition with the runtime-generated prototype.
2. Create/import a small prototype tileset.
3. Paint the first map on the `Ground` Tilemap.
4. Move blocking geometry to the `Collision` Tilemap.
5. Move trees and scenery to `Props` and `Foreground`.
6. Reduce `FieldBootstrap` until it only spawns gameplay actors and temporary test content.
7. Eventually remove runtime world generation entirely.

## First map target

The first authored map should remain deliberately small. It only needs enough space to validate:

- 8-direction player movement
- controller input
- collision
- Y-based sorting
- walking behind/in front of props
- one interactable NPC or object
- one enemy encounter

Do not build a large world before these interactions work correctly on the authored Isometric Tilemap.
