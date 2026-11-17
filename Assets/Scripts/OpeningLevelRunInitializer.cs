using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Owns the opening-level run boundary. Placed once in <c>OpeningLevel</c>, its
/// <see cref="Awake"/> clears the run-scoped wing unlock before any gameplay
/// component reads it, so every run starts wingless regardless of what the previous
/// run or a previous application session left behind.
/// </summary>
[DisallowMultipleComponent]
public class OpeningLevelRunInitializer : MonoBehaviour
{
    private static bool hasInitializedScene;
    private static SceneHandle initializedSceneHandle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        hasInitializedScene = false;
        initializedSceneHandle = default;
    }

    private void Awake()
    {
        Scene scene = gameObject.scene;

        // Guard against a second initializer in the same scene resetting the unlock
        // later in the run; the state is per scene load, not per component instance.
        if (hasInitializedScene && initializedSceneHandle == scene.handle) return;

        hasInitializedScene = true;
        initializedSceneHandle = scene.handle;
        WingUnlockState.BeginRun();
    }
}
