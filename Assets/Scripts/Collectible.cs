using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Trigger-based pickup that only Nexus (the "Player"-tagged character) can collect.
/// Fires <see cref="onCollected"/> once and keeps a session-wide count so the pickup
/// can later be extended into Health / Power / Shield / Summon stats.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Collectible : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool destroyOnPickup = true;
    [SerializeField] private UnityEvent onCollected;

    /// <summary>Number of collectibles picked up this session.</summary>
    public static int CollectedCount { get; private set; }

    /// <summary>Raised for every successful pickup, for counters, audio or UI to hook into.</summary>
    public static event Action<Collectible> AnyCollected;

    /// <summary>Raised only for this instance's pickup.</summary>
    public event Action<Collectible> Collected;

    public UnityEvent OnCollectedEvent => onCollected;
    public bool HasBeenCollected { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        CollectedCount = 0;
        AnyCollected = null;
    }

    private void Awake()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        if (trigger == null)
        {
            Debug.LogError($"[Collectible] '{name}' has no Collider2D to act as a pickup trigger.", this);
            enabled = false;
            return;
        }

        trigger.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (HasBeenCollected || other == null) return;
        if (!other.CompareTag(playerTag)) return;

        HasBeenCollected = true;
        CollectedCount++;

        Collected?.Invoke(this);
        onCollected?.Invoke();
        AnyCollected?.Invoke(this);

        Debug.Log($"[Collectible] Picked up '{name}'. Total collected: {CollectedCount}.");

        if (destroyOnPickup) Destroy(gameObject);
        else gameObject.SetActive(false);
    }
}
