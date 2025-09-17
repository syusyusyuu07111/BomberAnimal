Shader "Custom/URP/StencilMask"
{
    SubShader
    {
        Pass
        {
            // 色　深度をoffにしてステンシルだけつけるようにする----------------------------------------------------------------------
            ColorMask 0
            ZWrite Off
            ZTest Always
            Cull Off
            //-------------------------------------------------------------------------------------------------------------------------
            //ステンシル１を付ける
            Stencil { Ref 1 Comp Always Pass Replace ZFail Replace Fail Replace }
        }
    }
}
