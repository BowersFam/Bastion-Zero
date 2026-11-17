using System.Collections.Generic;
using UnityEngine;

namespace BastionZero.Core
{
    /// <summary>
    /// Run-scoped record of which traversal abilities the hero owns (glossary: "Ability",
    /// SYS6 §3 owns the unlock order and sources; SYS1 only reads the flags).
    /// Memory-only by design: a fresh run always starts on the base kit, and nothing here is
    /// written to disk — persistence is SYS6's job and arrives with the rest of the slice.
    /// </summary>
    public static class HeroAbilities
    {
        /// <summary>Traversal abilities in the order SYS6 §3.2 grants them.</summary>
        public enum Ability
        {
            /// <summary>PE-U1 — the vertical slice's unlock; gates GT1 and GT2.</summary>
            WallCling = 0,

            /// <summary>PE-U-slice deferred: glide is not in the vertical slice (13 §5.1).</summary>
            Glide = 1,

            /// <summary>Later unlock (deferred past the slice).</summary>
            Grapple = 2,

            /// <summary>PE-U11 — later unlock, excluded from the slice by DB-3 / FD-7.</summary>
            AirDash = 3,

            /// <summary>Projectile bounce, later unlock.</summary>
            ProjectileBounce = 4,

            /// <summary>Glide down-strike, later unlock.</summary>
            GlideDownStrike = 5,

            /// <summary>Recovery jump, later unlock.</summary>
            RecoveryJump = 6,
        }

        private static readonly HashSet<Ability> owned = new HashSet<Ability>();

        /// <summary>True when the hero owns the ability.</summary>
        public static bool Has(Ability ability) => owned.Contains(ability);

        /// <summary>Shorthand for the slice's wall-cling gate (PE-U1).</summary>
        public static bool WallClingUnlocked => Has(Ability.WallCling);

        /// <summary>Grants an ability for the rest of the run. Safe to call repeatedly.</summary>
        public static void Unlock(Ability ability)
        {
            if (!owned.Add(ability)) return;

            Debug.Log($"[HeroAbilities] Unlocked {ability} for this run.");
        }

        /// <summary>Clears every ability. Called by the run initializer before gameplay starts.</summary>
        public static void ResetForNewRun()
        {
            owned.Clear();
        }

        /// <summary>Base kit: no traversal abilities owned.</summary>
        public static void ResetToBaseKit()
        {
            owned.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            // Domain entry (including every editor play session) always starts on the base kit.
            owned.Clear();
        }
    }
}
