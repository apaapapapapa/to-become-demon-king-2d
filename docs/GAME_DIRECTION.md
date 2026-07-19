# Game Direction

## Core concept

`To Become Demon King` is planned as an isometric 2D/2.5D RPG with a handcrafted pixel-art presentation.

The visual target is not a direct copy of Witchbrook. The reference point is its broad presentation style:

- isometric world view
- richly layered pixel-art environments
- a sense of depth despite 2D assets
- readable character silhouettes
- atmospheric lighting
- exploration and interaction as important parts of the experience

## Initial design assumptions

These assumptions are intentionally lightweight and can change after prototyping.

### Player fantasy

The player begins as a relatively weak character and works toward becoming a Demon King.

### Early gameplay pillars

1. Exploration
2. Character interaction
3. Combat
4. Growth/progression

The prototype should validate these pillars before building a large story or content set.

## Vertical slice

The first playable slice should contain a single compact area.

Suggested flow:

```text
Start in a small settlement or field
  -> walk around
  -> talk to one NPC
  -> receive a simple objective
  -> encounter one enemy
  -> defeat the enemy
  -> return or receive a result
```

The goal is to prove the basic game feel, not to finalize the story.

## Story development policy

Do not write the full scenario first.

Before implementation, define only:

- who the protagonist is
- why the protagonist wants or needs to become the Demon King
- the tone of the world
- the central conflict

Build the first playable slice, then refine the story based on what the game systems express well.

## Visual direction

### Perspective

Use an isometric or near-isometric presentation.

### Environment

Prefer tile-based construction for the prototype. Use separate visual layers for:

- ground
- walls/structures
- props
- foreground occluders
- effects

### Characters

Start with placeholder sprites. Final character art should come after movement scale, camera distance, collision size, and animation requirements are known.

### Lighting

Use URP 2D lighting selectively. Avoid building gameplay that depends on expensive effects before performance targets are established.

## Platform assumptions

Primary initial target:

- Windows / Steam

Future portability target:

- Nintendo console platform(s), subject to platform approval and SDK access

Therefore the game should not depend on mouse-only interaction or Steam-specific APIs in core gameplay code.
