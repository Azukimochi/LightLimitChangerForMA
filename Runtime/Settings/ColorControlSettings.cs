namespace io.github.azukimochi;

[Serializable]
public sealed class ColorControlSettings : ISettings
{
    string ISettings.ParameterPrefix => "Color";

    string ISettings.DisplayName => "Color Control";

    [GeneralControl(GeneralControlType.ColorControlHue)]
    [VectorField(VectorField.X)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_MainTexHSVG.x")]
    [Range(-1, 1)]
    public Parameter<float> Hue = 0;

    [GeneralControl(GeneralControlType.ColorControlSaturation)]
    [VectorField(VectorField.Y)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_MainTexHSVG.y")]
    [Range(0, 2)]
    public Parameter<float> Saturation = 1;

    [GeneralControl(GeneralControlType.ColorControlBrightness)]
    [VectorField(VectorField.Z)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_MainTexHSVG.z")]
    [Range(0, 2)]
    public Parameter<float> Brightness = 1;

    [GeneralControl(GeneralControlType.ColorControlGamma)]
    [VectorField(VectorField.W)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_MainTexHSVG.w")]
    [Range(0.01f, 2)]
    public Parameter<float> Gamma = 1;
}
