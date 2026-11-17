namespace BastionZero.Damage
{
    /// <summary>
    /// SYS2 §5 rule 2 — damage classes. Designers never pick raw numbers, only a class; the
    /// pip value of each class lives in <see cref="DamageTuning"/> under its CD-19…CD-41 field.
    /// </summary>
    public enum DamageClass
    {
        /// <summary>CD-19 — enemy contact, small projectiles. 1 pip.</summary>
        D_Light = 0,

        /// <summary>CD-20 — telegraphed heavy attacks, boss contact. 2 pips.</summary>
        D_Heavy = 1,

        /// <summary>CD-21 — spikes, saws, lasers. 3 pips. Ignores post-hit i-frames (SYS2 §8.2).</summary>
        D_Hazard = 2,

        /// <summary>CD-41 — bottomless pits and the kill plane. 2 pips plus the safe-ground warp.</summary>
        D_Pit = 3,

        /// <summary>Lava, crush. All pips; ignores reduction and every i-frame.</summary>
        D_Lethal = 4,
    }

    /// <summary>
    /// SYS2 §7 rule 1 — the surface a hazard presents, which decides the knockback direction.
    /// </summary>
    public enum HazardSurface
    {
        /// <summary>Not a surface hazard (saw, laser, enemy): push away from the source centre.</summary>
        None = 0,

        /// <summary>Floor spikes: up and toward the side the hero came from.</summary>
        Floor = 1,

        /// <summary>Ceiling spikes: down.</summary>
        Ceiling = 2,

        /// <summary>Wall spikes on the hero's left: push right.</summary>
        LeftWall = 3,

        /// <summary>Wall spikes on the hero's right: push left.</summary>
        RightWall = 4,
    }
}
