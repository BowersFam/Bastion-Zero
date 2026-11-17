using UnityEngine;

namespace BastionZero.Damage
{
    /// <summary>
    /// SYS2 tuning asset. One field per spec ID from 06-collision-damage.md §3.2, with the tag
    /// and the reasoning carried in the tooltip so a value can be traced back to its row.
    /// Every number is [estimated] and lives here; none of them are written into code.
    /// </summary>
    [CreateAssetMenu(menuName = "Bastion Zero/Tuning/Collision and Damage (SYS2)", fileName = "DamageTuning")]
    public class DamageTuning : ScriptableObject
    {
        [Header("CD-01…CD-07 · Collision and hurt box (06 §3.2)")]
        [Tooltip("CD-01 Player collision box width — 0.75 tiles [estimated]. Fits 1-tile gaps with margin.")]
        [Min(0.01f)] public float CD_01_CollisionBoxWidth = 0.75f;

        [Tooltip("CD-02 Player collision box height, standing — 1.75 tiles [estimated]. Slightly under the 2-tile drawn height.")]
        [Min(0.01f)] public float CD_02_CollisionBoxHeight = 1.75f;

        [Tooltip("CD-04 Player hurtbox width — 0.5 tiles [estimated]. Forgiving sideways.")]
        [Min(0.01f)] public float CD_04_HurtboxWidth = 0.5f;

        [Tooltip("CD-05 Player hurtbox height, standing — 1.375 tiles [estimated]. Excludes the top of the head and the feet.")]
        [Min(0.01f)] public float CD_05_HurtboxHeight = 1.375f;

        [Tooltip("CD-07 Hazard hitbox inset per exposed side — 0.125 tiles (2 px) [estimated]. Biases every hazard toward the player.")]
        [Min(0f)] public float CD_07_HazardInset = 0.125f;

        [Tooltip("CD-08 Enemy contact hitbox inset vs the enemy hurtbox — 0.125 tiles [estimated]. Grazing an enemy must not hurt.")]
        [Min(0f)] public float CD_08_ContactHitboxInset = 0.125f;

        [Header("CD-17…CD-23 · Pips and damage classes")]
        [Tooltip("CD-17 Start HP — 5 pips [estimated] (basis: S4, S24).")]
        [Min(1)] public int CD_17_StartHP = 5;

        [Tooltip("CD-18 Max HP — 10 pips [estimated]. Decided (user): 10 pips. Supersedes SYS1 row 45's 8.")]
        [Min(1)] public int CD_18_MaxHP = 10;

        [Tooltip("CD-19 D-Light — 1 pip [estimated] (basis: S6). Enemy contact and small projectiles.")]
        [Min(0)] public int CD_19_DLightPips = 1;

        [Tooltip("CD-20 D-Heavy — 2 pips [estimated] (basis: S26). Telegraphed heavy attacks.")]
        [Min(0)] public int CD_20_DHeavyPips = 2;

        [Tooltip("CD-21 D-Hazard — 3 pips [estimated] (basis: S6). Spikes, saws, lasers.")]
        [Min(0)] public int CD_21_DHazardPips = 3;

        [Tooltip("CD-41 D-Pit — 2 pips [estimated]. 20% of a 10-pip bar, below a spike's 3.")]
        [Min(0)] public int CD_41_DPitPips = 2;

        [Tooltip("CD-22 Damage reduction, from the shop upgrade — 1 pip, on hits of 2 or more [estimated]. 0 = upgrade not owned.")]
        [Min(0)] public int CD_22_DamageReduction = 0;

        [Tooltip("CD-23 Minimum damage per hit — 1 pip [estimated]. A blocked D-Light inside the block system can still deal 0 (X25).")]
        [Min(0)] public int CD_23_MinDamagePerHit = 1;

        [Header("CD-24…CD-28 · Invulnerability and knockback")]
        [Tooltip("CD-24 Post-hit i-frames — 1.0 s [estimated]. Blocks D-Light and D-Heavy; hazards ignore them (§8.2).")]
        [Min(0f)] public float CD_24_PostHitIFrames = 1f;

        [Tooltip("CD-25 Hazard re-hit lockout on the same instance — 0.5 s [estimated]. Decided (user): adopted.")]
        [Min(0f)] public float CD_25_HazardReHitLockout = 0.5f;

        [Tooltip("CD-26 Contact re-arm after the overlap is gone — 1 frame [estimated]. §6 rule 4.")]
        [Min(0f)] public float CD_26_ContactRearm = 1f / 60f;

        [Tooltip("CD-27 Knockback hold fraction — 0.5 × the duration [estimated]. Then linear decay to zero.")]
        [Range(0f, 1f)] public float CD_27_KnockbackHoldFraction = 0.5f;

        [Tooltip("CD-28 Knockback same-x tie zone — 0.25 tiles [estimated]. Inside it the push goes opposite the hero's facing.")]
        [Min(0f)] public float CD_28_KnockbackTieZone = 0.25f;

        [Tooltip("CD-S49 Knockback duration, no cancel — 0.25 s [estimated]. Cancelled by an attack (§7.5, SYS3).")]
        [Min(0f)] public float CD_S49_KnockbackDuration = 0.25f;

        [Tooltip("CD-S50 Knockback horizontal speed — 7 tiles/s [estimated]. Clears a 1-tile spike strip.")]
        [Min(0f)] public float CD_S50_KnockbackHorizontal = 7f;

        [Tooltip("CD-S50 Knockback upward speed — 9 tiles/s [estimated].")]
        [Min(0f)] public float CD_S50_KnockbackUp = 9f;

        [Header("CD-29…CD-34 · Warp, death and respawn")]
        [Tooltip("CD-29 Hazard warp arrival i-frames — 1.0 s [estimated]. Covers hazards as well as enemies, so a warp cannot loop.")]
        [Min(0f)] public float CD_29_WarpArrivalIFrames = 1f;

        [Tooltip("CD-30 Death D-1 freeze — 0.15 s [estimated]. World hit-pause, input off, hurtbox and collision off.")]
        [Min(0f)] public float CD_30_DeathD1Freeze = 0.15f;

        [Tooltip("CD-31 Death D-2 animation — 0.8 s [estimated]. Body locked in place, camera holds.")]
        [Min(0f)] public float CD_31_DeathD2Animation = 0.8f;

        [Tooltip("CD-32 Death D-3 fade — 0.4 s [estimated].")]
        [Min(0f)] public float CD_32_DeathD3Fade = 0.4f;

        [Tooltip("CD-33 Respawn HP — full (= max HP) [estimated]. Decided (user): full, not the reference's 3.")]
        public bool CD_33_RespawnFullHP = true;

        [Tooltip("CD-34 Respawn grace i-frames, all damage — 1.5 s [estimated]. D-5.")]
        [Min(0f)] public float CD_34_RespawnGraceIFrames = 1.5f;

        [Tooltip("CD-42 D-Pit warp fade — 0.4 s [estimated]. Reuses the hazard warp fade (SYS1 row 51).")]
        [Min(0f)] public float CD_42_PitWarpFade = 0.4f;

        [Header("CD-37…CD-38 · Feedback")]
        [Tooltip("CD-37 I-frame flicker rate — 15 Hz [estimated]. One toggle every 4 frames at 60 fps.")]
        [Min(0f)] public float CD_37_IFrameFlickerHz = 15f;

        [Tooltip("CD-38 Low-health signal at — 1 pip [estimated] (basis: S24).")]
        [Min(0)] public int CD_38_LowHealthPips = 1;

        [Header("Tuning change log")]
        [Tooltip("Record every edit to an [estimated] value here so the table and the asset cannot drift apart.")]
        [TextArea(3, 10)] public string changeLog =
            "v0 — created from spec v3.2 (06-collision-damage.md). No [estimated] value has been retuned yet.";
    }
}
