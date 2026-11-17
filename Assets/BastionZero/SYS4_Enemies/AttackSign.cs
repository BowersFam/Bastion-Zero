using UnityEngine;

namespace BastionZero.Enemies
{
    /// <summary>
    /// SYS4 §3.2 / §14: the universal attack sign. Decision (g) has it on by default; it appears
    /// at the start of E-TELEGRAPH and vanishes when the attack starts, so the sign-to-hit delay
    /// is always the telegraph length (§3.2 "no hidden delays").
    ///
    /// Place this component on the sign's own child object, not on the enemy root: the
    /// §14 size slider is applied to this transform's local scale.
    /// </summary>
    [DisallowMultipleComponent]
    public class AttackSign : MonoBehaviour
    {
        [Tooltip("The sprite that draws the sign. Found on this object when left empty.")]
        [SerializeField] private SpriteRenderer signRenderer;

        [Tooltip("§14 'Attack sign on/off'. Decision (g): on by default.")]
        [SerializeField] private bool signsEnabled = true;

        private bool visible;

        /// <summary>True while the sign is drawn.</summary>
        public bool IsVisible => visible;

        /// <summary>§14 on/off hook for the options menu.</summary>
        public bool SignsEnabled
        {
            get => signsEnabled;
            set => signsEnabled = value;
        }

        private void Awake()
        {
            if (signRenderer == null) signRenderer = GetComponent<SpriteRenderer>();
            Hide();
        }

        /// <summary>Draws the sign in the given colour at the given §14 scale.</summary>
        public void Show(Color colour, float scale)
        {
            if (!signsEnabled || signRenderer == null) return;

            visible = true;
            signRenderer.enabled = true;
            signRenderer.color = colour;
            transform.localScale = Vector3.one * Mathf.Clamp(scale, 0.1f, 2f);
        }

        /// <summary>Hides the sign.</summary>
        public void Hide()
        {
            visible = false;
            if (signRenderer != null) signRenderer.enabled = false;
        }
    }
}
