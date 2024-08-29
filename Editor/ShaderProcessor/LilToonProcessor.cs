namespace io.github.azukimochi;

internal sealed class LilToonProcessor : ShaderProcessor
{
    public override string QualifiedName => BuiltinSupportedShaders.LilToon;
    public override string DisplayName => "LilToon";

    #region ShaderProperties

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
    public const string _EmissionBlend = "_EmissionBlend";
    public const string _Emission2ndBlend = "_Emission2ndBlend";
    public const string _VertexLigthStrength = "_VertexLightStrength";
    public const string _ShadowEnvStrength = "_ShadowEnvStrength";

    #endregion

    public override void ConfigureGeneralAnimation(ConfigureGeneralAnimationContext context)
    {
        var (min, max) = context.Range;
        string propertyName = context.Type switch
        {
            GeneralControlType.MinLight => _LightMinLimit,
            GeneralControlType.MaxLight => _LightMaxLimit,
            GeneralControlType.Monochrome => _MonochromeLighting,
            GeneralControlType.Unlit => _AsUnlit,
            GeneralControlType.ColorControlHue => $"{_MainTexHSVG}.x",
            GeneralControlType.ColorControlSaturation => $"{_MainTexHSVG}.y",
            GeneralControlType.ColorControlBrightness => $"{_MainTexHSVG}.z",
            GeneralControlType.ColorControlGamma => $"{_MainTexHSVG}.w",
            GeneralControlType.EmissionStrength => _EmissionBlend,
            _ => ""
        };

        context.Renderers.AnimateAllFloat(context.AnimationClip, $"{MaterialAnimationKeyPrefix}{propertyName}", AnimationCurve.Linear(0, min, 1 / 60f, max));
    }

    public override void ConfigureShaderSpecificAnimation(ConfigureShaderSpecificAnimationContext context)
    {
        string propertyName = context.Name switch
        {
            nameof(LilToonSettings.VertexLightStrength) => _VertexLigthStrength,
            nameof(LilToonSettings.ShadowEnvStrength) => _ShadowEnvStrength,
            _ => null,
        };
        if (propertyName is null)
            return;

        var (min, max) = context.Range;
        context.Renderers.AnimateAllFloat(context.AnimationClip, $"{MaterialAnimationKeyPrefix}{propertyName}", AnimationCurve.Linear(0, min, 1 / 60f, max));
    }
}
