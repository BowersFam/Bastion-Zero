using UnityEngine;

namespace BastionZero.Animation
{
    /// <summary>
    /// Animation tuning asset. One field per AN spec ID, with the reasoning in the tooltip so a
    /// value can be traced back to the art contract in
    /// <c>Assets/Art/Characters/Nexus/HD2D/_ref/design_spec.md</c>. No animation number lives in code.
    /// </summary>
    [CreateAssetMenu(menuName = "Bastion Zero/Tuning/Player Animation (AN)", fileName = "PlayerAnimationTuning")]
    public class PlayerAnimationTuning : ScriptableObject
    {
        [Header("AN-01…AN-02 · Cycle distance (anti foot-slide)")]
        [Tooltip("AN-01 Walk cycle distance — 2.40 u. The ground distance one full 8-pose walk cycle should " +
                 "look like it covers. Cycle rate = speed × clip length ÷ this, so the planted foot stays planted.")]
        [Min(0.01f)] public float AN_01_WalkCycleDistance = 2.4f;

        [Tooltip("AN-02 Run cycle distance — 5.60 u. Same contract for the sprint: at MV-01 (8.5 u/s) the " +
                 "0.6667 s clip resolves to a rate of about 1.01, i.e. the art's own cadence.")]
        [Min(0.01f)] public float AN_02_RunCycleDistance = 5.6f;

        [Header("AN-03 · Pose floor")]
        [Tooltip("AN-03 Minimum pose hold — 3 display frames at 60 fps. Caps the resolved cycle rate at " +
                 "60 ÷ (12 × 3) ≈ 1.67, so a fast sprint can never strobe the 8-pose clips.")]
        [Min(1f)] public float AN_03_MinPoseHoldFrames = 3f;

        [Header("AN-04 · Wings")]
        [Tooltip("AN-04 Wing flap rate scale — 1.0 ×. Multiplies the flap cycle rate. Raise for a busier beat, " +
                 "lower for a heavier one; the wing clip is 8 poses at 12 fps.")]
        [Range(0.25f, 3f)] public float AN_04_WingFlapRateScale = 1f;
    }
}
