using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Turns a freshly generated sprite sheet (art on a flat, opaque backdrop) into the alpha sheet
/// <see cref="NexusArtTools"/> measures and slices.
///
/// <para>The art model always returns an opaque image, but the slicer keys its cell bounds on
/// <c>alpha &gt; 8</c>. This steps the backdrop colour out of the alpha channel instead of asking
/// the model to edit the picture again, so a generated sheet reaches the slicer byte-identical to
/// what was drawn — no second pass that could smear the pinstripes or nudge a pose.</para>
///
/// <para>The backdrop is sampled from eight points on the border rather than assumed to be pure
/// white, so a sheet that came back slightly warm or slightly grey still keys cleanly. Pixels
/// within <see cref="SolidDistance"/> of that colour are opaque (the armour's own light greys are
/// well past it); pixels within <see cref="ClearDistance"/> are fully transparent; the band between
/// ramps linearly, which keeps the antialiased silhouette edge from ringing white.</para>
/// </summary>
public static class NexusGridKeyer
{
    public const string WorkRoot = "Assets/Art/Characters/Nexus/HD2D/_work";
    public const string GeneratedRoot = "Assets/Generated_Images";

    /// <summary>The Cinder-Mite sheet. It is drawn in play, so the edge-matte repair covers it too.</summary>
    public const string MitePath = GeneratedRoot + "/cinder_mite.png";

    /// <summary>Colour distance from the backdrop at or below which a pixel is fully transparent.</summary>
    private const float ClearDistance = 8f;

    /// <summary>Colour distance at or above which a pixel is fully opaque.</summary>
    private const float SolidDistance = 24f;

    /// <summary>
    /// The three combo sheets as generated this pass: source file and the state it belongs to.
    /// The third grid is saved under the generator's fallback name, hence the rename here.
    /// </summary>
    private static readonly (string Source, string State)[] ComboSheets =
    {
        ("body_attack_g1_grid.png", "attack_g1"),
        ("body_attack_g2_grid.png", "attack_g2"),
        ("image_2.png", "attack_g3"),
    };

    [MenuItem("Bastion Zero/Art/Key Generated Combo Grids")]
    public static void KeyComboGrids()
    {
        int keyed = 0;

        foreach ((string source, string state) in ComboSheets)
        {
            string from = $"{GeneratedRoot}/{source}";
            string to = $"Assets/Art/Characters/Nexus/HD2D/_work/body_{state}_grid_edited.png";

            if (Key(from, to)) keyed++;
        }

        AssetDatabase.Refresh();
        Debug.Log($"[NexusGridKeyer] {keyed}/{ComboSheets.Length} combo grids keyed.");
    }

    /// <summary>Keys <paramref name="sourcePath"/>'s backdrop out and writes <paramref name="destPath"/>.</summary>
    public static bool Key(string sourcePath, string destPath)
    {
        string fullSource = Path.Combine(Directory.GetCurrentDirectory(), sourcePath);
        if (!File.Exists(fullSource))
        {
            Debug.LogError($"[NexusGridKeyer] missing sheet {sourcePath}.");
            return false;
        }

        var loaded = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!loaded.LoadImage(File.ReadAllBytes(fullSource)))
        {
            Debug.LogError($"[NexusGridKeyer] could not decode {sourcePath}.");
            return false;
        }

        int width = loaded.width;
        int height = loaded.height;
        Color32[] pixels = loaded.GetPixels32();

        Color32 backdrop = SampleBackdrop(pixels, width, height);
        float solid = SolidDistance;
        float clear = ClearDistance;
        float span = Mathf.Max(1f, solid - clear);

        int cleared = 0;

        for (int i = 0; i < pixels.Length; i++)
        {
            Color32 pixel = pixels[i];
            // Colour distance to the backdrop, measured on the worst channel: a keyed backdrop is
            // the only thing on a generated sheet that sits that close to its own border colour.
            int distance = Mathf.Max(
                Mathf.Abs(pixel.r - backdrop.r),
                Mathf.Max(Mathf.Abs(pixel.g - backdrop.g), Mathf.Abs(pixel.b - backdrop.b)));

            if (distance >= solid) continue;

            if (distance <= clear)
            {
                pixel.a = 0;
                cleared++;
            }
            else
            {
                // Ramp the alpha so the antialiased rim fades instead of stepping.
                pixel.a = (byte)Mathf.RoundToInt(255f * (distance - clear) / span);
            }

            pixels[i] = pixel;
        }

        BleedEdgeColour(pixels, width, height, 24);

        var keyed = new Texture2D(width, height, TextureFormat.RGBA32, false);
        keyed.SetPixels32(pixels);
        keyed.Apply();

        string fullDest = Path.Combine(Directory.GetCurrentDirectory(), destPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullDest));
        File.WriteAllBytes(fullDest, keyed.EncodeToPNG());

        Object.DestroyImmediate(loaded);
        Object.DestroyImmediate(keyed);

        AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceSynchronousImport);

        Debug.Log(
            $"[NexusGridKeyer] {destPath}: {width}×{height}, backdrop {backdrop.r},{backdrop.g},{backdrop.b}, " +
            $"{cleared} pixels cleared.");

        return true;
    }

    /// <summary>
    /// Steps the backdrop colour out of the rim pixels as well as the backdrop itself.
    ///
    /// <para>The alpha ramp above fades the silhouette correctly but leaves every rimmed pixel
    /// carrying the colour it had on the sheet — the near-white backdrop. Blending that in draws a
    /// pale halo over the figure, which is why an attack frame showed a white fringe that the idle
    /// frame did not. Each pixel still carrying partial alpha takes the average of its opaque
    /// neighbours instead. Alpha is never touched, so the silhouette, the union bounds and the
    /// slice registration are all unchanged.</para>
    /// </summary>
    private static void BleedEdgeColour(Color32[] pixels, int width, int height, int passes)
    {
        var known = new bool[pixels.Length];
        for (int i = 0; i < pixels.Length; i++) known[i] = pixels[i].a == 255;

        var touched = new List<int>();
        var colours = new List<Color32>();

        for (int pass = 0; pass < passes; pass++)
        {
            touched.Clear();
            colours.Clear();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = y * width + x;
                    if (known[i]) continue;

                    int r = 0, g = 0, b = 0, n = 0;
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int ny = y + dy;
                        if (ny < 0 || ny >= height) continue;

                        for (int dx = -1; dx <= 1; dx++)
                        {
                            if (dx == 0 && dy == 0) continue;
                            int nx = x + dx;
                            if (nx < 0 || nx >= width) continue;

                            int j = ny * width + nx;
                            if (!known[j]) continue;

                            r += pixels[j].r;
                            g += pixels[j].g;
                            b += pixels[j].b;
                            n++;
                        }
                    }

                    if (n == 0) continue;

                    touched.Add(i);
                    colours.Add(new Color32((byte)(r / n), (byte)(g / n), (byte)(b / n), pixels[i].a));
                }
            }

            if (touched.Count == 0) break;

            for (int k = 0; k < touched.Count; k++)
            {
                pixels[touched[k]] = colours[k];
                known[touched[k]] = true;
            }
        }

        // Any pixel still unclaimed is far from the figure; give it the figure's own colour so no
        // sample of this sheet can ever carry the backdrop into a filtered or mip-mapped draw. Only
        // the colour changes: a faint pixel far from the figure keeps the alpha it already had.
        long r2 = 0, g2 = 0, b2 = 0;
        int opaque = 0;
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].a != 255) continue;
            r2 += pixels[i].r;
            g2 += pixels[i].g;
            b2 += pixels[i].b;
            opaque++;
        }

        if (opaque == 0) return;

        byte flatR = (byte)(r2 / opaque);
        byte flatG = (byte)(g2 / opaque);
        byte flatB = (byte)(b2 / opaque);

        for (int i = 0; i < pixels.Length; i++)
        {
            if (known[i]) continue;
            pixels[i] = new Color32(flatR, flatG, flatB, pixels[i].a);
        }
    }

    /// <summary>Fully opaque, partially transparent and fully clear pixel counts of one image.</summary>
    public struct AlphaCounts
    {
        public int Opaque;
        public int Semi;
        public int Clear;

        public static AlphaCounts Of(Color32[] pixels)
        {
            var counts = new AlphaCounts();
            foreach (Color32 pixel in pixels)
            {
                if (pixel.a == 255) counts.Opaque++;
                else if (pixel.a == 0) counts.Clear++;
                else counts.Semi++;
            }

            return counts;
        }

        public bool SameAs(AlphaCounts other) =>
            Opaque == other.Opaque && Semi == other.Semi && Clear == other.Clear;

        public override string ToString() => $"opaque {Opaque} / semi {Semi} / clear {Clear}";
    }

    /// <summary>
    /// Every sprite the edge-matte repair covers: each body and wing frame the hero draws, then the
    /// Cinder-Mite. The _work grids and _ref sheets are sources, never drawn, so they are left alone.
    /// </summary>
    public static List<string> EdgeMattePaths()
    {
        List<string> paths = NexusArtTools.FramePaths();
        paths.Add(MitePath);
        return paths;
    }

    [MenuItem("Bastion Zero/Art/Repair Sprite Edge Mattes")]
    public static string RepairAllEdgeMattes() => RepairEdgeMattes(EdgeMattePaths());

    /// <summary>
    /// Rewrites each image in place with the rim and backdrop colour bled in from its own opaque
    /// pixels, so neither a mip level nor a filtered sample can pull a pale or black fringe in from
    /// the transparent area. Alpha is never written, and that is checked rather than assumed: each
    /// file is read back from disk, and its opaque / semi-transparent / clear counts and every alpha
    /// byte must match the original. Safe to re-run — the bleed reads only opaque pixels, so a second
    /// pass writes the same bytes.
    /// </summary>
    public static string RepairEdgeMattes(IReadOnlyList<string> assetPaths)
    {
        int repaired = 0;
        int alphaKept = 0;
        var problems = new List<string>();
        var total = new AlphaCounts();

        foreach (string assetPath in assetPaths)
        {
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
            if (!File.Exists(fullPath))
            {
                problems.Add($"{assetPath}: missing");
                continue;
            }

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!texture.LoadImage(File.ReadAllBytes(fullPath)))
                {
                    problems.Add($"{assetPath}: could not decode");
                    continue;
                }

                Color32[] pixels = texture.GetPixels32();
                AlphaCounts before = AlphaCounts.Of(pixels);
                var alphaBefore = new byte[pixels.Length];
                for (int i = 0; i < pixels.Length; i++) alphaBefore[i] = pixels[i].a;

                BleedEdgeColour(pixels, texture.width, texture.height, 24);

                texture.SetPixels32(pixels);
                texture.Apply();
                File.WriteAllBytes(fullPath, texture.EncodeToPNG());

                // The check reads the written file, not the buffer that was encoded.
                if (!texture.LoadImage(File.ReadAllBytes(fullPath)))
                {
                    problems.Add($"{assetPath}: could not decode after writing");
                    continue;
                }

                Color32[] written = texture.GetPixels32();
                AlphaCounts after = AlphaCounts.Of(written);

                bool identical = written.Length == alphaBefore.Length;
                for (int i = 0; identical && i < written.Length; i++) identical = written[i].a == alphaBefore[i];

                Debug.Log(
                    $"[NexusGridKeyer] edge matte {assetPath}: before {before}; after {after}" +
                    (identical ? "" : " — ALPHA CHANGED"));

                if (!before.SameAs(after) || !identical)
                    problems.Add($"{assetPath}: before {before}, after {after}, alpha identical {identical}");
                else
                    alphaKept++;

                total.Opaque += after.Opaque;
                total.Semi += after.Semi;
                total.Clear += after.Clear;
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            repaired++;
        }

        AssetDatabase.Refresh();

        string summary =
            $"[NexusGridKeyer] edge matte repaired on {repaired}/{assetPaths.Count} images; " +
            $"alpha unchanged on {alphaKept}/{repaired}; totals {total}." +
            (problems.Count == 0 ? "" : " PROBLEMS: " + string.Join(" | ", problems));

        if (problems.Count == 0) Debug.Log(summary);
        else Debug.LogError(summary);

        return summary;
    }

    /// <summary>The average of eight border samples — the backdrop colour the sheet was drawn on.</summary>
    private static Color32 SampleBackdrop(Color32[] pixels, int width, int height)
    {
        int inset = Mathf.Max(2, Mathf.Min(width, height) / 64);

        Vector2Int[] points =
        {
            new Vector2Int(inset, inset),
            new Vector2Int(width / 2, inset),
            new Vector2Int(width - 1 - inset, inset),
            new Vector2Int(inset, height / 2),
            new Vector2Int(width - 1 - inset, height / 2),
            new Vector2Int(inset, height - 1 - inset),
            new Vector2Int(width / 2, height - 1 - inset),
            new Vector2Int(width - 1 - inset, height - 1 - inset),
        };

        int r = 0, g = 0, b = 0;

        foreach (Vector2Int point in points)
        {
            Color32 pixel = pixels[point.y * width + point.x];
            r += pixel.r;
            g += pixel.g;
            b += pixel.b;
        }

        int count = points.Length;
        return new Color32((byte)(r / count), (byte)(g / count), (byte)(b / count), 255);
    }
}
