using System.Collections.Generic;

namespace io.github.azukimochi;

internal abstract class BasePrefs<T> : ScriptableSingleton<T>, IPreferences where T : ScriptableSingleton<T>
{
    public List<string> Presets = new();

    List<string> IPreferences.Presets => Presets;

    public void Save() => Save(true);
}
