#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using BastionZero.Damage;
using BastionZero.DebugTools;
using BastionZero.Movement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Step 2 (SYS2) project tooling: physics layers, the damage tuning asset, the hero's hurtbox
/// and health wiring, and the DM_Gym hazard gym. Every action is idempotent, so it can be re-run.
/// Menu: Bastion Zero/SYS2.
/// </summary>
public static class Sys2Tools
{
    private const string TuningFolder = "Assets/BastionZero/Tuning";
    private const string TuningPath = TuningFolder + "/DamageTuning.asset";
    private const string MovementTuningPath = TuningFolder + "/PlayerMovementTuning.asset";
    private const string HeroPrefabPath = "Assets/BastionZero/Prefabs/Hero.prefab";
    private const string SceneFolder = "Assets/BastionZero/Scenes";
    private const string GymScenePath = SceneFolder + "/DM_Gym.unity";
    private const string OpeningLevelPath = "Assets/Scenes/OpeningLevel.unity";
    private const string HurtboxName = "Hurtbox";

    private const string HeroHurtboxLayerName = "HeroHurtbox";
    private const string HazardLayerName = "Hazard";

    // ── Entry points ────────────────────────────────────────────────────────────────

    /// <summary>Layers, tuning asset and the hero's hurtbox/health wiring, in one pass.</summary>
    [MenuItem("Bastion Zero/SYS2/1 · Create Damage Tuning and Wire Hero")]
    public static void Configure()
    {
        EnsureFolder(TuningFolder);

        int hurtboxLayer = EnsureLayer(HeroHurtboxLayerName);
        int hazardLayer = EnsureLayer(HazardLayerName);
        if (hurtboxLayer < 0 || hazardLayer < 0) return;

        DamageTuning tuning = CreateOrLoadTuning();

        // The motor's casts must never treat a trigger volume — or the hero's own hurtbox — as
        // solid geometry, and safe-ground recording must know which layer means "hazard".
        ConfigureMovementTuning(hazardLayer, hurtboxLayer);

        // OpeningLevel's Nexus is a plain scene object, so it is wired directly.
        EditorSceneManager.SaveOpenScenes();
        if (EditorSceneManager.GetActiveScene().path != OpeningLevelPath)
        {
            EditorSceneManager.OpenScene(OpeningLevelPath, OpenSceneMode.Single);
        }

        foreach (PlayerMovement motor in Object.FindObjectsByType<PlayerMovement>(FindObjectsInactive.Include))
        {
            if (PrefabUtility.IsPartOfPrefabInstance(motor.gameObject)) continue;
            AttachHeroComponents(motor.gameObject, tuning, hurtboxLayer, hazardLayer);
        }

        EditorSceneManager.SaveOpenScenes();

        // The prefab carries the same components into MV_Gym and DM_Gym.
        if (System.IO.File.Exists(HeroPrefabPath))
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(HeroPrefabPath);
            try
            {
                AttachHeroComponents(contents, tuning, hurtboxLayer, hazardLayer);
                PrefabUtility.SaveAsPrefabAsset(contents, HeroPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Re-saving the prefab above can re-dirty a scene that holds an instance of it, so the
        // scene save has to come last or the wired references never reach disk.
        EditorSceneManager.SaveOpenScenes();

        Debug.Log($"[Sys2Tools] Damage tuning at {TuningPath}; hero hurtbox and health wired. " +
                  $"Layers: {HeroHurtboxLayerName}={hurtboxLayer}, {HazardLayerName}={hazardLayer}.");
    }

    /// <summary>Builds Assets/BastionZero/Scenes/DM_Gym.unity and opens it.</summary>
    [MenuItem("Bastion Zero/SYS2/2 · Build DM_Gym Scene")]
    public static void BuildGym()
    {
        EnsureFolder(SceneFolder);
        EditorSceneManager.SaveOpenScenes();

        GameObject heroPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HeroPrefabPath);
        DamageTuning tuning = AssetDatabase.LoadAssetAtPath<DamageTuning>(TuningPath);

        if (heroPrefab == null || tuning == null)
        {
            Debug.LogError("[Sys2Tools] Run 'Bastion Zero/SYS2/1 · Create Damage Tuning and Wire Hero' first.");
            return;
        }

        Scene gym = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        BuildGymContents(heroPrefab, tuning);

        EditorSceneManager.SaveScene(gym, GymScenePath);
        RegisterInBuildSettings(GymScenePath);
        AssetDatabase.Refresh();

        // The first save of a brand-new scene can drop asset references, so they are re-applied
        // against the saved scene (and only then does the write stick).
        RewireDamageTuning(tuning);
        EditorSceneManager.SaveScene(gym, GymScenePath);
        AssetDatabase.SaveAssets();

        Debug.Log($"[Sys2Tools] DM_Gym built at {GymScenePath}.");
    }

    private static void RewireDamageTuning(DamageTuning tuning)
    {
        foreach (Hazard2D hazard in Object.FindObjectsByType<Hazard2D>(FindObjectsInactive.Include))
            SetObject(hazard, "tuning", tuning);

        foreach (PitVolume2D pit in Object.FindObjectsByType<PitVolume2D>(FindObjectsInactive.Include))
            SetObject(pit, "tuning", tuning);

        foreach (ContactDamager2D contact in Object.FindObjectsByType<ContactDamager2D>(FindObjectsInactive.Include))
            SetObject(contact, "tuning", tuning);
    }

    // ── Tuning, layers and wiring ───────────────────────────────────────────────────

    private static DamageTuning CreateOrLoadTuning()
    {
        DamageTuning tuning = AssetDatabase.LoadAssetAtPath<DamageTuning>(TuningPath);
        if (tuning != null) return tuning;

        tuning = ScriptableObject.CreateInstance<DamageTuning>();
        AssetDatabase.CreateAsset(tuning, TuningPath);

        // Import synchronously: a reference to an asset that has not been imported yet is
        // silently written as null when the prefab is saved later in this same call.
        AssetDatabase.ImportAsset(TuningPath, ImportAssetOptions.ForceSynchronousImport);
        EditorUtility.SetDirty(tuning);

        return tuning;
    }

    private static void ConfigureMovementTuning(int hazardLayer, int hurtboxLayer)
    {
        PlayerMovementTuning movement = AssetDatabase.LoadAssetAtPath<PlayerMovementTuning>(MovementTuningPath);
        if (movement == null)
        {
            Debug.LogWarning("[Sys2Tools] PlayerMovementTuning not found; run the SYS1 tools first.");
            return;
        }

        var serialized = new SerializedObject(movement);

        SerializedProperty solid = serialized.FindProperty("solidLayers");
        if (solid != null)
        {
            int mask = ~0;
            mask &= ~(1 << hazardLayer);
            mask &= ~(1 << hurtboxLayer);
            solid.intValue = mask;
        }

        SerializedProperty hazards = serialized.FindProperty("hazardLayers");
        if (hazards != null) hazards.intValue = 1 << hazardLayer;

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(movement);
    }

    private static void AttachHeroComponents(GameObject hero, DamageTuning tuning, int hurtboxLayer, int hazardLayer)
    {
        var capsule = hero.GetComponent<CapsuleCollider2D>();
        if (capsule == null)
        {
            Debug.LogError($"[Sys2Tools] {hero.name} has no CapsuleCollider2D; SYS2 wiring skipped.", hero);
            return;
        }

        Transform visual = hero.transform.Find("VisualRoot");
        SpriteRenderer sprite = visual != null ? visual.GetComponent<SpriteRenderer>() : hero.GetComponentInChildren<SpriteRenderer>(true);

        Transform hurtboxTransform = hero.transform.Find(HurtboxName);
        if (hurtboxTransform == null)
        {
            var hurtboxObject = new GameObject(HurtboxName);
            hurtboxTransform = hurtboxObject.transform;
            hurtboxTransform.SetParent(hero.transform, false);
        }

        hurtboxTransform.localPosition = Vector3.zero;
        hurtboxTransform.localRotation = Quaternion.identity;
        hurtboxTransform.localScale = Vector3.one;
        hurtboxTransform.gameObject.layer = hurtboxLayer;

        BoxCollider2D hurtbox = hurtboxTransform.GetComponent<BoxCollider2D>();
        if (hurtbox == null) hurtbox = hurtboxTransform.gameObject.AddComponent<BoxCollider2D>();
        hurtbox.isTrigger = true;

        HeroBodySetup body = GetOrAdd<HeroBodySetup>(hero);
        HeroHealth health = GetOrAdd<HeroHealth>(hero);
        DamageStateReadout readout = GetOrAdd<DamageStateReadout>(hero);
        SafeGroundRecorder recorder = hero.GetComponent<SafeGroundRecorder>();

        SetObject(body, "tuning", tuning);
        SetObject(body, "collisionBox", capsule);
        SetObject(body, "hurtbox", hurtbox);

        SetObject(health, "tuning", tuning);
        SetObject(health, "movement", hero.GetComponent<PlayerMovement>());
        SetObject(health, "safeGround", recorder);
        SetObject(health, "body", body);
        SetObject(health, "spriteRenderer", sprite);

        SetObject(readout, "health", health);
        SetObject(readout, "safeGround", recorder);

        if (recorder != null) SetInt(recorder, "hazardLayers", 1 << hazardLayer);
    }

    private static T GetOrAdd<T>(GameObject host) where T : Component
    {
        T component = host.GetComponent<T>();
        if (component == null) component = host.AddComponent<T>();
        return component;
    }

    // ── DM_Gym ──────────────────────────────────────────────────────────────────────

    private static void BuildGymContents(GameObject heroPrefab, DamageTuning tuning)
    {
        var root = new GameObject("DM_Gym").transform;

        // Floor with a 6-tile pit at x 2…8, matching MV-D2's widest base-kit gap.
        Solid(root, "Floor_Left", new Vector2(-5.5f, -0.5f), new Vector2(15f, 1f), Grey());
        Solid(root, "Floor_Right", new Vector2(10.5f, -0.5f), new Vector2(5f, 1f), Grey());

        // Left wall, so a wall-spike hit can be read without leaving the floor.
        Solid(root, "Wall_Left", new Vector2(-13.5f, 4f), new Vector2(1f, 9f), Brown());

        // D-Hazard: a 3-tile floor spike strip at x −4…−1.
        ConfigureHazard(
            TriggerBox(root, "Spikes_Floor", new Vector2(-2.5f, 0.2f), new Vector2(3f, 0.4f), Red()),
            tuning, HazardSurface.Floor);

        // Wall spikes on the hero's left: the hit must push right.
        ConfigureHazard(
            TriggerBox(root, "Spikes_Wall", new Vector2(-12.8f, 2f), new Vector2(0.4f, 2f), Red()),
            tuning, HazardSurface.LeftWall);

        // D-Pit below the gap, and a kill plane under everything (SYS2 §9 trigger table).
        ConfigurePit(root, "PitVolume", new Vector2(5f, -2.5f), new Vector2(6f, 5f), tuning);
        ConfigurePit(root, "KillPlane", new Vector2(0f, -12f), new Vector2(44f, 2f), tuning);

        // D-Light contact body: stands on the right floor, no solid collider (SYS2 §1 rule 4).
        GameObject dummy = TriggerBox(root, "ContactDummy", new Vector2(11f, 1f), new Vector2(1f, 2f), Orange());
        var contact = dummy.AddComponent<ContactDamager2D>();
        SetObject(contact, "tuning", tuning);
        SetInt(contact, "hurtboxLayers", 1 << LayerMask.NameToLayer(HeroHurtboxLayerName));

        // Hero.
        GameObject hero = (GameObject)PrefabUtility.InstantiatePrefab(heroPrefab);
        hero.name = "Hero";
        hero.transform.position = new Vector3(-9f, 0.91f, 0f);

        // Camera: same framing as the SYS1 gym, so the two scenes read alike.
        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(-2f, 1.66f, -10f);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 8.5f;
        camera.backgroundColor = new Color(0.08f, 0.09f, 0.11f, 1f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        cameraObject.AddComponent<AudioListener>();

        GymCameraFollow follow = cameraObject.AddComponent<GymCameraFollow>();
        follow.SetTarget(hero.transform);

        var note = new GameObject("README");
        note.transform.SetParent(root, false);
        var text = note.AddComponent<TextMesh>();
        text.text = "SYS2 DM_Gym — spikes (3 pips), pit warp (2 pips), kill plane, contact dummy (1 pip), D-1…D-5 (F7/F8 readouts)";
        text.fontSize = 48;
        text.characterSize = 0.12f;
        text.color = new Color(0.85f, 0.9f, 1f, 0.9f);
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font != null) note.GetComponent<MeshRenderer>().sharedMaterial = text.font.material;
        note.transform.position = new Vector3(-4f, 5f, 0f);
    }

    private static void ConfigureHazard(GameObject host, DamageTuning tuning, HazardSurface surface)
    {
        var hazard = host.AddComponent<Hazard2D>();
        SetObject(hazard, "tuning", tuning);
        SetInt(hazard, "hurtboxLayers", 1 << LayerMask.NameToLayer(HeroHurtboxLayerName));
        SetEnum(hazard, "surface", (int)surface);
    }

    private static void ConfigurePit(Transform parent, string name, Vector2 centre, Vector2 size, DamageTuning tuning)
    {
        GameObject host = TriggerBox(parent, name, centre, size, new Color(0.22f, 0.08f, 0.12f, 1f));
        var pit = host.AddComponent<PitVolume2D>();
        SetObject(pit, "tuning", tuning);
        SetInt(pit, "heroLayers", ~0);
    }

    private static GameObject Solid(Transform parent, string name, Vector2 center, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(center.x, center.y, 0f);

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = Sys1Tools.UnitSprite();
        renderer.color = color;
        renderer.sortingOrder = 0;

        var box = go.AddComponent<BoxCollider2D>();
        box.size = Vector2.one;

        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        return go;
    }

    /// <summary>
    /// An authoring-only volume on the Hazard layer: never solid, never drawn as contact
    /// geometry, always excluded from the motor's solid mask.
    /// </summary>
    private static GameObject TriggerBox(Transform parent, string name, Vector2 center, Vector2 size, Color color)
    {
        GameObject go = Solid(parent, name, center, size, color);

        var box = go.GetComponent<BoxCollider2D>();
        box.isTrigger = true;

        go.layer = LayerMask.NameToLayer(HazardLayerName);
        return go;
    }

    // ── Serialized-property plumbing ────────────────────────────────────────────────

    private static void SetObject(Object target, string fieldName, Object value)
    {
        if (target == null) return;

        var serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(fieldName);
        if (property == null)
        {
            Debug.LogWarning($"[Sys2Tools] {target.GetType().Name} has no field '{fieldName}'.");
            return;
        }

        property.objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        // Belt and braces: an asset reference applied to an object created in the same call as
        // its scene can be dropped by the first save, so the field is written directly too.
        FieldInfo field = target.GetType().GetField(fieldName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (field == null)
        {
            Debug.LogWarning($"[Sys2Tools] {target.GetType().Name}.{fieldName} could not be assigned.");
            return;
        }

        field.SetValue(target, value);
        EditorUtility.SetDirty(target);
    }

    private static void SetInt(Object target, string fieldName, int value)
    {
        if (target == null) return;

        var serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(fieldName);
        if (property == null)
        {
            Debug.LogWarning($"[Sys2Tools] {target.GetType().Name} has no field '{fieldName}'.");
            return;
        }

        property.intValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetEnum(Object target, string fieldName, int enumValueIndex)
    {
        if (target == null) return;

        var serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(fieldName);
        if (property == null)
        {
            Debug.LogWarning($"[Sys2Tools] {target.GetType().Name} has no field '{fieldName}'.");
            return;
        }

        property.enumValueIndex = enumValueIndex;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    // ── Plumbing ────────────────────────────────────────────────────────────────────

    private static int EnsureLayer(string layerName)
    {
        int existing = LayerMask.NameToLayer(layerName);
        if (existing >= 0) return existing;

        var tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty element = layers.GetArrayElementAtIndex(i);
            if (!string.IsNullOrEmpty(element.stringValue)) continue;

            element.stringValue = layerName;
            tagManager.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            return i;
        }

        Debug.LogError($"[Sys2Tools] No free user layer for '{layerName}'. Free one in Project Settings > Tags and Layers.");
        return -1;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
        string leaf = System.IO.Path.GetFileName(path);

        if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, leaf);
    }

    private static void RegisterInBuildSettings(string scenePath)
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (scenes.Exists(s => s.path == scenePath)) return;

        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static Color Grey() => new Color(0.35f, 0.36f, 0.38f, 1f);
    private static Color Red() => new Color(0.8f, 0.28f, 0.28f, 1f);
    private static Color Orange() => new Color(0.9f, 0.6f, 0.2f, 1f);
    private static Color Brown() => new Color(0.5f, 0.38f, 0.26f, 1f);
}
#endif
