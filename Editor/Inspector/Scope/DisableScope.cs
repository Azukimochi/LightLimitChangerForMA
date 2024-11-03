using System;
using System.Collections.Generic;
using System.Text;

namespace io.github.azukimochi;

internal readonly ref struct DisableScope
{
    public DisableScope(bool disabled)
    {
        EditorGUI.BeginDisabledGroup(disabled);
    }

    public void Dispose()
    {
        EditorGUI.EndDisabledGroup();
    }

    public static DisableScope Disable() => new(true);

    public static DisableScope Enable() => new(false);

    public static DisableScope If(bool disabled) => new(disabled);
}
