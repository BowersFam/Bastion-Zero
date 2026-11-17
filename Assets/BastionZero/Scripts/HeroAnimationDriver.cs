using BastionZero.Movement;
using UnityEngine;

namespace BastionZero.Animation
{
    /// <summary>
    /// Drives the two locomotion cycle-rate parameters on <c>Nexus.controller</c> from the motor's
    /// real ground speed, so the walk and run clips advance at the rate the hero actually travels
    /// and the planted foot never slides. Also feeds the wing flap rate.
    ///
    /// The parameters are consumed by the Walk / Run / WingFlap states themselves (their playback
    /// speed points at a parameter), so nothing here touches clip assets and the motor stays
    /// untouched: <see cref="PlayerMovement.Velocity"/> is read-only for this script.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class HeroAnimationDriver : MonoBehaviour
    {
        private static readonly int WalkCycleSpeedParam = Animator.StringToHash("WalkCycleSpeed");
        private static readonly int RunCycleSpeedParam = Animator.StringToHash("RunCycleSpeed");
        private static readonly int WingFlapRateParam = Animator.StringToHash("WingFlapRate");

        /// <summary>Both locomotion clips are 8 poses at 12 fps.</summary>
        private const float LocomotionClipLength = 8f / 12f;
        private const float DisplayFps = 60f;
        private const float SourceFps = 12f;

        [Header("References")]
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private Animator animator;
        [SerializeField] private PlayerAnimationTuning tuning;

        private bool loggedResolvedRates;

        private void Reset()
        {
            movement = GetComponent<PlayerMovement>();
            animator = GetComponent<Animator>();
        }

        private void Awake()
        {
            if (movement == null) movement = GetComponent<PlayerMovement>();
            if (animator == null) animator = GetComponent<Animator>();
        }

        private void LateUpdate()
        {
            if (animator == null || movement == null || tuning == null) return;

            float speed = Mathf.Abs(movement.Velocity.x);
            float maxRate = DisplayFps / (SourceFps * Mathf.Max(1f, tuning.AN_03_MinPoseHoldFrames));

            float walkRate = CycleRate(speed, tuning.AN_01_WalkCycleDistance, maxRate);
            float runRate = CycleRate(speed, tuning.AN_02_RunCycleDistance, maxRate);

            animator.SetFloat(WalkCycleSpeedParam, walkRate);
            animator.SetFloat(RunCycleSpeedParam, runRate);
            animator.SetFloat(WingFlapRateParam, tuning.AN_04_WingFlapRateScale);

            if (loggedResolvedRates || speed < 0.5f) return;

            loggedResolvedRates = true;
            Debug.Log(
                $"[HeroAnimationDriver] resolved cycle rates at {speed:0.00} u/s: " +
                $"walk {walkRate:0.000} (distance {tuning.AN_01_WalkCycleDistance:0.00} u), " +
                $"run {runRate:0.000} (distance {tuning.AN_02_RunCycleDistance:0.00} u), " +
                $"cap {maxRate:0.000}, wing flap ×{tuning.AN_04_WingFlapRateScale:0.00}.",
                this);
        }

        /// <summary>Speed ÷ (cycle distance ÷ clip length), clamped to the pose-hold floor.</summary>
        private static float CycleRate(float speed, float cycleDistance, float maxRate)
        {
            if (cycleDistance <= 0.0001f) return 0f;
            return Mathf.Clamp(speed * LocomotionClipLength / cycleDistance, 0f, maxRate);
        }
    }
}
