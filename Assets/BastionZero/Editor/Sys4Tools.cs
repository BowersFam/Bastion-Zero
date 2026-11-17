using BastionZero.Combat;
using BastionZero.Damage;
using BastionZero.Enemies;
using BastionZero.Movement;
using BastionZero.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace BastionZero.EditorTools
{
    /// <summary>
    /// SYS4 slice tooling: the EnemyTuning asset, the §3.2 attack-sign sprite, the two patroller
    /// prefabs (fodder P-F and soldier P-S of §4) and the EM_Gym measurement scene.
    ///
    /// Every menu item is idempotent — re-running replaces what it owns instead of duplicating it.
    /// The gym is regenerated from this menu item, never hand-edited, so the scene and the §4
    /// table can never drift apart.
    /// </summary>
    public static class Sys4Tools
    {
        private const string TuningPath = "Assets/BastionZero/Tuning/EnemyTuning.asset";
        private const string SignSpritePath = "Assets/BastionZero/Art/AttackSign.png";
        private const string PrefabDir = "Assets/BastionZero/Prefabs/";
        private const string MitePrefabPath = "Assets/BastionZero/Prefabs/Enemy_CinderMite.prefab";
        private const string HeroPrefabPath = "Assets/BastionZero/Prefabs/Hero.prefab";
        private const string GymUnitPath = "Assets/BastionZero/Art/GymUnit.png";
        private const string WorldCameraTuningPath = "Assets/BastionZero/Tuning/WorldCameraTuning.asset";
        private const string ScenePath = "Assets/BastionZero/Scenes/EM_Gym.unity";

        // 4294963711 as a signed int: every layer except 9, 10 and 11 (HeroHurtbox, Hazard,
        // EnemyHurtbox). Same convention PlayerMovementTuning.solidLayers uses, so an enemy's
        // ledge and wall probes see exactly the geometry the hero does.
        private const int SolidMask = -3585;
        private const int HeroHurtboxMask = 1 << 9;

        private static readonly Color FloorColour = new Color(0.35f, 0.36f, 0.38f, 1f);
        private static readonly Color PlatformColour = new Color(0.44f, 0.46f, 0.47f, 1f);

        // ── 1 · tuning asset and sign sprite ────────────────────────────────────────

        [MenuItem("Bastion Zero/SYS4/1 · Create Tuning Asset And Attack Sign", false, 40)]
        public static void CreateTuningAndSign()
        {
            EnemyTuning tuning = EnsureTuning();
            Sprite sign = EnsureSignSprite();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = tuning;
            EditorGUIUtility.PingObject(sign != null ? sign : tuning);

            Debug.Log($"[SYS4] Tuning asset '{TuningPath}' and attack sign '{SignSpritePath}' are ready.");
        }

        // ── 2 · patroller prefabs ───────────────────────────────────────────────────

        [MenuItem("Bastion Zero/SYS4/2 · Build Patroller Prefabs", false, 41)]
        public static void BuildPatrollerPrefabs()
        {
            EnemyTuning tuning = EnsureTuning();
            Sprite sign = EnsureSignSprite();
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(MitePrefabPath);

            if (source == null)
            {
                Debug.LogError($"[SYS4] Missing base prefab '{MitePrefabPath}'. Run the SYS3 tooling first.");
                return;
            }

            BuildOne(source, tuning, sign, "Enemy_Patroller_Fodder", EnemyTier.Fodder, 3f);
            BuildOne(source, tuning, sign, "Enemy_Patroller_Soldier", EnemyTier.Soldier, 3f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[SYS4] Built 'Enemy_Patroller_Fodder' (P-F) and 'Enemy_Patroller_Soldier' (P-S).");
        }

        private static void BuildOne(GameObject source, EnemyTuning tuning, Sprite signSprite,
                                     string prefabName, EnemyTier tier, float patrolHalfRange)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            instance.name = prefabName;

            var target = instance.GetComponent<EnemyCombatTarget>();
            if (target == null)
            {
                Debug.LogError($"[SYS4] '{MitePrefabPath}' has no EnemyCombatTarget; cannot build '{prefabName}'.");
                Object.DestroyImmediate(instance);
                return;
            }

            var targetSo = new SerializedObject(target);
            targetSo.FindProperty("tier").enumValueIndex = (int)tier;
            targetSo.ApplyModifiedPropertiesWithoutUndo();

            // The sign lives on its own child so the §14 size slider only scales the sign.
            Transform previousSign = instance.transform.Find("AttackSign");
            if (previousSign != null) Object.DestroyImmediate(previousSign.gameObject);

            var signObject = new GameObject("AttackSign");
            signObject.transform.SetParent(instance.transform, false);
            signObject.transform.localPosition = new Vector3(tuning.attackSignOffset.x, tuning.attackSignOffset.y, 0f);

            var signRenderer = signObject.AddComponent<SpriteRenderer>();
            signRenderer.sprite = signSprite;
            signRenderer.color = tuning.attackSignColour;
            signRenderer.sortingOrder = 20;
            signRenderer.enabled = false;

            var sign = signObject.AddComponent<AttackSign>();

            var ai = instance.GetComponent<EnemyAI>();
            if (ai == null) ai = instance.AddComponent<EnemyAI>();

            var aiSo = new SerializedObject(ai);
            aiSo.FindProperty("tuning").objectReferenceValue = tuning;
            aiSo.FindProperty("target").objectReferenceValue = target;
            aiSo.FindProperty("contact").objectReferenceValue = instance.GetComponent<ContactDamager2D>();
            aiSo.FindProperty("body").objectReferenceValue = instance.GetComponentInChildren<SpriteRenderer>(true);
            aiSo.FindProperty("sign").objectReferenceValue = sign;
            aiSo.FindProperty("artFacesRight").boolValue = false;
            aiSo.FindProperty("patrolHalfRangeTiles").floatValue = patrolHalfRange;
            aiSo.FindProperty("startFacingRight").boolValue = false;
            aiSo.FindProperty("solidLayers").intValue = SolidMask;
            aiSo.FindProperty("heroHurtboxLayers").intValue = HeroHurtboxMask;
            aiSo.ApplyModifiedPropertiesWithoutUndo();

            // The AI moves the body by transform at 60 Hz, so it is drawn between ticks while every
            // probe and collider stays on the true tick position.
            if (instance.GetComponent<EnemyRenderSmoother>() == null) instance.AddComponent<EnemyRenderSmoother>();

            PrefabUtility.SaveAsPrefabAsset(instance, PrefabDir + prefabName + ".prefab");
            Object.DestroyImmediate(instance);
        }

        // ── 3 · the gym scene ───────────────────────────────────────────────────────

        [MenuItem("Bastion Zero/SYS4/3 · Build EM_Gym Scene", false, 42)]
        public static void BuildGymScene()
        {
            EnemyTuning tuning = EnsureTuning();
            Sprite blockSprite = AssetDatabase.LoadAssetAtPath<Sprite>(GymUnitPath);
            var heroPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HeroPrefabPath);
            var cameraTuning = AssetDatabase.LoadAssetAtPath<WorldCameraTuning>(WorldCameraTuningPath);
            var fodderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "Enemy_Patroller_Fodder.prefab");
            var soldierPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "Enemy_Patroller_Soldier.prefab");

            if (blockSprite == null || heroPrefab == null || cameraTuning == null ||
                fodderPrefab == null || soldierPrefab == null)
            {
                Debug.LogError("[SYS4] EM_Gym needs GymUnit.png, Hero.prefab, WorldCameraTuning.asset and both " +
                               "patroller prefabs. Run SYS4 step 2 first.");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new GameObject("EM_Gym");
            var bounds = root.AddComponent<CameraRoomBounds>();
            var boundsSo = new SerializedObject(bounds);
            boundsSo.FindProperty("center").vector2Value = new Vector2(0f, 7f);
            boundsSo.FindProperty("size").vector2Value = new Vector2(40f, 26f);
            boundsSo.ApplyModifiedPropertiesWithoutUndo();

            // Ground whose top surface is exactly y 0, as every SYS1/SYS2 assumption requires.
            CreateBlock(root.transform, "Floor", new Vector2(0f, -3f), new Vector2(40f, 6f), FloorColour, blockSprite);
            CreateBlock(root.transform, "Wall_Left", new Vector2(-19.5f, 3f), new Vector2(1f, 8f), FloorColour, blockSprite);
            CreateBlock(root.transform, "Wall_Right", new Vector2(19.5f, 3f), new Vector2(1f, 8f), FloorColour, blockSprite);

            // Two ledges at different heights: a patroller on either must turn at the edge.
            CreateBlock(root.transform, "Ledge_A", new Vector2(4f, 1.5f), new Vector2(6f, 1f), PlatformColour, blockSprite);
            CreateBlock(root.transform, "Ledge_B", new Vector2(-5f, 3.5f), new Vector2(5f, 1f), PlatformColour, blockSprite);

            var hero = (GameObject)PrefabUtility.InstantiatePrefab(heroPrefab);
            hero.name = "Hero";
            hero.transform.position = new Vector3(-8f, 0.91f, 0f);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 3f, -10f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.11f);
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000f;
            cameraObject.AddComponent<UniversalAdditionalCameraData>();
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<CombatCameraShake>();

            var worldCamera = cameraObject.AddComponent<WorldCamera>();
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
            cameraSo.FindProperty("logTransitions").boolValue = false;
            cameraSo.ApplyModifiedPropertiesWithoutUndo();

            // One fodder on the floor, one fodder on a ledge (the ledge-turn case), one soldier
            // on the floor at the far end. Fodder and soldier are §13's T1 teaching pair.
            PlaceEnemy(root.transform, fodderPrefab, "Mite_Fodder_A", new Vector3(-2f, 0.45f, 0f), true);
            PlaceEnemy(root.transform, fodderPrefab, "Mite_Fodder_Ledge", new Vector3(4f, 2.45f, 0f), false);
            PlaceEnemy(root.transform, soldierPrefab, "Mite_Soldier", new Vector3(9f, 0.45f, 0f), false);

            var readme = new GameObject("README");
            readme.transform.SetParent(root.transform, false);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);

            // The first save of a brand-new scene can drop an asset reference set on a scene object
            // (the camera's tuning came back None that way), so it is re-applied against the saved
            // scene and saved again — the same guard Sys3Tools.BuildGym uses for CM_Gym.
            var cameraFix = new SerializedObject(worldCamera);
            cameraFix.FindProperty("tuning").objectReferenceValue = cameraTuning;
            cameraFix.ApplyModifiedPropertiesWithoutUndo();

            System.Reflection.FieldInfo tuningField = typeof(WorldCamera).GetField("tuning",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);
            if (tuningField != null) tuningField.SetValue(worldCamera, cameraTuning);

            EditorUtility.SetDirty(worldCamera);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SYS4] Built '{ScenePath}': 2 fodder (one on Ledge_A for the ledge turn) and 1 soldier, " +
                      "hero at (-8, 0.91), floor top at y 0.");
        }

        private static void PlaceEnemy(Transform parent, GameObject prefab, string name, Vector3 position, bool patrol)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = name;
            instance.transform.SetParent(parent, true);
            instance.transform.position = position;

            // The ledge fodder patrols a short beat so it reaches the edge and turns inside a
            // couple of seconds; the floor pair get the default 3-tile beat.
            var ai = instance.GetComponent<EnemyAI>();
            if (ai == null) return;

            var so = new SerializedObject(ai);
            so.FindProperty("patrolHalfRangeTiles").floatValue = patrol ? 3f : 2.5f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateBlock(Transform parent, string blockName, Vector2 centre,
                                              Vector2 size, Color colour, Sprite sprite)
        {
            var go = new GameObject(blockName);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(centre.x, centre.y, 0f);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = colour;
            renderer.drawMode = SpriteDrawMode.Simple;

            var box = go.AddComponent<BoxCollider2D>();
            box.size = Vector2.one;

            return go;
        }

        // ── Shared assets ───────────────────────────────────────────────────────────

        private static EnemyTuning EnsureTuning()
        {
            var tuning = AssetDatabase.LoadAssetAtPath<EnemyTuning>(TuningPath);
            if (tuning != null) return tuning;

            tuning = ScriptableObject.CreateInstance<EnemyTuning>();
            AssetDatabase.CreateAsset(tuning, TuningPath);
            return tuning;
        }

        /// <summary>
        /// Draws the §3.2 attack sign: a 16 px diamond at PPU 16, so one sign is one tile at the
        /// §14 slider's 100%. White fill with a dark rim, tinted per attack class at runtime.
        /// </summary>
        private static Sprite EnsureSignSprite()
        {
            if (AssetDatabase.LoadAssetAtPath<Sprite>(SignSpritePath) != null)
            {
                ConfigureSignImporter();
                return AssetDatabase.LoadAssetAtPath<Sprite>(SignSpritePath);
            }

            const int size = 16;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Mathf.Abs(x - (size - 1) * 0.5f) + Mathf.Abs(y - (size - 1) * 0.5f);

                    Color colour;
                    if (d > 6.5f) colour = Color.clear;
                    else if (d > 5f) colour = new Color(0.16f, 0.12f, 0.1f, 1f);
                    else colour = Color.white;

                    texture.SetPixel(x, y, colour);
                }
            }

            texture.Apply();

            string absolute = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), SignSpritePath);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(absolute));
            System.IO.File.WriteAllBytes(absolute, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(SignSpritePath, ImportAssetOptions.ForceUpdate);
            ConfigureSignImporter();

            return AssetDatabase.LoadAssetAtPath<Sprite>(SignSpritePath);
        }

        private static void ConfigureSignImporter()
        {
            if (!(AssetImporter.GetAtPath(SignSpritePath) is TextureImporter importer)) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 16f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        private static void AddToBuildSettings(string path)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].path == path) return;
            }

            var updated = new EditorBuildSettingsScene[scenes.Length + 1];
            System.Array.Copy(scenes, updated, scenes.Length);
            updated[scenes.Length] = new EditorBuildSettingsScene(path, true);
            EditorBuildSettings.scenes = updated;
        }
    }
}
