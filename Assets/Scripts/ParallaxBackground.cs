using UnityEngine;

/// <summary>
/// Moves a single background layer relative to the camera to fake depth.
/// The layer keeps a fixed starting offset from the camera and only follows a
/// fraction of the camera's travel, so distant layers drift less than nearer ones.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Camera to follow. Falls back to Camera.main when left empty.")]
    [SerializeField] private Transform cameraTransform;

    [Header("Parallax")]
    [Tooltip("0 = pinned to the world, 1 = glued to the camera. Distant layers use smaller values.")]
    [Range(0f, 1f)]
    [SerializeField] private float parallaxFactor = 0.3f;

    [Tooltip("Keep the layer's height fixed so camera bobbing during jumps does not shake it.")]
    [SerializeField] private bool lockVertical = true;

    private Vector3 startPosition;
    private Vector3 cameraStartPosition;

    private void Start()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraTransform == null)
        {
            Debug.LogWarning($"[ParallaxBackground] '{name}' has no camera assigned; parallax disabled.", this);
            enabled = false;
            return;
        }

        startPosition = transform.position;
        cameraStartPosition = cameraTransform.position;
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 cameraDelta = cameraTransform.position - cameraStartPosition;

        float x = startPosition.x + cameraDelta.x * parallaxFactor;
        float y = lockVertical ? startPosition.y : startPosition.y + cameraDelta.y * parallaxFactor;

        transform.position = new Vector3(x, y, startPosition.z);
    }
}
