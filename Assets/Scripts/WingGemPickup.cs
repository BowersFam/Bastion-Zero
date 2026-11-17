using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// The level's final touch pickup: on the first Nexus contact it grants the
/// run-scoped wing unlock, swaps in the winged animation set, plays the reveal and
/// turns itself off. The run's starting state belongs to
/// <see cref="OpeningLevelRunInitializer"/>, never to this pickup.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WingGemPickup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private NexusWingAppearance appearance;
    [SerializeField] private SpriteRenderer gemVisual;

    [Header("Events")]
    [SerializeField] private UnityEvent onCollected;

    [Header("Power Up")]
    [Tooltip("Tint for the burst that plays when Nexus takes the gem.")]
    [SerializeField] private Color powerUpColor = new Color(1f, 0.86f, 0.38f, 1f);

    /// <summary>True once Nexus has taken the gem.</summary>
    public bool HasBeenCollected { get; private set; }

    /// <summary>Raised once, on the accepted pickup. Consumed by feedback and UI.</summary>
    public event Action<WingGemPickup> Collected;

    private void Awake()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        if (trigger == null)
        {
            Debug.LogError($"[WingGemPickup] '{name}' has no Collider2D to act as a pickup trigger.", this);
            enabled = false;
            return;
        }

        trigger.isTrigger = true;

        if (appearance == null) appearance = FindAnyObjectByType<NexusWingAppearance>();
        if (gemVisual == null) gemVisual = GetComponentInChildren<SpriteRenderer>(true);

        trigger.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (HasBeenCollected || other == null) return;
        if (!other.CompareTag(playerTag)) return;

        HasBeenCollected = true;

        if (appearance != null)
        {
            appearance.UnlockAndReveal();
        }
        else
        {
            // The progression reward must never be lost just because the reveal
            // component is missing from the scene.
            WingUnlockState.UnlockWings();
            Debug.LogWarning($"[WingGemPickup] '{name}' found no NexusWingAppearance; unlock granted without the reveal.", this);
        }

        Collected?.Invoke(this);

        GemPowerUpEffect.Play(transform.position, powerUpColor);

        if (gemVisual != null)
        {
            gemVisual.enabled = false;
        }

        Collider2D trigger = GetComponent<Collider2D>();
        if (trigger != null) trigger.enabled = false;

        Debug.Log($"[WingGemPickup] '{name}' collected — Nexus's wings are awake.");

        onCollected?.Invoke();
    }
}
