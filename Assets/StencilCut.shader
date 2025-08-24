Shader "URP/Unlit/StencilCut"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="SRPDefaultUnlit" }

            Cull Back
            ZWrite On
            ZTest LEqual

            // �� �}�X�N���������ꏊ�iRef 1�j�͕`���Ȃ�
            Stencil { Ref 1  Comp NotEqual }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            float4 _BaseMap_ST, _BaseColor;

            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings   { float4 positionHCS:SV_POSITION; float2 uv:TEXCOORD0; };

            Varyings vert(Attributes v){
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                return o;
            }

            half4 frag(Varyings i):SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;
                return c;
            }
            ENDHLSL
        }
    }
    Fallback Off
}
