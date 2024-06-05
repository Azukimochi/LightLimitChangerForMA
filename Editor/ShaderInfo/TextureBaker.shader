Shader "Hidden/LightLimitChanger/TextureBaker/Default"
{
    Properties
    {
        [NoScaleOffset][MainTexture] _MainTex ("Texture", 2D) = "white" {}
        [MainColor] _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            Texture2D _MainTex;
            SamplerState sampler_MainTex;
            float4 _Color;

            appdata vert (appdata v)
            {
                v.vertex = UnityObjectToClipPos(v.vertex);
                return v;
            }

            float4 frag(appdata i) : SV_Target
            {
                float4 col = _MainTex.Sample(sampler_MainTex, i.uv);
                col *= _Color;
                return col;
            }

            ENDHLSL
        }
    }
}