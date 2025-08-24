Shader "Universal Render Pipeline/Custom/FloorEarly"
{
    Properties{
        _BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        // �����}�X�N(1999)��葁���`��
        Tags { "RenderType"="Opaque" "Queue"="Geometry-2" } // = 1998

        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS: POSITION; float2 uv : TEXCOORD0; };
            struct Varyings   { float4 positionHCS: SV_POSITION; float2 uv : TEXCOORD0; };

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            float4 _BaseColor;

            Varyings vert (Attributes IN){
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }
            half4 frag (Varyings IN) : SV_Target {
                half4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                return col;
            }
            ENDHLSL
        }
    }
    Fallback Off
}
