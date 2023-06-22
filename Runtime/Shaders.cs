using System;

namespace io.github.azukimochi
{
    [Flags]
    public enum Shaders : int
    {
        lilToon = 1 << 0,
        Sunao = 1 << 1,
        Poiyomi = 1 << 2,
    }
}
