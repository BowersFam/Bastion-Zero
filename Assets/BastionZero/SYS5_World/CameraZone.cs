using UnityEngine;

namespace BastionZero.World
{
    /// <summary>
    /// SYS5 §10.1 <b>C-ZONE</b>: an authored framing volume. While the hero is inside, the camera
    /// locks one axis or holds a fixed anchor — shafts, gauntlets and reveal shots. Leaving the
    /// volume eases back to C-FOLLOW over WC-W15 (0.4 s).
    /// </summary>
    [DisallowMultipleComponent]
    public class CameraZone : MonoBehaviour
    {
        public enum ZoneMode
        {
            /// <summary>Follow vertically, hold the authored x (a shaft).</summary>
            LockHorizontal = 0,

            /// <summary>Follow horizontally, hold the authored y (a gauntlet corridor).</summary>
            LockVertical = 1,

            /// <summary>Ignore the hero entirely and frame the anchor.</summary>
            FixedAnchor = 2,
        }

        [SerializeField] private ZoneMode mode = ZoneMode.LockHorizontal;

        [Tooltip("Trigger volume in world units (1 tile = 1 unit). Only the x/y of size are used.")]
        [SerializeField] private Vector2 center = Vector2.zero;
        [SerializeField] private Vector2 size = new Vector2(24f, 13.5f);

        [Tooltip("The point the zone frames. Left at zero it uses the volume's own centre.")]
        [SerializeField] private Vector2 anchor = Vector2.zero;

        [Tooltip("Also draw the volume in the Scene view.")]
        [SerializeField] private bool drawGizmo = true;

        /// <summary>Which axis the zone takes over.</summary>
        public ZoneMode Mode => mode;

        /// <summary>The trigger volume in world space.</summary>
        public Rect WorldRect => new Rect(center - size * 0.5f, size);

        /// <summary>The point this zone frames.</summary>
        public Vector2 Anchor => anchor == Vector2.zero ? center : anchor;

        /// <summary>True when a world point is inside the volume (z ignored).</summary>
        public bool Contains(Vector2 worldPoint) => WorldRect.Contains(worldPoint);

        /// <summary>Sets the volume from code (room prefabs, test harnesses).</summary>
        public void SetZone(ZoneMode zoneMode, Vector2 worldCenter, Vector2 worldSize, Vector2 worldAnchor)
        {
            mode = zoneMode;
            center = worldCenter;
            size = worldSize;
            anchor = worldAnchor;
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmo) return;
            Rect r = WorldRect;
            Gizmos.color = new Color(1f, 0.8f, 0.3f, 0.9f);
            Gizmos.DrawWireCube(new Vector3(r.center.x, r.center.y, 0f),
                                new Vector3(r.width, r.height, 0f));
            Gizmos.DrawSphere(new Vector3(Anchor.x, Anchor.y, 0f), 0.2f);
        }
    }
}
