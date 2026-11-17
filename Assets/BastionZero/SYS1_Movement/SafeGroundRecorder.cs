using UnityEngine;
using BastionZero.Core;

namespace BastionZero.Movement
{
    /// <summary>
    /// SYS1's safe-ground recorder (05-movement row 52; consumed by SYS2 §9 for the pit warp and
    /// by SYS6 §7.3 for the death marker). Records the last position the hero stood on for the
    /// MV dwell time on solid, non-hazard ground.
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    [DisallowMultipleComponent]
    public class SafeGroundRecorder : MonoBehaviour
    {
        [Tooltip("The motor whose grounded state is sampled.")]
        [SerializeField] private PlayerMovement movement;

        [Tooltip("Dwell time on safe ground before the position is recorded. SYS1 row 52: 0.2 s.")]
        [Min(0f)] public float dwellSeconds = 0.2f;

        [Tooltip("Hazard layers that make ground unsafe. SYS2 sets these when hazards arrive.")]
        [SerializeField] private LayerMask hazardLayers;

        [Tooltip("Box used to test the ground under the hero for hazards.")]
        [SerializeField] private Vector2 hazardProbeSize = new Vector2(0.5f, 0.1f);

        private float dwell;

        /// <summary>Last position where the hero stood safely for the full dwell time.</summary>
        public Vector2 LastSafeGround { get; private set; }

        /// <summary>True once a safe ground position has been recorded.</summary>
        public bool HasRecorded { get; private set; }

        private void Awake()
        {
            if (movement == null) movement = GetComponent<PlayerMovement>();
            if (movement == null) enabled = false;

            LastSafeGround = transform.position;
        }

        private void FixedUpdate()
        {
            if (movement == null) return;

            bool safe = movement.IsGrounded
                        && !movement.IsKnockedBack
                        && movement.State != MovementState.H_WARP
                        && !IsOnHazard();

            if (!safe)
            {
                dwell = 0f;
                return;
            }

            dwell += Time.fixedDeltaTime;
            if (dwell < dwellSeconds) return;

            LastSafeGround = transform.position;
            HasRecorded = true;
        }

        /// <summary>Pit damage warps here (SYS2 CD-31 / CD-32).</summary>
        public Vector2 WarpTarget => HasRecorded ? LastSafeGround : transform.position;

        private bool IsOnHazard()
        {
            if (hazardLayers.value == 0) return false;

            Vector2 probeCenter = (Vector2)transform.position + Vector2.down * 0.05f;
            return Physics2D.OverlapBox(probeCenter, hazardProbeSize, 0f, hazardLayers) != null;
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(LastSafeGround, 0.15f);
            Gizmos.DrawLine(transform.position, LastSafeGround);
        }
    }
}
