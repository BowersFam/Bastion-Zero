using System.Text;
using BastionZero.Enemies;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Play-mode probe, read-only for the project (nothing saved). Proves whether a transform change
/// made inside the render window reaches the rendered image: it renders the main camera into a
/// RenderTexture three times — patroller body hidden, normal, and displaced +2 tiles from inside
/// beginCameraRendering (removed again in endCameraRendering) — and measures the pixel centroid of
/// the body in each. Also reports tick / transform / collider positions outside the render window.
/// </summary>
public static class RenderWindowProbe
{
    private static Transform displacedTarget;
    private static Vector3 displacement;

    private static void OnBeginCamera(ScriptableRenderContext context, Camera camera)
    {
        if (displacedTarget != null) displacedTarget.position += displacement;
    }

    private static void OnEndCamera(ScriptableRenderContext context, Camera camera)
    {
        if (displacedTarget != null) displacedTarget.position -= displacement;
    }

    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";

        Camera cam = Camera.main;
        if (cam == null) return "no main camera";

        // Pick the patroller nearest the camera centre that is drawn and in view.
        EnemyRenderSmoother pick = null;
        float best = float.MaxValue;
        foreach (EnemyRenderSmoother s in Object.FindObjectsByType<EnemyRenderSmoother>(FindObjectsInactive.Exclude))
        {
            var r = s.GetComponent<SpriteRenderer>();
            if (r == null || !r.enabled) continue;
            Vector3 vp = cam.WorldToViewportPoint(s.transform.position);
            if (vp.x < 0.1f || vp.x > 0.75f || vp.y < 0.05f || vp.y > 0.95f) continue;
            float d = Mathf.Abs(vp.x - 0.4f);
            if (d < best) { best = d; pick = s; }
        }
        if (pick == null) return "no drawn patroller in view";

        var body = pick.GetComponent<SpriteRenderer>();
        var box = pick.GetComponent<BoxCollider2D>();
        var sb = new StringBuilder();

        sb.AppendLine($"target {pick.name}; frame {Time.frameCount}; LastDrawnFrame {pick.LastDrawnFrame}");
        sb.AppendLine($"outside render window: transform.x {pick.transform.position.x:F4}, TickPosition.x {pick.TickPosition.x:F4}, " +
                      $"collider.bounds.center.x {(box != null ? box.bounds.center.x : float.NaN):F4}, RenderOffset.x {pick.RenderOffset.x:F4}, " +
                      $"LastDrawnPosition.x {pick.LastDrawnPosition.x:F4}");

        const int w = 640, h = 360;
        RenderTexture previousTarget = cam.targetTexture;
        var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Vector3 before = pick.transform.position;

        try
        {
            cam.targetTexture = rt;

            body.enabled = false;
            Color32[] hidden = RenderOnce(cam, rt, tex);
            body.enabled = true;
            Color32[] normal = RenderOnce(cam, rt, tex);

            displacedTarget = pick.transform;
            displacement = new Vector3(2f, 0f, 0f);
            RenderPipelineManager.beginCameraRendering += OnBeginCamera;
            RenderPipelineManager.endCameraRendering += OnEndCamera;
            Color32[] displaced;
            try
            {
                displaced = RenderOnce(cam, rt, tex);
            }
            finally
            {
                RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
                RenderPipelineManager.endCameraRendering -= OnEndCamera;
                displacedTarget = null;
            }

            (int nNormal, float cxNormal) = DiffCentroid(normal, hidden, w);
            (int nDisplaced, float cxDisplaced) = DiffCentroid(displaced, hidden, w);
            float pxPerUnit = h / (2f * cam.orthographicSize);

            sb.AppendLine($"render {w}x{h}, ortho {cam.orthographicSize:F2} -> {pxPerUnit:F2} px/unit; expected shift for +2 tiles {2f * pxPerUnit:F1} px");
            sb.AppendLine($"normal body pixels {nNormal}, centroid x {cxNormal:F1}");
            sb.AppendLine($"displaced-in-window body pixels {nDisplaced}, centroid x {cxDisplaced:F1}");
            sb.AppendLine($"measured shift {(cxDisplaced - cxNormal):F1} px");
        }
        finally
        {
            body.enabled = true;
            cam.targetTexture = previousTarget;
            RenderTexture.active = null;
            rt.Release();
            Object.Destroy(rt);
            Object.Destroy(tex);
        }

        sb.AppendLine($"transform restored exactly: {pick.transform.position == before} (x {pick.transform.position.x:F4})");
        return sb.ToString();
    }

    private static Color32[] RenderOnce(Camera cam, RenderTexture rt, Texture2D tex)
    {
        cam.Render();
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply(false);
        RenderTexture.active = previous;
        return tex.GetPixels32();
    }

    private static (int count, float centroidX) DiffCentroid(Color32[] a, Color32[] b, int width)
    {
        long sumX = 0;
        int n = 0;
        for (int i = 0; i < a.Length; i++)
        {
            int d = Mathf.Abs(a[i].r - b[i].r) + Mathf.Abs(a[i].g - b[i].g) + Mathf.Abs(a[i].b - b[i].b);
            if (d <= 12) continue;
            sumX += i % width;
            n++;
        }
        return (n, n > 0 ? (float)sumX / n : float.NaN);
    }
}
