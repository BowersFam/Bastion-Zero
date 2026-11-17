using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Trigger receiver for Nexus's attack. The collider stays disabled outside the
/// slash's active window (driven by <see cref="NexusAttack"/>), so walking through
/// an attackable target never counts as a strike.
/// Dispatch is deduplicated per swing so a target can only be hit once per slash.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AttackHitbox2D : MonoBehaviour
{
    [Tooltip("Owner tag to ignore so Nexus never hits itself.")]
    [SerializeField] private string ownerTag = "Player";

    private readonly HashSet<IAttackReceiver> hitThisSwing = new HashSet<IAttackReceiver>();
    private Collider2D hitCollider;

    /// <summary>Raised once per swing when a target is accepted. Arg is the contact point.</summary>
    public event Action<Vector3> Hit;

    private void Awake()
    {
        hitCollider = GetComponent<Collider2D>();
        if (hitCollider == null)
        {
            Debug.LogError($"[AttackHitbox2D] '{name}' has no Collider2D to act as the slash volume.", this);
            enabled = false;
            return;
        }

        hitCollider.isTrigger = true;
        // The window is owned by NexusAttack; nothing is live until it opens one.
        hitCollider.enabled = false;
    }

    /// <summary>Opens or closes the slash volume. Opening a window clears the hit set.</summary>
    public void SetActive(bool active)
    {
        if (hitCollider == null) hitCollider = GetComponent<Collider2D>();
        if (hitCollider == null) return;

        if (active) hitThisSwing.Clear();

        // Enabling a collider alone only fires OnTriggerEnter2D on the next physics
        // step, which can miss a target already overlapping at swing start. Sweep
        // the authored box immediately so the first frame of the window connects.
        if (active && hitCollider is BoxCollider2D box)
        {
            Vector2 center = box.transform.TransformPoint(box.offset);
            Vector3 scale = box.transform.lossyScale;
            Vector2 size = new Vector2(box.size.x * Mathf.Abs(scale.x), box.size.y * Mathf.Abs(scale.y));
            float angle = box.transform.eulerAngles.z;

            Collider2D[] overlaps = Physics2D.OverlapBoxAll(center, size, angle);
            for (int i = 0; i < overlaps.Length; i++) Dispatch(overlaps[i]);
        }

        hitCollider.enabled = active;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Dispatch(other);
    }

    private void Dispatch(Collider2D other)
    {
        if (other == null) return;
        if (!string.IsNullOrEmpty(ownerTag) && other.CompareTag(ownerTag)) return;

        IAttackReceiver receiver = other.GetComponentInParent<IAttackReceiver>();
        if (receiver == null) return;
        if (!hitThisSwing.Add(receiver)) return;

        Hit?.Invoke(other.bounds.ClosestPoint(hitCollider != null ? hitCollider.bounds.center : transform.position));

        receiver.ReceiveAttack();
    }
}
