using System.Collections;
using System.Collections.Generic;
using BastionZero.Movement;
using UnityEngine;

namespace BastionZero.Damage
{
    /// <summary>
    /// A bottomless pit or the kill plane (SYS2 §9 trigger table). Applies D-Pit damage and
    /// warps the hero to the last safe ground, with the C42 fade and the C29 arrival window.
    /// Decision 1: pits damage and warp, they never kill outright — unless the pips run out,
    /// in which case D-1 takes over.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    [DisallowMultipleComponent]
    public class PitVolume2D : MonoBehaviour
    {
        [Tooltip("SYS2 tuning asset. Required.")]
        [SerializeField] private DamageTuning tuning;

        [Tooltip("Layers that count as the hero's body. Leave as Everything and filter by component.")]
        [SerializeField] private LayerMask heroLayers = ~0;

        [Tooltip("Off makes this a warp-only volume, which is what the kill plane under a room uses.")]
        [SerializeField] private bool applyDamage = true;

        [Tooltip("Take the volume shape from the BoxCollider2D on this object, which is authoring data only.")]
        [SerializeField] private bool useColliderShape = true;

        [SerializeField] private Vector2 volumeSize = Vector2.one;
        [SerializeField] private Vector2 volumeOffset = Vector2.zero;

        private bool warping;
        private BoxCollider2D box;
        private readonly List<Collider2D> overlaps = new List<Collider2D>(8);
        private ContactFilter2D filter;

        /// <summary>True while a warp from this volume is running.</summary>
        public bool IsWarping => warping;

        /// <summary>World-space rect this volume tests against.</summary>
        public Rect WorldVolume
        {
            get
            {
                Vector2 centre = (Vector2)transform.position + volumeOffset;
                return new Rect(centre - volumeSize * 0.5f, volumeSize);
            }
        }

        private void Awake()
        {
            box = GetComponent<BoxCollider2D>();
            if (useColliderShape && box != null)
            {
                // box.size is local, so a volume authored by scaling its transform is only read
                // correctly once the scale is folded in.
                Vector2 scale = transform.lossyScale;
                volumeSize = new Vector2(box.size.x * Mathf.Abs(scale.x), box.size.y * Mathf.Abs(scale.y));
                volumeOffset = new Vector2(box.offset.x * Mathf.Abs(scale.x), box.offset.y * Mathf.Abs(scale.y));
            }

            filter = new ContactFilter2D { useTriggers = true };
            filter.SetLayerMask(heroLayers);
        }

        private void FixedUpdate()
        {
            if (warping || tuning == null) return;
            if (volumeSize.x <= 0f || volumeSize.y <= 0f) return;

            Vector2 centre = (Vector2)transform.position + volumeOffset;
            int count = Physics2D.OverlapBox(centre, volumeSize, 0f, filter, overlaps);

            for (int i = 0; i < count; i++)
            {
                HeroHealth health = overlaps[i].GetComponentInParent<HeroHealth>();
                if (health == null || health.IsDead) continue;

                StartCoroutine(WarpRoutine(health));
                return;
            }
        }

        private IEnumerator WarpRoutine(HeroHealth health)
        {
            warping = true;

            var movement = health.GetComponent<PlayerMovement>();
            var safeGround = health.GetComponent<SafeGroundRecorder>();
            Vector2 target = safeGround != null ? safeGround.WarpTarget : (Vector2)health.transform.position;

            if (applyDamage)
            {
                health.TryTakeDamage(new DamageInfo
                {
                    Class = DamageClass.D_Pit,
                    SourcePosition = (Vector2)transform.position,
                    Surface = HazardSurface.None,
                    EntryVelocityX = health.Velocity.x,
                    Source = this,
                });
            }

            // HP reached 0: D-1 owns the hero now, so no warp from here.
            if (health.IsDead)
            {
                warping = false;
                yield break;
            }

            ScreenFade fade = ScreenFade.Ensure();
            float fadeTime = tuning.CD_42_PitWarpFade;

            yield return fade.FadeTo(1f, fadeTime);

            if (movement != null) movement.WarpTo(target);

            yield return fade.FadeTo(0f, fadeTime);

            if (movement != null) movement.EndWarp();

            // C29: covers hazards as well as actors, so a warp can never loop into damage.
            health.GrantArrivalInvulnerability(tuning.CD_29_WarpArrivalIFrames);

            warping = false;
        }

        private void OnDrawGizmosSelected()
        {
            Rect rect = WorldVolume;
            Gizmos.color = new Color(0.4f, 0.7f, 1f, 0.9f);
            Gizmos.DrawWireCube(rect.center, rect.size);
        }
    }
}
