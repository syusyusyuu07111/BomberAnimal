Shader "URP/Utils/StencilMask"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-20" } // 先に描く

        Pass
        {
            Name "StencilMask"
            Tags { "LightMode"="SRPDefaultUnlit" }

            Cull Back
            ZWrite Off           // 深度は書かない（色も書かない）
            ZTest Always
            ColorMask 0

            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace     // ステンシルに 1 を書く
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings   { float4 positionHCS : SV_POSITION; };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                return o;
            }
            half4 frag() : SV_Target { return 0; }
            ENDHLSL
        }
    }
    Fallback Off
}
