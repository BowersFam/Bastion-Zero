using System.IO;
using System.Text;
using UnityEngine;

/// <summary>Read-only alpha/colour probe for the HD-2D frames and the mite. Writes nothing.</summary>
public static class AlphaProbe
{
    public static string Execute()
    {
        string[] paths =
        {
            "Assets/Art/Characters/Nexus/HD2D/standing/00.png",
            "Assets/Art/Characters/Nexus/HD2D/walking/03.png",
            "Assets/Art/Characters/Nexus/HD2D/running/02.png",
            "Assets/Art/Characters/Nexus/HD2D/falling/01.png",
            "Assets/Art/Characters/Nexus/HD2D/attack_g1/02.png",
            "Assets/Art/Characters/Nexus/HD2D/wings/idle/00.png",
            "Assets/Generated_Images/cinder_mite.png",
        };

        var sb = new StringBuilder();

        foreach (string path in paths)
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), path)));
            Color32[] px = tex.GetPixels32();

            // Buckets: 0 | 1-31 | 32-127 | 128-223 | 224-247 | 248-253 | 254 | 255
            int[] b = new int[8];
            int max = 0;
            long cr = 0, cg = 0, cb = 0; int cn = 0;      // clear pixels' RGB
            long rr = 0, rg = 0, rb = 0; int rn = 0;      // faint rim 1-127
            long ir = 0, ig = 0, ib = 0; int inn = 0;     // near-solid >= 224

            foreach (Color32 p in px)
            {
                int a = p.a;
                if (a > max) max = a;
                int k = a == 0 ? 0 : a < 32 ? 1 : a < 128 ? 2 : a < 224 ? 3 : a < 248 ? 4 : a < 254 ? 5 : a == 254 ? 6 : 7;
                b[k]++;

                if (a == 0) { cr += p.r; cg += p.g; cb += p.b; cn++; }
                else if (a < 128) { rr += p.r; rg += p.g; rb += p.b; rn++; }
                else if (a >= 224) { ir += p.r; ig += p.g; ib += p.b; inn++; }
            }

            string Mean(long r, long g, long bl, int n) => n == 0 ? "-" : $"{r / n},{g / n},{bl / n}";

            sb.Append($"{Path.GetFileName(Path.GetDirectoryName(path))}/{Path.GetFileName(path)} {tex.width}x{tex.height} fmt {tex.format} max {max} | ");
            sb.Append($"0:{b[0]} 1-31:{b[1]} 32-127:{b[2]} 128-223:{b[3]} 224-247:{b[4]} 248-253:{b[5]} 254:{b[6]} 255:{b[7]} | ");
            sb.Append($"clearRGB {Mean(cr, cg, cb, cn)} rimRGB {Mean(rr, rg, rb, rn)} solidRGB {Mean(ir, ig, ib, inn)}\n");

            Object.DestroyImmediate(tex);
        }

        return sb.ToString();
    }
}
