using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Trigger volume that marks the opening level complete for Nexus.
/// Deliberately scene-agnostic: it raises an event and a log line so a later
/// region transition can hook in without this script knowing about any scene.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LevelExit : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string completionMessage = "Opening level complete — the forest path continues.";
    [SerializeField] private bool completeOnlyOnce = true;
    [SerializeField] private UnityEvent onLevelComplete;

    /// <summary>Raised the first time Nexus reaches this exit.</summary>
    public event Action<LevelExit> Completed;

    public bool IsComplete { get; private set; }

    private void Awake()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        if (trigger == null)
        {
            Debug.LogError($"[LevelExit] '{name}' has no Collider2D trigger.", this);
            enabled = false;
            return;
        }

        trigger.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsComplete && completeOnlyOnce) return;
        if (!other.CompareTag(playerTag)) return;

        IsComplete = true;

        Debug.Log($"[LevelExit] {completionMessage}");

        Completed?.Invoke(this);
        onLevelComplete?.Invoke();
    }
}
