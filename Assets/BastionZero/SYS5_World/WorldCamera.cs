using System;
using BastionZero.Combat;
using BastionZero.Movement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BastionZero.World
{
    /// <summary>
    /// SYS5 §10 world camera. Owns the whole camera state list — C-FOLLOW, C-ZONE, C-LOOK,
    /// C-ARENA, C-CUT and C-HOLD — and every value it frames with comes from
    /// <see cref="WorldCameraTuning"/> under its WC-W row.
    ///
    /// Framing is two independent channels:
    /// <list type="bullet">
    /// <item>horizontal: look-ahead (WC-W4/W5/W11) → dead zone (WC-W2) → trailing (WC-W3);</item>
    /// <item>vertical: jump follow (WC-W6) → platform snap (WC-W7) → fall lock and look-down
    /// (WC-W8) around the grounded placement of WC-W9.</item>
    /// </list>
    /// The room clamp (WC-W10) is applied last, so the camera never frames past the room edge even
    /// when the look-ahead pushes into a wall.
    ///
    /// The camera runs at execution order −100 and steps on <c>Time.unscaledDeltaTime</c>, so it
    /// keeps framing through hit-stop (WC-W13); <see cref="BastionZero.Combat.CombatCameraShake"/>
    /// runs at 1000 and adds its offset on top.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public class WorldCamera : MonoBehaviour
    {
        [Header("Binding")]
        [SerializeField] private Transform target;
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private HeroCombat combat;
        [SerializeField] private Camera viewCamera;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private WorldCameraTuning tuning;

        [Tooltip("Hero origin to the soles of the sprite, in the hero's local space. −0.91 for the " +
                 "256 px frame at 128 px/unit, the same point GroundCheck sits on.")]
        [SerializeField] private float heroFeetLocalY = -0.91f;

        [Header("Room clamp (WC-W10)")]
        [SerializeField] private CameraRoomBounds roomBounds;

        [Tooltip("When no room bounds are assigned, look for one on this object, a parent, or " +
                 "anywhere in the loaded scene.")]
        [SerializeField] private bool findRoomBounds = true;

        [Header("Readout")]
        [SerializeField] private bool logTransitions;

        private CameraState state = CameraState.C_FOLLOW;

        // Horizontal channel.
        private float camX;
        private int lookSign = 1;
        private float lookCurrentTiles;
        private float holdTimer;
        private float turnAnchorX;
        private bool turnAnchored;

        // Vertical channel.
        private float verticalY;
        private bool wasGrounded = true;
        private bool hasFramed;
        private float takeoffCamY;
        private float takeoffHeroY;
        private float apexHeroY;
        private float airTime;
        private float fallTime;
        private float fastFallTime;
        private float lookDownCurrent;
        private bool snapActive;
        private float snapFromY;
        private float snapToY;
        private float snapElapsed;

        // C-LOOK.
        private float lookHoldTimer;
        private float lookPanY;
        private InputAction moveAction;

        // C-ZONE.
        private CameraZone[] zones = Array.Empty<CameraZone>();
        private CameraZone activeZone;
        private float zoneBlend;

        // C-ARENA.
        private Rect arenaFrame;
        private bool arenaActive;

        // C-CUT.
        private int cutFramesLeft;

        // WC-W1.
        private float orthoSize;

        /// <summary>The state the camera is framing in this frame (SYS5 §10.1).</summary>
        public CameraState State => state;

        /// <summary>WC-W1: the orthographic size resolved from the viewport and the game view aspect.</summary>
        public float OrthographicSize => orthoSize;

        /// <summary>WC-W4/W5/W11: the signed look-ahead currently applied, in tiles.</summary>
        public float LookAheadTiles => lookCurrentTiles;

        /// <summary>WC-W4: which way the dual-forward focus currently leans (−1 / +1).</summary>
        public int LookAheadSign => lookSign;

        /// <summary>WC-W15 C-LOOK: the current vertical pan, in tiles.</summary>
        public float LookPanTiles => lookPanY;

        /// <summary>WC-W7: true while a platform snap is easing the vertical channel.</summary>
        public bool VerticalSnapActive => snapActive;

        /// <summary>WC-W10: the clamp rectangle in use, or a zero rect when there is none.</summary>
        public Rect RoomBounds => roomBounds != null ? roomBounds.WorldRect : default(Rect);

        /// <summary>
        /// WC-W14: the live camera rectangle in world units — look-ahead included — which SYS4
        /// uses for its off-screen telegraph and R3's off-camera respawn checks.
        /// </summary>
        public Rect WorldViewport
        {
            get
            {
                float halfH = orthoSize > 0f ? orthoSize : 6.75f;
                float aspect = viewCamera != null ? viewCamera.aspect : (16f / 9f);
                float halfW = halfH * aspect;
                Vector2 c = transform.position;
                return new Rect(c.x - halfW, c.y - halfH, halfW * 2f, halfH * 2f);
            }
        }

        // ── Public API for the other SYS5 pieces ────────────────────────────────────────

        /// <summary>Hands the camera its hero and tuning when a room loader builds the scene.</summary>
        public void Bind(Transform hero, PlayerMovement playerMovement, HeroCombat heroCombat,
                         WorldCameraTuning cameraTuning)
        {
            target = hero;
            movement = playerMovement;
            combat = heroCombat;
            if (cameraTuning != null) tuning = cameraTuning;
            hasFramed = false;
        }

        /// <summary>Hands the camera the bounds of the room it should not frame past.</summary>
        public void SetRoomBounds(CameraRoomBounds bounds)
        {
            roomBounds = bounds;
        }

        /// <summary>C-CUT: repositions instantly (transitions, warp, respawn). One frame.</summary>
        public void CutTo(Vector2 worldCentre)
        {
            camX = worldCentre.x;
            verticalY = worldCentre.y;
            lookCurrentTiles = 0f;
            lookPanY = 0f;
            lookDownCurrent = 0f;
            zoneBlend = 0f;
            snapActive = false;
            hasFramed = true;
            transform.position = new Vector3(camX, verticalY, transform.position.z);
            state = CameraState.C_CUT;
            cutFramesLeft = 1;
            Log("C-CUT → " + worldCentre);
        }

        /// <summary>C-ARENA: locks a fixed frame over the arena until the fight ends.</summary>
        public void EnterArena(Rect worldFrame)
        {
            arenaFrame = worldFrame;
            arenaActive = true;
            state = CameraState.C_ARENA;
            Log("C-ARENA " + worldFrame);
        }

        /// <summary>C-ARENA exit, on B-DEFEAT or the last wave clearing.</summary>
        public void ExitArena()
        {
            if (!arenaActive) return;
            arenaActive = false;
            if (state == CameraState.C_ARENA) state = CameraState.C_FOLLOW;
            hasFramed = false;
            Log("C-FOLLOW (arena cleared)");
        }

        /// <summary>C-HOLD: freezes the camera where it is (SYS2 D-2 death hold).</summary>
        public void Hold()
        {
            state = CameraState.C_HOLD;
            Log("C-HOLD");
        }

        /// <summary>C-HOLD release; the camera re-frames the hero on respawn.</summary>
        public void ReleaseHold()
        {
            if (state == CameraState.C_HOLD) state = CameraState.C_FOLLOW;
            hasFramed = false;
            Log("C-FOLLOW (hold released)");
        }

        /// <summary>Re-scans the loaded scene for C-ZONE volumes.</summary>
        public void RefreshZones()
        {
            zones = FindObjectsByType<CameraZone>(FindObjectsInactive.Exclude);
            activeZone = null;
            zoneBlend = 0f;
        }

        // ── Lifecycle ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            Resolve();
            ApplyViewport();
            LookForRoomBounds();
        }

        private void Start()
        {
            RefreshZones();
            FrameInstantly();
        }

        private void OnValidate()
        {
            Resolve();
            if (viewCamera != null && tuning != null)
            {
                viewCamera.orthographic = true;
                viewCamera.orthographicSize = tuning.ResolveOrthographicSize(viewCamera.aspect);
                orthoSize = viewCamera.orthographicSize;
            }
        }

        private void Resolve()
        {
            if (viewCamera == null) viewCamera = GetComponent<Camera>();
            if (viewCamera == null) viewCamera = Camera.main;

            if (target == null && movement != null) target = movement.transform;

            if (target == null)
            {
                var found = GameObject.Find("Hero");
                if (found != null)
                {
                    target = found.transform;
                    if (movement == null) movement = found.GetComponent<PlayerMovement>();
                    if (combat == null) combat = found.GetComponent<HeroCombat>();
                    if (playerInput == null) playerInput = found.GetComponent<PlayerInput>();
                }
            }

            if (movement == null && target != null) movement = target.GetComponent<PlayerMovement>();
            if (combat == null && target != null) combat = target.GetComponent<HeroCombat>();
            if (playerInput == null && target != null) playerInput = target.GetComponent<PlayerInput>();

            if (moveAction == null && playerInput != null && playerInput.actions != null)
            {
                moveAction = playerInput.actions.FindAction("Player/Move", false);
            }
        }

        private void LookForRoomBounds()
        {
            if (roomBounds != null || !findRoomBounds) return;
            roomBounds = GetComponentInParent<CameraRoomBounds>();
            if (roomBounds == null) roomBounds = FindAnyObjectByType<CameraRoomBounds>();
        }

        private void ApplyViewport()
        {
            if (viewCamera == null || tuning == null) return;
            viewCamera.orthographic = true;
            orthoSize = tuning.ResolveOrthographicSize(viewCamera.aspect);
            viewCamera.orthographicSize = orthoSize;
        }

        private void FrameInstantly()
        {
            if (target == null) return;
            if (movement != null) lookSign = movement.IsFacingLeft ? -1 : 1;
            wasGrounded = movement == null || movement.IsGrounded;
            verticalY = FeetY() + (tuning != null ? tuning.W9_FeetBelowCentreTiles : 3f);
            camX = target.position.x;
            lookCurrentTiles = 0f;
            lookDownCurrent = 0f;
            transform.position = new Vector3(camX, verticalY, transform.position.z);
            ClampHard();
            hasFramed = true;
        }

        private void LateUpdate()
        {
            if (tuning == null) return;
            if (target == null)
            {
                Resolve();
                if (target == null) return;
            }

            ApplyViewport();

            // WC-W13: the camera ignores time scale, so hit-stop never freezes the framing.
            float dt = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);

            if (state == CameraState.C_HOLD) return;

            if (cutFramesLeft > 0)
            {
                cutFramesLeft--;
                if (cutFramesLeft == 0) state = CameraState.C_FOLLOW;
                return;
            }

            if (!hasFramed) FrameInstantly();

            UpdateZoneState();
            UpdateLookState(dt);

            if (state == CameraState.C_ARENA)
            {
                Vector2 fixedFrame = arenaFrame.center;
                transform.position = new Vector3(fixedFrame.x, fixedFrame.y, transform.position.z);
                return;
            }

            Vector2 follow = StepFollow(dt);
            Vector2 final = BlendZone(follow, dt);

            transform.position = new Vector3(final.x, final.y, transform.position.z);
            ClampHard();
        }

        // ── C-ZONE ────────────────────────────────────────────────────────────────────

        private void UpdateZoneState()
        {
            if (state == CameraState.C_ARENA) return;

            Vector2 hero = target.position;
            CameraZone inside = null;
            for (int i = 0; i < zones.Length; i++)
            {
                if (zones[i] != null && zones[i].isActiveAndEnabled && zones[i].Contains(hero))
                {
                    inside = zones[i];
                    break;
                }
            }

            if (inside != null)
            {
                if (activeZone != inside)
                {
                    activeZone = inside;
                    Log("C-ZONE " + inside.Mode);
                }
                if (state == CameraState.C_FOLLOW || state == CameraState.C_LOOK)
                {
                    state = CameraState.C_ZONE;
                }
            }
            else if (activeZone != null)
            {
                activeZone = null;
                if (state == CameraState.C_ZONE) state = CameraState.C_FOLLOW;
                Log("C-FOLLOW (left zone)");
            }
        }

        // ── C-LOOK ────────────────────────────────────────────────────────────────────

        private void UpdateLookState(float dt)
        {
            if (state == CameraState.C_ARENA || state == CameraState.C_ZONE)
            {
                lookPanY = 0f;
                lookHoldTimer = 0f;
                return;
            }

            float vertical = 0f;
            if (moveAction != null)
            {
                Vector2 move = moveAction.ReadValue<Vector2>();
                if (Mathf.Abs(move.y) >= tuning.W15_LookInputDeadZone) vertical = Mathf.Sign(move.y);
            }

            bool idle = movement == null ||
                        (movement.IsGrounded &&
                         Mathf.Abs(movement.Velocity.x) <= tuning.LookIdleSpeedTilesPerSecond);

            if (idle && vertical != 0f)
            {
                lookHoldTimer += dt;
                if (lookHoldTimer >= tuning.W15_LookHoldSeconds) state = CameraState.C_LOOK;
            }
            else
            {
                lookHoldTimer = 0f;
                if (state == CameraState.C_LOOK) state = CameraState.C_FOLLOW;
            }

            float wantedPan = state == CameraState.C_LOOK ? vertical * tuning.W15_LookTiles : 0f;
            float speed = tuning.W15_LookTiles / Mathf.Max(0.001f, tuning.W15_LookEaseSeconds);
            lookPanY = Mathf.MoveTowards(lookPanY, wantedPan, speed * dt);
        }

        // ── C-FOLLOW ──────────────────────────────────────────────────────────────────

        private float FeetY() => target.position.y + heroFeetLocalY;

        private Vector2 StepFollow(float dt)
        {
            float heroX = target.position.x;
            float heroY = target.position.y;
            bool grounded = movement == null || movement.IsGrounded;

            // ── horizontal ────────────────────────────────────────────────────────────
            bool clinging = movement != null && movement.IsClinging;
            bool bouncing = combat != null && combat.AirborneHitChain > 0;

            int inputSign = 0;
            if (movement != null && !clinging)
            {
                float ix = movement.MoveInput.x;
                if (Mathf.Abs(ix) > 0.3f) inputSign = ix > 0f ? 1 : -1;
            }
            if (inputSign != 0) holdTimer += dt; else holdTimer = 0f;

            bool armed;
            int desiredSign = lookSign;

            if (clinging)
            {
                // WC-W11: against a wall the look-ahead flips to the jump direction.
                desiredSign = movement.ClingSide != 0 ? movement.ClingSide : lookSign;
                armed = true;
            }
            else if (bouncing && movement != null)
            {
                desiredSign = movement.IsFacingLeft ? -1 : 1;
                armed = true;
            }
            else if (holdTimer >= tuning.W4_HoldSeconds && movement != null)
            {
                // WC-W4: the look-ahead only appears after 0.25 s of held input.
                desiredSign = movement.IsFacingLeft ? -1 : 1;
                armed = true;
            }
            else
            {
                armed = false;
            }

            // WC-W4 dual-forward focus: a flip waits until the hero has travelled the turn
            // threshold in the new direction.
            if (armed && desiredSign != lookSign)
            {
                if (!turnAnchored)
                {
                    turnAnchored = true;
                    turnAnchorX = heroX;
                }
                if (Mathf.Abs(heroX - turnAnchorX) >= tuning.W4_TurnThresholdTiles)
                {
                    lookSign = desiredSign;
                    turnAnchored = false;
                }
            }
            else
            {
                turnAnchored = false;
            }

            float lookTiles = bouncing ? tuning.W5_BounceLookAheadTiles : tuning.W4_LookAheadTiles;
            float lookTarget = armed ? lookSign * lookTiles : 0f;
            float lookSpeed = tuning.W4_LookAheadTiles / Mathf.Max(0.001f, tuning.W4_EaseSeconds);
            lookCurrentTiles = Mathf.MoveTowards(lookCurrentTiles, lookTarget, lookSpeed * dt);

            float framingX = heroX + lookCurrentTiles;
            float halfZone = tuning.W2_HorizontalDeadZoneTiles * 0.5f;

            float targetX = camX;
            if (framingX > camX + halfZone) targetX = framingX - halfZone;
            else if (framingX < camX - halfZone) targetX = framingX + halfZone;

            targetX = ClampAxis(targetX, true);

            float hT = 1f - Mathf.Exp(-dt / Mathf.Max(0.001f, tuning.W3_HorizontalTrailSeconds));
            camX = Mathf.Lerp(camX, targetX, hT);

            // ── vertical ──────────────────────────────────────────────────────────────
            float feetY = FeetY();

            if (grounded && !wasGrounded)
            {
                // Landing: WC-W7 eases to the new ground height.
                BeginSnap(feetY + tuning.W9_FeetBelowCentreTiles);
                airTime = 0f;
                fallTime = 0f;
                fastFallTime = 0f;
                lookDownCurrent = 0f;
            }
            else if (!grounded && wasGrounded)
            {
                // Take-off: WC-W6 holds the frame, then follows 0.6× the rise.
                takeoffCamY = verticalY;
                takeoffHeroY = heroY;
                apexHeroY = heroY;
                airTime = 0f;
                fallTime = 0f;
                fastFallTime = 0f;
                snapActive = false;
            }

            if (grounded)
            {
                lookDownCurrent = 0f;
                float wanted = feetY + tuning.W9_FeetBelowCentreTiles;
                if (!snapActive)
                {
                    if (Mathf.Abs(wanted - verticalY) > tuning.W7_SnapDeadbandTiles) BeginSnap(wanted);
                    else verticalY = wanted;
                }
                StepSnap(dt);
            }
            else
            {
                airTime += dt;
                apexHeroY = Mathf.Max(apexHeroY, heroY);

                float vy = movement != null ? movement.Velocity.y : 0f;
                if (vy < 0f) fallTime += dt; else fallTime = 0f;

                if (airTime < tuning.W6_JumpFollowDelaySeconds)
                {
                    // WC-W6: hold the frame for the first 0.2 s of the jump.
                    verticalY = takeoffCamY;
                }
                else if (fallTime < tuning.W8_FallLockSeconds)
                {
                    verticalY = takeoffCamY + (heroY - takeoffHeroY) * tuning.W6_JumpFollowFactor;
                }
                else
                {
                    // WC-W8: locked on — full follow of the descent from the apex.
                    float apexOffset = (apexHeroY - takeoffHeroY) * tuning.W6_JumpFollowFactor;
                    verticalY = takeoffCamY + apexOffset + (heroY - apexHeroY);
                }

                float fallCap = movement != null && movement.Tuning != null
                    ? movement.Tuning.MV_17_FallCap
                    : 19f;
                if (vy <= -tuning.W8_FallSpeedFraction * fallCap) fastFallTime += dt;
                else fastFallTime = 0f;

                // WC-W8 look-down: a steady lead, eased in once, never accumulating.
                float leadTarget = fastFallTime >= tuning.W8_LookDownHoldSeconds
                    ? tuning.W8_LookDownTiles
                    : 0f;
                float leadSpeed = tuning.W8_LookDownTiles /
                                  Mathf.Max(0.001f, tuning.W7_PlatformSnapSeconds);
                lookDownCurrent = Mathf.MoveTowards(lookDownCurrent, leadTarget, leadSpeed * dt);
                verticalY -= lookDownCurrent;
            }

            wasGrounded = grounded;

            float y = ClampAxis(verticalY + lookPanY, false);
            return new Vector2(camX, y);
        }

        private void BeginSnap(float toY)
        {
            snapFromY = verticalY;
            snapToY = toY;
            snapElapsed = 0f;
            snapActive = true;
        }

        private void StepSnap(float dt)
        {
            if (!snapActive) return;
            snapElapsed += dt;
            float t = tuning.W7_PlatformSnapSeconds > 0f
                ? Mathf.Clamp01(snapElapsed / tuning.W7_PlatformSnapSeconds)
                : 1f;
            verticalY = Mathf.Lerp(snapFromY, snapToY, Mathf.SmoothStep(0f, 1f, t));
            if (t >= 1f) snapActive = false;
        }

        // ── C-ZONE blend ──────────────────────────────────────────────────────────────

        private Vector2 BlendZone(Vector2 follow, float dt)
        {
            float ease = Mathf.Max(0.001f, tuning.W15_ZoneEaseSeconds);

            if (state == CameraState.C_ZONE && activeZone != null)
                zoneBlend = Mathf.MoveTowards(zoneBlend, 1f, dt / ease);
            else
                zoneBlend = Mathf.MoveTowards(zoneBlend, 0f, dt / ease);

            if (zoneBlend <= 0f || activeZone == null) return follow;

            Vector2 anchor = activeZone.Anchor;
            switch (activeZone.Mode)
            {
                case CameraZone.ZoneMode.LockHorizontal:
                    return new Vector2(Mathf.Lerp(follow.x, anchor.x, zoneBlend), follow.y);
                case CameraZone.ZoneMode.LockVertical:
                    return new Vector2(follow.x, Mathf.Lerp(follow.y, anchor.y, zoneBlend));
                default:
                    return Vector2.Lerp(follow, anchor, zoneBlend);
            }
        }

        // ── WC-W10 room clamp ─────────────────────────────────────────────────────────

        private float HalfViewportWidth()
        {
            float halfH = orthoSize > 0f ? orthoSize : 6.75f;
            float aspect = viewCamera != null ? viewCamera.aspect : (16f / 9f);
            return halfH * aspect;
        }

        private float ClampAxis(float value, bool horizontal)
        {
            if (roomBounds == null) return value;
            Rect r = roomBounds.WorldRect;

            float half = horizontal
                ? HalfViewportWidth()
                : (orthoSize > 0f ? orthoSize : 6.75f);
            float min = horizontal ? r.xMin : r.yMin;
            float max = horizontal ? r.xMax : r.yMax;

            // A room narrower than the viewport is centred on the room instead of clamped.
            if (max - min <= half * 2f) return (min + max) * 0.5f;
            return Mathf.Clamp(value, min + half, max - half);
        }

        private void ClampHard()
        {
            if (roomBounds == null) return;
            Vector3 p = transform.position;
            float x = ClampAxis(p.x, true);
            float y = ClampAxis(p.y, false);
            if (!Mathf.Approximately(x, p.x) || !Mathf.Approximately(y, p.y))
            {
                transform.position = new Vector3(x, y, p.z);
            }
        }

        private void Log(string message)
        {
            if (logTransitions) Debug.Log("[WorldCamera] " + message);
        }
    }
}
