# Technical Spec — Opening Level Prototype

## Unity Setup
- Unity 6 LTS (6000.x) or latest LTS
- Template: 2D (URP)
- Input: New Input System only
- Physics: Rigidbody2D + Collider2D
- Camera: Cinemachine

## Movement Requirements
- Rigidbody2D-based controller
- Horizontal acceleration and deceleration
- Variable jump height
- Coyote time
- Jump buffering
- Basic wall jump optional but welcome
- Ground and air control feel polished

## Required Scripts
- `PlayerMovement` — handles all movement and jump logic
- `Collectible` — simple pickup that can later be extended for Health / Power / Shield / Summon stats
- `LevelExit` — trigger that marks level complete

## Level Construction
- Primary geometry via Tilemap
- At least one moving or one-way platform
- One simple puzzle (pressure plate, switch, or timed door)
- 3–5 collectibles placed with varying visibility
- Parallax background layers preferred

## Constraints
- Keep scripts simple and commented
- Avoid ScriptableObjects unless they clearly reduce complexity
- No summons, combat, or advanced state machines in this pass
- Player GameObject must be named "Nexus"

## Folder Convention
- Scripts/
- Prefabs/
- Scenes/
- Art/ (or Tilemaps/)
