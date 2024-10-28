namespace io.github.azukimochi;

[Serializable]
public sealed class EmissionControlSettings : ISettings
{
    string ISettings.ParameterPrefix => "Emission";

    string ISettings.DisplayName => "Emission";

    [GeneralControl(GeneralControlType.EmissionStrength)]
    [Range(0, 1)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_EmissionBlend")]
    public Parameter<float> Strength = 1;
}
