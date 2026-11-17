using System;
using System.Collections;
using BastionZero.Combat;
using BastionZero.Movement;
using UnityEngine;

namespace BastionZero.Damage
{
    /// <summary>The D-1…D-5 death sequence (SYS2 §9), plus None while the hero is alive.</summary>
    public enum DeathPhase
    {
        None = 0,
        D1_Freeze = 1,
        D2_Animation = 2,
        D3_Fade = 3,
        D4_Respawn = 4,
        D5_Grace = 5,
    }

    /// <summary>
    /// The hero's pips, invulnerability windows and death sequence — SYS2 §5, §8 and §9.
    /// Every damage source in the game funnels through <see cref="TryTakeDamage"/>; nothing else
    /// is allowed to subtract pips.
    /// </summary>
    [DisallowMultipleComponent]
    public class HeroHealth : MonoBehaviour
    {
        [Tooltip("SYS2 tuning asset. Required.")]
        [SerializeField] private DamageTuning tuning;

        [Tooltip("The motor. Bound from this object when left empty.")]
        [SerializeField] private PlayerMovement movement;

        [Tooltip("Records the last safe ground the hero stood on; the pit warp and respawn use it.")]
        [SerializeField] private SafeGroundRecorder safeGround;

        [Tooltip("Owns the collision box and hurtbox. Bound from this object when left empty.")]
        [SerializeField] private HeroBodySetup body;

        [Tooltip("The sprite that flickers during i-frames. Bound from the children when left empty.")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        // SYS3 §10: the dash i-frames, the block and the parry are offered the hit before any
        // pip is taken. Empty until SYS3 exists, which leaves the system-2 behaviour unchanged.
        private IHeroDamageGuard guard;

        private DeathPhase phase = DeathPhase.None;
        private float postHitTimer;
        private float graceTimer;
        private float flickerClock;
        private int lastDamageFrame = -1;
        private float lastDamagePriority = float.MaxValue;
        private bool worldFrozen;
        private float storedTimeScale = 1f;
        private Vector2? respawnOverride;
        private Coroutine deathRoutine;

        /// <summary>Pips left. Starts at CD-17, never above CD-18.</summary>
        public int CurrentHP { get; private set; }

        /// <summary>CD-18 max pips.</summary>
        public int MaxHP => tuning != null ? tuning.CD_18_MaxHP : 0;

        /// <summary>True from D-1 until the respawn finishes (D-5 grace counts as alive).</summary>
        public bool IsDead => phase != DeathPhase.None && phase != DeathPhase.D5_Grace;

        /// <summary>Which death sub-state the hero is in.</summary>
        public DeathPhase Phase => phase;

        /// <summary>Seconds of post-hit invulnerability left (CD-24).</summary>
        public float PostHitInvulnerability => Mathf.Max(0f, postHitTimer);

        /// <summary>Seconds of all-damage invulnerability left (CD-29 warp arrival / CD-34 grace).</summary>
        public float GraceInvulnerability => Mathf.Max(0f, graceTimer);

        /// <summary>True while C24 blocks D-Light and D-Heavy; hazards still land.</summary>
        public bool IsPostHitInvulnerable => postHitTimer > 0f;

        /// <summary>True while every damage class is blocked (C29 / C34).</summary>
        public bool IsGraceInvulnerable => graceTimer > 0f;

        /// <summary>CD-38: the low-health signal is on.</summary>
        public bool IsLowHealth => CurrentHP > 0 && tuning != null && CurrentHP <= tuning.CD_38_LowHealthPips;

        /// <summary>The most recent hit, for readouts and audio.</summary>
        public DamageInfo LastDamage { get; private set; }

        /// <summary>True once the hero has been damaged at least once this run.</summary>
        public bool HasTakenDamage { get; private set; }

        /// <summary>The hero's current horizontal/vertical speed, for hazard knockback direction.</summary>
        public Vector2 Velocity => movement != null ? movement.Velocity : Vector2.zero;

        /// <summary>Fires on every accepted hit, after the pips are gone.</summary>
        public event Action<DamageInfo> Damaged;

        /// <summary>Fires whenever the pip count changes: (current, max).</summary>
        public event Action<int, int> HealthChanged;

        /// <summary>Fires once when HP reaches 0, at D-1.</summary>
        public event Action<DamageInfo> Died;

        /// <summary>Fires at D-4 once the hero is back on their feet.</summary>
        public event Action Respawned;

        private void Awake()
        {
            if (tuning == null)
            {
                Debug.LogError("[SYS2] HeroHealth has no DamageTuning assigned; the hero cannot take damage.", this);
                enabled = false;
                return;
            }

            Bind();

            CurrentHP = Mathf.Clamp(tuning.CD_17_StartHP, 1, Mathf.Max(1, tuning.CD_18_MaxHP));
        }

        private void Bind()
        {
            if (movement == null) movement = GetComponent<PlayerMovement>();
            if (safeGround == null) safeGround = GetComponent<SafeGroundRecorder>();
            if (body == null) body = GetComponent<HeroBodySetup>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
            if (guard == null) guard = GetComponent<IHeroDamageGuard>();
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            if (postHitTimer > 0f) postHitTimer = Mathf.Max(0f, postHitTimer - dt);
            if (graceTimer > 0f) graceTimer = Mathf.Max(0f, graceTimer - dt);

            TickFlicker();
        }

        private void OnDisable()
        {
            // Never leave the world paused if the hero is torn down mid-death.
            if (worldFrozen) FreezeWorld(false);
            if (spriteRenderer != null) spriteRenderer.enabled = true;
        }

        // ── Damage ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// The single entry point for damage (SYS2 §5 rule 3). Returns false when the hit was
        /// refused by an i-frame window, by the same-frame priority rule, or because the hero
        /// is already dying.
        /// </summary>
        public bool TryTakeDamage(DamageInfo info)
        {
            if (!enabled || tuning == null) return false;
            if (IsDead) return false;

            // C29 / C34: an all-damage window covers every class, hazards and pits included.
            if (graceTimer > 0f) return false;

            // C24: post-hit i-frames stop actors cold. Hazards ignore them (§8.2); a pit
            // volume ignores them too (§9 trigger table).
            if (postHitTimer > 0f && (info.Class == DamageClass.D_Light || info.Class == DamageClass.D_Heavy))
                return false;

            // §1.7: one resolution per frame, and the first (highest priority) source wins.
            if (lastDamageFrame == Time.frameCount && info.Priority >= lastDamagePriority) return false;

            // SYS3 §10: the guard is offered the hit before any pip is taken. A guard consumes the
            // frame's resolution either way, so two sources on one frame still resolve once.
            int chipPips = 0;
            if (guard != null && guard.TryGuard(ref info, out chipPips))
            {
                lastDamageFrame = Time.frameCount;
                lastDamagePriority = info.Priority;
                return ApplyChip(chipPips, info);
            }

            int pips = PipsFor(info.Class);
            if (info.Class == DamageClass.D_Lethal)
            {
                pips = CurrentHP;
            }
            else
            {
                // §5 rule 3: reduction only counts on hits of 2 or more, then the floor applies.
                int reduction = pips >= 2 ? Mathf.Max(0, tuning.CD_22_DamageReduction) : 0;
                pips = Mathf.Max(tuning.CD_23_MinDamagePerHit, pips - reduction);
            }

            lastDamageFrame = Time.frameCount;
            lastDamagePriority = info.Priority;

            CurrentHP = Mathf.Max(0, CurrentHP - pips);
            LastDamage = info;
            HasTakenDamage = true;

            if (CurrentHP > 0 && info.Class != DamageClass.D_Lethal)
                postHitTimer = Mathf.Max(postHitTimer, tuning.CD_24_PostHitIFrames);

            Damaged?.Invoke(info);
            HealthChanged?.Invoke(CurrentHP, MaxHP);

            if (CurrentHP <= 0)
            {
                Died?.Invoke(info);
                BeginDeath();
            }
            else
            {
                ApplyKnockback(info);
            }

            return true;
        }

        /// <summary>
        /// §5.3: chip from a blocked D-Heavy. It is damage, so the C24 window starts as normal,
        /// but it never takes the last pip — the hero cannot be killed through a guard.
        /// </summary>
        private bool ApplyChip(int chipPips, DamageInfo info)
        {
            int applied = Mathf.Min(Mathf.Max(0, chipPips), Mathf.Max(0, CurrentHP - 1));
            if (applied <= 0) return false;

            CurrentHP -= applied;
            LastDamage = info;
            HasTakenDamage = true;
            postHitTimer = Mathf.Max(postHitTimer, tuning.CD_24_PostHitIFrames);

            Damaged?.Invoke(info);
            HealthChanged?.Invoke(CurrentHP, MaxHP);
            return true;
        }

        /// <summary>Pip cost of a damage class, straight from the CD-19…CD-41 fields.</summary>
        public int PipsFor(DamageClass damageClass)
        {
            if (tuning == null) return 0;

            switch (damageClass)
            {
                case DamageClass.D_Light: return tuning.CD_19_DLightPips;
                case DamageClass.D_Heavy: return tuning.CD_20_DHeavyPips;
                case DamageClass.D_Hazard: return tuning.CD_21_DHazardPips;
                case DamageClass.D_Pit: return tuning.CD_41_DPitPips;
                case DamageClass.D_Lethal: return CurrentHP;
                default: return 0;
            }
        }

        /// <summary>Grants the C29 all-damage window, which hazards are also blocked by.</summary>
        public void GrantArrivalInvulnerability(float seconds)
        {
            graceTimer = Mathf.Max(graceTimer, Mathf.Max(0f, seconds));
        }

        /// <summary>Healing for SYS6's flask. Clamped to CD-18.</summary>
        public void Heal(int pips)
        {
            if (pips <= 0 || IsDead) return;

            int before = CurrentHP;
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + pips);
            if (CurrentHP != before) HealthChanged?.Invoke(CurrentHP, MaxHP);
        }

        private void ApplyKnockback(DamageInfo info)
        {
            if (movement == null || tuning == null) return;

            movement.ApplyKnockback(
                KnockbackImpulse(info),
                tuning.CD_S49_KnockbackDuration,
                tuning.CD_27_KnockbackHoldFraction);
        }

        /// <summary>SYS2 §7 rule 1: deterministic knockback, no seeded direction.</summary>
        public Vector2 KnockbackImpulse(DamageInfo info)
        {
            if (tuning == null || movement == null) return Vector2.zero;

            bool facingLeft = movement.IsFacingLeft;
            float back = facingLeft ? 1f : -1f;

            float horizontal;
            switch (info.Surface)
            {
                case HazardSurface.Floor:
                    horizontal = Mathf.Abs(info.EntryVelocityX) > 0.01f
                        ? -Mathf.Sign(info.EntryVelocityX)
                        : back;
                    break;

                case HazardSurface.LeftWall:
                    horizontal = 1f;
                    break;

                case HazardSurface.RightWall:
                    horizontal = -1f;
                    break;

                case HazardSurface.Ceiling:
                    horizontal = Mathf.Abs(info.EntryVelocityX) > 0.01f
                        ? -Mathf.Sign(info.EntryVelocityX)
                        : 0f;
                    break;

                default:
                    float dx = transform.position.x - info.SourcePosition.x;
                    horizontal = Mathf.Abs(dx) < tuning.CD_28_KnockbackTieZone ? back : Mathf.Sign(dx);
                    break;
            }

            float vertical = info.Surface == HazardSurface.Ceiling ? -1f : 1f;

            return new Vector2(
                horizontal * tuning.CD_S50_KnockbackHorizontal,
                vertical * tuning.CD_S50_KnockbackUp);
        }

        // ── Death and respawn (SYS2 §9) ─────────────────────────────────────────────────

        private void BeginDeath()
        {
            if (deathRoutine != null) StopCoroutine(deathRoutine);
            deathRoutine = StartCoroutine(DeathRoutine());
        }

        private IEnumerator DeathRoutine()
        {
            // D-1 — lethal-hit freeze: input off, both boxes off, world hit-pause.
            phase = DeathPhase.D1_Freeze;
            postHitTimer = 0f;
            if (movement != null) movement.enabled = false;
            if (safeGround != null) safeGround.enabled = false;
            if (body != null) body.SetCollidersEnabled(false);
            FreezeWorld(true);

            yield return new WaitForSecondsRealtime(tuning.CD_30_DeathD1Freeze);

            FreezeWorld(false);

            // D-2 — death animation: the body stays where it died, the camera holds.
            phase = DeathPhase.D2_Animation;
            yield return new WaitForSecondsRealtime(tuning.CD_31_DeathD2Animation);

            // D-3 — fade. SYS6 owns the coin penalty and the death marker; both hook here.
            phase = DeathPhase.D3_Fade;
            ScreenFade fade = ScreenFade.Ensure();
            yield return fade.FadeTo(1f, tuning.CD_32_DeathD3Fade);

            // D-4 — respawn load: full HP at the respawn point.
            phase = DeathPhase.D4_Respawn;
            Respawn();
            yield return fade.FadeTo(0f, tuning.CD_32_DeathD3Fade);

            // D-5 — respawn grace: everything is blocked, input is back on.
            phase = DeathPhase.D5_Grace;
            graceTimer = Mathf.Max(graceTimer, tuning.CD_34_RespawnGraceIFrames);
            Respawned?.Invoke();

            yield return new WaitForSecondsRealtime(tuning.CD_34_RespawnGraceIFrames);

            if (phase == DeathPhase.D5_Grace) phase = DeathPhase.None;
            deathRoutine = null;
        }

        private void Respawn()
        {
            Vector2 point = RespawnPoint;

            if (movement != null)
            {
                movement.enabled = true;
                movement.PlaceAt(point);
            }

            if (body != null) body.SetCollidersEnabled(true);

            CurrentHP = tuning.CD_33_RespawnFullHP ? MaxHP : Mathf.Max(1, CurrentHP);
            HealthChanged?.Invoke(CurrentHP, MaxHP);

            // Re-enable only after the hero is standing at the spawn point, so the recorder
            // cannot mistake the death site for safe ground.
            if (safeGround != null) safeGround.enabled = true;
        }

        /// <summary>
        /// Where D-4 puts the hero. X17: the later of the last checkpoint and the last rest
        /// point; neither exists until SYS5/SYS6, so this falls back to the last safe ground.
        /// SYS5 checkpoints call <see cref="SetRespawnOverride"/>.
        /// </summary>
        public Vector2 RespawnPoint
        {
            get
            {
                if (respawnOverride.HasValue) return respawnOverride.Value;
                if (safeGround != null && safeGround.HasRecorded) return safeGround.LastSafeGround;
                if (safeGround != null) return safeGround.WarpTarget;
                return transform.position;
            }
        }

        /// <summary>Pins the respawn point (SYS5 checkpoint / SYS6 rest point).</summary>
        public void SetRespawnOverride(Vector2? point)
        {
            respawnOverride = point;
        }

        private void FreezeWorld(bool freeze)
        {
            if (freeze)
            {
                if (worldFrozen) return;
                storedTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                worldFrozen = true;
                return;
            }

            if (!worldFrozen) return;
            Time.timeScale = storedTimeScale <= 0f ? 1f : storedTimeScale;
            worldFrozen = false;
        }

        // ── Feedback (CD-37) ────────────────────────────────────────────────────────────

        private void TickFlicker()
        {
            if (spriteRenderer == null || tuning == null) return;

            bool invulnerable = postHitTimer > 0f || graceTimer > 0f;
            if (!invulnerable)
            {
                if (!spriteRenderer.enabled) spriteRenderer.enabled = true;
                flickerClock = 0f;
                return;
            }

            // CD-37 is blinks per second, so the sprite toggles at twice that rate.
            flickerClock += Time.unscaledDeltaTime;
            float hz = Mathf.Max(0.01f, tuning.CD_37_IFrameFlickerHz);
            bool visible = Mathf.FloorToInt(flickerClock * hz * 2f) % 2 == 0;
            spriteRenderer.enabled = visible;
        }
    }
}
