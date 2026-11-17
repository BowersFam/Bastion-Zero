using UnityEngine;

namespace BastionZero.Damage
{
    /// <summary>
    /// The contact hitbox of an actor (SYS2 §6). Enemies are SYS4's, but the contact rule is
    /// SYS2's: a hitbox inset by CD-08, D-Light by default, and a per-overlap re-arm so a hero
    /// who is still standing inside an enemy is not hit again the moment i-frames end (§6 rule 4).
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    [DisallowMultipleComponent]
    public class ContactDamager2D : MonoBehaviour
    {
        [Tooltip("SYS2 tuning asset. Required.")]
        [SerializeField] private DamageTuning tuning;

        [Tooltip("Damage class dealt on contact. D-Light unless the enemy is a heavy brawler.")]
        [SerializeField] private DamageClass damageClass = DamageClass.D_Light;

        [Tooltip("The hero's hurtbox layer (HeroHurtbox).")]
        [SerializeField] private LayerMask hurtboxLayers;

        [Tooltip("SYS2 §6 rule 1: contact damage is on by default; turn it off for enemies whose " +
                 "danger should come only from telegraphed attacks.")]
        [SerializeField] private bool contactDamageEnabled = true;

        [Tooltip("Take the hitbox shape from the BoxCollider2D on this object, which is authoring data only.")]
        [SerializeField] private bool useColliderShape = true;

        [SerializeField] private Vector2 hitboxSize = Vector2.one;
        [SerializeField] private Vector2 hitboxOffset = Vector2.zero;

        private bool armed = true;

        /// <summary>True when the hero may be damaged by this body again.</summary>
        public bool IsArmed => armed;

        /// <summary>
        /// SYS4's live switch for the §15 rule that contact damage is off in E-SPAWN, E-STAGGER
        /// and E-DEAD. Independent of the authored flag and of the component's enabled state,
        /// which SYS3's death hand-off owns.
        /// </summary>
        public bool ContactDamageEnabled
        {
            get => contactDamageEnabled;
            set => contactDamageEnabled = value;
        }

        private void Awake()
        {
            var box = GetComponent<BoxCollider2D>();
            if (useColliderShape && box != null)
            {
                // box.size is local, so a body authored by scaling its transform is only read
                // correctly once the scale is folded in.
                Vector2 scale = transform.lossyScale;
                hitboxSize = new Vector2(box.size.x * Mathf.Abs(scale.x), box.size.y * Mathf.Abs(scale.y));
                hitboxOffset = new Vector2(box.offset.x * Mathf.Abs(scale.x), box.offset.y * Mathf.Abs(scale.y));
            }
        }

        private void FixedUpdate()
        {
            if (!contactDamageEnabled || tuning == null) return;

            // CD-08: the contact hitbox is smaller than the body, so a graze must not hurt.
            float inset = tuning.CD_08_ContactHitboxInset * 2f;
            Vector2 size = new Vector2(
                Mathf.Max(0.01f, hitboxSize.x - inset),
                Mathf.Max(0.01f, hitboxSize.y - inset));

            Vector2 centre = (Vector2)transform.position + hitboxOffset;
            Collider2D overlap = Physics2D.OverlapBox(centre, size, 0f, hurtboxLayers);

            HeroHealth health = overlap != null ? overlap.GetComponentInParent<HeroHealth>() : null;

            if (health == null)
            {
                // §6 rule 4 / CD-26: the overlap has been absent, so contact re-arms.
                armed = true;
                return;
            }

            if (!armed) return;

            armed = false;
            health.TryTakeDamage(new DamageInfo
            {
                Class = damageClass,
                SourcePosition = (Vector2)transform.position,
                Surface = HazardSurface.None,
                EntryVelocityX = health.Velocity.x,
                Source = this,
            });
        }

        private void OnDrawGizmosSelected()
        {
            float inset = tuning != null ? tuning.CD_08_ContactHitboxInset * 2f : 0f;
            Vector2 size = new Vector2(
                Mathf.Max(0.01f, hitboxSize.x - inset),
                Mathf.Max(0.01f, hitboxSize.y - inset));

            Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.9f);
            Gizmos.DrawWireCube((Vector2)transform.position + hitboxOffset, size);
        }
    }
}
