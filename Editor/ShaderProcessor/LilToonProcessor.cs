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
    
    private static readonly LazyObject<Material> bakerMaterial = new(() => new Material(Shader.Find("Hidden/ltsother_baker")));

    public override bool IsTargetMaterial(Material material)
    {
        if (material.shader.name.Contains("lilToon", StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }

    public override string GetMaterialPropertyNameFromTypeOrName(GeneralControlType type, string name)
    {
        return (type, name) switch
        {
            (GeneralControlType.MinLight, _) => _LightMinLimit,
            (GeneralControlType.MaxLight, _) => _LightMaxLimit,
            (GeneralControlType.Monochrome, _) => _MonochromeLighting,
            (GeneralControlType.Unlit, _) => _AsUnlit,
            (GeneralControlType.ColorControlHue, _) => $"{_MainTexHSVG}.x",
            (GeneralControlType.ColorControlSaturation, _) => $"{_MainTexHSVG}.y",
            (GeneralControlType.ColorControlBrightness, _) => $"{_MainTexHSVG}.z",
            (GeneralControlType.ColorControlGamma, _) => $"{_MainTexHSVG}.w",
            (GeneralControlType.EmissionStrength, _) => _EmissionBlend,

            (_, nameof(LilToonSettings.VertexLightStrength)) => _VertexLigthStrength,
            (_, nameof(LilToonSettings.ShadowEnvStrength)) => _ShadowEnvStrength,

            _ => null
        };
    }

    public override void ConfigureGeneralAnimation(ConfigureGeneralAnimationContext context)
    {
        if (context.Type is GeneralControlType.ColorControlHue)
        {
            context.Range *= 0.5f;
        }

        base.ConfigureGeneralAnimation(context);
    }

    public override void NormalizeMaterial(Material material)
    {
        var colorCtrl = Processor.Component.General.ColorControl;
        if (!colorCtrl.Any(x => x.Enable))
            return;

        var hsvg = material.Get(_MainTexHSVG, new Vector4(0, 1, 1, 1));
        var writeback = hsvg;

        hsvg.x = !colorCtrl.Hue.Enable ? 0 : hsvg.x;
        hsvg.y = !colorCtrl.Saturation.Enable ? 1 : hsvg.y;
        hsvg.z = !colorCtrl.Brightness.Enable ? 1 : hsvg.z;
        hsvg.w = !colorCtrl.Gamma.Enable ? 1 : hsvg.w;

        writeback.x = colorCtrl.Hue.Enable ? 0 : writeback.x;
        writeback.y = colorCtrl.Saturation.Enable ? 1 : writeback.y;
        writeback.z = colorCtrl.Brightness.Enable ? 1 : writeback.z;
        writeback.w = colorCtrl.Gamma.Enable ? 1 : writeback.w;


        var maintex = material.Get(_MainTex, Texture2D.whiteTexture);

        Material mat = bakerMaterial;
        mat.SetVector(_MainTexHSVG, hsvg);

        var baked = TextureBaker.Bake(maintex, mat, maintex.format);
        material.SetTexture(_MainTex, baked);
        material.SetVector(_MainTexHSVG, writeback);
    }
}