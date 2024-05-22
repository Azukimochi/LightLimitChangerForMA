using System;
using System.Linq;
using nadena.dev.ndmf;
using UnityEngine;

namespace io.github.azukimochi
{
    partial class ShaderInfo
    {
        public sealed class LilToon : ShaderInfo
        {
            public static LilToon Instance { get; } = new LilToon();

            public const string _LightMinLimit = "_LightMinLimit";
            public const string _LightMaxLimit = "_LightMaxLimit";
            public const string _AsUnlit = "_AsUnlit";
            public const string _MainTexHSVG = "_MainTexHSVG";
            public const string _Color = "_Color";
            public const string _Color2nd = "_Color2nd";
            public const string _Color3rd = "_Color3rd";
            public const string _MainTex = "_MainTex";
            public const string _Main2ndTex = "_Main2ndTex";
            public const string _Main3rdTex = "_Main3rdTex";
            public const string _MainGradationTex = "_MainGradationTex";
            public const string _MainGradationStrength = "_MainGradationStrength";
            public const string _MainColorAdjustMask = "_MainColorAdjustMask";
            public const string _MonochromeLighting = "_MonochromeLighting";

            private static class PropertyIDs
            {
                public static readonly int LightMinLimit = Shader.PropertyToID(_LightMinLimit);
                public static readonly int LightMaxLimit = Shader.PropertyToID(_LightMaxLimit);
                public static readonly int AsUnlit = Shader.PropertyToID(_AsUnlit);
                public static readonly int MainTexHSVG = Shader.PropertyToID(_MainTexHSVG);
                public static readonly int Color = Shader.PropertyToID(_Color);
                public static readonly int Color2nd = Shader.PropertyToID(_Color2nd);
                public static readonly int Color3rd = Shader.PropertyToID(_Color3rd);
                public static readonly int MainTex = Shader.PropertyToID(_MainTex);
                public static readonly int Main2ndTex = Shader.PropertyToID(_Main2ndTex);
                public static readonly int Main3rdTex = Shader.PropertyToID(_Main3rdTex);
                public static readonly int MainGradationTex = Shader.PropertyToID(_MainGradationTex);
                public static readonly int MainGradationStrength = Shader.PropertyToID(_MainGradationStrength);
                public static readonly int MainColorAdjustMask = Shader.PropertyToID(_MainColorAdjustMask);
            }

            private static class DefaultParameters
            {
                public static readonly float LightMinLimit = 0.05f;
                public static readonly float LightMaxLimit = 1f;
                public static readonly Color Color = Color.white;
                public static readonly Color Color2nd = Color.white;
                public static readonly Color Color3rd = Color.white;
                public static readonly Vector4 MainTexHSVG = new Vector4(0, 1, 1, 1);
                public static readonly float MainGradationStrength = 0;
            }

            public override bool TryNormalizeMaterial(Material material, LightLimitChangerObjectCache cache)
            {
                var textureBaker = TextureBaker.GetInstance<DefaultTextureBaker>(cache);

                // MainTexture
                bool bakeProcessed = false;

                bakeProcessed |= BakeMainTex(material, cache, textureBaker);
                bakeProcessed |= Bake2ndOr3rdTex(material, cache, textureBaker, (PropertyIDs.Main2ndTex, PropertyIDs.Color2nd), DefaultParameters.Color2nd);
                bakeProcessed |= Bake2ndOr3rdTex(material, cache, textureBaker, (PropertyIDs.Main3rdTex, PropertyIDs.Color3rd), DefaultParameters.Color3rd);

                return bakeProcessed;
            }

            private bool BakeMainTex(Material material, LightLimitChangerObjectCache cache, DefaultTextureBaker textureBaker)
            {
                bool bakeFlag = false;
                bool isColorAdjusted = false;

                var tex = material.GetTexture(PropertyIDs.MainTex);
                if (tex is RenderTexture)
                    return false;

                if (tex != null)
                    textureBaker.Texture = tex;

                // MainColor
                if (material.GetColor(PropertyIDs.Color) != DefaultParameters.Color)
                {
                    textureBaker.Color = material.GetColor(PropertyIDs.Color);
                    material.SetColor(PropertyIDs.Color, DefaultParameters.Color);
                    bakeFlag = true;
                }

                // HSV / Gamma
                if (material.GetOrDefault(PropertyIDs.MainTexHSVG, DefaultParameters.MainTexHSVG) != DefaultParameters.MainTexHSVG)
                {
                    textureBaker.HSVG = material.GetVector(PropertyIDs.MainTexHSVG);
                    material.SetVector(PropertyIDs.MainTexHSVG, DefaultParameters.MainTexHSVG);
                    bakeFlag = true;
                    isColorAdjusted = true;
                }

                // Gradation
                if (material.GetOrDefault<Texture>(PropertyIDs.MainGradationTex) != null && material.GetFloat(PropertyIDs.MainGradationStrength) != DefaultParameters.MainGradationStrength)
                {
                    textureBaker.GradationMap = material.GetTexture(PropertyIDs.MainGradationTex);
                    textureBaker.GradationStrength = material.GetFloat(PropertyIDs.MainGradationStrength);
                    material.SetTexture(PropertyIDs.MainGradationTex, null);
                    material.SetFloat(PropertyIDs.MainGradationStrength, DefaultParameters.MainGradationStrength);
                    bakeFlag = true;
                    isColorAdjusted = true;
                }

                // Color Adujust Mask
                if (isColorAdjusted && material.GetOrDefault<Texture>(PropertyIDs.MainColorAdjustMask) != null)
                {
                    textureBaker.Mask = material.GetTexture(PropertyIDs.MainColorAdjustMask);
                    material.SetTexture(PropertyIDs.MainColorAdjustMask, null);
                    bakeFlag = true;
                }

                // Run Bake
                if (bakeFlag)
                {
                    var baked = cache.Register(textureBaker.Bake());
                    if (tex != null)
                        ObjectRegistry.RegisterReplacedObject(tex, baked);
                    material.SetTexture(PropertyIDs.MainTex, baked);
                }

                return bakeFlag;
            }

            private bool Bake2ndOr3rdTex(Material material, LightLimitChangerObjectCache cache, DefaultTextureBaker textureBaker, (int Texture, int Color) propertyIds, Color defaultColor)
            {
                if (!material.HasProperty(propertyIds.Texture))
                    return false;

                textureBaker.Reset();
                bool bakeFlag = false;

                var tex = material.GetTexture(propertyIds.Texture);
                if (tex is RenderTexture)
                    return false;

                if (tex != null)
                    textureBaker.Texture = tex;

                if (material.GetColor(propertyIds.Color) != defaultColor)
                {
                    textureBaker.Color = material.GetColor(propertyIds.Color);
                    material.SetColor(propertyIds.Color, defaultColor);
                    bakeFlag = true;
                }

                if (bakeFlag)
                {
                    var baked = cache.Register(textureBaker.Bake());
                    if (tex != null)
                        ObjectRegistry.RegisterReplacedObject(tex, baked);
                    material.SetTexture(propertyIds.Texture, baked);
                }

                return bakeFlag;
            }

            public override bool IsTargetShader(Shader shader)
            {
                if (shader.name.Contains("lilToon", StringComparison.OrdinalIgnoreCase))
                    return true;

                // カスタムシェーダーの名前にlilToonが入ってない時のことを考慮して、パラメーターが含まれるかどうかをチェックする
                if (_propertyIDsArrayCache == null)
                {
                    // 横着
                    _propertyIDsArrayCache = typeof(PropertyIDs).GetFields().Select(x => (int)x.GetValue(null)).ToArray();
                }
                return _propertyIDsArrayCache.Intersect(shader.EnumeratePropertyNameIDs()).Count() == _propertyIDsArrayCache.Length;
            }

            private static int[] _propertyIDsArrayCache;

            public override void SetControlAnimation(in ControlAnimationContainer container, in ControlAnimationParameters parameters)
            {
                if (container.ControlType.HasFlag(LightLimitControlType.LightMin))
                {
                    container.Default.SetParameterAnimation(parameters, _LightMinLimit, parameters.MinLightValue);
                    container.Control.SetParameterAnimation(parameters, _LightMinLimit, parameters.MinLightValue, parameters.MaxLightValue);
                }

                if (container.ControlType.HasFlag(LightLimitControlType.LightMax))
                {
                    container.Default.SetParameterAnimation(parameters, _LightMaxLimit, parameters.MaxLightValue);
                    container.Control.SetParameterAnimation(parameters, _LightMaxLimit, parameters.MinLightValue, parameters.MaxLightValue);
                }

                if (container.ControlType.HasFlag(LightLimitControlType.Saturation))
                {
                    container.Default.SetParameterAnimation(parameters, _MainTexHSVG, DefaultParameters.MainTexHSVG);

                    container.Control.SetParameterAnimation(parameters, $"{_MainTexHSVG}.x", 1, 1);
                    container.Control.SetParameterAnimation(parameters, $"{_MainTexHSVG}.y", 0, 2);
                    container.Control.SetParameterAnimation(parameters, $"{_MainTexHSVG}.z", 1, 1);
                    container.Control.SetParameterAnimation(parameters, $"{_MainTexHSVG}.w", 1, 1);
                }
                if (container.ControlType.HasFlag(LightLimitControlType.Monochrome))
                {
                    container.Default.SetParameterAnimation(parameters, _MonochromeLighting, 0);
                    container.Control.SetParameterAnimation(parameters, _MonochromeLighting, 0, 1);
                }

                if (container.ControlType.HasFlag(LightLimitControlType.Unlit))
                {
                    container.Default.SetParameterAnimation(parameters, _AsUnlit, 0);
                    container.Control.SetParameterAnimation(parameters, _AsUnlit, 0, 1);
                }

                if (container.ControlType.HasFlag(LightLimitControlType.ColorTemperature))
                {
                    container.Default.SetParameterAnimation(parameters, _Color, DefaultParameters.Color);
                    container.Default.SetParameterAnimation(parameters, _Color2nd, DefaultParameters.Color2nd);
                    container.Default.SetParameterAnimation(parameters, _Color3rd, DefaultParameters.Color3rd);

                    container.Control.SetColorTempertureAnimation(parameters, _Color);
                    container.Control.SetColorTempertureAnimation(parameters, _Color2nd);
                    container.Control.SetColorTempertureAnimation(parameters, _Color3rd);
                }
            }

            public override bool TryGetLightMinMaxValue(Material material, out float min, out float max)
            {
                min = material.GetOrDefault(PropertyIDs.LightMinLimit, DefaultParameters.LightMinLimit);
                max = material.GetOrDefault(PropertyIDs.LightMaxLimit, DefaultParameters.LightMaxLimit);
                return true;
            }
        }
    }
}
