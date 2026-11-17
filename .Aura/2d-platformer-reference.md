# 2D Side-Scrolling Platformer Reference
### For AI game-dev assistants (Bezi, Aura) · Engine: Unity 6 (2D / URP) · Version 1.1 · Nov 2026 (revised after expert review)

> **Reference games:** Dead Cells · Hollow Knight · Shovel Knight · Metroid (Super Metroid, Metroid Dread) · Mega Man (classic + X) · The Messenger
> **Supporting sources:** Celeste (Maddy Thorson's game-feel thread), Game Maker's Toolkit, Vlambeer's "Art of Screenshake", developer blogs and postmortems. All are listed in §10 Sources.

### Engine stack (pin these, and generate code for this stack only)
| Item | Version this doc assumes | Notes |
|---|---|---|
| Unity | **Unity 6 (6000.x LTS)** | Confirm the exact version in `ProjectSettings/ProjectVersion.txt` [VERIFY against the project]. |
| Render pipeline | **URP 17.x** with the **2D Renderer** | URP's own **Pixel Perfect Camera** is built in. **Don't install** the separate 2D Pixel Perfect package, which only works with the Built-In Render Pipeline [S24]. |
| Cinemachine | **Cinemachine 3.x** | Namespace `Unity.Cinemachine`. Classes: `CinemachineCamera`, `CinemachinePositionComposer`, `CinemachineConfiner2D`, `CinemachinePixelPerfect`, `CinemachineImpulseSource`. Cinemachine 2 used the `Cinemachine` namespace, `CinemachineVirtualCamera` and `CinemachineFramingTransposer`. **Don't mix the two** [S25]. |
| Input | **Input System 1.x** (new) | Use `InputAction.WasPressedThisFrame()` and `WasReleasedThisFrame()`. Keep the Update Mode at "Process Events In Dynamic Update" so presses are read in `Update` (§3.1). |
| Physics | 2D physics, `Time.fixedDeltaTime = 1f/60f` | Unity's default is 0.02 s (50 Hz). Set it explicitly (§3.1). |

**Unity 6 API names (AI tools often use the old ones):**
- `Rigidbody2D.linearVelocity`, not `velocity` [S26].
- `Rigidbody2D.bodyType = RigidbodyType2D.Kinematic`, not `isKinematic`.
- `Object.FindFirstObjectByType<T>()` and `FindAnyObjectByType<T>()`, not `FindObjectOfType`.
- On colliders, use Composite Operation = Merge, not "Used By Composite".

### Loading this doc into Bezi and Aura
- **Bezi:** load this file as a **Page** for reference. Put `ai-rules.txt` in **Rules**. Bezi's docs say rules should be short, constant, positive statements without conditionals or anti-examples, and must not conflict [S27].
- **Aura:** attach this file in chat, and put `ai-rules.txt` in Custom Instructions or project memory [S28]. The `Saved/.Aura/project_memory.txt` path is documented for Unreal. Whether it exists for Unity projects is **unconfirmed** [VERIFY].
- **Both:** treat this doc as **read-only**. Propose edits to the designer instead of rewriting it.

---

## 0. Table of Contents
1. How to use this doc (rules for the AI, labels, glossary)
2. Per-game breakdown
3. Player controller and game feel
4. Combat
5. Level design (with recipes for a room, a stage and a region)
6. Puzzles and traversal challenges
7. Art style and asset creation
8. Audio and feedback
9. Implementation checklists and common pitfalls
10. Sources

---

## 1. How to Use This Doc (Rules for the AI)

### 1.1 Labels used in this document
| Label | Meaning | How to treat it |
|---|---|---|
| **[SOURCED]** | Stated by the developer or a cited source (see §10). | Treat as fact about that game. Don't change it when you quote it. |
| **[REC]** | A recommended *starting value* from common practice. It is **not** from any reference game. | Use it as a default and expose it as a tunable field. Expect to retune it in playtests. |
| **[SOURCED-wiki]** | Confirmed by a community wiki, a TAS or frame-data resource, or a game's public source code. These are tertiary sources. | Fine as calibration data. Cite it as "per the community wiki," not as a developer statement. |
| **[VERIFY]** | Not confirmed by any source here. | **Never state it as fact.** Ask the designer, or check it in the game, before relying on it. |
| **[UNITY]** | A Unity-specific implementation note. | Check API names against the project's Unity version. |

### 1.2 Core rules for the AI
1. **Feel first, art second.** Prototype movement and combat in greybox before you make final art. *The Messenger* team's mandate was "not make the game beautiful until it was fun" [SOURCED, S12].
2. **Every number is a tunable.** Expose speeds, timers, gravity and windows as serialized fields or ScriptableObjects. Never hard-code them. Most values here are [REC] starting points.
3. **Work in tiles and pixels, not arbitrary units.** Define one grid (for example 16 px = 1 tile = 1 Unity unit) and size the character, jump arcs, rooms and gaps on it.
4. **Forgive the player's input. Don't forgive their decisions.** Coyote time, jump buffering and corner correction widen timing windows "in the player's favor" [SOURCED, S1]. Difficulty should come from the level and enemy design, not from strict input timing.
5. **Readability beats detail.** The player, hazards, enemies, interactables and background must each be readable at a glance from value (lightness), hue and silhouette. See §7.4.
6. **Teach without text.** Introduce a mechanic safely, develop it, twist it, then conclude (§5.3). Mega Man X's intro stage teaches movement, shooting and the wall jump through level layout [SOURCED, S16].
7. **Telegraph everything that can hurt the player.** An enemy may only attack after it has been on screen for 0.3–0.5 s, and every attack has an anticipation pose and sound (§4.4). Scripted pursuers are the exception, and they announce themselves with sound (§4.4).
8. **One responsibility per component.** Keep input, motor (physics), state machine, animation and feedback (VFX, SFX, camera) in separate components that talk through events. Gameplay timing (hitboxes, i-frames, timers) runs in FixedUpdate ticks from data. Animation is display only (§3.1).
9. **Data-drive content.** Weapons, enemies, rooms and abilities should be ScriptableObjects (or similar) so content can be added without code changes.
10. **When unsure, cite the game.** If you borrow a pattern, name it ("Shovel Knight-style breakable checkpoint") so the designer can check the intent.
11. **Verify in Play mode before you report a task done.** Check the console for errors and the feature against its checklist (§9).
12. **Create editor automation only on request.** Make import presets, AssetPostprocessors and Cinemachine rigs only when the designer asks, then verify them. Aura's docs say it doesn't author AssetPostprocessors unless asked and can only inspect Cinemachine and Timeline assets [S28b].
13. **Clean up generated art before importing it.** Every generated image goes through the pixel-cleanup step (§7.10) before it is used as a gameplay sprite.

### 1.3 Glossary
| Term | Definition |
|---|---|
| **Coyote time** | A short grace window after walking off a ledge in which jump still works [S1]. |
| **Jump buffer** | A jump pressed shortly before landing fires on the landing frame [S1]. |
| **Apex hang / half-gravity peak** | Reduced gravity near the top of a held jump. Celeste applies half gravity there [S1]. |
| **Variable jump height** | Releasing jump early cuts upward velocity, giving short hops. |
| **Corner correction** | Nudges the player around a ceiling corner they clip with their head, or onto a ledge they clip while dashing [S1]. |
| **Hitstop / hit pause / freeze frame** | Freezing the game (or the attacker and target) for a few frames on impact. Dead Cells freezes 1 frame on a crit, then slows the game briefly [SOURCED, S9]. |
| **I-frames** | A period of invulnerability after being hit or while dodging. |
| **Pogo / down-strike** | A downward attack that bounces the player off an enemy or object (and off hazards in Hollow Knight only). It is Shovel Knight's Shovel Drop and Hollow Knight's down-slash. |
| **Cloudstep** | The Messenger's earned extra jump: hit something in midair and you get another jump, as long as you keep finding targets [SOURCED, S12]. |
| **Ability gate / lock-and-key** | An obstacle that can only be passed with a specific ability, so the ability acts as a "key" (Metroidvania). |
| **Sequence break** | Getting past a gate early through skill or an exploit. Super Metroid allows this with advanced techniques [S17]. |
| **Shortcut** | A one-way door or lift opened from the far side that links a deep area back to a hub or save point. |
| **Bench / save room** | A rest point that saves, heals and respawns the player. In Hollow Knight, resting commits new rooms to the map, but only if you own the **Quill** and that area's map [SOURCED-wiki, S29]. |
| **Hazard respawn** | After touching a hazard (spikes, pit), the player warps to the last safe ground and takes a small amount of damage, instead of going back to a checkpoint (Hollow Knight style). See §5.8. |
| **Attack token** | A shared budget that limits how many enemies may attack at the same moment (§4.4). |
| **Save ID** | A stable unique ID on each placed persistent object (broken wall, shortcut, pickup, boss) so its state can be saved (§5.9). |
| **Biome** | A themed region with its own tileset, enemies, palette and mechanic. |
| **Concept graph** | Dead Cells' per-biome node graph that tells the generator the level's length, special rooms and labyrinth level [SOURCED, S5]. |
| **Kishōtenketsu (4-step)** | Introduce → develop → twist → conclude stage structure [S14]. |
| **Telegraph / tell** | The wind-up animation, flash or sound before an attack. |
| **Parallax** | Background layers that scroll at different rates to suggest depth. |
| **PPU** | Pixels Per Unit, the Unity sprite import setting that maps texture pixels to world units [UNITY]. |
| **Palette ramp** | An ordered set of shades of one hue, used for shading. |
| **Selout** | Selective outlining: an outline colored by the adjacent fill instead of pure black. |

---

## 2. Per-Game Breakdown

### 2.1 Dead Cells (Motion Twin / Evil Empire, 2018)
**Genre:** Roguelite "roguevania": procedural levels, permanent death and persistent unlocks.

| Aspect | Notes |
|---|---|
| **Does best** | Combat feel and weapon variety, plus procedural levels that still feel handmade. |
| **Signature mechanics** | Two weapons and two skills per run, with weapons swapped constantly. Stats are color-coded: Brutality red, Tactics purple, Survival green. Each point multiplies the damage of items that scale with that color by **×1.15, compounding**. Dual-color items scale with the higher of their two stats [SOURCED-wiki, S20]. Details are in §4.8. Dodge-roll with i-frames. Touching an enemy's body generally doesn't hurt; damage comes from attacks, projectiles and specific lunges [SOURCED-wiki, S40]. When you die you return to the hub. |
| **Level structure** | A hybrid of fixed and procedural [SOURCED, S5]: (1) The world map, the links between biomes and the key locations are fixed and handmade. (2) Rooms ("tiles") are handmade and tagged with their entrances and exits and a purpose such as combat, treasure or merchant. (3) Each biome has a **concept graph** that sets the length, special rooms, how labyrinthine it is, and how far the entrance is from the nearest exit. (4) The generator picks a random room for each node from that biome's pool and retries until it fits the constraints. (5) Monster count comes from the total length of combat tiles. Rooms are biome-specific: sewers are tight to force mob management, and ramparts are more linear. |
| **Art style** | Pixel art produced through a **3D-to-2D pipeline** [SOURCED, S3]. A pixel model sheet comes first. Then a simple 3D model is rigged in 3ds Max and rendered small *without anti-aliasing*. Each frame is exported as a PNG with a normal map and lit with a toon shader. The character is about 50 px tall in game [SOURCED, S3]. Dead Cells' animation frame rate isn't confirmed. The "30 fps" line in S3 is about the earlier ScarKrow prototype. |
| **Combat** | Fighting-game-style feedback with "particles, stop frames, slow downs." A crit freezes 1 frame, slows the game for a few tenths of a second, and adds a blood spray and an impact sound [SOURCED, S9]. Attacks are pose-to-pose, and in-betweens are added only before or after a key, never between keys [SOURCED, S3]. |
| **What to borrow** | The hybrid room-plus-graph generator, per-weapon gimmicks (a dagger that crits on a backstab [S9]; the developer also gave a hypothetical "sword that crits below 50% HP" as an **example pattern**, not a shipped weapon), crit hitstop with slow-motion, and the 3D-to-pixel pipeline for big casts and cheap animation retakes. |

### 2.2 Hollow Knight (Team Cherry, 2017; Unity)
**Genre:** A Metroidvania with a large interconnected world.

| Aspect | Notes |
|---|---|
| **Does best** | Atmosphere, world interconnection, tense exploration, and pattern-learning boss fights. |
| **Signature mechanics** | The Nail melee attacks in 4 directions, including a down-slash pogo. A pogo bounce refreshes the Mothwing Cloak dash and the Monarch Wings double jump, and so do landing and a Mantis Claw wall cling [SOURCED-wiki, S30, S31]. Hits build Soul, and Soul is spent to heal (Focus) or cast spells, which is a risk/reward loop. Charms are build modifiers. When you die you drop your currency at a "shade" that you must reach and kill to recover it. |
| **Level structure** | An interconnected world of themed areas with ability gates (dash, wall cling, double jump and others). **Maps are staged:** area maps are bought from the cartographer Cornifer (and his wife Iselda's shop). Explored rooms are committed to the map **when you rest at a bench, provided you own the Quill and that area's map** [S15; SOURCED-wiki, S29]. Benches are checkpoint, heal and map-update points, so how sparse they are sets the tension. |
| **Art style** | **Hand-drawn** 2D. Art and traditional frame-by-frame animation are drawn in Photoshop and saved as plain PNGs. Unity's default shaders (`Sprites-Default`, `Sprites-Diffuse`) are used with small modifications [SOURCED, S7]. The muted, near-monochrome look was chosen partly for speed: "Simple is fast. Monotone is fast" [SOURCED, S8]. Depth comes from layered foreground and background sprites, fog, and light shafts. |
| **Combat** | Short-range melee with knockback on both the player and the enemy, which makes spacing a skill. Nail hits push the Knight back; the Steady Body charm exists to remove that recoil [SOURCED-wiki, S32]. Bosses have clear telegraphs and change phase. Contact damage is the baseline threat. |
| **What to borrow** | The Soul loop (offense feeds healing), benches that update the map, and hand-drawn layered parallax with a strict value structure (dark midground, lighter fog behind, pale player). |

### 2.3 Shovel Knight (Yacht Club Games, 2014)
**Genre:** Linear stage-based action platformer, a "rose-tinted" NES game.

| Aspect | Notes |
|---|---|
| **Does best** | Faithful but modernized NES feel, clean teaching stages, and an economy-driven risk/reward design. |
| **Signature mechanics** | The **Shovel Drop** pogo (down-attack bounces off enemies and objects), dig piles, relics (sub-weapons that use magic), and **breakable checkpoints** [SOURCED, S11]. Walking past a checkpoint activates it. Breaking it takes 3 hits and visibly cracks it, gives gold, and removes the checkpoint. The flame color shows how much gold it holds. When you die, the tally screen shows broken checkpoints as X's. |
| **Level structure** | Mega Man-like themed stages chosen from a world map. Each stage introduces and remixes 1–2 gimmicks and ends in a Knight boss. Hidden walls hide treasure. |
| **Art style** | Based on the NES palette (54 colors) plus **4 extra colors**: #22123B dark purple, #360900 deep red, #9E9E5C beige (made for Polar Knight's cloak and skin and used nowhere else), and #824E00 light brown (the skin-diversity color) [SOURCED, S2]. Sprites use **4–5 colors plus transparency** instead of the NES's 3 [S2]. **Virtual resolution is 400×240**, with each game pixel drawn 4.5×4.5 screen pixels at 1080p [SOURCED, S2]. Note that 400×240 is **5:3** (the 3DS top-screen shape), so 4.5× gives 1800×1080, not 16:9. A Yacht Club developer confirmed the PC build uses **non-pixel-perfect** scaling [S38]. **Don't use it as an integer-scaling example.** There is no flicker from the NES's 8-sprites-per-line limit, but flashing on and off is used on purpose for post-hit invincibility instead of transparency. Palette cycling is used for enemy and object damage and for explosions [S2]. Backgrounds average **5–6 parallax layers**, which Yacht Club says helped gameplay readability [S2]. |
| **Combat** | Contact damage and simple enemies. A damage color-cycle shows every hit on an enemy or object, and the player flashes on and off during i-frames [S2]. Bosses have readable patterns. |
| **What to borrow** | The pogo as both a combat and a traversal verb, breakable checkpoints for opt-in difficulty, limited palettes with a few "cheater" colors, and palette-swap shaders for enemy variants and hit flashes. |

### 2.4 Metroid (Super Metroid 1994, Metroid Dread 2021)
**Genre:** The template for the Metroidvania: an isolated, atmospheric exploration game.

| Aspect | Notes |
|---|---|
| **Does best** | Ability-gated nonlinear exploration, environmental storytelling, and isolation. |
| **Signature mechanics** | Upgrades such as the Morph Ball, bombs, missiles, Grapple Beam, Speed Booster and Varia Suit. Doors are color-locked by weapon type. The **Melee Counter** debuted in *Metroid: Samus Returns* (2017). Dread keeps it and adds the Dash Melee, an aerial counter and E.M.M.I. pursuit zones [SOURCED-wiki, S41; S19]. |
| **Level structure** | Abilities act as keys. Each upgrade solves a specific obstacle and stays useful elsewhere. The game starts guided and opens up after key upgrades. It can be mapped as a **dependency graph** [S17, S18]. It shows the player they "can't go here yet" with distinct doors and terrain [S18]. Advanced techniques allow sequence breaks. Dread's **E.M.M.I. zones** are separate "Dread Zones" with their own visual treatment (steam, screen static) and a chase mechanic [SOURCED, S19]. |
| **Art style** | Super Metroid uses dark, saturated 16-bit pixel art with an organic and mechanical (Giger/Alien) look [S19]. Dread is 2.5D in HD. |
| **Combat** | Ranged combat with aim directions. The melee counter (Samus Returns, Dread) rewards reading the enemy's flash tell (§4.6). |
| **What to borrow** | Dependency-graph planning, visible locks you see before you have the key, save and map rooms, and "dread zones" as a change of pace. |

### 2.5 Mega Man (Classic NES series, Mega Man X)
**Genre:** Linear run-and-gun platformer with stage select.

| Aspect | Notes |
|---|---|
| **Does best** | Tight run-and-gun, readable enemy patterns, and boss order as a puzzle. |
| **Signature mechanics** | Free choice of Robot Master stage. Defeating a boss gives its weapon. Weaknesses follow a **rock-paper-scissors** cycle, so no weapon is strongest overall [SOURCED, S21 Inafune]. Charge shot. Up to 3 uncharged buster shots can be on screen at once in classic Mega Man; in later games a full charge shot is usually limited to 1 on screen [SOURCED-wiki, S33, S34]. X adds wall-slide and wall-jump from the start. **The dash comes from the Leg Parts capsule in Chill Penguin's stage**, not the intro stage [SOURCED-wiki, S35, S37]. X's normal dash lowers his hitbox but gives **no i-frames** [S35, S36]. Armor upgrades follow. |
| **Level structure** | Each themed stage introduces 1–2 gimmicks, escalates them, sends the player through a boss corridor (often a mid-boss and a shutter door), then the Robot Master. **The X intro stage teaches by layout** (movement, shooting, enemy behavior, and the wall jump out of the collapsing-highway pit) with no text [S16, S35]. |
| **Calibration** | Mega Man 2, per community frame data [SOURCED-wiki, S43, S44]: walk about 1.375 px/frame (about 5.2 tiles/s), jump 4.875 px/frame with gravity 0.25 px/frame², apex after about 20 frames (0.33 s), height about 50 px (about 3 tiles), with the same gravity going up and down. There is a first-press "inch step" before full speed, with no acceleration ramp; its exact frame count is [VERIFY]. |
| **Art style** | NES-limited sprites. Mega Man's color was changed from red to blue because the NES palette is strong in blues [SOURCED, S2]. Big, readable boss sprites. |
| **Combat** | Ranged combat with limited projectiles on screen. Enemies have learnable fixed patterns, and bosses have a small move set with a clear tell for each move. |
| **What to borrow** | The weakness cycle, stage structure built around gimmicks, the boss shutter-door ritual, and teaching levels without tutorial text. |

### 2.6 The Messenger (Sabotage Studio, 2018)
**Genre:** Ninja Gaiden-style linear action platformer that turns into a Metroidvania after a time-travel twist.

| Aspect | Notes |
|---|---|
| **Does best** | "Frictionless" movement and a visual and mechanical era shift. |
| **Signature mechanics** | **Cloudstep:** "our take on the double jump where you have to hit something to earn your extra jump, repeatable as long as you can find targets." The team designed level layouts around it [SOURCED, S12]. It also has a rope dart (grapple), a wingsuit and wall climbing. There are **no cooldowns or glide limits**, and attacking never cancels the run [S12]. A charged sword beam was cut because it worked against cloudstepping [S12]. |
| **Level structure** | Linear stages in 8-bit first. After the time travel the game opens into a Metroidvania [S13]. **The 8-bit/16-bit shift** swaps art, music (two versions of every track), and **collision sets**: a path can be closed in 8-bit and open in 16-bit. Late in development the team added moving "dimensional shapes" that show the other era through a stencil window. Collision, hazards and SFX in those shapes follow the era currently visible at that spot [SOURCED, S12]. |
| **Art style** | 8-bit NES style in the past and 16-bit SNES style in the future [S13]. |
| **Combat** | Melee slashes that also power traversal (a hit refreshes the cloudstep). Bosses have recognizable moves, phases, and stun windows to punish: "a dance between offense and defense" [SOURCED, S12]. |
| **What to borrow** | Using hits as traversal resources, dual collision layers for world-state puzzles, cutting any feature that fights the core verb, and starting with single-screen "vignettes" to set movement metrics [S12]. |

### 2.7 Cross-game summary: what to take from each
| Need | Best reference |
|---|---|
| Combat feel and juice | Dead Cells |
| World structure and atmosphere | Hollow Knight, Super Metroid |
| Stage teaching and bosses | Mega Man, Shovel Knight |
| Movement flow | The Messenger, Celeste (supporting) |
| Replayable layout | Dead Cells |
| Retro art constraints | Shovel Knight, The Messenger |
| Hand-drawn art | Hollow Knight |
| Pacing with tension zones | Metroid Dread |

---

## 3. Player Controller and Game Feel

### 3.1 Architecture [UNITY]
```
PlayerInput (New Input System) ──► InputBuffer (timestamps presses)
        │
        ▼
PlayerStateMachine (Idle, Run, Jump, Fall, WallSlide, Dash, LedgeHang, Attack, Hurt, Dead)
        │ requests velocity / actions
        ▼
PlayerMotor2D (kinematic: casts + collision resolution)  ◄── PlayerStats (ScriptableObject of tunables)
        │ events (Jumped, Landed, Hit, Dashed)
        ▼
Feedback layer: Animator · VFX · SFX · CameraShake · Hitstop service
```
- **Motor choice.**
  - **Kinematic (recommended for precision platformers):** use a `Rigidbody2D` with `bodyType = RigidbodyType2D.Kinematic`. Sweep with `Rigidbody2D.Cast` / `Collider2D.Cast` (or `Physics2D.BoxCast`) and resolve the X and Y axes separately. This gives exact control over slopes, one-way platforms and corner correction.
  - **Dynamic `Rigidbody2D`:** faster to set up, but it fights you on friction, sliding and bouncing. If you use it, set friction to 0 with a `PhysicsMaterial2D`, use interpolation, and set `linearVelocity` directly. Don't use `AddForce` for running.
- **Kinematic motor rules [UNITY] [REC]:**
  - **Move with `Rigidbody2D.MovePosition`** in `FixedUpdate`, never by writing `transform.position`, or interpolation does nothing.
  - **Skin width 0.01–0.02 u.** Cast the collider shrunk by the skin, then stop the move at `hit.distance - skin`.
  - **Turn off "Queries Start In Colliders"** (Project Settings → Physics 2D) so casts don't hit the player's own collider or report false hits from inside a surface.
  - **Ground check:** count the player as grounded only when a downward cast hits **and `vy ≤ 0`**. For **2 physics ticks after a jump**, ignore ground. Otherwise the takeoff tick re-arms coyote time and allows a free second jump.
  - **Depenetration:** after moving platforms or crushers move, run `Physics2D.OverlapCollider` / `Collider2D.Distance` and push the player out along the minimum separation. If they can't be pushed out, apply crush damage and a hazard respawn (§5.8).
  - **Contact filter:** use a `ContactFilter2D` with the Ground and OneWay layer masks. Ignore OneWay hits unless the player is above the platform's top and moving down.
- **Update loop and input timing [UNITY]:**
  - Set `Time.fixedDeltaTime = 1f/60f` (0.016667). A literal 0.0166 drifts from 60 Hz, and Unity's default of 0.02 is 50 Hz.
  - Set Rigidbody2D Interpolation to Interpolate so rendering stays smooth between physics ticks.
  - **Read input in `Update`** with `WasPressedThisFrame()` / `WasReleasedThisFrame()`, and store each press with a timestamp in an `InputBuffer`. **Consume it in `FixedUpdate`**, by checking `now - pressTime <= bufferTime`. Never poll button *presses* in `FixedUpdate`, because a press between ticks gets missed.
  - **Hitstop and timers:** decide which clock the coyote and buffer timers use. This doc's default: gameplay timers run on **physics ticks that don't advance during hitstop**, so a buffered jump survives a freeze. If you use unscaled timestamps instead, a freeze longer than the buffer will expire the input.
  - Write every tunable in **seconds and units per second**, never "per frame," so changing the tick rate doesn't break the tuning.
- **Layers.** Use separate layers for Ground, OneWay (`PlatformEffector2D` for dynamic bodies, or a custom one-way check for kinematic), Hazard, Enemy, PlayerHurtbox, PlayerHitbox, Interactable and Climbable.
- **Hitbox vs. hurtbox.** Make them separate colliders on separate layers.
- **Frame data drives hitboxes, not Animation Events.** Animation Events fire on render frames, so their timing drifts with frame rate and with the Animator's update mode during hitstop. Store startup, active and recovery as **tick counts** in the `WeaponDefinition` and enable hitboxes from a FixedUpdate tick counter. The Animator is display only. It can be set to Animate Physics so it stays in step. Animation Events remain fine for cosmetic things like footsteps and dust.
- **Facing and flipping (one method for the whole project):** flip the **root** with `transform.localScale = new Vector3(facing, 1, 1)`. Child hitboxes, VFX anchors and muzzle points mirror automatically. Keep sprites at scale ±1 only, and never scale them to resize. (The alternative, `SpriteRenderer.flipX` plus mirrored offsets in code, is valid but don't mix the two.)
- **Tunables.** Keep a `PlayerStats` ScriptableObject that holds every value in the tables below, so the designer can edit them during Play mode.

### 3.2 Units and metrics
Define these first, because everything else depends on them.

| Metric | [REC] starting value | Notes |
|---|---|---|
| Tile size | 16 px (pixel art) or 64–128 px (hand-drawn, at source resolution) | 1 tile = 1 Unity unit (PPU = 16 for 16 px tiles). |
| Player collider | about 0.75 tiles wide × 1.5–2 tiles tall | Make it narrower than the sprite so the player can slip through gaps. |
| Full jump height | 3–4 tiles | Set the height first, then derive gravity from it (§3.4). |
| Short hop height | 1–1.5 tiles | Comes from the jump cut. |
| Max horizontal jump distance | 4–6 tiles at top speed | Standard gaps must stay under this by at least 1 tile. |
| Run speed | 5–9 tiles/s | Mega Man is about 5.2 tiles/s [SOURCED-wiki, S43]. Above about 9 needs a wider view and more look-ahead. |

The full geometry metrics table (ledges, shafts, clearances, door sockets) is in §5.6.

Game references: Celeste runs at 320×180 and treats a 2 px wall-jump tolerance as "a quarter of a tile," which implies 8 px tiles [SOURCED, S1]. Dead Cells' hero is about 50 px tall [SOURCED, S3]. Hollow Knight's art is high-resolution hand-drawn work, so its "pixel" metrics don't apply.

### 3.3 Horizontal movement
| Parameter | [REC] range | Effect | Reference flavor |
|---|---|---|---|
| Max run speed | 5–9 u/s | Overall pace | Messenger/Dead Cells are fast; Mega Man is about 5.2 [SOURCED-wiki, S43] |
| Ground acceleration (time to reach max speed) | 0–0.1 s | Responsiveness | Classic Mega Man has a first-press "inch step," then full speed with no ramp (frame count [VERIFY]). A "weighty" feel is about 0.15 s. |
| Ground deceleration (time to stop) | 0–0.08 s | Stopping precision | Precision games stop almost instantly |
| Turnaround multiplier | 1.5–2× accel | Snappy reversals | — |
| Air acceleration | 50–80% of ground | Air control | Celeste uses 65% [SOURCED-wiki, S42]. Classic Castlevania has none (avoid). |
| Air deceleration with no input | 20–50% of ground | Momentum kept in the air | — |
| Run-cancels-on-attack | Light attacks keep run momentum. Heavy attacks may root the player, set per `WeaponDefinition.rootsPlayer`. | Flow | The Messenger never cancels the run on attack [SOURCED, S12]. That's one game's choice, not a universal rule. |

Implementation: use `velocity.x = Mathf.MoveTowards(velocity.x, input * maxSpeed, accel * dt)` on the motor's own velocity, then move by `velocity * dt` with `MovePosition`. Pick the accel rate based on whether the player is grounded or airborne, turning or not, and pressing input or not.

### 3.4 Jump
**Derive the physics from design values. Don't guess at them.**
```
gravity       = (2 * jumpHeight) / (timeToApex^2)
jumpVelocity  = gravity * timeToApex          // = sqrt(2 * gravity * jumpHeight)
```
| Parameter | [REC] range | Notes |
|---|---|---|
| Jump height | 3–4 tiles | Use the max height (jump held). |
| Time to apex | 0.3–0.45 s | Shorter feels snappier. |
| Fall gravity multiplier | 1.2–2.0×, default 1.5. Use 1.0 for a retro feel. | A faster fall reduces the floaty feel. Above 2.0 shortens the horizontal jump and feels heavy. Mega Man and Celeste use the same gravity rising and falling [S42, S43]. |
| Jump-cut (release early) | Multiply upward velocity by 0.4–0.5 once, *or* apply 2–3× gravity while rising with the button released | This gives variable height. |
| **Apex hang** | 0.5× gravity when \|vy\| < threshold (**25–40% of jumpVelocity**) **and the button is held** | Celeste uses half gravity at the peak while jump is held [SOURCED, S1]. Its threshold is 40 against a jump speed of 105, about 38% [SOURCED-wiki, S42]. Below 20% is barely noticeable. |
| Max fall speed (terminal) | **1.0–1.6× jumpVelocity** (about 15–25 u/s for typical jumps) | Celeste is 160/105, about 1.5× [S42]. Keeps the camera and collisions stable and gives the player time to read where they'll land. |
| Fast-fall (hold down) | 1.3–1.6× normal terminal speed | Optional. |
| **Coyote time** | 0.08–0.12 s (about 5–7 ticks at 60 Hz) | [SOURCED that it exists, S1]. Celeste uses 0.1 s [SOURCED-wiki, S42]. Cancel it once a jump has been used. |
| **Jump buffer** | 0.1–0.15 s | The jump fires on the landing frame [SOURCED behavior, S1]. |
| Head-bonk corner correction | Nudge up to **4–8 px (0.25–0.5 tile on a 16 px grid)** sideways | Celeste does this [SOURCED, S1]. Its value is 4 px on 8 px tiles, which is half a tile [SOURCED-wiki, S42]. |
| Ledge or dash corner correction | Pop up onto a ledge if the miss is ≤ 0.25–0.5 tiles | [S1]. Celeste's dash corner correction is 4 px [S42]. |
| Platform momentum inheritance | Add the platform's velocity to the jump, and keep it for about 0.1 s after the platform stops | Celeste "lift momentum storage" [S1]. |
| Double jump height | 70–90% of the first jump | It should feel like a correction, not a reset. |

**Coyote and buffer logic** (pseudocode). `Update` stores the press times, and this runs in `FixedUpdate`:
```
// Update():  if (jump.WasPressedThisFrame()) input.jumpPressTime = tickClock;
//            if (jump.WasReleasedThisFrame()) input.jumpReleased = true;
// FixedUpdate(): tickClock += dt  (doesn't advance during hitstop)

if (ignoreGroundTicks > 0) ignoreGroundTicks--;
bool grounded = ignoreGroundTicks == 0 && vy <= 0 && GroundCastHits();
if (grounded) coyoteTimer = coyoteTime; else coyoteTimer -= dt;

bool buffered = (tickClock - input.jumpPressTime) <= bufferTime;
if (buffered && coyoteTimer > 0) {
    vy = jumpVelocity;
    coyoteTimer = 0;
    input.jumpPressTime = -999;   // consume the press
    ignoreGroundTicks = 2;        // don't re-ground on the takeoff tick
}
if (input.jumpReleased && vy > 0) vy *= jumpCutMultiplier;
input.jumpReleased = false;

float g = gravity * (vy < 0 ? fallMult : 1f);
if (jumpHeld && Mathf.Abs(vy) < apexThreshold) g *= apexGravityMult;
vy = Mathf.Max(vy - g * dt, -maxFallSpeed);
```

**Celeste calibration data** from its public `Player.cs` [SOURCED-wiki, S42]. Units are px and px/s at 8 px per tile, 320×180. Divide by 8 to get tiles.
| Constant | Value | In tiles |
|---|---|---|
| JumpSpeed | 105 px/s | ≈ 13 tiles/s |
| MaxFall | 160 px/s | ≈ 1.5× JumpSpeed |
| HalfGravThreshold | 40 px/s | ≈ 38% of JumpSpeed |
| JumpGraceTime (coyote) | 0.1 s | — |
| AirMult (air control) | 0.65 | — |
| UpwardCornerCorrection / DashCornerCorrection | 4 px / 4 px | 0.5 tile |
| WallJumpCheckDist | 3 px (the S1 article says 2 px) | ≈ 0.4 tile |
| WallJumpForceTime (input lock) | 0.16 s | — |
| DashSpeed / EndDashSpeed | 240 / 160 px/s | Keeps ≈ 67% |
| DashCooldown | 0.2 s | — |

### 3.5 Dash
| Parameter | [REC] range | Notes |
|---|---|---|
| Dash distance | 3–5 tiles | Gap metrics must account for it. |
| Dash duration | 0.12–0.2 s | Use constant velocity and zero gravity during the dash. |
| Cooldown | **0.15–0.3 s minimum cooldown, plus a refresh** on landing, wall contact, pogo or hit | Celeste uses 0.2 s [S42]. Hollow Knight's dash cooldown is 0.6 s (0.4 s with the Dashmaster charm), and a pogo refresh does **not** override it [SOURCED-wiki, S30]. With zero cooldown, pogo refresh enables dash-spam exploits. The Messenger avoids tracked cooldowns on its traversal moves [S12]. |
| Air dashes per airtime | 1 | Refresh on ground, wall, pogo, or a hit (the cloudstep style). |
| I-frames during dash | Optional, 0–100% of the duration | A Dead Cells-style dodge-roll has them. Mega Man X's normal dash has **none**; it only lowers his hitbox. The exceptions are later armors, such as the X2 Mach Dash startup and the X8 Shadow Dash [SOURCED-wiki, S35, S36]. |
| End-of-dash momentum | **50–70%** for a traversal dash, **0–20%** for a combat dodge-roll | Celeste keeps 160/240, about 67% [S42]. More kept momentum gives a "slide" feel. |
| Dash-jump | Keep the dash speed through the jump | This is Mega Man X's long jump. |

### 3.6 Wall slide, wall jump and wall climb
| Parameter | [REC] range | Notes |
|---|---|---|
| Wall slide max speed | 2–4 u/s | Slide only while pressing into the wall (Mega Man X style) or automatically. Pick one and stay consistent. |
| Wall stick time | 0.1–0.2 s | Gives the player time to press away without losing contact. |
| Wall jump velocity | Horizontal 0.8–1.2× run speed, vertical 0.8–1× jumpVelocity | A strong push away allows zig-zag climbing. |
| Input lock after wall jump | 0.1–0.2 s, or a reduced air control curve | Stops the player from instantly steering back into the wall. Celeste uses 0.16 s [S42]. |
| Wall-jump detection distance | 1–3 px beyond the collider | Celeste's article says 2 px, and more for a harder "super wall jump" [SOURCED, S1]. Its source code uses 3 px [S42]. |
| Climb (The Messenger / Celeste) | Climb speed 2–4 u/s. Stamina is optional, and The Messenger has none. | Needs a "climbable" surface rule (all walls, or tagged walls only). |

### 3.7 Ledge grab and ledge climb
| Parameter | [REC] range | Notes |
|---|---|---|
| Detection | Two raycasts: the chest ray hits the wall and the head ray hits nothing | Requires the player to be falling or near the apex. |
| Grab window (vertical) | 0.25–0.5 tiles | Generous is better. |
| Climb-up | Animation-driven, 0.2–0.35 s, with the end position snapped to the top of the tile | Allow a jump-cancel out of the hang. |
| Auto-grab vs. input | Auto-grab plus "down" to drop | Fewer inputs to manage. |

### 3.8 Pogo / down-strike
| Behavior | Shovel Knight (Shovel Drop) | Hollow Knight (down-slash) | [REC] implementation |
|---|---|---|---|
| Trigger | Down in the air puts you in a sustained "drop" stance [SOURCED as the core mechanic, S11b] | Down plus attack in the air gives one slash | Choose a stance (sustained) or a strike (one-shot). |
| Bounce targets | Enemies, bubbles, dirt blocks and destructibles. Spikes kill on touch, and no source confirms pogoing hazards. | Enemies, spikes, saws and certain objects | Tag these with a `Pogoable` component. |
| Bounce velocity | — | — | 0.8–1.0× jumpVelocity (fixed, so jump height can't change the result). A held jump can extend it a little. |
| Refresh | — | Refreshes the dash and double jump, but the 0.6 s dash cooldown still applies [SOURCED-wiki, S30, S31] | Refresh air resources on a pogo, and keep the minimum dash cooldown (§3.5). This lets you build "pogo chain" challenges. |
| Hitstop | Small | Small | 2–4 ticks [REC]. |

**Design use:** the pogo (Hollow Knight style) makes hazards (spikes) into platforms and enemies into stepping stones. You can build whole traversal challenges out of chains of pogo targets over pits.

### 3.9 Cloudstep and rope dart (The Messenger)
- **Cloudstep [SOURCED, S12]:** after you hit an enemy, projectile or lantern-type object in midair, you get a new jump. This repeats while targets remain.
  - **Implementation:**
    - An `OnHitConfirmed` event while airborne sets `airJumpsAvailable = 1`.
    - Add a short hitstop (2–3 frames [REC]) and a distinct SFX so the player feels the jump was "earned."
    - Place targets in arcs 3–5 tiles apart [REC] so a chain is readable.
- **Rope dart / grapple:** fire it in a direction (or toward the nearest valid anchor) and pull the player to the anchor point.
  - [REC] cast range: 5–8 tiles.
  - [REC] pull speed: 15–25 u/s.
  - End with momentum carried forward, or with a hang.
  - Show valid anchors clearly in the art (a distinct shape or color, see §7.4).
- **Wingsuit:** a glide with no duration limit (no tracked meters [S12]). Use a horizontal speed of about 1–1.2× run speed and a sink rate of 1–2 u/s [REC].

### 3.10 Hit response: hitstop, screen shake, knockback and i-frames
| Parameter | [REC] range | Notes |
|---|---|---|
| Hitstop, light hit | 2–4 ticks (0.03–0.07 s) | Freeze the attacker and target, or set `Time.timeScale` to about 0 briefly. Use unscaled time for the hitstop timer itself. |
| Hitstop, heavy hit or crit | 4–8 frames, plus 0.1–0.3 s at 0.3–0.5× speed | Dead Cells crit: a 1-frame freeze, then a slowdown of "a few tenths of a second" [SOURCED, S9]. |
| Hitstop, player hurt | 4–8 ticks | Lets the player see what hit them. |
| **Hitstop cap** | Total global hitstop ≤ 0.15 s per 1 s of combat | Multi-hit and rapid weapons otherwise stutter the whole game. Use per-entity hitstop (freeze only the attacker and target) for rapid hits. |
| Screen shake, light | Amplitude 0.05–0.1 u, 0.1 s | Use trauma-based shake (amplitude = trauma²) with Perlin noise. |
| Screen shake, heavy or boss | 0.2–0.4 u, 0.2–0.4 s | Add an accessibility option to turn it down or off. |
| Enemy knockback | 2–6 u/s horizontally, 0.1–0.2 s | Scale it by weapon weight. Heavy enemies resist it. |
| Player knockback on hurt | 4–8 u/s away, plus a small upward pop | Briefly lock input (0.15–0.3 s). |
| Player self-knockback on hit (recoil) | 1–3 u/s | In Hollow Knight, nail hits push the Knight back; the Steady Body charm exists to remove that recoil [SOURCED-wiki, S32]. |
| Player i-frames after hurt | 0.8–1.5 s | Show them by flashing the sprite on and off (Shovel Knight style [SOURCED, S2]) or with an alpha blink. Offer a reduced-flash option (§8.1). |
| Enemy hit flash | 1–3 ticks at full white, or a palette cycle | Shovel Knight uses a damage color-cycle on enemies and objects [SOURCED, S2]. |

[UNITY] **Hitstop service:** a singleton that sets `Time.timeScale = 0f` and waits with `WaitForSecondsRealtime`. Merge overlapping requests by keeping the max duration, not the sum, and enforce the hitstop cap. While `timeScale` is 0, `FixedUpdate` doesn't run, so tick-based gameplay timers (§3.1) pause automatically. Camera shake should run in unscaled time, or with Cinemachine Impulse.

### 3.11 Camera [UNITY: Cinemachine 3]
| Parameter | [REC] range | Notes |
|---|---|---|
| Orthographic size | **Ortho size = reference height ÷ (2 × PPU).** For 320×180 at PPU 16 that's 180 ÷ 32 = 5.625, a view of 20×11.25 tiles. | For hand-drawn art, set the view to about 20–28 tiles wide. Celeste uses 320×180 (40×22.5 tiles of 8 px) [S1]. |
| Dead zone (horizontal) | 1–2 tiles | Small idle jitter shouldn't move the camera. |
| Look-ahead | 2–4 tiles in the facing or moving direction, with smoothing 0.3–0.6 s | Faster games need more. |
| Vertical follow | Snap to the ground's Y on landing (platform snapping) and don't follow every jump | Stops nausea from constant vertical motion. Follow vertically only during long falls or climbs. |
| Damping | X: 0.1–0.3, Y: 0.3–0.6 | — |
| Room bounds | `CinemachineConfiner2D` with a `PolygonCollider2D` (or `CompositeCollider2D`) for each room | Metroid and Hollow Knight confine the camera to each room. |
| Room transition | A scroll transition (Metroid/Mega Man door scroll) of 0.3–0.6 s, or a fade | Freeze gameplay during it. |
| Boss camera | A locked or wider framing that keeps the whole arena visible | Every boss attack starts inside the frame. |

**Cinemachine setup [UNITY]:**
- **Version:** this doc targets **Cinemachine 3** (`using Unity.Cinemachine;`). Use a `CinemachineCamera` with a `CinemachinePositionComposer` body and a `CinemachineConfiner2D`. Cinemachine 2 names (`CinemachineVirtualCamera`, `CinemachineFramingTransposer`, `using Cinemachine;`) belong to the old version. Don't mix them [S25].
- **Pixel art:** add the **`CinemachinePixelPerfect` extension to every `CinemachineCamera`** [S25]. Blends between cameras aren't pixel-perfect mid-transition, so use cuts or short blends.
- **Upscale Render Texture** on URP's Pixel Perfect Camera restricts which ortho sizes are valid and can shift framing. Test framing with it on before tuning the dead zone.
- **Confiner:** when you change a confiner's shape at runtime, invalidate its cache. CM3 uses `InvalidateBoundingShapeCache()` and CM2 used `InvalidateCache()` [VERIFY both names against the installed package].
- **Shake:** use a `CinemachineImpulseSource` on the shaker and a `CinemachineImpulseListener` on the camera.
- **Aura note:** Aura can inspect Cinemachine assets but doesn't reliably author them [S28b]. Build Cinemachine rigs only when the designer asks, and verify them in Play mode.

---

## 4. Combat

### 4.1 Combat pillars
1. **Readable:** every threat has a tell. The Messenger team warns against bosses "moving in random directions with a bunch of projectiles flying all over" [SOURCED, S12].
2. **Weighty:** impact comes from hitstop, VFX, SFX and knockback together. "It's the sum of all these things" [SOURCED, S9].
3. **Rhythmic:** alternate defense (dodge and read) with offense windows (stun, recovery). The Messenger calls this "a dance between offense and defense" [S12].
4. **Integrated with movement:** attacks feed traversal (pogo, cloudstep) and never stop movement unless that's intended.

### 4.2 Melee attack anatomy
Every attack has a timeline stored in its `WeaponDefinition` as **physics-tick counts at 60 Hz**. A FixedUpdate counter enables the hitbox from that data. The animation only displays it (§3.1).

| Phase | Purpose | [REC] light attack | [REC] heavy attack |
|---|---|---|---|
| **Startup (anticipation)** | Commitment; player startup is short | 2–5 ticks | 10–25 ticks |
| **Active** | Hitbox is live | 2–4 ticks | 4–8 ticks |
| **Recovery** | Vulnerable and can't act | 6–12 ticks | 15–30 ticks |
| **Cancel window** | The next combo hit, a dodge or a jump can interrupt | The last 30–50% of recovery | Dodge-cancel only |
| Combo chain | — | 2–3 hits, with a stronger finisher | 1–2 |

**Dead Cells rule:** time the key poses with the fewest frames first. Add in-betweens only before or after keys, never between them, and let VFX (smears, arcs) carry the motion [SOURCED, S3]. To make a weapon feel heavier, "add an extra frame or two so that it lasts 0.2 of a second longer" on the impact [SOURCED, S9].

**Directional attacks:** forward, up and down (pogo in the air), as in Hollow Knight. The hitbox should reach about 1.5–2.5 tiles for a sword-length melee [REC].

**Root vs. keep momentum:** light attacks keep run momentum. A heavy weapon may root the player during startup and active ticks, set by `WeaponDefinition.rootsPlayer`.

**One hit per swing:** each attack instance gets an `attackId` (an incrementing int). Each hurtbox keeps a small set of recent `attackId`s and ignores repeats, so one swing can't hit the same target twice. Multi-hit weapons issue a new `attackId` per hit tick.

### 4.3 Ranged attacks (Mega Man / Metroid)
| Parameter | [REC] range | Notes |
|---|---|---|
| Projectile speed | 12–20 u/s | It must outrun the player's run speed. |
| Max player shots on screen | 3 uncharged shots; 1 fully charged shot | Classic Mega Man allows up to 3 uncharged buster shots on screen. In later games a full charge is usually limited to 1 [SOURCED-wiki, S33, S34]. A limit makes spacing matter. |
| Fire rate | 4–8 shots/s | — |
| Charge shot | Charge for 0.5–1.5 s, with 2 visual stages | Show it with sprite flashing and an audio pitch rise. |
| Aim directions | Fixed forward (Mega Man) / 8-way (Metroid) / free aim (Dread uses a free-aim mode) | Pick one and design enemy placement around it. |
| Muzzle flash, recoil, shell VFX | Always | Vlambeer's juice list [S22]. |

### 4.4 Enemy design
**Make each enemy a "sentence."** Give it one verb, one tell, one weakness, and a place where it makes sense.

| Archetype | Verb | Tell | Counterplay | Examples |
|---|---|---|---|---|
| Walker / patroller | Paces and turns at edges | None needed (passive) | Jump over or hit it | Mega Man Mets, HK crawlers |
| Charger | Rushes along the ground | Pause, crouch, flash, sound | Jump over, or punish the recovery | HK husks, Dead Cells zombies |
| Turret / shooter | Fixed projectile | Muzzle glow 0.3–0.6 s [REC] | Dodge, reflect, or close distance | Mega Man turrets |
| Flyer (homing) | Moves toward the player slowly | Wing-flap SFX | Up or down attacks, pogo | HK Vengefly, Mega Man Telly |
| Shielded | Blocks from the front | Visible shield | Attack from behind or above, or pogo | Mega Man Sniper Joe, Dead Cells Shieldbearer. Shovel Knight has shielded foes too, but their name is unconfirmed [VERIFY]. (Shield Knight is Shovel Knight's ally, not an enemy.) |
| Bomber / area denial | Lobs arcing projectiles | Wind-up | Move out of the arc | Dead Cells Grenadier [VERIFY] |
| Ambusher | Hides in the floor, ceiling or wall | Small tell such as a twitch or particles | Read the environment | Metroid ceiling creatures |
| Heavy / elite | Big hits, high HP | Long anticipation | Bait and punish | Dead Cells elites |
| Pursuer (zone) | Invulnerable chaser | Its sound and a zone visual | Stealth, or flee to the exit | Dread's E.M.M.I. [S19] |

**Rules:**
- **Telegraph length scales with damage:** 0.3–0.5 s for light attacks and 0.6–1.2 s for heavy ones [REC].
- **Tells use three channels:** an animation pose, a color or flash (often a white or red glint), and a sound.
- **Activation and the on-screen attack rule [REC]:** enemy AI wakes when the enemy is within **2–4 tiles outside the camera view**, so it's already moving when it scrolls in instead of visibly waking late. It may start an attack only after it has been **on screen for at least 0.3–0.5 s**. Scripted pursuers (E.M.M.I.-style) are the exception, and they must announce themselves by sound first.
- **Contact damage:** decide globally whether touching an enemy hurts the player. Hollow Knight, Mega Man and Metroid say yes. In Dead Cells, touching an enemy's body generally doesn't hurt; damage comes from attacks, projectiles and specific lunges such as the zombie's leap [SOURCED-wiki, S39, S40]. Communicate the choice through the art.
- **Placement teaches:** first show the enemy alone on flat ground, then on a platform edge, then in a pair, then combined with a hazard (§5.3).
- **Variants:** use palette swaps and stat changes for the "elite" or later-biome versions (Shovel Knight's limited palettes made enemy variants [S2]).
- **Dead Cells monster placement:** the number of monsters scales with the combat-tile length of the level, and each monster type has its own placement constraints [SOURCED, S5].

**Enemy AI skeleton [UNITY] [REC]**, one state machine per enemy, with per-enemy data in an `EnemyDefinition` ScriptableObject:
```
Idle/Patrol ──(player in aggro range AND line of sight)──► Aggro (approach or hold range)
Aggro ──(in attack range AND holds an attack token AND on screen ≥ 0.3–0.5 s)──► Telegraph
Telegraph (pose + flash + sound, length from data) ──► Attack (active ticks)
Attack ──► Recover (punish window) ──► Aggro
Aggro ──(player beyond leash range for > 2–3 s)──► Return to spawn ──► Patrol
Any ──(hit)──► Hurt/Stagger (if poise broken) ──► Aggro
Any ──(HP ≤ 0)──► Dead (death pop, drops, report to save system if persistent)
```
- **Walkers** use an edge probe (a ray down, ahead of the feet) and a wall probe (a ray forward) so they turn instead of falling off platforms.
- **Attack tokens:** an `AttackTokenManager` holds N tokens (N = 1–2 in exploration and 2–3 in arenas). An enemy must take a token to enter Telegraph and gives it back after Recover. This stops piles of enemies all attacking on the same tick.
- Aggro range 6–10 tiles and leash range 12–16 tiles [REC].

### 4.5 Damage model [REC]
| Element | Rule | Reference |
|---|---|---|
| Player HP | Count it in **hits**, not large numbers. 4–9 hit points shown as pips or masks. | HK uses masks. Mega Man uses a pip bar. |
| Enemy HP | 1–3 player hits for basic enemies, 5–15 for elites | — |
| Enemy damage i-frames | **None** for enemies (every new `attackId` lands), plus the one-hit-per-swing rule (§4.2) | Keeps combos responsive. |
| Poise / stagger | Each enemy has a poise value. Hits subtract weapon poise damage. At 0 it staggers (interrupt plus a short punish window), then poise resets. | Heavy weapons stagger and light ones don't. |
| Damage types | Physical plus 2–4 elements or statuses, each with a per-enemy resistance multiplier (0, 0.5, 1, 2) | The Mega Man weakness cycle [S21] is a resistance table where each boss has one ×2 weakness. |
| Status effects | Burn (damage over time, spreads), bleed (damage over time, stacks), freeze (stops the enemy; usable as a platform, §6.2), poison (slow damage over time, stacks) | Dead Cells-style statuses [VERIFY the exact Dead Cells rules]. Each status has a duration, stack cap, tick rate and a distinct color and particle. |
| Crits | A conditional ×1.5–2 multiplier plus crit feedback (§3.10) | Dead Cells weapon gimmicks [S9] |

### 4.6 Parry and counter windows [REC]
| Element | Value |
|---|---|
| Counterable tell | A distinct **flash** (one color reserved for "counterable") plus a sound, starting 0.3–0.5 s before the hit |
| Parry / counter input window | 0.1–0.2 s (6–12 ticks) before the hit lands |
| On success | Cancel the enemy attack, add a hitstop of 6–10 ticks, then stagger the enemy or play a counter animation for big damage |
| On failure | The normal hit plus a short recovery, so spamming parry is punished |
| References | The Metroid Melee Counter (Samus Returns, Dread) [S41], Dead Cells shield parry [VERIFY the window] |
### 4.7 Boss design
**Mega Man pattern boss:**
- 3–5 moves, each with a unique tell.
- A deterministic cycle, or a weighted-random choice with no immediate repeats.
- A small arena (one screen) behind a shutter door, and a full health bar shown at the start.
- Weak to one weapon (rock-paper-scissors [S21]).

**Hollow Knight / Messenger phase boss:**
- **Phase structure:** P1 teaches 2–3 moves. P2, at about 66% HP [REC], adds 1–2 moves and speeds up. P3, at about 33% HP, changes the arena (hazards, a floor collapse) or combines moves.
- **Stagger windows:** after enough damage or a specific punish, the boss is "stunned or panting," and "you get to deal a meaningful chunk of damage" [SOURCED, S12].
- **Phase transitions** are short invulnerable beats (a roar or a pose) with a camera shake. They show the player that progress was made.

**Boss length and HP [REC]:**
- Aim for a **time-to-kill of 60–150 s** on a first successful clear (shorter for early bosses).
- Derive HP from the numbers: `bossHP = expectedPlayerDPS × targetFightSeconds × uptime`. Uptime is the fraction of the fight the player can actually hit the boss, usually 0.3–0.5.
- Recalculate when weapon damage changes, and check it against playtest times.

**Boss checklist:**
| Item | Rule |
|---|---|
| Arena | Fits on 1–1.5 screens. The camera is locked or zoomed so the whole arena is visible. |
| Tells | Every move has a tell of ≥ 0.4 s [REC]. Fast moves get a pre-flash. |
| Punish windows | At least one safe punish window after each move. |
| Fairness | No unavoidable damage. The safe zone or dodge timing is consistent. |
| Feedback | The boss flashes on every hit, and HP is visible (bar or damage states). |
| Retry | A checkpoint right outside the arena. Skippable intro on retry. |
| Spectacle | Big readable silhouette, and attacks that use the whole screen. Treat the boss as a reward of pixel animation [S12]. |

### 4.8 Dead Cells weapon variety and the roguelite loop
- **Loadout:** 2 weapons plus 2 skills. Swap often, because the difficulty curve pushes you to change ("that sword's not doing as much damage as you need") [SOURCED, S9].
- **Each weapon has one gimmick** [SOURCED, S9]. Examples:
  - A dagger crits on a backstab.
  - Freeze grenades set up fast weapons.
  - *Example pattern* (the developer's hypothetical, not a shipped weapon): a sword variant that always crits below 50% HP.
- **Stat colors** [SOURCED-wiki, S20]:
  - Brutality (red), Tactics (purple) and Survival (green).
  - Each point in a stat multiplies the damage of items that scale with it by **×1.15, compounding**: damage × 1.15^(n−1) at stat level n.
  - HP is a base of 100 times a separate per-color multiplier, with diminishing returns. Survival gives the most HP and Tactics the least.
  - Dual-color items scale with the **higher** of their two stats. Colorless items scale with your highest stat.
  - Stats come from Scrolls of Power (most let you pick between 2 or 3 colors), amulets, and **scroll fragments: 4 fragments = +1** (Boss Cell 3 and up).
  - Rule of thumb: commit to a build within a run.
- **Affix pattern [REC]:** base weapon + 1–2 random affixes ("+burn on hit," "crit on frozen") + a quality tier. Keep affixes **conditional** so they create play patterns, not flat stat increases.
- **Loop:**
  1. A run goes through biomes and fights.
  2. You collect currency ("cells") along the way.
  3. You spend it at a checkpoint between biomes.
  4. When you die you lose the run but keep permanent unlocks.
  5. You choose the next run's branch (biomes reached by different exits).
- **Data model [UNITY]:**
  - `WeaponDefinition` (SO): `damage`, `frameData`, `hitbox`, `gimmickCondition`, `scalingColors[]`, `animSet` and `sfxSet`.
  - `AffixDefinition` (SO): `trigger` (OnHit, OnCrit, OnKill), `condition` and `effect`.
  - Weapons are rolled from weighted loot tables per biome.

### 4.9 Damage feedback stack (apply on every confirmed hit)
1. Hitstop (§3.10).
2. Target flash or palette cycle, 1–3 ticks.
3. Knockback on the target, and optional recoil on the attacker.
4. An impact VFX at the contact point (a spark, slash arc or blood or ichor), oriented along the attack direction.
5. Hit SFX (layered, with ±5–10% random pitch [REC]).
6. A damage number (optional; Dead Cells shows them, Hollow Knight doesn't).
7. Screen shake only on heavy, crit or kill hits. Don't shake on every hit, because players get numb to it.
8. A death "pop": the enemy disintegrates, explodes or flashes, then leaves debris or a corpse ("permanence" [S22]).

### 4.10 Encounter pacing and healing
- **Beat structure:** traverse (low tension) → encounter (high) → reward or rest (low). Don't put back-to-back arenas without a breather.
- **Density [REC]:** 1–3 enemies per screen in exploration areas and 4–8 in arena lock-ins. Per the Dead Cells pattern, scale enemy count by the length of the combat space [S5].
- **Arena lock-in:** doors close and waves spawn (2–3 waves). Doors open with a strong audio cue.
- **Mix archetypes:** pair a *pressure* enemy (charger or flyer) with a *space-control* enemy (turret or bomber).
- **Recovery:** after a hard encounter, place a health pickup, a checkpoint or a quiet visual vista.

**Healing economy [REC]** (set it together with checkpoint spacing, §5.8):
| Model | How it works | Pairs with |
|---|---|---|
| Earned healing (Hollow Knight Focus) | Hits fill a meter, and the player spends it on a slow, interruptible heal | Sparse benches. Combat itself is the healing source, which rewards aggression. |
| Limited charges (Dead Cells flask) | N charges per run or per rest, refilled at specific points | Procedural runs and between-biome checkpoints. |
| Pickups (Mega Man) | Enemies and level placements drop health | Dense checkpoints and linear stages. |
- Pick **one primary model**. A heal should take 0.5–1.2 s and be interruptible, so healing mid-fight is a decision [REC].
- Restore full HP and healing charges at every save or rest point.

---

## 5. Level Design

### 5.1 Structure types
| Type | Reference | Structure | When to use |
|---|---|---|---|
| **Linear stage** | Mega Man, Shovel Knight, early Messenger | A left-to-right or up-down sequence. Gimmick-themed, with checkpoints and a boss at the end. | Teaching, tight pacing, arcade replay. |
| **Metroidvania world** | Super Metroid, Hollow Knight, late Messenger | An interconnected map with gates, backtracking, shortcuts and hubs. | Exploration and discovery. |
| **Procedural roguelite** | Dead Cells | Handmade rooms assembled by per-biome graphs, inside a fixed world map [S5]. | Replayability. |
| **Hybrid** | The Messenger | Linear first, then the twist unlocks Metroidvania backtracking [S13]. | Changing the experience halfway through. |

### 5.2 Metroidvania world structure and gating
**Core principle:** abilities are keys. Each ability opens specific gates *and* stays useful for movement or combat everywhere [S17, S18].

**Gate types:**
| Gate | Key | Visual language |
|---|---|---|
| A high ledge | Double jump or wall jump | A visible, unreachable ledge with a reward or path on it |
| A wide gap | Dash, grapple or glide | A gap wider than the max jump |
| A narrow tunnel | Morph ball or slide | A low opening that's clearly too small |
| A cracked wall or floor | Bombs or a heavy attack | Cracks, different color or texture (be consistent!) |
| A colored door | A specific weapon (Metroid) | A color-coded door frame |
| A hazard floor (acid, heat) | A suit upgrade | Distinct hazard color and particles |
| A dark room | A lantern | Darkness overlay |
| A one-way door or lift | Reaching the far side | An arrow or lever visible from the locked side |
| An era or world state | A dimension switch (The Messenger) | Different art set [S12] |

**Rules:**
1. **Show the lock before the key.** The player should see the gate (and remember it) well before they get the ability. Super Metroid uses distinct doors and terrain to say "you can't go here yet" [S18].
2. **Each ability unlocks at least 3 things:** the main path forward, 1 or 2 old secrets, and a new traversal verb in combat.
3. **Backtracking must be shorter than the first trip.** Open shortcuts (one-way doors, lifts, collapsing walls) that link deep areas back to hubs and save rooms.
4. **Hubs:** 1 central hub with 3–5 spokes [REC]. The spokes interconnect lower down to make loops.
5. **Save and rest spacing [REC]:** a save point every 3–6 rooms on the critical path, one right before each boss, and fewer in "dread" areas to raise tension. Hollow Knight uses sparse benches for disorientation, for example in Deepnest [S15].
6. **Maps:** support auto-mapping, and optionally staged map acquisition. In Hollow Knight you buy an area's map from Cornifer or Iselda, and with the Quill, new rooms are added when you rest [S15, S29]. Mark doors, saves, gates you've seen, and collected or uncollected items.
7. **Dependency graph first** (GMTK method [S17, S18]). Before building, chart: Area → Ability → Unlocks. Check it with an **editor reachability validator** [UNITY] [REC]: for each ability set in pickup order, flood-fill from the start room through doors and gates the player can pass with that set. Confirm the next ability and the exit are reachable each time, and that there's always a path back out (no soft-lock). Run it after every layout change.
8. **Sequence breaks:** decide on purpose whether they're allowed. If they are, guard the critical story beats with hard gates.
9. **Change of pace:** add a "dread zone" or a gimmick area (Dread E.M.M.I. zones [S19]) every 2–3 regions.
10. **Fast travel and map stations:** once a region is cleared, backtracking needs a fast-travel network. Examples are Hollow Knight's Stag stations and Super Metroid's map rooms, which reveal a whole region's layout. [REC]: 1 fast-travel node per region, placed next to a save point, plus 1 map station per region that reveals that region's map.
11. **Room streaming [UNITY] [REC]:**
    - Make each room a **prefab** (or an additive scene for big regions), each with its own Grid and Tilemaps.
    - Load the neighboring rooms in the background. Swap rooms during the transition scroll or fade, while gameplay is frozen.
    - Keep the player, the camera rig, the HUD and managers in a persistent bootstrap scene marked `DontDestroyOnLoad`, so they survive room loads.
    - The camera confiner switches to the new room's shape on entry.

### 5.3 Linear stage teaching (Mega Man, Shovel Knight, Nintendo 4-step)
**The Kishōtenketsu 4 steps** [S14]:
1. **Introduce (Ki):** show the new element in a safe space, where failure costs little (no pit underneath).
2. **Develop (Shō):** use it again with more risk (a pit, an enemy nearby, timing).
3. **Twist (Ten):** recontextualize it (combine it with another element, reverse it, speed it up).
4. **Conclude (Ketsu):** a final test or a celebratory use, then retire it.

**Mega Man X intro lesson** [S16, S35]: teach without text. Start with a safe space to move and shoot, then simple enemies, then a collapsing highway that drops X into a pit that's only escapable by **wall jumping**. That forces the player to discover the mechanic. (The dash isn't taught here. It comes later from the Leg Parts in Chill Penguin's stage [S37].)

### 5.4 Dead Cells procedural assembly from handmade rooms
Follow these 6 steps (adapted from S5):
1. **Fixed frame:** hand-design the world map, the biome connections, and where permanent keys or runes sit.
2. **Room library:** hand-build rooms. Tag each with `biome`, `purpose` (combat, treasure, merchant, entrance, exit, boss, secret), `exits` (N/S/E/W door sockets with positions) and `size`.
3. **Concept graph per biome:** set the nodes (entrance, exit, specials), the path length, the branching factor ("labyrinth-ness"), and the minimum distance from entrance to exit.
4. **Generate:** for each node, pick a random room from the biome pool that matches the required sockets and purpose. If it doesn't match, retry with a different room.
5. **Populate:** budget monsters from combat-room length. Each monster type has constraints (a flyer needs open air, a turret needs a ledge) [S5].
6. **Loot and secrets:** place them on tagged anchors inside rooms.

[UNITY] Store each room as a prefab with a `RoomData` component (sockets, tags) and a Tilemap child. Use a seeded `System.Random` so runs are reproducible.

### 5.5 Era and world-state shifting (The Messenger)
- Use **two collision sets per room** (Tilemap A for the 8-bit era and Tilemap B for the 16-bit era) plus an era flag [S12].
  - Global toggle: swap the art, music (crossfade between two synced stems) and the active collision.
  - Local windows (the advanced version): a moving stencil shape reveals the other era. Collision and hazards then have to check which era is visible *at the contact point* [SOURCED, S12]. It's expensive, so **build the global toggle first, and build local windows only when the designer asks for them.**
- **Implementation [UNITY] [REC]:**
  - **Global toggle:** put each era's Tilemap and its Composite Collider on its own physics layer (`Era8`, `Era16`). The motor's `ContactFilter2D` layer mask includes only the active era's layer. To toggle, swap the mask, the visible Tilemap renderers and the music stem. Before switching, check that the player isn't overlapping solid ground in the target era. If they are, block the switch or push them out (depenetration, §3.1).
  - **Local windows:** keep both era layers active in the filter. For each contact, test whether the contact point lies inside a window shape (`Collider2D.OverlapPoint` on the window's trigger collider). Keep a contact only if its era matches the era visible at that point. Hazards and SFX emitters do the same test.
- **Level uses:**
  - A path is closed in one era and open in the other.
  - An enemy exists only in one era.
  - A platform appears when you switch eras mid-jump.

### 5.6 Camera framing and room sizes
| Room type | [REC] size in screens | Purpose |
|---|---|---|
| Corridor | 2–4 screens wide × 1 tall | Movement, simple encounters |
| Shaft | 1 wide × 2–4 tall | Vertical traversal, falling, climbing |
| Arena | 1–1.5 × 1 | A combat lock-in or boss |
| Hub | 2 × 2 | Several exits, a landmark, an NPC or save |
| Secret nook | ≤ 1 × 1 | Reward |
| Save room | 1 × 1 | Rest, quiet, with distinct music |

**Framing rules:**
- Keep the floor in the lower third of the view for horizontal travel. During vertical shafts, center the player or add look-ahead in the direction they're moving (down while falling).
- Leave visibility ahead of the player of at least **runSpeed × 0.75 s (about 5–7 tiles)** [REC], so it scales with speed. Hazards must appear on screen with ≥ 0.5 s of reaction time.
- Avoid "leaps of faith." If a jump lands off screen, show the landing with a camera hint zone, a visible landmark, or by zooming out.
- Room edges should align with the camera confiner so the camera doesn't show outside the playfield.

**Level geometry metrics [REC]**. Derive these from the controller tunables (§3), then lock them. Starting values assume a 3.5-tile jump, a 2-tile-tall player and a 16 px grid.
| Metric | Starting value | Rule |
|---|---|---|
| Max jump-up ledge (no double jump) | 3 tiles | Full jump height minus 0.5 tile |
| Max jump gap (running) | 4 tiles | Max horizontal jump minus 1 tile |
| Safety margin | Horizontal 1 tile, vertical 0.5 tile | Apply to every required jump |
| Wall-jump shaft width | Min 2 tiles, max 4 tiles | Narrower is cramped; wider can't be climbed with zig-zag jumps |
| Drop before a hard landing | 6–8 tiles | Optional land-stun or dust VFX |
| Standard ceiling height | 4–5 tiles in corridors | Keeps full jumps possible |
| Crawl or slide clearance | 1 tile | Only if crouch or slide exists |
| Door socket size | 3 tiles tall × 1 tile thick, floor on socket row 0 | Every room uses the same socket so rooms connect |
| One-way platform thickness | Visually 0.25–0.5 tile; collider at the top edge | Use a distinct top-lip art style |
| Slopes | **22.5° and 45° only**, with matching Rule Tiles | No sliding when idle on slopes. Stick to the slope going downhill. |
| Ladders | 1 tile wide. Up or down to grab, snap to the ladder center, and snap to standing at the top. | Jumping off a ladder is allowed |

**Metrics gym [UNITY]:** before building levels, build a `MetricsGym` scene. It contains one test for each row above: ledges at 1–5 tiles, gaps at 2–6 tiles, shafts at 2–5 tiles wide, both slope angles, a ladder, one-way platforms, a door socket and a pogo-target chain. Re-run it after every controller tuning change.

### 5.7 Sightlines, landmarks and secrets
- **Sightlines:** let players see the reward or exit before they can reach it (the "carrot"). Use **line of sight to teach the route**: show a ledge from below before the player climbs to it.
- **Landmarks:** every hub and region needs one unique, big, memorable visual (a giant statue, a broken machine, a waterfall). Players navigate by "the room with the big skull," not by a coordinate.
- **Area identity** has four layers: (1) a palette, (2) a tileset silhouette (jagged vs. rounded), (3) its music, and (4) one signature mechanic or hazard.
- **Secrets:**
  - *Breakable walls:* use a consistent, subtle tell (a slightly different tile, a crack, an off-grid texture, particles). Shovel Knight and Hollow Knight both hide paths in walls. Use one rule for the whole game.
  - *Hidden paths through the foreground:* a foreground layer fades out as the player enters.
  - *Off-path ledges:* visible but needing a later ability (re-uses gating).
  - *Rule:* about 1 optional secret per 2–3 rooms in exploration areas [REC].
  - *Reward:* currency, a health or energy shard (4 shards = 1 upgrade is a common pattern), lore, or a cosmetic.

### 5.8 Checkpoints
- **Placement:**
  - Before every difficulty spike.
  - After every hard section, so the player never repeats a section they've already mastered.
  - Outside every boss arena.
- **Visuals:** checkpoints should be **visible**. Yacht Club moved from invisible checkpoints to visible ones because "visual indicators… give you something to strive for and prevent confusion" [SOURCED, S11].
- **Opt-in difficulty:** breakable checkpoints. Breaking one gives a reward, and dying afterward sends the player to the previous checkpoint [S11].
- **Hazard respawn vs. death respawn** (Hollow Knight style):
  - Touching a **hazard** (spikes, pits, acid) costs a small amount of damage, such as 1 hit. The player then warps back to the **last safe ground** without dying.
  - Only reaching 0 HP sends the player back to the checkpoint or bench.
  - **Implementation [REC]:**
    - Record a `lastSafePosition` whenever the player has been grounded on non-hazard, non-moving ground for at least 0.1–0.2 s.
    - On hazard contact: hitstop, fade or flash, warp to `lastSafePosition`, then 0.5–1 s of i-frames.
  - This is essential for pogo and spike rooms, because a failed spike chain shouldn't cost the whole section.

### 5.9 Persistent world state and save IDs [UNITY] [REC]
- **Stable IDs:**
  - Every placed object whose state must persist gets a `SaveId` component holding a GUID string. That includes breakable walls, opened shortcuts and doors, pickups, upgrade shards, defeated bosses, activated levers and map stations.
  - Generate the GUID once in the editor (`OnValidate`, only if it's empty) and **never regenerate it**.
  - Duplicating an object in the editor must give it a new ID. Detect duplicate IDs with an editor validation pass.
- **Save data:** a `WorldState` that maps `SaveId → state` (for example `opened`, `collected`, `defeated`), plus the player's data (abilities, HP max, currency, map rooms seen, last bench). On `Start`, each object reads its own state and applies it, so broken walls stay broken and pickups stay collected.
- **Reset rules** (pick one set and document it):
  | Event | Resets | Persists |
  |---|---|---|
  | Hazard respawn | Nothing | Everything |
  | Death | Room enemies, timed puzzles, unsaved currency (or currency drops, HK-style) | All `SaveId` state saved at the last rest |
  | Rest at a bench or save point | Respawns all regular enemies (HK-style) and refills HP and healing | Bosses, shortcuts, walls, pickups, abilities |
  | Room exit | Enemies and puzzle progress inside the room (see §6.5) | `SaveId` state |
- AI tools often forget this layer, which makes secrets close again and bosses respawn. Every persistent object needs a `SaveId`.

### 5.10 Recipes

#### Recipe A: How to build a room
1. **Define its purpose.** Pick one: traverse, combat, puzzle, secret, rest, hub or boss. Write a one-line intent such as "teach wall jump over a safe floor."
2. **Pick dimensions** from §5.6 and set the camera confiner to match.
3. **Mark entrances and exits** on the grid. Use the standard door socket from the metrics table (§5.6: 3 tiles tall, floor on socket row 0) so rooms connect cleanly.
4. **Greybox in Tilemap** with only 3 tiles: solid, one-way and hazard. Use the level geometry metrics from §5.6 for every gap and ledge, and test any unusual jump in the metrics gym first.
5. **Place the critical path first.** Test it at minimum skill (no dash, no advanced tech).
6. **Add the challenge** using the 4-step structure in miniature: a safe demo, a risky repeat, then a combination.
7. **Place enemies** with their archetype intent (§4.4). Check that they don't activate off screen.
8. **Add the reward or secret,** at least one sightline-visible "carrot."
9. **Playtest:** check that **a first-timer clears it within 3 attempts**, and that it takes an expert 5–15 seconds [REC]. Fix any cheap deaths.
10. **State pass:** give every persistent object a `SaveId` (§5.9), and check the room's puzzle reset rules and soft-lock exit (§6.5).
11. **Art pass:** tileset rules, background parallax, a landmark if it's a hub, lighting, then VFX (§7).
12. **Audio pass:** ambience loop, and interactive SFX.
13. **Tag** the room's data (biome, purpose, sockets) if it's used procedurally.

#### Recipe B: How to build a stage (linear, Mega Man / Shovel Knight style)
1. **Theme and gimmick:** one visual theme plus 1–2 core gimmicks (for example "conveyor belts and falling blocks").
2. **Beat sheet** (about 4–8 minutes of play [REC]):
   | Section | Content |
   |---|---|
   | 1. Intro | A safe gimmick introduction and 1 basic enemy |
   | 2. Develop | The gimmick plus a pit or enemy |
   | **Checkpoint 1** | |
   | 3. Twist | Gimmick A combined with gimmick B, or a reversal |
   | 4. Mid-boss or arena | Pays off the skills learned so far |
   | **Checkpoint 2** | |
   | 5. Conclude | The hardest combination, then a breather |
   | 6. Boss corridor | Shutter door, health refill, **Checkpoint 3** |
   | 7. Boss | Uses the stage's gimmick in the arena |
3. **One secret per section** (a breakable wall or an alternate high route).
4. **Retire the gimmick** at the end of the stage. Don't repeat it everywhere.
5. **Difficulty ramp across stages:** later stages combine gimmicks from earlier ones.
6. **Playtest the "death map":** log where deaths happen, then soften spikes or add checkpoints.

#### Recipe C: How to build a Metroidvania region
1. **Region brief:** a name, a palette (2–3 dominant hues, see §7.4), a landmark, a music mood, a signature hazard or mechanic, and the ability found here.
2. **Place it on the world dependency graph:** list the abilities it requires to enter, the ability it grants, and the gates it contains for *other* regions' abilities (future backtracking).
3. **Macro layout on graph paper or in a whiteboard tool:**
   - 10–25 rooms [REC].
   - 1 save room near the entry.
   - 1 mid-region save.
   - 1 pre-boss save.
   - 1 fast-travel node and 1 map station (§5.2).
   - 1 map station or vendor.
4. **Critical path:** entry → ability room → boss → exit. Make it about 60% of the rooms.
5. **Loops and shortcuts:** 2–3 shortcuts that open from the deep side back to the entry save [REC].
6. **Side content:** 30–40% of the rooms are optional. They hold secrets, upgrade shards, lore and NPCs.
7. **Gate seeding:** put 2–4 visible gates in this region for abilities found later.
8. **Ability tutorial room:** right after the ability pickup, add a room that *requires* it in a safe 4-step demo, then an exit that also needs it.
9. **Build the rooms** using Recipe A, greybox first.
10. **Traversal test:** time the path from entry save to boss (aim for 5–10 minutes the first time and 1–2 minutes after shortcuts are open [REC]). Check that it can't soft-lock.
11. **Map pass:** confirm the map shows the rooms and marks gates you've seen. Optionally add an area map for purchase.
12. **Art and audio identity pass:** parallax set, ambient particles, music, and the landmark vista placed at a natural pause point.

---

## 6. Puzzles and Traversal Challenges

### 6.1 Puzzle philosophy for action platformers
- Puzzles are **light and spatial**, solved by movement and timing more than by logic. The player should understand the rule in under 10 seconds [REC].
- The **solution uses the player's existing verbs** (jump, attack, dash, pogo, cloudstep, grapple, era shift). Don't add puzzle-only controls.
- Make failure cheap: respawn near the puzzle and reset it automatically.

### 6.2 Puzzle and challenge catalog
| Type | Description | Reference | Implementation notes [UNITY] |
|---|---|---|---|
| **Switch / door** | Hit a switch to open a door, optionally on a timer | Universal | A `Switch` raises a UnityEvent or C# event and a `Door` listens. Timed: show the countdown with ticking SFX. |
| **Pressure plate / weight** | Stand on it or put an object on it to hold it | Common | Use `OverlapBox` and count bodies. |
| **Timed run** | Reach a door before it closes | Mega Man, Dead Cells timed doors | Make the door visible from the switch if you can. |
| **Moving platforms** | Linear, circular or triggered paths | Universal | Kinematic movement. Carry the player by adding the platform delta, plus momentum inheritance [S1]. |
| **Crumbling / falling platforms** | Collapse after a short delay once touched | Mega Man, Shovel Knight | [REC] 0.3–0.6 s delay with a shake tell, then a respawn after 2–3 s. |
| **Appearing blocks** | Blocks appear and vanish in a rhythm | Mega Man "Yoku" blocks | Run a visible cycle with an audio beat. Keep the pattern deterministic. |
| **Conveyor / wind** | Adds external velocity | Mega Man, Celeste | Add a velocity bias to the motor, not to the input. |
| **Pogo chains** | Bounce across enemies, bubbles or spikes | Shovel Knight, Hollow Knight | `Pogoable` tags, spaced 3–5 tiles apart [REC]. |
| **Cloudstep chains** | Hit lanterns or projectiles in the air to keep jumping | The Messenger [S12] | Target spacing 3–5 tiles [REC]. Show the target "lit" while it's available. |
| **Grapple routes** | Swing or pull between anchors | The Messenger, Super Metroid | Make anchors obvious. Avoid more than 3 swings in a row without a rest platform. |
| **Breakable environment** | Destroy blocks to open a path or find secrets | Shovel Knight dig piles, Metroid bomb blocks | One consistent tell (§5.7). |
| **Destructible-by-weapon type** | Only a specific weapon or ability breaks it | Metroid doors, Mega Man weapon blocks | Color-code it in the art. |
| **Era / world-state switch** | Toggle the world to change the layout | The Messenger [S12] | Two Tilemaps plus a flag (§5.5). |
| **Light / dark** | Light reveals platforms or blocks enemies | Common | URP 2D lights and a mask. |
| **Enemy-as-tool** | Freeze, push or ride an enemy to reach a spot | Metroid (freeze with the Ice Beam), Mega Man | Give enemies a "frozen" state they keep for 3–5 s [REC] that acts as solid ground. |
| **Projectile redirection** | Reflect a shot to hit a switch | Common | Hitbox on the reflectable layer. |
| **Ability combinations** | Must chain 2–3 abilities (for example wall jump → dash → pogo) | Hollow Knight late game, Super Metroid | Introduce each ability alone first, and only then combine them. |

### 6.3 Secret discovery design
- **Three tiers:**
  - **Visible-unreachable:** you see it and need a later ability.
  - **Hinted:** a crack, a breeze particle, an odd tile, or a coin trail leading off screen.
  - **Hidden:** fake walls and off-camera ledges, only for experts and completionists.
- Use **one tell vocabulary** across the whole game, and teach it once early on, with a guaranteed reward.
- Use **audio hints**: a faint chime near a secret, or a hollow sound when you hit a fake wall.
- **Rewards scale with how hard the secret is to find**: currency for easy secrets and an upgrade shard or lore for hard ones.

### 6.4 Difficulty ramps
| Stage | What changes | Example |
|---|---|---|
| 1. Isolation | One element, safe floor | A single moving platform over the ground |
| 2. Consequence | Add a pit or hazard | The same platform over spikes |
| 3. Timing | Add a rhythm or deadline | Two platforms out of phase |
| 4. Combination | Add a second element | Moving platforms plus a turret |
| 5. Mastery | Fewer safe spots, faster speed, or longer chains | 3 pogo targets over spikes, then a grapple |
| 6. Optional expert | Harder side route for a reward | A Celeste-style strawberry or a Shovel Knight broken-checkpoint run |

Rules:
- Raise **one** variable at a time.
- Put a checkpoint between stages 3 and 4 in hard sections.
- Let skilled players opt into difficulty (breakable checkpoints [S11], optional routes) instead of forcing it on everyone.

### 6.5 Puzzle state, reset and soft-lock rules [REC]
| Rule | Detail |
|---|---|
| **State model** | Each puzzle element (switch, door, block, platform) has an explicit state enum and a `Reset()` method. A `PuzzleController` per room owns the elements and their solved flag. |
| **On player death** | Reset all *unsolved* puzzles in the room to their start state. Solved puzzles stay solved. |
| **On hazard respawn** | Don't reset anything, so a spike hit doesn't undo progress (§5.8). |
| **On room exit** | Reset unsolved puzzles (for example, pushed blocks return). Solved puzzles persist through their `SaveId` (§5.9). |
| **On rest** | Same as room exit. Solved stays solved. |
| **Saving** | Save only the solved flag (and permanent results like an opened door), not mid-puzzle positions. |
| **Soft-lock escape** | Every puzzle room has an always-available exit or a visible **reset lever** that restores the start state. Check that no pushable or one-way configuration can trap the player. |
| **Timed doors** | Show the countdown visually and with ticking audio. Offer an assist option that extends or removes timers (§8.1). |
| **Determinism** | Puzzle timing (cycling blocks, moving platforms) runs on physics ticks from a room-start phase, so the pattern is the same on every attempt. |

---

## 7. Art Style and Asset Creation

### 7.1 Choose a pipeline
| Pipeline | Reference | Pros | Cons | Unity setup |
|---|---|---|---|---|
| **Pixel art (hand-pixeled)** | Shovel Knight, The Messenger, Mega Man | Strong retro identity, small files, consistent readability | Animating is slow, and camera and scale rules are strict | Point filter, no compression, Pixel Perfect Camera |
| **3D → pixel (rendered sprites)** | Dead Cells [S3] | Cheap animation retakes, reusable rigs, normal maps for lighting | Needs a toolchain. Can look "3D-ish" if the pixels aren't cleaned up. | Render small with no AA, then use normal maps with URP 2D lights |
| **Hand-drawn (raster)** | Hollow Knight [S7] | Painterly atmosphere, any scale, and fast for small teams when kept simple and monotone [S8] | Large textures. Frame-by-frame animation takes many drawings. | Bilinear filter, PPU 100 or the source density, compression allowed, Sprite Atlas (see the hand-drawn checklist in §7.10) |
| **Skeletal 2D (bone rigs)** | Many modern indies | Smooth, cheap animation | Can look "puppet-like" | Unity 2D Animation package (PSD Importer, bones) |
| **2.5D (3D models, side camera)** | Metroid Dread | Full lighting, dynamic camera | 3D production cost | Standard URP 3D |

**For an AI generating assets:** commit to one pipeline per project, and keep pixel-art and smooth hand-drawn sprites out of the same gameplay layer. Images from an image generator are **not** pixel art until they pass the cleanup step in §7.10.

### 7.2 Resolution and pixel-perfect rules
| Setting | [REC] / sourced value | Notes |
|---|---|---|
| Reference resolution (pixel art) | **320×180 or 640×360** | Both scale by a whole number to 720p, 1080p, 1440p and 4K. 384×216 and 480×270 don't scale evenly at 1440p. Celeste is 320×180 [SOURCED, S1]. Shovel Knight (400×240 at 4.5×, 5:3, non-pixel-perfect on PC [S2, S38]) is **not** an integer-scaling example. |
| Tile size | 16×16 px (most common), or 8×8 / 32×32 | Characters are usually 1.5–3 tiles tall. |
| Character height | 24–48 px for pixel art at 320–480 width | Dead Cells' hero is about 50 px [SOURCED, S3]. |
| PPU | **One project-wide PPU**, equal to the tile size (16 here) | [UNITY] The Pixel Perfect Camera's Assets PPU must match the sprites' PPU [SOURCED, S23]. Grid cell size stays 1. |
| Filter mode | Point (no filter) | [S23] |
| Compression | None | [S23] |
| Pivot | Characters: bottom-center, set in pixel units on a whole pixel. **Tiles: center (0.5, 0.5)**, which matches Tilemap's default Tile Anchor of (0.5, 0.5). | Prevents "swimming" between frames. If you want corner pivots on tiles, set every Tilemap's Tile Anchor to (0, 0). Pick one and don't mix them. |
| Mixels | **Never** mix pixel scales (for example a 2× sprite next to a 1× sprite) | Exception: deliberate UI or portrait art. |
| Rotation and scale (pixel art only) | Keep pixel sprites at scale ±1 and rotation 0. The root ±1 flip for facing (§3.1) is allowed. To rotate, use pre-rendered angles. | Rotated or scaled pixel art breaks up. Hand-drawn art may use transform squash and stretch and rotation. |
| Sub-pixel motion | Snap the rendered position to the pixel grid. Physics can stay at sub-pixel precision. | Pixel Perfect Camera "Upscale Render Texture" or snapping [UNITY]. |

[UNITY] URP 2D setup (Unity 6):
1. Install URP and create a 2D Renderer.
2. Add URP's built-in **Pixel Perfect Camera** to the main camera. **Don't install the separate "2D Pixel Perfect" package**, which only works with the Built-In Render Pipeline [S24].
3. Set Assets PPU to 16 and the Reference Resolution to 320×180 (or 640×360).
4. Choose the Crop Frame: None, Pillarbox, Letterbox, Windowbox or Stretch Fill [S24].
5. With Cinemachine 3, add the **`CinemachinePixelPerfect`** extension to every `CinemachineCamera` [S25].

### 7.3 Palettes
- **Use a limited master palette** of 32–64 colors for pixel art [REC]. Shovel Knight uses the NES 54 plus 4 "cheater" colors, added only to fill gaps: dark purple bridging black to cool colors, deep red bridging black to warm colors, a beige made only for Polar Knight's cloak and skin, and a light brown (#824E00) for darker skin tones [SOURCED, S2].
- **Colors per sprite:** 4–5 plus transparency in a Shovel Knight-style NES-plus look [SOURCED, S2]. A 16-bit (SNES / Messenger 16-bit) style allows 8–16 per sprite [REC].
- **Ramps:**
  - Build hue-shifted ramps. Shadows shift toward cool or purple and highlights toward warm or yellow. Don't just darken.
  - Use 3–5 steps per material [REC].
- **Per-biome palette:** 2–3 dominant hues plus a shared "gameplay palette" (the player, hazards and pickups) that stays constant across biomes.
- **Palette-swap shader:** store sprites as indexed (grayscale index) textures with a palette lookup texture. This gives enemy variants, damage flashes and color cycling cheaply, the way Shovel Knight does it [SOURCED, S2].

### 7.4 Readability: the most important art rule
**Value hierarchy (lightness), from highest contrast to lowest:**
1. **Player:** the highest contrast against every background, with a unique accent color. Mega Man's blue fit the NES palette [S2].
2. **Hazards and enemy attacks:** a distinct "danger" hue (often red, magenta or orange) used **only** for danger. Projectiles get a bright core and a dark outline.
3. **Enemies:** contrast with the midground, and use a consistent outline or rim treatment.
4. **Interactables** (grapple points, breakables, switches, pogo and cloudstep targets): a consistent shape language and a pulse or glint.
5. **Playable midground** (solid ground): a clear edge and medium contrast. The **top surface of a platform** should be lighter or highlighted.
6. **Background:** low contrast and desaturated, shifted toward the fog or sky color. It should **never** have the same value as the playable midground.
7. **Foreground decoration:** dark silhouettes, sparse, and **never** covering hazards. Fade it out when the player is behind it.

**Tests (run on every screen):**
- **Grayscale test:** desaturate the screenshot. Can you still tell the player, hazards, enemies and ground apart?
- **Squint or blur test:** blur by about 4 px. The silhouettes and the walkable path should still read.
- **Silhouette test:** fill the character with black. Can you still tell what it is and what its pose or attack is?
- **Color-blind check:** simulate protanopia and deuteranopia. Back up any red/green danger signal with a shape or animation.

**Rule:** collision must match the visuals. If it looks solid, it's solid. If it looks like background, it isn't. Use a consistent edge or lip on solid tiles.

### 7.5 Parallax layers
| Layer | [REC] scroll factor (relative to the camera) | Content | Contrast |
|---|---|---|---|
| Far sky | 0.0–0.1 | Gradient, moon, clouds | Lowest |
| Far background | 0.1–0.3 | Mountains, city skyline, landmark silhouette | Low |
| Mid background | 0.4–0.7 | Structures, trees, large shapes | Low-medium |
| **Gameplay layer** | 1.0 | Tiles, characters, enemies | Highest |
| Foreground | 1.1–1.5 | Pillars, leaves, chains (sparse) | Dark silhouette |

[UNITY] Parallax script: `layer.position = startPos + cameraDelta * (1 - factor)`, snapped to pixels for pixel art. Or use separate cameras or sorting layers. Use **Sorting Layers** named Background_Far, Background_Mid, Ground, Characters, FX and Foreground.

**Layer count:** Shovel Knight averages **5–6 background layers**, and Yacht Club says the parallax improved gameplay readability, not just looks [SOURCED, S2]. Use 3–6 background layers [REC].

**Atmosphere:** add fog or gradient overlays between layers (the Hollow Knight look). Atmospheric perspective means each layer further back shifts toward the sky or fog color.

### 7.6 Animation principles
| Principle | Application |
|---|---|
| **Anticipation** | Every attack and jump has a wind-up pose. **Player** wind-ups are short (2–5 ticks for light attacks, §4.2) to stay responsive. **Enemy** wind-ups are the telegraph and are much longer (0.3–1.2 s, §4.4). Hold the wind-up pose for the full telegraph length. |
| **Pose-to-pose** | Key the strongest poses first and time them. Add in-betweens only where needed (the Dead Cells rule [S3]). |
| **Smears and VFX arcs** | Fast attacks use smear frames or slash VFX instead of many in-betweens [S3]. |
| **Squash and stretch** | Stretch on the jump takeoff and squash on landing. Keep it subtle in pixel art (1–2 px) or apply it as transform scale on hand-drawn art. Keep the volume constant. |
| **Follow-through and overlap** | Hair, capes and scarves lag behind (The Messenger's scarf, Hollow Knight's cloak). |
| **Holds** | Hold the impact frame a little longer (hitstop) so it reads. |
| **Readable silhouettes in every frame** | Don't let limbs cross the body silhouette on key frames. |
| **Consistent timing grid** | Pixel animation is often authored "on 2s" (each drawing held 2 game frames at 60 fps). Pick one display rate per character. |

**[REC] Frame counts** for pixel art. These are starting points and none come from the reference games.
| Animation | Frames | Frame time |
|---|---|---|
| Idle | 4–8 | 100–150 ms |
| Run cycle | 6–10 | 60–100 ms |
| Jump (rise / apex / fall) | 1–2 / 1–2 / 1–2 | Driven by velocity |
| Land | 2–3 | 50–70 ms |
| Light attack (player) | 3–6 drawings (anticipation 1–2, active 1–2, recovery 2–3) | Timed to the tick data (§4.2), with startup 2–5 ticks |
| Heavy attack | 6–12 | Matches frame data |
| Hurt | 2–3 | Held during hitstop |
| Death | 6–12 | — |
| Wall slide | 2–4 | Loop |
| Dash | 2–4 plus afterimage VFX | — |
| Enemy telegraph | 2–4 drawings plus a hold frame | Hold for the telegraph length: 0.3–0.5 s for light and 0.6–1.2 s for heavy (§4.4) |

[UNITY]
- Drive the Animator from state machine parameters, or play states directly with `animator.Play(hash)` for precise action games. Avoid tangled transition graphs.
- Hitboxes come from the `WeaponDefinition` tick data in FixedUpdate (§3.1), not from Animation Events. Use **Animation Events** only for cosmetic things like footstep SFX, dust and VFX.
- For precise sync, set the Animator Update Mode to **Animate Physics**, and freeze it during hitstop (because `timeScale = 0`).
- Facing: flip the root with `localScale.x = ±1` (§3.1).
- For sprite swaps, use a single Sprite Library (2D Animation) for variants.

### 7.7 Lighting and VFX
- **URP 2D Lights [UNITY]:**
  - Global Light for the ambient level, plus Point and Freeform lights for local mood.
  - Enable normal maps on sprites that need volume (the Dead Cells approach: a rendered normal map plus a toon shader [S3]).
  - Shadow casters for dramatic scenes.
  - Keep gameplay readability above mood. The player should always be lit.
  - **Pixel art warning:** smooth 2D light falloff and bloom create sub-pixel gradients and off-palette colors on pixel art. Use banded light falloff (a stepped falloff texture or a toon ramp), or quantize the lit result to the palette in a post-process shader.
- **Bloom** (URP post-processing): use a low threshold only on emissive elements such as projectiles, pickups and magic. Too much bloom breaks pixel crispness.
- **VFX types:**
  - Impact sparks, dust on land, run, wall slide and dash.
  - Afterimages for the dash.
  - Slash arcs.
  - Ambient particles (spores, embers, rain, dust motes) per biome.
  - Death bursts.
- **Pixel VFX:** author them as sprite sheets in the same pixel grid and palette. Use particle systems with point-filtered textures and pixel-snapping, or a pixel-perfect render texture.
- **Blend modes:** additive for light and energy effects (Dead Cells used blend modes over exported frames [S3]).

### 7.8 Tilesets
- **Tile size consistency:** one grid, for example 16 px [REC].
- **Rule Tiles [UNITY]:** use the 2D Tilemap Extras package. Rule Tiles auto-place edges and corners. The minimum set is 47 tiles for full "blob" auto-tiling, or 16 for a simple set [REC].
- **Visual edges:** the top surfaces get a highlight lip or grass so "standable" is clear. Undersides are darker.
- **Variation:** use 2–4 variants per common tile (Random Tile or weighted Rule Tile) to avoid visible repetition.
- **Collision:**
  - Use a `TilemapCollider2D` with a `CompositeCollider2D` to make smooth merged shapes. In Unity 6, set the Tilemap Collider 2D's **Composite Operation = Merge** (older versions call it "Used By Composite"). The Rigidbody2D on the Tilemap is Static.
  - **Composite Geometry Type = Polygons** if the motor uses Overlap or depenetration checks (§3.1). "Outlines" produces only edges, so an overlap test from inside a solid returns nothing.
  - Use a separate Tilemap for one-way platforms and another for hazards.
  - Use one Grid per room prefab, with cell size 1 (PPU 16).
  - A Tilemap Renderer in **Chunk** mode needs its tiles in a Sprite Atlas. **Extrude tile sprites by 1–2 px** (padding in the atlas) to stop seams.
- **Decoration layers:** decorations go on a non-colliding Tilemap or as sprites, never on the collision Tilemap.

### 7.9 UI and HUD
- Show a **minimal HUD**: health, resource (Soul or magic), currency, and the equipped sub-weapon. Put them in the corners, away from the action.
- **The HUD style matches the game's pixel grid** (the same scale and palette). Hollow Knight uses hand-drawn masks for health, and Shovel Knight uses NES-style pips.
- **Boss HP:** a large bar at the bottom or top, named.
- **Damage numbers:** optional (Dead Cells shows them). They must not cover the player.
- **Map screen:** clear icons for saves, gates, the player's position, collectibles and notes.
- **Readable fonts:** use a pixel font at integer scale for pixel games. Text should be ≥ 8 px tall at reference resolution [REC].
- [UNITY] Use a Screen Space – Camera canvas with the pixel-perfect camera, or render the UI at native resolution with a pixel-font import at the right PPU.

### 7.10 Asset creation checklist (for the AI generating or reviewing art)

**Pixel cleanup step for AI-generated images (mandatory).** Image generators (Aura saves them to `Assets/Generated_Images/`) output anti-aliased, off-grid, off-palette images. Before one becomes a gameplay sprite:
1. Remove the background so it's fully transparent, with no semi-transparent fringe.
2. Downscale to the target pixel size (for example a 32×48 frame) with **nearest-neighbor** sampling.
3. Quantize to the **master palette** and enforce the per-sprite color limit (4–5 plus transparency for an NES-plus look, 8–16 for 16-bit).
4. Remove anti-aliasing, stray single pixels and jaggies by hand or with a script. Make the outlines a consistent 1 px (selout if that's the style).
5. Check the pivot, the frame size and the frame-to-frame alignment (no swimming).
6. Run the full checklist below, then move the cleaned file out of `Generated_Images` into the art folder with the correct name.

**Pixel art checklist:**
- [ ] It matches the project pipeline (pixel, hand-drawn or 3D→pixel). No mixels.
- [ ] Its size is on the grid. The PPU matches the one project-wide PPU (16 here).
- [ ] The pivot is bottom-center in pixel units (characters) or center (0.5, 0.5) to match the Tilemap's Tile Anchor (tiles).
- [ ] It uses only the master palette. The per-sprite color count is within the limit (for example 4–5 for an NES-plus look).
- [ ] The silhouette is readable in black fill.
- [ ] It passes the grayscale test against its intended background.
- [ ] Danger colors are used only for danger. Interactables use the interactable shape language.
- [ ] Animations have anticipation, key poses and holds, and match the gameplay tick data.
- [ ] Cosmetic animation events are marked (footstep, VFX). Hitbox timing lives in the `WeaponDefinition`.
- [ ] Import settings: Point filter, no compression, the correct sprite mode (Multiple) and slicing.
- [ ] Generated images went through the cleanup step above.
- [ ] It's in a **Sprite Atlas** grouped by biome or character [UNITY].
- [ ] A normal map, if 2D lighting is used.
- [ ] Naming: `chr_player_run_00.png`, `tile_sewer_ground.png`, `fx_hit_spark_00.png`, `bg_sewer_far.png`.
- [ ] A palette-swap variant, if it's an enemy with tiers.

**Hand-drawn checklist** (a separate pipeline, so don't apply the pixel rules):
- [ ] Authored at 1.5–2× display size. Line weight is consistent with the cast.
- [ ] PPU 100 or the project's source density, the same on every hand-drawn sprite.
- [ ] Bilinear filter. Mipmaps on only if the camera zooms out. High-quality compression allowed.
- [ ] Clean alpha edges (no halos). Silhouette and grayscale tests pass.
- [ ] Packed in a Sprite Atlas, with a normal map if lit.

**Import presets [UNITY]:** enforce these settings with a Unity **Preset** per art folder, or with an `AssetPostprocessor` that sets PPU, filter, compression, pivot in pixel units, and Multiple mode with slicing. Create these only when the designer asks. Aura's docs say it doesn't author AssetPostprocessors unless asked explicitly [S28b].

### 7.11 Spec tables [REC] (starting values only)
**Sprite sizes (16 px grid, 320×180 or 640×360 reference):**
| Asset | Size (px) |
|---|---|
| Player | 16–24 wide × 24–48 tall (frame canvas 32×48 or 48×48) |
| Small enemy | 16×16 – 24×24 |
| Medium enemy | 32×32 – 48×48 |
| Large enemy or elite | 48×64 – 64×64 |
| Boss | 64×64 – 160×128 (up to most of the screen height) |
| Tile | 16×16 |
| Pickup | 8×8 – 16×16 |
| Projectile | 4×4 – 12×12 |
| HUD icon | 8×8 – 16×16 |
| Parallax layer | Screen width × 1–2 (a seamless horizontal loop) |

**Hand-drawn (Hollow Knight-style), at source resolution:**
| Asset | Recommendation |
|---|---|
| Authoring resolution | Draw at 1.5–2× the display size and downscale on export |
| Line weight | Consistent across characters at the same display scale |
| Atlas texture | 2048 or 4096 square, with tight packing |
| PPU | 100, or the source pixel density (for example 128 if one 128 px tile = 1 unit). Keep it the same for every hand-drawn sprite. |
| Filter | Bilinear |
| Mipmaps | Off for sprites shown at a fixed scale. On if the camera zooms out or sprites are shown smaller. |
| Compression | Allowed (for example ASTC or BC7 at high quality). Check gradients and edges for artifacts. |

---

## 8. Audio and Feedback (brief)
| Element | Rule |
|---|---|
| **SFX priority** | The player's actions and danger tells come first, so they must never be masked by music or ambience. |
| **Tells in audio** | Every enemy attack has a wind-up sound. Pursuers have a signature sound (Dread's E.M.M.I. call was based on a bird call, and "sounds that are only scary in certain situations" [SOURCED, S19]). |
| **Variation** | Randomize pitch ±5–10% and keep 2–4 variants per repeated SFX (footsteps, hits) [REC]. |
| **Hit layering** | Impact = transient (snap) + body (thud) + tail (debris). Heavy weapons add bass [S22]. |
| **Music** | One theme per biome. Save rooms get a calm theme. Bosses get their own track. The Messenger keeps an 8-bit and a 16-bit version of every track for its era shift [SOURCED, S12]. Use synced stems so era or intensity changes can crossfade. |
| **Ducking** | Duck the music briefly on big hits, boss roars and pickups (Unity Audio Mixer snapshots or sidechain). |
| **Haptics** | Rumble on hurt, heavy hits and landing from a big fall. Make it optional. |
| **Feedback stack** | Every meaningful event gets visual + audio + (optionally) camera and haptics feedback. See Vlambeer's juice list [S22]: animation, impact effects, hit flashes, knockback, permanence (debris), camera shake, hit pause, bass. |

### 8.1 Accessibility [REC]
Celeste's Assist Mode is the model to follow [VERIFY the exact Celeste option list]: options that change difficulty without locking content.
| Option | Detail |
|---|---|
| Game speed | 50–100% in 10% steps. Scales `Time.timeScale`, while the tick-based tunables stay correct. |
| Extra or infinite i-frames | A longer invulnerability after hits, or invincibility |
| Infinite air dash / stamina | Removes the air-dash count and any stamina limit |
| Timer extension | Extend or remove puzzle and door timers (§6.5) |
| Input | Fully remappable controls (Input System rebinding), with hold vs. toggle options |
| Screen shake | A slider from 0 to 100% |
| Flashes | Reduced-flash mode: replace the i-frame blink and hit flashes with an outline or tint. Avoid large full-screen flashes above 3 per second (photosensitivity). |
| Readability | Color-blind-safe danger colors backed by shape and animation (§7.4), and a text size option for UI |
| Audio cues | Subtitles or visual indicators for important audio tells (off-screen pursuer sounds) |

---

## 9. Implementation Checklists and Common Pitfalls

### 9.1 Project setup [UNITY]
- [ ] The engine stack is pinned (top of doc): Unity 6, URP 17 with a 2D Renderer, Cinemachine 3, Input System 1. Use Unity 6 API names (`linearVelocity`, `bodyType`, `FindFirstObjectByType`).
- [ ] URP's built-in Pixel Perfect Camera for pixel art (not the separate package) [S24].
- [ ] Packages: 2D Tilemap Editor, 2D Tilemap Extras, 2D Sprite, 2D Animation (optional), Cinemachine 3, Input System.
- [ ] One project-wide PPU (16 here), with every sprite imported at it, enforced by import presets (§7.10).
- [ ] Project Settings → Physics 2D → **Queries Start In Colliders off** (§3.1).
- [ ] Physics2D layers and a collision matrix defined (§3.1).
- [ ] Sorting layers defined (§7.5).
- [ ] `Time.fixedDeltaTime = 1f/60f` (0.016667; Unity's default is 0.02 = 50 Hz), with interpolation on the player.
- [ ] A `PlayerStats` ScriptableObject plus Play-mode tuning.
- [ ] A debug overlay: show colliders, velocity, state, the coyote and buffer timers, and the frame-step key.
- [ ] A `MetricsGym` scene built before any levels (§5.6).
- [ ] A save system with `SaveId` components and a `WorldState` (§5.9).
- [ ] Accessibility options planned (§8.1).

### 9.2 Player controller checklist
- [ ] Gravity is derived from the jump height and apex time.
- [ ] Fall gravity multiplier and a terminal velocity cap.
- [ ] Variable jump height (jump cut).
- [ ] Coyote time and a jump buffer. Presses are read in Update and consumed in FixedUpdate.
- [ ] The ground check needs `vy ≤ 0` and ignores ground for 2 ticks after a jump, so coyote time isn't re-armed on takeoff.
- [ ] Movement uses `MovePosition` with a skin width, plus depenetration after platforms move.
- [ ] Apex hang (optional).
- [ ] Corner correction (head and ledge).
- [ ] One-way platforms with drop-through (down + jump).
- [ ] Slopes, if used: 22.5° and 45° only, no sliding when idle, and stick to the ground going downhill (§5.6).
- [ ] Ladders, if used: snap to center, snap to standing at the top.
- [ ] Facing uses a root `localScale.x = ±1` flip.
- [ ] Moving platforms carry the player, and the player inherits momentum.
- [ ] Dash, wall slide and jump, ledge grab, and pogo, as the design requires.
- [ ] Hurt state, knockback, i-frames and the i-frame visual.
- [ ] Hazard respawn to the last safe ground (small damage) is separate from a death respawn at the checkpoint (§5.8).
- [ ] Respawn at the checkpoint, with state reset (velocity, buffers, abilities refreshed).

### 9.3 Combat checklist
- [ ] Separate hitbox and hurtbox layers. Each `attackId` hits each target once (§4.2).
- [ ] Frame data as tick counts in `WeaponDefinition`, with hitboxes enabled from FixedUpdate. Animation Events are for cosmetics only.
- [ ] The hitstop service merges overlapping requests and enforces the cap (≤ 0.15 s per 1 s of combat).
- [ ] Damage feedback stack (§4.9).
- [ ] Damage model: HP in hits, poise, resistances and statuses (§4.5).
- [ ] Enemy telegraphs on 3 channels (pose, flash, sound).
- [ ] Enemy AI state machine with attack tokens (§4.4).
- [ ] Enemy AI wakes 2–4 tiles off screen, and attacks start only after 0.3–0.5 s on screen.
- [ ] Boss phases, punish windows, a checkpoint outside, and a skippable intro. HP is derived from a 60–150 s target time-to-kill.
- [ ] One healing model, refilled at rest points (§4.10).

### 9.4 Level checklist
- [ ] Gaps and ledges use the level geometry metrics (§5.6) with a safety margin of 1 tile horizontally and 0.5 tile vertically, checked in the metrics gym.
- [ ] Every room has a camera confiner, and rooms with 2 or more exits have transitions.
- [ ] No leaps of faith. Hazards are visible with ≥ 0.5 s of reaction time.
- [ ] Checkpoints before spikes in difficulty and outside bosses.
- [ ] The dependency graph passes the editor reachability validator (§5.2). There are no soft-locks (for example, the player can always get back out of a pit room).
- [ ] Every persistent object has a unique `SaveId`. Reset rules are documented (§5.9).
- [ ] Every puzzle room has a reset lever or an always-available exit (§6.5).
- [ ] Each region has a fast-travel node and a map station (§5.2).
- [ ] Each region has a landmark, a palette, music and a signature mechanic.
- [ ] Secrets follow the single tell vocabulary.

### 9.5 Common pitfalls and fixes
| Pitfall | Symptom | Fix |
|---|---|---|
| Floaty jump | Feels like the moon | Raise the fall gravity multiplier (1.2–2.0×, default 1.5), shorten the time to apex, and cut the jump on release |
| Free double jump on takeoff | The player can jump again right after jumping | The ground check needs `vy ≤ 0` and ignores ground for 2 ticks after a jump (§3.1) |
| Missed jump presses | The jump sometimes doesn't fire | Read presses in Update with timestamps and consume them in FixedUpdate. Never poll presses in FixedUpdate. |
| Attack timing drifts | Hits land at different times at different frame rates | Drive hitboxes from tick data in FixedUpdate, not from Animation Events |
| Wrong Unity API | Compile errors, or `velocity` is obsolete | Use Unity 6 names (`linearVelocity`, `bodyType`) and Cinemachine 3 names (`Unity.Cinemachine`) |
| Raw generated art | Blurry, off-palette, anti-aliased sprites | The pixel cleanup step (§7.10) before import |
| Tiles offset by half a tile | Tiles don't line up with the grid | Tile pivots at center (0.5, 0.5) to match Tilemap's Tile Anchor, or set the anchor to (0, 0) everywhere |
| Secrets close again | Broken walls reappear after loading | `SaveId` plus `WorldState` (§5.9) |
| Spike rooms feel punishing | Players replay a whole section after one spike | Hazard respawn to the last safe ground (§5.8) |
| Mushy controls | Slow to start or stop | Near-instant ground accel and decel. Check for input lag (poll in Update). |
| Unfair deaths at edges | "I pressed jump!" | Coyote time, a jump buffer and corner correction [S1] |
| Physics jitter | The sprite shakes while the camera follows | Rigidbody2D interpolation. Camera in LateUpdate, or Cinemachine with the correct update method. Pixel snap. |
| Pixel art shimmer or blur | Lines wobble when moving | Pixel Perfect Camera, Point filter, no compression, the same PPU everywhere [S23] |
| Mixels | Sprites at different pixel densities | Enforce one PPU and keep pixel sprites at scale ±1 |
| Tilemap seams or gaps | Thin lines between tiles | Sprite Atlas with padding, Point filter, pixel snapping |
| Stuck on tile edges | The player catches on seams between flat tiles | Composite Collider 2D (Composite Operation = Merge, Geometry Type = Polygons) |
| Background confused with ground | The player jumps onto scenery | The value hierarchy and grayscale test (§7.4) |
| Off-screen hits | Damage from unseen enemies | AI wakes 2–4 tiles off screen, attacks start only after 0.3–0.5 s on screen, plus look-ahead and telegraphs (§4.4) |
| Random boss chaos | The player can't learn the fight | Recognizable moves, consistent tells, punish windows [S12] |
| Backtracking fatigue | Long empty revisits | Shortcuts, fast travel, new content on old routes after an upgrade |
| Feature fights the core verb | Players skip the core mechanic | Cut it (The Messenger cut the sword beam because it hurt cloudstepping [S12]) |
| Over-juiced | Constant shake, players get numb to it | Save screen shake for heavy, crit and kill hits. Add an accessibility slider. |
| Cooldown friction | Movement feels throttled | Refresh resources through play (landing, hitting) instead of long timers [S12]. Keep only the short anti-spam minimum (0.15–0.3 s, §3.5). |
| Hitstop stutter | Rapid weapons freeze the game constantly | The hitstop cap, or per-entity hitstop (§3.10) |
| Procedural sameness | Every run feels identical | A per-biome concept graph, purpose-tagged rooms, biome-specific rooms [S5] |
| Procedural nonsense | Broken connections, unreachable rooms | Socket validation, then a reachability check (flood fill with the player's metrics) after generation |
| Art before fun | Rework when the mechanics change | Greybox until fun [S12]. Use a 3D→pixel or skeletal pipeline if retakes will be frequent [S3]. |

---

## 10. Sources
Developer sources are primary. Analysis videos and articles are secondary. Wiki and fan sources are tertiary, so verify them before relying on them. **Sources S24–S44 were added from the expert review (Nov 12, 2026).** The reviewer checked them; this revision didn't re-open every one.

| ID | Source | Used for |
|---|---|---|
| S1 | Maddy Thorson, "Celeste & Forgiveness" (2020): https://www.maddymakesgames.com/articles/celeste_and_forgiveness/ (original thread: https://twitter-thread.com/t/1238338574220546049) | Coyote time, jump buffer, half-gravity apex, corner correction, lift momentum, 2 px wall-jump window, 320×180 |
| S2 | Yacht Club Games, "Breaking the NES" (2014): https://www.yachtclubgames.com/blog/breaking-the-nes/ (mirror: https://www.gamedeveloper.com/design/breaking-the-nes-for-shovel-knight) | NES 54 colors + 4 extra hex values (beige for Polar Knight only), 4–5 colors per sprite, 400×240 virtual resolution at 4.5×, i-frame flashing vs. palette cycling, 5–6 parallax layers, Mega Man's color |
| S2b | Code Like It's 198x, "Shovel Knight": https://code198x.com/vault/games/shovel-knight/ | Secondary. Its 400×240 figure is now confirmed by S2 itself. |
| S2c | Engadget, "Why Shovel Knight couldn't run on a real NES" (2014): https://www.engadget.com/2014-06-27-why-shovel-knight-couldnt-run-on-a-real-nes.html | NES vertical resolution, 4.5× pixels at 1080p, 16:9 |
| S3 | Thomas Vasseur, "Art Design Deep Dive: Using a 3D pipeline for 2D animation in Dead Cells" (2018): https://www.gamedeveloper.com/production/art-design-deep-dive-using-a-3d-pipeline-for-2d-animation-in-i-dead-cells-i- | 3D→pixel pipeline, ~50 px hero, no AA, normal maps and toon shader, pose-to-pose, blend modes. (Its "30 fps" line refers to the ScarKrow prototype, not Dead Cells.) |
| S4 | Game Anim, "Dead Cells: A 3D Pipeline For 2D Animation": https://www.gameanim.com/2018/01/31/dead-cells-3d-pipeline-2d-animation/ | Supporting S3 |
| S5 | Sébastien Bénard, "Building the Level Design of a procedurally generated Metroidvania: a hybrid approach": https://www.gamedeveloper.com/design/building-the-level-design-of-a-procedurally-generated-metroidvania-a-hybrid-approach- (also https://deepnight.net/tutorial/the-level-design-of-dead-cells-a-hybrid-approach/) | Fixed frame, handmade rooms, concept graph, monster budget |
| S6 | Ars Technica, "War Stories: Designing Dead Cells" (2019): https://arstechnica.com/gaming/2019/07/war-stories-designing-dead-cells-was-a-marriage-of-man-and-machine/ | Supporting S5 |
| S7 | Unity, "Hollow Knight – Made with Unity": https://unity.com/made-with-unity/hollow-knight | Hand-drawn Photoshop PNGs, Sprites-Default and Diffuse shaders |
| S8 | Nintendo Life, "Bugging Out With Hollow Knight's Team Cherry" (2018): https://www.nintendolife.com/news/2018/01/feature_bugging_out_with_hollow_knights_team_cherry, and MCV/DEVELOP, "When We Made… Hollow Knight": https://mcvuk.com/development-news/when-we-made-hollow-knight/ | "Simple is fast. Monotone is fast." Hand-drawn for simplicity |
| S9 | Game Developer, "Designing each of the 50 weapons in Dead Cells to feel distinctive" (2017): https://www.gamedeveloper.com/design/designing-each-of-the-50-weapons-in-i-dead-cells-i-to-feel-distinctive | Weapon gimmicks (the below-50% crit sword is a hypothetical example), fighting-game feedback, 1-frame crit freeze plus slowdown, extra frames for weight |
| S10 | Game Developer, Sébastien Bénard Dead Cells design stream (2018): https://www.gamedeveloper.com/design/sebastien-benard-breaks-down-the-fine-details-of-i-dead-cells-i-design | Video, supporting |
| S11 | Yacht Club Games, "Check Point Design" (2014): https://www.yachtclubgames.com/blog/check-point-design/ | Breakable checkpoints, 3 hits, flame color = value, tally screen |
| S11b | Game Informer, "Shovel Knight Devs Discuss Co-Op, Pogo Jumping, And Beyond" (2014): https://gameinformer.com/b/features/archive/2014/07/29/shovel-knight-devs-discuss-co-op-pogo-jumping-and-beyond | Pogo (not read in full here, [VERIFY] specifics) |
| S12 | The Geekly Grind, "Perfecting The Ninja Way: A Conversation With The Messenger Development Team" (2018): https://www.thegeeklygrind.com/all-posts/perfecting-the-ninja-way-a-conversation-with-the-messenger-development-team | "Frictionless," no cooldowns, cloudstep definition, cut sword beam, boss philosophy, 8/16-bit collision sets and stencils, dual music |
| S13 | GamesBeat, "The Messenger is the ninja Metroidvania that leaps from 8-bit to 16-bit art" (2018): https://gamesbeat.com/the-messenger-is-the-ninja-metroidvania-that-leaps-from-8-bit-to-16-bit-art/ | Time-travel era shift, NES→SNES analogy, rope dart and wingsuit |
| S14 | Game Maker's Toolkit, "Super Mario 3D World's 4 Step Level Design": https://www.youtube.com/watch?v=dBmIkEvEBtA | Kishōtenketsu introduce, develop, twist, conclude |
| S15 | Game Maker's Toolkit, "The World Design of Hollow Knight (Boss Keys)": https://www.youtube.com/watch?v=7ITtPPE-pXE, and Unwinnable, "Cartography is Hollow Knight's Greatest Reward": https://unwinnable.com/2017/06/29/cartography-is-hollow-knights-greatest-reward/ | Staged maps, bench-commit mapping, sparse benches |
| S16 | Egoraptor, "Sequelitis – Mega Man Classic vs. Mega Man X": https://youtu.be/8FpigqfcvlM, and analysis: http://vincentpaquin.blogspot.com/2012/10/sequelitis-analysis-of-tutorial-by-gamer.html | Teaching through the X intro stage |
| S17 | Game Maker's Toolkit, "The World Design of Super Metroid (Boss Keys)": https://www.youtube.com/watch?v=nn2MXwplMZA | Ability keys, dependency charts, sequence breaks |
| S18 | Game Developer, "Looking back at the level design triumphs of Super Metroid" (2018): https://www.gamedeveloper.com/design/looking-back-at-the-level-design-triumphs-of-i-super-metroid-i- | Gating signals, dependency chart |
| S19 | Nintendo Everything, translation of Famitsu's Yoshio Sakamoto interview (2022): https://nintendoeverything.com/metroid-dread-hd-emmi-interview-yoshio-sakamoto/ | E.M.M.I. "Dread Zones," steam and static visuals, sound design, dash melee origin, Alien/Giger influence |
| S20 | Official Dead Cells Wiki, "Stats": https://deadcells.wiki.gg/wiki/Stats | Brutality, Tactics and Survival colors, ×1.15 compounding per point, HP multipliers, dual-color = higher stat, Scrolls of Power, 4 fragments = +1 (tertiary) |
| S21 | Keiji Inafune interview (Play Magazine, 2004) via The Mega Man Network: https://themmnetwork.com/interviews/interviews-the-best-damn-mega-man-feature-period-play-magazine-042604/, and shmuplations, "The Birth of Mega Man": https://shmuplations.com/megaman/ | Rock-paper-scissors weakness design |
| S22 | Jan Willem Nijman (Vlambeer), "The Art of Screenshake," INDIGO 2013: https://www.youtube.com/watch?v=AJdEqssNZ-U | Juice list: hit pause, shake, knockback, permanence, bass |
| S23 | Unity Manual, "Add a pixel perfect camera" (URP): https://docs.unity3d.com/Manual/urp/2d-pixelperfect-prep-sprites.html | Asset PPU, reference resolution, Point filter, no compression. (The separate 2D Pixel Perfect package link was removed. That package is for the Built-In pipeline only.) |
| S24 | Unity 6 Manual, "Pixel Perfect Camera component reference" (URP): https://docs.unity3d.com/6000.0/Documentation/Manual/urp/2d-pixelperfect-ref.html | URP's built-in Pixel Perfect Camera, don't install the separate package, Crop Frame options |
| S25 | Cinemachine 3.1 manual, "Cinemachine Pixel Perfect": https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachinePixelPerfect.html | CM3 class names and namespace, pixel-perfect extension per camera, blends |
| S26 | Unity 6 Scripting API, `Rigidbody2D.linearVelocity`: https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/rigidbody2d/linearvelocity | Unity 6 API rename |
| S27 | Bezi docs, "Rules": https://docs.bezi.com/context/rules, and "Pages": https://docs.bezi.com/context/pages | Short, constant, positive rules; Pages for long reference |
| S28 | Aura docs, "Advanced settings": https://www.tryaura.dev/documentation/advanced-settings/ | Custom Instructions and project memory |
| S28b | Aura docs, "Unity editor agent": https://www.tryaura.dev/documentation/unity-editor-agent/ | Generated images go to `Assets/Generated_Images/`. AssetPostprocessors only on explicit request. Cinemachine and Timeline assets are inspect-only. |
| S29 | Hollow Knight Wiki, "Map and Quill": https://hollowknight.wiki/w/Map_and_Quill_(Hollow_Knight) | Bench map commit needs the Quill and the area map; Cornifer and Iselda |
| S30 | Hollow Knight Wiki, "Mothwing Cloak": https://hollowknight.wiki/w/Mothwing_Cloak | Dash refresh on pogo, landing and wall cling; 0.6 s cooldown (0.4 s with Dashmaster) |
| S31 | Hollow Knight Wiki, "Monarch Wings": https://hollowknight.wiki/w/Monarch_Wings | Double-jump refresh |
| S32 | Hollow Knight Wiki, "Steady Body": https://hollowknight.wiki/w/Steady_Body | Nail recoil exists (the charm removes it) |
| S33 | Mega Man Wiki, "Mega Buster (Mega Man 1)" data: https://www.megamanwiki.com/wiki/Data:Mega_Buster/Mega_Man_1 | 3 shots on screen |
| S34 | The Mega Man Network, "Get your weapons ready" (2014): https://themmnetwork.com/2014/03/17/get-your-weapons-ready-mega-man-nes/ | Shot limits |
| S35 | Mega Man Wiki (Miraheze), "Dash": https://megaman.miraheze.org/wiki/Dash | X's dash comes from the Leg Parts, has no i-frames, lowers the hitbox; armor exceptions |
| S36 | TASVideos, "Mega Man X game resources": https://tasvideos.org/GameResources/SNES/MegaManX | Dash behavior |
| S37 | StrategyWiki, "Mega Man X / Chill Penguin": https://strategywiki.org/wiki/Mega_Man_X/Chill_Penguin | Leg Parts location |
| S38 | Steam discussion with a Yacht Club developer on Shovel Knight PC scaling: https://steamcommunity.com/app/250760/discussions/0/46476145015440549 | Non-pixel-perfect scaling on PC |
| S39 | Dead Cells Wiki, "Zombie": https://deadcells.wiki.gg/wiki/Zombie | The zombie's leap does damage |
| S40 | Dead Cells Wiki, "Mechanics": https://deadcells.wiki.gg/wiki/Mechanics (with S39) | Touching an enemy's body generally doesn't hurt |
| S41 | Metroid Wiki, "Melee Counter": https://metroid.fandom.com/wiki/Melee_Counter | Debuted in Samus Returns; Dread adds the Dash Melee and an aerial counter |
| S42 | Celeste public source, `Player.cs` (NoelFB): https://raw.githubusercontent.com/NoelFB/Celeste/master/Source/Player/Player.cs | Calibration constants (§3.4) |
| S43 | AtomechLLC, "jump-test-classic": https://github.com/AtomechLLC/jump-test-classic | Mega Man 2 movement numbers (community) |
| S44 | GameFAQs, Mega Man 9 board, physics thread: https://gamefaqs.gamespot.com/boards/950625-mega-man-9/46926500 | Mega Man movement numbers (community) |

**Note for the AI:** values tagged **[REC]** are this document's own recommended starting values, based on common practice. They are not measured from the reference games. **[SOURCED-wiki]** values come from community wikis, TAS resources or public source code, so cite them as such. Values tagged **[VERIFY]** are unconfirmed. Never state them as fact; ask the designer.
