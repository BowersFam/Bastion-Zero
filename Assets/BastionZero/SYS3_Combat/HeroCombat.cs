using System;
using BastionZero.Core;
using BastionZero.Damage;
using BastionZero.Movement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BastionZero.Combat
{
    /// <summary>The SYS3 §11 combat states. Movement states stay in <see cref="MovementState"/>.</summary>
    public enum CombatState
    {
        K_IDLE = 0,
        G1 = 1,
        G2 = 2,
        G3 = 3,
        AIR1 = 4,
        WALL1 = 5,
        C_DASH = 6,
        BK_RAISE = 7,
        BK_HOLD = 8,
        BK_STUN = 9,
        BK_RELEASE = 10,
        BK_BREAK = 11,
    }

    /// <summary>
    /// SYS3's combat overlay: the ground chain and context attacks (§3), buffering and same-frame
    /// priority (§3.3, §3.5), the dash (§5.2), the block and its parry window (§5.3–§5.4), the
    /// deterministic crits and feel rows (§7, §8) and the player side of the damage model (§9).
    ///
    /// The overlay never blocks movement (S1 overlay rule): every attack, block and dash is timed
    /// here and driven into the SYS1 motor through its speed-override hook or read from its state.
    /// Frame windows are counted in whole 60 Hz frames, one per FixedUpdate, so the spec's frame
    /// tables are exact.
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    [DisallowMultipleComponent]
    public class HeroCombat : MonoBehaviour, IHeroDamageGuard
    {
        private const string AttackActionName = "Player/Attack";
        private const string DashActionName = "Player/Dash";
        private const string BlockActionName = "Player/Block";
        private const double NoBuffer = -999d;

        private static readonly int AttackParam = Animator.StringToHash("Attack");
        private static readonly int AttackChainParam = Animator.StringToHash("AttackChain");

        [Header("References")]
        [SerializeField] private CombatTuning tuning;
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private HeroHealth health;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private Animator animator;
        [SerializeField] private SlashVolume2D slash;
        [SerializeField] private HitStopController hitStop;
        [SerializeField] private CombatCameraShake cameraShake;
        [SerializeField] private NexusFeelFeedback feel;

        [Header("Slash placement (1 tile = 1 unit, authored facing right)")]
        [SerializeField] private Vector2 groundSlashOffset = new Vector2(0.62f, 0.05f);
        [SerializeField] private Vector2 groundSlashSize = new Vector2(1.1f, 0.9f);
        [SerializeField] private Vector2 airSlashOffset = new Vector2(0.55f, 0f);
        [SerializeField] private Vector2 airSlashSize = new Vector2(1.0f, 1.1f);
        [SerializeField] private Vector2 wallSlashOffset = new Vector2(0.55f, 0f);
        [SerializeField] private Vector2 wallSlashSize = new Vector2(1.0f, 1.0f);

        [Header("Weapon (SYS6 grants the tiers)")]
        [SerializeField, Range(1, 5)] private int weaponTier = 1;

        private InputAction attackAction;
        private InputAction dashAction;
        private InputAction blockAction;
        private bool inputReady;

        private CombatState state = CombatState.K_IDLE;
        private int frameInState = 1;

        // The attack that is running, resolved from the tuning asset when it starts.
        private AttackId currentAttack = AttackId.None;
        private int attackStartup;
        private int attackActive;
        private int attackRecovery;
        private int attackChainFrom = -1;
        private float attackMultiplier = 1f;
        private HitKind baseKind = HitKind.H1_GroundChain;
        private bool chainWindowOpen;
        private bool slashOpen;
        private bool slashMirror;

        // Buffers, memory and timers.
        private double attackBufferedAt = NoBuffer;
        private double dashBufferedAt = NoBuffer;
        private double blockBufferedAt = NoBuffer;
        private bool blockHeld;
        private float comboMemory;
        private int chainIndex;
        private float dashCooldown;
        private int airborneHits;

        // Guard.
        private int guardPoints;
        private float guardRegenAccumulator;
        private float guardRegenDelay;
        private float parryLockout;
        private float parryCoverTimer;
        private bool parryWindowOpen;
        private bool parriedThisHold;
        private float blockstunTimer;
        private float blockstunPushSpeed;
        private float guardBreakTimer;
        private bool chipInProgress;

        // ── Readout surface ─────────────────────────────────────────────────────────

        public CombatState State => state;
        public int FrameInState => frameInState;
        public AttackId CurrentAttack => currentAttack;
        public bool IsAttacking => IsAttackState();
        public bool IsDashing => state == CombatState.C_DASH;
        public bool IsGuarding => state == CombatState.BK_HOLD || state == CombatState.BK_STUN;
        public bool IsGuardBroken => state == CombatState.BK_BREAK;
        public bool IsParryWindowOpen => parryWindowOpen;
        public int GuardPoints => guardPoints;
        public int GuardMaxPoints => tuning != null ? tuning.Block_GuardPoints : 0;
        public float GuardBreakRemaining => Mathf.Max(0f, guardBreakTimer);
        public float BlockstunRemaining => Mathf.Max(0f, blockstunTimer);

        /// <summary>Speed the blockstun push is applying, in units per second.</summary>
        public float BlockstunPushSpeed => blockstunPushSpeed;
        public float ComboMemoryRemaining => Mathf.Max(0f, comboMemory);
        public int ChainIndex => chainIndex;
        public float DashCooldownRemaining => Mathf.Max(0f, dashCooldown);
        public int AirborneHitChain => airborneHits;
        public int WeaponTier => weaponTier;

        /// <summary>DB-1: true on frames 3–10 of the dash, against enemy sources only.</summary>
        public bool DashIFramesActive =>
            state == CombatState.C_DASH
            && tuning != null
            && frameInState >= tuning.Dash_IFrameStartFrame
            && frameInState <= tuning.Dash_IFrameEndFrame;

        public HitKind LastHitKind { get; private set; }
        public float LastDamageDealt { get; private set; }
        public bool LastHitWasCrit { get; private set; }
        public int HitsLanded { get; private set; }
        public int CritsLanded { get; private set; }
        public int BlocksLanded { get; private set; }
        public int ParriesLanded { get; private set; }
        public int DashIFrameNegations { get; private set; }
        public string LastEvent { get; private set; } = "—";

        // ── Lifecycle ───────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (movement == null) movement = GetComponent<PlayerMovement>();
            if (health == null) health = GetComponent<HeroHealth>();
            if (playerInput == null) playerInput = GetComponent<PlayerInput>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (slash == null) slash = GetComponentInChildren<SlashVolume2D>(true);
            if (hitStop == null) hitStop = GetComponentInChildren<HitStopController>(true);
            if (cameraShake == null) cameraShake = FindAnyObjectByType<CombatCameraShake>(FindObjectsInactive.Include);
            if (feel == null) feel = GetComponent<NexusFeelFeedback>();

            // SYS3 supersedes the prototype slash driver: two components must never both consume
            // Player/Attack, and the chain owns the hit window now.
            NexusAttack legacy = GetComponent<NexusAttack>();
            if (legacy != null)
            {
                legacy.enabled = false;
                Debug.Log($"[SYS3] HeroCombat supersedes NexusAttack on '{name}'; the prototype slash driver is disabled.", this);
            }

            if (tuning == null)
            {
                Debug.LogError("[SYS3] HeroCombat has no CombatTuning assigned; combat disabled.", this);
                enabled = false;
                return;
            }

            if (playerInput == null || playerInput.actions == null)
            {
                Debug.LogError($"[SYS3] '{name}' has no PlayerInput with an actions asset; combat disabled.", this);
                enabled = false;
                return;
            }

            attackAction = playerInput.actions.FindAction(AttackActionName);
            dashAction = playerInput.actions.FindAction(DashActionName);
            blockAction = playerInput.actions.FindAction(BlockActionName);

            if (attackAction == null)
            {
                Debug.LogError($"[SYS3] Could not find '{AttackActionName}' in '{playerInput.actions.name}'. Combat disabled.", this);
                enabled = false;
                return;
            }

            if (dashAction == null || blockAction == null)
            {
                Debug.LogWarning(
                    $"[SYS3] '{DashActionName}' and/or '{BlockActionName}' are missing from " +
                    $"'{playerInput.actions.name}'; dash and block stay unavailable.", this);
            }

            guardPoints = tuning.Block_GuardPoints;
            inputReady = true;

            if (movement != null)
            {
                movement.Landed += OnLanded;
                movement.StateChanged += OnMovementStateChanged;
            }

            if (health != null) health.Damaged += OnHeroDamaged;
        }

        private void OnEnable()
        {
            if (!inputReady) return;

            if (attackAction != null) attackAction.performed += OnAttackInput;
            if (dashAction != null) dashAction.performed += OnDashInput;
            if (blockAction != null)
            {
                blockAction.performed += OnBlockPressed;
                blockAction.canceled += OnBlockReleased;
            }
        }

        private void OnDisable()
        {
            if (attackAction != null) attackAction.performed -= OnAttackInput;
            if (dashAction != null) dashAction.performed -= OnDashInput;
            if (blockAction != null)
            {
                blockAction.performed -= OnBlockPressed;
                blockAction.canceled -= OnBlockReleased;
            }

            blockHeld = false;
            if (slash != null) slashOpen = false;
            ResetAttackAnimator();
        }

        private void OnDestroy()
        {
            if (movement != null)
            {
                movement.Landed -= OnLanded;
                movement.StateChanged -= OnMovementStateChanged;
            }

            if (health != null) health.Damaged -= OnHeroDamaged;
        }

        // ── Input ───────────────────────────────────────────────────────────────────

        private void OnAttackInput(InputAction.CallbackContext context)
        {
            if (context.performed) attackBufferedAt = Now();
        }

        private void OnDashInput(InputAction.CallbackContext context)
        {
            if (context.performed) dashBufferedAt = Now();
        }

        private void OnBlockPressed(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            blockBufferedAt = Now();
            blockHeld = true;
        }

        private void OnBlockReleased(InputAction.CallbackContext context)
        {
            blockHeld = false;
        }

        private static double Now() => Time.realtimeSinceStartupAsDouble;

        private static double Age(double stamp) => Now() - stamp;

        // ── Frame machine ───────────────────────────────────────────────────────────

        private void FixedUpdate()
        {
            if (!inputReady || tuning == null) return;

            StepFrame();
        }

        private void StepFrame()
        {
            if (health != null && health.IsDead)
            {
                if (state != CombatState.K_IDLE) AbortToIdle();
                return;
            }

            TickTimers();

            if (IsAttackState()) StepAttackFrame();
            else if (state == CombatState.C_DASH) StepDashFrame();
            else if (IsGuardState()) StepGuardFrame();

            ResolveInput();
        }

        private void TickTimers()
        {
            float dt = SpecUnits.FixedDeltaTime;

            if (dashCooldown > 0f) dashCooldown = Mathf.Max(0f, dashCooldown - dt);
            if (parryLockout > 0f) parryLockout = Mathf.Max(0f, parryLockout - dt);
            if (parryCoverTimer > 0f) parryCoverTimer = Mathf.Max(0f, parryCoverTimer - dt);

            if (comboMemory > 0f)
            {
                comboMemory = Mathf.Max(0f, comboMemory - dt);
                if (comboMemory <= 0f) chainIndex = 0;
            }

            // §5.3: 1 guard point per 0.5 s, once 0.75 s have passed with no blocked hit.
            if (guardPoints < tuning.Block_GuardPoints)
            {
                if (guardRegenDelay > 0f)
                {
                    guardRegenDelay = Mathf.Max(0f, guardRegenDelay - dt);
                }
                else
                {
                    guardRegenAccumulator += tuning.Block_GuardRegenPointsPerSecond * dt;
                    while (guardRegenAccumulator >= 1f && guardPoints < tuning.Block_GuardPoints)
                    {
                        guardPoints++;
                        guardRegenAccumulator -= 1f;
                    }
                }
            }
            else
            {
                guardRegenAccumulator = 0f;
            }

            if (blockstunTimer > 0f) blockstunTimer = Mathf.Max(0f, blockstunTimer - dt);
            if (guardBreakTimer > 0f) guardBreakTimer = Mathf.Max(0f, guardBreakTimer - dt);

            if (Age(attackBufferedAt) > tuning.AttackBufferSeconds) attackBufferedAt = NoBuffer;
            if (Age(dashBufferedAt) > tuning.DashBufferSeconds) dashBufferedAt = NoBuffer;
            if (Age(blockBufferedAt) > tuning.BlockBufferSeconds) blockBufferedAt = NoBuffer;
        }

        /// <summary>§3.5 same-frame priority: dash, jump (motor-owned), attack, block.</summary>
        private void ResolveInput()
        {
            bool dashBuffered = Age(dashBufferedAt) <= tuning.DashBufferSeconds;
            if (dashBuffered && TryStartDash())
            {
                dashBufferedAt = NoBuffer;
                // A dash on the same frame as an attack consumes the frame; the attack is dropped.
                attackBufferedAt = NoBuffer;
                blockBufferedAt = NoBuffer;
                return;
            }

            bool attackBuffered = Age(attackBufferedAt) <= tuning.AttackBufferSeconds;
            if (attackBuffered && TryStartAttack())
            {
                attackBufferedAt = NoBuffer;
                return;
            }

            bool blockPressed = Age(blockBufferedAt) <= tuning.BlockBufferSeconds;
            if (blockPressed && TryStartBlock()) blockBufferedAt = NoBuffer;
        }

        // ── Attacks (§3) ────────────────────────────────────────────────────────────

        private void StepAttackFrame()
        {
            int activeFirst = attackStartup + 1;
            int activeLast = attackStartup + attackActive;
            int recoveryFirst = activeLast + 1;
            int recoveryLast = activeLast + attackRecovery;

            if (frameInState == activeFirst)
            {
                slash?.BeginSwing();
                slashOpen = true;
            }

            if (slashOpen) PollSlash();

            if (frameInState >= activeLast) slashOpen = false;

            // §3.1: a buffered attack may advance the chain from recovery frame 4.
            chainWindowOpen = attackChainFrom > 0
                              && frameInState >= recoveryFirst + attackChainFrom - 1;

            if (frameInState >= recoveryLast) FinishAttack();
            else frameInState++;
        }

        private void FinishAttack()
        {
            ResetAttackAnimator();

            bool hasNext = NextChainHit(currentAttack) != AttackId.None;
            chainIndex = hasNext ? ChainIndexOf(currentAttack) : 0;
            comboMemory = hasNext ? tuning.ComboMemorySeconds : 0f;

            currentAttack = AttackId.None;
            chainWindowOpen = false;
            slashOpen = false;
            SetState(CombatState.K_IDLE);
        }

        private bool TryStartAttack()
        {
            if (movement != null &&
                (movement.State == MovementState.H_KNOCK || movement.State == MovementState.H_WARP))
            {
                return false;
            }

            if (state == CombatState.C_DASH) return false;

            AttackId next;
            if (IsAttackState())
            {
                if (!chainWindowOpen) return false;
                next = NextChainHit(currentAttack);
            }
            else if (state == CombatState.K_IDLE)
            {
                next = ContextAttack();
            }
            else
            {
                return false;
            }

            if (next == AttackId.None) return false;

            BeginAttack(next);
            return true;
        }

        /// <summary>Picks the attack the hero's S1 state allows (§3.2).</summary>
        private AttackId ContextAttack()
        {
            if (movement != null && movement.IsClinging) return AttackId.WALL1;
            if (movement != null && !movement.IsGrounded) return AttackId.AIR1;

            switch (chainIndex)
            {
                case 1: return AttackId.G2;
                case 2: return AttackId.G3;
                default: return AttackId.G1;
            }
        }

        private static AttackId NextChainHit(AttackId attack)
        {
            switch (attack)
            {
                case AttackId.G1: return AttackId.G2;
                case AttackId.G2: return AttackId.G3;
                default: return AttackId.None;
            }
        }

        private static int ChainIndexOf(AttackId attack)
        {
            switch (attack)
            {
                case AttackId.G1: return 1;
                case AttackId.G2: return 2;
                case AttackId.G3: return 3;
                default: return 0;
            }
        }

        private void BeginAttack(AttackId attack)
        {
            currentAttack = attack;
            chainWindowOpen = false;
            parriedThisHold = false;

            switch (attack)
            {
                case AttackId.G1:
                    attackStartup = tuning.G1_StartupFrames;
                    attackActive = tuning.G1_ActiveFrames;
                    attackRecovery = tuning.G1_RecoveryFrames;
                    attackChainFrom = tuning.G1_NextHitCancelFromRecoveryFrame;
                    attackMultiplier = tuning.G1_DamageMultiplier;
                    baseKind = HitKind.H1_GroundChain;
                    PoseSlash(groundSlashOffset, groundSlashSize, movement != null && movement.IsFacingLeft);
                    break;

                case AttackId.G2:
                    attackStartup = tuning.G2_StartupFrames;
                    attackActive = tuning.G2_ActiveFrames;
                    attackRecovery = tuning.G2_RecoveryFrames;
                    attackChainFrom = tuning.G2_NextHitCancelFromRecoveryFrame;
                    attackMultiplier = tuning.G2_DamageMultiplier;
                    baseKind = HitKind.H1_GroundChain;
                    PoseSlash(groundSlashOffset, groundSlashSize, movement != null && movement.IsFacingLeft);
                    break;

                case AttackId.G3:
                    attackStartup = tuning.G3_StartupFrames;
                    attackActive = tuning.G3_ActiveFrames;
                    attackRecovery = tuning.G3_RecoveryFrames;
                    attackChainFrom = tuning.G3_NextHitCancelFromRecoveryFrame;
                    attackMultiplier = tuning.G3_DamageMultiplier;
                    baseKind = HitKind.H2_ChainFinisher;
                    PoseSlash(groundSlashOffset, groundSlashSize, movement != null && movement.IsFacingLeft);
                    break;

                case AttackId.AIR1:
                    attackStartup = tuning.AIR1_StartupFrames;
                    attackActive = tuning.AIR1_ActiveFrames;
                    attackRecovery = tuning.AIR1_RecoveryFrames;
                    attackChainFrom = -1;
                    attackMultiplier = tuning.AIR1_DamageMultiplier;
                    baseKind = HitKind.H4_Airborne;
                    PoseSlash(airSlashOffset, airSlashSize, movement != null && movement.IsFacingLeft);
                    break;

                case AttackId.WALL1:
                    attackStartup = tuning.WALL1_StartupFrames;
                    attackActive = tuning.WALL1_ActiveFrames;
                    attackRecovery = tuning.WALL1_RecoveryFrames;
                    attackChainFrom = -1;
                    attackMultiplier = tuning.WALL1_DamageMultiplier;
                    baseKind = HitKind.H4_Airborne;
                    // §3.2: WALL1 hits away from the wall only.
                    PoseSlash(wallSlashOffset, wallSlashSize, movement != null && movement.ClingSide > 0);
                    break;
            }

            SetState(ToCombatState(attack));

            // Which swing of the chain this is. The int and the trigger are written in the same
            // frame the logic commits to the swing, so the controller's G1/G2/G3 node can never
            // disagree with the chain index the damage model is using.
            if (animator != null) animator.SetInteger(AttackChainParam, ChainIndexOf(attack));

            // The controller has ground attack nodes only; an air or wall swing has no
            // animation node yet, and firing the trigger there would leave it stale (NexusAttack
            // documents the same trap). Air-attack art is logged as a SYS3 gap.
            if (animator != null && movement != null && movement.IsGrounded)
            {
                animator.SetTrigger(AttackParam);
            }

            LastEvent = "Attack " + attack;
        }

        private void PoseSlash(Vector2 offset, Vector2 size, bool mirror)
        {
            slashMirror = mirror;
            if (slash != null) slash.Pose(offset, size);
        }

        private void PollSlash()
        {
            if (slash == null) return;

            var targets = slash.Poll(slashMirror);
            for (int i = 0; i < targets.Count; i++)
            {
                IAttackTarget2D target = targets[i];
                slash.MarkHit(target);

                if (target == null || target.IsDead) continue;

                CombatHit hit = BuildHit(target);
                if (!target.ReceiveHit(hit)) continue;

                OnHitLanded(target, hit);
            }
        }

        /// <summary>§9 rule 1 plus the three deterministic crit triggers of §7.</summary>
        private CombatHit BuildHit(IAttackTarget2D target)
        {
            bool behind = IsBehind(target);
            CritTrigger crit = CritTrigger.None;

            if (target.IsStaggered) crit = CritTrigger.StaggeredTarget;
            else if (IsAirborneAttack(currentAttack) && airborneHits >= 2) crit = CritTrigger.AirborneChain;
            else if (IsGroundChain(currentAttack) && target.IsInAttackStartup && behind) crit = CritTrigger.BehindDuringStartup;

            float damage = tuning.W_BaseUnits * tuning.WeaponTierMultiplier(weaponTier) * attackMultiplier;
            if (crit != CritTrigger.None) damage *= tuning.Crit_Multiplier;

            HitKind kind = crit != CritTrigger.None ? HitKind.H3_Crit : baseKind;
            float knockbackTiles = EnemyKnockbackTiles(kind, target);

            Vector2 away = (Vector2)target.Position - (Vector2)transform.position;
            away = away.sqrMagnitude > 0.0001f ? away.normalized : Vector2.right;

            return new CombatHit
            {
                Attack = currentAttack,
                Kind = kind,
                Damage = damage,
                PoiseDamage = tuning.PoiseBasePerHit * attackMultiplier,
                Crit = crit,
                IsCrit = crit != CritTrigger.None,
                Origin = transform.position,
                Knockback = away * knockbackTiles,
                FromBehind = behind,
            };
        }

        private bool IsBehind(IAttackTarget2D target)
        {
            float dx = transform.position.x - target.Position.x;
            if (Mathf.Abs(dx) < 0.0001f) return false;
            return target.IsFacingRight ? dx < 0f : dx > 0f;
        }

        private float EnemyKnockbackTiles(HitKind kind, IAttackTarget2D target)
        {
            bool light = target != null && target.Tier == EnemyTier.Fodder;

            switch (kind)
            {
                case HitKind.H1_GroundChain: return light ? tuning.H1_EnemyKnockbackTiles : 0f;
                case HitKind.H2_ChainFinisher: return tuning.H2_EnemyKnockbackTiles;
                case HitKind.H3_Crit: return tuning.H3_EnemyKnockbackTiles;
                case HitKind.H4_Airborne: return tuning.H4_EnemyKnockbackTiles;
                default: return 0f;
            }
        }

        private void OnHitLanded(IAttackTarget2D target, CombatHit hit)
        {
            HitsLanded++;
            LastHitKind = hit.Kind;
            LastDamageDealt = hit.Damage;
            LastHitWasCrit = hit.IsCrit;
            if (hit.IsCrit) CritsLanded++;

            LastEvent = (hit.IsCrit ? "CRIT " + hit.Crit : "Hit " + hit.Kind) + " " + hit.Damage.ToString("0");

            // The prototype NexusAttack used to raise the hitbox event this component listens to;
            // SYS3 disabled it, so the spark, slash clip and nudge are driven from here instead.
            feel?.PlaySlashFeedback(target.Position);

            GameObject targetGo = target is Component component ? component.gameObject : null;
            bool killed = target.IsDead;

            if (hit.IsCrit)
            {
                // H3: 1 f of global freeze, then the crit slow-down.
                hitStop?.PlayGlobal(tuning.H3_GlobalFreezeFrames, tuning.H3_SlowSeconds, tuning.H3_SlowTimeScale);
                cameraShake?.Shake(tuning.H3_ShakeAmplitudePixels, tuning.H3_ShakeSeconds);

                // H7 still rides on a killing crit, as a local stop on top of the global one.
                if (killed)
                {
                    hitStop?.PlayLocal(tuning.H7_ExtraStopFrames, gameObject, targetGo);
                    cameraShake?.Shake(tuning.H7_ExtraShakePixels, tuning.H7_ExtraShakeSeconds);
                }
            }
            else
            {
                ApplyHitFeel(hit.Kind, targetGo, killed);
            }

            // §6 H9 rides on the hit that breaks poise.
            if (target.IsStaggered)
            {
                ApplyHitFeel(HitKind.H9_Stagger, targetGo, false);
                LastEvent += " STAGGER";
            }

            if (killed) LastEvent += " KILL";

            // §3.2 AIR1: a connect grants the stored air-jump (S1 §3) and feeds the bounce chain.
            if (currentAttack == AttackId.AIR1)
            {
                airborneHits++;
                movement?.GrantBounceAirJump();
            }
        }

        /// <summary>
        /// Applies one §8 row. <paramref name="killing"/> folds the H7 kill bonus into the same
        /// stop and shake instead of playing a second, separate one.
        /// </summary>
        private void ApplyHitFeel(HitKind kind, GameObject targetGo, bool killing)
        {
            int attackerStop;
            int targetStop;
            float amplitude;
            float seconds;

            switch (kind)
            {
                case HitKind.H1_GroundChain:
                    attackerStop = tuning.H1_StopAttackerFrames;
                    targetStop = tuning.H1_StopTargetFrames;
                    amplitude = tuning.H1_ShakeAmplitudePixels;
                    seconds = tuning.H1_ShakeSeconds;
                    break;

                case HitKind.H2_ChainFinisher:
                    attackerStop = tuning.H2_StopAttackerFrames;
                    targetStop = tuning.H2_StopTargetFrames;
                    amplitude = tuning.H2_ShakeAmplitudePixels;
                    seconds = tuning.H2_ShakeSeconds;
                    break;

                case HitKind.H4_Airborne:
                    attackerStop = tuning.H4_StopAttackerFrames;
                    targetStop = tuning.H4_StopTargetFrames;
                    amplitude = tuning.H4_ShakeAmplitudePixels;
                    seconds = tuning.H4_ShakeSeconds;
                    break;

                case HitKind.H9_Stagger:
                    attackerStop = tuning.H9_StopAttackerFrames;
                    targetStop = tuning.H9_StopTargetFrames;
                    amplitude = tuning.H9_ShakeAmplitudePixels;
                    seconds = tuning.H9_ShakeSeconds;
                    break;

                case HitKind.H16_BlockLight:
                    attackerStop = tuning.H16_StopAttackerFrames;
                    targetStop = tuning.H16_StopTargetFrames;
                    amplitude = tuning.H16_ShakeAmplitudePixels;
                    seconds = tuning.H16_ShakeSeconds;
                    break;

                case HitKind.H17_Parry:
                    attackerStop = tuning.H17_StopAttackerFrames;
                    targetStop = tuning.H17_StopTargetFrames;
                    amplitude = tuning.H17_ShakeAmplitudePixels;
                    seconds = tuning.H17_ShakeSeconds;
                    break;

                case HitKind.H18_BlockHeavy:
                    attackerStop = tuning.H18_StopAttackerFrames;
                    targetStop = tuning.H18_StopTargetFrames;
                    amplitude = tuning.H18_ShakeAmplitudePixels;
                    seconds = tuning.H18_ShakeSeconds;
                    break;

                default:
                    return;
            }

            if (killing)
            {
                attackerStop += tuning.H7_ExtraStopFrames;
                targetStop += tuning.H7_ExtraStopFrames;
                amplitude += tuning.H7_ExtraShakePixels;
                seconds += tuning.H7_ExtraShakeSeconds;
            }

            hitStop?.PlayLocal(attackerStop, targetStop, gameObject, targetGo);
            cameraShake?.Shake(amplitude, seconds);
        }

        // ── Dash (§5.2, C-DASH) ─────────────────────────────────────────────────────

        private bool TryStartDash()
        {
            if (tuning == null) return false;
            if (state == CombatState.C_DASH) return false;
            if (dashCooldown > 0f) return false;
            if (IsGuardState() && state == CombatState.BK_BREAK) return false;

            if (movement != null)
            {
                if (movement.State == MovementState.H_KNOCK || movement.State == MovementState.H_WARP) return false;

                // No dash on the active frames of an attack (§5.2 entry list).
                if (IsAttackState() && IsInActiveWindow()) return false;

                // DB-3: the air dash is a later unlock; before it, DASH in the air does nothing.
                if (!movement.IsGrounded && !tuning.Dash_AirDashUnlocked) return false;
            }

            StartDash();
            return true;
        }

        private void StartDash()
        {
            bool facingLeft = movement != null && movement.IsFacingLeft;
            float direction = facingLeft ? -1f : 1f;

            // A dash out of a guard drops the guard (§5.2 entry list).
            if (IsGuardState()) EndGuard();

            if (movement != null)
            {
                movement.BeginSpeedOverride(
                    direction * tuning.Dash_SpeedTilesPerSecond,
                    CombatTuning.Frames(tuning.Dash_DurationFrames),
                    true);
            }

            SetState(CombatState.C_DASH);
            LastEvent = "C-DASH";
        }

        private void StepDashFrame()
        {
            if (frameInState >= tuning.Dash_DurationFrames)
            {
                EndDash();
                return;
            }

            frameInState++;
        }

        private void EndDash()
        {
            dashCooldown = tuning.Dash_CooldownSeconds;

            // §5.2 exit momentum: run speed if a direction is held, otherwise stop.
            float exitSpeed = 0f;
            if (movement != null)
            {
                float input = movement.MoveInput.x;
                if (Mathf.Abs(input) > 0.01f) exitSpeed = Mathf.Sign(input) * movement.Tuning.MV_01_RunSpeed;
                movement.EndSpeedOverride(exitSpeed);
            }

            SetState(CombatState.K_IDLE);
            LastEvent = "Dash end";
        }

        // ── Block and parry (§5.3, §5.4, C-BLOCK) ───────────────────────────────────

        private bool TryStartBlock()
        {
            if (tuning == null || movement == null) return false;
            if (blockAction == null) return false;
            if (state == CombatState.BK_RAISE || state == CombatState.BK_HOLD) return false;
            if (state == CombatState.BK_STUN || state == CombatState.BK_RELEASE) return false;
            if (state == CombatState.BK_BREAK) return false;
            if (state == CombatState.C_DASH) return false;

            // §5.3: ground states only — no block in the air, on a wall, or in the glide.
            if (!movement.IsGrounded || movement.IsClinging) return false;

            // Recovery frames only; not the startup or the active window of an attack.
            if (IsAttackState() && !IsInRecovery()) return false;

            EnterGuard();
            return true;
        }

        private void EnterGuard()
        {
            if (IsAttackState())
            {
                ResetAttackAnimator();
                currentAttack = AttackId.None;
                chainWindowOpen = false;
                slashOpen = false;
            }

            parriedThisHold = false;
            parryWindowOpen = false;
            guardRegenAccumulator = 0f;

            SetState(CombatState.BK_RAISE);
            LastEvent = "BK-RAISE";
        }

        private void StepGuardFrame()
        {
            switch (state)
            {
                case CombatState.BK_RAISE:
                    if (!blockHeld)
                    {
                        // §5.4 whiff tap: released during the raise, no parry was possible.
                        parryLockout = tuning.Parry_TapLockoutSeconds;
                        SetState(CombatState.BK_RELEASE);
                        return;
                    }

                    if (frameInState >= tuning.Block_RaiseFrames)
                    {
                        SetState(CombatState.BK_HOLD);
                        LastEvent = "BK-HOLD";
                        return;
                    }

                    frameInState++;
                    return;

                case CombatState.BK_HOLD:
                    // §5.4: the parry window is the first 8 f of BK-HOLD, after the 4 f raise.
                    parryWindowOpen = frameInState <= tuning.Parry_WindowFrames && parryLockout <= 0f;

                    if (!blockHeld)
                    {
                        if (parryWindowOpen && !parriedThisHold)
                        {
                            parryLockout = tuning.Parry_TapLockoutSeconds;
                        }

                        SetState(CombatState.BK_RELEASE);
                        return;
                    }

                    // §5.3: the guard is ground-only; losing the floor drops it.
                    if (movement != null && !movement.IsGrounded)
                    {
                        EndGuard();
                        return;
                    }

                    ApplyGuardWalk();
                    frameInState++;
                    return;

                case CombatState.BK_STUN:
                    parryWindowOpen = false;

                    if (blockstunTimer <= 0f)
                    {
                        // Back to holding without reopening the parry window.
                        SetState(CombatState.BK_HOLD);
                        frameInState = tuning.Parry_WindowFrames + 1;
                        return;
                    }

                    frameInState++;
                    return;

                case CombatState.BK_RELEASE:
                    if (frameInState >= tuning.Block_ReleaseFrames)
                    {
                        SetState(CombatState.K_IDLE);
                        return;
                    }

                    frameInState++;
                    return;

                case CombatState.BK_BREAK:
                    parryWindowOpen = false;

                    if (guardBreakTimer <= 0f)
                    {
                        guardPoints = tuning.Block_GuardPoints;
                        guardRegenAccumulator = 0f;
                        SetState(CombatState.K_IDLE);
                        LastEvent = "Guard restored";
                        return;
                    }

                    frameInState++;
                    return;
            }
        }

        /// <summary>§5.3: walk at 0.35× run speed with the facing locked.</summary>
        private void ApplyGuardWalk()
        {
            if (movement == null) return;

            float walk = movement.MoveInput.x * movement.Tuning.MV_01_RunSpeed * tuning.Block_WalkSpeedMultiplier;
            movement.BeginSpeedOverride(walk, 2f * SpecUnits.FixedDeltaTime, false);
        }

        private void EnterGuardStun(float seconds, float pushTiles, Vector2 from)
        {
            blockstunTimer = Mathf.Max(blockstunTimer, seconds);
            SetState(CombatState.BK_STUN);

            if (movement != null && pushTiles > 0f && seconds > 0f)
            {
                float direction = Mathf.Sign(transform.position.x - from.x);
                if (Mathf.Abs(direction) < 0.01f) direction = movement.IsFacingLeft ? 1f : -1f;
                blockstunPushSpeed = direction * (pushTiles / seconds);
                movement.BeginSpeedOverride(blockstunPushSpeed, seconds, false);
            }
        }

        private void EnterGuardBreak()
        {
            guardPoints = 0;
            guardBreakTimer = tuning.Block_GuardBreakSeconds;
            SetState(CombatState.BK_BREAK);
            parryWindowOpen = false;

            // H19: 6 f global freeze, 3 px / 0.20 s, 0.5 tiles back.
            hitStop?.PlayGlobal(tuning.H19_GlobalFreezeFrames, 0f, 1f);
            cameraShake?.Shake(tuning.H19_ShakeAmplitudePixels, tuning.H19_ShakeSeconds);

            if (movement != null)
            {
                float direction = movement.IsFacingLeft ? 1f : -1f;
                movement.BeginSpeedOverride(
                    direction * (tuning.H19_PlayerKnockbackTiles / tuning.Block_GuardBreakSeconds),
                    tuning.Block_GuardBreakSeconds,
                    false);
            }

            LastEvent = "BK-BREAK";
        }

        private void EndGuard()
        {
            parryWindowOpen = false;
            blockstunTimer = 0f;
            SetState(CombatState.K_IDLE);
            movement?.EndSpeedOverride(0f);
        }

        // ── Damage guard (§10) ─────────────────────────────────────────────────────

        /// <summary>
        /// §10: what the dash, the block and the parry do to an incoming hit. Hazards, pits,
        /// lethal volumes and the kill plane never reach here (decision a; S2-doc §8 rule 2).
        /// </summary>
        public bool TryGuard(ref DamageInfo info, out int chipPips)
        {
            chipPips = 0;
            chipInProgress = false;

            if (info.Class == DamageClass.D_Hazard ||
                info.Class == DamageClass.D_Pit ||
                info.Class == DamageClass.D_Lethal)
            {
                return false;
            }

            // DB-1: 8 f of enemy-only i-frames, frames 3–10 of the dash.
            if (DashIFramesActive)
            {
                DashIFrameNegations++;
                LastEvent = "Dash i-frames";
                return true;
            }

            if (!IsGuarding) return false;
            if (!IsFront(info.SourcePosition)) return false;

            if (parryCoverTimer > 0f)
            {
                // Already parried this hitbox: covered for the rest of its active frames.
                return true;
            }

            if (parryWindowOpen)
            {
                ResolveParry(info);
                return true;
            }

            ResolveBlock(info, out chipPips);
            return true;
        }

        private bool IsFront(Vector2 sourcePosition)
        {
            if (movement == null) return true;

            float facing = movement.IsFacingLeft ? -1f : 1f;
            float dx = sourcePosition.x - transform.position.x;
            return dx * facing >= 0f;
        }

        /// <summary>§5.3: a front block costs guard points, a D-Heavy chips 1 pip.</summary>
        private void ResolveBlock(DamageInfo info, out int chipPips)
        {
            chipPips = 0;

            bool heavy = info.Class == DamageClass.D_Heavy;
            int cost = heavy ? tuning.Block_HeavyGuardCost : tuning.Block_LightGuardCost;

            guardPoints = Mathf.Max(0, guardPoints - cost);
            guardRegenDelay = tuning.Block_GuardRegenDelaySeconds;
            guardRegenAccumulator = 0f;

            if (heavy) chipPips = tuning.Block_HeavyChipPips;

            // Chip is damage, so HeroHealth raises Damaged for it; the guard must survive that.
            if (chipPips > 0) chipInProgress = true;

            EnterGuardStun(
                heavy ? tuning.Block_HeavyBlockstunSeconds : tuning.Block_LightBlockstunSeconds,
                heavy ? tuning.Block_HeavyPushTiles : tuning.Block_LightPushTiles,
                info.SourcePosition);

            BlocksLanded++;
            LastEvent = heavy ? "Block D-Heavy (chip " + chipPips + ")" : "Block D-Light";

            ApplyHitFeel(heavy ? HitKind.H18_BlockHeavy : HitKind.H16_BlockLight, null, false);

            if (guardPoints <= 0) EnterGuardBreak();
        }

        /// <summary>§5.4: a hit inside the window is negated, costs 0 guard points and pays no refund.</summary>
        private void ResolveParry(DamageInfo info)
        {
            bool heavy = info.Class == DamageClass.D_Heavy;

            ParriesLanded++;
            parriedThisHold = true;
            parryCoverTimer = CombatTuning.Frames(tuning.Parry_CoverFrames);

            LastEvent = heavy ? "PARRY D-Heavy" : "PARRY D-Light";

            // The attacker takes poise damage (25 light / 50 heavy); fodder and soldiers break.
            IAttackTarget2D attacker = info.Source != null
                ? info.Source.GetComponentInParent<IAttackTarget2D>()
                : null;

            if (attacker != null)
            {
                // §8 H17: the parried attacker is pushed back away from the hero.
                Vector2 away = attacker.Position - (Vector2)transform.position;
                away = away.sqrMagnitude > 0.0001f ? away.normalized : Vector2.right;

                attacker.ReceiveHit(new CombatHit
                {
                    Attack = AttackId.None,
                    Kind = HitKind.H17_Parry,
                    Damage = 0f,
                    PoiseDamage = heavy ? tuning.Parry_HeavyPoiseDamage : tuning.Parry_LightPoiseDamage,
                    Origin = transform.position,
                    Knockback = away * tuning.H17_EnemyKnockbackTiles,
                });
            }

            ApplyHitFeel(HitKind.H17_Parry, attacker is Component component ? component.gameObject : null, false);
        }

        // ── State helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Clears the attack trigger and the chain index together. Leaving the int set would let a
        /// finished chain re-select G2/G3 on the next, unrelated swing.
        /// </summary>
        private void ResetAttackAnimator()
        {
            if (animator == null) return;

            animator.ResetTrigger(AttackParam);
            animator.SetInteger(AttackChainParam, 0);
        }

        private void SetState(CombatState next)
        {
            state = next;
            frameInState = 1;
        }

        private bool IsAttackState()
        {
            switch (state)
            {
                case CombatState.G1:
                case CombatState.G2:
                case CombatState.G3:
                case CombatState.AIR1:
                case CombatState.WALL1:
                    return true;
                default:
                    return false;
            }
        }

        private bool IsGuardState()
        {
            switch (state)
            {
                case CombatState.BK_RAISE:
                case CombatState.BK_HOLD:
                case CombatState.BK_STUN:
                case CombatState.BK_RELEASE:
                case CombatState.BK_BREAK:
                    return true;
                default:
                    return false;
            }
        }

        private bool IsInActiveWindow()
        {
            if (!IsAttackState()) return false;
            return frameInState > attackStartup && frameInState <= attackStartup + attackActive;
        }

        private bool IsInRecovery()
        {
            if (!IsAttackState()) return false;
            return frameInState > attackStartup + attackActive;
        }

        private static bool IsGroundChain(AttackId attack)
        {
            return attack == AttackId.G1 || attack == AttackId.G2 || attack == AttackId.G3;
        }

        private static bool IsAirborneAttack(AttackId attack)
        {
            return attack == AttackId.AIR1;
        }

        private static CombatState ToCombatState(AttackId attack)
        {
            switch (attack)
            {
                case AttackId.G1: return CombatState.G1;
                case AttackId.G2: return CombatState.G2;
                case AttackId.G3: return CombatState.G3;
                case AttackId.AIR1: return CombatState.AIR1;
                case AttackId.WALL1: return CombatState.WALL1;
                default: return CombatState.K_IDLE;
            }
        }

        // ── SYS1 events ─────────────────────────────────────────────────────────────

        private void OnLanded(float impactSpeed)
        {
            // The bounce chain is "consecutive airborne hits without landing or clinging".
            airborneHits = 0;
        }

        private void OnMovementStateChanged(MovementState next)
        {
            if (next == MovementState.W_CLING) airborneHits = 0;
        }

        private void OnHeroDamaged(DamageInfo info)
        {
            // A blocked D-Heavy applies chip, which is damage: it must not drop the guard, which
            // is already in BK-STUN by the time this fires (§5.3).
            if (chipInProgress)
            {
                chipInProgress = false;
                return;
            }

            // §3.1 / §11: a hit taken ends the chain and clears the attack and guard layers.
            comboMemory = 0f;
            chainIndex = 0;
            airborneHits = 0;
            AbortToIdle();
        }

        private void AbortToIdle()
        {
            ResetAttackAnimator();

            if (state == CombatState.C_DASH && movement != null)
            {
                movement.EndSpeedOverride(movement.Velocity.x);
            }
            else if (IsGuardState() && movement != null)
            {
                movement.EndSpeedOverride(0f);
            }

            currentAttack = AttackId.None;
            chainWindowOpen = false;
            slashOpen = false;
            parryWindowOpen = false;
            blockstunTimer = 0f;
            guardBreakTimer = 0f;
            SetState(CombatState.K_IDLE);
        }

        /// <summary>Test hook for SYS6's weapon tiers.</summary>
        public void SetWeaponTier(int tier)
        {
            weaponTier = Mathf.Clamp(tier, 1, 5);
        }

        private void OnDrawGizmosSelected()
        {
            if (slash == null) return;

            Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.5f);
            Gizmos.DrawWireSphere(slash.WorldCenter, 0.12f);
        }
    }
}
