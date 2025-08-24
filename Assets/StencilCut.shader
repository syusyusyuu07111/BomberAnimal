Shader "URP/Unlit/StencilCut"
{
    Properties{
        _BaseColor ("Color", Color) = (1,1,1,1)
        _StencilRef("Stencil Ref", Int) = 1
        _HoleCountF("Hole Count (float)", Float) = 0

        // �����ő�8�ixyz=���SWS, w=���a�j
        _Hole0 ("Hole0", Vector) = (0,0,0,0)
        _Hole1 ("Hole1", Vector) = (0,0,0,0)
        _Hole2 ("Hole2", Vector) = (0,0,0,0)
        _Hole3 ("Hole3", Vector) = (0,0,0,0)
        _Hole4 ("Hole4", Vector) = (0,0,0,0)
        _Hole5 ("Hole5", Vector) = (0,0,0,0)
        _Hole6 ("Hole6", Vector) = (0,0,0,0)
        _Hole7 ("Hole7", Vector) = (0,0,0,0)
    }
    SubShader{
        Tags{ "RenderType"="Opaque" "Queue"="Geometry" }

        Pass{
            Name "Forward"
            Tags{ "LightMode"="SRPDefaultUnlit" }
            Cull Back
            ZWrite On
            ZTest LEqual

            // �����̃X�e���V���؂蔲��
            Stencil { Ref [_StencilRef]  Comp NotEqual }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // --- SRP Batcher �Ή��i�F����CBUFFER�j ---
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
            CBUFFER_END

            // --- ���p�����[�^�i�O���[�o���B�z��/ints�g��Ȃ��j ---
            float  _HoleCountF;
            float4 _Hole0,_Hole1,_Hole2,_Hole3,_Hole4,_Hole5,_Hole6,_Hole7;

            struct Attributes { float3 positionOS : POSITION; };
            struct Varyings   { float4 positionHCS : SV_POSITION; float3 positionWS : TEXCOORD0; };

            Varyings vert(Attributes IN){
                Varyings OUT;
                float3 ws = TransformObjectToWorld(IN.positionOS);
                OUT.positionWS  = ws;
                OUT.positionHCS = TransformWorldToHClip(ws);
                return OUT;
            }

            // �e�����ʃ`�F�b�N�i�z��/���I�C���f�b�N�X/�^�L���X�g���g��Ȃ��j
            void ClipHole(float3 P, float4 H, float enableThreshold)
            {
                // enableThreshold=0��Hole0, 1��Hole1...  _HoleCountF ������ȏ�Ȃ�L��
                if (_HoleCountF > enableThreshold && H.w > 0.0)
                {
                    if (distance(P, H.xyz) < H.w) discard;
                }
            }

            half4 frag(Varyings IN) : SV_Target
            {
                ClipHole(IN.positionWS, _Hole0, 0.0);
                ClipHole(IN.positionWS, _Hole1, 1.0);
                ClipHole(IN.positionWS, _Hole2, 2.0);
                ClipHole(IN.positionWS, _Hole3, 3.0);
                ClipHole(IN.positionWS, _Hole4, 4.0);
                ClipHole(IN.positionWS, _Hole5, 5.0);
                ClipHole(IN.positionWS, _Hole6, 6.0);
                ClipHole(IN.positionWS, _Hole7, 7.0);

                return _BaseColor;
            }
            ENDHLSL
        }
    }
    Fallback Off
}
