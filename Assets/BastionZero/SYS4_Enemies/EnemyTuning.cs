using System;
using BastionZero.Combat;
using BastionZero.Damage;
using UnityEngine;

namespace BastionZero.Enemies
{
    /// <summary>
    /// One column of the SYS4 §4 patroller timing table. Fodder (P-F) and Soldier (P-S) are two
    /// instances of this class, so every row of the table survives as a named field with its
    /// section reference in the tooltip. Values are the spec's [estimated] numbers.
    /// </summary>
    [Serializable]
    public class PatrollerProfile
    {
        [Tooltip("§4 'Patrol speed (tiles/s)'.")]
        public float patrolSpeed = 2.5f;

        [Tooltip("§4 'Chase speed (tiles/s)'.")]
        public float chaseSpeed = 4f;

        [Tooltip("§4 'Detection (tiles)' — horizontal forward range.")]
        public float detectionTiles = 6f;

        [Tooltip("§3.3 'Detection behind' — 50% of the forward range.")]
        public float detectionBehindMultiplier = 0.5f;

        [Tooltip("§4 'Detection (tiles)' — vertical half-band (±2).")]
        public float detectionVerticalTiles = 2f;

        [Tooltip("§4 'Attack range (tiles)'.")]
        public float attackRange = 1.25f;

        [Tooltip("§4 'Telegraph' (seconds). §3.2 floors a D-Light telegraph at 0.35 s.")]
        public float telegraphSeconds = 0.40f;

        [Tooltip("§4 'Active' (seconds).")]
        public float activeSeconds = 0.10f;

        [Tooltip("§4 'Active' — how far the strike lunges, in tiles.")]
        public float lungeTiles = 0.5f;

        [Tooltip("§4 'Recovery' (seconds).")]
        public float recoverySeconds = 0.50f;

        [Tooltip("§4 'Cooldown' (seconds).")]
        public float cooldownSeconds = 0.80f;

        [Tooltip("§4 'Leash (tiles from home)'.")]
        public float leashTiles = 10f;

        [Tooltip("§4 'Damage' — the strike's class (contact is SYS2's D-Light on the body).")]
        public DamageClass strikeClass = DamageClass.D_Light;
    }

    /// <summary>
    /// SYS4 §3–§4 tuning. One asset holds the shared aggro/leash/token framework and both
    /// patroller columns, so an encounter is balanced by editing rows rather than literals.
    /// </summary>
    [CreateAssetMenu(menuName = "Bastion Zero/Tuning/Enemy AI (SYS4)", fileName = "EnemyTuning")]
    public class EnemyTuning : ScriptableObject
    {
        [Header("Patroller (§4) — the two slice variants")]
        public PatrollerProfile fodder = new PatrollerProfile
        {
            patrolSpeed = 2.5f,
            chaseSpeed = 4f,
            detectionTiles = 6f,
            detectionVerticalTiles = 2f,
            attackRange = 1.25f,
            telegraphSeconds = 0.40f,
            activeSeconds = 0.10f,
            lungeTiles = 0.5f,
            recoverySeconds = 0.50f,
            cooldownSeconds = 0.80f,
            leashTiles = 10f,
            strikeClass = DamageClass.D_Light,
        };

        public PatrollerProfile soldier = new PatrollerProfile
        {
            patrolSpeed = 2f,
            chaseSpeed = 4.5f,
            detectionTiles = 8f,
            detectionVerticalTiles = 2f,
            attackRange = 1.5f,
            telegraphSeconds = 0.45f,
            activeSeconds = 0.12f,
            lungeTiles = 1f,
            recoverySeconds = 0.60f,
            cooldownSeconds = 1.00f,
            leashTiles = 14f,
            strikeClass = DamageClass.D_Light,
        };

        [Header("Shared framework (§3.3 aggro and leash)")]
        [Tooltip("§3.3 'E-NOTICE duration'.")]
        public float noticeSeconds = 0.3f;

        [Tooltip("§3.3 'Lost line of sight' — seconds before E-SEARCH.")]
        public float lostSightSeconds = 3f;

        [Tooltip("§3.3 'E-SEARCH duration'.")]
        public float searchSeconds = 2f;

        [Tooltip("§3.3 leash rule: how long E-RETURN may run before giving up back to E-IDLE.")]
        public float returnTimeoutSeconds = 6f;

        [Tooltip("§3.3 'Re-aggro lockout after E-RETURN starts'.")]
        public float reAggroLockoutSeconds = 1f;

        [Tooltip("§3.2 'Direction lock' — aim tracks until this many seconds before E-ATTACK.")]
        public float directionLockSeconds = 0.15f;

        [Header("Shared framework (§3.4 attack tokens)")]
        [Tooltip("§3.4 'Enemies allowed in E-TELEGRAPH or E-ATTACK at once (melee)', early tiers.")]
        public int meleeAttackTokens = 2;

        [Header("Push and death (SYS3 §8 enemy column, applied here)")]
        [Tooltip("Gap fill [estimated]: seconds a §8 push takes to travel its distance. The spec gives "
                 + "push distances only; 0.15 s equals SYS3 LightFlinchSeconds, so a light enemy's slide "
                 + "ends with its flinch. The AI's own walking waits while a push runs.")]
        public float knockbackSeconds = 0.15f;

        [Tooltip("Gap fill [estimated]: E-DEAD fades the body over this many seconds after the H7 fling, "
                 + "then enters E-DORMANT. 0.4 s equals SYS2's D-3 death fade.")]
        public float deadFadeSeconds = 0.4f;

        [Header("Strike hitbox (authored facing right)")]
        public Vector2 strikeSize = new Vector2(1.2f, 0.9f);
        public Vector2 strikeOffset = new Vector2(0.7f, 0f);

        [Header("Attack sign (§3.2 / §14)")]
        [Tooltip("§14 'Attack-sign size' slider, 10%–200%. Decision (g): the sign is on by default.")]
        [Range(0.1f, 2f)] public float attackSignScale = 1f;

        [Tooltip("§3.2 the universal sign colour for a blockable, parryable attack.")]
        public Color attackSignColour = new Color(1f, 0.78f, 0.25f, 1f);

        [Tooltip("§3.2 the distinct sign colour for an unblockable attack.")]
        public Color unblockableSignColour = new Color(1f, 0.27f, 0.2f, 1f);

        [Tooltip("World offset from the enemy origin to the sign, in tiles.")]
        public Vector2 attackSignOffset = new Vector2(0f, 1.15f);

        [Header("Body probes")]
        [Tooltip("Half-extents of the walking body, used by the ledge and wall probes.")]
        public Vector2 bodyHalfExtents = new Vector2(0.55f, 0.4f);

        [Tooltip("How far below the feet the ledge probe looks for floor.")]
        public float ledgeProbeDepth = 0.6f;

        [Tooltip("§4 'Poise threshold / decay' is owned by SYS3's tier table; this only stops a "
                 + "poise tier being interrupted by its own flinch timer.")]
        public bool soldierUsesPoise = true;

        /// <summary>The §4 column that matches a tier. Non-patroller tiers fall back to soldier.</summary>
        public PatrollerProfile ProfileFor(EnemyTier tier)
        {
            return tier == EnemyTier.Fodder ? fodder : soldier;
        }
    }
}
