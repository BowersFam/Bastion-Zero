// Bastion Zero — "bring anything in and make it match Nexus" conversion tool.
//
// Renders a 3D / PBR prop through Assets/Shaders/PixelNature.shader into an
// offscreen target sized at (32 * designScale) pixels per world unit, then saves
// it as a point-filtered sprite. At its design scale the resulting sprite has
// exactly the same texel density as Nexus (32 px/unit), so imported art sits on
// the same pixel grid instead of fighting it.
//
// Menu: Bastion Zero/Bake Pixel Nature Sprites
// Programmatic: PixelNatureBaker.BakeOne("Grass1") / PixelNatureBaker.BakeAll()
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class PixelNatureBaker
{
    public const float NexusPPU = 32f;
    public static bool DebugSaveRaw = false;
    public static bool ForceOpaque = false;
    const string OutFolder = "Assets/Art/JunglePixel";
    const string ShaderPath = "Assets/Shaders/PixelNature.shader";
    const string KitRoot = "Assets/BK/PureNature_Jungle/Prefabs/";
    const float Tilt = 11f;      // degrees of downward camera pitch: shows a little leaf mass
    const float Margin = 1.06f;
    const int Supersample = 4;   // renders 4x then box-filters: alpha-tested foliage needs it
    const float ToneGamma = 1.7f;   // lifts the baked ramp into Nexus's tonal range

    public struct Recipe
    {
        public readonly string Folder;
        public readonly string Name;
        public readonly float Scale;   // design scale: sprite is placed at this scale in-scene
        public Recipe(string folder, string name, float scale) { Folder = folder; Name = name; Scale = scale; }
    }

    // Design scales assume a 2-unit-tall player and a 16-unit-tall camera view.
    public static readonly Recipe[] Recipes =
    {
        // canopy / background trees
        new Recipe("Trees", "Kapokier1", 0.55f),
        new Recipe("Trees", "Kapokier2", 0.55f),
        new Recipe("Trees", "Kapokier3", 0.45f),
        new Recipe("Trees", "Kapokier4", 0.50f),
        new Recipe("Trees", "Banana1", 1.60f),
        new Recipe("Trees", "Banana2", 1.40f),
        new Recipe("Trees", "Banana3", 0.90f),
        new Recipe("Trees", "Banana4", 0.80f),
        new Recipe("Trees", "Banana5", 0.70f),
        new Recipe("Trees", "Banana6", 0.65f),
        new Recipe("Trees", "Musa1", 2.00f),
        new Recipe("Trees", "Musa2", 2.00f),
        new Recipe("Trees", "Musa3", 1.60f),
        new Recipe("Trees", "Rattle1", 2.20f),
        new Recipe("Trees", "Rattle2", 1.60f),
        new Recipe("Trees", "Rattle3", 1.30f),

        // mid and ground cover
        new Recipe("Plants", "BigFern1", 2.20f),
        new Recipe("Plants", "BigFern2", 2.00f),
        new Recipe("Plants", "BigFern3", 1.40f),
        new Recipe("Plants", "Asplenium1", 2.60f),
        new Recipe("Plants", "Asplenium2", 2.20f),
        new Recipe("Plants", "Asplenium3", 1.80f),
        new Recipe("Plants", "Anthurium1", 2.60f),
        new Recipe("Plants", "Anthurium2", 2.40f),
        new Recipe("Plants", "Anthurium3", 1.80f),
        new Recipe("Plants", "Elephant1", 2.00f),
        new Recipe("Plants", "Elephant2", 1.30f),
        new Recipe("Plants", "Elephant3", 1.20f),
        new Recipe("Plants", "Elephant4", 1.00f),
        new Recipe("Plants", "Grass1", 3.00f),
        new Recipe("Plants", "Grass2", 2.20f),
        new Recipe("Plants", "Grass3", 1.80f),
        new Recipe("Plants", "Heliconia1", 3.00f),
        new Recipe("Plants", "Heliconia2", 2.40f),
        new Recipe("Plants", "Heliconia3", 2.40f),
        new Recipe("Plants", "Philo1", 2.40f),
        new Recipe("Plants", "Philo2", 2.00f),
        new Recipe("Plants", "Philo3", 1.60f),
        new Recipe("Plants", "Shrub1", 2.40f),
        new Recipe("Plants", "Shrub2", 2.00f),
        new Recipe("Plants", "Shrub3", 1.80f),
        new Recipe("Plants", "ThinFern1", 2.60f),
        new Recipe("Plants", "ThinFern2", 2.20f),
        new Recipe("Plants", "ThinFern3", 1.80f),
        new Recipe("Plants", "ThinFern4", 1.40f),
        new Recipe("Plants", "ThinFern5", 1.10f),
        new Recipe("Plants", "Branches", 2.00f),

        // rocks and cliffs
        new Recipe("Rocks", "Block_1", 2.40f),
        new Recipe("Rocks", "Block_2", 2.00f),
        new Recipe("Rocks", "Block_3", 2.40f),
        new Recipe("Rocks", "JungleBoulder_0", 2.00f),
        new Recipe("Rocks", "JungleBoulder_1", 1.60f),
        new Recipe("Rocks", "JungleBoulder_2", 1.20f),
        new Recipe("Rocks", "JungleBoulder_3", 1.00f),
        new Recipe("Rocks", "JungleCliff_0", 0.90f),
        new Recipe("Rocks", "JungleCliff_1", 1.00f),
        new Recipe("Rocks", "JungleCliff_2", 1.20f),
        new Recipe("Rocks", "JungleCliff_3", 1.20f),
        new Recipe("Rocks", "JungleCliff_4", 1.10f),
    };

    [MenuItem("Bastion Zero/Bake Pixel Nature Sprites")]
    public static void BakeAllFromMenu()
    {
        var report = BakeAll();
        Debug.Log(report);
    }

    public static string BakeAll()
    {
        AssetDatabase.ImportAsset(ShaderPath, ImportAssetOptions.ForceUpdate);
        var sb = new StringBuilder();
        int ok = 0, failed = 0;
        foreach (var r in Recipes)
        {
            var line = BakeOne(r, out var okOne);
            sb.AppendLine(line);
            if (okOne) ok++; else failed++;
        }
        var header = string.Format(CultureInfo.InvariantCulture,
            "baked={0} failed={1} ppu={2}\n", ok, failed, NexusPPU);
        WriteReport(header + sb);
        return header + "see AuraBakeReport.txt";
    }

    public static string BakeOneByName(string name)
    {
        foreach (var r in Recipes)
        {
            if (r.Name != name) continue;
            var line = BakeOne(r, out var ok);
            WriteReport(line);
            return line;
        }
        return "no recipe named " + name;
    }

    static string BakeOne(Recipe recipe, out bool success)
    {
        success = false;
        var prefabPath = KitRoot + recipe.Folder + "/" + recipe.Name + ".prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) return "MISSING " + prefabPath;

        var shader = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
        if (shader == null) return "MISSING SHADER " + ShaderPath;

        float ppu = NexusPPU * recipe.Scale;
        var previewScene = EditorSceneManager.NewPreviewScene();
        GameObject instance = null;
        GameObject camGo = null;
        Texture2D result = null;
        try
        {
            instance = (GameObject)Object.Instantiate(prefab);
            instance.transform.position = Vector3.zero;
            instance.transform.rotation = Quaternion.identity;
            SceneManager.MoveGameObjectToScene(instance, previewScene);

            // Collect the LOD0 renderers only, then drop the LOD groups.
            var drawSet = new List<MeshRenderer>();
            var lodGroups = instance.GetComponentsInChildren<LODGroup>(true);
            if (lodGroups.Length > 0)
            {
                foreach (var g in lodGroups)
                {
                    var lods = g.GetLODs();
                    if (lods.Length == 0) continue;
                    foreach (var r in lods[0].renderers)
                    {
                        var mr = r as MeshRenderer;
                        if (mr != null && mr.GetComponent<MeshFilter>() != null && !drawSet.Contains(mr))
                            drawSet.Add(mr);
                    }
                }
            }
            if (drawSet.Count == 0)
            {
                foreach (var mr in instance.GetComponentsInChildren<MeshRenderer>(true))
                    if (mr.GetComponent<MeshFilter>() != null) drawSet.Add(mr);
            }
            foreach (var g in lodGroups) Object.DestroyImmediate(g);
            foreach (var mr in drawSet) mr.enabled = true;
            foreach (var col in instance.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(col);

            if (drawSet.Count == 0) return "NOMESH " + recipe.Name;

            // Bounds of what we will actually draw.
            var bounds = new Bounds(instance.transform.position, Vector3.zero);
            bool first = true;
            foreach (var mr in drawSet)
            {
                var b = mr.bounds;
                if (first) { bounds = b; first = false; }
                else bounds.Encapsulate(b);
            }

            float rad = Tilt * Mathf.Deg2Rad;
            float extY = Mathf.Abs(bounds.size.y) * Mathf.Cos(rad) + Mathf.Abs(bounds.size.z) * Mathf.Sin(rad);
            float extX = Mathf.Abs(bounds.size.x);

            int w = Mathf.Max(8, Mathf.CeilToInt(extX * Margin * ppu));
            int h = Mathf.Max(8, Mathf.CeilToInt(extY * Margin * ppu));
            int ss = Supersample;
            while (ss > 1 && (w * ss > 4096 || h * ss > 4096)) ss /= 2;
            int rw = w * ss, rh = h * ss;
            if (w > 2048 || h > 2048)
                return "TOOBIG " + recipe.Name + " " + w + "x" + h;

            camGo = new GameObject("__pxcam");
            SceneManager.MoveGameObjectToScene(camGo, previewScene);
            var cam = camGo.AddComponent<Camera>();
            cam.enabled = false;
            cam.orthographic = true;
            cam.orthographicSize = (extY * Margin) * 0.5f;
            cam.aspect = (float)rw / rh;
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = bounds.size.magnitude + 200f;
            cam.transform.rotation = Quaternion.Euler(Tilt, 0f, 0f);
            cam.transform.position = bounds.center - cam.transform.forward * (bounds.extents.magnitude + 50f);

            // Converted materials, one per source material.
            var matCache = new Dictionary<Material, Material>();
            var draws = new List<KeyValuePair<MeshRenderer, Material[]>>();
            foreach (var mr in drawSet)
            {
                var mf = mr.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null) continue;
                var srcMats = mr.sharedMaterials;
                var mats = new Material[Mathf.Max(1, mf.sharedMesh.subMeshCount)];
                for (int i = 0; i < mats.Length; i++)
                {
                    var src = i < srcMats.Length ? srcMats[i] : null;
                    mats[i] = ConvertMaterial(src, shader, matCache);
                }
                draws.Add(new KeyValuePair<MeshRenderer, Material[]>(mr, mats));
            }
            if (draws.Count == 0) return "NODRAW " + recipe.Name;

            result = Render(cam, draws, rw, rh, out var path2);
            if (result == null) return "BLANK " + recipe.Name + " (" + path2 + ")";

            var big = result;
            if (DebugSaveRaw)
                File.WriteAllBytes(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "AuraRaw_" + recipe.Name + ".png")),
                                   big.EncodeToPNG());
            result = Downsample(big, w, h, ss);
            Object.DestroyImmediate(big);

            result = PostProcess(result, out int cx, out int cy, out int cw, out int ch);
            var pixels = result.GetPixels32();
            int srcW = result.width;
            var outTex = new Texture2D(cw, ch, TextureFormat.RGBA32, false, false);
            var cropped = new Color32[cw * ch];
            // The command-buffer pass renders through GL.GetGPUProjectionMatrix(proj, true),
            // which flips Y into the target; ReadPixels then hands back a top-down image.
            // Copy each source row into the mirrored destination row so the sprite is the
            // right way up (without this, every prop bakes upside down).
            for (int y = 0; y < ch; y++)
                System.Array.Copy(pixels, (cy + y) * srcW + cx, cropped, (ch - 1 - y) * cw, cw);
            outTex.SetPixels32(cropped);
            outTex.Apply(false, false);

            var png = outTex.EncodeToPNG();
            Object.DestroyImmediate(outTex);
            Object.DestroyImmediate(result);
            result = null;

            Directory.CreateDirectory(OutFolder);
            var outPath = OutFolder + "/" + recipe.Name + ".png";
            File.WriteAllBytes(outPath, png);
            AssetDatabase.ImportAsset(outPath, ImportAssetOptions.ForceUpdate);
            ApplyImportSettings(outPath, ppu);

            foreach (var m in matCache.Values) Object.DestroyImmediate(m);
            success = true;
            return string.Format(CultureInfo.InvariantCulture,
                "OK {0} {1}x{2} ppu={3:0.##} scale={4:0.##}", recipe.Name, cw, ch, ppu, recipe.Scale);
        }
        catch (System.Exception e)
        {
            return "ERROR " + recipe.Name + " " + e.GetType().Name + ": " + e.Message;
        }
        finally
        {
            if (result != null) Object.DestroyImmediate(result);
            if (camGo != null) Object.DestroyImmediate(camGo);
            if (instance != null) Object.DestroyImmediate(instance);
            EditorSceneManager.ClosePreviewScene(previewScene);
        }
    }

    // Draws the prop with an immediate command buffer (pipeline agnostic).
    static Texture2D Render(Camera cam, List<KeyValuePair<MeshRenderer, Material[]>> draws,
                            int w, int h, out string note)
    {
        note = "commandbuffer";
        var rt = RenderTexture.GetTemporary(w, h, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        var prevActive = RenderTexture.active;
        Texture2D tex = null;
        try
        {
            var cmd = new CommandBuffer();
            cmd.SetRenderTarget(rt);
            cmd.ClearRenderTarget(true, true, new Color(0f, 0f, 0f, 0f));
            cmd.SetViewProjectionMatrices(cam.worldToCameraMatrix,
                                          GL.GetGPUProjectionMatrix(cam.projectionMatrix, true));
            foreach (var pair in draws)
            {
                var mr = pair.Key;
                var mf = mr.GetComponent<MeshFilter>();
                int subs = Mathf.Min(mf.sharedMesh.subMeshCount, pair.Value.Length);
                for (int s = 0; s < subs; s++)
                    cmd.DrawRenderer(mr, pair.Value[s], s, 0);
            }
            Graphics.ExecuteCommandBuffer(cmd);

            RenderTexture.active = rt;
            tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0, false);
            tex.Apply(false, false);

            if (!HasOpaquePixels(tex))
            {
                // Fallback: regular camera render into the target.
                note = "camera.render";
                Object.DestroyImmediate(tex);
                tex = null;
                cam.targetTexture = rt;
                cam.enabled = true;
                cam.Render();
                cam.enabled = false;
                cam.targetTexture = null;
                RenderTexture.active = rt;
                tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
                tex.ReadPixels(new Rect(0, 0, w, h), 0, 0, false);
                tex.Apply(false, false);
                if (!HasOpaquePixels(tex))
                {
                    Object.DestroyImmediate(tex);
                    tex = null;
                }
            }
        }
        catch (System.Exception e)
        {
            note = note + " EX " + e.Message;
            if (tex != null) { Object.DestroyImmediate(tex); tex = null; }
        }
        finally
        {
            RenderTexture.active = prevActive;
            RenderTexture.ReleaseTemporary(rt);
        }
        return tex;
    }

    static bool HasOpaquePixels(Texture2D tex)
    {
        var px = tex.GetPixels32();
        for (int i = 0; i < px.Length; i++)
            if (px[i].a > 32) return true;
        return false;
    }

    static readonly string[] AlbedoProps = { "_MainTex", "_BaseMap", "_Diffuse", "_Albedo", "_AlbedoMap", "_Texture0" };

    static readonly string[] RampProps = { "_ColorVariation", "_Color01", "_Color02", "_Color1", "_Color2" };

    // True when the albedo map carries no hue of its own, which in this kit means the
    // material's colour ramp (or the mesh's vertex colours) supplies the real colour.
    // Coloured albedos must keep their own hue: their ramps are shadow tints, not bases.
    static bool AlbedoIsNeutral(Texture tex)
    {
        var t2 = tex as Texture2D;
        if (t2 == null) return true;
        var rt = RenderTexture.GetTemporary(16, 16, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        var prev = RenderTexture.active;
        Graphics.Blit(t2, rt);
        RenderTexture.active = rt;
        var cp = new Texture2D(16, 16, TextureFormat.RGBA32, false, false);
        cp.ReadPixels(new Rect(0, 0, 16, 16), 0, 0);
        cp.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        var px = cp.GetPixels32();
        Object.DestroyImmediate(cp);
        float r = 0f, g = 0f, b = 0f; int n = 0;
        foreach (var c in px)
        {
            if (c.a < 32) continue;
            r += c.r; g += c.g; b += c.b; n++;
        }
        if (n == 0) return true;
        r /= n; g /= n; b /= n;
        float mx = Mathf.Max(r, Mathf.Max(g, b));
        float mn = Mathf.Min(r, Mathf.Min(g, b));
        if (mx <= 1f) return true;
        return (mx - mn) / mx < 0.12f;
    }

    static Material ConvertMaterial(Material src, Shader shader, Dictionary<Material, Material> cache)
    {
        if (src == null) return null;
        if (cache.TryGetValue(src, out var cached)) return cached;

        var m = new Material(shader);
        m.hideFlags = HideFlags.HideAndDontSave;
        Texture tex = null;
        Color col = Color.white;
        Vector2 tiling = Vector2.one, offset = Vector2.zero;
        // The kit's shaders name their albedo differently per family:
        // BK/Vegetation Leaves uses _Diffuse, most others use _MainTex.
        string albedoProp = null;
        foreach (var candidate in AlbedoProps)
        {
            if (!src.HasProperty(candidate)) continue;
            var t = src.GetTexture(candidate);
            if (t == null) continue;
            tex = t;
            albedoProp = candidate;
            break;
        }
        if (tex == null) tex = src.mainTexture;
        if (src.HasProperty("_BaseColor")) col = src.GetColor("_BaseColor");
        else if (src.HasProperty("_Color")) col = src.GetColor("_Color");

        // Several families in this kit ship a near-white albedo map and put the real
        // hue in a shader colour ramp (BK/Grass lerps _Color01 -> _Color02, the leaf
        // shader uses _ColorVariation, and some meshes carry the tint in vertex colour).
        // Take the most saturated ramp colour so foliage bakes green instead of grey.
        Color best = Color.white; float bestScore = 0f;
        foreach (var rampProp in RampProps)
        {
            if (!src.HasProperty(rampProp)) continue;
            var c = src.GetColor(rampProp);
            c.a = 1f;
            float mx = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
            float mn = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
            if (mx <= 0.001f) continue;
            float score = (mx - mn) / mx * mx;
            if (score > bestScore) { bestScore = score; best = c; }
        }
        if (bestScore > 0.05f && AlbedoIsNeutral(tex)) col = best;

        if (albedoProp != null)
        {
            tiling = src.GetTextureScale(albedoProp);
            offset = src.GetTextureOffset(albedoProp);
        }

        // The kit uses per-instance colour variation; keep the brightest, most
        // representative tint rather than an accidental one-off.
        if (col.r < 0.2f && col.g < 0.2f && col.b < 0.2f) col = Color.white;
        col.a = 1f;

        m.SetTexture("_MainTex", tex);
        m.SetTextureScale("_MainTex", tiling);
        m.SetTextureOffset("_MainTex", offset);
        m.SetColor("_Color", col);
        m.SetFloat("_AlphaCutoff", ForceOpaque ? 0f : 0.4f);
        cache[src] = m;
        return m;
    }

    // Alpha-weighted box filter: resolves the alpha-tested foliage into a solid
    // silhouette at the target pixel grid, then re-snaps to the colour ramp so the
    // averaging blur does not introduce off-palette values.
    public static string LastDownsampleInfo = "";

    static Texture2D Downsample(Texture2D src, int tw, int th, int factor)
    {
        var sp = src.GetPixels32();
        int sw = src.width;
        int opaque = 0, sumOpaque = 0;
        for (int i = 0; i < sp.Length; i++) if (sp[i].a > 32) opaque++;
        var outp = new Color32[tw * th];
        const float levels = 14f;
        int kept = 0;
        LastDownsampleInfo = "src=" + src.width + "x" + src.height + " px=" + sp.Length + " opaqueSrc=" + opaque;
        for (int y = 0; y < th; y++)
        {
            for (int x = 0; x < tw; x++)
            {
                float sumA = 0f, r = 0f, g = 0f, b = 0f;
                for (int sy = 0; sy < factor; sy++)
                {
                    int py = y * factor + sy;
                    for (int sx = 0; sx < factor; sx++)
                    {
                        int pxi = py * sw + x * factor + sx;
                        var c = sp[pxi];
                        float a = c.a / 255f;
                        sumA += a;
                        r += c.r * a;
                        g += c.g * a;
                        b += c.b * a;
                    }
                }
                float cov = sumA / (factor * factor);
                if (sumA <= 0f || cov < 0.5f)
                {
                    outp[y * tw + x] = new Color32(0, 0, 0, 0);
                    continue;
                }
                kept++;
                sumOpaque++;
                float inv = 1f / sumA;
                float lr = Mathf.Pow(Mathf.Clamp01(r * inv / 255f), 1f / ToneGamma);
                float lg = Mathf.Pow(Mathf.Clamp01(g * inv / 255f), 1f / ToneGamma);
                float lb = Mathf.Pow(Mathf.Clamp01(b * inv / 255f), 1f / ToneGamma);
                byte qr = (byte)Mathf.Clamp(Mathf.Round(lr * levels) / levels * 255f, 0f, 255f);
                byte qg = (byte)Mathf.Clamp(Mathf.Round(lg * levels) / levels * 255f, 0f, 255f);
                byte qb = (byte)Mathf.Clamp(Mathf.Round(lb * levels) / levels * 255f, 0f, 255f);
                outp[y * tw + x] = new Color32(qr, qg, qb, 255);
            }
        }
        var tex = new Texture2D(tw, th, TextureFormat.RGBA32, false, false);
        tex.SetPixels32(outp);
        tex.Apply(false, false);
        LastDownsampleInfo += " factor=" + factor + " kept=" + kept + "/" + (tw * th);
        return tex;
    }

    // Darker 1px rim inside the silhouette plus a solid 1px outline outside it.
    static Texture2D PostProcess(Texture2D src, out int cx, out int cy, out int cw, out int ch)
    {
        int w = src.width, h = src.height;
        var px = src.GetPixels32();
        var outp = (Color32[])px.Clone();
        var rim = new Color32(22, 32, 26, 255);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int i = y * w + x;
                bool solid = px[i].a > 110;
                if (solid)
                {
                    bool edge = false;
                    if (x == 0 || y == 0 || x == w - 1 || y == h - 1) edge = true;
                    else
                    {
                        if (px[i - 1].a <= 110 || px[i + 1].a <= 110 ||
                            px[i - w].a <= 110 || px[i + w].a <= 110) edge = true;
                    }
                    if (edge)
                    {
                        outp[i] = new Color32(
                            (byte)(px[i].r * 0.55f),
                            (byte)(px[i].g * 0.58f),
                            (byte)(px[i].b * 0.55f), 255);
                    }
                }
                else if (px[i].a < 40)
                {
                    bool near = false;
                    if (x > 0 && px[i - 1].a > 110) near = true;
                    if (!near && x < w - 1 && px[i + 1].a > 110) near = true;
                    if (!near && y > 0 && px[i - w].a > 110) near = true;
                    if (!near && y < h - 1 && px[i + w].a > 110) near = true;
                    if (near) outp[i] = rim;
                }
            }
        }

        // Crop to the used area with 1px padding.
        int minX = w, minY = h, maxX = -1, maxY = -1;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (outp[y * w + x].a <= 32) continue;
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }
        }
        if (maxX < 0) { minX = 0; minY = 0; maxX = w - 1; maxY = h - 1; }
        minX = Mathf.Max(0, minX - 1); minY = Mathf.Max(0, minY - 1);
        maxX = Mathf.Min(w - 1, maxX + 1); maxY = Mathf.Min(h - 1, maxY + 1);
        cx = minX; cy = minY; cw = maxX - minX + 1; ch = maxY - minY + 1;

        var outTex = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
        outTex.SetPixels32(outp);
        outTex.Apply(false, false);
        return outTex;
    }

    static void ApplyImportSettings(string path, float ppu)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.filterMode = FilterMode.Point;
        ti.mipmapEnabled = false;
        ti.alphaIsTransparency = true;
        ti.wrapMode = TextureWrapMode.Clamp;
        ti.npotScale = TextureImporterNPOTScale.None;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.maxTextureSize = 2048;
        ti.spritePixelsPerUnit = ppu;

        var settings = new TextureImporterSettings();
        ti.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteExtrude = 0;
        ti.SetTextureSettings(settings);
        ti.SaveAndReimport();
    }

    static void WriteReport(string text)
    {
        var p = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "AuraBakeReport.txt"));
        File.WriteAllText(p, text);
    }
}
