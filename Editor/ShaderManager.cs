using System;
using System.Collections.Generic;
using System.Text;

namespace io.github.azukimochi;

internal static class ShaderManager
{
    // TODO （そのうち） : プラグインか何かで差し込めるようにしたい

    private static readonly string[] builtinShaderIDs =
    {
        BuiltinSupportedShaders.LilToon,
        BuiltinSupportedShaders.Poiyomi,
        BuiltinSupportedShaders.Sunao,
        BuiltinSupportedShaders.UnlitWF,
    };

    private static readonly string[] builtinShaderNames =
    {
        "LilToon",
        "Poiyomi",
        "Sunao",
        "UnlitWF",
    };

    public static string[] GetSupportedShaderIDs
        => builtinShaderIDs;

    public static string [] GetSupportedShaderNames
        => builtinShaderNames;
}
