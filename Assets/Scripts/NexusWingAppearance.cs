using UnityEngine;

/// <summary>
/// Reveals the hero's wings without ever swapping the controller or the sprite set.
///
/// The wings are a separate <c>Wings</c> layer in <see cref="Nexus.controller"/> that animates
/// <c>VisualRoot/WingRoot</c> only. Collecting the gem raises that layer's weight from 0 to 1, so the
/// body keeps playing the same baked art at the same resolution - a wing unlock can never change how
/// the character looks otherwise. There is no reveal node on the Base Layer: it would have had to
/// play a body clip, and every body clip is the wingless art.
///
/// The layer is created at default weight 0 by
/// <see cref="NexusAnimatorTools"/>, so a hero that has not collected the gem renders no wings at all.
/// </summary>
[RequireComponent(typeof(Animator))]
public class NexusWingAppearance : MonoBehaviour
{
    private const string WingLayerName = "Wings";

    [Header("References")]
    [SerializeField] private Animator animator;
    [Tooltip("SpriteRenderer on VisualRoot - the body. Its flipX is mirrored onto the wings.")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [Tooltip("SpriteRenderer on VisualRoot/WingRoot - the wing layer's only animated target.")]
    [SerializeField] private SpriteRenderer wingRenderer;

    private int wingLayer = -1;
    private bool wingsApplied;

    /// <summary>True when the wing layer is carrying weight.</summary>
    public bool WingsApplied => wingsApplied;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();

        wingLayer = animator != null ? animator.GetLayerIndex(WingLayerName) : -1;

        if (wingLayer < 0)
        {
            Debug.LogError(
                $"[NexusWingAppearance] '{name}' has no '{WingLayerName}' layer in its controller; " +
                "wings cannot be revealed. Re-run Bastion Zero/Art/Build Nexus Wing Layer.", this);
            return;
        }

        animator.SetLayerWeight(wingLayer, 0f);

        if (bodyRenderer == null)
        {
            Transform visual = transform.Find("VisualRoot");
            if (visual != null) bodyRenderer = visual.GetComponent<SpriteRenderer>();
        }

        if (wingRenderer == null)
        {
            Transform wing = transform.Find("VisualRoot/WingRoot");
            if (wing != null) wingRenderer = wing.GetComponent<SpriteRenderer>();
        }
    }

    private void LateUpdate()
    {
        if (wingRenderer == null || bodyRenderer == null) return;

        // The wings are their own SpriteRenderer, so the motor's flipX on the body does not reach them.
        if (wingRenderer.flipX != bodyRenderer.flipX) wingRenderer.flipX = bodyRenderer.flipX;
    }

    private void Start()
    {
        // Apply before the first visible animation update so a returning player never sees a
        // single wingless frame at spawn.
        ApplyStoredUnlock();
    }

    /// <summary>Applies the saved unlock silently (no reveal) if the gem was already collected.</summary>
    public void ApplyStoredUnlock()
    {
        if (!WingUnlockState.IsUnlocked) return;
        ApplyWings();
    }

    /// <summary>
    /// Persists the unlock and brings the wing layer up to full weight.
    /// Called by <see cref="WingGemPickup"/> on the first collection.
    /// </summary>
    public void UnlockAndReveal()
    {
        // Save before any visual change: the persisted flag must survive even if the layer is missing.
        WingUnlockState.UnlockWings();

        ApplyWings();
    }

    private void ApplyWings()
    {
        if (wingsApplied || animator == null || wingLayer < 0) return;

        animator.SetLayerWeight(wingLayer, 1f);
        wingsApplied = true;

        Debug.Log(
            $"[NexusWingAppearance] '{name}': wing layer {wingLayer} weight set to " +
            $"{animator.GetLayerWeight(wingLayer):0.##} - body art unchanged.",
            this);
    }
}
