using BastionZero.Damage;
using BastionZero.Movement;
using UnityEngine;

namespace BastionZero.DebugTools
{
    /// <summary>
    /// Step 2 debug tooling: pips, invulnerability windows, the D-1…D-5 death phase and the
    /// safe-ground point the pit warp and respawn will use. F7 toggles it.
    /// Sits under the SYS1 readout so the two stack.
    /// </summary>
    [DisallowMultipleComponent]
    public class DamageStateReadout : MonoBehaviour
    {
        [Tooltip("Health component to read. Bound from this object when left empty.")]
        [SerializeField] private HeroHealth health;

        [Tooltip("Safe-ground recorder to read. Bound from this object when left empty.")]
        [SerializeField] private SafeGroundRecorder safeGround;

        [SerializeField] private bool visible = true;

        private GUIStyle style;

        private void Awake()
        {
            Bind();
        }

        private void Bind()
        {
            if (health == null) health = GetComponent<HeroHealth>();
            if (safeGround == null) safeGround = GetComponent<SafeGroundRecorder>();
        }

        private void Update()
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.f7Key.wasPressedThisFrame) visible = !visible;
        }

        private void OnGUI()
        {
            if (!visible) return;

            Bind();
            if (health == null) return;

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

            int pipCount = Mathf.Max(0, health.CurrentHP);
            string pips = new string('|', pipCount) + new string('.', Mathf.Max(0, health.MaxHP - pipCount));

            string lastHit = health.HasTakenDamage
                ? $"{health.LastDamage.Class} from {(health.LastDamage.Source != null ? health.LastDamage.Source.name : "?")}"
                : "none yet";

            Vector2 safe = safeGround != null ? safeGround.WarpTarget : Vector2.zero;

            string body =
                $"SYS2 damage\n" +
                $"pips       {pips}  {health.CurrentHP}/{health.MaxHP}{(health.IsLowHealth ? "  LOW" : "")}\n" +
                $"i-frames   post-hit {health.PostHitInvulnerability,4:F2} s   grace {health.GraceInvulnerability,4:F2} s\n" +
                $"death      D {health.Phase}   dead {health.IsDead}\n" +
                $"last hit   {lastHit}\n" +
                $"safe land  x {safe.x,7:F2}   y {safe.y,7:F2}\n" +
                $"F7 hide";

            GUI.Box(new Rect(12f, 190f, 420f, 130f), body, style);
        }
    }
}
