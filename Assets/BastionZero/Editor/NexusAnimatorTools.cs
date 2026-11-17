#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Builds the wing half of the Nexus animation set: three 8-frame sprite clips bound to
/// <see cref="WingBindingPath"/>, and the <c>Wings</c> layer that plays them, plus the two
/// cycle-speed parameters the hero driver writes. The layer ships at weight 0 so the wings stay
/// invisible until <c>NexusWingAppearance</c> raises the weight.
///
/// Every transition on both layers is forced to zero duration: cross-dissolving two sprites
/// ghosts the silhouette, which is exactly the stutter the reference pass set out to remove.
///
/// Idempotent - safe to re-run from the menu after regenerating wing art.
/// </summary>
public static class NexusAnimatorTools
{
    public const string ControllerPath = "Assets/Animations/Nexus.controller";
    public const string WingClipFolder = "Assets/Animations/Wing";
    public const string WingBindingPath = "VisualRoot/WingRoot";
    public const string WingLayerName = "Wings";

    public const string SpeedParam = "Speed";
    public const string GlidingParam = "Gliding";
    public const string WalkCycleParam = "WalkCycleSpeed";
    public const string RunCycleParam = "RunCycleSpeed";
    public const string WingFlapRateParam = "WingFlapRate";

    private const int SampleRate = 12;
    private const int WingFrames = 8;

    private static readonly string[] WingClipNames =
    {
        "Nexus_Wing_Idle",
        "Nexus_Wing_Flap",
        "Nexus_Wing_Glide",
    };

    private static readonly string[] WingStateNames =
    {
        "WingIdle",
        "WingFlap",
        "WingGlide",
    };

    /// <summary>One animator condition, so a transition can be declared in a single call.</summary>
    private readonly struct Condition
    {
        public readonly AnimatorConditionMode Mode;
        public readonly float Threshold;
        public readonly string Parameter;

        public Condition(AnimatorConditionMode mode, string parameter, float threshold = 0f)
        {
            Mode = mode;
            Parameter = parameter;
            Threshold = threshold;
        }
    }

    [MenuItem("Bastion Zero/Art/Build Nexus Wing Layer")]
    public static void BuildWingLayerMenu() => BuildWingLayer();

    /// <summary>Creates the AN tuning asset if it is missing. Idempotent.</summary>
    [MenuItem("Bastion Zero/Tuning/Create Player Animation Tuning")]
    public static void CreateAnimationTuningMenu() => CreateAnimationTuning();

    public static bool CreateAnimationTuning()
    {
        const string path = "Assets/BastionZero/Tuning/PlayerAnimationTuning.asset";

        if (AssetDatabase.LoadAssetAtPath<BastionZero.Animation.PlayerAnimationTuning>(path) != null)
        {
            Debug.Log($"[NexusAnimatorTools] {path} already exists.");
            return true;
        }

        var asset = ScriptableObject.CreateInstance<BastionZero.Animation.PlayerAnimationTuning>();
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        Debug.Log($"[NexusAnimatorTools] created {path}.");
        return true;
    }

    /// <summary>Creates the wing clips, wires the Wings layer and silences every cross-fade.</summary>
    public static int BuildWingLayer()
    {
        AnimationClip[] clips = BuildWingClips();
        if (clips == null) return 0;

        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            Debug.LogError($"[NexusAnimatorTools] missing controller {ControllerPath}.");
            return 0;
        }

        EnsureParameter(controller, WalkCycleParam);
        EnsureParameter(controller, RunCycleParam);
        EnsureParameter(controller, WingFlapRateParam);

        AnimatorControllerLayer[] layers = controller.layers;

        if (layers.Length > 0)
        {
            if (layers[0].defaultWeight <= 0f)
            {
                layers[0].defaultWeight = 1f;
                Debug.Log("[NexusAnimatorTools] Base Layer default weight was 0; set to 1.");
            }

            ZeroDurations(layers[0].stateMachine);
            DriveCycleRate(layers[0].stateMachine, "Walk", WalkCycleParam);
            DriveCycleRate(layers[0].stateMachine, "Run", RunCycleParam);
        }

        int wingIndex = System.Array.FindIndex(layers, layer => layer.name == WingLayerName);
        if (wingIndex < 0)
        {
            var stateMachine = new AnimatorStateMachine
            {
                name = WingLayerName,
                hideFlags = HideFlags.HideInHierarchy,
            };

            AssetDatabase.AddObjectToAsset(stateMachine, controller);

            var wingLayer = new AnimatorControllerLayer
            {
                name = WingLayerName,
                stateMachine = stateMachine,
                defaultWeight = 0f,
                blendingMode = AnimatorLayerBlendingMode.Override,
            };

            var grown = new AnimatorControllerLayer[layers.Length + 1];
            System.Array.Copy(layers, grown, layers.Length);
            grown[layers.Length] = wingLayer;
            layers = grown;
            wingIndex = layers.Length - 1;
        }

        layers[wingIndex].defaultWeight = 0f;
        layers[wingIndex].blendingMode = AnimatorLayerBlendingMode.Override;

        AnimatorStateMachine wings = layers[wingIndex].stateMachine;
        AnimatorState idle = FindOrAddState(wings, WingStateNames[0], new Vector3(260, 0, 0), clips[0]);
        AnimatorState flap = FindOrAddState(wings, WingStateNames[1], new Vector3(260, 90, 0), clips[1]);
        AnimatorState glide = FindOrAddState(wings, WingStateNames[2], new Vector3(260, 180, 0), clips[2]);

        wings.defaultState = idle;

        // The flap cycle is rate-scaled per frame by HeroAnimationDriver so the beat tracks speed.
        flap.speedParameterActive = true;
        flap.speedParameter = WingFlapRateParam;

        Wire(wings, idle, flap, new[] { new Condition(AnimatorConditionMode.Greater, SpeedParam, 0.15f) });
        Wire(wings, flap, idle, new[] { new Condition(AnimatorConditionMode.Less, SpeedParam, 0.1f) });

        Wire(wings, idle, glide, new[] { new Condition(AnimatorConditionMode.If, GlidingParam) });
        Wire(wings, flap, glide, new[] { new Condition(AnimatorConditionMode.If, GlidingParam) });

        Wire(wings, glide, flap, new[]
        {
            new Condition(AnimatorConditionMode.IfNot, GlidingParam),
            new Condition(AnimatorConditionMode.Greater, SpeedParam, 0.1f),
        });

        Wire(wings, glide, idle, new[]
        {
            new Condition(AnimatorConditionMode.IfNot, GlidingParam),
            new Condition(AnimatorConditionMode.Less, SpeedParam, 0.1f),
        });

        controller.layers = layers;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"[NexusAnimatorTools] Wings layer built: {WingStateNames.Length} states, layer index {wingIndex}, " +
            $"default weight 0, all cross-fades 0 s. Controller now has {controller.layers.Length} layers " +
            $"and {controller.parameters.Length} parameters.");

        return WingStateNames.Length;
    }

    // ── Combo attack states (SYS3 §3.1, CF-A1–A3) ───────────────────────────────────

    public const string AttackChainParam = "AttackChain";
    public const string AttackClipFolder = "Assets/Animations/Unwinged";

    private static readonly string[] AttackClipNames =
    {
        "Nexus_Attack_G1",
        "Nexus_Attack_G2",
        "Nexus_Attack_G3",
    };

    /// <summary>Animator state names are the combat spec's own state names (SYS3 §11).</summary>
    private static readonly string[] AttackStateNames = { "G1", "G2", "G3" };

    /// <summary>States lifted out: the single-attack node and the two that play superseded 64 px art.</summary>
    private static readonly string[] LegacyStateNames = { "Attack", "WingReveal", "Glide" };

    /// <summary>Every Base Layer state from which a ground attack may begin.</summary>
    private static readonly string[] AttackEntryStates = { "Idle", "Walk", "Run", "Land" };

    /// <summary>
    /// Replaces the single <c>Attack</c> node with <c>G1</c>/<c>G2</c>/<c>G3</c> and removes the
    /// <c>WingReveal</c> and <c>Glide</c> nodes, which still play the 64 px terminal-guardians clips
    /// from before the wing layer existed. The chain step is selected by the <c>AttackChain</c> int,
    /// gated on the same <c>Attack</c> trigger, so a finished chain can never re-enter on a stale
    /// index. Idempotent.
    /// </summary>
    [MenuItem("Bastion Zero/Art/Build Nexus Combo States")]
    public static int BuildAttackStatesMenu() => BuildAttackStates();

    public static int BuildAttackStates()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            Debug.LogError($"[NexusAnimatorTools] missing controller {ControllerPath}.");
            return 0;
        }

        var clips = new AnimationClip[AttackClipNames.Length];
        for (int i = 0; i < AttackClipNames.Length; i++)
        {
            string clipPath = $"{AttackClipFolder}/{AttackClipNames[i]}.anim";
            clips[i] = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);

            if (clips[i] == null)
            {
                Debug.LogError(
                    $"[NexusAnimatorTools] missing clip {clipPath}; run Bastion Zero/Art/Rebuild Body Clips first.");
                return 0;
            }
        }

        EnsureIntParameter(controller, AttackChainParam);

        AnimatorControllerLayer[] layers = controller.layers;
        if (layers.Length == 0)
        {
            Debug.LogError("[NexusAnimatorTools] controller has no layers.");
            return 0;
        }

        AnimatorStateMachine machine = layers[0].stateMachine;

        int removed = 0;
        foreach (string legacy in LegacyStateNames)
        {
            if (RemoveStateWithEdges(machine, legacy)) removed++;
        }

        RemoveParameter(controller, "WingsGranted");

        var states = new AnimatorState[AttackStateNames.Length];
        for (int i = 0; i < AttackStateNames.Length; i++)
        {
            states[i] = FindOrAddState(machine, AttackStateNames[i], new Vector3(760, 60 - i * 90, 0), clips[i]);
        }

        // Entry: the trigger picks the swing, so the state matches the chain index HeroCombat set.
        for (int step = 0; step < AttackStateNames.Length; step++)
        {
            var conditions = new[]
            {
                new Condition(AnimatorConditionMode.If, "Attack"),
                new Condition(AnimatorConditionMode.Equals, AttackChainParam, step + 1),
            };

            foreach (string entry in AttackEntryStates)
            {
                AnimatorState from = FindState(machine, entry);
                if (from != null) Wire(machine, from, states[step], conditions);
            }
        }

        // Chain: the logic only fires the trigger on the frame the cancel window opens, so no exit
        // time — the next swing starts exactly when the previous one becomes cancellable.
        Wire(machine, states[0], states[1], new[]
        {
            new Condition(AnimatorConditionMode.If, "Attack"),
            new Condition(AnimatorConditionMode.Equals, AttackChainParam, 2),
        });

        Wire(machine, states[1], states[2], new[]
        {
            new Condition(AnimatorConditionMode.If, "Attack"),
            new Condition(AnimatorConditionMode.Equals, AttackChainParam, 3),
        });

        // Exit: the logic ends the attack on the clip's last frame, so let the clip finish. A 0.9
        // exit time would cut the settle pose short and pop the body back to Idle mid-recovery.
        AnimatorState idle = FindState(machine, "Idle");
        AnimatorState walk = FindState(machine, "Walk");
        AnimatorState run = FindState(machine, "Run");

        foreach (AnimatorState attack in states)
        {
            if (idle != null)
                WireExit(machine, attack, idle, new[] { new Condition(AnimatorConditionMode.Less, SpeedParam, 0.1f) });

            if (walk != null)
                WireExit(machine, attack, walk, new[]
                {
                    new Condition(AnimatorConditionMode.Greater, SpeedParam, 0.1f),
                    new Condition(AnimatorConditionMode.Less, SpeedParam, 3.5f),
                });

            if (run != null)
                WireExit(machine, attack, run, new[] { new Condition(AnimatorConditionMode.Greater, SpeedParam, 3.5f) });
        }

        ZeroDurations(machine);

        controller.layers = layers;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"[NexusAnimatorTools] combo states built: {AttackStateNames.Length} states " +
            $"({string.Join(", ", AttackStateNames)}), {removed} legacy state(s) removed, " +
            $"parameter '{AttackChainParam}' ensured, every transition 0 s. " +
            $"Controller now has {machine.states.Length} Base Layer states and " +
            $"{controller.parameters.Length} parameters.");

        return AttackStateNames.Length;
    }

    // ── Clips ───────────────────────────────────────────────────────────────────────

    private static AnimationClip[] BuildWingClips()
    {
        EnsureFolder("Assets/Animations", "Wing");

        var clips = new AnimationClip[WingClipNames.Length];

        for (int c = 0; c < WingClipNames.Length; c++)
        {
            string state = WingStateNames[c].Substring(4).ToLowerInvariant();
            string clipPath = $"{WingClipFolder}/{WingClipNames[c]}.anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);

            if (clip == null)
            {
                clip = new AnimationClip { name = WingClipNames[c] };
                AssetDatabase.CreateAsset(clip, clipPath);
            }

            var keys = new ObjectReferenceKeyframe[WingFrames];
            bool complete = true;

            for (int i = 0; i < WingFrames; i++)
            {
                string spritePath = $"Assets/Art/Characters/Nexus/HD2D/wings/{state}/{i:00}.png";
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

                if (sprite == null)
                {
                    Debug.LogError($"[NexusAnimatorTools] {WingClipNames[c]}: missing sprite {spritePath}.");
                    complete = false;
                    break;
                }

                keys[i] = new ObjectReferenceKeyframe { time = i / (float)SampleRate, value = sprite };
            }

            if (!complete) return null;

            var binding = new EditorCurveBinding
            {
                path = WingBindingPath,
                type = typeof(SpriteRenderer),
                propertyName = "m_Sprite",
            };

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            clip.frameRate = SampleRate;

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.stopTime = WingFrames / (float)SampleRate;
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            EditorUtility.SetDirty(clip);
            clips[c] = clip;

            Debug.Log($"[NexusAnimatorTools] {WingClipNames[c]}: {WingFrames} keys on the 1/{SampleRate} grid, length {settings.stopTime:0.####}s, loop.");
        }

        AssetDatabase.SaveAssets();
        return clips;
    }

    // ── Wiring helpers ──────────────────────────────────────────────────────────────

    private static void EnsureParameter(AnimatorController controller, string name)
    {
        foreach (AnimatorControllerParameter parameter in controller.parameters)
        {
            if (parameter.name == name) return;
        }

        controller.AddParameter(name, AnimatorControllerParameterType.Float);
        Debug.Log($"[NexusAnimatorTools] added float parameter '{name}'.");
    }

    private static void EnsureIntParameter(AnimatorController controller, string name)
    {
        foreach (AnimatorControllerParameter parameter in controller.parameters)
        {
            if (parameter.name == name) return;
        }

        controller.AddParameter(name, AnimatorControllerParameterType.Int);
        Debug.Log($"[NexusAnimatorTools] added int parameter '{name}'.");
    }

    private static void RemoveParameter(AnimatorController controller, string name)
    {
        foreach (AnimatorControllerParameter parameter in controller.parameters)
        {
            if (parameter.name != name) continue;

            controller.RemoveParameter(parameter);
            Debug.Log($"[NexusAnimatorTools] removed parameter '{name}'.");
            return;
        }
    }

    private static AnimatorState FindState(AnimatorStateMachine machine, string name)
    {
        foreach (ChildAnimatorState child in machine.states)
        {
            if (child.state.name == name) return child.state;
        }

        return null;
    }

    /// <summary>
    /// Deletes a state together with every edge that touches it — outgoing, incoming and the
    /// AnyState edges. A leftover AnyState edge would keep pointing at a destroyed state.
    /// </summary>
    private static bool RemoveStateWithEdges(AnimatorStateMachine machine, string name)
    {
        AnimatorState target = FindState(machine, name);
        if (target == null) return false;

        foreach (AnimatorStateTransition transition in target.transitions) target.RemoveTransition(transition);

        foreach (ChildAnimatorState child in machine.states)
        {
            AnimatorStateTransition[] transitions = child.state.transitions;
            for (int i = transitions.Length - 1; i >= 0; i--)
            {
                if (transitions[i].destinationState == target) child.state.RemoveTransition(transitions[i]);
            }
        }

        AnimatorStateTransition[] anyState = machine.anyStateTransitions;
        for (int i = anyState.Length - 1; i >= 0; i--)
        {
            if (anyState[i].destinationState == target) machine.RemoveAnyStateTransition(anyState[i]);
        }

        machine.RemoveState(target);
        Debug.Log($"[NexusAnimatorTools] state '{name}' removed with its edges.");
        return true;
    }

    /// <summary>
    /// Wires a transition that waits for the clip to finish (exit time 1) before taking its
    /// conditions, so a one-shot attack can never be cut off mid-swing.
    /// </summary>
    private static void WireExit(AnimatorStateMachine machine, AnimatorState from, AnimatorState to, Condition[] conditions)
    {
        for (int i = from.transitions.Length - 1; i >= 0; i--)
        {
            if (from.transitions[i].destinationState != to) continue;
            from.RemoveTransition(from.transitions[i]);
        }

        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = true;
        transition.exitTime = 1f;
        transition.hasFixedDuration = true;
        transition.duration = 0f;

        foreach (Condition condition in conditions)
        {
            transition.AddCondition(condition.Mode, condition.Threshold, condition.Parameter);
        }

        EditorUtility.SetDirty(machine);
    }

    private static void ZeroDurations(AnimatorStateMachine machine)
    {
        int touched = 0;

        foreach (ChildAnimatorState child in machine.states)
        {
            foreach (AnimatorStateTransition transition in child.state.transitions)
            {
                transition.hasFixedDuration = true;
                transition.duration = 0f;
                touched++;
            }
        }

        foreach (AnimatorStateTransition transition in machine.anyStateTransitions)
        {
            transition.hasFixedDuration = true;
            transition.duration = 0f;
            touched++;
        }

        foreach (ChildAnimatorStateMachine sub in machine.stateMachines)
        {
            ZeroDurations(sub.stateMachine);
        }

        if (touched > 0) Debug.Log($"[NexusAnimatorTools] {machine.name}: {touched} transition(s) set to 0 s.");
    }

    /// <summary>
    /// Points a locomotion state's playback speed at a float parameter, so the frame rate of the
    /// cycle follows the hero's real ground speed instead of sliding its feet.
    /// </summary>
    private static void DriveCycleRate(AnimatorStateMachine machine, string stateName, string parameter)
    {
        foreach (ChildAnimatorState child in machine.states)
        {
            if (child.state.name != stateName) continue;

            child.state.speedParameterActive = true;
            child.state.speedParameter = parameter;
            Debug.Log($"[NexusAnimatorTools] state '{stateName}' playback rate driven by '{parameter}'.");
            return;
        }

        Debug.LogWarning($"[NexusAnimatorTools] state '{stateName}' not found in '{machine.name}'.");
    }

    private static AnimatorState FindOrAddState(AnimatorStateMachine machine, string name, Vector3 position, Motion motion)
    {
        foreach (ChildAnimatorState child in machine.states)
        {
            if (child.state.name != name) continue;

            if (child.state.motion != motion) child.state.motion = motion;
            return child.state;
        }

        AnimatorState state = machine.AddState(name, position);
        state.motion = motion;
        return state;
    }

    private static void Wire(AnimatorStateMachine machine, AnimatorState from, AnimatorState to, Condition[] conditions)
    {
        for (int i = from.transitions.Length - 1; i >= 0; i--)
        {
            if (from.transitions[i].destinationState != to) continue;
            from.RemoveTransition(from.transitions[i]);
        }

        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.hasFixedDuration = true;
        transition.duration = 0f;
        transition.exitTime = 1f;

        foreach (Condition condition in conditions)
        {
            transition.AddCondition(condition.Mode, condition.Threshold, condition.Parameter);
        }

        EditorUtility.SetDirty(machine);
    }

    private static void EnsureFolder(string parent, string leaf)
    {
        string path = $"{parent}/{leaf}";
        if (AssetDatabase.IsValidFolder(path)) return;

        AssetDatabase.CreateFolder(parent, leaf);
    }
}
#endif
