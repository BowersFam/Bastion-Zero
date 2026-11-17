#if UNITY_EDITOR
using System.Collections.Generic;
using BastionZero.DebugTools;
using BastionZero.Movement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Step 1 (SYS1) project tooling: creates the SYS1 tuning asset, wires it onto Nexus, and
/// builds the MV_Gym measurement scene. Every action is idempotent, so it can be re-run.
/// Menu: Bastion Zero/SYS1.
/// </summary>
public static class Sys1Tools
{
    private const string TuningFolder = "Assets/BastionZero/Tuning";
    private const string TuningPath = TuningFolder + "/PlayerMovementTuning.asset";
    private const string PrefabFolder = "Assets/BastionZero/Prefabs";
    private const string HeroPrefabPath = PrefabFolder + "/Hero.prefab";
    private const string SceneFolder = "Assets/BastionZero/Scenes";
    private const string GymScenePath = SceneFolder + "/MV_Gym.unity";
    private const string OpeningLevelPath = "Assets/Scenes/OpeningLevel.unity";

    private static Sprite unitSprite;

    // ── Entry points ────────────────────────────────────────────────────────────────

    /// <summary>Creates the tuning asset and wires it onto Nexus in OpeningLevel.</summary>
    [MenuItem("Bastion Zero/SYS1/1 · Create Tuning Asset and Wire Nexus")]
    public static void Execute()
    {
        EnsureFolder(TuningFolder);

        PlayerMovementTuning tuning = CreateOrLoadTuning();
        AssignTuning(tuning);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[Sys1Tools] Tuning asset ready at {TuningPath} and assigned to Nexus.");
    }

    /// <summary>Builds Assets/BastionZero/Scenes/MV_Gym.unity and opens it.</summary>
    [MenuItem("Bastion Zero/SYS1/2 · Build MV_Gym Scene")]
    public static void BuildGym()
    {
        EnsureFolder(PrefabFolder);
        EnsureFolder(SceneFolder);

        // Any pending edits in the open scene are written first: opening OpeningLevel would
        // otherwise raise a modal save prompt and stall the build.
        EditorSceneManager.SaveOpenScenes();

        if (EditorSceneManager.GetActiveScene().path != OpeningLevelPath)
        {
            EditorSceneManager.OpenScene(OpeningLevelPath, OpenSceneMode.Single);
        }

        GameObject nexus = GameObject.Find("Nexus");
        if (nexus == null)
        {
            Debug.LogError("[Sys1Tools] Nexus not found in OpeningLevel; cannot build the hero prefab.");
            return;
        }

        PlayerMovementTuning tuning = CreateOrLoadTuning();
        AssignTuning(tuning);

        GameObject heroPrefabAsset = PrefabUtility.SaveAsPrefabAsset(nexus, HeroPrefabPath);
        if (heroPrefabAsset == null)
        {
            Debug.LogError("[Sys1Tools] Could not save the hero prefab.");
            return;
        }

        Scene gym = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        BuildGymContents(heroPrefabAsset);

        EditorSceneManager.SaveScene(gym, GymScenePath);
        RegisterInBuildSettings(GymScenePath);
        AssetDatabase.Refresh();

        Debug.Log($"[Sys1Tools] MV_Gym built at {GymScenePath}.");
    }

    // ── Tuning asset and wiring ─────────────────────────────────────────────────────

    private static PlayerMovementTuning CreateOrLoadTuning()
    {
        PlayerMovementTuning tuning = AssetDatabase.LoadAssetAtPath<PlayerMovementTuning>(TuningPath);
        if (tuning != null) return tuning;

        tuning = ScriptableObject.CreateInstance<PlayerMovementTuning>();
        AssetDatabase.CreateAsset(tuning, TuningPath);
        EditorUtility.SetDirty(tuning);

        return tuning;
    }

    private static void AssignTuning(PlayerMovementTuning tuning)
    {
        GameObject nexus = GameObject.Find("Nexus");
        if (nexus == null)
        {
            Debug.LogError("[Sys1Tools] Nexus not found; tuning not assigned.");
            return;
        }

        PlayerMovement motor = nexus.GetComponent<PlayerMovement>();
        if (motor == null)
        {
            Debug.LogError("[Sys1Tools] Nexus has no PlayerMovement component.");
            return;
        }

        var serialized = new SerializedObject(motor);
        SerializedProperty tuningProperty = serialized.FindProperty("tuning");
        if (tuningProperty != null)
        {
            tuningProperty.objectReferenceValue = tuning;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        if (nexus.GetComponent<SafeGroundRecorder>() == null) nexus.AddComponent<SafeGroundRecorder>();
        if (nexus.GetComponent<MovementStateReadout>() == null) nexus.AddComponent<MovementStateReadout>();

        Scene scene = nexus.scene;
        EditorSceneManager.MarkSceneDirty(scene);
        if (!string.IsNullOrEmpty(scene.path))
        {
            EditorSceneManager.SaveScene(scene);
        }
    }

    // ── MV_Gym ──────────────────────────────────────────────────────────────────────

    private static void BuildGymContents(GameObject heroPrefabAsset)
    {
        var root = new GameObject("MV_Gym").transform;

        // Flat run: x −6…4 on the floor line y = 0.
        Solid(root, "Ground_A_Run", new Vector2(-1f, -0.5f), new Vector2(10f, 1f), Grey());

        // MV-D2 base-kit gap: exactly 6 tiles, x 4…10.
        Solid(root, "Ground_B_Land", new Vector2(16f, -0.5f), new Vector2(12f, 1f), Grey());

        // Hard-gate reference: 9 tiles, x 22…31 (MV-D3 needs the air dash to clear this).
        Solid(root, "Ground_C_Far", new Vector2(35f, -0.5f), new Vector2(8f, 1f), Grey());

        // MV-D1 ledge heights: 3.5 tiles is reachable, 5.0 tiles is not, on the base kit.
        // The 5.0 ledge sits over the 9-tile pit so nothing standable is within jump reach of
        // it: MV-D3 wants a ≥ 5-tile ledge to be a hard gate, and a ledge beside the 3.5 one
        // would quietly route around the gate (the 3.5 ledge can reach 5.0).
        Solid(root, "Ledge_3p5_Reachable", new Vector2(17f, 3.25f), new Vector2(3f, 0.5f), Green());
        Solid(root, "Ledge_5p0_Unreachable", new Vector2(26.5f, 4.75f), new Vector2(3f, 0.5f), Red());

        // MV-21 head-bump test: underside at y = 2.5, hero is 2 tiles tall.
        Solid(root, "Ceiling_HeadBump", new Vector2(34f, 3f), new Vector2(5f, 1f), Orange());

        // W_CLING / W_JUMP wall: x 38…39, y 0…12.
        Solid(root, "ClingWall", new Vector2(38.5f, 6f), new Vector2(1f, 12f), Brown());

        // One-way platform: passable from below, landable from above. Kept left of the spawn
        // so it cannot catch the 6-tile-gap landing arc, which would hide the gap result.
        Solid(root, "OneWay_Platform", new Vector2(-2.5f, 2.2f), new Vector2(3f, 0.3f), Blue(), oneWay: true);

        // Hero.
        GameObject hero = (GameObject)PrefabUtility.InstantiatePrefab(heroPrefabAsset);
        hero.name = "Hero";
        hero.transform.position = new Vector3(0f, 0.91f, 0f);

        // Gym-only ability grant so the wall states can be exercised.
        var grant = new GameObject("GymAbilityGrant");
        grant.transform.SetParent(root, false);
        grant.AddComponent<GymAbilityGrant>();

        // Camera.
        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 1.66f, -10f);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 8.5f;
        camera.backgroundColor = new Color(0.08f, 0.09f, 0.11f, 1f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        cameraObject.AddComponent<AudioListener>();

        GymCameraFollow follow = cameraObject.AddComponent<GymCameraFollow>();
        follow.SetTarget(hero.transform);

        // A short note in the hierarchy so the gym's purpose survives the session.
        var note = new GameObject("README");
        note.transform.SetParent(root, false);
        var text = note.AddComponent<TextMesh>();
        text.text = "SYS1 MV_Gym — 6t gap, 9t gap, 3.5t/5t ledges, head-bump ceiling, cling wall (F8 readout)";
        text.fontSize = 48;
        text.characterSize = 0.12f;
        text.color = new Color(0.85f, 0.9f, 1f, 0.9f);
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font != null)
        {
            note.GetComponent<MeshRenderer>().sharedMaterial = text.font.material;
        }

        note.transform.position = new Vector3(-4f, 4f, 0f);
    }

    private static GameObject Solid(Transform parent, string name, Vector2 center, Vector2 size, Color color, bool oneWay = false)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(center.x, center.y, 0f);

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = UnitSprite();
        renderer.color = color;
        renderer.sortingOrder = 0;

        var box = go.AddComponent<BoxCollider2D>();
        box.size = Vector2.one;

        if (oneWay)
        {
            var effector = go.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            box.usedByEffector = true;
        }

        // localScale is assigned last: setting drawMode/effector first would reshape it.
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        return go;
    }

    internal static Sprite UnitSprite()
    {
        if (unitSprite != null) return unitSprite;

        const string artFolder = "Assets/BastionZero/Art";
        const string assetPath = artFolder + "/GymUnit.png";

        EnsureFolder(artFolder);

        // The block must be a real imported asset: a Sprite built with Sprite.Create lives in
        // memory only, so a saved scene would come back with an unassigned sprite and the
        // gym geometry would be invisible in the game view.
        if (!System.IO.File.Exists(assetPath))
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            System.IO.File.WriteAllBytes(assetPath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 1f;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();

        unitSprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (unitSprite != null) unitSprite.name = "GymUnit";

        return unitSprite;
    }

    private static Color Grey() => new Color(0.35f, 0.36f, 0.38f, 1f);
    private static Color Green() => new Color(0.35f, 0.75f, 0.42f, 1f);
    private static Color Red() => new Color(0.8f, 0.28f, 0.28f, 1f);
    private static Color Orange() => new Color(0.9f, 0.6f, 0.2f, 1f);
    private static Color Brown() => new Color(0.5f, 0.38f, 0.26f, 1f);
    private static Color Blue() => new Color(0.35f, 0.6f, 0.9f, 1f);

    // ── Plumbing ────────────────────────────────────────────────────────────────────

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
}
#endif
