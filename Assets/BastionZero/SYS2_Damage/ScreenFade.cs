using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BastionZero.Damage
{
    /// <summary>
    /// The screen fade shared by the pit warp (CD-42) and death D-3 (CD-32). Builds its own
    /// overlay canvas on first use so no scene has to carry UI for it.
    /// </summary>
    [DisallowMultipleComponent]
    public class ScreenFade : MonoBehaviour
    {
        [Tooltip("Overlay canvas order. Above gameplay UI so the fade actually covers the screen.")]
        [SerializeField] private int sortingOrder = 32000;

        [SerializeField] private Color fadeColour = Color.black;

        private static ScreenFade instance;
        private Image image;

        /// <summary>The active fade, or null before one has been created.</summary>
        public static ScreenFade Instance => instance;

        /// <summary>Current opacity of the overlay, 0 = clear, 1 = fully covered.</summary>
        public float Alpha => image != null ? image.color.a : 0f;

        /// <summary>Finds the fade in the scene, or creates one.</summary>
        public static ScreenFade Ensure()
        {
            if (instance != null) return instance;

            var host = new GameObject("Screen Fade (SYS2)");
            instance = host.AddComponent<ScreenFade>();
            instance.Build();
            return instance;
        }

        private void Awake()
        {
            if (instance == null) instance = this;
            Build();
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        private void Build()
        {
            if (image != null) return;

            var canvas = GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            if (GetComponent<CanvasScaler>() == null) gameObject.AddComponent<CanvasScaler>();
            if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();

            var fadeObject = new GameObject("Fade", typeof(RectTransform), typeof(Image));
            fadeObject.transform.SetParent(transform, false);

            var rect = fadeObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            image = fadeObject.GetComponent<Image>();
            image.color = new Color(fadeColour.r, fadeColour.g, fadeColour.b, 0f);
            image.raycastTarget = false;
        }

        /// <summary>Fades to <paramref name="targetAlpha"/> over <paramref name="duration"/> seconds.</summary>
        public IEnumerator FadeTo(float targetAlpha, float duration)
        {
            Build();

            float start = image.color.a;
            if (duration <= 0f)
            {
                SetAlpha(targetAlpha);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                SetAlpha(Mathf.Lerp(start, targetAlpha, Mathf.Clamp01(elapsed / duration)));
                yield return null;
            }

            SetAlpha(targetAlpha);
        }

        /// <summary>Sets the overlay opacity immediately.</summary>
        public void SetAlpha(float alpha)
        {
            Build();
            image.color = new Color(fadeColour.r, fadeColour.g, fadeColour.b, Mathf.Clamp01(alpha));
        }
    }
}
