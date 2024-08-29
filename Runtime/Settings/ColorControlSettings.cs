namespace io.github.azukimochi;

[Serializable]
public sealed class ColorControlSettings : ISettings
{
    string ISettings.ParameterPrefix => "Color";

    string ISettings.DisplayName => "Color Control";

    [GeneralControl(GeneralControlType.ColorControlHue)]
    [Range(-1, 1)]
    public Parameter<float> Hue = 0;

    [GeneralControl(GeneralControlType.ColorControlSaturation)]
    [Range(0, 2)]
    public Parameter<float> Saturation = 1;

    [GeneralControl(GeneralControlType.ColorControlBrightness)]
    [Range(0, 2)]
    public Parameter<float> Brightness = 1;

    [GeneralControl(GeneralControlType.ColorControlGamma)]
    [Range(0.01f, 2)]
    public Parameter<float> Gamma = 1;
}
