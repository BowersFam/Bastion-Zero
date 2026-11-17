using UnityEngine;

/// <summary>
/// Run-scoped progression flag for Nexus's wing unlock.
/// The unlock lives in memory only. <see cref="BeginRun"/> clears it when the
/// opening-level run starts, it holds for the rest of that run, and it is never
/// written to <see cref="PlayerPrefs"/> — so a new run always begins wingless and
/// nothing carries over into another application session.
/// </summary>
public static class WingUnlockState
{
    private static bool isUnlocked;

    /// <summary>True once the wings gem has been collected during the current run.</summary>
    public static bool IsUnlocked => isUnlocked;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        // Domain entry (including every editor play session) always starts locked.
        isUnlocked = false;
    }

    /// <summary>
    /// Clears the unlock for a fresh run. Called once, by the level's run initializer,
    /// before any gameplay component reads the state.
    /// </summary>
    public static void BeginRun()
    {
        isUnlocked = false;
        Debug.Log("[WingUnlockState] Run started — Nexus begins wingless.");
    }

    /// <summary>Grants the wing unlock for the remainder of this run. Safe to call repeatedly.</summary>
    public static void UnlockWings()
    {
        if (isUnlocked) return;

        isUnlocked = true;
        Debug.Log("[WingUnlockState] Wings unlocked for this run.");
    }
}
