using System;

namespace io.github.azukimochi
{
    [Flags]
    public enum LightLimitControlType
    {
        Light = LightMin | LightMax, // 1 << 0,
        Saturation = 1 << 1,
        Unlit = 1 << 2,
        ColorTemperature = 1 << 3,
        LightMin = 1 << 4,
        LightMax = 1 << 5,
        Monochrome = 1 << 6,

        AdditionalControls = Saturation | Unlit | ColorTemperature | Monochrome,
    }
}
