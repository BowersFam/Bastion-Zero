using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// The gem's power-up moment: a burst of golden motes, two expanding rings, a bright
/// core flash and a short-lived point light, all built at runtime so the pickup needs
/// no extra prefab wiring.
/// </summary>
public class GemPowerUpEffect : MonoBehaviour
{
    private static Sprite dotSprite;
    private static Sprite ringSprite;

    private const float TotalDuration = 1.9f;

    private readonly List<Mote> motes = new List<Mote>();
    private readonly List<Ring> rings = new List<Ring>();
    private Light2D flash;
    private SpriteRenderer core;
    private float age;

    /// <summary>Spawns the full power-up burst at a world position.</summary>
    public static void Play(Vector3 position, Color color)
    {
        var go = new GameObject("GemPowerUpFX");
        go.transform.position = position;
        go.AddComponent<GemPowerUpEffect>().Build(color);
    }

    private void Build(Color color)
    {
        EnsureSprites();

        rings.Add(new Ring
        {
            renderer = MakeSprite("Ring_Slow", ringSprite, color, 6, 0f, 0.06f),
            delay = 0f,
            duration = 0.75f,
            startScale = 0.5f,
            endScale = 9f,
        });

        rings.Add(new Ring
        {
            renderer = MakeSprite("Ring_Fast", ringSprite, new Color(1f, 0.99f, 0.86f, 1f), 7, 0f, 0.12f),
            delay = 0.13f,
            duration = 0.55f,
            startScale = 0.3f,
            endScale = 5.5f,
        });

        core = MakeSprite("Core_Flash", dotSprite, new Color(1f, 0.95f, 0.72f, 1f), 8, 0f, 0f);
        core.transform.localScale = Vector3.one * 4.5f;

        for (int i = 0; i < 34; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float speed = Random.Range(1.6f, 5.4f);
            Color tint = Color.Lerp(color, new Color(1f, 0.98f, 0.85f, 1f), Random.value);
            motes.Add(new Mote
            {
                renderer = MakeSprite($"Mote_{i:00}", dotSprite, tint, 9, 0f, 1f),
                velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * 1.15f + 0.55f) * speed,
                gravity = Random.Range(1.2f, 3.4f),
                life = Random.Range(0.45f, 1.05f),
                size = Random.Range(0.16f, 0.42f),
                riseOnly = false,
            });
        }

        // A slower column of motes that keeps drifting up after the burst, so the
        // moment reads as a power-up rather than a one-frame pop.
        for (int i = 0; i < 14; i++)
        {
            motes.Add(new Mote
            {
                renderer = MakeSprite($"Rise_{i:00}", dotSprite, new Color(1f, 0.98f, 0.86f, 1f), 9, 0f, 0.8f),
                velocity = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(1.1f, 2.6f)),
                gravity = -0.35f,
                life = Random.Range(0.9f, 1.6f),
                size = Random.Range(0.2f, 0.5f),
                riseOnly = true,
            });
        }

        flash = gameObject.AddComponent<Light2D>();
        flash.lightType = Light2D.LightType.Point;
        flash.color = color;
        flash.intensity = 0f;
        flash.pointLightOuterRadius = 7f;
        flash.pointLightInnerRadius = 0.6f;
        flash.shadowsEnabled = false;
    }

    private void Update()
    {
        age += Time.deltaTime;

        float burst = Mathf.Clamp01(age / TotalDuration);
        if (core != null)
        {
            float t = Mathf.Clamp01(age / 0.35f);
            core.transform.localScale = Vector3.one * Mathf.Lerp(1.2f, 6.5f, t);
            SetAlpha(core, 1f - t);
        }

        if (flash != null)
        {
            // Fast rise, longer decay, then done.
            float intensity = age < 0.12f
                ? Mathf.Lerp(0f, 7f, age / 0.12f)
                : Mathf.Lerp(7f, 0f, Mathf.Clamp01((age - 0.12f) / 1.1f));
            flash.intensity = intensity;
        }

        for (int i = rings.Count - 1; i >= 0; i--)
        {
            Ring ring = rings[i];
            float t = (age - ring.delay) / ring.duration;
            if (t < 0f) continue;
            if (t >= 1f)
            {
                Destroy(ring.renderer.gameObject);
                rings.RemoveAt(i);
                continue;
            }

            float eased = 1f - (1f - t) * (1f - t);
            ring.renderer.transform.localScale = Vector3.one * Mathf.Lerp(ring.startScale, ring.endScale, eased);
            SetAlpha(ring.renderer, 1f - t);
        }

        for (int i = motes.Count - 1; i >= 0; i--)
        {
            Mote mote = motes[i];
            float t = age / mote.life;
            if (t >= 1f)
            {
                Destroy(mote.renderer.gameObject);
                motes.RemoveAt(i);
                continue;
            }

            mote.velocity.y -= mote.gravity * Time.deltaTime;
            mote.velocity *= 1f - (mote.riseOnly ? 0.4f : 2.2f) * Time.deltaTime;
            mote.renderer.transform.localPosition += (Vector3)(mote.velocity * Time.deltaTime);

            float scale = mote.size * (mote.riseOnly ? Mathf.Lerp(1f, 0.35f, t) : Mathf.Lerp(1.1f, 0.2f, t));
            mote.renderer.transform.localScale = Vector3.one * scale;

            float alpha = mote.riseOnly ? Mathf.SmoothStep(0f, 1f, t / 0.25f) * (1f - t) : 1f - t * t;
            SetAlpha(mote.renderer, alpha);
        }

        if (burst >= 1f && motes.Count == 0 && rings.Count == 0) Destroy(gameObject);
    }

    private SpriteRenderer MakeSprite(string name, Sprite sprite, Color color, int order, float alpha, float scale)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one * scale;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = order;
        renderer.color = new Color(color.r, color.g, color.b, alpha);
        return renderer;
    }

    private static void SetAlpha(SpriteRenderer renderer, float alpha)
    {
        Color c = renderer.color;
        c.a = Mathf.Clamp01(alpha);
        renderer.color = c;
    }

    private static void EnsureSprites()
    {
        if (dotSprite == null) dotSprite = BuildDot();
        if (ringSprite == null) ringSprite = BuildRing();
    }

    private static Sprite BuildDot()
    {
        const int size = 32;
        var pixels = new Color32[size * size];
        float half = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f - half) / half;
                float dy = (y + 0.5f - half) / half;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01(1f - d);
                a = a * a;
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
        }

        return ToSprite(pixels, size, size);
    }

    private static Sprite BuildRing()
    {
        const int size = 64;
        var pixels = new Color32[size * size];
        float half = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f - half) / half;
                float dy = (y + 0.5f - half) / half;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                // Bright rim with a soft inner and outer falloff.
                float rim = Mathf.Clamp01(1f - Mathf.Abs(d - 0.86f) / 0.16f);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(rim * rim * 255f));
            }
        }

        return ToSprite(pixels, size, size);
    }

    private static Sprite ToSprite(Color32[] pixels, int width, int height)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
        };
        texture.SetPixels32(pixels);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 32f);
        sprite.name = "GemFX";
        return sprite;
    }

    private struct Mote
    {
        public SpriteRenderer renderer;
        public Vector2 velocity;
        public float gravity;
        public float life;
        public float size;
        public bool riseOnly;
    }

    private struct Ring
    {
        public SpriteRenderer renderer;
        public float delay;
        public float duration;
        public float startScale;
        public float endScale;
    }
}
