using System.Collections.Generic;

namespace io.github.azukimochi;

internal interface IPreferences
{
    Dictionary<string, string> Presets { get; }
    void Save();
}