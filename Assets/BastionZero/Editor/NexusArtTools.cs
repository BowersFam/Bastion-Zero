#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// HD-2D sprite pipeline for the hero.
///
/// The art model returns a grid of poses. This measures the union bounding box of the whole
/// grid once, then maps every cell through <b>one</b> scale and offset. Per-frame bounding-box
/// normalisation is deliberately not used: rescaling each pose to its own extents makes the
/// figure breathe in and out between frames, which reads as stutter.
///
/// The design contract lives in
/// <c>Assets/Art/Characters/Nexus/HD2D/_ref/design_spec.md</c>.
/// </summary>
public static class NexusArtTools
{
    public const string WorkRoot = "Assets/Art/Characters/Nexus/HD2D/_work";
    public const string BodyOut = "Assets/Art/Characters/Nexus/HD2D";
    public const string WingOut = "Assets/Art/Characters/Nexus/HD2D/wings";

    public const string WinglessClips = "Assets/Animations/Unwinged";
    public const string BodyBindingPath = "VisualRoot";

    public const int PixelsPerUnit = 128;
    public const int BodyFrameSize = 256;
    public const int WingFrameSize = 512;
    public const int SampleRate = 12;

    /// <summary>Design size of the body in the output frame: 230 px / 128 = 1.797 u.</summary>
    public const float BodyTargetHeight = 230f;

    /// <summary>
    /// The standing grid's measured union height. Every body state is sliced with this, so all of
    /// them render at one identical scale (0.4733). A state that measures its own span changes the
    /// hero's size when it plays — the second stutter source after per-frame normalisation.
    /// </summary>
    public const float BodyReferenceSpan = 486f;

    /// <summary>Design wingspan in the output frame: 358 px / 128 = 2.797 u.</summary>
    public const float WingTargetSpan = 358f;

    /// <summary>Feet land here in the frame: (14 - 128) / 128 = -0.891 u, matching GroundCheck.</summary>
    public const int BodyAnchorY = 14;

    /// <summary>
    /// Wings' union centre lands here: (346 - 256) / 128 = +0.70 u above the hero origin, so a
    /// 2 u tall wing pair spans roughly -0.3..+1.7 u behind the shoulders.
    /// </summary>
    public const int WingAnchorY = 346;

    // Wing cells are 512×384: two across, four down.
    public const int WingCellWidth = 512;
    public const int WingCellHeight = 384;

    /// <summary>Where a set's cells sit in the grid and how they land in the output frame.</summary>
    public class Layout
    {
        public string Set;
        public string State;
        public int Frames;
        public int Cols;
        public int Rows;
        public int CellW;
        public int CellH;
        public int OutSize;
        public string OutFolder;

        /// <summary>Design size of the measured reference span in output pixels.</summary>
        public float TargetSpan;
        /// <summary>True: the reference span is the union's height. False: its width.</summary>
        public bool SpanIsHeight;
        /// <summary>Where the reference point lands in the output frame.</summary>
        public int AnchorX;
        public int AnchorY;
        /// <summary>True: vertical reference is the union's lowest row (feet). False: its centre.</summary>
        public bool AnchorFeet;
        /// <summary>True: horizontal reference is each cell's centre line. False: the union's centre.</summary>
        public bool HorizontalFromCellCentre;
        /// <summary>Shared reference span in cell pixels. 0 measures this grid instead.</summary>
        public float ReferenceSpan;

        public int GridWidth => Cols * CellW;
        public int GridHeight => Rows * CellH;

        public override string ToString() =>
            $"{Set}/{State}: {Frames}f {Cols}×{Rows} cells {CellW}×{CellH} → {OutSize}px " +
            $"(target {TargetSpan}px, span {(ReferenceSpan > 0 ? ReferenceSpan.ToString("0") : "measured")})";
    }

    public static Layout BodyLayout(string state, int frames)
    {
        var layout = new Layout
        {
            Set = "body",
            State = state,
            Frames = frames,
            CellH = 512,
            OutSize = BodyFrameSize,
            OutFolder = $"{BodyOut}/{state}",
            TargetSpan = BodyTargetHeight,
            SpanIsHeight = true,
            AnchorX = BodyFrameSize / 2,
            AnchorY = BodyAnchorY,
            AnchorFeet = true,
            HorizontalFromCellCentre = true,
        };

        switch (frames)
        {
            case 8:
                layout.Cols = 4;
                layout.Rows = 2;
                layout.CellW = 384;
                break;
            case 6:
                layout.Cols = 3;
                layout.Rows = 2;
                layout.CellW = 512;
                break;
            case 9:
                layout.Cols = 3;
                layout.Rows = 3;
                layout.CellW = 512;
                break;
            default:
                layout.Cols = 2;
                layout.Rows = 2;
                layout.CellW = 512;
                break;
        }

        return layout;
    }

    public static Layout WingLayout(string state, int frames)
    {
        return new Layout
        {
            Set = "wings",
            State = state,
            Frames = frames,
            Cols = 2,
            Rows = 4,
            CellW = WingCellWidth,
            CellH = WingCellHeight,
            OutSize = WingFrameSize,
            OutFolder = $"{WingOut}/{state}",
            TargetSpan = WingTargetSpan,
            SpanIsHeight = false,
            AnchorX = WingFrameSize / 2,
            AnchorY = WingAnchorY,
            AnchorFeet = false,
            HorizontalFromCellCentre = false,
        };
    }

    public static string EditedGridPath(Layout layout) =>
        $"{WorkRoot}/{layout.Set}_{layout.State}_grid_edited.png";

    // ── Measurement ─────────────────────────────────────────────────────────────────

    public struct CellBox
    {
        public int MinX, MaxX, MinY, MaxY;
        public bool Empty;
        public int Width => MaxX - MinX + 1;
        public int Height => MaxY - MinY + 1;
    }

    public class GridMeasure
    {
        public CellBox[] Cells;
        public int MinX, MaxX, MinY, MaxY;
        public bool Empty;
        public int Width => MaxX - MinX + 1;
        public int Height => MaxY - MinY + 1;
        public float CentreX => (MinX + MaxX) * 0.5f;
        public float CentreY => (MinY + MaxY) * 0.5f;
        /// <summary>Height for a body set, width for a wing set.</summary>
        public float Span(bool height) => height ? Height : Width;
    }

    /// <summary>Reads the edited grid and measures every cell plus the union, in cell-local pixels.</summary>
    public static GridMeasure Analyse(Layout layout, out int gridWidth, out int gridHeight)
    {
        Color32[] grid = ReadPng(EditedGridPath(layout), out gridWidth, out gridHeight);
        if (grid == null) return null;

        if (gridWidth != layout.GridWidth || gridHeight != layout.GridHeight)
        {
            Debug.LogError(
                $"[NexusArtTools] {layout.Set}/{layout.State}: grid is {gridWidth}×{gridHeight}, " +
                $"the layout needs {layout.GridWidth}×{layout.GridHeight}. {layout}");
            return null;
        }

        var measure = new GridMeasure
        {
            Cells = new CellBox[layout.Frames],
            MinX = int.MaxValue,
            MaxX = -1,
            MinY = int.MaxValue,
            MaxY = -1,
        };

        for (int i = 0; i < layout.Frames; i++)
        {
            CellBox box = MeasureCell(grid, gridWidth, layout, i);
            measure.Cells[i] = box;

            if (box.Empty) continue;

            if (box.MinX < measure.MinX) measure.MinX = box.MinX;
            if (box.MaxX > measure.MaxX) measure.MaxX = box.MaxX;
            if (box.MinY < measure.MinY) measure.MinY = box.MinY;
            if (box.MaxY > measure.MaxY) measure.MaxY = box.MaxY;
        }

        measure.Empty = measure.MaxX < 0;
        return measure;
    }

    private static CellBox MeasureCell(Color32[] grid, int gridWidth, Layout layout, int index)
    {
        var box = new CellBox
        {
            MinX = int.MaxValue,
            MaxX = -1,
            MinY = int.MaxValue,
            MaxY = -1,
        };

        int cellX = (index % layout.Cols) * layout.CellW;
        int rowFromBottom = layout.Rows - 1 - (index / layout.Cols);
        int cellY = rowFromBottom * layout.CellH;

        for (int y = 0; y < layout.CellH; y++)
        {
            int row = (cellY + y) * gridWidth + cellX;

            for (int x = 0; x < layout.CellW; x++)
            {
                int at = row + x;
                if (at < 0 || at >= grid.Length) continue;
                if (grid[at].a <= 8) continue;

                if (x < box.MinX) box.MinX = x;
                if (x > box.MaxX) box.MaxX = x;
                if (y < box.MinY) box.MinY = y;
                if (y > box.MaxY) box.MaxY = y;
            }
        }

        box.Empty = box.MaxX < 0;
        return box;
    }

    // ── Slicing ─────────────────────────────────────────────────────────────────────

    /// <summary>Reports the measured union and where every frame would land. Writes nothing.</summary>
    public static bool Validate(Layout layout)
    {
        GridMeasure measure = Analyse(layout, out _, out _);
        if (measure == null) return false;

        if (measure.Empty)
        {
            Debug.LogError($"[NexusArtTools] validate {layout.Set}/{layout.State}: no opaque pixels.");
            return false;
        }

        float span = layout.ReferenceSpan > 0f
            ? layout.ReferenceSpan
            : measure.Span(layout.SpanIsHeight);

        Debug.Log(
            $"[NexusArtTools] validate {layout.Set}/{layout.State}: union x{measure.MinX}..{measure.MaxX} " +
            $"y{measure.MinY}..{measure.MaxY} {measure.Width}×{measure.Height}, span {span:0.#} px " +
            $"→ scale {(layout.TargetSpan / Mathf.Max(1f, span)):0.####}");

        bool ok = true;

        for (int i = 0; i < measure.Cells.Length; i++)
        {
            CellBox box = measure.Cells[i];
            if (box.Empty)
            {
                Debug.LogError($"[NexusArtTools] validate {layout.Set}/{layout.State} {i:00}: cell is empty.");
                ok = false;
                continue;
            }

            int minY = MapToFrame(box.MinY, measure, layout, span, true);
            int maxY = MapToFrame(box.MaxY, measure, layout, span, true);
            int minX = MapToFrame(box.MinX, measure, layout, span, false);
            int maxX = MapToFrame(box.MaxX, measure, layout, span, false);

            string note = "";
            if (box.MinX <= 1 || box.MaxX >= layout.CellW - 2 ||
                box.MinY <= 1 || box.MaxY >= layout.CellH - 2)
                note += " CLIPPED-AT-CELL-EDGE";

            Debug.Log(
                $"[NexusArtTools] validate {layout.Set}/{layout.State} {i:00}: " +
                $"cell x{box.MinX}..{box.MaxX} y{box.MinY}..{box.MaxY} {box.Width}×{box.Height} " +
                $"→ frame x{minX}..{maxX} y{minY}..{maxY}{note}");
        }

        Debug.Log($"[NexusArtTools] validate {layout.Set}/{layout.State} → {(ok ? "OK" : "NEEDS REGENERATION")}");
        return ok;
    }

    /// <summary>Cuts the edited grid into frames through one shared mapping and applies sprite settings.</summary>
    public static bool Slice(Layout layout)
    {
        GridMeasure measure = Analyse(layout, out int gridWidth, out _);
        if (measure == null) return false;

        if (measure.Empty)
        {
            Debug.LogError($"[NexusArtTools] {layout.Set}/{layout.State}: grid has no opaque pixels.");
            return false;
        }

        float span = layout.ReferenceSpan > 0f
            ? layout.ReferenceSpan
            : measure.Span(layout.SpanIsHeight);

        float scale = layout.TargetSpan / Mathf.Max(1f, span);

        Color32[] grid = ReadPng(EditedGridPath(layout), out _, out _);
        EnsureFolder(layout.OutFolder);

        for (int i = 0; i < layout.Frames; i++)
        {
            int cellX = (i % layout.Cols) * layout.CellW;
            int rowFromBottom = layout.Rows - 1 - (i / layout.Cols);
            int cellY = rowFromBottom * layout.CellH;

            float refX = layout.HorizontalFromCellCentre ? layout.CellW * 0.5f : measure.CentreX;
            float refY = layout.AnchorFeet ? measure.MinY : measure.CentreY;

            Color32[] frame = new Color32[layout.OutSize * layout.OutSize];
            SampleCell(grid, gridWidth, cellX, cellY, layout, refX, refY, scale, frame);

            string framePath = $"{layout.OutFolder}/{i:00}.png";
            WritePng(framePath, frame, layout.OutSize, layout.OutSize);
            AssetDatabase.ImportAsset(framePath, ImportAssetOptions.ForceSynchronousImport);
            ApplySpriteSettings(framePath);
        }

        Debug.Log(
            $"[NexusArtTools] {layout.Set}/{layout.State}: {layout.Frames} frames → {layout.OutFolder} " +
            $"({layout.OutSize}px @ {PixelsPerUnit} px/unit), scale {scale:0.####}, " +
            $"union x{measure.MinX}..{measure.MaxX} y{measure.MinY}..{measure.MaxY}. {layout}");

        return true;
    }

    private static int MapToFrame(int cellValue, GridMeasure measure, Layout layout, float span, bool vertical)
    {
        float scale = layout.TargetSpan / Mathf.Max(1f, span);
        float refValue = vertical
            ? (layout.AnchorFeet ? measure.MinY : measure.CentreY)
            : (layout.HorizontalFromCellCentre ? layout.CellW * 0.5f : measure.CentreX);
        int anchor = vertical ? layout.AnchorY : layout.AnchorX;

        return Mathf.RoundToInt(anchor + (cellValue - refValue) * scale);
    }

    private static void SampleCell(
        Color32[] grid, int gridWidth, int cellX, int cellY, Layout layout,
        float refX, float refY, float scale, Color32[] frame)
    {
        int outSize = layout.OutSize;
        float inverse = 1f / scale;

        for (int y = 0; y < outSize; y++)
        {
            float cy = (y - layout.AnchorY) * inverse + refY;
            if (cy < 0f || cy >= layout.CellH) continue;

            int gridY = cellY + Mathf.RoundToInt(cy);
            if (gridY < 0) continue;

            int gridRow = gridY * gridWidth + cellX;
            int outRow = y * outSize;

            for (int x = 0; x < outSize; x++)
            {
                float cx = (x - layout.AnchorX) * inverse + refX;
                if (cx < 0f || cx >= layout.CellW) continue;

                int at = gridRow + Mathf.RoundToInt(cx);
                if (at < 0 || at >= grid.Length) continue;

                frame[outRow + x] = grid[at];
            }
        }
    }

    // ── Clip rebuilding ─────────────────────────────────────────────────────────────

    private struct ClipRecipe
    {
        public string Clip;
        public string State;
        public int[] Frames;
        public bool Loop;
        /// <summary>Poses per second. 0 means <see cref="SampleRate"/>.</summary>
        public float Fps;
        /// <summary>
        /// Explicit key times, in seconds, one per entry in <see cref="Frames"/>. Null means the keys
        /// sit on the uniform 1/<see cref="Fps"/> grid. Used where a swing should travel fast and
        /// then hold, which a uniform grid cannot express.
        /// </summary>
        public float[] KeyTimes;
        /// <summary>Clip length in seconds. 0 means <c>Frames.Length / fps</c>.</summary>
        public float Length;
    }

    /// <summary>Attack poses per second for G1 and G2: the fast swings of the chain.</summary>
    public const float AttackFps = 20f;

    /// <summary>
    /// Every body clip's frame sequence. Keys land on the 1/fps grid — and the clip stops
    /// one whole pose after the last key, so a repeated final frame is never a loop hitch. The three
    /// combo clips override those times through <see cref="ClipRecipe.KeyTimes"/> where a uniform
    /// grid would step the swing; their last key still lands one pose short of the clip's end, because
    /// the animator measures a state by <c>AnimationClip.length</c> (final key + one frame), so that
    /// is what pins 0.300 s to CF-A1/A2's 18 f and 0.450 s to CF-A3's 27 f.
    /// </summary>
    private static readonly ClipRecipe[] BodyRecipes =
    {
        new ClipRecipe { Clip = "Nexus_Idle", State = "standing", Frames = new[] { 0, 1, 2, 3, 4, 5, 6, 7 }, Loop = true },
        new ClipRecipe { Clip = "Nexus_Walk", State = "walking", Frames = new[] { 0, 1, 2, 3, 4, 5, 6, 7 }, Loop = true },
        new ClipRecipe { Clip = "Nexus_Run", State = "running", Frames = new[] { 0, 1, 2, 3, 4, 5, 6, 7 }, Loop = true },
        new ClipRecipe { Clip = "Nexus_Jump", State = "jumping", Frames = new[] { 0, 1, 2, 3, 4, 5 }, Loop = false },
        new ClipRecipe { Clip = "Nexus_Takeoff", State = "jumping", Frames = new[] { 0, 1 }, Loop = false },
        new ClipRecipe { Clip = "Nexus_Fall", State = "falling", Frames = new[] { 0, 1, 2, 3 }, Loop = true },
        new ClipRecipe { Clip = "Nexus_Land", State = "falling", Frames = new[] { 3, 0 }, Loop = false },
        new ClipRecipe { Clip = "Nexus_Attack_G1", State = "attack_g1", Frames = new[] { 0, 1, 2, 3, 4, 5 }, Loop = false, Fps = AttackFps },
        new ClipRecipe { Clip = "Nexus_Attack_G2", State = "attack_g2", Frames = new[] { 0, 1, 2, 3, 4, 5 }, Loop = false, Fps = AttackFps },
        // G3 travels the whole overhead arc by 0.28 s and then holds the forward pose until the end
        // of CF-A3's 27 f. A uniform 13.333 fps grid would step the chop every 75 ms, which reads as
        // a stutter on the heaviest swing in the chain. The last key sits one pose short of the end
        // because AnimationClip.length is the final key plus one frame at the clip's rate, and the
        // animator measures this state by that length: 0.40 + 1/20 = the 0.45 s CF-A3 asks for.
        new ClipRecipe
        {
            Clip = "Nexus_Attack_G3", State = "attack_g3", Frames = new[] { 0, 1, 2, 3, 4, 5 }, Loop = false,
            Fps = AttackFps, KeyTimes = new[] { 0f, 0.08f, 0.16f, 0.22f, 0.28f, 0.40f }, Length = 0.45f,
        },
    };

    [MenuItem("Bastion Zero/Art/Rebuild Body Clips")]
    public static void RebuildAllClipsMenu() => RebuildBodyClips();

    /// <summary>The three combo grids, in chain order, with the span each is sliced at.</summary>
    private static readonly (string State, int Frames, float Span)[] ComboStates =
    {
        ("attack_g1", 6, BodyReferenceSpan),
        ("attack_g2", 6, BodyReferenceSpan),
        ("attack_g3", 6, BodyReferenceSpan),
    };

    /// <summary>
    /// Validates and slices the three combo grids. All three share the standing grid's
    /// <see cref="BodyReferenceSpan"/> like every other body state, so the swing is exactly the same
    /// figure size as Idle, Walk and Run.
    ///
    /// <para>G3 used to measure its own grid (span 0) because the art model drew its poses ~8 %
    /// shorter than the other states (446 px against standing's 489 px). That exception is gone:
    /// the current G3 sheet measures 482 px against 484 px for G1, so the shared span costs it
    /// 0.8 % and the one-scale rule holds across the whole cast of states again.</para>
    /// </summary>
    [MenuItem("Bastion Zero/Art/Slice Combo Attack States")]
    public static int SliceComboAttacks()
    {
        int sliced = 0;

        foreach ((string state, int frames, float span) in ComboStates)
        {
            Layout layout = BodyLayout(state, frames);
            layout.ReferenceSpan = span;

            if (!Validate(layout)) continue;
            if (Slice(layout)) sliced++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[NexusArtTools] {sliced}/{ComboStates.Length} combo states sliced.");
        return sliced;
    }

    public static int RebuildBodyClips()
    {
        int rebuilt = 0;

        foreach (ClipRecipe recipe in BodyRecipes)
        {
            string clipPath = $"{WinglessClips}/{recipe.Clip}.anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);

            if (clip == null)
            {
                Debug.LogError($"[NexusArtTools] missing clip {clipPath}.");
                continue;
            }

            float fps = recipe.Fps > 0f ? recipe.Fps : SampleRate;
            float length = recipe.Length > 0f ? recipe.Length : recipe.Frames.Length / fps;
            var keys = new ObjectReferenceKeyframe[recipe.Frames.Length];
            bool complete = true;

            for (int i = 0; i < recipe.Frames.Length; i++)
            {
                string spritePath = $"{BodyOut}/{recipe.State}/{recipe.Frames[i]:00}.png";
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

                if (sprite == null)
                {
                    Debug.LogError($"[NexusArtTools] {recipe.Clip}: missing sprite {spritePath}.");
                    complete = false;
                    break;
                }

                keys[i] = new ObjectReferenceKeyframe
                {
                    time = recipe.KeyTimes != null && i < recipe.KeyTimes.Length ? recipe.KeyTimes[i] : i / fps,
                    value = sprite,
                };
            }

            if (!complete) continue;

            var binding = new EditorCurveBinding
            {
                path = BodyBindingPath,
                type = typeof(SpriteRenderer),
                propertyName = "m_Sprite",
            };

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            clip.frameRate = fps;

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.stopTime = length;
            settings.loopTime = recipe.Loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            EditorUtility.SetDirty(clip);
            rebuilt++;

            Debug.Log(
                $"[NexusArtTools] {recipe.Clip}: {recipe.Frames.Length} keys on the 1/{fps} grid, " +
                $"length {settings.stopTime:0.####}s, loop {recipe.Loop}.");
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[NexusArtTools] {rebuilt}/{BodyRecipes.Length} body clips rebuilt.");
        return rebuilt;
    }

    // ── Plumbing ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Every frame the hero draws, as asset paths: each body state's PNGs, then each wing state's.
    /// The _work grids and _ref sheets are sources, never drawn, so they are not included.
    /// </summary>
    public static List<string> FramePaths()
    {
        var paths = new List<string>();
        AddStateFrames(paths, BodyOut);
        AddStateFrames(paths, WingOut);
        return paths;
    }

    private static void AddStateFrames(List<string> paths, string root)
    {
        string fullRoot = Path.GetFullPath(root);
        if (!Directory.Exists(fullRoot)) return;

        string[] folders = Directory.GetDirectories(fullRoot);
        System.Array.Sort(folders, System.StringComparer.Ordinal);

        foreach (string folder in folders)
        {
            string state = Path.GetFileName(folder);

            // Source sheets live under _work and _ref; the wing frames are walked as their own root.
            if (state.StartsWith("_") || $"{root}/{state}" == WingOut) continue;

            string[] files = Directory.GetFiles(folder, "*.png");
            System.Array.Sort(files, System.StringComparer.Ordinal);

            foreach (string file in files) paths.Add($"{root}/{state}/{Path.GetFileName(file)}");
        }
    }

    /// <summary>
    /// Re-applies the sprite import settings to every hero frame, body and wings, and to the
    /// Cinder-Mite through its own importer, so a sampling change reaches sprites that were sliced or
    /// imported before it. Reports how many now carry Point + mipmaps + ignoreMipmapLimit.
    /// </summary>
    [MenuItem("Bastion Zero/Art/Reapply Sprite Import Settings")]
    public static string ReapplyImportSettings()
    {
        List<string> frames = FramePaths();
        foreach (string frame in frames) ApplySpriteSettings(frame);

        Sys3Tools.ImportMiteSprite();

        var all = new List<string>(frames) { NexusGridKeyer.MitePath };
        var missed = new List<string>();
        int sampled = 0;

        foreach (string path in all)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            bool applied = importer != null && importer.mipmapEnabled && importer.ignoreMipmapLimit &&
                           importer.filterMode == FilterMode.Point;

            if (applied) sampled++;
            else missed.Add(path);
        }

        string summary =
            $"[NexusArtTools] sprite import settings re-applied: {sampled}/{all.Count} sprites " +
            $"({frames.Count} hero frames + the Cinder-Mite) on Point + mipmaps + ignoreMipmapLimit." +
            (missed.Count == 0 ? "" : " NOT APPLIED: " + string.Join(", ", missed));

        if (missed.Count == 0) Debug.Log(summary);
        else Debug.LogError(summary);

        return summary;
    }

    private static void ApplySpriteSettings(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = PixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;

        // Point keeps the hard-edged pixels, but a 256 px frame draws at about 127 px (ortho 8.5 at
        // 1080p), and Point alone then skips every other texel, so the pinstripes shimmer as the
        // sprite moves. Mipmaps give Point a pre-filtered level to sample; ignoreMipmapLimit keeps the
        // quality level's global mipmap limit from dropping the full-size level instead.
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
    }

    public static Color32[] ReadPng(string assetPath, out int width, out int height)
    {
        width = 0;
        height = 0;

        string fullPath = Path.GetFullPath(assetPath);
        if (!File.Exists(fullPath)) return null;

        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        try
        {
            if (!texture.LoadImage(File.ReadAllBytes(fullPath))) return null;

            width = texture.width;
            height = texture.height;
            return texture.GetPixels32();
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(texture);
        }
    }

    private static void WritePng(string assetPath, Color32[] pixels, int width, int height)
    {
        string fullPath = Path.GetFullPath(assetPath);
        string folder = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder)) Directory.CreateDirectory(folder);

        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        try
        {
            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(fullPath, texture.EncodeToPNG());
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(texture);
        }
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        string leaf = Path.GetFileName(path);

        if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, leaf);
    }
}
#endif
