using System.Collections.Generic;
using UnityEngine;

namespace BastionZero.Combat
{
    /// <summary>
    /// The sword's active volume (SYS3 §3, the "Active" window). The window itself is owned by
    /// <see cref="HeroCombat"/>; this component only sweeps the authored box on demand.
    ///
    /// The sweep is explicit rather than a trigger collider, so the first active frame always
    /// connects on a target that is already overlapping (a collider enabled on that frame would
    /// otherwise wait for the next physics step). The per-swing set implements S1 row 25: one hit
    /// per target per swing.
    /// </summary>
    public class SlashVolume2D : MonoBehaviour
    {
        [Tooltip("Layers searched for targets. Trigger colliders are included.")]
        [SerializeField] private LayerMask targetLayers = ~0;

        [Tooltip("Authored for the right-facing case; x is mirrored when the hero faces left.")]
        [SerializeField] private Vector2 localOffset = new Vector2(0.62f, 0.05f);

        [SerializeField] private Vector2 size = new Vector2(1.1f, 0.9f);

        private readonly List<Collider2D> overlaps = new List<Collider2D>(8);
        private readonly List<IAttackTarget2D> found = new List<IAttackTarget2D>(4);
        private readonly HashSet<IAttackTarget2D> hitThisSwing = new HashSet<IAttackTarget2D>();
        private ContactFilter2D filter;
        private bool filterReady;
        private Transform cached;

        /// <summary>Targets the current swing has already delivered to.</summary>
        public int SwingHitCount => hitThisSwing.Count;

        /// <summary>Where the volume sits this frame, for debug drawing and readouts.</summary>
        public Vector2 WorldCenter
        {
            get
            {
                Transform t = cached != null ? cached : transform;
                Vector2 offset = localOffset;
                if (t.lossyScale.x < 0f) offset.x = -offset.x;
                return t.TransformPoint(offset);
            }
        }

        /// <summary>Re-poses the volume for the attack that is about to run (§3.2 context attacks).</summary>
        public void Pose(Vector2 offset, Vector2 boxSize)
        {
            localOffset = offset;
            size = boxSize;
        }

        /// <summary>Opens a swing: the per-target lockout clears.</summary>
        public void BeginSwing()
        {
            hitThisSwing.Clear();
        }

        /// <summary>
        /// Sweeps once and returns every target not yet hit this swing. The caller builds and
        /// resolves the hit, then calls <see cref="MarkHit"/> for each target it delivered to.
        /// </summary>
        public List<IAttackTarget2D> Poll(bool facingLeft)
        {
            found.Clear();

            Transform t = cached != null ? cached : (cached = transform);
            Vector2 offset = localOffset;
            if (facingLeft) offset.x = -offset.x;

            Vector3 scale = t.lossyScale;
            Vector2 worldSize = new Vector2(
                size.x * Mathf.Abs(scale.x),
                size.y * Mathf.Abs(scale.y));

            if (!filterReady)
            {
                filter = new ContactFilter2D
                {
                    useTriggers = true,
                    useLayerMask = true,
                    layerMask = targetLayers,
                };
                filterReady = true;
            }

            overlaps.Clear();
            Physics2D.OverlapBox(t.TransformPoint(offset), worldSize, t.eulerAngles.z, filter, overlaps);

            for (int i = 0; i < overlaps.Count; i++)
            {
                Collider2D other = overlaps[i];
                if (other == null) continue;

                IAttackTarget2D target = other.GetComponentInParent<IAttackTarget2D>();
                if (target == null) continue;
                if (hitThisSwing.Contains(target)) continue;

                found.Add(target);
            }

            return found;
        }

        /// <summary>Registers a delivered target so the rest of the active frames skip it.</summary>
        public void MarkHit(IAttackTarget2D target)
        {
            if (target != null) hitThisSwing.Add(target);
        }

        private void OnDrawGizmosSelected()
        {
            Transform t = transform;
            Vector3 scale = t.lossyScale;
            Gizmos.matrix = Matrix4x4.TRS(
                t.TransformPoint(localOffset),
                t.rotation,
                new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), 1f));
            Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.35f);
            Gizmos.DrawCube(Vector3.zero, size);
        }
    }
}
