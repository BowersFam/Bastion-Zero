using UnityEngine;

namespace BastionZero.Damage
{
    /// <summary>
    /// Applies the SYS2 box values to the hero's authored colliders, so CD-01/CD-02/CD-04/CD-05
    /// are the single source of truth instead of numbers hand-typed into the prefab.
    /// The collision box's bottom stays pinned to the sprite's feet, so changing its height
    /// grows the hero upward rather than sinking him into the floor.
    /// </summary>
    [DisallowMultipleComponent]
    public class HeroBodySetup : MonoBehaviour
    {
        [Tooltip("SYS2 tuning asset. Required.")]
        [SerializeField] private DamageTuning tuning;

        [Tooltip("The hero's collision box (CD-01 width, CD-02 height). Terrain only.")]
        [SerializeField] private CapsuleCollider2D collisionBox;

        [Tooltip("The hero's hurtbox child (CD-04 width, CD-05 height). Receives damage.")]
        [SerializeField] private BoxCollider2D hurtbox;

        [Tooltip("Local y of the sprite's feet relative to the hero root, from the art. " +
                 "The collision box is grown from this line.")]
        [SerializeField] private float feetLocalY = -0.90625f;

        [Tooltip("Local y of the hurtbox centre. 0 keeps it centred on the body, trimming the " +
                 "head and the feet equally.")]
        [SerializeField] private float hurtboxCentreLocalY = 0f;

        public CapsuleCollider2D CollisionBox => collisionBox;
        public BoxCollider2D Hurtbox => hurtbox;

        private void Awake()
        {
            Apply();
        }

        /// <summary>Re-applies the tuning values to both colliders.</summary>
        public void Apply()
        {
            if (tuning == null)
            {
                Debug.LogError("[SYS2] HeroBodySetup has no DamageTuning assigned; collider sizes are unchanged.", this);
                return;
            }

            if (collisionBox != null)
            {
                float height = tuning.CD_02_CollisionBoxHeight;
                collisionBox.size = new Vector2(tuning.CD_01_CollisionBoxWidth, height);
                collisionBox.offset = new Vector2(0f, feetLocalY + height * 0.5f);
            }

            if (hurtbox != null)
            {
                hurtbox.size = new Vector2(tuning.CD_04_HurtboxWidth, tuning.CD_05_HurtboxHeight);
                hurtbox.offset = new Vector2(0f, hurtboxCentreLocalY);
            }
        }

        /// <summary>Used by D-1 / D-2: both boxes go off while the hero is dying.</summary>
        public void SetCollidersEnabled(bool value)
        {
            if (collisionBox != null) collisionBox.enabled = value;
            if (hurtbox != null) hurtbox.enabled = value;
        }
    }
}
