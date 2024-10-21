using System.Collections.Generic;
using System.Linq;

namespace io.github.azukimochi;

internal abstract class BasePrefs<T> : ScriptableSingleton<T>, IPreferences, ISerializationCallbackReceiver where T : ScriptableSingleton<T>
{
    [SerializeField]
    private Preset[] Presets;

    Dictionary<string, string> IPreferences.Presets => presetDict;

    private Dictionary<string, string> presetDict;

    public void OnAfterDeserialize()
    {
        if (Presets == null)
            presetDict = new();
        else
            presetDict = Presets.ToDictionary(x => x.Name, x => x.Data);
    }

    public void OnBeforeSerialize()
    {
        if (presetDict == null)
            Presets = Array.Empty<Preset>();
        else
            Presets = presetDict.Select(x => new Preset(x.Key, x.Value)).ToArray();
    }

    public void Save() => Save(true);

    [Serializable]
    private struct Preset
    {
        public string Name;
        public string Data;

        public Preset(string name, string data)
        {
            Name = name;
            Data = data;
        }
    }
}
