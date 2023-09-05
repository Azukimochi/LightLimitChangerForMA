using System;
using System.Xml.Linq;
using UnityEngine;

namespace io.github.azukimochi
{
    partial class ShaderInfo
    {
        public sealed class Poiyomi : ShaderInfo
        {
            public static Poiyomi Instance { get; } = new Poiyomi();

            public const string _LightingMinLightBrightness = nameof(_LightingMinLightBrightness);
            public const string _LightingCap = nameof(_LightingCap);
            public const string _MainColorAdjustToggle = nameof(_MainColorAdjustToggle);
            public const string _Saturation = nameof(_Saturation);
            public const string _Color = nameof(_Color);
            public const string _MainTex = nameof(_MainTex);

            private const string Animated_Suffix = "Animated";
            private const string Flag_IsAnimated = "1";

            public override string[] ShaderParameters { get; } = { _LightingMinLightBrightness, _LightingCap, _MainColorAdjustToggle, _Saturation, _Color, _MainTex };

            public override Shaders ShaderType => Shaders.Poiyomi;

            protected override string DefaultShaderName => ".poiyomi/Poiyomi 8.1/Poiyomi Toon";

            public override bool TryNormalizeMaterial(Material material, TextureBaker textureBaker)
            {
                bool bakeFlag = false;
                textureBaker.IsPoiyomiMode = true;
                {
                    var tex = material.GetTexture(PropertyIDs[_MainTex]);
                    if (tex != null)
                        textureBaker.Texture = tex;

                    {
                        var id = PropertyIDs[_Color];
                        if (DefaultParameters[id] != material.GetColor(id))
                        {
                            textureBaker.Color = material.GetColor(id);
                            material.SetColor(id, DefaultParameters[id].Color);
                            bakeFlag = true;
                        }
                    }

                    {
                        var id = PropertyIDs[_Saturation];
                        if (DefaultParameters[id] != material.GetFloat(id))
                        {
                            var sat = material.GetFloat(id);
                            textureBaker.HSVG = new Vector4(0, sat, 1, 1);
                            material.SetFloat(id, DefaultParameters[id].Float);
                            bakeFlag = true;
                        }
                    }

                    if (bakeFlag)
                    {
                        material.SetTexture(PropertyIDs[_MainTex], BakeTexture(textureBaker));
                    }
                }

                return bakeFlag;
            }

            public override bool IsTargetShader(Shader shader)
            {
                return
                    shader.name.IndexOf(nameof(Poiyomi), StringComparison.OrdinalIgnoreCase) != -1 ||
                    ContainsParameter(shader);
            }

            public static void EnableColorAdjust(Material material)
            {
                material.SetFloat(_MainColorAdjustToggle, 1);
                material.EnableKeyword($"{_MainColorAdjustToggle.ToUpperInvariant()}_ON");
                material.EnableKeyword("COLOR_GRADING_HDR");
            }

            public static void SetAnimatedFlags(Material material, bool allowColorTempControl, bool allowSaturationControl)
            {
                material.SetOverrideTag($"{_LightingCap}{Animated_Suffix}", Flag_IsAnimated);
                material.SetOverrideTag($"{_LightingMinLightBrightness}{Animated_Suffix}", Flag_IsAnimated);

                if (allowColorTempControl )
                {
                    material.SetOverrideTag($"{_Color}{Animated_Suffix}", Flag_IsAnimated);
                }
                if (allowSaturationControl)
                {
                    material.SetOverrideTag($"{_Saturation}{Animated_Suffix}", Flag_IsAnimated);
                }
            }
        }
    }
}
