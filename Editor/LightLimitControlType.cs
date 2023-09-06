using System;

namespace io.github.azukimochi
{
    [Flags]
    public enum LightLimitControlType
    {
        Light = 1 << 0,
        Saturation = 1 << 1,
        Unlit = 1 << 2,
        ColorTemperature = 1 << 3,
    }
}
