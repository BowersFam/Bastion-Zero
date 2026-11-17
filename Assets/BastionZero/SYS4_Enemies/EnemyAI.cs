using BastionZero.Combat;
using BastionZero.Core;
using BastionZero.Damage;
using UnityEngine;

namespace BastionZero.Enemies
{
    /// <summary>SYS4 §3.1, the shared base state list. Names are the spec's own state IDs.</summary>
    public enum EnemyState
    {
        E_DORMANT = 0,
        E_SPAWN = 1,
        E_IDLE = 2,
        E_PATROL = 3,
        E_NOTICE = 4,
        E_ENGAGE = 5,
        E_TELEGRAPH = 6,
        E_ATTACK = 7,
        E_RECOVER = 8,
        E_COOLDOWN = 9,
        E_FLINCH = 10,
        E_STAGGER = 11,
        E_STATUS = 12,
        E_SEARCH = 13,
        E_RETURN = 14,
        E_DEAD = 15,
    }

    /// <summary>
    /// SYS4's patroller AI (§4): the shared §3.1 state machine, §3.2 telegraph rules with the
    /// universal attack sign, §3.3 aggro and leash, §3.4 attack tokens, and the §4 fodder/soldier
    /// timing columns.
    ///
    /// This is the locomotion and attack half of an enemy. Damage in, HP, poise, statuses and the
    /// death hand-off stay in SYS3's <see cref="EnemyCombatTarget"/>; contact damage stays in
    /// SYS2's <see cref="ContactDamager2D"/>. The AI only reads and drives those. SYS3 also settles
    /// each hit's §8 push distance; the AI only carries the body that far, then fades a dead body
    /// out (E-DEAD) and stops drawing it (E-DORMANT).
    ///
    /// Timing is counted in whole 60 Hz ticks from <see cref="SpecUnits.FixedDeltaTime"/>, one per
    /// FixedUpdate, so §4's second values become the spec's frame counts exactly.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyCombatTarget))]
    public class EnemyAI : MonoBehaviour
    {
        // §3.4 'Enemies allowed in E-TELEGRAPH or E-ATTACK at once (melee)' is a shared cap, so
        // the pool is static. A domain reload leaves it nonzero, hence the reset below.
        private static int tokensInUse;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetAttackTokens() => tokensInUse = 0;

        /// <summary>Gap the wall probe keeps between the body and a wall, in tiles.</summary>
        private const float WallClearance = 0.06f;

        [Header("References")]
        [Tooltip("SYS4 tuning asset. Required.")]
        [SerializeField] private EnemyTuning tuning;

        [Tooltip("SYS3's damageable body. Found on this object when left empty.")]
        [SerializeField] private EnemyCombatTarget target;

        [Tooltip("SYS2's contact hitbox, switched off in E-SPAWN / E-STAGGER / E-DEAD (§15).")]
        [SerializeField] private ContactDamager2D contact;

        [Tooltip("The body sprite, flipped to face the walk direction.")]
        [SerializeField] private SpriteRenderer body;

        [Tooltip("The §3.2 attack sign. Place its component on the sign child object.")]
        [SerializeField] private AttackSign sign;

        [Header("Authored placement")]
        [Tooltip("True when the sprite art faces +x before any flip.")]
        [SerializeField] private bool artFacesRight;

        [Tooltip("How far either side of the spawn point the patrol walks, in tiles (§4 E-PATROL).")]
        [SerializeField] private float patrolHalfRangeTiles = 3f;

        [Tooltip("Initially faces left. The author sets the patrol direction.")]
        [SerializeField] private bool startFacingRight;

        [Tooltip("Solid geometry for the ledge and wall probes (Ground, Default or both).")]
        [SerializeField] private LayerMask solidLayers;

        [Tooltip("The hero's hurtbox layer, used by the strike overlap (HeroHurtbox).")]
        [SerializeField] private LayerMask heroHurtboxLayers;

        private PatrollerProfile profile;
        private HeroHealth hero;

        private EnemyState state = EnemyState.E_IDLE;
        private int stateFrames;

        private int telegraphFrames;
        private int activeFrames;
        private int recoveryFrames;
        private int cooldownFrames;
        private int searchFrames;
        private int returnFrames;

        private Vector2 homePosition;
        private Vector2 lastKnownHeroPosition;
        private int facing = -1;
        private int patrolDirection = -1;
        private float lostSightTimer;
        private float reAggroLockout;
        private bool heroVisible;
        private bool holdsToken;

        // §8 push: SYS3 resolves the distance, this carries the body over tuning.knockbackSeconds.
        private int pushTicksRemaining;
        private int pushDirection;
        private float pushStepTiles;
        private float pushRequestedTiles;
        private float pushTravelledTiles;
        private HitKind pushKind;
        private string pushStoppedBy;
        private bool pushedThisTick;
        private int deadFadeFrames;

        // ── Readout surface (debug overlays and verification) ────────────────────────

        /// <summary>Current §3.1 state.</summary>
        public EnemyState State => state;

        /// <summary>Ticks spent in the current state.</summary>
        public int StateFrames => stateFrames;

        /// <summary>The §4 column this enemy is running.</summary>
        public PatrollerProfile Profile => profile;

        /// <summary>Where the enemy was placed; §3.3 leash and E-RETURN measure from here.</summary>
        public Vector2 HomePosition => homePosition;

        /// <summary>True while §3.3 detection reports line of sight.</summary>
        public bool HeroVisible => heroVisible;

        /// <summary>+1 facing right, −1 facing left.</summary>
        public int Facing => facing;

        /// <summary>True while this enemy holds one of the §3.4 melee attack tokens.</summary>
        public bool HoldsAttackToken => holdsToken;

        /// <summary>How many melee attack tokens are in use across every enemy.</summary>
        public static int TokensInUse => tokensInUse;

        /// <summary>True while a §8 push is carrying the body.</summary>
        public bool IsPushed => pushTicksRemaining > 0;

        // ── Lifecycle ───────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (target == null) target = GetComponent<EnemyCombatTarget>();
            if (contact == null) contact = GetComponent<ContactDamager2D>();
            if (body == null) body = GetComponentInChildren<SpriteRenderer>(true);
            if (sign == null) sign = GetComponentInChildren<AttackSign>(true);

            if (tuning == null)
            {
                Debug.LogError($"[SYS4] '{name}' has no EnemyTuning assigned; AI disabled.", this);
                enabled = false;
                return;
            }

            if (target == null)
            {
                Debug.LogError($"[SYS4] '{name}' needs an EnemyCombatTarget; AI disabled.", this);
                enabled = false;
                return;
            }

            profile = tuning.ProfileFor(target.Tier);
            homePosition = transform.position;
            lastKnownHeroPosition = homePosition;

            facing = startFacingRight ? 1 : -1;
            patrolDirection = facing;
            ApplyFacing();

            target.Hit += OnTargetHit;
            target.Died += OnTargetDied;
        }

        private void OnEnable()
        {
            // A freshly enabled enemy must not inherit a stale state mid-attack.
            if (profile == null) return;
            EnterState(patrolHalfRangeTiles > 0.01f ? EnemyState.E_PATROL : EnemyState.E_IDLE);
        }

        private void OnDisable()
        {
            ReleaseToken();

            if (target != null)
            {
                target.SetAttackStartup(false);
            }

            if (sign != null) sign.Hide();
        }

        private void OnDestroy()
        {
            ReleaseToken();

            if (target != null)
            {
                target.Hit -= OnTargetHit;
                target.Died -= OnTargetDied;
            }
        }

        // ── Frame machine ───────────────────────────────────────────────────────────

        private void FixedUpdate()
        {
            if (tuning == null || profile == null) return;
            if (state == EnemyState.E_DORMANT) return;

            stateFrames++;

            if (state == EnemyState.E_DEAD)
            {
                TickDead();
                return;
            }

            if (target.IsDead)
            {
                EnterState(EnemyState.E_DEAD);
                return;
            }

            // §8: a push carries the body first and holds the AI's own walking for this tick.
            pushedThisTick = TickPush();

            // §3.1 interrupt priority: E-STATUS > E-STAGGER > E-FLINCH > everything else.
            if (TryInterrupt()) return;

            TickState(SpecUnits.FixedDeltaTime);
        }

        /// <summary>
        /// Holds the machine in whichever interrupt state SYS3's body currently reports, and
        /// releases it back into the fight when that window closes.
        /// </summary>
        private bool TryInterrupt()
        {
            if (target.IsStunned)
            {
                if (state != EnemyState.E_STATUS) EnterState(EnemyState.E_STATUS);
                return true;
            }

            if (target.IsStaggered)
            {
                if (state != EnemyState.E_STAGGER) EnterState(EnemyState.E_STAGGER);
                return true;
            }

            // §3.1 'E-FLINCH: light enemies only'.
            if (target.IsLightTier && target.HitStunRemaining > 0f)
            {
                if (state != EnemyState.E_FLINCH) EnterState(EnemyState.E_FLINCH);
                return true;
            }

            if (state == EnemyState.E_FLINCH || state == EnemyState.E_STAGGER || state == EnemyState.E_STATUS)
            {
                // The interrupt window has closed: resume the fight from E-ENGAGE.
                EnterState(heroVisible ? EnemyState.E_ENGAGE : EnemyState.E_PATROL);
                return true;
            }

            return false;
        }

        private void TickState(float dt)
        {
            switch (state)
            {
                case EnemyState.E_PATROL: TickPatrol(dt); break;
                case EnemyState.E_IDLE: TickIdle(dt); break;
                case EnemyState.E_NOTICE: TickNotice(dt); break;
                case EnemyState.E_ENGAGE: TickEngage(dt); break;
                case EnemyState.E_TELEGRAPH: TickTelegraph(); break;
                case EnemyState.E_ATTACK: TickAttack(dt); break;
                case EnemyState.E_RECOVER: TickRecover(); break;
                case EnemyState.E_COOLDOWN: TickCooldown(); break;
                case EnemyState.E_SEARCH: TickSearch(dt); break;
                case EnemyState.E_RETURN: TickReturn(dt); break;
            }
        }

        // ── States ──────────────────────────────────────────────────────────────────

        private void TickPatrol(float dt)
        {
            UpdateSight(dt);
            if (reAggroLockout > 0f) reAggroLockout -= dt;

            if (heroVisible && reAggroLockout <= 0f)
            {
                EnterState(EnemyState.E_NOTICE);
                return;
            }

            if (patrolHalfRangeTiles <= 0.01f) return;

            // Turn at the patrol limit, then walk; a blocked step (ledge or wall) also turns.
            float offset = transform.position.x - homePosition.x;
            if (patrolDirection > 0 && offset >= patrolHalfRangeTiles) patrolDirection = -1;
            else if (patrolDirection < 0 && offset <= -patrolHalfRangeTiles) patrolDirection = 1;

            if (!Walk(patrolDirection, profile.patrolSpeed, dt)) patrolDirection = -patrolDirection;
        }

        private void TickIdle(float dt)
        {
            UpdateSight(dt);
            if (reAggroLockout > 0f) reAggroLockout -= dt;

            if (heroVisible && reAggroLockout <= 0f) EnterState(EnemyState.E_NOTICE);
        }

        private void TickNotice(float dt)
        {
            UpdateSight(dt);

            if (hero != null)
            {
                lastKnownHeroPosition = hero.transform.position;
                SetFacing(lastKnownHeroPosition.x >= transform.position.x);
            }

            if (stateFrames >= Mathf.Max(1, Mathf.RoundToInt(tuning.noticeSeconds * SpecUnits.FramesPerSecond)))
            {
                EnterState(EnemyState.E_ENGAGE);
            }
        }

        private void TickEngage(float dt)
        {
            UpdateSight(dt);

            if (!heroVisible && lostSightTimer >= tuning.lostSightSeconds)
            {
                EnterState(EnemyState.E_SEARCH);
                return;
            }

            // §3.3 leash, measured from home.
            if (Mathf.Abs(transform.position.x - homePosition.x) > profile.leashTiles)
            {
                EnterState(EnemyState.E_SEARCH);
                return;
            }

            if (hero == null)
            {
                EnterState(EnemyState.E_RETURN);
                return;
            }

            if (heroVisible) lastKnownHeroPosition = hero.transform.position;
            SetFacing(lastKnownHeroPosition.x >= transform.position.x);

            float horizontal = Mathf.Abs(lastKnownHeroPosition.x - transform.position.x);
            float vertical = Mathf.Abs(lastKnownHeroPosition.y - transform.position.y);

            if (heroVisible && horizontal <= profile.attackRange && vertical <= profile.detectionVerticalTiles)
            {
                // §3.4: an enemy without a token waits inside its range instead of swinging.
                if (TryAcquireToken()) EnterState(EnemyState.E_TELEGRAPH);
                return;
            }

            Walk(facing, profile.chaseSpeed, dt);
        }

        private void TickTelegraph()
        {
            // §7: poise damage counts ×2 during an enemy's attack startup.
            target.SetAttackStartup(true);
            ShowSign();

            // §3.2 direction lock: aim tracks until directionLockSeconds before E-ATTACK.
            int lockFrames = Mathf.Max(1, Mathf.RoundToInt(tuning.directionLockSeconds * SpecUnits.FramesPerSecond));
            if (stateFrames <= telegraphFrames - lockFrames && hero != null)
            {
                lastKnownHeroPosition = hero.transform.position;
                SetFacing(lastKnownHeroPosition.x >= transform.position.x);
            }

            if (stateFrames >= telegraphFrames) EnterState(EnemyState.E_ATTACK);
        }

        private void TickAttack(float dt)
        {
            target.SetAttackStartup(true);

            Strike();

            // The lunge never leaves the ledge or passes through a wall (Walk checks both).
            if (activeFrames > 0)
            {
                float lungeSpeed = profile.lungeTiles / (activeFrames * SpecUnits.FixedDeltaTime);
                Walk(facing, lungeSpeed, dt);
            }

            if (stateFrames >= activeFrames) EnterState(EnemyState.E_RECOVER);
        }

        private void TickRecover()
        {
            if (stateFrames >= recoveryFrames) EnterState(EnemyState.E_COOLDOWN);
        }

        private void TickCooldown()
        {
            if (stateFrames < cooldownFrames) return;

            UpdateSight(0f);
            EnterState(heroVisible ? EnemyState.E_ENGAGE : EnemyState.E_PATROL);
        }

        private void TickSearch(float dt)
        {
            UpdateSight(dt);

            // §3.3 leash: once the chase has left the leash radius the enemy gives up and walks
            // home. This has to be checked before re-detecting, or an enemy held at the leash
            // limit with the hero in sight bounces E-ENGAGE <-> E-SEARCH every tick and never
            // returns.
            if (Mathf.Abs(transform.position.x - homePosition.x) > profile.leashTiles)
            {
                EnterState(EnemyState.E_RETURN);
                return;
            }

            if (heroVisible)
            {
                EnterState(EnemyState.E_ENGAGE);
                return;
            }

            if (stateFrames >= searchFrames)
            {
                EnterState(EnemyState.E_RETURN);
                return;
            }

            float dx = lastKnownHeroPosition.x - transform.position.x;
            if (Mathf.Abs(dx) > 0.1f) Walk(dx > 0f ? 1 : -1, profile.chaseSpeed, dt);
        }

        private void TickReturn(float dt)
        {
            float dx = homePosition.x - transform.position.x;

            if (Mathf.Abs(dx) <= 0.1f || stateFrames >= returnFrames)
            {
                // §3.3 decision (h): returning home keeps the damage taken; only poise resets.
                EnterState(EnemyState.E_IDLE);
                return;
            }

            Walk(dx > 0f ? 1 : -1, profile.patrolSpeed, dt);
        }

        /// <summary>
        /// E-DEAD: the H7 fling plays out first, then the body fades over tuning.deadFadeSeconds and
        /// the enemy goes E-DORMANT.
        /// </summary>
        private void TickDead()
        {
            if (TickPush()) return;

            int fadeFrames = Mathf.Max(1, Mathf.RoundToInt(tuning.deadFadeSeconds * SpecUnits.FramesPerSecond));
            deadFadeFrames++;

            if (body != null)
            {
                Color colour = body.color;
                colour.a = 1f - Mathf.Clamp01(deadFadeFrames / (float)fadeFrames);
                body.color = colour;
            }

            if (deadFadeFrames >= fadeFrames) EnterState(EnemyState.E_DORMANT);
        }

        // ── State entry ─────────────────────────────────────────────────────────────

        private void EnterState(EnemyState next)
        {
            EnemyState previous = state;

            if (previous == EnemyState.E_TELEGRAPH || previous == EnemyState.E_ATTACK)
            {
                target.SetAttackStartup(false);
                if (sign != null) sign.Hide();
                ReleaseToken();
            }

            state = next;
            stateFrames = 0;

            switch (next)
            {
                case EnemyState.E_TELEGRAPH:
                    telegraphFrames = Mathf.Max(1, Mathf.RoundToInt(profile.telegraphSeconds * SpecUnits.FramesPerSecond));
                    break;

                case EnemyState.E_ATTACK:
                    activeFrames = Mathf.Max(1, Mathf.RoundToInt(profile.activeSeconds * SpecUnits.FramesPerSecond));
                    break;

                case EnemyState.E_RECOVER:
                    recoveryFrames = Mathf.Max(1, Mathf.RoundToInt(profile.recoverySeconds * SpecUnits.FramesPerSecond));
                    ReleaseToken();
                    break;

                case EnemyState.E_COOLDOWN:
                    cooldownFrames = Mathf.Max(1, Mathf.RoundToInt(profile.cooldownSeconds * SpecUnits.FramesPerSecond));
                    break;

                case EnemyState.E_SEARCH:
                    searchFrames = Mathf.Max(1, Mathf.RoundToInt(tuning.searchSeconds * SpecUnits.FramesPerSecond));
                    break;

                case EnemyState.E_RETURN:
                    returnFrames = Mathf.Max(1, Mathf.RoundToInt(tuning.returnTimeoutSeconds * SpecUnits.FramesPerSecond));
                    reAggroLockout = tuning.reAggroLockoutSeconds;
                    ReleaseToken();
                    break;

                case EnemyState.E_DEAD:
                    ReleaseToken();
                    if (sign != null) sign.Hide();
                    deadFadeFrames = 0;
                    break;

                case EnemyState.E_DORMANT:
                    // Nothing of the enemy draws any more; SYS3 already removed its hitboxes.
                    ReleaseToken();
                    if (body != null) body.enabled = false;
                    if (sign != null) sign.Hide();
                    Debug.Log($"[SYS4] {name} E_DORMANT (from {previous}).", this);
                    break;
            }

            // §15: contact damage is off in E-SPAWN, E-STAGGER, E-DEAD and E-DORMANT.
            if (contact != null)
            {
                contact.ContactDamageEnabled =
                    next != EnemyState.E_SPAWN &&
                    next != EnemyState.E_STAGGER &&
                    next != EnemyState.E_DEAD &&
                    next != EnemyState.E_DORMANT;
            }
        }

        // ── Attack tokens (§3.4) ────────────────────────────────────────────────────

        private bool TryAcquireToken()
        {
            if (holdsToken) return true;

            int cap = tuning != null ? Mathf.Max(1, tuning.meleeAttackTokens) : 1;
            if (tokensInUse >= cap) return false;

            tokensInUse++;
            holdsToken = true;
            return true;
        }

        private void ReleaseToken()
        {
            if (!holdsToken) return;

            holdsToken = false;
            tokensInUse = Mathf.Max(0, tokensInUse - 1);
        }

        // ── The strike hitbox ───────────────────────────────────────────────────────

        /// <summary>
        /// Tests the §4 attack hitbox against the hero's hurtbox on every active frame. SYS2's
        /// post-hit i-frames make a multi-frame overlap resolve once, so no per-swing guard is
        /// needed and a hero who steps in late is still hit.
        /// </summary>
        private void Strike()
        {
            if (heroHurtboxLayers.value == 0) return;

            Vector2 centre = (Vector2)transform.position
                             + new Vector2(tuning.strikeOffset.x * facing, tuning.strikeOffset.y);

            Collider2D overlap = Physics2D.OverlapBox(centre, tuning.strikeSize, 0f, heroHurtboxLayers);
            if (overlap == null) return;

            HeroHealth health = overlap.GetComponentInParent<HeroHealth>();
            if (health == null) return;

            health.TryTakeDamage(new DamageInfo
            {
                Class = profile.strikeClass,
                SourcePosition = transform.position,
                Surface = HazardSurface.None,
                EntryVelocityX = health.Velocity.x,
                // SYS3's parry reads this back to find the attacker it should stagger.
                Source = this,
            });
        }

        // ── Perception (§3.3) ───────────────────────────────────────────────────────

        private void UpdateSight(float dt)
        {
            heroVisible = CanSeeHero();

            if (heroVisible)
            {
                lostSightTimer = 0f;
                if (hero != null) lastKnownHeroPosition = hero.transform.position;
            }
            else
            {
                lostSightTimer += dt;
            }
        }

        private bool CanSeeHero()
        {
            if (hero == null) hero = FindAnyObjectByType<HeroHealth>();
            if (hero == null || hero.IsDead) return false;

            Vector2 self = transform.position;
            Vector2 point = hero.transform.position;

            if (Mathf.Abs(point.y - self.y) > profile.detectionVerticalTiles) return false;

            float dx = point.x - self.x;
            if (Mathf.Abs(dx) > profile.detectionTiles) return false;

            // §3.3 'Detection behind: 50% of forward range'.
            bool ahead = dx * facing >= 0f;
            if (!ahead && Mathf.Abs(dx) > profile.detectionTiles * profile.detectionBehindMultiplier) return false;

            // §3.3 line of sight: solid geometry blocks; the hero's own body does not.
            RaycastHit2D blocked = Physics2D.Linecast(self, point, solidLayers);
            if (blocked.collider != null && !IsHeroCollider(blocked.collider)) return false;

            return true;
        }

        // ── Locomotion ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Steps one tick in a direction, refusing to walk into a wall or off a ledge (§4: the
        /// patroller turns at both). Returns false when the step was blocked.
        /// </summary>
        private bool Walk(int direction, float speed, float dt)
        {
            if (direction == 0) return false;

            SetFacing(direction > 0);

            // §8: the AI's own steps wait while a push carries the body.
            if (pushedThisTick) return true;

            if (WallAhead(direction) || !GroundAhead(direction)) return false;

            transform.position += new Vector3(direction * speed * dt, 0f, 0f);
            return true;
        }

        private void SetFacing(bool right)
        {
            facing = right ? 1 : -1;
            ApplyFacing();
        }

        private void ApplyFacing()
        {
            if (target != null) target.SetFacing(facing > 0);
            if (body != null) body.flipX = (facing > 0) != artFacesRight;
        }

        private bool WallAhead(int direction)
        {
            Vector2 origin = transform.position;
            float distance = tuning.bodyHalfExtents.x + WallClearance;
            Vector2 ray = direction > 0 ? Vector2.right : Vector2.left;

            RaycastHit2D hit = Physics2D.Raycast(origin, ray, distance, solidLayers);
            return hit.collider != null && !IsHeroCollider(hit.collider);
        }

        /// <param name="lookAhead">Extra tiles past the leading edge, for testing a step before it is taken.</param>
        private bool GroundAhead(int direction, float lookAhead = 0f)
        {
            // Start above the foot line and cast through it, so an enemy authored standing on the
            // floor cannot read "ground" from a ray that begins inside the floor collider.
            float halfX = tuning.bodyHalfExtents.x;
            float halfY = tuning.bodyHalfExtents.y;

            Vector2 origin = (Vector2)transform.position + new Vector2(direction * (halfX + 0.05f + lookAhead), halfY * 0.5f);
            float distance = halfY * 1.5f + tuning.ledgeProbeDepth;

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, distance, solidLayers);
            return hit.collider != null && !IsHeroCollider(hit.collider);
        }

        private static bool IsHeroCollider(Collider2D collider)
        {
            return collider != null && collider.GetComponentInParent<HeroHealth>() != null;
        }

        // ── Push (§8 distance resolved by SYS3) ──────────────────────────────────────

        /// <summary>Starts the push SYS3 resolved for this hit; a new hit replaces a running push.</summary>
        private void BeginPush(in CombatHit hit, float tiles, HitKind kind)
        {
            if (pushTicksRemaining > 0)
            {
                pushStoppedBy = "the next hit";
                FinishPush();
            }

            if (tiles <= 0f) return;

            int ticks = Mathf.Max(1, Mathf.RoundToInt(tuning.knockbackSeconds * SpecUnits.FramesPerSecond));

            pushDirection = PushDirection(hit);
            pushRequestedTiles = tiles;
            pushTravelledTiles = 0f;
            pushStepTiles = tiles / ticks;
            pushTicksRemaining = ticks;
            pushKind = kind;
            pushStoppedBy = null;
        }

        /// <summary>Horizontal only: the push's own x, else away from where the hit came from.</summary>
        private int PushDirection(in CombatHit hit)
        {
            if (Mathf.Abs(hit.Knockback.x) > 0.0001f) return hit.Knockback.x > 0f ? 1 : -1;

            float dx = transform.position.x - hit.Origin.x;
            if (Mathf.Abs(dx) > 0.0001f) return dx > 0f ? 1 : -1;

            return -facing;
        }

        /// <summary>Moves one tick of the running push. True while a push is carrying the body.</summary>
        private bool TickPush()
        {
            if (pushTicksRemaining <= 0) return false;

            float wanted = Mathf.Min(pushStepTiles, pushRequestedTiles - pushTravelledTiles);
            float allowed = PushAllowance(pushDirection, wanted, out string stoppedBy);

            if (allowed > 0f)
            {
                transform.position += new Vector3(pushDirection * allowed, 0f, 0f);
                pushTravelledTiles += allowed;
            }

            pushTicksRemaining--;

            if (stoppedBy != null)
            {
                pushStoppedBy = stoppedBy;
                pushTicksRemaining = 0;
            }

            if (pushTicksRemaining == 0) FinishPush();
            return true;
        }

        private void FinishPush()
        {
            pushTicksRemaining = 0;

            Debug.Log(
                $"[SYS4] {name} push {pushKind} requested {pushRequestedTiles:0.###} → travelled " +
                $"{pushTravelledTiles:0.###} tiles" + (pushStoppedBy != null ? $" (stopped by {pushStoppedBy})." : "."),
                this);
        }

        /// <summary>
        /// How far this tick of a push may slide, up to <paramref name="wanted"/>. Enemies have no
        /// gravity, so a wall stops the body at the clearance <see cref="WallAhead"/> keeps, and a
        /// ledge stops it while <see cref="GroundAhead"/> still finds floor.
        /// </summary>
        private float PushAllowance(int direction, float wanted, out string stoppedBy)
        {
            stoppedBy = null;
            float allowed = wanted;

            float reach = tuning.bodyHalfExtents.x + WallClearance;
            Vector2 ray = direction > 0 ? Vector2.right : Vector2.left;
            RaycastHit2D wall = Physics2D.Raycast(transform.position, ray, reach + wanted, solidLayers);

            if (wall.collider != null && !IsHeroCollider(wall.collider) && wall.distance - reach < wanted)
            {
                allowed = Mathf.Max(0f, wall.distance - reach);
                stoppedBy = "wall";
            }

            if (GroundAhead(direction, allowed)) return allowed;

            stoppedBy = "ledge";
            if (!GroundAhead(direction)) return 0f;

            // Narrow down to the last offset that still has floor under the leading probe.
            float lo = 0f;
            float hi = allowed;
            for (int i = 0; i < 8; i++)
            {
                float mid = (lo + hi) * 0.5f;
                if (GroundAhead(direction, mid)) lo = mid;
                else hi = mid;
            }

            return lo;
        }

        // ── SYS3 events ─────────────────────────────────────────────────────────────

        /// <summary>§3.3 'Damage alert: any damage → instant E-ENGAGE (skips E-NOTICE)'.</summary>
        private void OnTargetHit(EnemyCombatTarget victim, CombatHit hit)
        {
            if (!isActiveAndEnabled) return;
            if (state == EnemyState.E_DEAD || state == EnemyState.E_DORMANT) return;

            // §8: SYS3 has already settled how far this hit pushes the body (H7 on a kill).
            BeginPush(hit, victim.ResolvedKnockbackTiles, victim.ResolvedKnockbackKind);

            if (hero == null) hero = FindAnyObjectByType<HeroHealth>();
            if (hero != null) lastKnownHeroPosition = hero.transform.position;

            // The interrupt states win on the next tick; do not stomp them here.
            if (state == EnemyState.E_STAGGER || state == EnemyState.E_STATUS || state == EnemyState.E_FLINCH) return;

            EnterState(EnemyState.E_ENGAGE);
        }

        private void OnTargetDied(EnemyCombatTarget victim)
        {
            if (!isActiveAndEnabled) return;
            EnterState(EnemyState.E_DEAD);
        }

        // ── Presentation ────────────────────────────────────────────────────────────

        private void ShowSign()
        {
            if (sign == null) return;

            // The patroller slice has no unblockable attack yet, so tuning.unblockableSignColour
            // is unused; it is authored now for the archetypes that need it (§3.2).
            sign.Show(tuning.attackSignColour, tuning.attackSignScale);
        }

        private void OnDrawGizmosSelected()
        {
            if (tuning == null || profile == null) return;

            // §3.3 detection box: forward range, half the range behind, ±vertical.
            float forward = profile.detectionTiles;
            float behind = forward * profile.detectionBehindMultiplier;
            float centre = (forward - behind) * 0.5f;

            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.55f);
            Gizmos.DrawWireCube(
                (Vector3)transform.position + new Vector3(centre * facing, 0f, 0f),
                new Vector3(forward + behind, profile.detectionVerticalTiles * 2f, 0.1f));

            // §4 attack hitbox at its authored facing.
            Gizmos.color = new Color(1f, 0.35f, 0.2f, 0.6f);
            Gizmos.DrawWireCube(
                (Vector3)transform.position + new Vector3(tuning.strikeOffset.x * facing, tuning.strikeOffset.y, 0f),
                new Vector3(tuning.strikeSize.x, tuning.strikeSize.y, 0.1f));

            // §3.3 leash radius from home.
            Gizmos.color = new Color(1f, 0.9f, 0.3f, 0.35f);
            Gizmos.DrawLine(
                new Vector3(homePosition.x - profile.leashTiles, homePosition.y, 0f),
                new Vector3(homePosition.x + profile.leashTiles, homePosition.y, 0f));
        }
    }
}
