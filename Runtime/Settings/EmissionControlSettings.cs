namespace io.github.azukimochi;

[Serializable]
public sealed class EmissionControlSettings : ISettings
{
    /// <summary>
    /// エミッション制御を有効にする
    /// </summary>
    public bool UseEmissionControl = false;

    public Parameter<float> Strength = 1;
}
