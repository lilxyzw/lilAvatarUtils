Shader "Hidden/lilAvatarUtils/FallbackHidden"
{
    SubShader
    {
        Pass
        {
            Tags { "LightMode"="Dummy" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            float4 vert() : SV_POSITION{return float4(0,0,0,0);}
            float4 frag() : SV_Target{return float4(0,0,0,0);}
            ENDCG
        }
    }
}