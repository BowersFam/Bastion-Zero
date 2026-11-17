using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Gives a static brazier sprite a living flame: the flame and its glow breathe on
/// per-brazier noise, and the point light pulses with them.
/// </summary>
public class BrazierFlicker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer flame;
    [SerializeField] private SpriteRenderer glow;
    [SerializeField] private Light2D light2D;

    [Header("Flicker")]
    [SerializeField] private float baseIntensity = 1.15f;
    [SerializeField] private float intensityVariance = 0.35f;
    [SerializeField] private float radiusVariance = 0.8f;
    [SerializeField] private float speed = 2.4f;

    private Vector3 flameBaseScale;
    private Vector3 glowBaseScale;
    private Color glowBaseColor;
    private float seed;

    private void Awake()
    {
        seed = Mathf.Abs(GetEntityId().GetHashCode() % 1000) * 0.017f;

        if (flame != null) flameBaseScale = flame.transform.localScale;
        if (glow != null)
        {
            glowBaseScale = glow.transform.localScale;
            glowBaseColor = glow.color;
        }

        if (light2D != null) baseIntensity = light2D.intensity;
    }

    private void Update()
    {
        float t = Time.time * speed + seed;
        float noise = Mathf.PerlinNoise(t, seed) - 0.5f;
        float grain = Mathf.PerlinNoise(t * 3.7f, seed + 4.1f) - 0.5f;
        float flicker = noise + grain * 0.45f;

        if (flame != null)
        {
            flame.transform.localScale = new Vector3(
                flameBaseScale.x * (1f + flicker * 0.16f),
                flameBaseScale.y * (1f + flicker * 0.34f),
                flameBaseScale.z);
        }

        if (glow != null)
        {
            float pulse = 1f + flicker * 0.22f;
            glow.transform.localScale = glowBaseScale * pulse;
            glow.color = new Color(glowBaseColor.r, glowBaseColor.g, glowBaseColor.b,
                glowBaseColor.a * (1f + flicker * 0.35f));
        }

        if (light2D != null)
        {
            light2D.intensity = baseIntensity + flicker * intensityVariance;
            light2D.pointLightOuterRadius = 5.5f + flicker * radiusVariance;
        }
    }
}
