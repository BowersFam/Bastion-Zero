// Bastion Zero — pixel-art conversion filter for third-party 3D / PBR art.
//
// Purpose: make anything brought into the project (the PureNature_Jungle kit and
// any future import) read in the same visual language as Nexus: 32 pixels per
// world unit, a small shading ramp, a quantised palette and 4x4 ordered dithering.
//
// This shader is *bake-time*, not runtime. Rendered from a fixed view into an
// offscreen target sized at (32 * designScale) pixels per unit, it produces a
// sprite whose on-screen texel density is identical to Nexus's. See
// Assets/Editor/PixelNatureBaker.cs for the tool that drives it.
//
// Pipeline independent: plain vertex/fragment pass, no lighting uniforms.
Shader "Bastion/PixelNature"
{
    Properties
    {
        _MainTex        ("Albedo", 2D) = "white" {}
        _Color          ("Tint", Color) = (1,1,1,1)
        _AlphaCutoff    ("Alpha Cutoff", Range(0,1)) = 0.4
        _LightDir       ("Light Dir (xyz, object space)", Vector) = (-0.45, 0.82, -0.36, 0)
        _ShadeSteps     ("Shade Steps", Float) = 3
        _ShadowMul      ("Shadow Multiplier", Range(0,1.5)) = 0.52
        _LightMul       ("Light Multiplier", Range(0,2)) = 1.18
        _Saturation     ("Saturation", Range(0,2)) = 0.9
        _ColorLevels    ("Colour Levels", Float) = 14
        _Dither         ("Dither", Range(0,1)) = 0.6
        _RimMul         ("Self-shadow Rim", Range(0,1)) = 0.62
    }

    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" "IgnoreProjector"="True" }
        Cull Off
        ZWrite On
        ZTest LEqual

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _AlphaCutoff;
            float4 _LightDir;
            float _ShadeSteps;
            float _ShadowMul;
            float _LightMul;
            float _Saturation;
            float _ColorLevels;
            float _Dither;
            float _RimMul;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 nrm : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // Object space normal: props are baked at identity, so this is a
                // stable, view-independent shading direction per prop.
                o.nrm = v.normal;
                return o;
            }

            // 4x4 ordered Bayer: M4[y][x] = 4*M2[y&1][x&1] + M2[(y>>1)&1][(x>>1)&1]
            float bayer4(float2 pixel)
            {
                int x = (int)fmod(pixel.x, 4.0);
                int y = (int)fmod(pixel.y, 4.0);
                int m2a = (x & 1) == 0 ? ((y & 1) == 0 ? 0 : 2) : ((y & 1) == 0 ? 3 : 1);
                int m2b = ((x >> 1) & 1) == 0 ? (((y >> 1) & 1) == 0 ? 0 : 2) : (((y >> 1) & 1) == 0 ? 3 : 1);
                int v = m2a * 4 + m2b;
                return (v + 0.5) / 16.0;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 src = tex2D(_MainTex, i.uv);
                float alpha = src.a * _Color.a;
                clip(alpha - _AlphaCutoff);

                float3 c = src.rgb * _Color.rgb;

                // --- quantised lighting ramp -------------------------------------
                float3 n = normalize(i.nrm);
                float ndl = dot(n, normalize(_LightDir.xyz)) * 0.5 + 0.5;
                float steps = max(1.0, _ShadeSteps);
                float ramp = floor(ndl * steps + 0.5) / steps;
                float shade = lerp(_ShadowMul, _LightMul, saturate(ramp));
                c *= shade;

                // --- saturation --------------------------------------------------
                float lum = dot(c, float3(0.299, 0.587, 0.114));
                c = lerp(lum.xxx, c, _Saturation);

                // --- palette quantisation in gamma space -------------------------
                // The bake target is a linear RenderTexture and the result is saved
                // as an sRGB PNG, so quantise the display values the eye will see.
                c = saturate(c);
                c = LinearToGammaSpace(c);
                float lv = max(2.0, _ColorLevels);
                float d = bayer4(i.pos.xy) * _Dither / lv;
                c = floor(c * lv + d) / lv;

                return fixed4(c, 1.0);
            }
            ENDCG
        }
    }
    FallBack Off
}
