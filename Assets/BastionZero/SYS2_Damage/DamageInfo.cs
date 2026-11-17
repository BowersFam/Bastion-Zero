using UnityEngine;

namespace BastionZero.Damage
{
    /// <summary>
    /// One damage event on its way into <see cref="HeroHealth"/>. Everything a source knows about
    /// the hit that decides pips, knockback direction and same-frame priority (SYS2 §1.7, §7.1).
    /// </summary>
    public struct DamageInfo
    {
        /// <summary>Which damage class the hit belongs to (SYS2 §5 rule 2).</summary>
        public DamageClass Class;

        /// <summary>World position of the source, used for the "away from the source" rule.</summary>
        public Vector2 SourcePosition;

        /// <summary>Hazard surface that produced the hit; <see cref="HazardSurface.None"/> for actors.</summary>
        public HazardSurface Surface;

        /// <summary>The hero's horizontal speed as the hit resolved; floor spikes push against it.</summary>
        public float EntryVelocityX;

        /// <summary>The component that produced the hit, for debug readouts and later systems.</summary>
        public Component Source;

        /// <summary>
        /// SYS2 §1.7: instant-death volume, then hazard, then enemy hitbox, then a bounce grant.
        /// Lower wins when two sources land on the same frame.
        /// </summary>
        public int Priority
        {
            get
            {
                switch (Class)
                {
                    case DamageClass.D_Lethal: return 0;
                    case DamageClass.D_Pit: return 1;
                    case DamageClass.D_Hazard: return 1;
                    default: return 2;
                }
            }
        }
    }
}
