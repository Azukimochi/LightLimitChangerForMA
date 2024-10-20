using System.IO;
using nadena.dev.ndmf.preview;
using UnityEditor;

namespace io.github.azukimochi;

internal static class LightLimitChangerPrefab
{
    public static event Action<string, UpdatePresetMode> UpdatePreset;

    public enum UpdatePresetMode
    {
        Append,
        Remove,
    }

    private const string DefaultSettingsPrefabGUID = "c34f27003cae48a459266092c574f293";
    public static GameObject DefaultSettings => AssetUtils.FromGUID<GameObject>(DefaultSettingsPrefabGUID);

    static LightLimitChangerPrefab()
    {
    }
}
