namespace io.github.azukimochi;

[Serializable]
public sealed class ColorControlSettings : ISettings
{
    /// <summary>
    /// カラー制御を有効にする
    /// </summary>
    public bool AllowColorControl = false;

    public Parameter<float> Hue = 0;
    public Parameter<float> Saturation = 1;
    public Parameter<float> Brightness = 1;
}
