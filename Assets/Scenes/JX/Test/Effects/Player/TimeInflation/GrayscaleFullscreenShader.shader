Shader "Custom/FullscreenGrayscaleStencil"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay+100" }
        Pass
        {
            Name "FullscreenPass"
            ZTest Always Cull Off ZWrite Off

            // 👇 关键：只对非Stencil=1区域生效
            Stencil
            {
                Ref 1
                Comp NotEqual
                Pass Keep
                ReadMask 255
                WriteMask 192
            }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 texCoord : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float2 positionSS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.positionCS = float4(positionSS, 0, 1);
                output.texCoord = GetFullScreenTriangleTexCoord(input.vertexID);
                return output;
            }

            TEXTURE2D_X(_CameraColorTexture);
            SAMPLER(sampler_CameraColorTexture);

            float4 Frag(Varyings input) : SV_Target
            {
                float4 color = SAMPLE_TEXTURE2D_X(_CameraColorTexture, sampler_CameraColorTexture, input.texCoord);
                float3 grayColor = float3(gray, gray, gray);
                return float4(grayColor, color.a);
            }

            ENDHLSL
        }
    }
    FallBack Off
}
