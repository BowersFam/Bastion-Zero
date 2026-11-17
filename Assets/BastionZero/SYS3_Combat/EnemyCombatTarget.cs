using System;
using BastionZero.Damage;
using UnityEngine;

namespace BastionZero.Combat
{
    /// <summary>
    /// A damageable enemy body for SYS3: HP bands (§9 rule 2), the two hit-stun tiers (§6 rule 3),
    /// the four enemy-only statuses (§7, decision e) and the death hand-off that removes the
    /// contact hitbox first (§12, S2-doc §1 rule 7).
    ///
    /// Locomotion, attacks and animation belong to SYS4 and are not here: the SYS3 gym uses this
    /// component as a stationary target so the whole damage path is exercised without AI.
    /// Each hit's §8 push is resolved into <see cref="ResolvedKnockbackTiles"/> before
    /// <see cref="Hit"/> is raised, for SYS4 to apply; a stationary target does not slide.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyCombatTarget : MonoBehaviour, IAttackTarget2D
    {
        [Tooltip("SYS3 tuning asset. Required.")]
        [SerializeField] private CombatTuning tuning;

        [Tooltip("HP band and poise rule (§9 rule 2, §6 rule 3).")]
        [SerializeField] private EnemyTier tier = EnemyTier.Fodder;

        [Tooltip("Which way the front faces. The gym dummies face the hero (left).")]
        [SerializeField] private bool facingRight = false;

        [Tooltip("Overrides the tier's HP band when above 0 (test only).")]
        [SerializeField] private float hpOverride = 0f;

        [Tooltip("Tinted while the flinch or stagger window runs, then restored.")]
        [SerializeField] private SpriteRenderer body;

        [SerializeField] private Color hitTint = new Color(1f, 0.75f, 0.6f, 1f);
        [SerializeField] private Color deadTint = new Color(0.25f, 0.22f, 0.2f, 1f);

        private Collider2D hurtbox;
        private ContactDamager2D contact;
        private float basePoise;
        private float flinchTimer;
        private float staggerTimer;
        private float poise;
        private float stunTimer;
        private float slowTimer;
        private int slowStacks;
        private float burnTimer;
        private float burnTickTimer;
        private float bleedTimer;
        private float bleedTickTimer;
        private int bleedStacks;
        private float tintTimer;
        private float lastCCApplyTime = -999f;
        private int ccApplications;
        private bool attackStartup;

        /// <summary>Current HP in damage units.</summary>
        public float CurrentHP { get; private set; }

        /// <summary>HP band the target started with.</summary>
        public float MaxHP { get; private set; }

        /// <summary>Accumulated poise damage; 0 for tiers that only flinch.</summary>
        public float Poise => poise;

        /// <summary>Poise threshold for this tier; 0 when the tier flinches instead.</summary>
        public float PoiseThreshold => basePoise;

        /// <summary>The most recent hit resolved against this target.</summary>
        public CombatHit LastHit { get; private set; }

        /// <summary>
        /// §8 enemy push for the latest hit, in tiles, set before <see cref="Hit"/> is raised: the
        /// hit's own knockback, at least H9 on the hit that breaks poise, exactly H7 on a lethal hit,
        /// and 0 for a status kill. SYS4 moves the body; this only decides the distance.
        /// </summary>
        public float ResolvedKnockbackTiles { get; private set; }

        /// <summary>The §8 row that set <see cref="ResolvedKnockbackTiles"/>.</summary>
        public HitKind ResolvedKnockbackKind { get; private set; }

        /// <summary>Hits that connected, including the killing one.</summary>
        public int HitsTaken { get; private set; }

        /// <summary>Fires on every connected hit.</summary>
        public event Action<EnemyCombatTarget, CombatHit> Hit;

        /// <summary>Fires once when HP reaches 0.</summary>
        public event Action<EnemyCombatTarget> Died;

        // ── IAttackTarget2D ─────────────────────────────────────────────────────────

        public bool IsStaggered => staggerTimer > 0f;

        public bool IsInAttackStartup => attackStartup;

        public Vector2 Position => transform.position;

        public bool IsFacingRight => facingRight;

        public EnemyTier Tier => tier;

        public bool IsDead { get; private set; }

        /// <summary>True for the tiers that flinch on every hit instead of carrying a meter.</summary>
        public bool IsLightTier => basePoise <= 0f;

        /// <summary>Remaining flinch (light tiers) or stagger (poise tiers) time.</summary>
        public float HitStunRemaining => Mathf.Max(flinchTimer, staggerTimer);

        /// <summary>True while a stun status holds the target (§7).</summary>
        public bool IsStunned => stunTimer > 0f;

        /// <summary>§7 slow: −20% move and attack speed per stack. SYS4 reads this.</summary>
        public float SpeedMultiplier => Mathf.Clamp(1f - (tuning != null ? tuning.Status_SlowPerStack : 0f) * slowStacks, 0.01f, 1f);

        /// <summary>Active slow stacks (§7, up to 5).</summary>
        public int SlowStacks => slowStacks;

        /// <summary>Remaining burn and bleed time, for the readout.</summary>
        public float BurnRemaining => Mathf.Max(0f, burnTimer);

        /// <summary>Remaining bleed time (§7).</summary>
        public float BleedRemaining => Mathf.Max(0f, bleedTimer);

        private void Awake()
        {
            if (tuning == null)
            {
                Debug.LogError("[SYS3] EnemyCombatTarget has no CombatTuning assigned; hits cannot resolve.", this);
                enabled = false;
                return;
            }

            hurtbox = GetComponent<Collider2D>();
            contact = GetComponent<ContactDamager2D>();
            if (body == null) body = GetComponentInChildren<SpriteRenderer>(true);

            Restore();
        }

        /// <summary>Puts the target back at full HP and zero poise (EN-A10 uses this on a reset).</summary>
        public void Restore()
        {
            MaxHP = hpOverride > 0f ? hpOverride : tuning.HPFor(tier);
            CurrentHP = MaxHP;
            basePoise = tuning.PoiseFor(tier);
            poise = 0f;
            flinchTimer = 0f;
            staggerTimer = 0f;
            stunTimer = 0f;
            slowTimer = 0f;
            slowStacks = 0;
            burnTimer = 0f;
            bleedTimer = 0f;
            bleedStacks = 0;
            tintTimer = 0f;
            ccApplications = 0;
            lastCCApplyTime = -999f;
            attackStartup = false;
            HitsTaken = 0;
            IsDead = false;
            ResolvedKnockbackTiles = 0f;
            ResolvedKnockbackKind = HitKind.None;

            if (hurtbox != null) hurtbox.enabled = true;
            if (contact != null) contact.enabled = true;
            if (body != null) body.color = Color.white;
        }

        /// <summary>SYS4 calls this so §7's behind-during-startup crit can fire and poise counts ×2.</summary>
        public void SetAttackStartup(bool inStartup)
        {
            attackStartup = inStartup;
        }

        /// <summary>
        /// SYS4 turns the body to face its target. The front decides crit trigger 3 (behind during
        /// startup) and which way the enemy's own strike box is placed. Presentation (the sprite
        /// flip) is left to the caller so a body whose art faces left is not mirrored twice.
        /// </summary>
        public void SetFacing(bool right)
        {
            facingRight = right;
        }

        // ── Damage in ───────────────────────────────────────────────────────────────

        public bool ReceiveHit(in CombatHit hit)
        {
            if (IsDead) return false;

            CurrentHP = Mathf.Max(0f, CurrentHP - hit.Damage);
            LastHit = hit;
            HitsTaken++;

            tintTimer = 0.10f;
            if (body != null) body.color = hitTint;

            bool brokePoise = ApplyHitStun(hit);

            // SYS4 reads the push inside Hit, so it is settled first; a poise break and a lethal
            // outcome are both known by now.
            ResolveKnockback(hit, brokePoise, CurrentHP <= 0f);

            Hit?.Invoke(this, hit);

            if (CurrentHP <= 0f) Kill();
            return true;
        }

        /// <summary>§8 enemy column: the hit's own push, raised to H9 on a poise break, H7 on a kill.</summary>
        private void ResolveKnockback(in CombatHit hit, bool brokePoise, bool lethal)
        {
            float tiles = hit.Knockback.magnitude;
            HitKind kind = hit.Kind;

            if (lethal)
            {
                tiles = tuning.H7_BodyFlungTiles;
                kind = HitKind.H7_Kill;
            }
            else if (brokePoise && tuning.H9_EnemyKnockbackTiles > tiles + KnockbackEpsilonTiles)
            {
                tiles = tuning.H9_EnemyKnockbackTiles;
                kind = HitKind.H9_Stagger;
            }

            ResolvedKnockbackTiles = tiles;
            ResolvedKnockbackKind = kind;
        }

        /// <summary>
        /// A normalised 2D vector times 0.5 can measure 0.49999997, so a strict compare would relabel
        /// an H17 parry (0.5) as H9 (0.5). Only a genuinely shorter push is raised to H9.
        /// </summary>
        private const float KnockbackEpsilonTiles = 0.0001f;

        /// <summary>
        /// §6 rule 3: light tiers flinch, everyone else builds a poise meter. Returns true on the hit
        /// that breaks poise.
        /// </summary>
        private bool ApplyHitStun(in CombatHit hit)
        {
            if (IsLightTier)
            {
                flinchTimer = Mathf.Max(flinchTimer, tuning.LightFlinchSeconds);
                return false;
            }

            if (staggerTimer > 0f) return false;

            float poiseDamage = hit.PoiseDamage;
            if (attackStartup) poiseDamage *= tuning.PoiseStartupMultiplier;

            poise += poiseDamage;
            if (poise < basePoise) return false;

            poise = 0f;
            staggerTimer = tuning.PoiseStaggerSeconds;
            return true;
        }

        /// <summary>
        /// §7: applies one of the four enemy-only statuses. A duration of 0 uses the status's
        /// own spec value; elites and bosses take the −20% modifier and the CC re-application cap.
        /// </summary>
        public void ApplyStatus(CombatStatus status, float seconds = 0f)
        {
            if (IsDead || status == CombatStatus.None) return;

            float duration = seconds > 0f ? seconds : DefaultDuration(status);

            if (tier == EnemyTier.Elite || tier == EnemyTier.Boss)
            {
                duration *= tuning.EliteBoss_DurationMultiplier;
            }

            switch (status)
            {
                case CombatStatus.Stun:
                    duration = ApplyCCModifier(duration);
                    stunTimer = Mathf.Max(stunTimer, duration);
                    attackStartup = false;
                    break;

                case CombatStatus.Slow:
                    duration = ApplyCCModifier(duration);
                    slowStacks = Mathf.Min(tuning.Status_SlowMaxStacks, slowStacks + 1);
                    slowTimer = Mathf.Max(slowTimer, duration);
                    if (slowStacks >= tuning.Status_SlowMaxStacks)
                    {
                        // The 5th stack freezes briefly and resets the stacks (§7).
                        stunTimer = Mathf.Max(stunTimer, tuning.Status_SlowFreezeSeconds);
                        slowStacks = 0;
                        slowTimer = 0f;
                    }
                    break;

                case CombatStatus.Burn:
                    burnTimer = Mathf.Max(burnTimer, duration);
                    if (burnTickTimer <= 0f) burnTickTimer = tuning.Status_BurnTickSeconds;
                    break;

                case CombatStatus.Bleed:
                    bleedStacks = Mathf.Min(tuning.Status_BleedMaxStacks, bleedStacks + 1);
                    bleedTimer = Mathf.Max(bleedTimer, duration);
                    if (bleedTickTimer <= 0f) bleedTickTimer = tuning.Status_BleedTickSeconds;
                    break;
            }
        }

        private float DefaultDuration(CombatStatus status)
        {
            switch (status)
            {
                case CombatStatus.Stun: return tuning.Status_StunSeconds;
                case CombatStatus.Slow: return tuning.Status_SlowSeconds;
                case CombatStatus.Burn: return tuning.Status_BurnSeconds;
                case CombatStatus.Bleed: return tuning.Status_BleedSeconds;
                default: return 0f;
            }
        }

        /// <summary>§7 elite/boss row: crowd control gets 50% shorter on each re-application within 5 s.</summary>
        private float ApplyCCModifier(float duration)
        {
            if (tier != EnemyTier.Elite && tier != EnemyTier.Boss) return duration;

            float now = Time.time;
            if (now - lastCCApplyTime > tuning.EliteBoss_CCReapplyWindowSeconds) ccApplications = 0;
            lastCCApplyTime = now;
            ccApplications++;

            if (ccApplications <= 1) return duration;
            return duration * tuning.EliteBoss_CCReapplyMultiplier;
        }

        private void Kill()
        {
            if (IsDead) return;

            IsDead = true;
            CurrentHP = 0f;
            flinchTimer = 0f;
            staggerTimer = 0f;
            stunTimer = 0f;

            // §12 / S2-doc §1 rule 7: the killing blow removes the contact hitbox first, so a
            // simultaneous contact hit cannot land after death.
            if (contact != null) contact.enabled = false;
            if (hurtbox != null) hurtbox.enabled = false;
            if (body != null) body.color = deadTint;

            Died?.Invoke(this);
        }

        private void Update()
        {
            if (IsDead)
            {
                return;
            }

            float dt = Time.deltaTime;

            if (flinchTimer > 0f) flinchTimer = Mathf.Max(0f, flinchTimer - dt);
            if (staggerTimer > 0f) staggerTimer = Mathf.Max(0f, staggerTimer - dt);
            if (stunTimer > 0f) stunTimer = Mathf.Max(0f, stunTimer - dt);

            // §6: the poise meter decays at a fixed rate and resets on a stagger.
            if (poise > 0f && staggerTimer <= 0f)
            {
                poise = Mathf.Max(0f, poise - tuning.PoiseDecayFor(tier) * dt);
            }

            TickStatus(CombatStatus.Burn, ref burnTimer, ref burnTickTimer, tuning.Status_BurnTickSeconds, tuning.Status_BurnDamageUnits);
            TickStatus(CombatStatus.Bleed, ref bleedTimer, ref bleedTickTimer, tuning.Status_BleedTickSeconds, tuning.Status_BleedDamageUnits * Mathf.Max(1, bleedStacks));

            if (slowTimer > 0f)
            {
                slowTimer = Mathf.Max(0f, slowTimer - dt);
                if (slowTimer <= 0f) slowStacks = 0;
            }

            if (tintTimer > 0f)
            {
                tintTimer = Mathf.Max(0f, tintTimer - dt);
                if (tintTimer <= 0f && body != null) body.color = Color.white;
            }
        }

        private void TickStatus(CombatStatus status, ref float timer, ref float tickTimer, float tickSeconds, float damagePerTick)
        {
            if (timer <= 0f)
            {
                tickTimer = 0f;
                return;
            }

            timer = Mathf.Max(0f, timer - Time.deltaTime);
            tickTimer -= Time.deltaTime;
            if (tickTimer > 0f) return;

            tickTimer = tickSeconds;
            ApplyStatusDamage(status, damagePerTick);
        }

        private void ApplyStatusDamage(CombatStatus status, float units)
        {
            if (IsDead || units <= 0f) return;

            CurrentHP = Mathf.Max(0f, CurrentHP - units);
            if (CurrentHP > 0f) return;

            // A status kill has no hit behind it, so there is no H7 fling.
            ResolvedKnockbackTiles = 0f;
            ResolvedKnockbackKind = HitKind.None;
            Kill();
        }
    }
}
