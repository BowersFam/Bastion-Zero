using BastionZero.Core;
using UnityEngine;

namespace BastionZero.Combat
{
    /// <summary>
    /// SYS3 §8 screen shake: amplitude in pixels (at 16 px per tile) with a linear falloff to
    /// zero over the row's duration, plus the global on/off toggle and intensity slider the
    /// spec's accessibility note asks for.
    ///
    /// The script runs after the follow camera (execution order 1000) and only ever adds an
    /// offset on top of the position someone else wrote, so the two never accumulate.
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public class CombatCameraShake : MonoBehaviour
    {
        [Header("Accessibility (§8 footer)")]
        [SerializeField] private bool shakeEnabled = true;
        [Range(0f, 1f)]
        [SerializeField] private float intensity = 1f;

        private float timer;
        private float duration;
        private float amplitudeUnits;
        private Vector3 basePosition;
        private Vector3 appliedPosition;
        private bool applied;

        /// <summary>True while an offset is being applied this frame.</summary>
        public bool IsShaking => timer > 0f;

        /// <summary>Current amplitude in pixels, for the readout.</summary>
        public float CurrentAmplitudePixels => intensity > 0f ? amplitudeUnits * SpecUnits.PixelsPerTile : 0f;

        /// <summary>Applies one §8 row. A weaker shake never cuts a stronger one short.</summary>
        public void Shake(float amplitudePixels, float seconds)
        {
            if (!shakeEnabled || amplitudePixels <= 0f || seconds <= 0f) return;

            float units = CombatTuning.PixelsToUnits(amplitudePixels) * Mathf.Clamp01(intensity);
            if (units <= 0f) return;

            if (units >= amplitudeUnits || timer <= 0f)
            {
                amplitudeUnits = units;
                duration = seconds;
                timer = seconds;
            }
        }

        /// <summary>Stops the shake and leaves the camera where its owner put it.</summary>
        public void Cancel()
        {
            timer = 0f;
            amplitudeUnits = 0f;
            if (applied)
            {
                transform.position = basePosition;
                applied = false;
            }
        }

        private void LateUpdate()
        {
            // Undo last frame's offset only if nobody else moved the camera since; a follow
            // camera writes a fresh position every frame, and that write wins.
            if (applied && transform.position == appliedPosition)
            {
                transform.position = basePosition;
            }

            applied = false;
            basePosition = transform.position;

            if (timer <= 0f) return;

            timer = Mathf.Max(0f, timer - Time.unscaledDeltaTime);
            if (timer <= 0f)
            {
                amplitudeUnits = 0f;
                return;
            }

            float falloff = duration > 0f ? Mathf.Clamp01(timer / duration) : 0f;
            Vector2 jitter = Random.insideUnitCircle * (amplitudeUnits * falloff);

            transform.position = basePosition + new Vector3(jitter.x, jitter.y, 0f);
            appliedPosition = transform.position;
            applied = true;
        }
    }
}
