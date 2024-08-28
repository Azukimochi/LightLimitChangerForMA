using nadena.dev.ndmf;
using UnityEngine;

[assembly: ExportsPlugin(typeof(io.github.azukimochi.LightLimitChanger))]

namespace io.github.azukimochi
{
    public sealed partial class LightLimitChanger : Plugin<LightLimitChanger>
    {
        internal const string Title = "LightLimitChanger";

        public override string QualifiedName => "io.github.azukimochi.light-limit-changer";

        public override string DisplayName => Title;

        private const string ModularAvatarQualifiedName = "nadena.dev.modular-avatar";

        public override Color? ThemeColor => new Color(0.0f, 0.2f, 0.8f);

        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming).Run(DisplayName, Run);
        }

        private void Run(BuildContext context)
        {
            using var processor = new LightLimitChangerProcessor(context);
            if (processor.Component.TargetShader.Contains(BuiltinSupportedShaders.LilToon))
            {
                processor.AddProcessor<LilToonProcessor>();
            }

            processor.Run();
        }
    }
}
