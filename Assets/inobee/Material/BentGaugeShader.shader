Shader "Custom/BentGaugeShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        
        // グロー設定
        _GlowColor ("Glow Color", Color) = (0,1,1,1)
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 1
        _GlowWidth ("Glow Width", Range(0, 1)) = 0.3
        
        // アニメーション
        _ScanLineSpeed ("Scan Line Speed", Range(0, 10)) = 2
        _ScanLineWidth ("Scan Line Width", Range(0, 0.5)) = 0.1
        _ScanLineIntensity ("Scan Line Intensity", Range(0, 5)) = 1
        
        // グラデーション
        _GradientStart ("Gradient Start", Color) = (0,1,1,1)
        _GradientEnd ("Gradient End", Color) = (0,0.5,1,1)
        _UseGradient ("Use Gradient", Float) = 1
        
        // Stencil
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _GlowColor;
            float _GlowIntensity;
            float _GlowWidth;
            float _ScanLineSpeed;
            float _ScanLineWidth;
            float _ScanLineIntensity;
            float4 _GradientStart;
            float4 _GradientEnd;
            float _UseGradient;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // ベーステクスチャ
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // グラデーション
                float4 baseColor = _Color;
                if (_UseGradient > 0.5)
                {
                    baseColor = lerp(_GradientStart, _GradientEnd, i.uv.x);
                }
                
                col *= baseColor * i.color;
                
                // 上下端にグロー（エッジグロー）
                float edgeDist = abs(i.uv.y - 0.5) * 2.0; // 0-1
                float edgeGlow = 1.0 - smoothstep(1.0 - _GlowWidth, 1.0, edgeDist);
                edgeGlow = pow(edgeGlow, 2.0);
                
                col.rgb += _GlowColor.rgb * edgeGlow * _GlowIntensity;
                
                // スキャンライン（移動する光）
                float scanPos = frac(i.uv.x - _Time.y * _ScanLineSpeed);
                float scanLine = 1.0 - abs(scanPos - 0.5) * 2.0; // 0-1
                scanLine = smoothstep(0.0, _ScanLineWidth, scanLine);
                
                col.rgb += _GlowColor.rgb * scanLine * _ScanLineIntensity;
                
                // ハイライト（上部を明るく）
                float highlight = smoothstep(0.3, 0.7, i.uv.y);
                col.rgb += highlight * 0.2;
                
                return col;
            }
            ENDCG
        }
    }
}
