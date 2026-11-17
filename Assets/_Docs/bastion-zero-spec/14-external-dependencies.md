# Bastion Zero spec — Dependencies outside the six systems

> Part of the Bastion Zero clean-room mechanics spec v3.2 (14 Nov 2026). Narrative, tools, save, UI, audio/VFX, input, accessibility, physics, telemetry hooks.
> Precedence: user decisions (file 01) > owning system > later system. Tags: [sourced], [community-measured], [estimated] (file 01).

---

## Dependencies on other systems
- **Narrative:** the opening event (OQ-H1); who runs the shop (no reference names).
- **Level design / tools:** blockout graph for WC-G metrics, the no-softlock reachability check (SYS5 §6.3), pit safe-point authoring, gate icons.
- **Save system:** persistence classes P0–P3, write triggers, marker state, flask, stock tiers.
- **UI:** pips, guard points, flask charges, wallet, marker icon, shop silhouettes, map sketch and ink, gate icons, attack sign and its size slider, completion.
- **Audio / VFX:** hit-feel table CF-H1–CF-H21, block, parry, reflect and guard-break cues, dash i-frame read, unblockable sign colour, attack sign, telegraph cues, coin pickup, save icon.
- **Input:** buffers (MV-20, CF-B1–CF-B4, CF-B6), priority dash > jump > attack > block > skill, a held BLOCK button (tap timing for the parry) and a DASH button, rebinding.
- **Accessibility:** attack-sign toggle and size, telegraph, damage and parry-window assists (EN-X1–EN-X4), lethal-hit guard (CD-35–CD-36), shake slider, death-penalty-off assist.
- **Physics:** kinematic motor with continuous collision for grapple speeds; ceiling-clip guards (SYS1 dependencies, S5).
- **Playtest and telemetry:** coin and death logs (OQ-H6), pip-loss per rest interval (OQ-H2), slice success criteria VS-S1–VS-S11, parry and dash-through rates (OQ-M15, OQ-M16).

