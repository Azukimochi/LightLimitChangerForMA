﻿namespace io.github.azukimochi;

[Serializable]
public sealed class ColorControlSettings : ISettings
{
    /// <summary>
    /// カラー制御を有効にする
    /// </summary>
    public bool AllowColorControl = false;

    [GeneralControl(GeneralControlType.ColorControlHue)]
    [Range(-1, 1)]
    public Parameter<float> Hue = 0;

    [GeneralControl(GeneralControlType.ColorControlSaturation)]
    [Range(0, 2)]
    public Parameter<float> Saturation = 1;

    [GeneralControl(GeneralControlType.ColorControlBrightness)]
    [Range(0, 2)]
    public Parameter<float> Brightness = 1;
}
