using System;
using UnityEngine;

namespace io.github.azukimochi
{
    partial class ShaderInfo
    {
        public sealed class Sunao : ShaderInfo
        {
            public static Sunao Instance { get; } = new Sunao();

            public const string _MinimumLight = nameof(_MinimumLight);
            public const string _DirectionalLight = nameof(_DirectionalLight);
            public const string _PointLight = nameof(_PointLight);
            public const string _Unlit = nameof(_Unlit);
            public const string _SHLight = nameof(_SHLight);
            public const string _Color = nameof(_Color);
            public const string _MainTex = nameof(_MainTex);
            public const string _SubTex = nameof(_SubTex);
            public const string _SubColor = nameof(_SubColor);

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
            }

            private static class DefaultParameters
            {
                public static readonly Color Color = Color.white;
            }

            public override bool TryNormalizeMaterial(Material material, TextureBaker textureBaker)
            {
                bool result = false;
                bool bakeFlag = false;

                {
                    var tex = material.GetTexture(PropertyIDs.MainTex);
                    if (tex != null)
                        textureBaker.Texture = tex;

                    if (material.GetColor(PropertyIDs.Color) != DefaultParameters.Color)
                    {
                        textureBaker.Color = material.GetColor(PropertyIDs.Color);
                        material.SetColor(PropertyIDs.Color, DefaultParameters.Color);
                        bakeFlag = true;
                    }

                    if (bakeFlag)
                    {
                        material.SetTexture(PropertyIDs.MainTex, BakeTexture(textureBaker));
                    }

                    result |= bakeFlag;
                }

                return result;
            }

            public override bool IsTargetShader(Shader shader)
            {
                return shader.name.Contains(nameof(Sunao), StringComparison.OrdinalIgnoreCase);
            }

            public override void SetControlAnimation(in ControlAnimationContainer container, in ControlAnimationParameters parameters)
            {
                switch (container.ControlType)
                {
                    case LightLimitControlType.Light:

                        container.Default.SetParameterAnimation(parameters, _MinimumLight, parameters.MinLightValue);
                        container.Default.SetParameterAnimation(parameters, _DirectionalLight, parameters.MaxLightValue);
                        container.Default.SetParameterAnimation(parameters, _PointLight, parameters.MaxLightValue);
                        container.Default.SetParameterAnimation(parameters, _SHLight, parameters.MaxLightValue);

                        var curve = Utils.Animation.Linear(parameters.MinLightValue, parameters.MaxLightValue);
                        container.Control.SetParameterAnimation(parameters, _MinimumLight, curve);
                        container.Control.SetParameterAnimation(parameters, _DirectionalLight, curve);
                        container.Control.SetParameterAnimation(parameters, _PointLight, curve);
                        container.Control.SetParameterAnimation(parameters, _SHLight, curve);

                        break;

                    case LightLimitControlType.Unlit:

                        container.Default.SetParameterAnimation(parameters, _Unlit, 0);
                        container.Control.SetParameterAnimation(parameters, _Unlit, 0, 1);

                        break;

                    case LightLimitControlType.ColorTemperature:

                        container.Default.SetParameterAnimation(parameters, _Color, DefaultParameters.Color);
                        container.Control.SetColorTempertureAnimation(parameters, _Color, DefaultParameters.Color);

                        break;
                }
            }
        }
    }
}
