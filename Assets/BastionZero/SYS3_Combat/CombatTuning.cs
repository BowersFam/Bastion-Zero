using UnityEngine;
using BastionZero.Core;

namespace BastionZero.Combat
{
    /// <summary>
    /// SYS3 tuning. Every number the system uses is a field here, named by the spec ID it
    /// comes from (system-3-combat.md §3, §5, §6, §7, §8, §9). Nothing in the runtime code
    /// is a magic number.
    ///
    /// Sections the slice cannot reach yet keep no fields, so no dead knobs exist:
    /// §3.2 GLIDE-DOWN (no A-GLIDE state in SYS1) and CROUCH1 (no G-CROUCH), §3.4 slot B
    /// archetypes (slot B is acquired in SYS6), §4 skills (no skill content is defined).
    /// See TUNING_CHANGES.md, "SYS3" for the list.
    /// </summary>
    [CreateAssetMenu(fileName = "CombatTuning", menuName = "Bastion Zero/SYS3 Combat Tuning")]
    public class CombatTuning : ScriptableObject
    {
        // ── §3.1 Ground chain, slot A ────────────────────────────────────────────────
        [Header("§3.1 Ground chain G1 — opener")]
        public int G1_StartupFrames = 3;
        public int G1_ActiveFrames = 6;
        public int G1_RecoveryFrames = 9;
        [Tooltip("Recovery frame from which a buffered attack advances to G2. -1 = no chain.")]
        public int G1_NextHitCancelFromRecoveryFrame = 4;
        public float G1_DamageMultiplier = 1.0f;

        [Header("§3.1 Ground chain G2")]
        public int G2_StartupFrames = 3;
        public int G2_ActiveFrames = 6;
        public int G2_RecoveryFrames = 9;
        public int G2_NextHitCancelFromRecoveryFrame = 4;
        public float G2_DamageMultiplier = 1.0f;

        [Header("§3.1 Ground chain G3 — finisher")]
        public int G3_StartupFrames = 5;
        public int G3_ActiveFrames = 6;
        public int G3_RecoveryFrames = 16;
        [Tooltip("-1: the chain ends on G3 (the spec lists no next-hit cancel).")]
        public int G3_NextHitCancelFromRecoveryFrame = -1;
        public float G3_DamageMultiplier = 2.0f;

        [Header("§3.1 Combo")]
        [Tooltip("Seconds after a hit's recovery ends during which an attack still continues the chain.")]
        public float ComboMemorySeconds = 0.35f;

        // ── §3.2 Air and wall attacks ────────────────────────────────────────────────
        [Header("§3.2 AIR1 — air slash")]
        public int AIR1_StartupFrames = 2;
        public int AIR1_ActiveFrames = 6;
        public int AIR1_RecoveryFrames = 10;
        [Tooltip("§9 rule 1 M_hit. The spec lists 1.0, 1.0, 2.0 for the chain and omits the context attacks, so they deal base damage.")]
        public float AIR1_DamageMultiplier = 1f;

        [Header("§3.2 WALL1 — wall-cling slash")]
        public int WALL1_StartupFrames = 2;
        public int WALL1_ActiveFrames = 6;
        public int WALL1_RecoveryFrames = 10;
        [Tooltip("§9 rule 1 M_hit. The spec lists 1.0, 1.0, 2.0 for the chain and omits the context attacks, so they deal base damage.")]
        public float WALL1_DamageMultiplier = 1f;

        // ── §3.3 Buffering ───────────────────────────────────────────────────────────
        [Header("§3.3 Input buffers (seconds; 6 f = 0.10 s)")]
        public float AttackBufferSeconds = 0.10f;
        public float DashBufferSeconds = 0.10f;
        [Tooltip("Press buffer only; the block itself is held.")]
        public float BlockBufferSeconds = 0.10f;

        // ── §5.2 Dash, state C-DASH ──────────────────────────────────────────────────
        [Header("§5.2 C-DASH")]
        public int Dash_DurationFrames = 12;
        [Tooltip("3.5 tiles over 0.20 s = 17.5 tiles/s (§5.2 Distance).")]
        public float Dash_SpeedTilesPerSecond = 17.5f;
        [Tooltip("Seconds of cooldown after the dash ends; start-to-start is 0.60 s.")]
        public float Dash_CooldownSeconds = 0.40f;
        [Tooltip("DB-1: i-frames cover f3–f10 of the 12 f dash, enemy sources only.")]
        public int Dash_IFrameStartFrame = 3;
        public int Dash_IFrameEndFrame = 10;
        [Tooltip("DB-3: the air dash is a later traversal unlock. Off = DASH in the air does nothing.")]
        public bool Dash_AirDashUnlocked = false;

        // ── §5.3 Block, state C-BLOCK ────────────────────────────────────────────────
        [Header("§5.3 C-BLOCK")]
        public int Block_RaiseFrames = 4;
        public int Block_ReleaseFrames = 4;
        [Tooltip("Guard walk is 0.35× run speed (§5.3 Movement while guarding).")]
        public float Block_WalkSpeedMultiplier = 0.35f;
        public int Block_GuardPoints = 4;
        [Tooltip("Guard points a blocked D-Light costs (§5.3).")]
        public int Block_LightGuardCost = 1;
        [Tooltip("Guard points a blocked D-Heavy costs (§5.3).")]
        public int Block_HeavyGuardCost = 2;
        [Tooltip("1 point per 0.5 s = 2 points per second.")]
        public float Block_GuardRegenPointsPerSecond = 2f;
        public float Block_GuardRegenDelaySeconds = 0.75f;
        public float Block_GuardBreakSeconds = 0.8f;
        public float Block_LightBlockstunSeconds = 0.10f;
        public float Block_LightPushTiles = 0.25f;
        public float Block_HeavyBlockstunSeconds = 0.20f;
        public float Block_HeavyPushTiles = 0.75f;
        public int Block_HeavyChipPips = 1;
        [Tooltip("§5.3: chip never takes the last pip.")]
        public int Block_ChipFloorPips = 1;

        // ── §5.4 Parry, state BK-PARRY ───────────────────────────────────────────────
        [Header("§5.4 BK-PARRY (DB-2)")]
        [Tooltip("The first 8 f of BK-HOLD (frames 5–12 after the press). 0 guard points, no refund (DB-6).")]
        public int Parry_WindowFrames = 8;
        public float Parry_LightPoiseDamage = 25f;
        public float Parry_HeavyPoiseDamage = 50f;
        [Tooltip("Whiff-tap lockout: the next raise inside this window blocks but cannot parry.")]
        public float Parry_TapLockoutSeconds = 0.6f;
        [Tooltip(
            "§5.4: a parry covers the parried hitbox for the rest of its active frames. 6 f is " +
            "the S1 row 28 active window, so a multi-hit attack cannot parry-then-hit.")]
        public int Parry_CoverFrames = 6;

        // ── §6 Hit-stun tiers ───────────────────────────────────────────────────────
        [Header("§6 rule 3 — enemy hit-stun")]
        public float LightFlinchSeconds = 0.15f;
        public float PoiseStaggerSeconds = 1.0f;
        [Tooltip("Poise damage taken during an enemy attack's startup counts ×2.")]
        public float PoiseStartupMultiplier = 2.0f;
        [Tooltip(
            "Poise damage a player hit deals, before M_hit. SYS3 does not state this number " +
            "(SYS4 owns poise thresholds); 10 is the gap value logged in TUNING_CHANGES.md.")]
        public float PoiseBasePerHit = 10f;

        // ── §7 Crits and statuses (decisions d, e) ──────────────────────────────────
        [Header("§7 Crits (decision d)")]
        public float Crit_Multiplier = 2.0f;

        [Header("§7 Status: stun")]
        public float Status_StunSeconds = 1.0f;

        [Header("§7 Status: slow")]
        public float Status_SlowPerStack = 0.20f;
        public int Status_SlowMaxStacks = 5;
        public float Status_SlowSeconds = 3.0f;
        [Tooltip("The 5th stack freezes for this long and resets the stacks.")]
        public float Status_SlowFreezeSeconds = 1.5f;

        [Header("§7 Status: burn")]
        public float Status_BurnTickSeconds = 0.5f;
        public float Status_BurnSeconds = 3.0f;
        public float Status_BurnDamageUnits = 1f;

        [Header("§7 Status: bleed")]
        public float Status_BleedTickSeconds = 0.5f;
        public float Status_BleedSeconds = 4.0f;
        public float Status_BleedDamageUnits = 1f;
        public int Status_BleedMaxStacks = 3;

        [Header("§7 Elite / boss modifier")]
        public float EliteBoss_DurationMultiplier = 0.8f;
        public float EliteBoss_CCReapplyMultiplier = 0.5f;
        public float EliteBoss_CCReapplyWindowSeconds = 5f;

        // ── §8 Combat feel table ────────────────────────────────────────────────────
        // One field per implemented row, named by the H ID. H5 (glide down-strike),
        // H6 (projectile destroyed), H8 (heavy), H10 (skill), H11–H15 (SYS2's
        // player-takes-damage rows), H20 (dash start, no effect) carry no field because
        // no SYS3 code path produces them.
        [Header("§8 H1 — ground chain hit (G1/G2)")]
        public int H1_StopAttackerFrames = 2;
        public int H1_StopTargetFrames = 2;
        public float H1_ShakeAmplitudePixels = 0f;
        public float H1_ShakeSeconds = 0f;
        public float H1_PlayerKnockbackTiles = 0f;
        [Tooltip("Light enemies only (§8 H1).")]
        public float H1_EnemyKnockbackTiles = 0.25f;

        [Header("§8 H2 — chain finisher (G3)")]
        public int H2_StopAttackerFrames = 4;
        public int H2_StopTargetFrames = 4;
        public float H2_ShakeAmplitudePixels = 1f;
        public float H2_ShakeSeconds = 0.10f;
        public float H2_PlayerKnockbackTiles = 0f;
        public float H2_EnemyKnockbackTiles = 1.0f;

        [Header("§8 H3 — critical hit")]
        [Tooltip("1 f global freeze, then 0.2 s at 0.5× time scale (basis: S37).")]
        public int H3_GlobalFreezeFrames = 1;
        public float H3_SlowSeconds = 0.2f;
        public float H3_SlowTimeScale = 0.5f;
        public float H3_ShakeAmplitudePixels = 2f;
        public float H3_ShakeSeconds = 0.15f;
        public float H3_PlayerKnockbackTiles = 0f;
        public float H3_EnemyKnockbackTiles = 1.0f;

        [Header("§8 H4 — airborne bounce-grant hit (AIR1 / WALL1)")]
        public int H4_StopAttackerFrames = 3;
        public int H4_StopTargetFrames = 3;
        public float H4_ShakeAmplitudePixels = 0f;
        public float H4_ShakeSeconds = 0f;
        public float H4_PlayerKnockbackTiles = 0f;
        public float H4_EnemyKnockbackTiles = 0.25f;

        [Header("§8 H7 — kill bonus (adds to the hit type)")]
        public int H7_ExtraStopFrames = 2;
        public float H7_ExtraShakePixels = 1f;
        public float H7_ExtraShakeSeconds = 0.10f;
        public float H7_BodyFlungTiles = 1.5f;

        [Header("§8 H9 — enemy stagger (poise break)")]
        public int H9_StopAttackerFrames = 4;
        public int H9_StopTargetFrames = 6;
        public float H9_ShakeAmplitudePixels = 2f;
        public float H9_ShakeSeconds = 0.20f;
        public float H9_PlayerKnockbackTiles = 0f;
        public float H9_EnemyKnockbackTiles = 0.5f;

        [Header("§8 H16 — hit blocked, D-Light, front")]
        public int H16_StopAttackerFrames = 2;
        public int H16_StopTargetFrames = 2;
        public float H16_ShakeAmplitudePixels = 1f;
        public float H16_ShakeSeconds = 0.06f;
        public float H16_PlayerKnockbackTiles = 0.25f;
        public float H16_EnemyKnockbackTiles = 0f;

        [Header("§8 H17 — parry (DB-2)")]
        public int H17_StopAttackerFrames = 4;
        public int H17_StopTargetFrames = 4;
        public float H17_ShakeAmplitudePixels = 2f;
        public float H17_ShakeSeconds = 0.10f;
        public float H17_PlayerKnockbackTiles = 0f;
        public float H17_EnemyKnockbackTiles = 0.5f;

        [Header("§8 H18 — hit blocked, D-Heavy, chip")]
        public int H18_StopAttackerFrames = 4;
        public int H18_StopTargetFrames = 4;
        public float H18_ShakeAmplitudePixels = 2f;
        public float H18_ShakeSeconds = 0.12f;
        public float H18_PlayerKnockbackTiles = 0.75f;
        public float H18_EnemyKnockbackTiles = 0f;

        [Header("§8 H19 — guard break")]
        public int H19_GlobalFreezeFrames = 6;
        public float H19_ShakeAmplitudePixels = 3f;
        public float H19_ShakeSeconds = 0.20f;
        public float H19_PlayerKnockbackTiles = 0.5f;

        // ── §9 Damage scaling model ─────────────────────────────────────────────────
        [Header("§9 rule 1 — player to enemy")]
        [Tooltip("W_base: weapon base damage units for slot A.")]
        public float W_BaseUnits = 10f;
        [Tooltip("Fixed upgrade tiers T, found at fixed locations (SYS6).")]
        public float[] WeaponTiers = { 1.0f, 1.4f, 1.8f, 2.2f, 2.6f };

        [Header("§9 rule 2 — enemy HP bands")]
        public float HPBand_Fodder = 10f;
        public float HPBand_Soldier = 30f;
        public float HPBand_Brute = 80f;
        public float HPBand_Elite = 150f;
        [Tooltip("EN-B1 boss HP is 600 / 900 / 1,200; 600 is the first-boss value.")]
        public float HPBand_Boss = 600f;

        [Header("§6 / SYS4 — poise per tier (EN-P9, EN-S4, EN-E2, EN-B9)")]
        [Tooltip("Fodder and crawlers flinch instead of carrying a meter (§6 rule 3).")]
        public float Poise_Fodder = 0f;
        public float Poise_Soldier = 25f;
        public float Poise_Brute = 60f;
        [Tooltip("EN-E2: elite poise is base × 2.5.")]
        public float Poise_Elite = 62.5f;
        public float Poise_Boss = 120f;

        [Header("§6 — poise decay per second")]
        public float PoiseDecay_Soldier = 10f;
        public float PoiseDecay_Brute = 15f;
        public float PoiseDecay_Elite = 10f;
        public float PoiseDecay_Boss = 20f;

        [Header("Change log")]
        [TextArea(3, 8)]
        public string changeLog =
            "v0 (14 Nov 2026): every SYS3 §3, §5, §6, §7, §8 and §9 value the slice implements; " +
            "spec version 3.1. PoiseBasePerHit is a gap fill (SYS3 states no per-hit poise number).";

        // ── Helpers ─────────────────────────────────────────────────────────────────

        /// <summary>Converts a spec frame count to seconds at 60 fps.</summary>
        public static float Frames(int frames) => frames / (float)SpecUnits.FramesPerSecond;

        /// <summary>§9 rule 2: HP for an enemy tier.</summary>
        public float HPFor(EnemyTier tier)
        {
            switch (tier)
            {
                case EnemyTier.Fodder: return HPBand_Fodder;
                case EnemyTier.Soldier: return HPBand_Soldier;
                case EnemyTier.Brute: return HPBand_Brute;
                case EnemyTier.Elite: return HPBand_Elite;
                case EnemyTier.Boss: return HPBand_Boss;
                default: return HPBand_Fodder;
            }
        }

        /// <summary>§6 rule 3: the poise threshold, or 0 for a tier that only flinches.</summary>
        public float PoiseFor(EnemyTier tier)
        {
            switch (tier)
            {
                case EnemyTier.Soldier: return Poise_Soldier;
                case EnemyTier.Brute: return Poise_Brute;
                case EnemyTier.Elite: return Poise_Elite;
                case EnemyTier.Boss: return Poise_Boss;
                default: return Poise_Fodder;
            }
        }

        /// <summary>§6: poise decay per second for a tier.</summary>
        public float PoiseDecayFor(EnemyTier tier)
        {
            switch (tier)
            {
                case EnemyTier.Soldier: return PoiseDecay_Soldier;
                case EnemyTier.Brute: return PoiseDecay_Brute;
                case EnemyTier.Elite: return PoiseDecay_Elite;
                case EnemyTier.Boss: return PoiseDecay_Boss;
                default: return 0f;
            }
        }

        /// <summary>§9 rule 1: T[tier] for a 1-based weapon tier (SYS6 grants the upgrades).</summary>
        public float WeaponTierMultiplier(int oneBasedTier)
        {
            if (WeaponTiers == null || WeaponTiers.Length == 0) return 1f;
            int index = Mathf.Clamp(oneBasedTier - 1, 0, WeaponTiers.Length - 1);
            return WeaponTiers[index];
        }

        /// <summary>§8: shake amplitude in pixels converted to world units (16 px per tile).</summary>
        public static float PixelsToUnits(float pixels) => pixels / SpecUnits.PixelsPerTile;
    }
}
