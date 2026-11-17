using UnityEngine;

namespace BastionZero.DebugTools
{
    /// <summary>
    /// MV_Gym-only camera: keeps the hero framed while a measurement run is happening.
    /// Not SYS5 camera work — the real C-FOLLOW / C-ZONE / C-ARENA states arrive with the
    /// world system and replace this.
    /// </summary>
    public class GymCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;

        [Tooltip("Camera height above the hero's origin. Positive keeps the hero slightly below " +
                 "the middle of the frame, so the ground he stands on and the ground he drops to " +
                 "are both on screen.")]
        [SerializeField] private float verticalBias = 0.75f;

        [Tooltip("How quickly the camera catches up with the hero. Higher is tighter; " +
                 "0 snaps instantly.")]
        [SerializeField] private float followSharpness = 14f;

        private bool framed;

        public void SetTarget(Transform value)
        {
            target = value;
            framed = false;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            var desired = new Vector3(
                target.position.x,
                target.position.y + verticalBias,
                transform.position.z);

            // First frame after play starts: frame the hero exactly, no drift in from the
            // edit-mode camera position.
            if (!framed)
            {
                transform.position = desired;
                framed = true;
                return;
            }

            float t = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desired, t);
        }
    }
}
