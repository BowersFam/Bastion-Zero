using UnityEngine;

namespace BastionZero.World
{
    /// <summary>
    /// WC-W10 room-bound clamp: the rectangle the camera is never allowed to frame past.
    ///
    /// It lives on the room rather than on the camera, so a room prefab carries its own bounds and
    /// the room loader hands the current one to <see cref="WorldCamera"/> when the room changes. An
    /// axis shorter than the viewport is centred on the room instead of clamped, so a small room
    /// still frames sensibly.
    /// </summary>
    [DisallowMultipleComponent]
    public class CameraRoomBounds : MonoBehaviour
    {
        [Tooltip("Room rectangle in world units (1 tile = 1 unit). Only the x/y of size are used.")]
        [SerializeField] private Vector2 center = Vector2.zero;

        [SerializeField] private Vector2 size = new Vector2(24f, 13.5f);

        [Tooltip("Also draw the rectangle in the Scene view.")]
        [SerializeField] private bool drawGizmo = true;

        /// <summary>The room rectangle in world space.</summary>
        public Rect WorldRect => new Rect(center - size * 0.5f, size);

        /// <summary>Sets the rectangle from code (room prefabs, test harnesses).</summary>
        public void SetBounds(Vector2 worldCenter, Vector2 worldSize)
        {
            center = worldCenter;
            size = worldSize;
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmo) return;
            Rect r = WorldRect;
            Gizmos.color = new Color(0.35f, 0.8f, 1f, 0.9f);
            Gizmos.DrawWireCube(new Vector3(r.center.x, r.center.y, 0f),
                                new Vector3(r.width, r.height, 0f));
        }
    }
}
