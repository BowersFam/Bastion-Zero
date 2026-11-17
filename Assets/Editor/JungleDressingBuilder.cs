// Bastion Zero — dresses OpeningLevel with the baked PureNature jungle sprites.
//
// Everything it places is a SpriteRenderer on the "Default" sorting layer, spread
// across explicit sorting orders so it interleaves correctly with the tilemap
// (order 0/1), the hand-placed detail props (3-5), Nexus (10) and the
// collectibles (12). Nothing here has a collider, so the kinematic motor is
// unaffected.
//
// Menu: Bastion Zero/Dress Opening Level With Jungle
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class JungleDressingBuilder
{
    const string SpriteFolder = "Assets/Art/JunglePixel/";
    const float BatchPPU = 32f;

    struct Layer
    {
        public string Name;
        public string[] Sprites;
        public int SortingOrder;
        public float Z;
        public float MinX;
        public float MaxX;
        public float MinStep;
        public float MaxStep;
        public float BaseOffset;     // relative to the ground surface
        public Color Tint;
        public bool NeedsGround;
        public bool NeedsSolidGround; // skip the gap-filled spans over pits
        public float MinVisible, MaxVisible; // >0: bury the sprite so only this much shows above the ground line
        public Layer(string name, string[] sprites, int order, float z, float minX, float maxX,
                     float minStep, float maxStep, float baseOffset, Color tint, bool needsGround,
                     bool needsSolidGround = false, float minVisible = 0f, float maxVisible = 0f)
        {
            Name = name; Sprites = sprites; SortingOrder = order; Z = z;
            MinX = minX; MaxX = maxX; MinStep = minStep; MaxStep = maxStep;
            BaseOffset = baseOffset; Tint = tint; NeedsGround = needsGround;
            NeedsSolidGround = needsSolidGround; MinVisible = minVisible; MaxVisible = maxVisible;
        }
    }

    static readonly Layer[] Layers =
    {
        new Layer("Canopy", new[] { "Kapokier1", "Kapokier2", "Kapokier3", "Kapokier4", "Banana5", "Banana6" },
                  -85, 1.5f, -30f, 125f, 11f, 17f, -3.0f, new Color(0.92f, 1.0f, 0.94f, 1f), true),
        new Layer("MidGround", new[] { "Banana1", "Banana2", "Banana3", "Banana4", "Rattle1", "Rattle2", "Rattle3", "Musa1", "Musa2", "Musa3" },
                  -70, 1.0f, -26f, 118f, 5f, 9f, -1.1f, new Color(0.86f, 0.94f, 0.86f, 1f), true),
        new Layer("Cliffs", new[] { "JungleCliff_0", "JungleCliff_1", "JungleCliff_2", "JungleCliff_3", "JungleCliff_4", "JungleBoulder_3" },
                  -40, 0.6f, -20f, 112f, 10f, 16f, -2.4f, new Color(0.78f, 0.82f, 0.78f, 1f), true),
        new Layer("BackShrubs", new[] { "Elephant1", "Elephant2", "Elephant3", "Elephant4", "BigFern1", "BigFern2", "BigFern3", "Asplenium3", "Shrub2", "Shrub3" },
                  -20, 0.3f, -12f, 104f, 3.0f, 5.5f, -0.05f, new Color(0.95f, 1f, 0.95f, 1f), true),
        // Rock rim sitting on the tilemap surface so the ground itself reads as jungle,
        // not placeholder tiles. Boulders are buried to a visible lip and only placed
        // where the ground raycast actually hit, so nothing floats over a pit.
        new Layer("GroundEdge", new[] { "JungleBoulder_0", "JungleBoulder_1", "JungleBoulder_2", "JungleBoulder_3" },
                  1, 0.15f, -8f, 98f, 1.6f, 3.2f, 0f, new Color(0.50f, 0.55f, 0.50f, 1f), true,
                  true, 0.20f, 0.45f),
        new Layer("GroundCover", new[] { "Grass1", "Grass2", "Grass3", "ThinFern1", "ThinFern2", "ThinFern3", "ThinFern4", "Anthurium1", "Anthurium2", "Anthurium3", "Heliconia1", "Heliconia2", "Heliconia3", "Philo1", "Philo2", "Philo3", "Shrub1", "Asplenium1", "Asplenium2", "BigFern1" },
                  2, 0f, -8f, 98f, 1.3f, 2.6f, 0.02f, new Color(1f, 1f, 1f, 1f), true),
        new Layer("Foreground", new[] { "Elephant4", "BigFern3", "ThinFern5", "Asplenium3" },
                  34, -1f, -12f, 104f, 11f, 17f, -3.6f, new Color(0.38f, 0.44f, 0.42f, 0.96f), true),
    };

    [MenuItem("Bastion Zero/Dress Opening Level With Jungle")]
    public static void BuildFromMenu()
    {
        Debug.Log(Build());
    }

    public static string Build()
    {
        var scene = SceneManager.GetActiveScene();
        var sb = new StringBuilder();

        var profile = BuildGroundProfile(scene, out var solid);
        var exclusions = new List<Vector2>();
        CollectExclusions(scene, exclusions);

        // Start from a clean slate so re-running is idempotent.
        var old = GameObject.Find("LevelDetail");
        if (old != null)
        {
            var existing = old.transform.Find("Detail_Art/Jungle");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);
        }
        RemovePlaceholderVegetation(scene, sb);

        var detailRoot = GameObject.Find("LevelDetail");
        if (detailRoot == null) detailRoot = new GameObject("LevelDetail");
        var artRoot = detailRoot.transform.Find("Detail_Art");
        if (artRoot == null)
        {
            var go = new GameObject("Detail_Art");
            go.transform.SetParent(detailRoot.transform, false);
            artRoot = go.transform;
        }
        var jungle = new GameObject("Jungle");
        jungle.transform.SetParent(artRoot, false);

        var rng = new System.Random(20261112);
        int total = 0;

        foreach (var layer in Layers)
        {
            var root = new GameObject(layer.Name);
            root.transform.SetParent(jungle.transform, false);
            int placed = 0;
            float x = layer.MinX;
            while (x < layer.MaxX)
            {
                var ok = GroundAt(profile, x, out var groundY);
                if (ok && layer.NeedsSolidGround && !SolidAt(solid, x)) ok = false;
                if (ok || !layer.NeedsGround)
                {
                    bool excluded = (layer.Name == "GroundCover" || layer.Name == "GroundEdge") && IsExcluded(exclusions, x);
                    if (!excluded)
                    {
                        var name = layer.Sprites[rng.Next(layer.Sprites.Length)];
                        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFolder + name + ".png");
                        if (sprite != null)
                        {
                            var scale = sprite.pixelsPerUnit / BatchPPU;
                            // A visible-height layer is buried so only its lip shows above the
                            // surface: rock rims must not stand up like props.
                            float y = groundY + layer.BaseOffset;
                            if (layer.MaxVisible > 0f)
                            {
                                float worldHeight = sprite.bounds.size.y * scale;
                                float visible = Mathf.Lerp(layer.MinVisible, layer.MaxVisible, (float)rng.NextDouble());
                                y = groundY + visible - worldHeight;
                            }
                            var go = new GameObject(name);
                            go.transform.SetParent(root.transform, false);
                            go.transform.position = new Vector3(
                                x + (float)(rng.NextDouble() - 0.5) * 0.6f,
                                y,
                                layer.Z);
                            go.transform.localScale = new Vector3(scale, scale, 1f);
                            var sr = go.AddComponent<SpriteRenderer>();
                            sr.sprite = sprite;
                            sr.sortingOrder = layer.SortingOrder + rng.Next(0, 3);
                            sr.color = layer.Tint;
                            sr.flipX = rng.NextDouble() < 0.5;
                            placed++;
                            total++;
                        }
                    }
                }
                x += layer.MinStep + (float)rng.NextDouble() * (layer.MaxStep - layer.MinStep);
            }
            sb.AppendLine(layer.Name + " placed=" + placed + " order=" + layer.SortingOrder);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        sb.AppendLine("total=" + total);
        var p = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "AuraDressReport.txt"));
        File.WriteAllText(p, sb.ToString());
        return "dressed total=" + total + " (see AuraDressReport.txt)";
    }

    static void RemovePlaceholderVegetation(Scene scene, StringBuilder sb)
    {
        string[] names = { "P_Fern", "P_Grass", "P_Mushroom", "P_Rock" };
        var doomed = new List<GameObject>();
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                foreach (var n in names)
                {
                    if (t.name != n) continue;
                    if (t.GetComponentsInParent<Transform>(true).Length < 1) continue;
                    var path = PathOf(t);
                    if (!path.Contains("Detail_Art")) continue;
                    doomed.Add(t.gameObject);
                    break;
                }
            }
        }
        foreach (var go in doomed)
        {
            sb.AppendLine("removed placeholder " + PathOf(go.transform));
            Object.DestroyImmediate(go);
        }
    }

    static string PathOf(Transform t)
    {
        var s = t.name;
        while (t.parent != null) { t = t.parent; s = t.name + "/" + s; }
        return s;
    }

    static void CollectExclusions(Scene scene, List<Vector2> list)
    {
        string[] guarded =
        {
            "Door", "PressurePlate", "StrikeSwitch", "CorruptedBramble", "RouteGate", "LevelExit",
            "Collectible_01", "Collectible_02", "Collectible_03", "Collectible_04", "ViolentGem"
        };
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                foreach (var g in guarded)
                {
                    if (t.name != g) continue;
                    var b = t.position;
                    list.Add(new Vector2(b.x, 1.8f));
                    break;
                }
            }
        }
        // Keep the one-way deck clear so the platform silhouette reads.
        list.Add(new Vector2(21.5f, 3.0f));
    }

    static bool IsExcluded(List<Vector2> list, float x)
    {
        foreach (var e in list)
            if (Mathf.Abs(x - e.x) < e.y) return true;
        return false;
    }

    // Sampled once, then gap-filled so background layers stay continuous across pits.
    // 'solid' keeps the raw hits so surface-hugging layers can skip the filled spans.
    static float[] BuildGroundProfile(Scene scene, out bool[] solid)
    {
        const float minX = -40f, maxX = 140f, step = 0.5f;
        int count = Mathf.CeilToInt((maxX - minX) / step) + 1;
        var arr = new float[count];
        var known = new bool[count];
        var mask = 1 << 8;
        for (int i = 0; i < count; i++)
        {
            float x = minX + i * step;
            var hit = Physics2D.Raycast(new Vector2(x, 60f), Vector2.down, 120f, mask);
            if (hit.collider != null) { arr[i] = hit.point.y; known[i] = true; }
        }
        solid = (bool[])known.Clone();
        // Carry the last known height forward, then backward for leading gaps.
        float last = 0f;
        for (int i = 0; i < count; i++)
        {
            if (known[i]) last = arr[i];
            else arr[i] = last;
        }
        last = arr[count - 1];
        for (int i = count - 1; i >= 0; i--)
        {
            if (known[i]) last = arr[i];
            else arr[i] = last;
        }
        return arr;
    }

    const float ProfileMinX = -40f, ProfileStep = 0.5f;

    static bool GroundAt(float[] profile, float x, out float y)
    {
        int i = Mathf.RoundToInt((x - ProfileMinX) / ProfileStep);
        if (i < 0) i = 0;
        if (i >= profile.Length) i = profile.Length - 1;
        y = profile[i];
        return true;
    }

    static bool SolidAt(bool[] solid, float x)
    {
        int i = Mathf.RoundToInt((x - ProfileMinX) / ProfileStep);
        if (i < 0) i = 0;
        if (i >= solid.Length) i = solid.Length - 1;
        return solid[i];
    }
}
