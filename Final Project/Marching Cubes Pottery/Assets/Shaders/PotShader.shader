// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/PotShader"
{
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 norm: NORMAL;
                fixed4 clr : COLOR;
            };

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            v2f vert(appdata v)
            {
                v2f output;
                output.pos = UnityObjectToClipPos(v.vertex);
                output.norm = v.normal;
                output.clr.xyz = v.normal * 0.5 + 0.5;
                output.clr.w = 1;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                return input.clr;
            }
            ENDCG
        }
    }
}
