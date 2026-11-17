using UnityEngine;

namespace BastionZero.Movement
{
    /// <summary>
    /// SYS1 tuning asset. One field per spec ID from 05-movement.md, with the tag and the
    /// reasoning carried in the tooltip so a value can be traced back to the table.
    /// Every number is [estimated] unless the ID says otherwise, so all of them are tunable
    /// here and none of them are written into code.
    /// </summary>
    [CreateAssetMenu(menuName = "Bastion Zero/Tuning/Player Movement (SYS1)", fileName = "PlayerMovementTuning")]
    public class PlayerMovementTuning : ScriptableObject
    {
        [Header("MV-01…MV-06 · Horizontal (05-movement §3.1)")]
        [Tooltip("MV-01 Run speed — 8.5 tiles/s [estimated]. Mid-high in the 6.6–10.6 range.")]
        [Min(0f)] public float MV_01_RunSpeed = 8.5f;

        [Tooltip("MV-02 Ground accel time to full speed — 0.05 s [estimated]. Precision-platformer end of the range.")]
        [Min(0.001f)] public float MV_02_GroundAccelTime = 0.05f;

        [Tooltip("MV-03 Ground stop time — 0.04 s [estimated]. Near-instant, slight ease avoids jitter.")]
        [Min(0.001f)] public float MV_03_GroundStopTime = 0.04f;

        [Tooltip("MV-04 Turnaround time — 0.05 s [estimated]. Matches the reference feel.")]
        [Min(0.001f)] public float MV_04_TurnaroundTime = 0.05f;

        [Tooltip("MV-05 Air accel multiplier — 0.85 × ground [estimated]. Air deceleration reuses the same multiplier.")]
        [Range(0f, 2f)] public float MV_05_AirAccelMultiplier = 0.85f;

        [Tooltip("MV-06 Air turn multiplier — 1.0 × ground turn [estimated]. Never punish a reversal in the air.")]
        [Range(0f, 2f)] public float MV_06_AirTurnMultiplier = 1f;

        [Header("MV-07…MV-18 · Jump (05-movement §3.1)")]
        [Tooltip("MV-07 Held-rise speed — 13.8 tiles/s [estimated]. Solved for h = 4 tiles, T = 0.40 s.")]
        [Min(0f)] public float MV_07_HeldRiseSpeed = 13.8f;

        [Tooltip("MV-08 Max hold time — 0.18 s [estimated]. Middle of the 0.17–0.22 s range.")]
        [Min(0f)] public float MV_08_MaxHoldTime = 0.18f;

        [Tooltip("MV-09 Gravity — 62.7 tiles/s² [estimated]. From g = v²/(2(h − v·t)).")]
        [Min(0f)] public float MV_09_Gravity = 62.7f;

        [Tooltip("MV-10 Full jump height — 4.0 tiles [estimated]. Reference value for verification, not used in the integrator.")]
        [Min(0f)] public float MV_10_FullJumpHeight = 4f;

        [Tooltip("MV-11 Time to apex — 0.40 s [estimated]. Reference value for verification, not used in the integrator.")]
        [Min(0f)] public float MV_11_TimeToApex = 0.4f;

        [Tooltip("MV-12 Release cut — 0.5 × vy [estimated]. A 0.05 s tap gives about 1.07 tiles.")]
        [Range(0f, 1f)] public float MV_12_ReleaseCut = 0.5f;

        [Tooltip("MV-13 Minimum hold before release counts — 0.05 s [estimated]. Guarantees a readable minimum hop.")]
        [Min(0f)] public float MV_13_MinHoldTime = 0.05f;

        [Tooltip("MV-14 Apex band speed — 2.0 tiles/s [estimated]. Width of the reduced-gravity window.")]
        [Min(0f)] public float MV_14_ApexBandSpeed = 2f;

        [Tooltip("MV-15 Apex gravity multiplier — 0.5 × g [estimated]. Targets about 0.2 s of hang.")]
        [Range(0f, 1f)] public float MV_15_ApexGravityMultiplier = 0.5f;

        [Tooltip("MV-16 Fall gravity multiplier — 1.0 × g [estimated]. Feel comes from the held rise, not a heavy fall.")]
        [Range(0f, 4f)] public float MV_16_FallGravityMultiplier = 1f;

        [Tooltip("MV-17 Fall cap — 19 tiles/s [estimated]. 1.38× the rise speed.")]
        [Min(0f)] public float MV_17_FallCap = 19f;

        [Tooltip("MV-18 Fast-fall cap — 26 tiles/s [estimated]. Off by default; fast-fall is deferred out of the slice.")]
        [Min(0f)] public float MV_18_FastFallCap = 26f;

        [Tooltip("Fast-fall on/off. The slice keeps this off (13 §5.7).")]
        public bool MV_18_FastFallEnabled = false;

        [Header("MV-19…MV-27 · Buffers and air jumps")]
        [Tooltip("MV-19 Coyote time — 0.10 s [estimated].")]
        [Min(0f)] public float MV_19_CoyoteTime = 0.1f;

        [Tooltip("MV-20 Jump buffer — 0.10 s [estimated]. Measured from the input event's own timestamp.")]
        [Min(0f)] public float MV_20_JumpBuffer = 0.1f;

        [Tooltip("MV-21 Head-bump coyote suppression — 0.06 s [estimated]. Stops ceiling-clip free jumps.")]
        [Min(0f)] public float MV_21_HeadBumpSuppression = 0.06f;

        [Tooltip("MV-22 Stored air-jumps max — 1 count [estimated].")]
        [Min(0)] public int MV_22_StoredAirJumpsMax = 1;

        [Tooltip("MV-26 Hit-pause on an airborne qualifying hit — 0.05 s [estimated]. Applied by SYS3, stored here for traceability.")]
        [Min(0f)] public float MV_26_HitPause = 0.05f;

        [Tooltip("MV-27 Air stall on hit — 0.06 s with vy held at ≥ 0 [estimated]. Applied on a bounce grant.")]
        [Min(0f)] public float MV_27_AirStall = 0.06f;

        [Header("MV-39…MV-44 · Wall")]
        [Tooltip("MV-39 Wall climb speed — 4 tiles/s [estimated]. Up input while clinging.")]
        [Min(0f)] public float MV_39_WallClimbSpeed = 4f;

        [Tooltip("MV-40 Wall slide speed — 4 tiles/s [estimated]. Down input while clinging; the cling itself holds, it does not slide.")]
        [Min(0f)] public float MV_40_WallSlideSpeed = 4f;

        [Tooltip("MV-41 Wall-jump horizontal push — 6 tiles/s [estimated]. Decays into normal air control.")]
        [Min(0f)] public float MV_41_WallJumpPush = 6f;

        [Tooltip("MV-42 Re-cling lockout after a wall jump — 0.12 s [estimated]. Stops instant re-stick.")]
        [Min(0f)] public float MV_42_ReClingLockout = 0.12f;

        [Tooltip("MV-43 Hold-away release delay — 0.08 s [estimated]. Fixes eager-cling.")]
        [Min(0f)] public float MV_43_HoldAwayReleaseDelay = 0.08f;

        [Header("Motor geometry (ai-rules rule 3)")]
        [Tooltip("Gap kept between the hero and geometry after a blocked sweep. ai-rules: 0.01–0.02 u.")]
        [Range(0.005f, 0.03f)] public float collisionSkin = 0.015f;

        [Tooltip("Downward probe length used to re-acquire the floor.")]
        [Min(0.001f)] public float groundProbeDistance = 0.08f;

        [Tooltip("Sideways probe length used to find a cling-able wall.")]
        [Min(0.001f)] public float wallProbeDistance = 0.04f;

        [Tooltip("Largest sideways nudge applied when the head clips a ceiling corner.")]
        [Min(0f)] public float cornerCorrection = 0.1f;

        [Tooltip("Ceiling hits deeper than this are a real head bump, not a corner.")]
        [Min(0f)] public float cornerCorrectionMaxRise = 0.3f;

        [Tooltip("Layers that block the hero. One-way platforms on these layers are filtered by cast direction.")]
        public LayerMask solidLayers = ~0;

        [Tooltip("Layers that count as a hazard when recording safe ground (SYS2 fills these in).")]
        public LayerMask hazardLayers = 0;

        [Header("Derived rates (read-only)")]
        [Tooltip("Dwell time on safe ground before the position is recorded (SYS1 row 52 / SYS2 §9).")]
        [Min(0f)] public float safeGroundDwell = 0.2f;

        /// <summary>MV-01 / MV-02 — acceleration at full input on the ground.</summary>
        public float GroundAcceleration => MV_01_RunSpeed / Mathf.Max(0.0001f, MV_02_GroundAccelTime);

        /// <summary>MV-01 / MV-03 — deceleration toward zero with no input on the ground.</summary>
        public float GroundDeceleration => MV_01_RunSpeed / Mathf.Max(0.0001f, MV_03_GroundStopTime);

        /// <summary>2 × MV-01 / MV-04 — rate used while reversing direction.</summary>
        public float GroundTurnAcceleration => 2f * MV_01_RunSpeed / Mathf.Max(0.0001f, MV_04_TurnaroundTime);

        [Header("Tuning change log")]
        [Tooltip("Record every edit to an [estimated] value here so the table and the asset cannot drift apart.")]
        [TextArea(3, 10)] public string changeLog =
            "v0 — created from spec v3.2 (05-movement.md). No [estimated] value has been retuned yet.";
    }
}
