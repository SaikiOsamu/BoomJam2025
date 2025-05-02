Shader "UI/BackgroundEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Saturation ("Saturation", Range(0, 1)) = 0.25
        _OverlayColor ("Overlay Color", Color) = (0, 0, 0, 0.7)
        _BlurSize ("Blur Amount", Range(0, 1)) = 0.8
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        
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
            float _Saturation;
            float4 _OverlayColor;
            float _BlurSize;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                // Simple box blur
                float4 col = float4(0,0,0,0);
                float blurStep = _BlurSize * 0.01;
                
                for(float x = -blurStep; x <= blurStep; x += blurStep)
                {
                    for(float y = -blurStep; y <= blurStep; y += blurStep)
                    {
                        col += tex2D(_MainTex, i.uv + float2(x, y));
                    }
                }
                
                col /= 9.0; // Average the samples
                
                // Apply saturation
                float luminance = dot(col.rgb, float3(0.299, 0.587, 0.114));
                float3 desaturated = float3(luminance, luminance, luminance);
                col.rgb = lerp(desaturated, col.rgb, _Saturation);
                
                // Apply black overlay with opacity
                col.rgb = lerp(col.rgb, _OverlayColor.rgb, _OverlayColor.a);
                
                // Apply original alpha
                col.a *= i.color.a;
                
                return col;
            }
            ENDCG
        }
    }
}