namespace io.github.azukimochi;

[Serializable]
public sealed class EmissionControlSettings : ISettings
{
    string ISettings.ParameterPrefix => "Emission";

    string ISettings.DisplayName => "Emission";

    /// <summary>
    /// エミッション制御を有効にする
    /// </summary>
    public bool UseEmissionControl = false;

    [GeneralControl(GeneralControlType.EmissionStrength)]
    [Range(0, 1)]
    public Parameter<float> Strength = 1;
}
