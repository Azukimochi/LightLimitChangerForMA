using System.Collections.Generic;

namespace io.github.azukimochi;

internal interface IPreferences
{
    List<string> Presets { get; }
    void Save();
}