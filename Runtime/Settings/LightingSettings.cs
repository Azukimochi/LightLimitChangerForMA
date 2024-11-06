namespace io.github.azukimochi;

[Serializable]
[MenuIcon(Icons.Light)]
[SettingOptions(id: "lighting", displayName: "Lighting", parameterPrefix: "Light")]
public sealed class LightingSettings : ISettings
{
    string ISettings.ParameterPrefix => "Light";

    string ISettings.DisplayName => "Lighting";

    /// <summary>
    /// 明るさの下限
    /// </summary>
    [RangeParameter(nameof(MinLightRange))]
    [MinMaxRange(0, 1)]
    [GeneralControl(GeneralControlType.MinLight)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_LightMinLimit")]
    [MenuIcon(Icons.Light_Min)]
    public Parameter<float> MinLight = 0.05f;

    /// <summary>
    /// 明るさの上限
    /// </summary>
    [RangeParameter(nameof(MaxLightRange))]
    [MinMaxRange(0, 10)]
    [GeneralControl(GeneralControlType.MaxLight)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_LightMaxLimit")]
    [MenuIcon(Icons.Light_Max)]
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
    [MenuIcon(Icons.Monochrome)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_MonochromeLighting")]
    [Range(0, 1)]
    public Parameter<float> Monochrome = 0;

    /// <summary>
    /// 光の無視具合
    /// </summary>
    [GeneralControl(GeneralControlType.Unlit)]
    [MenuIcon(Icons.Unlit)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_AsUnlit")]
    [Range(0, 10)]
    public Parameter<float> Unlit = 0;
    
    /// <summary>
    /// エミッションの強度
    /// </summary>
    [GeneralControl(GeneralControlType.EmissionStrength)]
    [MenuIcon(Icons.Emission)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_EmissionBlend")]
    [Range(0, 1)]
    public Parameter<float> EmissionStrength = 1;
}
