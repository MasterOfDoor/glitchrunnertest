Shader "Custom/CRTOverlay"
{
    Properties
    {
        _ScanlineSpacing  ("Scanline Spacing",      Float)         = 4.0
        _ScanlineDarkness ("Scanline Darkness",     Range(0,1))    = 0.18
        _ScanlineSharp    ("Scanline Sharpness",    Range(1,16))   = 6.0
        _VignetteRadius   ("Vignette Radius",       Range(0,2))    = 1.0
        _VignetteStrength ("Vignette Strength",     Range(0,1))    = 0.75
        _VignetteSoftness ("Vignette Softness",     Range(0.01,1)) = 0.40
        _FlickerStrength  ("Flicker Strength",      Range(0,0.04)) = 0.012
        _FlickerSpeed     ("Flicker Speed",         Float)         = 8.0
        _CurvatureX       ("Curvature X",           Range(0,0.12)) = 0.03
        _CurvatureY       ("Curvature Y",           Range(0,0.12)) = 0.02

        _MainTex          ("Main Tex",    2D)    = "white" {}
        _Color            ("Tint",        Color) = (0,0,0,1)

        [HideInInspector] _StencilComp      ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil          ("Stencil ID",         Float) = 0
        [HideInInspector] _StencilOp        ("Stencil Operation",  Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask  ("Stencil Read Mask",  Float) = 255
        [HideInInspector] _ColorMask        ("Color Mask",         Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"           = "Overlay"
            "IgnoreProjector" = "True"
            "RenderType"      = "Transparent"
            "PreviewType"     = "Plane"
        }

        Stencil
        {
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull      Off
        Lighting  Off
        ZWrite    Off
        ZTest     [unity_GUIZTestMode]
        Blend     SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            float4    _MainTex_ST;
            float4    _Color;
            float4    _TextureSampleAdd;
            float4    _ClipRect;

            float _ScanlineSpacing;
            float _ScanlineDarkness;
            float _ScanlineSharp;
            float _VignetteRadius;
            float _VignetteStrength;
            float _VignetteSoftness;
            float _FlickerStrength;
            float _FlickerSpeed;
            float _CurvatureX;
            float _CurvatureY;

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPos = v.vertex;
                OUT.vertex   = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color    = v.color * _Color;
                return OUT;
            }

            float2 applyCurvature(float2 uv)
            {
                float2 cc = uv - 0.5;
                cc.x *= 1.0 + cc.y * cc.y * _CurvatureX * 4.0;
                cc.y *= 1.0 + cc.x * cc.x * _CurvatureY * 4.0;
                return cc + 0.5;
            }

            float scanlineMult(float2 uv)
            {
                float scanPos = frac(uv.y * _ScreenParams.y / _ScanlineSpacing);
                float band    = pow(abs(sin(scanPos * 3.14159265)), _ScanlineSharp);
                return 1.0 - band * _ScanlineDarkness;
            }

            float vignetteDark(float2 uv)
            {
                float2 d = (uv - 0.5) * 2.0;
                float  r = length(d) / _VignetteRadius;
                return smoothstep(1.0 - _VignetteSoftness, 1.0, r) * _VignetteStrength;
            }

            float flickerMult()
            {
                float t = _Time.y;
                float n = sin(t * _FlickerSpeed)
                        + sin(t * _FlickerSpeed * 2.73)
                        + sin(t * _FlickerSpeed * 0.41);
                return 1.0 + (n / 3.0) * _FlickerStrength;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Correct Unity UI clipping call
                if (!UnityGet2DClipping(IN.worldPos.xy, _ClipRect))
                    discard;

                float2 uv = applyCurvature(IN.texcoord);

                // Black border outside curved edge
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                    return fixed4(0, 0, 0, IN.color.a);

                float scan    = scanlineMult(uv);
                float vig     = vignetteDark(uv);
                float flick   = flickerMult();

                // Combine into a black overlay with varying alpha
                float darkness = saturate((1.0 - scan) + vig) * flick;
                return fixed4(0.0, 0.0, 0.0, darkness * IN.color.a);
            }
            ENDCG
        }
    }

    // Transparent fallback — never pink
    FallBack Off
}
