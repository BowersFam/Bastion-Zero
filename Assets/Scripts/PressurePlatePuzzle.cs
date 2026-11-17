using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Simple pressure plate: while Nexus stands on it the assigned door blocker is
/// opened, and it closes again when Nexus steps off. No timer, no softlock window.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PressurePlatePuzzle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Collider2D doorCollider;
    [Tooltip("Renderer on the door object. Hidden while the door is open; the door object itself is never deactivated.")]
    [SerializeField] private SpriteRenderer doorVisual;
    [SerializeField] private SpriteRenderer plateVisual;
    [SerializeField] private Transform plateVisualTransform;

    [Header("Visual State")]
    [SerializeField] private Color idleColor = new Color(0.34f, 0.30f, 0.24f, 1f);
    [SerializeField] private Color pressedColor = new Color(0.68f, 0.61f, 0.36f, 1f);
    [SerializeField] private float pressedSinkOffset = 0.06f;

    [Header("Behaviour")]
    [Tooltip("When true the door stays open after the plate is pressed once, so a lone player can pass through.")]
    [SerializeField] private bool latchOpen = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onPressed;
    [SerializeField] private UnityEvent onReleased;

    private int playersOnPlate;
    private Vector3 idleLocalPosition;
    private bool isPressed;
    private bool hasBeenPressed;
    private bool stateReported;
    private bool lastPressed;

    public bool IsPressed => isPressed;

    /// <summary>
    /// Raised when the plate's pressed state actually changes (arg is the new pressed
    /// state). The initial "released" state at scene start is not reported.
    /// </summary>
    public event Action<bool> StateChanged;

    private void Awake()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        if (trigger == null)
        {
            Debug.LogError($"[PressurePlatePuzzle] '{name}' has no Collider2D trigger.", this);
            enabled = false;
            return;
        }

        trigger.isTrigger = true;

        if (doorCollider == null)
        {
            Debug.LogError($"[PressurePlatePuzzle] '{name}' has no door collider assigned; the puzzle cannot open anything.", this);
            enabled = false;
            return;
        }

        if (doorVisual == null)
        {
            Debug.LogWarning($"[PressurePlatePuzzle] '{name}' has no door SpriteRenderer assigned; the door will unblock with no visual feedback.", this);
        }

        if (plateVisualTransform == null && plateVisual != null) plateVisualTransform = plateVisual.transform;
        if (plateVisualTransform != null) idleLocalPosition = plateVisualTransform.localPosition;

        ApplyState(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        playersOnPlate++;
        if (playersOnPlate == 1) ApplyState(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        playersOnPlate = Mathf.Max(0, playersOnPlate - 1);
        if (playersOnPlate == 0) ApplyState(false);
    }

    private void ApplyState(bool pressed)
    {
        if (pressed) hasBeenPressed = true;

        // With latching on, the door stays open once triggered.
        bool doorOpen = pressed || (latchOpen && hasBeenPressed);
        isPressed = pressed;

        if (doorCollider != null) doorCollider.enabled = !doorOpen;

        // The door object and its collider stay alive; only the visual is hidden, so
        // opening reads clearly without destroying the blocker.
        if (doorVisual != null) doorVisual.enabled = !doorOpen;

        Color visualColor = doorOpen ? pressedColor : idleColor;
        if (plateVisual != null) plateVisual.color = visualColor;

        if (plateVisualTransform != null)
        {
            plateVisualTransform.localPosition = doorOpen
                ? idleLocalPosition + new Vector3(0f, -pressedSinkOffset, 0f)
                : idleLocalPosition;
        }

        if (pressed)
        {
            Debug.Log("[PressurePlatePuzzle] Plate pressed — door opened.");
            onPressed?.Invoke();
        }
        else
        {
            Debug.Log($"[PressurePlatePuzzle] Plate released — door {(latchOpen ? "stays open" : "closed")}.");
            onReleased?.Invoke();
        }

        // C# route for listeners that are not UnityEvents (feedback, audio, UI).
        if (stateReported && lastPressed != pressed) StateChanged?.Invoke(pressed);
        stateReported = true;
        lastPressed = pressed;
    }
}
