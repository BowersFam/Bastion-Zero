using System;
using System.Reflection;
using UnityEngine;
using BastionZero.Movement;

/// <summary>
/// Centralises the moment-to-moment feel of Nexus: landing dust scaled by impact
/// speed, a spark on a connected slash, drifting motes during the glide, and short
/// wordless cues for the level's pickups, puzzle responses and exit.
/// Everything here is optional — missing clips, missing scene event sources, a missing
/// impulse source or a missing hitbox degrade to "no feedback" and never block
/// gameplay. Built procedurally so no extra prefabs or art wiring are required.
/// </summary>
[DisallowMultipleComponent]
public class NexusFeelFeedback : MonoBehaviour
{
    private static Sprite dotSprite;

    [Header("References")]
    [Tooltip("Bound automatically from this object when left empty.")]
    [SerializeField] private PlayerMovement movement;
    [Tooltip("Optional. Subscribed for the slash-impact spark.")]
    [SerializeField] private AttackHitbox2D hitbox;
    [Tooltip("Optional one-shot source. Left empty, feedback is visual only.")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("The character's body renderer; motes draw two sorting orders above it. Found on the VisualRoot child when left empty.")]
    [SerializeField] private SpriteRenderer characterRenderer;

    [Header("Clips (optional)")]
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip landClip;
    [SerializeField] private AudioClip slashClip;
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private AudioClip gemClip;
    [SerializeField] private AudioClip puzzleClip;
    [SerializeField] private AudioClip exitClip;

    [Header("Scene Event Sources (optional)")]
    [Tooltip("Scene-only wiring. Standard pickups arrive through Collectible.AnyCollected.")]
    [SerializeField] private PressurePlatePuzzle pressurePlate;
    [SerializeField] private StrikeSwitchPuzzle strikeSwitch;
    [SerializeField] private AttackableBramble bramble;
    [SerializeField] private WingGemPickup wingGem;
    [SerializeField] private LevelExit levelExit;

    [Header("Impact Thresholds")]
    [Tooltip("Landing slower than this produces no dust and no camera impulse.")]
    [SerializeField] private float minLandingSpeed = 4f;
    [Tooltip("Landing speed treated as a full-strength impact.")]
    [SerializeField] private float hardLandingSpeed = 14f;

    [Header("Colours")]
    [SerializeField] private Color dustColor = new Color(0.78f, 0.83f, 0.79f, 1f);
    [SerializeField] private Color slashColor = new Color(1f, 0.94f, 0.76f, 1f);
    [SerializeField] private Color pickupColor = new Color(0.95f, 0.92f, 0.62f, 1f);
    [SerializeField] private Color puzzleColor = new Color(0.82f, 0.66f, 1f, 1f);
    [SerializeField] private Color brambleColor = new Color(0.68f, 0.86f, 0.46f, 1f);

    [Header("Camera Impulse")]
    [Tooltip("Optional component exposing GenerateImpulse(float). Left empty, no camera shake is produced.")]
    [SerializeField] private Component impulseSource;
    [SerializeField] private string impulseMethodName = "GenerateImpulse";
    [SerializeField] private float maxImpulseStrength = 0.35f;

    private MethodInfo impulseMethod;
    private AudioSource resolvedAudio;
    private bool bound;

    /// <summary>Motes draw above the character (order 10 → 12), or at 11 when no renderer resolves.</summary>
    private int MoteSortingOrder => characterRenderer != null ? characterRenderer.sortingOrder + 2 : 11;

    /// <summary>Dust starts at least this far either side of the feet, outside the silhouette.</summary>
    private const float DustMinSideOffset = 0.6f;
    private const float DustMaxSideOffset = 0.9f;

    private void Awake()
    {
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (hitbox == null) hitbox = GetComponentInChildren<AttackHitbox2D>(true);
        if (audioSource == null) resolvedAudio = GetComponent<AudioSource>();
        else resolvedAudio = audioSource;

        if (characterRenderer == null)
        {
            Transform visual = transform.Find("VisualRoot");
            if (visual != null) characterRenderer = visual.GetComponent<SpriteRenderer>();
        }

        CacheImpulseMethod();
    }

    private void OnEnable()
    {
        Bind(movement);
    }

    private void OnDisable()
    {
        Unbind();
    }

    /// <summary>Subscribes to the motor, the slash volume and the level's event sources exactly once.</summary>
    public void Bind(PlayerMovement target)
    {
        if (bound) return;

        if (movement == null) movement = target;

        if (movement != null)
        {
            movement.Jumped += OnJumped;
            movement.Landed += OnLanded;
        }

        if (hitbox != null) hitbox.Hit += OnSlashHit;

        Collectible.AnyCollected += OnCollectibleCollected;

        if (pressurePlate != null) pressurePlate.StateChanged += OnPlateStateChanged;
        if (strikeSwitch != null) strikeSwitch.Activated += OnSwitchActivated;
        if (bramble != null) bramble.Cleared += OnBrambleCleared;
        if (wingGem != null) wingGem.Collected += OnGemCollected;
        if (levelExit != null) levelExit.Completed += OnLevelCompleted;

        bound = true;
    }

    /// <summary>Detaches every subscription. Safe to call repeatedly.</summary>
    public void Unbind()
    {
        if (!bound) return;

        if (movement != null)
        {
            movement.Jumped -= OnJumped;
            movement.Landed -= OnLanded;
        }

        if (hitbox != null) hitbox.Hit -= OnSlashHit;

        Collectible.AnyCollected -= OnCollectibleCollected;

        if (pressurePlate != null) pressurePlate.StateChanged -= OnPlateStateChanged;
        if (strikeSwitch != null) strikeSwitch.Activated -= OnSwitchActivated;
        if (bramble != null) bramble.Cleared -= OnBrambleCleared;
        if (wingGem != null) wingGem.Collected -= OnGemCollected;
        if (levelExit != null) levelExit.Completed -= OnLevelCompleted;

        bound = false;
    }

    private void OnJumped()
    {
        Play(jumpClip, 0.55f);
    }

    private void OnLanded(float impactSpeed)
    {
        if (impactSpeed < minLandingSpeed) return;

        float strength = Mathf.InverseLerp(minLandingSpeed, hardLandingSpeed, impactSpeed);
        Vector3 origin = transform.position + Vector3.down * 0.85f;

        DustBurst(origin, strength);
        Play(landClip, Mathf.Lerp(0.55f, 1f, strength));
        Impulse(strength);
    }

    private void OnSlashHit(Vector3 point)
    {
        PlaySlashFeedback(point);
    }

    /// <summary>
    /// The hit spark, slash clip and camera nudge for one landed swing. Public because SYS3's
    /// <c>HeroCombat</c> supersedes <c>NexusAttack</c> as the attack driver, so the hitbox event
    /// that used to raise this never fires; HeroCombat calls it on every landed hit instead.
    /// Safe to call with no audio source assigned.
    /// </summary>
    public void PlaySlashFeedback(Vector3 point)
    {
        SparkBurst(point, slashColor);
        Play(slashClip, 0.8f);

        if (maxImpulseStrength > 0f) Impulse(0.35f);
    }

    private void OnCollectibleCollected(Collectible collectible)
    {
        if (collectible == null) return;

        // The pickup destroys itself after raising the event, so read its position now.
        SparkBurst(collectible.transform.position, pickupColor, 6);
        Play(pickupClip, 0.5f);
    }

    private void OnGemCollected(WingGemPickup gem)
    {
        // The gem owns its own power-up burst; this only adds the sound and a soft nudge.
        Play(gemClip, 0.9f);
        Impulse(0.3f);
    }

    private void OnPlateStateChanged(bool pressed)
    {
        Play(puzzleClip, pressed ? 0.6f : 0.4f);
    }

    private void OnSwitchActivated()
    {
        if (strikeSwitch != null) SparkBurst(strikeSwitch.transform.position, puzzleColor, 8);
        Play(puzzleClip, 0.7f);
    }

    private void OnBrambleCleared()
    {
        // The slash that clears it already fired the hit spark and its clip; the clear
        // gets the greener burst that marks the path opening.
        if (bramble != null) SparkBurst(bramble.transform.position, brambleColor, 8);
    }

    private void OnLevelCompleted(LevelExit exit)
    {
        Play(exitClip, 0.8f);
    }

    private void Play(AudioClip clip, float volume)
    {
        if (clip == null || resolvedAudio == null) return;

        resolvedAudio.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    private void Impulse(float strength)
    {
        if (impulseMethod == null || impulseSource == null) return;

        float value = Mathf.Clamp01(strength) * maxImpulseStrength;
        impulseMethod.Invoke(impulseSource, new object[] { value });
    }

    private void CacheImpulseMethod()
    {
        if (impulseSource == null)
        {
            impulseMethod = null;
            return;
        }

        impulseMethod = impulseSource.GetType().GetMethod(
            impulseMethodName,
            BindingFlags.Instance | BindingFlags.Public,
            null,
            new[] { typeof(float) },
            null);

        if (impulseMethod == null)
        {
            Debug.LogWarning(
                $"[NexusFeelFeedback] '{impulseSource.GetType().Name}' has no public {impulseMethodName}(float); camera impulse disabled.", this);
        }
    }

    private void DustBurst(Vector3 origin, float strength)
    {
        int count = Mathf.RoundToInt(Mathf.Lerp(5f, 14f, strength));
        float speed = Mathf.Lerp(0.9f, 2.6f, strength);

        for (int i = 0; i < count; i++)
        {
            float side = i % 2 == 0 ? 1f : -1f;
            Vector2 velocity = new Vector2(
                side * UnityEngine.Random.Range(0.4f, 1f) * speed,
                UnityEngine.Random.Range(0.25f, 0.9f) * speed);

            SpawnMote(
                "LandDust",
                origin + new Vector3(
                    side * UnityEngine.Random.Range(DustMinSideOffset, DustMaxSideOffset),
                    UnityEngine.Random.Range(-0.05f, 0.1f),
                    0f),
                velocity,
                UnityEngine.Random.Range(0.28f, 0.5f),
                UnityEngine.Random.Range(0.12f, 0.26f) * Mathf.Lerp(0.8f, 1.4f, strength),
                dustColor);
        }
    }

    private void SparkBurst(Vector3 origin, Color color, int count = 10)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float speed = UnityEngine.Random.Range(1.4f, 3.6f);

            SpawnMote(
                "SlashSpark",
                origin,
                new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed,
                UnityEngine.Random.Range(0.14f, 0.3f),
                UnityEngine.Random.Range(0.08f, 0.18f),
                color);
        }
    }

    private void SpawnMote(string name, Vector3 position, Vector2 velocity, float life, float size, Color color)
    {
        var go = new GameObject(name);
        go.transform.position = position;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = DotSprite();
        renderer.sortingOrder = MoteSortingOrder;
        renderer.color = color;

        go.AddComponent<Mote>().Configure(velocity, life, size);
    }

    private static Sprite DotSprite()
    {
        if (dotSprite != null) return dotSprite;

        const int size = 32;
        var pixels = new Color32[size * size];
        float half = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f - half) / half;
                float dy = (y + 0.5f - half) / half;
                float a = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * a * 255f));
            }
        }

        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
        };
        texture.SetPixels32(pixels);
        texture.Apply();

        dotSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 32f);
        dotSprite.name = "FeelDot";
        return dotSprite;
    }

    /// <summary>One short-lived dot: ballistic, drag-damped and faded out over its life.</summary>
    private class Mote : MonoBehaviour
    {
        private Vector2 velocity;
        private float life;
        private float baseSize;
        private float age;
        private SpriteRenderer renderer;

        public void Configure(Vector2 initialVelocity, float lifetime, float size)
        {
            velocity = initialVelocity;
            life = Mathf.Max(0.01f, lifetime);
            baseSize = size;
            renderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            age += Time.deltaTime;
            if (age >= life)
            {
                Destroy(gameObject);
                return;
            }

            float t = age / life;

            velocity.y -= 9f * Time.deltaTime;
            velocity *= 1f - 3.2f * Time.deltaTime;
            transform.position += (Vector3)(velocity * Time.deltaTime);

            if (renderer == null) return;

            Color c = renderer.color;
            c.a = 1f - t * t;
            renderer.color = c;

            transform.localScale = Vector3.one * Mathf.Lerp(baseSize, baseSize * 0.35f, t);
        }
    }
}
