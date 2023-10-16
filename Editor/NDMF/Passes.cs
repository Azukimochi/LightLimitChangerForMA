using System.Collections.Generic;
using nadena.dev.ndmf;
using nadena.dev.ndmf.fluent;
using UnityEditor.Animations;
using static io.github.azukimochi.Passes;
using Object = UnityEngine.Object;

namespace io.github.azukimochi
{
    internal static partial class Passes
    {
        public static void RunningPasses(Sequence sequence)
        {
            sequence
            .Run(CloningMaterials).Then
            .Run(NormalizeMaterials).Then
            .Run(GenerateAdditionalControl).Then
            .Run(GenerateAnimations).Then
            .Run(Finalize);
        }

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

        private static Session GetSession(BuildContext context)
        {
            var session = context.GetState<Session>();
            session.InitializeSession(context);
            return session;
        }

        private static LightLimitChangerObjectCache GetObjectCache(BuildContext context)
        {
            var cache = context.GetState<LightLimitChangerObjectCache>();
            if (cache.Context != context)
                cache.Context = context;
            return cache;
        }

        internal abstract class LightLimitChangerBasePass<TPass> : Pass<TPass> where TPass : Pass<TPass>, new()
        {
            protected override void Execute(BuildContext context)
            {
                var session = GetSession(context);
                if (!session.IsValid())
                    return;
            
                var cache = GetObjectCache(context);

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

            public HashSet<Object> Excludes;

            private bool _initialized;

            public bool IsValid() => Settings != null;

            public void InitializeSession(BuildContext context)
            {
                if (_initialized)
                    return;

                Controller = new AnimatorController() { name = "Light Limit Controller" }.AddTo(GetObjectCache(context));
                Settings = context.AvatarRootObject.GetComponentInChildren<LightLimitChangerSettings>();
                var parameters = Parameters = Settings?.Parameters ?? LightLimitChangerParameters.Default;
                Excludes = new HashSet<Object>(Settings?.Excludes);

                List<ControlAnimationContainer> controls = new List<ControlAnimationContainer>();
                if (!parameters.IsSeparateLightControl)
                {
                    controls.Add(ControlAnimationContainer.Create(LightLimitControlType.Light, "Light", ParameterName_Value, parameters.DefaultLightValue, Icons.Light));
                }
                else
                {
                    controls.Add(ControlAnimationContainer.Create(LightLimitControlType.LightMin, "Min Light", ParameterName_Min, parameters.DefaultLightValue, Icons.Light_Min));
                    controls.Add(ControlAnimationContainer.Create(LightLimitControlType.LightMax, "Max Light", ParameterName_Max, parameters.DefaultLightValue, Icons.Light_Max));
                }

                controls.AddRange(new[]
                {
                    ControlAnimationContainer.Create(LightLimitControlType.Saturation, "Saturation", ParameterName_Saturation, 0.5f, Icons.Color),
                    ControlAnimationContainer.Create(LightLimitControlType.Unlit, "Unlit", ParameterName_Unlit, 0, Icons.Unlit),
                    ControlAnimationContainer.Create(LightLimitControlType.ColorTemperature, "ColorTemp", ParameterName_ColorTemp, 0.5f, Icons.Temp),
                });

                Controls = controls.ToArray();

                TargetControl = parameters.GetControlTypeFlags();

                _initialized = true;
            }
        }
    }
}
