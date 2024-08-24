using System;
using UnityEngine;

namespace io.github.azukimochi
{
    partial class ShaderInfo
    {
        public sealed class Sunao : ShaderInfo
        {
            public static Sunao Instance { get; } = new Sunao();

            public const string _MinimumLight = "_MinimumLight";
            public const string _DirectionalLight = "_DirectionalLight";
            public const string _PointLight = "_PointLight";
            public const string _Unlit = "_Unlit";
            public const string _SHLight = "_SHLight";
            public const string _Color = "_Color";
            public const string _MainTex = "_MainTex";
            public const string _SubTex = "_SubTex";
            public const string _SubColor = "_SubColor";
            public const string _MonochromeLit = "_MonochromeLit";

            private static class PropertyIDs
            {
                public static readonly int MinimumLight = Shader.PropertyToID(_MinimumLight);
                public static readonly int DirectionalLight = Shader.PropertyToID(_DirectionalLight);
                public static readonly int PointLight = Shader.PropertyToID(_PointLight);
                public static readonly int Unlit = Shader.PropertyToID(_Unlit);
                public static readonly int SHLight = Shader.PropertyToID(_SHLight);
                public static readonly int Color = Shader.PropertyToID(_Color);
                public static readonly int MainTex = Shader.PropertyToID(_MainTex);
                public static readonly int SubTex = Shader.PropertyToID(_SubTex);
                public static readonly int SubColor = Shader.PropertyToID(_SubColor);
                public static readonly int MonochromeLit = Shader.PropertyToID(_MonochromeLit);
            }

            private static class DefaultParameters
            {
                public static readonly float MinimumLight = 0;
                public static readonly float DirectionalLight = 1;
                public static readonly Color Color = Color.white;
                public static readonly float MonochromeLit = 0;
            }

            public override bool TryNormalizeMaterial(Material material, LightLimitChangerObjectCache cache)
            {
                bool result = false;
                bool bakeFlag = false;
                var textureBaker = TextureBaker.GetInstance<DefaultTextureBaker>(cache);

                {
                    var tex = material.GetTexture(PropertyIDs.MainTex);
                    if (tex is RenderTexture)
                        return false;

                    if (tex != null)
                        textureBaker.Texture = tex;

                    if (!material.GetColor(PropertyIDs.Color).Equals(DefaultParameters.Color, ShaderInfoUtility.IncludeField.RGB))
                    {
                        textureBaker.Color = material.GetColor(PropertyIDs.Color);
                        material.SetColor(PropertyIDs.Color, DefaultParameters.Color.With(a: textureBaker.Color.a));
                        bakeFlag = true;
                    }

                    if (bakeFlag)
                    {
                        var baked = cache.Register(textureBaker.Bake());
                        material.SetTexture(PropertyIDs.MainTex, baked);
                    }

                    result |= bakeFlag;
                }

                return result;
            }

            public override bool IsTargetShader(Shader shader)
            {
                return shader.name.Contains(nameof(Sunao), StringComparison.OrdinalIgnoreCase);
            }

            public override void SetControlAnimation(in ControlAnimationContainer container, in ControlAnimationParameters parameters, in LightLimitChangerParameters llc_parameters)
            {
                if (container.ControlType.HasFlag(LightLimitControlType.LightMin))
                {
                    container.Default.SetParameterAnimation(parameters, _MinimumLight, parameters.DefaultMinLightValue);

                    container.Control.SetParameterAnimation(parameters, _MinimumLight, parameters.MinLightValue, parameters.MaxLightValue);
                }

                if (container.ControlType.HasFlag(LightLimitControlType.LightMax))
                {
                    container.Default.SetParameterAnimation(parameters, _DirectionalLight, parameters.DefaultMaxLightValue);
                    container.Default.SetParameterAnimation(parameters, _PointLight, parameters.DefaultMaxLightValue);
                    container.Default.SetParameterAnimation(parameters, _SHLight, parameters.DefaultMaxLightValue);

                    var curve = Utils.Animation.Linear(parameters.MinLightValue, parameters.MaxLightValue);
                    container.Control.SetParameterAnimation(parameters, _DirectionalLight, curve);
                    container.Control.SetParameterAnimation(parameters, _PointLight, curve);
                    container.Control.SetParameterAnimation(parameters, _SHLight, curve);
                }

                if (container.ControlType.HasFlag(LightLimitControlType.Monochrome))
                {
                    container.Default.SetParameterAnimation(parameters, _MonochromeLit, parameters.DefaultMonochromeLightingValue);
                    container.Control.SetParameterAnimation(parameters, _MonochromeLit, 0, 1);
                }

                if (container.ControlType.HasFlag(LightLimitControlType.Unlit))
                {
                    container.Default.SetParameterAnimation(parameters, _Unlit, 0);
                    container.Control.SetParameterAnimation(parameters, _Unlit, 0, 1);
                }

                if (container.ControlType.HasFlag(LightLimitControlType.ColorTemperature))
                {
                    container.Default.SetParameterAnimation(parameters, _Color, DefaultParameters.Color);
                    container.Control.SetColorTempertureAnimation(parameters, _Color, DefaultParameters.Color);
                }
            }

            public override bool TryGetLightMinMaxValue(Material material, out float min, out float max)
            {
                min = material.GetOrDefault(PropertyIDs.MinimumLight, DefaultParameters.MinimumLight);
                max = material.GetOrDefault(PropertyIDs.DirectionalLight, DefaultParameters.DirectionalLight);
                return true;
            }

            public override bool TryGetMonochromeValue(Material material, out float monochrome, out float monochromeAdditive)
            {
                monochrome = material.GetOrDefault(PropertyIDs.MonochromeLit, DefaultParameters.MonochromeLit);
                monochromeAdditive = 1;
                return true;
            }
        }
    }
}
