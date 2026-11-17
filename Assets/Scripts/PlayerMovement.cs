using System;
using UnityEngine;
using UnityEngine.InputSystem;
using BastionZero.Core;

namespace BastionZero.Movement
{
    /// <summary>
    /// SYS1 player movement for the vertical slice: a collision-aware kinematic 2D motor with
    /// the ten slice states of 13-vertical-slice §5.1 (G_IDLE, G_RUN, A_RISE_HELD, A_RISE_FREE,
    /// A_APEX, A_FALL, W_CLING, W_JUMP, H_KNOCK, H_WARP).
    ///
    /// Every number comes from the SYS1 tuning asset and carries its MV-xx ID (05-movement §3.1);
    /// nothing here is a magic number. Displacement is resolved by collider casts and committed
    /// with <see cref="Rigidbody2D.MovePosition"/>, so the physics solver never moves the hero and
    /// one-way platforms are filtered by cast direction instead of by an effector.
    ///
    /// Attacks are an overlay (SYS3) and never block movement; <see cref="GrantBounceAirJump"/>
    /// is the single hook SYS3 needs from this system today, and <see cref="ApplyKnockback"/> /
    /// <see cref="WarpTo"/> are the two SYS2 writes into the H-KNOCK and H-WARP states.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public class PlayerMovement : MonoBehaviour
    {
        private const string MoveActionName = "Player/Move";
        private const string JumpActionName = "Player/Jump";

        private static readonly int SpeedParam = Animator.StringToHash("Speed");
        private static readonly int GroundedParam = Animator.StringToHash("Grounded");
        private static readonly int VerticalSpeedParam = Animator.StringToHash("VerticalSpeed");
        private static readonly int JumpStartedParam = Animator.StringToHash("JumpStarted");
        private static readonly int MovementStateParam = Animator.StringToHash("MovementState");
        private static readonly int WallClingParam = Animator.StringToHash("WallCling");

        [Header("References")]
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Tooltip("SYS1 tuning asset. Every MV-xx value is read from here.")]
        [SerializeField] private PlayerMovementTuning tuning;

        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private InputAction moveAction;
        private InputAction jumpAction;

        private readonly RaycastHit2D[] castHits = new RaycastHit2D[8];

        [SerializeField] private MovementState state = MovementState.G_IDLE;
        private MovementState previousState = MovementState.G_IDLE;

        private Vector2 velocity;
        private Vector2 moveInput;

        private float coyoteTimer;
        private float headBumpTimer;
        private float reClingTimer;
        private float knockTimer;
        private float knockDuration;
        private float knockHoldFraction = 0.5f;
        private float knockInitialHorizontal;
        private float airStallTimer;
        private float clingReleaseTimer;
        private int heldRiseTicks;
        private int holdWindowTicks;
        private int minHoldTicks;

        private int airJumpsRemaining;
        private int suppressGroundTicks;
        private int wallSide;
        private int clingSide;

        private bool isGrounded;
        private bool isClinging;
        private bool isHeldRise;
        private bool riseFromWallJump;
        private bool jumpHeld;
        private bool jumpPressPending;
        private bool warpActive;
        private bool facingLeft;
        private bool inputReady;
        private bool manualStep;
        private float pendingImpactSpeed;

        // SYS3 C-DASH and the guard walk: an external speed owns horizontal motion while this
        // runs. S1 has no dash or block state (S3-doc X9, X24), so this is the one hook they need.
        private float speedOverrideTimer;
        private float speedOverrideSpeed;
        private bool speedOverrideSuppressesCoyote;

        private double jumpPressedAt;

        /// <summary>Raised in the physics step that accepts a jump, wall jump or air jump.</summary>
        public event Action Jumped;

        /// <summary>Raised when the motor touches the floor after being airborne. Arg is the impact speed.</summary>
        public event Action<float> Landed;

        /// <summary>Raised whenever the movement state node changes.</summary>
        public event Action<MovementState> StateChanged;

        /// <summary>Current movement state node (spec names, see <see cref="MovementState"/>).</summary>
        public MovementState State => state;

        /// <summary>State the machine was in before the current one.</summary>
        public MovementState PreviousState => previousState;

        /// <summary>True while the ground cast reports a floor and the hero is not rising.</summary>
        public bool IsGrounded => isGrounded;

        /// <summary>True while holding a cling-able wall (W_CLING).</summary>
        public bool IsClinging => isClinging;

        /// <summary>True when the visual root is mirrored to face left.</summary>
        public bool IsFacingLeft => facingLeft;

        /// <summary>True while a knockback window owns the hero (H_KNOCK).</summary>
        public bool IsKnockedBack => knockTimer > 0f;

        /// <summary>Motor velocity in units per second. Consumers read this, never the body.</summary>
        public Vector2 Velocity => velocity;

        /// <summary>The tuning asset this motor is reading.</summary>
        public PlayerMovementTuning Tuning => tuning;

        /// <summary>Remaining coyote time in seconds (MV-19). Debug readout only.</summary>
        public float CoyoteTimer => coyoteTimer;

        /// <summary>Remaining re-cling lockout in seconds (MV-42). Debug readout only.</summary>
        public float ReClingTimer => Mathf.Max(0f, reClingTimer);

        /// <summary>Remaining head-bump coyote suppression in seconds (MV-21). Debug readout only.</summary>
        public float HeadBumpTimer => Mathf.Max(0f, headBumpTimer);

        /// <summary>Stored air-jumps still available (MV-22). Debug readout only.</summary>
        public int AirJumpsRemaining => airJumpsRemaining;

        /// <summary>True while a buffered press is inside its MV-20 window. Debug readout only.</summary>
        public bool HasBufferedJump => jumpPressPending && BufferedJumpAge() <= tuning.MV_20_JumpBuffer;

        /// <summary>Raw horizontal input, −1…1. SYS3 reads it for the guard walk and the dash exit.</summary>
        public Vector2 MoveInput => moveInput;

        /// <summary>True while a SYS3 speed override (dash, guard walk) owns horizontal motion.</summary>
        public bool IsSpeedOverridden => speedOverrideTimer > 0f;

        /// <summary>Wall side the hero is clinging to: +1 right, −1 left, 0 when not clinging.</summary>
        public int ClingSide => clingSide;

        /// <summary>
        /// SYS3 C-DASH and the guard walk: replaces horizontal motion with a constant speed for a
        /// duration, bypassing input steering, acceleration and turn-around. With
        /// <paramref name="suppressCoyoteAfter"/> set, the floor being lost during the window
        /// grants no coyote jump (S3-doc DB-3, X28), so a ledge dash cannot become a jump.
        /// </summary>
        public void BeginSpeedOverride(float horizontalSpeed, float durationSeconds, bool suppressCoyoteAfter = false)
        {
            speedOverrideSpeed = horizontalSpeed;
            speedOverrideTimer = Mathf.Max(0.0001f, durationSeconds);
            speedOverrideSuppressesCoyote = suppressCoyoteAfter;
        }

        /// <summary>
        /// Ends a speed override and sets the exit momentum in one step (§5.2: run speed if a
        /// direction is held, otherwise stop).
        /// </summary>
        public void EndSpeedOverride(float exitHorizontalSpeed)
        {
            speedOverrideTimer = 0f;
            speedOverrideSpeed = 0f;
            speedOverrideSuppressesCoyote = false;
            velocity.x = exitHorizontalSpeed;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();

            // The motor owns displacement: no solver gravity, and full kinematic
            // contacts so trigger volumes only need to see a body, never push one.
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.useFullKinematicContacts = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            if (bodyCollider == null)
            {
                Debug.LogError($"[PlayerMovement] '{name}' needs a Collider2D for cast-based collision. Movement disabled.", this);
                enabled = false;
                return;
            }

            if (tuning == null)
            {
                Debug.LogError($"[PlayerMovement] '{name}' has no SYS1 tuning asset assigned. Movement disabled.", this);
                enabled = false;
                return;
            }

            if (playerInput == null) playerInput = GetComponent<PlayerInput>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            airJumpsRemaining = 0;

            // The spec states the hold window in seconds; the machine runs whole 60 Hz ticks, so
            // both windows are rounded to the nearest tick (0.18 s = 10.8 ticks -> 11). Truncating
            // instead cost ~0.3 tiles of MV-10 jump height, which would eat most of the 0.5-tile
            // vertical safety margin the level metrics rely on.
            holdWindowTicks = Mathf.Max(1, Mathf.RoundToInt(tuning.MV_08_MaxHoldTime * SpecUnits.FramesPerSecond));
            minHoldTicks = Mathf.Max(1, Mathf.RoundToInt(tuning.MV_13_MinHoldTime * SpecUnits.FramesPerSecond));

            if (playerInput == null || playerInput.actions == null)
            {
                Debug.LogError($"[PlayerMovement] '{name}' has no PlayerInput with an assigned actions asset. Movement disabled.", this);
                enabled = false;
                return;
            }

            moveAction = playerInput.actions.FindAction(MoveActionName);
            jumpAction = playerInput.actions.FindAction(JumpActionName);

            if (moveAction == null || jumpAction == null)
            {
                Debug.LogError(
                    $"[PlayerMovement] Could not find '{MoveActionName}' and/or '{JumpActionName}' in " +
                    $"'{playerInput.actions.name}'. Movement disabled.", this);
                enabled = false;
                return;
            }

            inputReady = true;
        }

        private void OnEnable()
        {
            // PlayerInput owns the action-map lifecycle (defaultActionMap = "Player"),
            // so this component only subscribes to the actions it consumes.
            if (!inputReady || jumpAction == null) return;

            jumpAction.started += OnJumpStarted;
            jumpAction.canceled += OnJumpCanceled;
        }

        private void OnDisable()
        {
            if (jumpAction != null)
            {
                jumpAction.started -= OnJumpStarted;
                jumpAction.canceled -= OnJumpCanceled;
            }

            jumpHeld = false;
            jumpPressPending = false;
            isHeldRise = false;
        }

        private void Update()
        {
            if (!inputReady) return;

            // Sample the stick/keys once per frame; the physics step consumes the cached value.
            moveInput = moveAction.ReadValue<Vector2>();

            if (spriteRenderer != null) spriteRenderer.flipX = facingLeft;
        }

        private void FixedUpdate()
        {
            if (!inputReady || manualStep) return;

            Simulate(Time.fixedDeltaTime);
        }

        // ── Public surface used by SYS2 / SYS3 ────────────────────────────────────────────

        /// <summary>
        /// MV-22 / MV-25 / MV-27: a qualifying airborne hit grants the single stored air-jump
        /// and stalls the fall for MV-27. Called by SYS3's attack overlay on a bounce hit.
        /// </summary>
        public void GrantBounceAirJump()
        {
            if (tuning == null) return;

            airJumpsRemaining = Mathf.Max(airJumpsRemaining, tuning.MV_22_StoredAirJumpsMax);
            airStallTimer = tuning.MV_27_AirStall;

            if (velocity.y < 0f) velocity.y = 0f;
        }

        /// <summary>Enters H-KNOCK for the given duration. Called by SYS2 when damage lands.</summary>
        /// <param name="holdFraction">CD-27: fraction of the window the horizontal speed is held
        /// before it decays linearly to zero.</param>
        public void ApplyKnockback(Vector2 impulse, float duration, float holdFraction = 0.5f)
        {
            velocity = impulse;
            knockTimer = Mathf.Max(0f, duration);
            knockDuration = knockTimer;
            knockHoldFraction = Mathf.Clamp01(holdFraction);
            knockInitialHorizontal = impulse.x;
            isHeldRise = false;
            riseFromWallJump = false;
            isClinging = false;
            clingSide = 0;
            jumpPressPending = false;
        }

        /// <summary>
        /// Enters H-WARP and teleports. Called by SYS2 for pits and warp hazards; the fade and
        /// the safe-ground target are SYS2's, the state itself is SYS1's.
        /// </summary>
        public void WarpTo(Vector2 worldPosition)
        {
            velocity = Vector2.zero;
            warpActive = true;
            isClinging = false;
            clingSide = 0;
            isHeldRise = false;
            riseFromWallJump = false;
            jumpPressPending = false;

            if (body == null) body = GetComponent<Rigidbody2D>();
            body.position = worldPosition;
            transform.position = worldPosition;
        }

        /// <summary>Leaves H-WARP once the warp hazard's own sequence has finished.</summary>
        public void EndWarp()
        {
            warpActive = false;
        }

        /// <summary>Manual stepping for the debug frame stepper: stop the automatic FixedUpdate.</summary>
        public void SetManualStep(bool enabledManualStep)
        {
            manualStep = enabledManualStep;
        }

        /// <summary>Runs exactly one physics step, even while the motor is paused for stepping.</summary>
        public void StepOnce()
        {
            if (!inputReady || tuning == null) return;

            Simulate(Time.fixedDeltaTime);
        }

        /// <summary>Teleports without entering H-WARP (respawn placement).</summary>
        public void PlaceAt(Vector2 worldPosition)
        {
            velocity = Vector2.zero;
            isHeldRise = false;
            riseFromWallJump = false;
            isClinging = false;
            clingSide = 0;

            if (body == null) body = GetComponent<Rigidbody2D>();
            body.position = worldPosition;
            transform.position = worldPosition;
        }

        // ── One physics step ─────────────────────────────────────────────────────────────

        private void Simulate(float dt)
        {
            bool wasGrounded = isGrounded;

            TickTimers(dt);
            UpdateGroundedState();

            if (!wasGrounded && isGrounded)
            {
                // The blocking sweep already zeroed the vertical speed one tick before the
                // floor probe sees it, so the pre-impact speed is carried over from there.
                float impactSpeed = pendingImpactSpeed;
                pendingImpactSpeed = 0f;

                // The stored air-jump is only ever granted by a bounce (MV-22, glossary
                // "Stored air-jump"); landing spends it, it does not hand a free double jump
                // to every ground jump. Clearing it here keeps MV-D1's 3.5-tile ledge reachable
                // and the 5.0-tile ledge a real gate.
                airJumpsRemaining = 0;
                isHeldRise = false;
                riseFromWallJump = false;

                Landed?.Invoke(impactSpeed);
            }

            if (isGrounded) pendingImpactSpeed = 0f;

            UpdateWallContact();
            UpdateClingState(dt);

            if (!warpActive) ApplyJumpInput();

            ApplyVerticalMotion(dt);
            ApplyHorizontalMotion(dt);
            ClampFallSpeed();
            MoveWithCollisions(dt);

            ResolveState();
            PublishAnimator();
        }

        private void TickTimers(float dt)
        {
            coyoteTimer = isGrounded ? tuning.MV_19_CoyoteTime : Mathf.Max(0f, coyoteTimer - dt);

            // MV-21: after a head bump coyote is suppressed, so a ceiling clip cannot be
            // converted into a free jump on the way down.
            if (headBumpTimer > 0f)
            {
                headBumpTimer -= dt;
                coyoteTimer = 0f;
            }

            if (speedOverrideTimer > 0f)
            {
                speedOverrideTimer = Mathf.Max(0f, speedOverrideTimer - dt);

                // DB-3 / X28: a dash that runs off a ledge leaves no coyote window behind it.
                if (speedOverrideSuppressesCoyote && !isGrounded) coyoteTimer = 0f;
                if (speedOverrideTimer <= 0f) speedOverrideSuppressesCoyote = false;
            }

            if (reClingTimer > 0f) reClingTimer -= dt;
            if (knockTimer > 0f) knockTimer -= dt;
            if (airStallTimer > 0f) airStallTimer -= dt;

            // MV-20 expiry; consumption happens in ApplyJumpInput on an accepted press.
            if (jumpPressPending && BufferedJumpAge() > tuning.MV_20_JumpBuffer) jumpPressPending = false;
        }

        // ── Input ────────────────────────────────────────────────────────────────────────

        private void OnJumpStarted(InputAction.CallbackContext context)
        {
            // Store the press time, not a countdown: the press is stamped on the input
            // thread's clock and stays valid for MV-20 seconds of real time no matter how
            // many render frames pass before the next physics step reads it.
            jumpPressedAt = context.time > 0d ? context.time : Now();
            jumpPressPending = true;
            jumpHeld = true;
        }

        private void OnJumpCanceled(InputAction.CallbackContext context)
        {
            jumpHeld = false;
        }

        private static double Now() => Time.realtimeSinceStartupAsDouble;

        private double BufferedJumpAge()
        {
            double age = Now() - jumpPressedAt;
            return age > 0d ? age : 0d;
        }

        /// <summary>
        /// Resolves a buffered press against the state the hero is in: wall jump out of W_CLING,
        /// ground/coyote jump, or the single stored air jump (MV-22).
        /// </summary>
        private void ApplyJumpInput()
        {
            if (knockTimer > 0f || !jumpPressPending) return;

            // §5.2: a jump pressed during a ground dash waits for the dash to end, and only fires
            // if the hero is still grounded then; the press expires on its own MV-20 window.
            if (speedOverrideTimer > 0f) return;

            if (BufferedJumpAge() > tuning.MV_20_JumpBuffer) return;

            if (isClinging)
            {
                int side = clingSide;
                EndCling();

                velocity.y = tuning.MV_07_HeldRiseSpeed;
                velocity.x = -side * tuning.MV_41_WallJumpPush;   // MV-41 push, then normal air control
                reClingTimer = tuning.MV_42_ReClingLockout;       // MV-42 no instant re-stick

                BeginRise(true);
                jumpPressPending = false;
                return;
            }

            if (isGrounded || coyoteTimer > 0f)
            {
                velocity.y = tuning.MV_07_HeldRiseSpeed;
                isGrounded = false;
                coyoteTimer = 0f;
                suppressGroundTicks = 2;

                BeginRise(false);
                jumpPressPending = false;
                return;
            }

            if (airJumpsRemaining > 0)
            {
                airJumpsRemaining--;
                velocity.y = tuning.MV_07_HeldRiseSpeed;

                BeginRise(false);
                jumpPressPending = false;
            }
        }

        private void BeginRise(bool fromWallJump)
        {
            isHeldRise = true;
            heldRiseTicks = 0;
            riseFromWallJump = fromWallJump;

            // Visual feedback only. Fired here (not on the raw press) because a buffered jump
            // may not be accepted until several physics steps after the button press.
            if (animator != null) animator.SetTrigger(JumpStartedParam);

            Jumped?.Invoke();
        }

        // ── Ground, wall and cling ───────────────────────────────────────────────────────

        /// <summary>
        /// Grounds only on a downward sweep. Rising frames never re-acquire the floor, and two
        /// ticks are suppressed after a jump so the launch surface cannot cancel it.
        /// </summary>
        private void UpdateGroundedState()
        {
            isGrounded = velocity.y <= 0.01f && TryCast(Vector2.down, tuning.groundProbeDistance, out _);

            if (suppressGroundTicks > 0)
            {
                suppressGroundTicks--;
                isGrounded = false;
            }

            if (isGrounded && velocity.y < 0f) velocity.y = 0f;
        }

        /// <summary>Finds a wall on the side the player is pushing toward, for W_CLING entry.</summary>
        private void UpdateWallContact()
        {
            wallSide = 0;

            float horizontal = moveInput.x;
            if (Mathf.Abs(horizontal) <= 0.3f) return;

            Vector2 direction = horizontal > 0f ? Vector2.right : Vector2.left;
            if (!TryWallProbe(direction, out _)) return;

            wallSide = horizontal > 0f ? 1 : -1;
        }

        private void UpdateClingState(float dt)
        {
            bool owned = HeroAbilities.WallClingUnlocked;

            if (isClinging)
            {
                if (!owned || isGrounded)
                {
                    EndCling();
                    return;
                }

                Vector2 direction = clingSide > 0 ? Vector2.right : Vector2.left;
                if (!TryWallProbe(direction, out _))
                {
                    EndCling();
                    return;
                }

                // MV-43: holding away releases the cling only after the delay, which keeps a
                // deliberate drop feeling eager while removing accidental un-sticks.
                bool holdingAway = Mathf.Abs(moveInput.x) > 0.3f && Mathf.Sign(moveInput.x) != clingSide;
                if (holdingAway)
                {
                    clingReleaseTimer -= dt;
                    if (clingReleaseTimer <= 0f) EndCling();
                }
                else
                {
                    clingReleaseTimer = tuning.MV_43_HoldAwayReleaseDelay;
                }

                return;
            }

            // SYS2 §7.3: wall-cling cannot start while knockback owns the hero.
            if (!owned || isGrounded || wallSide == 0 || reClingTimer > 0f || knockTimer > 0f) return;

            BeginCling(wallSide);
        }

        private void BeginCling(int side)
        {
            isClinging = true;
            clingSide = side;
            clingReleaseTimer = tuning.MV_43_HoldAwayReleaseDelay;

            velocity.x = 0f;
            if (velocity.y < 0f) velocity.y = 0f;

            isHeldRise = false;
            riseFromWallJump = false;

            // X11 refreshes the air dash on a cling; the stored air-jump is not refreshed.
            airJumpsRemaining = 0;
        }

        private void EndCling()
        {
            isClinging = false;
            clingSide = 0;
        }

        // ── Motion integration ───────────────────────────────────────────────────────────

        private void ApplyVerticalMotion(float dt)
        {
            if (warpActive)
            {
                velocity = Vector2.zero;
                return;
            }

            if (isClinging)
            {
                // MV-44: no stamina, so a cling holds indefinitely. The hero can climb up at
                // MV-39 or descend at MV-40; with no vertical input the cling simply holds.
                float climb = moveInput.y;
                if (climb > 0.3f) velocity.y = tuning.MV_39_WallClimbSpeed;
                else if (climb < -0.3f) velocity.y = -tuning.MV_40_WallSlideSpeed;
                else velocity.y = 0f;
                return;
            }

            if (isHeldRise)
            {
                heldRiseTicks++;

                if (!jumpHeld && heldRiseTicks >= minHoldTicks)
                {
                    // MV-12 release cut, gated by the MV-13 minimum hold.
                    isHeldRise = false;
                    if (velocity.y > 0f) velocity.y *= tuning.MV_12_ReleaseCut;
                }
                else if (heldRiseTicks > holdWindowTicks)
                {
                    // Hold window spent: keep the current rise speed and hand over to gravity.
                    isHeldRise = false;
                }
                else
                {
                    // MV-07: the held phase is a constant-speed rise, not an accelerated one.
                    velocity.y = tuning.MV_07_HeldRiseSpeed;
                    return;
                }
            }

            float gravity = tuning.MV_09_Gravity;

            // MV-14 / MV-15 apex band, then the MV-16 fall multiplier.
            if (Mathf.Abs(velocity.y) <= tuning.MV_14_ApexBandSpeed) gravity *= tuning.MV_15_ApexGravityMultiplier;
            else if (velocity.y < 0f) gravity *= tuning.MV_16_FallGravityMultiplier;

            velocity.y -= gravity * dt;

            // MV-27: a bounce grant stalls the fall for a moment so the chain stays readable.
            if (airStallTimer > 0f && velocity.y < 0f) velocity.y = 0f;
        }

        private void ApplyHorizontalMotion(float dt)
        {
            if (warpActive)
            {
                velocity.x = 0f;
                return;
            }

            if (speedOverrideTimer > 0f)
            {
                // The override owns horizontal motion outright: no steering, no turn-around,
                // and (for the guard) no facing change.
                velocity.x = speedOverrideSpeed;
                return;
            }

            if (isClinging)
            {
                velocity.x = 0f;
                return;
            }

            if (knockTimer > 0f)
            {
                // H-KNOCK: no steering. SYS2 §7.3 / CD-27 — the horizontal impulse is held for
                // the first half of the window, then decays linearly to zero over the rest, so
                // the hero settles where he lands instead of sliding.
                float heldTime = knockDuration * knockHoldFraction;
                if (knockTimer > heldTime) return;

                float decayTime = Mathf.Max(0.0001f, knockDuration - heldTime);
                float speed = Mathf.Abs(knockInitialHorizontal);
                if (speed > 0.0001f)
                    velocity.x = Mathf.MoveTowards(velocity.x, 0f, speed / decayTime * dt);
                return;
            }

            float input = Mathf.Clamp(moveInput.x, -1f, 1f);
            bool hasInput = Mathf.Abs(input) > 0.01f;
            float target = input * tuning.MV_01_RunSpeed;

            if (hasInput)
            {
                facingLeft = input < 0f;
                if (spriteRenderer != null) spriteRenderer.flipX = facingLeft;
            }

            if (isGrounded)
            {
                float rate = !hasInput
                    ? tuning.GroundDeceleration                      // MV-03
                    : (velocity.x * target < 0f
                        ? tuning.GroundTurnAcceleration              // MV-04
                        : tuning.GroundAcceleration);                // MV-02

                velocity.x = Mathf.MoveTowards(velocity.x, target, rate * dt);
                return;
            }

            // Airborne with no input keeps its momentum: MV-05 and MV-06 tune steering, not drag.
            if (!hasInput) return;

            float airRate = velocity.x * target < 0f
                ? tuning.GroundTurnAcceleration * tuning.MV_06_AirTurnMultiplier
                : tuning.GroundAcceleration * tuning.MV_05_AirAccelMultiplier;

            velocity.x = Mathf.MoveTowards(velocity.x, target, airRate * dt);
        }

        private void ClampFallSpeed()
        {
            // MV-18 fast-fall stays off in the slice (13 §5.7), so MV-17 is the only cap.
            float cap = tuning.MV_18_FastFallEnabled ? tuning.MV_18_FastFallCap : tuning.MV_17_FallCap;
            if (velocity.y < -cap) velocity.y = -cap;
        }

        /// <summary>
        /// Resolves this step's displacement axis by axis against the nearest valid cast
        /// hit, then commits it through <see cref="Rigidbody2D.MovePosition"/>.
        /// </summary>
        private void MoveWithCollisions(float dt)
        {
            Vector2 delta = velocity * dt;

            if (Mathf.Abs(delta.x) > 0.000001f)
            {
                Vector2 direction = delta.x > 0f ? Vector2.right : Vector2.left;
                if (TryCast(direction, Mathf.Abs(delta.x), out RaycastHit2D hit))
                {
                    float allowed = Mathf.Max(0f, hit.distance - tuning.collisionSkin);
                    delta.x = direction.x * allowed;
                    velocity.x = 0f;
                }
            }

            if (Mathf.Abs(delta.y) > 0.000001f)
            {
                bool rising = delta.y > 0f;
                Vector2 direction = rising ? Vector2.up : Vector2.down;

                if (TryCast(direction, Mathf.Abs(delta.y), out RaycastHit2D hit))
                {
                    if (rising && TryCornerCorrect(ref delta, hit))
                    {
                        // A cleared ceiling corner is not a head bump.
                    }
                    else
                    {
                        if (rising)
                        {
                            // MV-21: a real head bump kills the rise and suppresses coyote.
                            headBumpTimer = tuning.MV_21_HeadBumpSuppression;
                            coyoteTimer = 0f;
                            isHeldRise = false;
                            riseFromWallJump = false;
                        }
                        else
                        {
                            // Remember how fast the fall was stopped: the landing event fires on
                            // the following tick, once the floor probe has re-acquired the ground.
                            pendingImpactSpeed = Mathf.Max(pendingImpactSpeed, -velocity.y);
                        }

                        float allowed = Mathf.Max(0f, hit.distance - tuning.collisionSkin);
                        delta.y = direction.y * allowed;
                        velocity.y = 0f;
                    }
                }
            }

            if (delta.sqrMagnitude > 0f) body.MovePosition(body.position + delta);
        }

        /// <summary>
        /// Slides the hero around a ceiling corner instead of killing the jump: when the head
        /// clips the edge of a platform and a small sideways nudge clears it, take the nudge.
        /// </summary>
        private bool TryCornerCorrect(ref Vector2 delta, RaycastHit2D ceiling)
        {
            if (tuning.cornerCorrection <= 0f) return false;
            if (ceiling.distance > tuning.cornerCorrectionMaxRise) return false;

            float push = Mathf.Abs(moveInput.x) > 0.01f ? Mathf.Sign(moveInput.x) : 0f;
            float full = tuning.cornerCorrection;
            float half = tuning.cornerCorrection * 0.5f;

            float[] offsets = push > 0f
                ? new[] { half, full, -half, -full }
                : push < 0f
                    ? new[] { -half, -full, half, full }
                    : new[] { half, -half, full, -full };

            Vector2 size = GetCastSize();
            if (size == Vector2.zero) return false;

            Vector2 center = bodyCollider.bounds.center;

            for (int i = 0; i < offsets.Length; i++)
            {
                float offset = offsets[i];
                Vector2 testCenter = center + new Vector2(offset, delta.y + tuning.collisionSkin);
                if (IsClear(testCenter, size))
                {
                    delta.x += offset;
                    return true;
                }
            }

            return false;
        }

        private bool IsClear(Vector2 center, Vector2 size)
        {
            Collider2D[] overlaps = bodyCollider is CapsuleCollider2D capsule
                ? Physics2D.OverlapCapsuleAll(center, size, capsule.direction, 0f, tuning.solidLayers)
                : Physics2D.OverlapBoxAll(center, size, 0f, tuning.solidLayers);

            for (int i = 0; i < overlaps.Length; i++)
            {
                Collider2D other = overlaps[i];
                if (!IsBlocking(other)) continue;
                if (IsOneWayPlatform(other)) continue;

                return false;
            }

            return true;
        }

        private Vector2 GetCastSize()
        {
            switch (bodyCollider)
            {
                case CapsuleCollider2D capsule:
                {
                    Vector3 scale = capsule.transform.lossyScale;
                    return new Vector2(
                        Mathf.Abs(capsule.size.x * scale.x),
                        Mathf.Abs(capsule.size.y * scale.y));
                }
                case BoxCollider2D box:
                {
                    Vector3 scale = box.transform.lossyScale;
                    return new Vector2(
                        Mathf.Abs(box.size.x * scale.x),
                        Mathf.Abs(box.size.y * scale.y));
                }
                default:
                    return Vector2.zero;
            }
        }

        /// <summary>Sweeps this object's own collider and returns the nearest hit that should block it.</summary>
        private bool TryCast(Vector2 direction, float distance, out RaycastHit2D best)
        {
            best = default;
            if (distance <= 0f) return false;

            // Cast() inherits the collider's shape, offset and scale, and never reports
            // the collider it belongs to.
            int count = bodyCollider.Cast(direction, castHits, distance + tuning.collisionSkin);

            bool found = false;
            for (int i = 0; i < count; i++)
            {
                RaycastHit2D hit = castHits[i];
                if (!IsValidHit(hit, direction)) continue;
                if (found && hit.distance >= best.distance) continue;

                best = hit;
                found = true;
            }

            return found;
        }

        /// <summary>Short sideways sweep used only to decide whether a wall is cling-able.</summary>
        private bool TryWallProbe(Vector2 direction, out RaycastHit2D best)
        {
            best = default;
            if (bodyCollider == null) return false;

            int count = bodyCollider.Cast(direction, castHits, tuning.wallProbeDistance + tuning.collisionSkin);

            bool found = false;
            for (int i = 0; i < count; i++)
            {
                RaycastHit2D hit = castHits[i];
                if (!IsBlocking(hit.collider)) continue;
                if (IsOneWayPlatform(hit.collider)) continue;
                if (hit.distance > 0f && Mathf.Abs(hit.normal.x) < 0.3f) continue;
                if (found && hit.distance >= best.distance) continue;

                best = hit;
                found = true;
            }

            return found;
        }

        private bool IsValidHit(RaycastHit2D hit, Vector2 direction)
        {
            Collider2D other = hit.collider;
            if (!IsBlocking(other)) return false;

            if (hit.distance <= 0f)
            {
                // Overlapping at the start of the sweep. Only a downward sweep may read that
                // as floor; elsewhere it would let resting ground contact block walking.
                return direction.y < 0f;
            }

            // Reject grazes that belong to the other axis (floor while walking, walls while falling).
            Vector2 normal = hit.normal;
            bool horizontalSweep = Mathf.Abs(direction.x) >= Mathf.Abs(direction.y);

            if (horizontalSweep && Mathf.Abs(normal.x) < 0.3f) return false;
            if (!horizontalSweep && Mathf.Abs(normal.y) < 0.3f) return false;

            if (IsOneWayPlatform(other))
            {
                if (direction.y > -0.01f) return false;                                  // sides and underside are pass-through
                if (bodyCollider.bounds.min.y < other.bounds.max.y - 0.05f) return false; // already below the deck
            }

            return true;
        }

        private bool IsBlocking(Collider2D other)
        {
            if (other == null || other.isTrigger) return false;
            if (other == bodyCollider) return false;
            if (other.attachedRigidbody != null && other.attachedRigidbody == body) return false;

            return (tuning.solidLayers.value & (1 << other.gameObject.layer)) != 0;
        }

        private static bool IsOneWayPlatform(Collider2D other)
        {
            if (other == null || !other.usedByEffector) return false;

            PlatformEffector2D effector = other.GetComponent<PlatformEffector2D>();
            return effector != null && effector.useOneWay;
        }

        // ── State resolution and presentation ────────────────────────────────────────────

        private void ResolveState()
        {
            MovementState next;

            if (warpActive) next = MovementState.H_WARP;
            else if (knockTimer > 0f) next = MovementState.H_KNOCK;
            else if (isClinging) next = MovementState.W_CLING;
            else if (isGrounded) next = Mathf.Abs(velocity.x) > 0.05f ? MovementState.G_RUN : MovementState.G_IDLE;
            else if (Mathf.Abs(velocity.y) <= tuning.MV_14_ApexBandSpeed) next = MovementState.A_APEX;
            else if (velocity.y > 0f)
            {
                next = riseFromWallJump
                    ? MovementState.W_JUMP
                    : (isHeldRise ? MovementState.A_RISE_HELD : MovementState.A_RISE_FREE);
            }
            else next = MovementState.A_FALL;

            if (next == state) return;

            previousState = state;
            state = next;
            StateChanged?.Invoke(state);
        }

        private void PublishAnimator()
        {
            if (animator == null) return;

            animator.SetFloat(SpeedParam, Mathf.Abs(velocity.x));
            animator.SetBool(GroundedParam, isGrounded);
            animator.SetFloat(VerticalSpeedParam, velocity.y);
            animator.SetInteger(MovementStateParam, (int)state);
            animator.SetBool(WallClingParam, isClinging);
        }

        private void OnDrawGizmosSelected()
        {
            Collider2D col = bodyCollider != null ? bodyCollider : GetComponent<Collider2D>();
            if (col == null || tuning == null) return;

            Bounds bounds = col.bounds;

            // Collision box (the movement box, not the hurtbox).
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(bounds.center, bounds.size);

            // Ground probe.
            Gizmos.color = isGrounded ? Color.green : Color.cyan;
            Gizmos.DrawWireCube(
                new Vector3(bounds.center.x, bounds.min.y - tuning.groundProbeDistance * 0.5f, 0f),
                new Vector3(bounds.size.x, tuning.groundProbeDistance, 0.1f));

            // Wall probes.
            float probe = tuning.wallProbeDistance + tuning.collisionSkin;
            Gizmos.color = isClinging ? Color.magenta : Color.white;
            Gizmos.DrawLine(new Vector3(bounds.max.x, bounds.center.y, 0f), new Vector3(bounds.max.x + probe, bounds.center.y, 0f));
            Gizmos.DrawLine(new Vector3(bounds.min.x, bounds.center.y, 0f), new Vector3(bounds.min.x - probe, bounds.center.y, 0f));
        }
    }
}
