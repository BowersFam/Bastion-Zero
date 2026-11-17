using UnityEngine;

namespace BastionZero.World
{
    /// <summary>
    /// SYS5 §10.2 camera values (tuning table rows WC-W1 … WC-W15).
    ///
    /// Every number the world camera uses lives here and carries the spec row it comes from, so a
    /// value can be traced back to the document and retuned without touching code. Values are in
    /// tiles and seconds; 1 tile = 1 unit (<see cref="BastionZero.Core.SpecUnits"/>).
    /// </summary>
    [CreateAssetMenu(fileName = "WorldCameraTuning",
                     menuName = "Bastion Zero/Tuning/Create World Camera Tuning")]
    public class WorldCameraTuning : ScriptableObject
    {
        [Header("WC-W1  Viewport")]
        [Tooltip("WC-W1: viewport 24 × 17 tiles. The camera resolves one orthographic " +
                 "size from the game view's aspect so both numbers are on screen at once.")]
        [Min(1f)] public float W1_ViewportWidthTiles = 24f;

        [Tooltip("WC-W1: 17 tiles tall (raised from the spec's 13.5 estimate — at 13.5 the ground " +
                 "and the platforms to land on were off the bottom of the screen). The camera sits " +
                 "W9 below centre, so 17 tall leaves 5.5 tiles of ground under the hero's feet.")]
        [Min(1f)] public float W1_ViewportHeightTiles = 17f;

        [Header("WC-W2  Horizontal dead zone")]
        [Tooltip("WC-W2: 1.5 tiles wide, centred on the framing point [estimated].")]
        [Min(0f)] public float W2_HorizontalDeadZoneTiles = 1.5f;

        [Header("WC-W3  Horizontal trailing")]
        [Tooltip("WC-W3: time constant of the horizontal trail, 0.12 s [estimated].")]
        [Min(0.001f)] public float W3_HorizontalTrailSeconds = 0.12f;

        [Header("WC-W4  Horizontal look-ahead")]
        [Tooltip("WC-W4: 3 tiles toward facing [estimated].")]
        [Min(0f)] public float W4_LookAheadTiles = 3f;

        [Tooltip("WC-W4: the look-ahead only starts after 0.25 s of held input [estimated].")]
        [Min(0f)] public float W4_HoldSeconds = 0.25f;

        [Tooltip("WC-W4: and it eases in over 0.5 s [estimated].")]
        [Min(0.001f)] public float W4_EaseSeconds = 0.5f;

        [Tooltip("WC-W4: dual-forward focus — the focus only flips once the hero has travelled " +
                 "1.5 tiles in the new direction [estimated].")]
        [Min(0f)] public float W4_TurnThresholdTiles = 1.5f;

        [Header("WC-W5  Look-ahead during a bounce chain")]
        [Tooltip("WC-W5: 4 tiles in the travel direction while a bounce chain is running " +
                 "[estimated]. Glide is cut from the slice, so this is the only W5 case that ships.")]
        [Min(0f)] public float W5_BounceLookAheadTiles = 4f;

        [Header("WC-W6  Vertical follow on a jump")]
        [Tooltip("WC-W6: wait 0.2 s after take-off before following at all [estimated].")]
        [Min(0f)] public float W6_JumpFollowDelaySeconds = 0.2f;

        [Tooltip("WC-W6: then follow 0.6× the rise [estimated].")]
        [Range(0f, 1f)] public float W6_JumpFollowFactor = 0.6f;

        [Header("WC-W7  Vertical platform snap")]
        [Tooltip("WC-W7: ease to the new ground height over 0.25 s on landing [estimated].")]
        [Min(0f)] public float W7_PlatformSnapSeconds = 0.25f;

        [Tooltip("A grounded height change smaller than this many tiles is followed directly " +
                 "instead of being snapped, so walking along flat ground never restarts the snap.")]
        [Min(0f)] public float W7_SnapDeadbandTiles = 0.25f;

        [Header("WC-W8  Fall lock and look-down")]
        [Tooltip("WC-W8: the camera locks onto the hero after 0.15 s of falling [estimated].")]
        [Min(0f)] public float W8_FallLockSeconds = 0.15f;

        [Tooltip("WC-W8: \"falling at ≥ 80 % of the fall cap (SYS1 MV-17)\" [estimated].")]
        [Range(0f, 1f)] public float W8_FallSpeedFraction = 0.8f;

        [Tooltip("WC-W8: for 0.2 s before the look-down engages [estimated].")]
        [Min(0f)] public float W8_LookDownHoldSeconds = 0.2f;

        [Tooltip("WC-W8: lead 3 tiles downward [estimated].")]
        [Min(0f)] public float W8_LookDownTiles = 3f;

        [Header("WC-W9  Hero vertical placement")]
        [Tooltip("WC-W9: the hero's feet sit 3 tiles below screen centre while grounded [estimated].")]
        [Min(0f)] public float W9_FeetBelowCentreTiles = 3f;

        [Header("WC-W10  Room-bound clamp")]
        [Tooltip("WC-W10: clamping eases over 0.15 s when look-ahead pushes into a wall [estimated].")]
        [Min(0.001f)] public float W10_ClampEaseSeconds = 0.15f;

        [Header("WC-W15  C-ZONE and C-LOOK")]
        [Tooltip("WC-W15: C-ZONE ease out, 0.4 s [estimated].")]
        [Min(0f)] public float W15_ZoneEaseSeconds = 0.4f;

        [Tooltip("WC-W15: C-LOOK hold before the pan starts, 0.4 s [estimated].")]
        [Min(0f)] public float W15_LookHoldSeconds = 0.4f;

        [Tooltip("WC-W15: C-LOOK pan distance, 4 tiles [estimated].")]
        [Min(0f)] public float W15_LookTiles = 4f;

        [Tooltip("WC-W15: C-LOOK ease back on release, 0.3 s [estimated].")]
        [Min(0.001f)] public float W15_LookEaseSeconds = 0.3f;

        [Tooltip("Dead zone around neutral on the look input before a pan counts as held.")]
        [Range(0f, 1f)] public float W15_LookInputDeadZone = 0.5f;

        [Header("C-LOOK gating (SYS5 §10.1: grounded and idle)")]
        [Tooltip("Horizontal speed under which the hero counts as idle for C-LOOK.")]
        [Min(0f)] public float LookIdleSpeedTilesPerSecond = 0.1f;

        /// <summary>
        /// WC-W1: the single orthographic size that shows the 24 × 13.5 tile viewport on a game
        /// view of this aspect. Both numbers are honoured; on a wide window the width rules, on a
        /// tall one the height does.
        /// </summary>
        public float ResolveOrthographicSize(float aspect)
        {
            float fromHeight = W1_ViewportHeightTiles * 0.5f;
            float fromWidth = aspect > 0.0001f
                ? (W1_ViewportWidthTiles / aspect) * 0.5f
                : fromHeight;
            return Mathf.Max(fromHeight, fromWidth);
        }
    }
}
