using System;
using System.ComponentModel;

namespace io.github.azukimochi
{
    [Obsolete("Use ShaderType instead")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [Flags]
    public enum Shaders : int
    {
        lilToon = 1 << 0,
        Sunao = 1 << 1,
        Poiyomi = 1 << 2,
    }
}
