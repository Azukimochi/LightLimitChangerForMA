using System;
using System.Collections.Generic;
using gomoru.su;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using nadena.dev.ndmf.fluent;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace io.github.azukimochi
{
    internal static partial class Passes
    {
        public static void RunningPasses(Sequence sequence)
        {
            sequence
                .Run(CollectTargetRenderers).Then
                .Run(CloningMaterials).Then
                .Run(NormalizeMaterials).Then
                .Run(GenerateAdditionalControl).Then
                .Run(GenerateAnimations).Then
                .Run(Finalize);
        }

        public readonly static CollectTargetRenderersPass CollectTargetRenderers = new CollectTargetRenderersPass();
        public readonly static CloningMaterialsPass CloningMaterials = new CloningMaterialsPass();
        public readonly static NormalizeMaterialsPass NormalizeMaterials = new NormalizeMaterialsPass();
        public readonly static GenerateAdditionalControlPass GenerateAdditionalControl = new GenerateAdditionalControlPass();
        public readonly static GenerateAnimationsPass GenerateAnimations = new GenerateAnimationsPass();
        public readonly static FinalizePass Finalize = new FinalizePass();

        internal const string ParameterName_Toggle = "LightLimitEnable";
        internal const string ParameterName_Value = "LightLimitValue";
        internal const string ParameterName_Min = "LightLimitMin";
        internal const string ParameterName_Max = "LightLimitMax";
        internal const string ParameterName_Saturation = "LightLimitSaturation";
        internal const string ParameterName_Unlit = "LightLimitUnlit";
        internal const string ParameterName_ColorTemp = "LightLimitColorTemp";
        internal const string ParameterName_Reset = "LightLimitReset";
        internal const string ParameterName_Monochrome = "LightLimitMonochrome";

        private static Session GetSession(BuildContext context)
        {
            var session = context.GetState<Session>();
            session.InitializeSession(context);
            return session;
        }

        private static LightLimitChangerObjectCache GetObjectCache(BuildContext context)
        {
            var cache = context.GetState<LightLimitChangerObjectCache>();
            if (cache.Container != context.AssetContainer)
                cache.Container = context.AssetContainer;
            return cache;
        }

        internal abstract class LightLimitChangerBasePass<TPass> : Pass<TPass> where TPass : Pass<TPass>, new()
        {
            protected LightLimitChangerObjectCache Cache => _cache;
            protected Session Session => _session;
            private LightLimitChangerObjectCache _cache;
            private Session _session;

            protected override void Execute(BuildContext context)
            {
                var session = GetSession(context);
                if ((!session.IsValid() || session.IsNoTargetRenderer) && (typeof(TPass) != typeof(FinalizePass) && typeof(TPass) != typeof(GenerateAnimationsPass)))
                    return;
                _session = session;
                var cache = _cache = GetObjectCache(context);

                Execute(context, session, cache);
            }

            protected abstract void Execute(BuildContext context, Session session, LightLimitChangerObjectCache cache);
        }

        internal sealed class Session
        {
            public LightLimitChangerSettings Settings;
            public LightLimitChangerParameters Parameters;
            public ControlAnimationContainer[] Controls;
            public AnimatorController Controller;
            public LightLimitControlType TargetControl;
            public HashSet<Renderer> TargetRenderers;
            public DirectBlendTree DirectBlendTree;
            public List<ParameterConfig> AvatarParameters;
            public bool IsNoTargetRenderer;

            public HashSet<Object> Excludes;

            private bool _initialized;

            public bool IsValid() => Settings != null;

            public void InitializeSession(BuildContext context)
            {
                InitializeSession(context.AvatarRootObject.GetComponentInChildren<LightLimitChangerSettings>(), GetObjectCache(context));
            }

            public void InitializeSession(LightLimitChangerSettings settings, LightLimitChangerObjectCache cache)
            {
                if (_initialized)
                    return;

                Controller = new AnimatorController() { name = "Light Limit Controller" }.AddTo(cache);
                DirectBlendTree = new DirectBlendTree();
                Settings = settings;
                var parameters = Parameters = Settings?.Parameters ?? new LightLimitChangerParameters();
                Excludes = new HashSet<Object>(Settings?.Excludes ?? (IEnumerable<Object>)Array.Empty<Object>());
                var targetControl = LightLimitControlType.Light;
                List<ControlAnimationContainer> controls = new List<ControlAnimationContainer>();
                var defaultAnimation = new AnimationClip() { name = "Default" };
                AvatarParameters = new List<ParameterConfig>();

                if (!parameters.IsSeparateLightControl)
                {
                    controls.Add(ControlAnimationContainer.Create(LightLimitControlType.Light, "Light", ParameterName_Value, parameters.DefaultLightValue, Icons.Light, defaultAnimation));
                }
                else
                {
                    controls.Add(ControlAnimationContainer.Create(LightLimitControlType.LightMin, "Min Light", ParameterName_Min, parameters.DefaultMinLightValue, Icons.Light_Min, defaultAnimation));
                    controls.Add(ControlAnimationContainer.Create(LightLimitControlType.LightMax, "Max Light", ParameterName_Max, parameters.DefaultMaxLightValue, Icons.Light_Max, defaultAnimation));
                }

                if (parameters.AllowColorTempControl)
                {
                    targetControl |= LightLimitControlType.ColorTemperature;
                    controls.Add(ControlAnimationContainer.Create(LightLimitControlType.ColorTemperature, "ColorTemp", ParameterName_ColorTemp, parameters.InitialTempControlValue, Icons.Temp, defaultAnimation));
                }

                if (parameters.AllowSaturationControl)
                {
                    targetControl |= LightLimitControlType.Saturation;
                    controls.Add(ControlAnimationContainer.Create(LightLimitControlType.Saturation, "Saturation", ParameterName_Saturation, parameters.InitialSaturationControlValue, Icons.Color, defaultAnimation));
                }

                if (parameters.AllowUnlitControl)
                {
                    targetControl |= LightLimitControlType.Unlit;
                    controls.Add(ControlAnimationContainer.Create(LightLimitControlType.Unlit, "Unlit", ParameterName_Unlit, parameters.InitialUnlitControlValue, Icons.Unlit, defaultAnimation));
                }

                if (parameters.AllowMonochromeControl)
                {
                    targetControl |= LightLimitControlType.Monochrome;
                    controls.Add(ControlAnimationContainer.Create(LightLimitControlType.Monochrome, "Monochrome", ParameterName_Monochrome, parameters.InitialMonochromeControlValue, Icons.Monochrome, defaultAnimation));
                }


                Controls = controls.ToArray();

                TargetControl = targetControl;

                TargetRenderers = new HashSet<Renderer>();

                _initialized = true;
            }

            public void AddParameter(ParameterConfig parameterConfig) => AvatarParameters.Add(parameterConfig);
        }
    }
}
