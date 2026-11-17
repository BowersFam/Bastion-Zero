using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Visible payoff for reaching the level exit. Builds its own screen-space overlay at
/// runtime, listens for <see cref="LevelExit.Completed"/> and fades in a completion card,
/// so reaching the exit reads as an event instead of a console line.
/// </summary>
[RequireComponent(typeof(LevelExit))]
public class LevelCompleteOverlay : MonoBehaviour
{
    [Header("Copy")]
    [SerializeField] private string title = "SANCTUM REACHED";
    [SerializeField] private string subtitle = "The forest path continues. Nexus's wings are awake.";

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 1.1f;
    [SerializeField] private float holdBeforeText = 0.25f;

    [Header("Colours")]
    [SerializeField] private Color veilColor = new Color(0.02f, 0.03f, 0.04f, 0.82f);
    [SerializeField] private Color titleColor = new Color(0.94f, 0.80f, 0.45f, 1f);
    [SerializeField] private Color subtitleColor = new Color(0.80f, 0.83f, 0.86f, 1f);

    private LevelExit exit;
    private CanvasGroup group;
    private Text titleText;
    private Text subtitleText;
    private RectTransform rule;
    private Image veil;
    private bool shown;

    private void Awake()
    {
        exit = GetComponent<LevelExit>();
        Build();
        SetHidden();
    }

    private void OnEnable()
    {
        if (exit != null) exit.Completed += HandleCompleted;
    }

    private void OnDisable()
    {
        if (exit != null) exit.Completed -= HandleCompleted;
    }

    private void HandleCompleted(LevelExit _)
    {
        if (shown) return;
        shown = true;
        StartCoroutine(Reveal());
    }

    private IEnumerator Reveal()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeDuration);
            group.alpha = Mathf.SmoothStep(0f, 1f, k);

            Color veilTint = veilColor;
            veilTint.a = veilColor.a * Mathf.SmoothStep(0f, 1f, k);
            veil.color = veilTint;
            yield return null;
        }

        group.alpha = 1f;

        // The title settles in slightly after the veil, so the moment has a beat to it.
        float w = 0f;
        while (w < holdBeforeText)
        {
            w += Time.unscaledDeltaTime;
            yield return null;
        }

        float slide = 0f;
        const float slideDuration = 0.5f;
        while (slide < slideDuration)
        {
            slide += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(slide / slideDuration);
            float eased = 1f - (1f - k) * (1f - k);
            titleText.transform.localScale = Vector3.one * Mathf.Lerp(0.92f, 1f, eased);
            titleText.color = new Color(titleColor.r, titleColor.g, titleColor.b, eased);
            subtitleText.color = new Color(subtitleColor.r, subtitleColor.g, subtitleColor.b, eased);
            rule.localScale = new Vector3(Mathf.Lerp(0f, 1f, eased), 1f, 1f);
            yield return null;
        }
    }

    private void SetHidden()
    {
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        Color veilTint = veilColor;
        veilTint.a = 0f;
        veil.color = veilTint;

        titleText.color = new Color(titleColor.r, titleColor.g, titleColor.b, 0f);
        subtitleText.color = new Color(subtitleColor.r, subtitleColor.g, subtitleColor.b, 0f);
        rule.localScale = new Vector3(0f, 1f, 1f);
    }

    private void Build()
    {
        var canvasGo = new GameObject("LevelCompleteCanvas", typeof(Canvas), typeof(CanvasScaler));
        canvasGo.transform.SetParent(transform, false);

        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        group = canvasGo.AddComponent<CanvasGroup>();

        veil = MakeImage(canvasGo.transform, "Veil", veilColor, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        RectTransform content = MakeRect(canvasGo.transform, "Content");
        content.anchorMin = new Vector2(0.5f, 0.5f);
        content.anchorMax = new Vector2(0.5f, 0.5f);
        content.pivot = new Vector2(0.5f, 0.5f);
        content.anchoredPosition = new Vector2(0f, 24f);
        content.sizeDelta = new Vector2(1400f, 400f);

        rule = MakeImage(content, "Rule", titleColor, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(560f, 3f)).rectTransform;
        rule.anchoredPosition = new Vector2(0f, 96f);

        Font font = LoadFont();
        titleText = MakeText(content, "Title", title, font, 84, FontStyle.Bold, titleColor, new Vector2(0f, 20f));
        subtitleText = MakeText(content, "Subtitle", subtitle, font, 30, FontStyle.Normal, subtitleColor, new Vector2(0f, -74f));
    }

    private static Font LoadFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font == null) Debug.LogWarning("[LevelCompleteOverlay] No built-in font available; the card will render without text.");
        return font;
    }

    private static RectTransform MakeRect(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    private static Image MakeImage(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 size)
    {
        RectTransform rect = MakeRect(parent, name);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);

        if (anchorMin == anchorMax)
        {
            rect.sizeDelta = size;
            rect.anchoredPosition = offsetMin;
        }
        else
        {
            rect.offsetMin = offsetMin;
            rect.offsetMax = Vector2.zero;
        }

        var image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static Text MakeText(Transform parent, string name, string content, Font font, int size, FontStyle style,
        Color color, Vector2 position)
    {
        RectTransform rect = MakeRect(parent, name);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(1400f, size * 2.2f);
        rect.anchoredPosition = position;

        var text = rect.gameObject.AddComponent<Text>();
        text.font = font;
        text.text = content;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }
}
