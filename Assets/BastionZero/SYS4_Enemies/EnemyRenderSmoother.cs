using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BastionZero.Enemies
{
    /// <summary>
    /// Render interpolation for an enemy that moves by <c>transform.position</c> in FixedUpdate.
    ///
    /// <para>Patrollers carry no Rigidbody2D (SYS4 moves them by transform), so they step at 60 Hz
    /// while the hero (Rigidbody2D Interpolate) and the camera move on every rendered frame. Above
    /// 60 fps that reads as judder. This draws the body between its last two tick positions — the
    /// same blend Rigidbody2D interpolation uses.</para>
    ///
    /// <para>The body's SpriteRenderer and its BoxCollider2D share the root transform, and in this
    /// Unity build a collider follows its transform as soon as the transform moves. So the drawn
    /// offset exists <b>only while a frame renders</b>: it is applied in
    /// <see cref="RenderPipelineManager.beginContextRendering"/> and removed in
    /// <see cref="RenderPipelineManager.endContextRendering"/>. No gameplay, input or physics code
    /// runs inside that window, so <see cref="EnemyAI"/>'s probes, slash overlaps and every collider
    /// read only ever see the true tick position. LateUpdate only computes the offset.</para>
    ///
    /// <para>Execution order −1000 keeps a fallback restore at the start of FixedUpdate, Update and
    /// LateUpdate, in case a render context never ended (an exception mid-render).</para>
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    public sealed class EnemyRenderSmoother : MonoBehaviour
    {
        /// <summary>A tick-to-tick move longer than this (tiles) is a teleport: snap, never blend.</summary>
        private const float SnapDistanceTiles = 1f;

        private static readonly List<EnemyRenderSmoother> Active = new List<EnemyRenderSmoother>();
        private static bool subscribed;

        private Vector3 previousTick;
        private Vector3 currentTick;
        private Vector3 tickStart;
        private bool tickPending;

        private Vector3 pendingOffset;
        private bool drawnOffTick;
        private Vector3 heldTick;
        private Vector3 drawnPosition;

        /// <summary>The true simulated position. Outside rendering this is always <c>transform.position</c>.</summary>
        public Vector3 TickPosition => drawnOffTick ? heldTick : transform.position;

        /// <summary>The offset the next render will draw the body at, relative to <see cref="TickPosition"/>.</summary>
        public Vector3 RenderOffset => pendingOffset;

        /// <summary>Where the next render will draw the body.</summary>
        public Vector3 DrawnPosition => TickPosition + pendingOffset;

        /// <summary>Where the body was drawn by the most recent render (diagnostic).</summary>
        public Vector3 LastDrawnPosition { get; private set; }

        /// <summary><c>Time.frameCount</c> of the most recent render that drew the body (diagnostic).</summary>
        public int LastDrawnFrame { get; private set; } = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Active.Clear();
            Unsubscribe();
        }

        private static void Subscribe()
        {
            if (subscribed) return;
            RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
            RenderPipelineManager.endContextRendering += OnEndContextRendering;
            subscribed = true;
        }

        private static void Unsubscribe()
        {
            if (!subscribed) return;
            RenderPipelineManager.beginContextRendering -= OnBeginContextRendering;
            RenderPipelineManager.endContextRendering -= OnEndContextRendering;
            subscribed = false;
        }

        private static void OnBeginContextRendering(ScriptableRenderContext context, List<Camera> cameras)
        {
            for (int i = 0; i < Active.Count; i++) Active[i].ApplyDrawnOffset();
        }

        private static void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras)
        {
            for (int i = 0; i < Active.Count; i++) Active[i].Restore();
        }

        private void OnEnable()
        {
            drawnOffTick = false;
            pendingOffset = Vector3.zero;
            previousTick = currentTick = tickStart = transform.position;
            tickPending = false;

            if (!Active.Contains(this)) Active.Add(this);
            Subscribe();
        }

        private void OnDisable()
        {
            Restore();
            pendingOffset = Vector3.zero;

            Active.Remove(this);
            if (Active.Count == 0) Unsubscribe();
        }

        private void FixedUpdate()
        {
            Restore();

            // The transform holds the previous tick's result: the start of this tick.
            tickStart = transform.position;
            tickPending = true;
        }

        private void Update()
        {
            Restore();

            if (tickPending)
            {
                // At least one tick ran since the last frame; the blend spans the latest one.
                previousTick = tickStart;
                currentTick = transform.position;
                tickPending = false;
            }
            else if (transform.position != currentTick)
            {
                // Moved outside a tick (placement, reset, editor drag): draw it where it is.
                previousTick = currentTick = transform.position;
            }
        }

        private void LateUpdate()
        {
            Restore();
            pendingOffset = Vector3.zero;

            if ((currentTick - previousTick).sqrMagnitude > SnapDistanceTiles * SnapDistanceTiles) return;

            // Something moved the body after Update: draw it where it is until the next tick.
            if (transform.position != currentTick) return;

            float alpha = Mathf.Clamp01((float)((Time.timeAsDouble - Time.fixedTimeAsDouble) / Time.fixedDeltaTime));
            pendingOffset = Vector3.Lerp(previousTick, currentTick, alpha) - currentTick;
        }

        private void ApplyDrawnOffset()
        {
            if (drawnOffTick) return;
            LastDrawnFrame = Time.frameCount;

            if (pendingOffset.sqrMagnitude == 0f)
            {
                LastDrawnPosition = transform.position;
                return;
            }

            heldTick = transform.position;
            drawnPosition = heldTick + pendingOffset;
            transform.position = drawnPosition;
            drawnOffTick = true;
            LastDrawnPosition = drawnPosition;
        }

        private void Restore()
        {
            if (!drawnOffTick) return;
            drawnOffTick = false;

            // Exact restore, unless something else moved the body while it was drawn off its tick.
            Vector3 now = transform.position;
            transform.position = now == drawnPosition ? heldTick : now - (drawnPosition - heldTick);
        }
    }
}
