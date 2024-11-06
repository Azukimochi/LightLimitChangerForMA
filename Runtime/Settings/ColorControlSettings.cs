namespace io.github.azukimochi;

[Serializable]
[MenuIcon(Icons.Color)]
[SettingOptions(id: "color-control", displayName: "Color Control", parameterPrefix: "Color")]
public sealed class ColorControlSettings : ISettings
{
    [GeneralControl(GeneralControlType.ColorControlHue)]
    [VectorField(VectorField.X)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_MainTexHSVG.x")]
    [MenuIcon(Icons.Hue)]
    [Range(-1, 1)]
    public Parameter<float> Hue = 0;

    [GeneralControl(GeneralControlType.ColorControlSaturation)]
    [VectorField(VectorField.Y)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_MainTexHSVG.y")]
    [MenuIcon(Icons.Saturation)]
    [Range(0, 2)]
    public Parameter<float> Saturation = 1;

    [GeneralControl(GeneralControlType.ColorControlBrightness)]
    [VectorField(VectorField.Z)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_MainTexHSVG.z")]
    [MenuIcon(Icons.Brightness)]
    [Range(0, 2)]
    public Parameter<float> Brightness = 1;

    [GeneralControl(GeneralControlType.ColorControlGamma)]
    [VectorField(VectorField.W)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_MainTexHSVG.w")]
    [MenuIcon(Icons.Gamma)]
    [Range(0.01f, 2)]
    public Parameter<float> Gamma = 1;
}
