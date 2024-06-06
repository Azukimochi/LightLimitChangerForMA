using System;
using nadena.dev.ndmf;
using UnityEngine;

namespace io.github.azukimochi
{
    partial class ShaderInfo
    {
        public sealed class Poiyomi : ShaderInfo
        {
            public static Poiyomi Instance { get; } = new Poiyomi();

            public const string _LightingMinLightBrightness = "_LightingMinLightBrightness";
            public const string _LightingCap = "_LightingCap";
            public const string _MainColorAdjustToggle = "_MainColorAdjustToggle";
            public const string _Saturation = "_Saturation";
            public const string _MonochromeLighting = "_LightingMonochromatic";
            public const string _Color = "_Color";
            public const string _MainTex = "_MainTex";

            public const string _EnableDissolve = "_EnableDissolve";
            public const string _DissolveTextureColor = "_DissolveTextureColor";
            public const string _DissolveToTexture = "_DissolveToTexture";
            
            private bool isDetectOldVersion = false;

            private static class PropertyIDs
            {
                public static readonly int LightingMinLightBrightness = Shader.PropertyToID(_LightingMinLightBrightness);
                public static readonly int LightingCap = Shader.PropertyToID(_LightingCap);
                public static readonly int MainColorAdjustToggle = Shader.PropertyToID(_MainColorAdjustToggle);
                public static readonly int Saturation = Shader.PropertyToID(_Saturation);
                public static readonly int Color = Shader.PropertyToID(_Color);
                public static readonly int MainTex = Shader.PropertyToID(_MainTex);
                public static readonly int EnableDissolve = Shader.PropertyToID(_EnableDissolve);
                public static readonly int DissolveTextureColor = Shader.PropertyToID(_DissolveTextureColor);
                public static readonly int DissolveToTexture = Shader.PropertyToID(_DissolveToTexture);
                public static readonly int MainColorAdjustTexture = Shader.PropertyToID("_MainColorAdjustTexture");
            }

            private static class DefaultParameters
            {
                public static readonly float LightingMinLightBrightness = 0;
                public static readonly float LightingCap = 1;
                public static readonly float Saturation = 0;
                public static readonly Color Color = Color.white;
            }

            private const string Animated_Suffix = "Animated";
            private const string Flag_IsAnimated = "1";

            public override bool TryNormalizeMaterial(Material material, LightLimitChangerObjectCache cache)
            {
                var textureBaker = TextureBaker.GetInstance<PoiyomiTextureBaker>(cache);
                bool result = false;

                {
                    bool bakeFlag = false;
                    var tex = material.GetOrDefault<Texture>(PropertyIDs.MainTex);
                    if (!(tex is RenderTexture))
                    {
                        if (tex != null)
                            textureBaker.Texture = tex;

                        var color = material.GetOrDefault(PropertyIDs.Color, DefaultParameters.Color);
                        if (color != DefaultParameters.Color)
                        {
                            textureBaker.Color = color;
                            material.TrySet(PropertyIDs.Color, DefaultParameters.Color);
                            bakeFlag = true;
                        }

                        var saturation = material.GetOrDefault(PropertyIDs.Saturation, DefaultParameters.Saturation);
                        if (saturation != DefaultParameters.Saturation)
                        {
                            textureBaker.Saturation = saturation;
                            material.TrySet(PropertyIDs.Saturation, DefaultParameters.Saturation);
                            bakeFlag = true;
                        }

                        // 焼き込むべきかどうかいまいちわかんなかったので保留
                        /*var adjustTexture = material.GetOrDefault<Texture>(PropertyIDs.MainColorAdjustTexture, null);
                        if (adjustTexture != null)
                        {
                            textureBaker.ColorAdjustTexture = adjustTexture;
                            material.TrySet<Texture>(PropertyIDs.MainColorAdjustTexture, null);
                            bakeFlag = true;
                        }*/

                        if (bakeFlag)
                        {
                            var baked = cache.Register(textureBaker.Bake());
                            material.TrySet(PropertyIDs.MainTex, baked);
                        }

                        result |= bakeFlag;
                    }
                }

                if (material.GetOrDefault(PropertyIDs.EnableDissolve, 0f) != 0)
                {
                    textureBaker.Reset();

                    bool bakeFlag = false;
                    var tex = material.GetOrDefault<Texture>(PropertyIDs.DissolveToTexture);
                    if (!(tex is RenderTexture))
                    {
                        if (tex != null)
                            textureBaker.Texture = tex;

                        var color = material.GetOrDefault(PropertyIDs.DissolveTextureColor, DefaultParameters.Color);
                        if (color != DefaultParameters.Color)
                        {
                            textureBaker.Color = color;
                            material.TrySet(PropertyIDs.DissolveTextureColor, DefaultParameters.Color);
                            bakeFlag = true;
                        }

                        if (bakeFlag)
                        {
                            var baked = cache.Register(textureBaker.Bake());
                            material.TrySet(PropertyIDs.DissolveToTexture, baked);
                        }

                        result |= bakeFlag;
                    }
                }


                return result;
            }

            public override bool IsTargetShader(Shader shader)
            {
                if (shader.name.Contains("7.3", StringComparison.OrdinalIgnoreCase) && !isDetectOldVersion)
                {
                    IError error = new ErrorMessage("NDMF.info.poiyomi_old_version", ErrorSeverity.NonFatal);
                    ErrorReport.ReportError(error);
                    isDetectOldVersion = true;
                }
                //return shader.name.Contains("poiyomi", StringComparison.OrdinalIgnoreCase);
                return shader.name.Contains(".poiyomi", StringComparison.OrdinalIgnoreCase) &&
                       ! shader.name.Contains("7.3", StringComparison.OrdinalIgnoreCase);
            }

            public override void SetControlAnimation(in ControlAnimationContainer container, in ControlAnimationParameters parameters)
            {
                if (container.ControlType.HasFlag(LightLimitControlType.LightMin))
                {
                    container.Default.SetParameterAnimation(parameters, _LightingMinLightBrightness, parameters.MinLightValue);
                    container.Control.SetParameterAnimation(parameters, _LightingMinLightBrightness, parameters.MinLightValue, parameters.MaxLightValue);
                }

                if (container.ControlType.HasFlag(LightLimitControlType.LightMax))
                {
                    container.Default.SetParameterAnimation(parameters, _LightingCap, parameters.MaxLightValue);
                    container.Control.SetParameterAnimation(parameters, _LightingCap, parameters.MinLightValue, parameters.MaxLightValue);
                }

                if (container.ControlType.HasFlag(LightLimitControlType.Saturation))
                {
                    container.Default.SetParameterAnimation(parameters, _Saturation, DefaultParameters.Saturation);
                    container.Control.SetParameterAnimation(parameters, _Saturation, -1, 1);
                }

                if (container.ControlType.HasFlag(LightLimitControlType.ColorTemperature))
                {
                    container.Default.SetParameterAnimation(parameters, _Color, DefaultParameters.Color);
                    container.Control.SetColorTempertureAnimation(parameters, _Color, DefaultParameters.Color);

                    container.Default.SetParameterAnimation(parameters, _DissolveTextureColor, DefaultParameters.Color);
                    container.Control.SetColorTempertureAnimation(parameters, _DissolveTextureColor, DefaultParameters.Color);
                }
                if (container.ControlType.HasFlag(LightLimitControlType.Monochrome))
                {
                    container.Default.SetParameterAnimation(parameters, _MonochromeLighting, 0);
                    container.Control.SetParameterAnimation(parameters, _MonochromeLighting, 0, 1);
                }
            }

            public override void AdditionalControl(Material material, in LightLimitChangerParameters parameters)
            {
                material.SetOverrideTag($"{_LightingCap}{Animated_Suffix}", Flag_IsAnimated);
                material.SetOverrideTag($"{_LightingMinLightBrightness}{Animated_Suffix}", Flag_IsAnimated);
                if (parameters.AllowSaturationControl || parameters.AllowColorTempControl)
                {
                    material.TrySet(PropertyIDs.MainColorAdjustToggle, 1f);
                    material.EnableKeyword($"{_MainColorAdjustToggle.ToUpperInvariant()}_ON");
                    material.EnableKeyword("COLOR_GRADING_HDR");

                    if (parameters.AllowColorTempControl)
                    {
                        material.SetOverrideTag($"{_Color}{Animated_Suffix}", Flag_IsAnimated);

                        if (material.GetOrDefault(PropertyIDs.EnableDissolve, 0f) != 0)
                            material.SetOverrideTag($"{_DissolveTextureColor}{Animated_Suffix}", Flag_IsAnimated);
                    }
                    if (parameters.AllowSaturationControl)
                    {
                        material.SetOverrideTag($"{_Saturation}{Animated_Suffix}", Flag_IsAnimated);
                    }
                }

                if (parameters.AllowMonochromeControl)
                {
                    material.SetOverrideTag($"{_MonochromeLighting}{Animated_Suffix}", Flag_IsAnimated);
                }
            }

            public override bool TryGetLightMinMaxValue(Material material, out float min, out float max)
            {
                min = material.GetOrDefault(PropertyIDs.LightingMinLightBrightness, DefaultParameters.LightingMinLightBrightness);
                max = material.GetOrDefault(PropertyIDs.LightingCap, DefaultParameters.LightingCap);
                return true;
            }
        }
    }
}
