using System.Diagnostics;
using nadena.dev.ndmf;
using UnityEngine;

[assembly: ExportsPlugin(typeof(io.github.azukimochi.LightLimitChanger))]

namespace io.github.azukimochi
{
    public sealed partial class LightLimitChanger : Plugin<LightLimitChanger>
    {
        internal const string Title = "Light Limit Changer";

        public override string QualifiedName => "io.github.azukimochi.light-limit-changer";

        public override string DisplayName => Title;

        private const string ModularAvatarQualifiedName = "nadena.dev.modular-avatar";

        public override Color? ThemeColor => new Color(0.0f, 0.2f, 0.8f);



        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming)
                .BeforePlugin(ModularAvatarQualifiedName)
                .Run(DisplayName, Run);
        }

        private void Run(BuildContext context)
        {
            using var processor = new LightLimitChangerProcessor(context);
            ConfigureBuiltinProcessors(processor);

            processor.Run();
        }

        private void ConfigureBuiltinProcessors(LightLimitChangerProcessor processor)
        {
            var targetShader = processor.Component?.TargetShader ?? TargetShaderContainer.Nothing;

            if (targetShader.Contains(BuiltinSupportedShaders.LilToon))
            {
                processor.AddProcessor<LilToonProcessor>();
            }

            if (targetShader.Contains(BuiltinSupportedShaders.Poiyomi))
            {
                processor.AddProcessor<PoiyomiProcessor>();
            }

            if (targetShader.Contains(BuiltinSupportedShaders.Sunao))
            {
                processor.AddProcessor<SunaoProcessor>();
            }

            if (targetShader.Contains(BuiltinSupportedShaders.UnlitWF))
            {
                processor.AddProcessor<UnlitWFProcessor>();
            }
        }
    }
}
