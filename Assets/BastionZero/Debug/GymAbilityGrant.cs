using BastionZero.Core;
using UnityEngine;

namespace BastionZero.DebugTools
{
    /// <summary>
    /// MV_Gym-only: grants the slice's traversal ability so W_CLING and W_JUMP can be
    /// exercised. The shipped flow starts on the base kit and unlocks wall-cling from the
    /// ability room behind the elite (PE-U1, FD-10); this component exists only in the gym.
    /// </summary>
    public class GymAbilityGrant : MonoBehaviour
    {
        [SerializeField] private bool grantWallCling = true;

        private void Awake()
        {
            if (grantWallCling) HeroAbilities.Unlock(HeroAbilities.Ability.WallCling);
        }

        private void OnDestroy()
        {
            HeroAbilities.ResetToBaseKit();
        }
    }
}
