Shader "URP/Destructible/ClipHolesUnlit"
{
    Properties{
        _BaseColor ("Color", Color) = (1,1,1,1)
        _HoleCountF ("Hole Count (float)", Float) = 0 // ※ cross-platform のため float で持つ
    }
    SubShader{
        Tags{ "RenderType"="Opaque" "Queue"="Geometry" }
        Pass{
            Name "Forward"
            Tags{ "LightMode"="SRPDefaultUnlit" }
            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // SRP Batcher 対応の定数バッファ
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float  _HoleCountF; // float で受けて後で int に変換
            CBUFFER_END

            #define MAX_HOLES 32
            float4 _HolePos[MAX_HOLES];   // xyz = 中心(WS), w = 半径

            struct Attributes { float3 positionOS : POSITION; };
            struct Varyings   { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS);
                OUT.positionCS = pos.positionCS;
                OUT.positionWS = pos.positionWS;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // ★ 重なりだけ非表示。_HoleCountF は float なので clamp → int 的に使う
                int count = (int) clamp(_HoleCountF + 0.5, 0.0, (float)MAX_HOLES);

                [unroll] for (int i = 0; i < MAX_HOLES; i++)
                {
                    if (i >= count) break;
                    float3 c = _HolePos[i].xyz;
                    float  r = _HolePos[i].w;
                    if (distance(IN.positionWS, c) < r) discard;
                }
                return _BaseColor;
            }
            ENDHLSL
        }
    }
    Fallback Off
}
