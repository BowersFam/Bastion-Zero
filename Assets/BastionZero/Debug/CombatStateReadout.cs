using BastionZero.Combat;
using UnityEngine;

namespace BastionZero.DebugTools
{
    /// <summary>
    /// Step 3 debug tooling: the combat overlay state, the chain and the buffers, the guard, the
    /// dash cooldown, the feel layer and the first enemy in the scene. F8 toggles it.
    /// Sits under the SYS2 readout so the three stack.
    /// </summary>
    [DisallowMultipleComponent]
    public class CombatStateReadout : MonoBehaviour
    {
        [Tooltip("Combat component to read. Bound from this object when left empty.")]
        [SerializeField] private HeroCombat combat;

        [Tooltip("Hit-stop controller to read. Searched once when left empty.")]
        [SerializeField] private HitStopController hitStop;

        [SerializeField] private bool visible = true;

        private GUIStyle style;
        private EnemyCombatTarget target;
        private float nextTargetSearch;

        /// <summary>The enemy the readout is showing, or null when the scene has none.</summary>
        public EnemyCombatTarget Target => target;

        private void Awake()
        {
            Bind();
        }

        private void Bind()
        {
            if (combat == null) combat = GetComponent<HeroCombat>();
            if (hitStop == null) hitStop = FindAnyObjectByType<HitStopController>(FindObjectsInactive.Exclude);
        }

        private void Update()
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.f8Key.wasPressedThisFrame) visible = !visible;
        }

        private void OnGUI()
        {
            if (!visible) return;

            Bind();
            if (combat == null) return;

            if (style == null)
            {
                style = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.UpperLeft,
                    fontSize = 13,
                    richText = false,
                };
                style.normal.textColor = Color.white;
            }

            // OnGUI runs at least twice a frame, so the scene is only swept when the current
            // target is gone and a moment has passed since the last sweep.
            if (target == null && Time.unscaledTime >= nextTargetSearch)
            {
                nextTargetSearch = Time.unscaledTime + 1f;
                target = FindAnyObjectByType<EnemyCombatTarget>(FindObjectsInactive.Exclude);
            }

            string guard = $"{combat.GuardPoints}/{combat.GuardMaxPoints}" +
                (combat.IsGuardBroken ? "  BREAK" : combat.IsParryWindowOpen ? "  PARRY" : "");

            string stop = hitStop == null
                ? "no controller"
                : $"local {(hitStop.IsLocalStopped ? hitStop.LocalStopFrames + " f" : "—")}   " +
                  $"global {(hitStop.IsGlobalStopped ? "on" : "—")}";

            string body =
                $"SYS3 combat\n" +
                $"state      {combat.State}   frame {combat.FrameInState,3}   attack {combat.CurrentAttack}   chain {combat.ChainIndex}\n" +
                $"memory     combo {combat.ComboMemoryRemaining,4:F2} s   air-chain {combat.AirborneHitChain}   tier {combat.WeaponTier}\n" +
                $"guard      {guard}   blockstun {combat.BlockstunRemaining,4:F2} s   break {combat.GuardBreakRemaining,4:F2} s\n" +
                $"dash       cd {combat.DashCooldownRemaining,4:F2} s   i-frames {combat.DashIFramesActive}   negations {combat.DashIFrameNegations}\n" +
                $"landed     hits {combat.HitsLanded}  crits {combat.CritsLanded}  blocks {combat.BlocksLanded}  parries {combat.ParriesLanded}\n" +
                $"stop       {stop}\n" +
                $"last       {combat.LastEvent}\n" +
                EnemyLine() +
                $"F8 hide";

            GUI.Box(new Rect(12f, 330f, 460f, 172f), body, style);
        }

        private string EnemyLine()
        {
            if (target == null) return "enemy      none in scene\n";

            string poise = target.IsLightTier
                ? "flinch tier"
                : $"{target.Poise:F0}/{target.PoiseThreshold:F0}" +
                  (target.IsStaggered ? "  STAGGER" : "");

            return $"enemy      {target.name} {target.Tier}  HP {target.CurrentHP:F0}/{target.MaxHP:F0}  " +
                   $"poise {poise}  hits {target.HitsTaken}\n";
        }
    }
}
