# Bastion Zero spec — Master glossary (generic terms)

> Part of the Bastion Zero clean-room mechanics spec v3.2 (14 Nov 2026). Neutral terms used across the spec. Use these names in code and UI strings.
> Precedence: user decisions (file 01) > owning system > later system. Tags: [sourced], [community-measured], [estimated] (file 01).

---

## 2. Master glossary (generic terms)
One definition per term. "Defined in" names the system that owns the term; later systems use it unchanged unless an X or CR row says otherwise.

| Term | Definition | Defined in |
|---|---|---|
| Tile | Grid unit of the world; 16 [estimated] px | SYS1 |
| H | Hero height unit; 2 [estimated] tiles | SYS1 |
| Hero | The player character | SYS1 |
| Movement state | One node of the player movement state machine (G-*, A-*, W-*, X-*, H-*, D-*) | SYS1 |
| Attack overlay | Attack layer that runs on top of movement states and never blocks movement | SYS1 (rule), SYS3 (states) |
| Held rise | Upward phase while jump is held, at constant speed up to a max hold time | SYS1 |
| Apex band | Speed band near the top of a jump where gravity is reduced | SYS1 |
| Coyote time | Grace period to jump after leaving a ledge | SYS1 |
| Jump buffer | Grace period in which an early jump press still fires on landing | SYS1 |
| Bounce-attack | Airborne attack that grants an air-jump when it hits a qualifying target | SYS1 |
| Qualifying target | Something whose airborne hit grants an air-jump: an enemy damaged by the hit, an anchor, a projectile after its unlock, a falling ceiling hazard (X7); never a shield-blocked hit or terrain | SYS1, refined SYS2 (X7), SYS4 (decision j), SYS5 §7.2 |
| Stored air-jump | The single air-jump held after a bounce grant | SYS1 |
| Bounce anchor | Placed, usually harmless target used to chain bounce-attacks | SYS1 (concept), SYS4 (F-A, R3) |
| Glide | Slow-fall traversal state with horizontal control | SYS1 |
| Updraft | Vertical force volume that lifts a gliding hero | SYS1 |
| Grapple | Horizontal hook-and-pull traversal to hook points | SYS1 |
| Wall-cling | Holding onto a cling-able wall without stamina | SYS1 |
| Fast-fall | Optional faster fall cap; off by default | SYS1 |
| Safe ground | Last solid, non-hazard ground the hero stood on for the dwell time; warp and marker target | SYS1 (recorder), SYS2 (use) |
| Collision box | Box used for movement against the world | SYS2 |
| Hurtbox | Box that receives damage | SYS2 |
| Hitbox | Box that deals damage | SYS2 |
| One-way platform | Platform passable from below and droppable through | SYS2 |
| Collapsing platform | Platform that shakes, falls and comes back (D2-5) | SYS2 |
| Pip | One unit of player health | SYS2 |
| Damage class | Fixed pip amount per hit: D-Light, D-Heavy, D-Hazard, D-Pit, D-Lethal | SYS2 |
| Kill plane | Out-of-bounds volume that warps the hero | SYS2 |
| Contact damage | Damage from touching an enemy body; on by default (D2-6) | SYS2 |
| Knockback | Deterministic push away from the damage source; state H-KNOCK | SYS1 (state), SYS2 (rules) |
| Post-hit i-frames | Invulnerability after taking damage; hazards ignore it | SYS1, SYS2 |
| Hazard re-hit lockout | Short window in which the same hazard instance can't hit again (D2-2) | SYS2 |
| Warp | Fade and teleport to safe ground after a pit or warp hazard | SYS1 (H-WARP), SYS2 |
| Death sub-states | D-1 freeze, D-2 animation, D-3 fade, D-4 respawn, D-5 grace | SYS2 |
| Lethal-hit guard | Assist-only option that survives a hit that would kill from a high pip count | SYS2 |
| Slot | Loadout position: A primary blade, B secondary weapon, C/D active skills | SYS3 |
| Ground chain | Three-hit primary combo G1–G3 | SYS3 |
| Startup / active / recovery | Frames before, during and after a hitbox is live | SYS3 |
| Combo memory | Time after a chain hit during which the chain can continue | SYS3 |
| Dash | Core defensive and traversal burst C-DASH: facing direction, standing hurtbox, enemy-only i-frames on f3–f10 (DB-1); on the ground from the start | SYS3 |
| Air dash | The dash used in the air; a later traversal unlock (DB-3) that opens GT17; refreshed by landing, W-CLING/W-JUMP or a bounce grant (X11) | SYS3, SYS5, SYS6 |
| Ledge dash | A ground dash that runs off a ledge; no jump may follow it (DB-3; X28) | SYS3 |
| Block | Core held guard C-BLOCK: front 180°, ground only; costs guard points; never stops hazards | SYS3 |
| Guard points | Block meter; a blocked D-Light costs 1, D-Heavy 2; 0 → guard break | SYS3 |
| Chip damage | The 1 [estimated] pip a blocked D-Heavy still deals; never takes the last pip | SYS3 |
| Guard break | BK-BREAK: a short helpless state when guard points reach 0 | SYS3 |
| Unblockable attack | Enemy attack marked with a distinct sign colour that the block and the parry don't stop | SYS4 |
| Secondary weapon archetype | Slot B type: heavy or ranged (shield removed, decision b) | SYS3 |
| Parry | Perfect-timing block (BK-PARRY): a hit inside the first 8 f after the raise is negated at no guard cost, gives poise damage, and reflects projectiles (DB-2) | SYS3 |
| Skill | Slot C/D action on its own cooldown timer | SYS3 |
| Cooldown class | Short, medium or long skill cooldown | SYS3 |
| Hit-stop | Brief local freeze of attacker and target on a hit | SYS3 |
| Screen shake | Camera displacement feedback, with on/off and intensity options | SYS3 |
| Poise | Per-enemy stagger meter that rises with hits and decays over time | SYS3 |
| Stagger | Poise-break interruption; enemy contact off | SYS3, SYS4 |
| Flinch | Light enemies' interruption on every hit | SYS3 |
| Critical hit (crit) | Deterministic ×2 [estimated] hit from a listed trigger; no random crits | SYS3 |
| Status | Enemy-side effect: stun, slow, burn, bleed (decision e) | SYS3 |
| Weapon tier | Fixed damage multiplier step T1–T5 | SYS3 (values), SYS6 (bought for coin, X20) |
| HP band | Enemy health class: fodder, soldier, brute, elite, boss | SYS3 |
| Archetype | Enemy behaviour family: patroller, ranged, charger, flyer, shielded | SYS4 |
| Elite | Modifier on any archetype: tougher, larger, one authored modifier | SYS4 |
| Elite modifier | One authored special behaviour per elite (M1–M6) | SYS4 |
| Telegraph | Wind-up before an attack, with the universal attack sign | SYS4 |
| Attack sign | Universal warning symbol shown at telegraph start; on by default (decision g) | SYS4 |
| Attack token | Shared cap on enemies telegraphing or attacking at once | SYS4 |
| Leash | Distance from home after which an enemy gives up | SYS4 |
| Dash/block-fit rule | Rule that every enemy attack is blockable and parryable from the front, or marked unblockable and avoidable by a jump, a dash out of reach, or a dash through on the i-frames | SYS4 |
| Respawn class | R0 persistent, R1 rest/death, R2 re-entry, R3 off-camera anchor | SYS4 |
| Threat points | Encounter cost unit per enemy, with surcharges | SYS4 |
| Progression tier | T1–T4 difficulty band of a room or region | SYS4 |
| Boss phase | HP-threshold section of a boss fight | SYS4 |
| Room | One loaded space with camera bounds; graph node | SYS5 |
| Link | Door, opening or drop between rooms; graph edge | SYS5 |
| Region | Cluster of rooms with one theme and hazard family | SYS5 |
| Hub | Room with degree ≥ 4 [estimated] | SYS5 |
| Gate | Link that needs an ability, key, switch or cleared fight | SYS5 |
| Sequence-break tolerance | SB0 must hold, SB1 skill skip allowed, SB2 open | SYS5 |
| Corridor act | Opening linear world phase (WP-CORRIDOR) before the world opens (WP-OPENING → WP-OPEN) | SYS5 |
| Opening event | The trigger that ends the corridor act (still open, OQ-H1) | SYS5, SYS6 |
| Secret | Hidden pocket or room behind a breakable or false wall, or a hidden route | SYS5 |
| Shortcut | Link opened from the far side that shortens a return route | SYS5 |
| Transition | Fade-out, load, place, fade-in sequence between rooms | SYS5 |
| Transition lift | Small upward boost so upward exits reach a ledge (X18) | SYS5 |
| Look-ahead | Camera offset toward the travel direction (decision n) | SYS5 |
| Dead zone | Camera window in which hero movement doesn't move the camera | SYS5 |
| Hybrid map | Dotted sketch on entry, inked at the next rest (decision l) | SYS5 |
| Rest point | Save point that heals, refills, respawns R1, inks the map, refreshes cooldowns | SYS5, extended SYS6 |
| Checkpoint | Save point that sets respawn only (plus a partial save write) | SYS5, extended SYS6 |
| Fast-travel node | Rest point from which the hero can travel to any found node | SYS5, SYS6 |
| Coin | The single persistent currency (decision r) | SYS6 |
| Wallet | Unspent coin, the only thing at risk on death | SYS6 |
| Cache | One-time coin container, small or large | SYS6 |
| Challenge room | Optional hard traversal room with a fixed reward | SYS5 (SC6), SYS6 |
| Shop / shop door | The one shop, entered through doors at rest points (decision t) | SYS6 |
| Stock tier | ST1–ST4 sets of shop items unlocked by world progress | SYS6 |
| Death marker | Recoverable object holding the dropped coin (decision s) | SYS6 |
| Flask | Fixed-charge healing item refilled at rest (decision u) | SYS6 |
| Pip fragment | Quarter of a max pip; 4 [estimated] make one (basis: HK model, S86) | SYS6 |
| Persistence class | P0 permanent, P0-W wallet, P1 rest-scoped, P2 room-scoped, P3 death-scoped | SYS6 |
| Technique | Non-gate-critical move that may be bought | SYS6 |
| Trinket / notch | Deferred post-v1 passive items and their capacity slots (decision x) | SYS6 |
| Completion % | Weighted sum of collected categories, capped at 100 [estimated] | SYS6 |

---

