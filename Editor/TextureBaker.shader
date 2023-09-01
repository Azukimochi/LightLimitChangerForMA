Shader "Hidden/LightLimitChanger/TextureBaker"
{
    Properties
    {
        [NoScaleOffset][MainTexture] _MainTex ("Texture", 2D) = "black" {}
        [NoScaleOffset] _Mask ("Mask", 2D) = "white" {}
        [NoScaleOffset] _GradationMap("Gradation", 2D) = "white" {}
        [HDR][MainColor] _Color ("Color", Color) = (1, 1, 1, 1)
        _HSVG ("HSVG", Vector) = (0,1,1,1)
        _GradationStrength("Gradation Strength", Range(0, 1)) = 0
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _Mask;
            sampler2D _GradationMap;
            float4 _Color;
            float4 _HSVG;
            float _GradationStrength;

            appdata vert (appdata v)
            {
                v.vertex = UnityObjectToClipPos(v.vertex);
                return v;
            }

            // https://github.com/lilxyzw/lilToon/blob/2ef370dc444172787c075ec3a822438c2bee26cb/Assets/lilToon/Shader/Includes/lil_common_functions.hlsl#L328C1-L342C2
            // Originally under MIT License
            // Copyright (c) 2020-2023 lilxyzw
            inline float3 lilToneCorrection(float3 c, float4 hsvg)
            {
                // gamma
                c = pow(abs(c), hsvg.w);
                // rgb -> hsv
                float4 p = (c.b > c.g) ? float4(c.bg,-1.0,2.0/3.0) : float4(c.gb,0.0,-1.0/3.0);
                float4 q = (p.x > c.r) ? float4(p.xyw, c.r) : float4(c.r, p.yzx);
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                float3 hsv = float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
                // shift
                hsv = float3(hsv.x+hsvg.x,saturate(hsv.y*hsvg.y),saturate(hsv.z*hsvg.z));
                // hsv -> rgb
                return hsv.z - hsv.z * hsv.y + hsv.z * hsv.y * saturate(abs(frac(hsv.x + float3(1.0, 2.0/3.0, 1.0/3.0)) * 6.0 - 3.0) - 1.0);
            }

            inline float3 gradationMap(float3 col, sampler2D gradationMap, float strength)
            {
                if(strength == 0) 
                    return col;
                
                float R = tex2D(gradationMap, float2(col.r, 0.5)).r;
                float G = tex2D(gradationMap, float2(col.g, 0.5)).g;
                float B = tex2D(gradationMap, float2(col.b, 0.5)).b;
                float3 rgb = float3(R, G, B);
                
                return lerp(col, rgb, strength);
            }

            float4 frag (appdata i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                float3 base = col.rgb;
                float mask = tex2D(_Mask, i.uv).r;

                col.rgb = lilToneCorrection(col.rgb, _HSVG);
                col.rgb = gradationMap(col.rgb, _GradationMap, _GradationStrength);
                col.rgb = lerp(base, col.rgb, mask);

                col *= _Color;
                
                return col;
            }

            ENDCG
        }
    }
}