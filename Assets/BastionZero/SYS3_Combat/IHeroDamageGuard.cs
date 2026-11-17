using BastionZero.Damage;

namespace BastionZero.Combat
{
    /// <summary>
    /// SYS3 §5 and §10: the guard layer sits between a damage source and
    /// <see cref="HeroHealth"/>. A guard may negate a hit (block, parry, dash i-frames)
    /// and, for a blocked D-Heavy, pass chip damage back as raw pips.
    ///
    /// Hazards, pits, lethal volumes and the kill plane are never offered here —
    /// decision (a) and S2-doc §8 rule 2 — so an implementation never sees those classes.
    /// </summary>
    public interface IHeroDamageGuard
    {
        /// <summary>
        /// Returns true when the guard consumed the hit. <paramref name="chipPips"/> is applied by
        /// the health system as raw damage; §5.3 keeps chip from taking the last pip.
        /// </summary>
        bool TryGuard(ref DamageInfo info, out int chipPips);
    }
}
