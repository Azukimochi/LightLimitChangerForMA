Shader "Hidden/LightLimitChanger/TextureBaker"
{
    Properties
    {
        [NoScaleOffset][MainTexture] _MainTex ("Texture", 2D) = "white" {}
        [NoScaleOffset] _Mask ("Mask", 2D) = "white" {}
        [NoScaleOffset] _GradationMap("Gradation", 2D) = "white" {}
        [MainColor] _Color ("Color", Color) = (1, 1, 1, 1)
        _HSVG ("HSVG", Vector) = (0,1,1,1)
        _GradationStrength("Gradation Strength", Range(0, 1)) = 0
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _ _POIYOMI

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            Texture2D _MainTex;
            Texture2D _Mask;
            Texture2D _GradationMap;
            SamplerState sampler_MainTex;
            SamplerState sampler_linear_clamp;
            float4 _Color;
            float4 _HSVG;
            float _GradationStrength;

            appdata vert (appdata v)
            {
                v.vertex = UnityObjectToClipPos(v.vertex);
                return v;
            }

            // Originally under MIT License
            // Copyright (c) 2020-2023 lilxyzw
            // https://github.com/lilxyzw/lilToon/blob/2ef370dc444172787c075ec3a822438c2bee26cb/Assets/lilToon/Shader/Includes/lil_common_functions.hlsl#L328C1-L342C2
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

            // https://github.com/lilxyzw/lilToon/blob/2ef370dc444172787c075ec3a822438c2bee26cb/Assets/lilToon/Shader/Includes/lil_common_functions_thirdparty.hlsl#L56C1-L59C2
            float3 lilLinearToSRGB(float3 col)
            {
                return saturate(1.055 * pow(abs(col), 0.416666667) - 0.055);
            }

            // https://github.com/lilxyzw/lilToon/blob/2ef370dc444172787c075ec3a822438c2bee26cb/Assets/lilToon/Shader/Includes/lil_common_functions_thirdparty.hlsl#L61C1-L64C2
            float3 lilSRGBToLinear(float3 col)
            {
                return col * (col * (col * 0.305306011 + 0.682171111) + 0.012522878);
            }

            // https://github.com/lilxyzw/lilToon/blob/2ef370dc444172787c075ec3a822438c2bee26cb/Assets/lilToon/Shader/Includes/lil_common_functions.hlsl#L344C1-L359C2
            float3 lilGradationMap(float3 col, Texture2D gradationMap, float strength)
            {
                if (strength == 0.0) 
                    return col;

                col = lilLinearToSRGB(col);
                float R = gradationMap.Sample(sampler_linear_clamp, float2(col.r,0.5)).r;
                float G = gradationMap.Sample(sampler_linear_clamp, float2(col.g,0.5)).g;
                float B = gradationMap.Sample(sampler_linear_clamp, float2(col.b,0.5)).b;

                float3 outrgb = float3(R,G,B);
                
                col = lilSRGBToLinear(col);
                outrgb = lilSRGBToLinear(outrgb);
                    
                return lerp(col, outrgb, strength);
            }

            float4 frag(appdata i) : SV_Target
            {
                float4 col = _MainTex.Sample(sampler_MainTex, i.uv);
                float3 base = col.rgb;
                float mask = _Mask.Sample(sampler_MainTex, i.uv).r;

                #ifndef _POIYOMI
                
                col.rgb = lilToneCorrection(col.rgb, _HSVG);
                col.rgb = lilGradationMap(col.rgb, _GradationMap, _GradationStrength);
                col.rgb = lerp(base, col.rgb, mask);
                col *= _Color;

                #else

                col *= _Color;
                col.rgb = lerp(col.rgb, dot(col.rgb, float3(0.3, 0.59, 0.11)), -(_HSVG.y));

                #endif

                return col;
            }

            ENDHLSL
        }
    }
}