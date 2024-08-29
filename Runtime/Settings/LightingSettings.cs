namespace io.github.azukimochi;

[Serializable]
public sealed class LightingSettings : ISettings
{
    string ISettings.ParameterPrefix => "Light";

    string ISettings.DisplayName => "Lighting";

    /// <summary>
    /// 明るさの下限
    /// </summary>
    [RangeParameter(nameof(MinLightRange))]
    [GeneralControl(GeneralControlType.MinLight)]
    [Range(0, 1)]
    public Parameter<float> MinLight = 0.05f;

    /// <summary>
    /// 明るさの上限
    /// </summary>
    [RangeParameter(nameof(MaxLightRange))]
    [GeneralControl(GeneralControlType.MaxLight)]
    [Range(0, 10)]
    public Parameter<float> MaxLight = 1;

    /// <summary>
    /// 明るさの下限の範囲
    /// </summary>
    [MinMaxSlider(0, 1)]
    public Vector2 MinLightRange = new(0, 1);

    /// <summary>
    /// 明るさの上限の範囲
    /// </summary>
    [MinMaxSlider(0, 10)]
    public Vector2 MaxLightRange = new(0, 1);

    /// <summary>
    /// 光の色の無視具合
    /// </summary>
    [GeneralControl(GeneralControlType.Monochrome)]
    [ShaderFeature(BuiltinSupportedShaders.LilToon)]
    [Range(0, 1)]
    public Parameter<float> Monochrome = 0;

    /// <summary>
    /// 光の無視具合
    /// </summary>
    [GeneralControl(GeneralControlType.Unlit)]
    [ShaderFeature(BuiltinSupportedShaders.LilToon)]
    [Range(0, 10)]
    public Parameter<float> Unlit = 0;
}
