using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Attack-operated environmental switch. A valid slash (never body contact)
/// activates the switch once and opens the linked route gate or bridge.
/// Deliberately separate from <see cref="PressurePlatePuzzle"/>.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class StrikeSwitchPuzzle : MonoBehaviour, IAttackReceiver
{
    [Header("Gate")]
    [SerializeField] private Collider2D gateCollider;
    [SerializeField] private SpriteRenderer gateVisual;

    [Header("Switch Visual")]
    [SerializeField] private SpriteRenderer switchVisual;
    [SerializeField] private Color idleColor = new Color(0.32f, 0.26f, 0.42f, 1f);
    [SerializeField] private Color activeColor = new Color(0.85f, 0.42f, 0.95f, 1f);
    [SerializeField] private float activePulseScale = 1.18f;

    [Header("Events")]
    [SerializeField] private UnityEvent onActivated;

    private Vector3 switchBaseScale = Vector3.one;
    private bool isOpen;

    /// <summary>True once a slash has activated the switch.</summary>
    public bool IsOpen => isOpen;

    /// <summary>Raised once, on the slash that activates the switch.</summary>
    public event Action Activated;

    private void Awake()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        if (trigger == null)
        {
            Debug.LogError($"[StrikeSwitchPuzzle] '{name}' has no Collider2D trigger.", this);
            enabled = false;
            return;
        }

        trigger.isTrigger = true;

        if (gateCollider == null)
        {
            Debug.LogError($"[StrikeSwitchPuzzle] '{name}' has no gate collider assigned; the switch cannot open the route.", this);
            enabled = false;
            return;
        }

        if (gateVisual == null)
        {
            Debug.LogWarning($"[StrikeSwitchPuzzle] '{name}' has no gate SpriteRenderer assigned; the gate will unblock with no visual feedback.", this);
        }

        if (switchVisual == null) switchVisual = GetComponent<SpriteRenderer>();
        if (switchVisual != null)
        {
            switchBaseScale = switchVisual.transform.localScale;
            switchVisual.color = idleColor;
        }

        SetOpen(false);
    }

    /// <summary>Accepts a slash. Repeat hits after activation are ignored.</summary>
    public void ReceiveAttack()
    {
        if (isOpen) return;

        Debug.Log($"[StrikeSwitchPuzzle] '{name}' struck — route gate open.");
        SetOpen(true);
        onActivated?.Invoke();
        Activated?.Invoke();
    }

    /// <summary>Opens or closes the linked gate. Opening is one-way once struck.</summary>
    public void SetOpen(bool open)
    {
        if (isOpen && !open) return;   // never re-close a struck switch

        isOpen = open;

        if (gateCollider != null) gateCollider.enabled = !open;
        if (gateVisual != null) gateVisual.enabled = !open;

        if (switchVisual != null)
        {
            switchVisual.color = open ? activeColor : idleColor;
            switchVisual.transform.localScale = open ? switchBaseScale * activePulseScale : switchBaseScale;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Body contact is intentionally ignored: only a live slash may activate it.
    }
}
