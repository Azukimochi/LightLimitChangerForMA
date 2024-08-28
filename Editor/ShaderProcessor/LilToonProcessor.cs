namespace io.github.azukimochi;

internal sealed class LilToonProcessor : ShaderProcessor
{
    public override string QualifiedName => BuiltinSupportedShaders.LilToon;
    public override string DisplayName => "LilToon";

    public override void ConfigureGeneralAnimation(in ConfigureGeneralAnimationContext context)
    {
        if (context.Type == GeneralControlType.MinLight)
        {
            foreach(var x in context.Renderers.Span)
            {
                AnimationUtility.SetEditorCurve(context.AnimationClip, x.ToPPtrCurve(""), AnimationCurve.Linear(0, 0, 1/60f, 1));
            }
        }
    }
}
