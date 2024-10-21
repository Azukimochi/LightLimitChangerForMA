using System.Collections;
using System.Collections.Generic;
using System.IO;
using nadena.dev.ndmf.preview;
using UnityEditor;

namespace io.github.azukimochi;

internal sealed class PresetManager
{
    private const string DefaultSettingsPrefabGUID = "c34f27003cae48a459266092c574f293";
    public static GameObject DefaultSettings => AssetUtils.FromGUID<GameObject>(DefaultSettingsPrefabGUID);

    public static PresetManager Local { get; } = new(Preferences.Local);

    public static PresetManager Global { get; } = new(Preferences.Global);

    private readonly IPreferences preferences;

    private PresetManager(IPreferences preferences) => this.preferences = preferences;

    public bool ContainsKey(string key) => preferences.Presets.ContainsKey(key);

    public bool TryAdd(string key, LightLimitChangerComponent component)
    {
        if (preferences.Presets.ContainsKey(key))
            return false;
        Update(key, component);
        return true;
    }

    public void Update(string key, LightLimitChangerComponent component)
    {
        var json = JsonUtility.ToJson(component);
        preferences.Presets[key] = json;
        preferences.Save();
    }

    public bool TryLoad(string key, LightLimitChangerComponent component)
    {
        if (!preferences.Presets.TryGetValue(key, out var json))
            return false;

        Undo.RecordObject(component, $"Load Preset {key}");
        JsonUtility.FromJsonOverwrite(json, component);
        return true;
    }

    public bool Remove(string key)
    {
        bool result = preferences.Presets.Remove(key);
        if (result)
            preferences.Save();
        return result;
    }

    public IEnumerable<string> Keys
    {
        get
        {
            IEnumerable<string> enumerable = preferences.Presets?.Keys;
            if (enumerable is null)
                enumerable = Array.Empty<string>();
            return enumerable;
        }
    }
}

