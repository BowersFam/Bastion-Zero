using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// The required attack-cleared hazard barrier. Its blocking collider stays solid
/// until a live slash hits it, so normal movement can never bypass the attack.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AttackableBramble : MonoBehaviour, IAttackReceiver
{
    [Header("References")]
    [Tooltip("The collider that blocks the route. Disabled on the first accepted hit.")]
    [SerializeField] private Collider2D blockingCollider;
    [Tooltip("Optional contact/damage-looking collider, disabled alongside the blocker.")]
    [SerializeField] private Collider2D contactCollider;
    [SerializeField] private SpriteRenderer[] visuals;

    [Header("Clear Effect")]
    [SerializeField] private float clearDuration = 0.28f;
    [SerializeField] private Color clearedTint = new Color(0.7f, 0.25f, 0.35f, 1f);

    [Header("Events")]
    [SerializeField] private UnityEvent onCleared;

    private float clearTimer;
    private bool isClearing;

    /// <summary>True once a slash has cleared the barrier.</summary>
    public bool IsCleared { get; private set; }

    /// <summary>Raised once, on the slash that clears the barrier.</summary>
    public event Action Cleared;

    private void Awake()
    {
        if (blockingCollider == null) blockingCollider = GetComponent<Collider2D>();

        if (blockingCollider == null)
        {
            Debug.LogError($"[AttackableBramble] '{name}' has no blocking collider assigned; the route is not blocked.", this);
            enabled = false;
            return;
        }

        blockingCollider.isTrigger = false;

        if (visuals == null || visuals.Length == 0) visuals = GetComponentsInChildren<SpriteRenderer>(true);
    }

    /// <summary>Accepts a slash. Repeat hits after the first are ignored.</summary>
    public void ReceiveAttack()
    {
        if (IsCleared) return;

        IsCleared = true;
        isClearing = true;
        clearTimer = clearDuration;

        if (blockingCollider != null) blockingCollider.enabled = false;
        if (contactCollider != null) contactCollider.enabled = false;

        Debug.Log($"[AttackableBramble] '{name}' cleared by a slash.");

        onCleared?.Invoke();
        Cleared?.Invoke();
    }

    private void Update()
    {
        if (!isClearing) return;

        clearTimer -= Time.deltaTime;
        float t = clearDuration > 0f ? Mathf.Clamp01(clearTimer / clearDuration) : 0f;

        if (visuals != null)
        {
            for (int i = 0; i < visuals.Length; i++)
            {
                SpriteRenderer sr = visuals[i];
                if (sr == null) continue;

                Color c = clearedTint;
                c.a = t;
                sr.color = c;
                sr.enabled = t > 0f;
            }
        }

        if (clearTimer <= 0f) isClearing = false;
    }
}
