using UnityEngine;

namespace BastionZero.Combat
{
    /// <summary>
    /// The attack states of SYS3 §3 and §11. Names are the spec's own state IDs.
    /// Context attacks the slice cannot reach yet (CROUCH1 needs G-CROUCH, GLIDE-DOWN
    /// needs A-GLIDE) are declared but never entered, so the enum stays the spec's list.
    /// </summary>
    public enum AttackId
    {
        None = 0,
        G1 = 1,
        G2 = 2,
        G3 = 3,
        AIR1 = 4,
        WALL1 = 5,
        GLIDE_DOWN = 6,
        CROUCH1 = 7,
        SEC_HEAVY = 8,
    }

    /// <summary>SYS3 §8, one row per hit type (H1…H20).</summary>
    public enum HitKind
    {
        None = 0,
        H1_GroundChain = 1,
        H2_ChainFinisher = 2,
        H3_Crit = 3,
        H4_Airborne = 4,
        H5_GlideDown = 5,
        H6_Projectile = 6,
        H7_Kill = 7,
        H8_Heavy = 8,
        H9_Stagger = 9,
        H10_Skill = 10,
        H11_TakeLight = 11,
        H12_TakeHeavy = 12,
        H13_TakeHazard = 13,
        H14_TakePit = 14,
        H15_Lethal = 15,
        H16_BlockLight = 16,
        H17_Parry = 17,
        H18_BlockHeavy = 18,
        H19_GuardBreak = 19,
        H20_DashStart = 20,
    }

    /// <summary>SYS3 §7: the three deterministic crit triggers (decision d). Never stack.</summary>
    public enum CritTrigger
    {
        None = 0,
        StaggeredTarget = 1,
        AirborneChain = 2,
        BehindDuringStartup = 3,
    }

    /// <summary>SYS3 §7: the four enemy-only statuses (decision e).</summary>
    public enum CombatStatus
    {
        None = 0,
        Stun = 1,
        Slow = 2,
        Burn = 3,
        Bleed = 4,
    }

    /// <summary>SYS3 §9 rule 2 HP bands. The tier also picks the poise rule of §6 rule 3.</summary>
    public enum EnemyTier
    {
        Fodder = 0,
        Soldier = 1,
        Brute = 2,
        Elite = 3,
        Boss = 4,
    }

    /// <summary>
    /// One resolved player-to-enemy hit. Built by <see cref="HeroCombat"/> per target at the
    /// moment the slash volume connects, then handed to the target (SYS3 §9 rule 1).
    /// </summary>
    public struct CombatHit
    {
        /// <summary>Which attack produced the hit.</summary>
        public AttackId Attack;

        /// <summary>The feel row this hit uses, before the crit and kill rows are layered on.</summary>
        public HitKind Kind;

        /// <summary>Final damage in damage units: W_base × T × C × M_hit (§9 rule 1).</summary>
        public float Damage;

        /// <summary>Poise damage handed to the target's meter (§6 rule 3).</summary>
        public float PoiseDamage;

        /// <summary>Which crit trigger fired, if any (§7).</summary>
        public CritTrigger Crit;

        /// <summary>True when one of the three deterministic triggers applied.</summary>
        public bool IsCrit;

        /// <summary>Where the hit came from, for the target's own facing checks.</summary>
        public Vector2 Origin;

        /// <summary>Knockback the target should take, in world units (§8 enemy column).</summary>
        public Vector2 Knockback;

        /// <summary>True when the attacker stood behind the target (§7 trigger 3).</summary>
        public bool FromBehind;
    }

    /// <summary>
    /// Anything the hero's blade can damage. SYS4's enemy AI will implement this; the SYS3
    /// gym dummy does too, so the whole damage path is exercised without AI.
    /// </summary>
    public interface IAttackTarget2D
    {
        /// <summary>Applies a resolved hit. Returns true when the hit connected on a live target.</summary>
        bool ReceiveHit(in CombatHit hit);

        /// <summary>True while a poise break has the target staggered (§6 rule 3).</summary>
        bool IsStaggered { get; }

        /// <summary>True during an attack's startup window — crit trigger 3 needs it (§7).</summary>
        bool IsInAttackStartup { get; }

        /// <summary>World position used for facing and knockback maths.</summary>
        Vector2 Position { get; }

        /// <summary>True when the target's front faces +x.</summary>
        bool IsFacingRight { get; }

        /// <summary>HP band of the target, for poise rules and reporting.</summary>
        EnemyTier Tier { get; }

        /// <summary>True once the target was killed and stops accepting hits.</summary>
        bool IsDead { get; }
    }
}
