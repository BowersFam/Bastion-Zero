using UnityEngine;
using UnityEngine.InputSystem;
using BastionZero.Movement;

/// <summary>
/// Consumes the project's existing Player/Attack action and coordinates the slash
/// animation with a short, precisely timed hit window. Startup and recovery are
/// silent (hitbox closed) so only the authored impact frames can hit targets.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class NexusAttack : MonoBehaviour
{
    private const string AttackActionName = "Player/Attack";
    private const string WingRevealStateName = "WingReveal";

    private static readonly int AttackParam = Animator.StringToHash("Attack");

    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Animator animator;
    [SerializeField] private AttackHitbox2D hitbox;
    [SerializeField] private Transform hitboxTransform;
    [SerializeField] private PlayerMovement movement;

    [Header("Hit Window Placement")]
    [Tooltip("Horizontal distance from Nexus's centre to the slash volume, in world units.")]
    [SerializeField] private float hitboxForwardOffset = 0.62f;
    [SerializeField] private float hitboxLocalHeight = 0.05f;

    [Header("Timing")]
    [SerializeField] private float startupDuration = 0.06f;
    [SerializeField] private float activeDuration = 0.14f;
    [SerializeField] private float recoveryDuration = 0.16f;

    private InputAction attackAction;
    private bool inputReady;

    private float phaseTimer;
    private int phase;   // 0 = idle, 1 = startup, 2 = active, 3 = recovery

    /// <summary>True from the start of startup until recovery has finished.</summary>
    public bool IsAttacking => phase != 0;

    private void Awake()
    {
        if (playerInput == null) playerInput = GetComponent<PlayerInput>();
        if (animator == null) animator = GetComponent<Animator>();
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (hitbox == null) hitbox = GetComponentInChildren<AttackHitbox2D>(true);
        if (hitboxTransform == null && hitbox != null) hitboxTransform = hitbox.transform;

        if (hitbox != null) hitbox.SetActive(false);

        if (playerInput == null || playerInput.actions == null)
        {
            Debug.LogError($"[NexusAttack] '{name}' has no PlayerInput with an assigned actions asset. Attack disabled.", this);
            enabled = false;
            return;
        }

        attackAction = playerInput.actions.FindAction(AttackActionName);
        if (attackAction == null)
        {
            Debug.LogError($"[NexusAttack] Could not find '{AttackActionName}' in '{playerInput.actions.name}'. Attack disabled.", this);
            enabled = false;
            return;
        }

        inputReady = true;
    }

    private void OnEnable()
    {
        // PlayerInput owns the action-map lifecycle; this component only subscribes
        // to the one action it consumes, matching PlayerMovement's pattern.
        // Bound to 'performed' rather than 'started': a Button action reports
        // Started then Performed, so a 'started' handler would never see
        // context.performed set.
        if (!inputReady || attackAction == null) return;
        attackAction.performed += BeginAttack;
    }

    private void OnDisable()
    {
        if (attackAction != null) attackAction.performed -= BeginAttack;

        if (hitbox != null) hitbox.SetActive(false);
        phase = 0;
        phaseTimer = 0f;
    }

    /// <summary>Input callback for Player/Attack.</summary>
    public void BeginAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        TryStartAttack();
    }

    /// <summary>
    /// Starts a slash when nothing else owns the character. Exposed so scripted
    /// interactions (or AI-driven input) can drive the same code path.
    /// </summary>
    public bool TryStartAttack()
    {
        if (!enabled || !inputReady || animator == null || hitbox == null) return false;
        if (IsAttacking) return false;
        if (IsWingRevealPlaying()) return false;

        // Attacks are a grounded action; the controller only routes to Attack from
        // Idle/Walk/Run, so firing a trigger mid-air would leave it stale.
        if (movement != null && !movement.IsGrounded) return false;

        PlaceHitbox(movement != null && movement.IsFacingLeft);
        animator.SetTrigger(AttackParam);

        phase = 1;
        phaseTimer = startupDuration;
        return true;
    }

    private bool IsWingRevealPlaying()
    {
        if (animator == null) return false;
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        return state.IsName(WingRevealStateName) && !animator.IsInTransition(0);
    }

    private void PlaceHitbox(bool facingLeft)
    {
        if (hitboxTransform == null) return;

        Vector3 local = hitboxTransform.localPosition;
        local.x = facingLeft ? -hitboxForwardOffset : hitboxForwardOffset;
        local.y = hitboxLocalHeight;
        hitboxTransform.localPosition = local;
    }

    private void Update()
    {
        if (phase == 0) return;

        phaseTimer -= Time.deltaTime;
        if (phaseTimer > 0f) return;

        switch (phase)
        {
            case 1: // startup finished — open the impact window
                phase = 2;
                phaseTimer = activeDuration;
                if (hitbox != null) hitbox.SetActive(true);
                break;

            case 2: // impact window finished — close and recover
                phase = 3;
                phaseTimer = recoveryDuration;
                if (hitbox != null) hitbox.SetActive(false);
                break;

            default: // recovery finished
                phase = 0;
                phaseTimer = 0f;
                break;
        }
    }
}
