namespace BastionZero.Core
{
    /// <summary>
    /// Units and conversion helpers for the Bastion Zero spec (file 01, "Tags and markers").
    /// The spec states values in tiles and frames; the project runs 1 tile = 1 unit and a
    /// 60 Hz fixed timestep, so every conversion the codebase needs lives here and nowhere else.
    /// </summary>
    public static class SpecUnits
    {
        /// <summary>Spec timestep: 60 [estimated] fps (file 01, Units).</summary>
        public const int FramesPerSecond = 60;

        /// <summary>Fixed timestep in seconds that matches <see cref="FramesPerSecond"/>.</summary>
        public const float FixedDeltaTime = 1f / FramesPerSecond;

        /// <summary>1 tile = 16 [estimated] px (file 01, Units).</summary>
        public const float PixelsPerTile = 16f;

        /// <summary>Hero height H = 2 [estimated] tiles (file 01, Units).</summary>
        public const float HeroHeightTiles = 2f;

        /// <summary>Pixels per unit the project imports at, so 1 tile is 1 unit (ai-rules rule 4).</summary>
        public const float PixelsPerUnit = PixelsPerTile;

        /// <summary>Converts a spec duration in frames to seconds.</summary>
        public static float Frames(int frames) => frames / (float)FramesPerSecond;

        /// <summary>Converts spec tiles to world units (identity at 1 tile = 1 unit).</summary>
        public static float Tiles(float tiles) => tiles * (PixelsPerTile / PixelsPerUnit);

        /// <summary>Converts a spec speed in tiles per second to world units per second.</summary>
        public static float TilesPerSecond(float tilesPerSecond) => Tiles(tilesPerSecond);
    }
}
