namespace io.github.azukimochi;

[Serializable]
public sealed class ColorControlSettings : ISettings
{
    /// <summary>
    /// カラー制御を有効にする
    /// </summary>
    public bool AllowColorControl = false;

    public Parameter<float> Hue = new(0) { Range = new(-1, 1) };
    public Parameter<float> Saturation = new(1) { Range = new(0, 2) };
    public Parameter<float> Brightness = new(1) { Range = new(0, 2) };
    public Parameter<float> Gamma = new(1) { Range = new(0.01f, 2) };
}
