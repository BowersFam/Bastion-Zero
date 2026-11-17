using BastionZero.Movement;
using UnityEngine;

namespace BastionZero.DebugTools
{
    /// <summary>
    /// Step 1 debug tooling: on-screen readout of the SYS1 state machine, a fixed-timestep
    /// sanity check against the spec's 60 fps, and single-step simulation.
    /// F8 toggles the readout, F9 pauses the motor, F10 advances exactly one physics step.
    /// </summary>
    [DisallowMultipleComponent]
    public class MovementStateReadout : MonoBehaviour
    {
        [Tooltip("Motor to read. Bound from this object when left empty.")]
        [SerializeField] private PlayerMovement movement;

        [SerializeField] private bool visible = true;
        [SerializeField] private bool paused;

        private GUIStyle style;

        private void Awake()
        {
            Bind();
        }

        private void Bind()
        {
            if (movement == null) movement = GetComponent<PlayerMovement>();
        }

        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                var keyboard = UnityEngine.InputSystem.Keyboard.current;

                if (keyboard.f8Key.wasPressedThisFrame) visible = !visible;

                if (keyboard.f9Key.wasPressedThisFrame)
                {
                    paused = !paused;
                    if (movement != null) movement.SetManualStep(paused);
                }

                if (keyboard.f10Key.wasPressedThisFrame && paused && movement != null)
                {
                    movement.StepOnce();
                }
            }
        }

        private void OnDisable()
        {
            if (movement != null) movement.SetManualStep(false);
        }

        private void OnGUI()
        {
            if (!visible) return;

            Bind();
            if (movement == null) return;

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

            bool timestepOk = Mathf.Abs(Time.fixedDeltaTime - BastionZero.Core.SpecUnits.FixedDeltaTime) < 0.0005f;

            string body =
                $"SYS1 movement   {(paused ? "[PAUSED — F10 steps]" : "")}\n" +
                $"state      {movement.State}  (was {movement.PreviousState})\n" +
                $"velocity   x {movement.Velocity.x,7:F2}   y {movement.Velocity.y,7:F2}\n" +
                $"grounded   {movement.IsGrounded}    clinging {movement.IsClinging}\n" +
                $"coyote     {movement.CoyoteTimer,5:F2} s   buffer armed {movement.HasBufferedJump}\n" +
                $"re-cling   {movement.ReClingTimer,5:F2} s   head-bump {movement.HeadBumpTimer,5:F2} s\n" +
                $"air jumps  {movement.AirJumpsRemaining}   wall-cling owned {BastionZero.Core.HeroAbilities.WallClingUnlocked}\n" +
                $"fixed dt   {Time.fixedDeltaTime:F5} s   {(timestepOk ? "60 Hz OK" : "NOT 1/60 — fix TimeManager")}\n" +
                $"F8 hide   F9 pause   F10 step one physics tick";

            GUI.Box(new Rect(12f, 12f, 420f, 168f), body, style);
        }
    }
}
