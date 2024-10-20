using System.Collections.Generic;

namespace io.github.azukimochi;

internal abstract class BasePrefs<T> : ScriptableSingleton<T>, IPreferences where T : ScriptableSingleton<T>
{
    [SerializeField]
    private List<string> presets;

    public List<string> Presets => presets ??= new();
    
    public void Save() => Save(true);
}
