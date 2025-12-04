Shader "Unlit/CuteDiagonalStripesBG_Multi"
{
    Properties
    {
        _BaseColor      ("Base Color", Color) = (0.96, 0.97, 0.85, 1)     // 全体の下地っぽい黄み
        _StripeColor1   ("Stripe Color 1", Color) = (0.90, 0.93, 0.70, 1) // 少しだけ濃い線
        _StripeColor2   ("Stripe Color 2", Color) = (0.96, 0.97, 0.85, 1) // 下地とほぼ同じで馴染ませる
        _StripeWidth    ("Stripe Width", Range(0.01, 0.3)) = 0.10         // 線の太さ
        _StripeSoft     ("Stripe Softness", Range(0.0001, 0.1)) = 0.03    // 境界のぼかし
        _StripeSpeed    ("Stripe Speed", Range(-0.6, 0.6)) = 0.02         // ゆっくり動かす
        _Angle          ("Angle (deg)", Range(-90, 90)) = 45              // 斜めの角度
        _StripeFreq     ("Stripe Frequency", Range(1, 40)) = 12           // 本数（多いほど細かくなる）
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Background"
            "RenderPipeline"="UniversalRenderPipeline"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            ZWrite Off
            Cull Off
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _StripeColor1;
                float4 _StripeColor2;
                float  _StripeWidth;
                float  _StripeSoft;
                float  _StripeSpeed;
                float  _Angle;
                float  _StripeFreq;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // ベース色
                float4 col = _BaseColor;

                // 角度を決めて、その方向に「座標」を作る
                float rad = radians(_Angle);
                float2 dir = float2(cos(rad), sin(rad));

                // 斜め方向の座標に、周波数を掛けて「何本も」出す
                float linePos = dot(uv, dir);
                linePos *= _StripeFreq;

                // 時間でゆっくり流す
                linePos += _Time.y * _StripeSpeed;

                // 0〜1で繰り返す
                linePos = frac(linePos);

                // 中心に近いところを線にする
                // 0〜1の真ん中(0.5)に線が来るようにする
                float stripe = abs(linePos * 2.0 - 1.0);

                // 線の太さ＋ぼかしでマスク
                float mask = smoothstep(_StripeWidth + _StripeSoft, _StripeWidth, stripe);

                // 2色でほんのり差をつける
                float4 stripeCol = lerp(_StripeColor2, _StripeColor1, mask);

                return stripeCol;
            }
            ENDHLSL
        }
    }
}
