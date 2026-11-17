#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using BastionZero.Combat;
using BastionZero.Damage;
using BastionZero.DebugTools;
using BastionZero.Movement;
using BastionZero.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// Step 3 (SYS3) project tooling: the combat tuning asset, the hero's combat overlay, the enemy
/// sprite import and prefab, and the CM_Gym combat gym. Every action is idempotent, so it can be
/// re-run. Menu: Bastion Zero/SYS3.
/// </summary>
public static class Sys3Tools
{
    private const string TuningFolder = "Assets/BastionZero/Tuning";
    private const string CombatTuningPath = TuningFolder + "/CombatTuning.asset";
    private const string DamageTuningPath = TuningFolder + "/DamageTuning.asset";
    private const string MovementTuningPath = TuningFolder + "/PlayerMovementTuning.asset";
    private const string WorldCameraTuningPath = TuningFolder + "/WorldCameraTuning.asset";
    private const string HeroPrefabPath = "Assets/BastionZero/Prefabs/Hero.prefab";
    private const string EnemyPrefabPath = "Assets/BastionZero/Prefabs/Enemy_CinderMite.prefab";
    private const string SceneFolder = "Assets/BastionZero/Scenes";
    private const string GymScenePath = SceneFolder + "/CM_Gym.unity";

    private const string MiteSpritePath = "Assets/Generated_Images/cinder_mite.png";

    // 1024 px canvas, creature ≈ 960 × 490 px. At 512 px per unit the mite is about 1.9 tiles
    // wide and 0.95 tiles tall, so it reads as a low swarmer next to the 2-tile hero.
    private const float MitePixelsPerUnit = 512f;

    private const string EnemyHurtboxLayerName = "EnemyHurtbox";
    private const string HeroHurtboxLayerName = "HeroHurtbox";
    private const string HazardLayerName = "Hazard";

    // ── Entry points ────────────────────────────────────────────────────────────────

    /// <summary>Combat tuning asset, the EnemyHurtbox layer and the hero's combat overlay.</summary>
    [MenuItem("Bastion Zero/SYS3/1 · Create Combat Tuning and Wire Hero")]
    public static void Configure()
    {
        EnsureFolder(TuningFolder);

        int enemyLayer = EnsureLayer(EnemyHurtboxLayerName);
        if (enemyLayer < 0) return;

        CombatTuning tuning = CreateOrLoadCombatTuning();
        ExcludeFromSolidLayers(enemyLayer);

        // The prefab carries the overlay into every scene that instantiates the hero.
        if (System.IO.File.Exists(HeroPrefabPath))
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(HeroPrefabPath);
            try
            {
                AttachCombatComponents(contents, tuning, enemyLayer);
                PrefabUtility.SaveAsPrefabAsset(contents, HeroPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }
        else
        {
            Debug.LogError($"[Sys3Tools] {HeroPrefabPath} not found; run the SYS1/SYS2 tools first.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.SaveOpenScenes();

        Debug.Log($"[Sys3Tools] Combat tuning at {CombatTuningPath}; hero combat overlay wired. " +
                  $"Layer: {EnemyHurtboxLayerName}={enemyLayer} (removed from solidLayers).");
    }

    /// <summary>Imports the Cinder-Mite sprite as a crisp single sprite and builds the enemy prefab.</summary>
    [MenuItem("Bastion Zero/SYS3/2 · Import Enemy Sprite and Build Enemy Prefab")]
    public static void BuildEnemyPrefab()
    {
        CombatTuning tuning = AssetDatabase.LoadAssetAtPath<CombatTuning>(CombatTuningPath);
        DamageTuning damage = AssetDatabase.LoadAssetAtPath<DamageTuning>(DamageTuningPath);
        int enemyLayer = LayerMask.NameToLayer(EnemyHurtboxLayerName);
        int heroHurtbox = LayerMask.NameToLayer(HeroHurtboxLayerName);

        if (tuning == null || damage == null || enemyLayer < 0)
        {
            Debug.LogError("[Sys3Tools] Run 'Bastion Zero/SYS3/1 · Create Combat Tuning and Wire Hero' first.");
            return;
        }

        Sprite sprite = ImportMiteSprite();
        if (sprite == null) return;

        EnsureFolder("Assets/BastionZero/Prefabs");

        var root = new GameObject("Enemy_CinderMite");
        try
        {
            root.layer = enemyLayer;

            var renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 5;

            // Hurtbox and contact hitbox share one trigger box: the hero walks through it, the
            // slash sweep and ContactDamager2D both read it, and killing disables it.
            var box = root.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(1.7f, 0.75f);
            box.offset = new Vector2(0f, -0.02f);

            var contact = root.AddComponent<ContactDamager2D>();
            SetObject(contact, "tuning", damage);
            SetInt(contact, "hurtboxLayers", 1 << heroHurtbox);
            SetEnum(contact, "damageClass", (int)DamageClass.D_Light);

            var target = root.AddComponent<EnemyCombatTarget>();
            SetObject(target, "tuning", tuning);
            SetEnum(target, "tier", (int)EnemyTier.Fodder);
            SetBool(target, "facingRight", false);
            SetObject(target, "body", renderer);

            PrefabUtility.SaveAsPrefabAsset(root, EnemyPrefabPath);
            AssetDatabase.ImportAsset(EnemyPrefabPath, ImportAssetOptions.ForceSynchronousImport);
        }
        finally
        {
            Object.DestroyImmediate(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[Sys3Tools] Cinder-Mite imported at {MitePixelsPerUnit} px/unit and saved to {EnemyPrefabPath}.");
    }

    /// <summary>Builds Assets/BastionZero/Scenes/CM_Gym.unity and opens it.</summary>
    [MenuItem("Bastion Zero/SYS3/3 · Build CM_Gym Scene")]
    public static void BuildGym()
    {
        EnsureFolder(SceneFolder);
        EditorSceneManager.SaveOpenScenes();

        GameObject heroPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HeroPrefabPath);
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);
        CombatTuning tuning = AssetDatabase.LoadAssetAtPath<CombatTuning>(CombatTuningPath);
        WorldCameraTuning cameraTuning = AssetDatabase.LoadAssetAtPath<WorldCameraTuning>(WorldCameraTuningPath);

        if (heroPrefab == null || enemyPrefab == null || tuning == null)
        {
            Debug.LogError("[Sys3Tools] Run 'SYS3/1' and 'SYS3/2' first.");
            return;
        }

        if (cameraTuning == null)
        {
            Debug.LogError($"[Sys3Tools] {WorldCameraTuningPath} not found; run 'Bastion Zero/Tuning/Create World Camera Tuning' first.");
            return;
        }

        Scene gym = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        BuildGymContents(heroPrefab, enemyPrefab, tuning, cameraTuning);

        EditorSceneManager.SaveScene(gym, GymScenePath);
        RegisterInBuildSettings(GymScenePath);
        AssetDatabase.Refresh();

        // The first save of a brand-new scene can drop asset references, so they are re-applied
        // against the saved scene (and only then does the write stick).
        foreach (EnemyCombatTarget target in Object.FindObjectsByType<EnemyCombatTarget>(FindObjectsInactive.Include))
            SetObject(target, "tuning", tuning);

        foreach (WorldCamera worldCamera in Object.FindObjectsByType<WorldCamera>(FindObjectsInactive.Include))
            SetObject(worldCamera, "tuning", cameraTuning);

        EditorSceneManager.SaveScene(gym, GymScenePath);
        AssetDatabase.SaveAssets();

        Debug.Log($"[Sys3Tools] CM_Gym built at {GymScenePath}.");
    }

    // ── Tuning, layers and wiring ───────────────────────────────────────────────────

    private static CombatTuning CreateOrLoadCombatTuning()
    {
        CombatTuning tuning = AssetDatabase.LoadAssetAtPath<CombatTuning>(CombatTuningPath);
        if (tuning != null) return tuning;

        tuning = ScriptableObject.CreateInstance<CombatTuning>();
        AssetDatabase.CreateAsset(tuning, CombatTuningPath);

        // Import synchronously: a reference to an asset that has not been imported yet is
        // silently written as null when the prefab is saved later in this same call.
        AssetDatabase.ImportAsset(CombatTuningPath, ImportAssetOptions.ForceSynchronousImport);
        EditorUtility.SetDirty(tuning);

        return tuning;
    }

    /// <summary>
    /// An enemy body is a trigger volume, never terrain. A new layer that stays inside the motor's
    /// solid mask would turn every enemy into a wall.
    /// </summary>
    private static void ExcludeFromSolidLayers(int enemyLayer)
    {
        PlayerMovementTuning movement = AssetDatabase.LoadAssetAtPath<PlayerMovementTuning>(MovementTuningPath);
        if (movement == null)
        {
            Debug.LogWarning("[Sys3Tools] PlayerMovementTuning not found; run the SYS1 tools first.");
            return;
        }

        var serialized = new SerializedObject(movement);

        SerializedProperty solid = serialized.FindProperty("solidLayers");
        if (solid != null) solid.intValue &= ~(1 << enemyLayer);

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(movement);
        AssetDatabase.SaveAssets();
    }

    private static void AttachCombatComponents(GameObject hero, CombatTuning tuning, int enemyLayer)
    {
        Transform slashTransform = hero.transform.Find("SlashVolume");
        if (slashTransform == null)
        {
            var slashObject = new GameObject("SlashVolume");
            slashTransform = slashObject.transform;
            slashTransform.SetParent(hero.transform, false);
        }

        // Pose() replaces both values per attack, so they only have to be sane in the editor.
        slashTransform.localPosition = Vector3.zero;
        slashTransform.localRotation = Quaternion.identity;
        slashTransform.localScale = Vector3.one;

        SlashVolume2D slash = GetOrAdd<SlashVolume2D>(slashTransform.gameObject);
        SetInt(slash, "targetLayers", 1 << enemyLayer);
        SetVector2(slash, "localOffset", new Vector2(0.62f, 0.05f));
        SetVector2(slash, "size", new Vector2(1.1f, 0.9f));

        // Hit-stop is looked up with GetComponentInChildren, so it lives on the hero root.
        HitStopController hitStop = GetOrAdd<HitStopController>(hero);

        HeroCombat combat = GetOrAdd<HeroCombat>(hero);
        CombatStateReadout readout = GetOrAdd<CombatStateReadout>(hero);

        SetObject(combat, "tuning", tuning);
        SetObject(combat, "movement", hero.GetComponent<PlayerMovement>());
        SetObject(combat, "health", hero.GetComponent<HeroHealth>());
        SetObject(combat, "playerInput", hero.GetComponent<UnityEngine.InputSystem.PlayerInput>());
        SetObject(combat, "animator", hero.GetComponentInChildren<Animator>());
        SetObject(combat, "slash", slash);
        SetObject(combat, "hitStop", hitStop);

        SetObject(readout, "combat", combat);
        SetObject(readout, "hitStop", hitStop);
    }

    /// <summary>
    /// Imports the Cinder-Mite as one crisp sprite. Public so the art tools' sampling re-apply covers
    /// it alongside the hero frames.
    /// </summary>
    public static Sprite ImportMiteSprite()
    {
        var importer = AssetImporter.GetAtPath(MiteSpritePath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError($"[Sys3Tools] {MiteSpritePath} not found or is not a texture.");
            return null;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = MitePixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;

        // The 1024 px canvas draws at about 127 px (ortho 8.5 at 1080p). Point alone skips seven of
        // every eight texels there and the shell shimmers as the mite walks; mipmaps give Point a
        // pre-filtered level, and ignoreMipmapLimit keeps the quality level's limit off it.
        importer.mipmapEnabled = true;
        importer.ignoreMipmapLimit = true;
        importer.wrapMode = TextureWrapMode.Clamp;

        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteAlignment = (int)SpriteAlignment.Center;
        settings.spriteGenerateFallbackPhysicsShape = false;
        importer.SetTextureSettings(settings);

        importer.SaveAndReimport();

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(MiteSpritePath);
        if (sprite == null)
        {
            Debug.LogError($"[Sys3Tools] {MiteSpritePath} did not import as a sprite.");
        }

        return sprite;
    }

    private static T GetOrAdd<T>(GameObject host) where T : Component
    {
        T component = host.GetComponent<T>();
        if (component == null) component = host.AddComponent<T>();
        return component;
    }

    // ── CM_Gym ──────────────────────────────────────────────────────────────────────

    private static void BuildGymContents(
        GameObject heroPrefab, GameObject enemyPrefab, CombatTuning tuning, WorldCameraTuning cameraTuning)
    {
        var root = new GameObject("CM_Gym").transform;

        // WC-W10 room clamp on the room root. y −6…20 is load-bearing: at a 17-tile viewport the
        // camera can only sit between yMin + 8.5 and yMax − 8.5, so a floor at y 0 needs yMin ≤ −5.5
        // for the resting frame to keep the feet 3 tiles below centre (WC-W9).
        CameraRoomBounds bounds = root.gameObject.AddComponent<CameraRoomBounds>();
        var boundsSo = new SerializedObject(bounds);
        boundsSo.FindProperty("center").vector2Value = new Vector2(0f, 7f);
        boundsSo.FindProperty("size").vector2Value = new Vector2(40f, 26f);
        boundsSo.ApplyModifiedPropertiesWithoutUndo();

        // One long floor, top at y = 0, so every measurement is a tile count from the ground. Six
        // tiles thick so solid ground fills the bottom of the frame instead of void.
        Solid(root, "Floor", new Vector2(0f, -3f), new Vector2(40f, 6f), Grey());

        // Right wall for WALL1 and for the wall-cling cancel.
        Solid(root, "Wall_Right", new Vector2(14.5f, 3f), new Vector2(1f, 8f), Brown());

        // A raised block to attack from above (AIR1) and to land on.
        Solid(root, "Ledge", new Vector2(6f, 1.5f), new Vector2(6f, 1f), Grey());

        // Fodder: poise threshold 0, so every hit flinches (EN-P1 band).
        SpawnEnemy(root, enemyPrefab, "Mite_Fodder", new Vector2(-2f, 0.45f), EnemyTier.Fodder, tuning);

        // Soldier band: carries a poise meter, so the stagger and the H9 row can be read.
        SpawnEnemy(root, enemyPrefab, "Mite_Soldier", new Vector2(3f, 0.45f), EnemyTier.Soldier, tuning);

        // Hero.
        GameObject hero = (GameObject)PrefabUtility.InstantiatePrefab(heroPrefab);
        hero.name = "Hero";
        hero.transform.position = new Vector3(-8f, 0.91f, 0f);

        // Camera: SYS5's WorldCamera with the SYS3 shake layer, the same rig EM_Gym uses. Ortho 8.5
        // and the feet + 3 resting frame come from WorldCameraTuning.
        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(-2f, 1.66f, -10f);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 8.5f;
        camera.backgroundColor = new Color(0.08f, 0.09f, 0.11f, 1f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        cameraObject.AddComponent<UniversalAdditionalCameraData>();
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<CombatCameraShake>();

        WorldCamera worldCamera = cameraObject.AddComponent<WorldCamera>();
        var cameraSo = new SerializedObject(worldCamera);
        cameraSo.FindProperty("target").objectReferenceValue = hero.transform;
        cameraSo.FindProperty("movement").objectReferenceValue = hero.GetComponent<PlayerMovement>();
        cameraSo.FindProperty("combat").objectReferenceValue = hero.GetComponent<HeroCombat>();
        cameraSo.FindProperty("viewCamera").objectReferenceValue = camera;
        cameraSo.FindProperty("playerInput").objectReferenceValue = hero.GetComponent<UnityEngine.InputSystem.PlayerInput>();
        cameraSo.FindProperty("tuning").objectReferenceValue = cameraTuning;
        cameraSo.FindProperty("heroFeetLocalY").floatValue = -0.91f;
        cameraSo.FindProperty("roomBounds").objectReferenceValue = bounds;
        cameraSo.FindProperty("findRoomBounds").boolValue = true;
        cameraSo.FindProperty("logTransitions").boolValue = true;
        cameraSo.ApplyModifiedPropertiesWithoutUndo();

        var note = new GameObject("README");
        note.transform.SetParent(root, false);
        TextMesh text = note.AddComponent<TextMesh>();
        text.text = "SYS3 CM_Gym — attack chain, AIR1, WALL1, dash (shift/RB), block + parry (Q/LB). F8 combat readout.";
        text.fontSize = 48;
        text.characterSize = 0.12f;
        text.color = new Color(0.85f, 0.9f, 1f, 0.9f);
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font != null) note.GetComponent<MeshRenderer>().sharedMaterial = text.font.material;
        note.transform.position = new Vector3(-4f, 5f, 0f);
    }

    private static void SpawnEnemy(
        Transform parent, GameObject prefab, string name, Vector2 position, EnemyTier tier, CombatTuning tuning)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = name;
        instance.transform.SetParent(parent, false);
        instance.transform.position = new Vector3(position.x, position.y, 0f);

        var target = instance.GetComponent<EnemyCombatTarget>();
        if (target == null) return;

        SetEnum(target, "tier", (int)tier);
        SetObject(target, "tuning", tuning);
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

    // ── Serialized-property plumbing ────────────────────────────────────────────────

    private static void SetObject(Object target, string fieldName, Object value)
    {
        if (target == null) return;

        var serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(fieldName);
        if (property == null)
        {
            Debug.LogWarning($"[Sys3Tools] {target.GetType().Name} has no field '{fieldName}'.");
            return;
        }

        property.objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        // Belt and braces: an asset reference applied to an object created in the same call as
        // its scene can be dropped by the first save, so the field is written directly too.
        FieldInfo field = target.GetType().GetField(fieldName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (field != null) field.SetValue(target, value);
        EditorUtility.SetDirty(target);
    }

    private static void SetInt(Object target, string fieldName, int value)
    {
        if (target == null) return;

        var serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(fieldName);
        if (property == null)
        {
            Debug.LogWarning($"[Sys3Tools] {target.GetType().Name} has no field '{fieldName}'.");
            return;
        }

        property.intValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetBool(Object target, string fieldName, bool value)
    {
        if (target == null) return;

        var serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(fieldName);
        if (property == null)
        {
            Debug.LogWarning($"[Sys3Tools] {target.GetType().Name} has no field '{fieldName}'.");
            return;
        }

        property.boolValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetVector2(Object target, string fieldName, Vector2 value)
    {
        if (target == null) return;

        var serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(fieldName);
        if (property == null)
        {
            Debug.LogWarning($"[Sys3Tools] {target.GetType().Name} has no field '{fieldName}'.");
            return;
        }

        property.vector2Value = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetEnum(Object target, string fieldName, int enumValueIndex)
    {
        if (target == null) return;

        var serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(fieldName);
        if (property == null)
        {
            Debug.LogWarning($"[Sys3Tools] {target.GetType().Name} has no field '{fieldName}'.");
            return;
        }

        property.enumValueIndex = enumValueIndex;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
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

        Debug.LogError($"[Sys3Tools] No free user layer for '{layerName}'. Free one in Project Settings > Tags and Layers.");
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
    private static Color Brown() => new Color(0.5f, 0.38f, 0.26f, 1f);
}
#endif
