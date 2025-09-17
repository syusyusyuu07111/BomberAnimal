Shader "Custom/URP/MaskedObject"
{
    SubShader
    {
        Pass
        {
            //ステンシルが１の場所だけ書かない　１じゃないとこ書く
            Stencil { Ref 1 Comp NotEqual Pass Keep Fail Keep ZFail Keep }
        }
    }
}
