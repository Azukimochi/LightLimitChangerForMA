using nadena.dev.ndmf;
using nadena.dev.ndmf.fluent;
using UnityEditor.Animations;
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
        internal const string ParameterName_Saturation = "LightLimitSaturation";
        internal const string ParameterName_Unlit = "LightLimitUnlit";
        internal const string ParameterName_ColorTemp = "LightLimitColorTemp";
        internal const string ParameterName_Reset = "LightLimitReset";

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

            private static Session GetSession(BuildContext context)
            {
                var session = context.GetState<Session>();
                if (session.Controller == null)
                {
                    session.Controller = new AnimatorController() { name = "Light Limit Controller" }.AddTo(GetObjectCache(context));
                    session.Settings = context.AvatarRootObject.GetComponentInChildren<LightLimitChangerSettings>();
                    session.Parameters = session.Settings?.Parameters ?? LightLimitChangerParameters.Default;
                }
                return session;
            }

            private static LightLimitChangerObjectCache GetObjectCache(BuildContext context)
            {
                var cache = context.GetState<LightLimitChangerObjectCache>();
                if (cache.Context != context)
                    cache.Context = context;
                return cache;
            }

            protected abstract void Execute(BuildContext context, Session session, LightLimitChangerObjectCache cache);
        }

        internal sealed class Session
        {
            public LightLimitChangerSettings Settings;
            public LightLimitChangerParameters Parameters;
            public AnimatorController Controller;

            public bool IsValid() => Settings != null;
        }
    }
}
