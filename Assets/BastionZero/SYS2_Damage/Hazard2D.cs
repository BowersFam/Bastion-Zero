using UnityEngine;

namespace BastionZero.Damage
{
    /// <summary>
    /// A static hazard volume — spikes, a saw, a laser segment (SYS2 §10). Tests the hero's
    /// hurtbox every physics step, applies its damage class and owns the per-instance re-hit
    /// lockout (CD-25). Hazards deliberately ignore post-hit i-frames (§8.2); the lockout is
    /// what stops one spike strip from being a guaranteed death.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    [DisallowMultipleComponent]
    public class Hazard2D : MonoBehaviour
    {
        [Tooltip("SYS2 tuning asset. Required.")]
        [SerializeField] private DamageTuning tuning;

        [Tooltip("Damage class this hazard deals. Spikes are D-Hazard (CD-21).")]
        [SerializeField] private DamageClass damageClass = DamageClass.D_Hazard;

        [Tooltip("Which way the hazard pushes on a hit (SYS2 §7 rule 1).")]
        [SerializeField] private HazardSurface surface = HazardSurface.Floor;

        [Tooltip("The hero's hurtbox layer (HeroHurtbox).")]
        [SerializeField] private LayerMask hurtboxLayers;

        [Tooltip("Take the hitbox size and offset from the BoxCollider2D on this object. " +
                 "The collider is authoring data only; it is never used for physics contact.")]
        [SerializeField] private bool useColliderShape = true;

        [SerializeField] private Vector2 hitboxSize = Vector2.one;
        [SerializeField] private Vector2 hitboxOffset = Vector2.zero;

        private float nextHitTime;
        private BoxCollider2D box;

        /// <summary>Seconds until this instance may damage the same hero again (CD-25).</summary>
        public float ReHitTimeLeft => Mathf.Max(0f, nextHitTime - Time.time);

        /// <summary>World-space rect this hazard tests against.</summary>
        public Rect WorldHitbox
        {
            get
            {
                Vector2 size = InsetSize();
                Vector2 centre = (Vector2)transform.position + hitboxOffset;
                return new Rect(centre - size * 0.5f, size);
            }
        }

        private void Awake()
        {
            box = GetComponent<BoxCollider2D>();
            if (useColliderShape && box != null)
            {
                // box.size is local, so a spike strip authored by scaling its transform is only
                // read correctly once the scale is folded in.
                Vector2 scale = transform.lossyScale;
                hitboxSize = new Vector2(box.size.x * Mathf.Abs(scale.x), box.size.y * Mathf.Abs(scale.y));
                hitboxOffset = new Vector2(box.offset.x * Mathf.Abs(scale.x), box.offset.y * Mathf.Abs(scale.y));
            }
        }

        private void FixedUpdate()
        {
            if (tuning == null || Time.time < nextHitTime) return;

            Vector2 size = InsetSize();
            if (size.x <= 0f || size.y <= 0f) return;

            Vector2 centre = (Vector2)transform.position + hitboxOffset;
            Collider2D overlap = Physics2D.OverlapBox(centre, size, 0f, hurtboxLayers);
            if (overlap == null) return;

            HeroHealth health = overlap.GetComponentInParent<HeroHealth>();
            if (health == null) return;

            var info = new DamageInfo
            {
                Class = damageClass,
                SourcePosition = (Vector2)transform.position,
                Surface = surface,
                EntryVelocityX = health.Velocity.x,
                Source = this,
            };

            // Only an accepted hit starts the lockout: a hit refused by the D-5 grace window
            // must not leave the hero unable to be hurt for the next half second.
            if (health.TryTakeDamage(info)) nextHitTime = Time.time + tuning.CD_25_HazardReHitLockout;
        }

        /// <summary>CD-07: the hitbox is inset from its art on every side, biased to the player.</summary>
        private Vector2 InsetSize()
        {
            float inset = tuning != null ? tuning.CD_07_HazardInset * 2f : 0f;
            return new Vector2(Mathf.Max(0.01f, hitboxSize.x - inset), Mathf.Max(0.01f, hitboxSize.y - inset));
        }

        private void OnDrawGizmosSelected()
        {
            Rect rect = WorldHitbox;
            Gizmos.color = new Color(1f, 0.35f, 0.2f, 0.9f);
            Gizmos.DrawWireCube(rect.center, rect.size);
        }
    }
}
